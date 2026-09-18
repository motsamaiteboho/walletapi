# Wallet API

A backend-focused wallet application built with **ASP.NET Core**, **PostgreSQL**, **Entity Framework Core**, and **Azure Service Bus**.

The solution implements the core wallet functionality required by the technical assessment:

- Retrieve a wallet's current balance
- Withdraw funds
- Prevent withdrawals when insufficient funds are available
- Ensure the wallet balance never becomes negative
- Record successful withdrawals as transactions
- Publish a withdrawal event
- Process withdrawal events asynchronously
- Handle transient failures through Service Bus retries
- Support dead-letter processing
- Provide automated unit and integration tests

The solution also demonstrates production-oriented engineering practices, including:

- Clean Architecture
- Optimistic concurrency
- Request idempotency
- Transactional Outbox
- Asynchronous event processing
- Structured logging
- Correlation IDs
- Problem Details error responses
- Downstream payment processing abstraction
- React demonstration client
- CI build and test automation

---

# 1. Solution Overview

The application represents a simple digital wallet.

A wallet contains:

- A unique wallet account ID
- A monetary balance
- A currency
- Creation timestamp
- A concurrency version

The primary operation is a withdrawal.

A successful withdrawal:

1. Validates the requested amount.
2. Retrieves the wallet.
3. Verifies that sufficient funds are available.
4. Updates the wallet balance.
5. Creates a wallet transaction.
6. Stores an idempotency record.
7. Creates a withdrawal event in the transactional outbox.
8. Commits the database transaction.
9. Publishes the event asynchronously through Azure Service Bus.
10. Processes the event through the withdrawal worker.

The wallet balance remains the authoritative financial state in PostgreSQL.

---

# 2. Assessment Scope

The core assessment functionality is intentionally kept simple.

## Required functionality

```text
GET wallet balance
        |
        v
POST withdrawal
        |
        +-- Validate amount
        +-- Check wallet
        +-- Check sufficient funds
        +-- Update balance
        +-- Create transaction
        +-- Publish withdrawal event
```

The implementation also provides additional engineering capabilities without changing the core wallet behaviour.

---

# 3. Architecture

The solution follows a Clean Architecture approach.

```text
                         +-----------------+
                         |   React Client  |
                         +--------+--------+
                                  |
                                  v
                         +-----------------+
                         |   Wallet API    |
                         | ASP.NET Core    |
                         +--------+--------+
                                  |
                                  v
                         +-----------------+
                         |   Application   |
                         |    Services     |
                         +--------+--------+
                                  |
                                  v
                         +-----------------+
                         |     Domain      |
                         | WalletAccount   |
                         +-----------------+
                                  |
                                  v
                         +-----------------+
                         | Infrastructure  |
                         | EF Core/Npgsql  |
                         +--------+--------+
                                  |
                                  v
                         +-----------------+
                         |   PostgreSQL    |
                         +--------+--------+
                                  |
                                  v
                         +-----------------+
                         | Transactional   |
                         |     Outbox      |
                         +--------+--------+
                                  |
                                  v
                         +-----------------+
                         | Outbox Publisher|
                         |     Worker      |
                         +--------+--------+
                                  |
                                  v
                         +-----------------+
                         | Azure Service   |
                         |      Bus        |
                         +--------+--------+
                                  |
                                  v
                         +-----------------+
                         | Withdrawal      |
                         |    Worker       |
                         +--------+--------+
                                  |
                                  v
                         +-----------------+
                         |    Payment      |
                         |   Processor     |
                         +-----------------+
```

For local development, the Azure Service Bus Emulator is used.

For production, the architecture can be deployed using Azure managed services.

---

# 4. Project Structure

```text
Wallet.Api/
|
+-- .github/
|   +-- workflows/
|       +-- ci.yml
|
+-- Wallet.Api/
|   +-- Controllers/
|   +-- Middleware/
|   +-- Program.cs
|   +-- appsettings.json
|
+-- Wallet.Application/
|   +-- Events/
|   +-- Features/
|   |   +-- WalletAccounts/
|   |       +-- Balance/
|   |       +-- Withdraw/
|   |       +-- Transactions/
|   +-- Exceptions/
|   +-- DependencyInjection.cs
|
+-- Wallet.Domain/
|   +-- Entities/
|   +-- Enums/
|   +-- Exceptions/
|   +-- ...
|
+-- Wallet.Infrastructure/
|   +-- Persistence/
|       +-- Configurations/
|       +-- Outbox/
|       +-- Payment/
|       +-- Repositories/
|       +-- WalletDbContext.cs
|
+-- Wallet.Worker/
|   +-- Messaging/
|   +-- Outbox/
|   +-- Worker.cs
|   +-- Program.cs
|
+-- Wallet.UnitTests/
|
+-- Wallet.IntegrationTests/
|
+-- wallet-client/
|   +-- src/
|   +-- package.json
|   +-- ...
|
+-- servicebus/
|   +-- config.json
|
+-- docker-compose.yml
+-- .gitignore
+-- README.md
+-- Wallet.Api.sln
```

---

# 5. Technical Choices and Rationale

## 5.1 ASP.NET Core

The backend is implemented using ASP.NET Core.

### Reasons

- Strong support for REST APIs
- Built-in dependency injection
- Mature middleware pipeline
- Strong typing
- Good automated testing support
- Suitable for enterprise backend applications
- Good integration with Azure services

---

# 6. Clean Architecture

The solution separates the system into:

```text
Domain
   |
   v
Application
   |
   v
Infrastructure
   |
   v
API / Worker
```

The domain layer contains the wallet's business rules and does not depend on infrastructure technologies.

For example, `WalletAccount` does not know about:

- Entity Framework Core
- PostgreSQL
- Azure Service Bus
- ASP.NET Core
- Redis
- Azure

This makes the business rules easier to test independently.

---

# 7. Domain Model

The main domain entity is:

```text
WalletAccount
```

It contains:

```text
Id
Balance
Currency
CreatedAt
Version
```

The domain controls balance-changing operations.

Withdrawals are performed through domain behaviour rather than allowing application code to arbitrarily modify the balance.

A withdrawal must satisfy:

```text
Amount > 0
Amount <= Balance
```

The domain therefore prevents a negative balance.

---

# 8. PostgreSQL

PostgreSQL is used as the primary database.

### Reasons

- Strong ACID transaction support
- Relational consistency
- Good support for financial-style data
- Mature ecosystem
- Excellent integration with .NET through Npgsql
- Suitable for both local and cloud deployment

PostgreSQL remains the **source of truth** for wallet balances.

---

# 9. Entity Framework Core

Entity Framework Core is used for database access.

It provides:

- Object-relational mapping
- Database migrations
- Change tracking
- Transaction support
- Optimistic concurrency
- Strongly typed queries

Database schema changes are managed using EF Core migrations.

---

# 10. Optimistic Concurrency

Wallet accounts use PostgreSQL's `xmin` as a concurrency token.

The resulting database update includes the expected row version:

```sql
UPDATE wallet_accounts
SET "Balance" = @balance
WHERE "Id" = @walletId
  AND xmin = @version
RETURNING xmin;
```

If another request modifies the wallet before the current request saves its changes, the expected version no longer matches.

The application converts the resulting concurrency exception into:

```text
409 Conflict
```

This prevents concurrent requests from silently overwriting one another.

---

# 11. Idempotency

The withdrawal endpoint requires an `Idempotency-Key`.

Example:

```http
Idempotency-Key: 4f0f2c6d-6f13-4a39-b3d6-9e6c9f0f8f3c
```

The request is hashed and stored together with the idempotency key.

This allows the API to distinguish between:

```text
Same key + same request
        |
        v
Return previously stored result
```

and:

```text
Same key + different request
        |
        v
409 Conflict
```

This protects against duplicate client requests caused by retries or network failures.

---

# 12. Wallet Transactions

Successful withdrawals create a transaction record.

The transaction contains information such as:

```text
Transaction ID
Wallet Account ID
Transaction Type
Amount
Balance After
Created At
```

Transaction history can be retrieved using:

```http
GET /api/wallet-accounts/{id}/transactions
```

Transactions are ordered from newest to oldest.

---

# 13. Transactional Outbox

Withdrawal events are implemented using the Transactional Outbox pattern.

Instead of publishing directly to Service Bus from the API, the event is stored in PostgreSQL as part of the same operation that updates the wallet.

```text
Withdrawal Request
        |
        +-- Update Wallet Balance
        |
        +-- Create Wallet Transaction
        |
        +-- Store Idempotency Record
        |
        +-- Store Outbox Message
                 |
                 v
            DB Commit
```

Only after the database transaction succeeds does the outbox publisher send the event to Service Bus.

This avoids the failure scenario:

```text
Wallet update succeeds
        |
        v
Service Bus unavailable
        |
        v
Withdrawal event lost
```

The event remains in the outbox and can be retried.

---

# 14. Azure Service Bus

Azure Service Bus is used for asynchronous withdrawal processing.

The local environment uses the Azure Service Bus Emulator.

The configured queue is:

```text
wallet-withdrawals
```

The event flow is:

```text
Wallet API
     |
     v
PostgreSQL Outbox
     |
     v
Outbox Publisher
     |
     v
Azure Service Bus
     |
     v
Withdrawal Worker
     |
     v
Payment Processor
```

---

# 15. Service Bus Message Handling

The consumer uses manual message completion.

The message is only completed after successful downstream processing.

```text
Receive message
      |
      v
Deserialize event
      |
      v
Process withdrawal
      |
      v
Success?
   /     \
 Yes      No
  |        |
  v        v
Complete Retry/DLQ
```

This prevents messages from being acknowledged before the actual processing has completed.

---

# 16. Retry and Dead Letter Queue

Transient failures are allowed to be retried by Service Bus.

Examples include:

```text
Payment service unavailable
Network timeout
Temporary downstream outage
```

The message remains incomplete and Service Bus can redeliver it.

Permanent validation failures can be explicitly dead-lettered.

For the local Service Bus Emulator, the queue is configured with:

```json
{
  "MaxDeliveryCount": 3
}
```

Therefore a message that repeatedly fails can eventually move to the Dead Letter Queue.

This behaviour was tested locally using the Service Bus Emulator.

---

# 17. Payment Processing Abstraction

The Service Bus consumer does not directly contain payment processing logic.

The application uses an abstraction:

```text
IWithdrawalProcessor
        |
        v
IPaymentProcessor
```

The current implementation uses a local payment processor.

The architecture allows this to later be replaced by a real external payment integration:

```text
IPaymentProcessor
       |
       +-- LocalPaymentProcessor
       |
       +-- ExternalPaymentProcessor
                    |
                    v
             Bank / Payment API
```

This keeps external infrastructure concerns isolated from the core wallet logic.

---

# 18. Downstream Payment Idempotency

Service Bus uses at-least-once delivery semantics, meaning a message can potentially be delivered more than once.

The solution therefore maintains a payment processing record associated with the event/message identity.

Conceptually:

```text
Service Bus MessageId
        |
        v
PaymentProcessingRecord
        |
        v
Already processed?
   /          \
 Yes           No
  |             |
  v             v
Skip          Process
```

The database uses a unique constraint on the event identity.

This provides protection against duplicate downstream processing.

For a real external payment provider, the payment reference should also be used as an idempotency key with the provider.

---

# 19. Correlation IDs

The API generates or accepts an `X-Correlation-ID`.

The value is returned in the response and included in application logging.

This allows requests to be traced across the application.

Conceptually:

```text
HTTP Request
     |
     v
Correlation ID
     |
     v
Wallet Operation
     |
     v
Database
     |
     v
Outbox
     |
     v
Service Bus
     |
     v
Worker
```

Correlation IDs are particularly useful when troubleshooting asynchronous workflows.

---

# 20. Error Handling

The API uses centralized exception handling and Problem Details responses.

Examples include:

### Insufficient funds

```text
422 Unprocessable Entity
```

### Concurrency conflict

```text
409 Conflict
```

### Idempotency conflict

```text
409 Conflict
```

### Invalid request

```text
400 Bad Request
```

### Unexpected failure

```text
500 Internal Server Error
```

The intention is to provide a consistent error contract rather than exposing implementation-specific exceptions to API consumers.

---

# 21. React Client

A lightweight React client is included.

The frontend provides:

- Current wallet balance
- Withdrawal form
- Transaction history
- Success messages
- Error messages
- Refresh capability

The frontend is intentionally simple because the primary assessment focus is the backend.

The API remains independently runnable without the React client.

---

# 22. Local Infrastructure

Docker Compose is used for local infrastructure.

The local environment includes:

```text
PostgreSQL
    |
    +-- Wallet database

Service Bus Emulator
    |
    +-- wallet-withdrawals queue

SQL Server
    |
    +-- Required by Service Bus Emulator
```

---

# 23. Prerequisites

Install the following:

- .NET 8 SDK
- Docker Desktop
- Node.js
- npm
- Git
- Visual Studio or VS Code

---

# 24. Setup

## 24.1 Clone the repository

```bash
git clone <repository-url>
cd Wallet.Api
```

---

# 25. Start Docker Infrastructure

Run:

```bash
docker compose up -d
```

Check running containers:

```bash
docker ps
```

Expected infrastructure includes:

```text
wallet-postgres
servicebus-emulator
servicebus-mssql
```

---

# 26. PostgreSQL

PostgreSQL is exposed locally using:

```text
Host: localhost
Port: 5433
Database: wallet
Username: postgres
Password: postgres
```

Example connection string:

```text
Host=localhost;Port=5433;Database=wallet;Username=postgres;Password=postgres
```

---

# 27. Service Bus Emulator

The Service Bus Emulator uses:

```text
Endpoint=sb://localhost;
SharedAccessKeyName=RootManageSharedAccessKey;
SharedAccessKey=SAS_KEY_VALUE;
UseDevelopmentEmulator=true;
```

The configured queue is:

```text
wallet-withdrawals
```

The emulator health endpoint is:

```text
http://localhost:5300/health
```

---

# 28. Database Migrations

Using Visual Studio Package Manager Console:

```powershell
Update-Database -Project Wallet.Infrastructure -StartupProject Wallet.Api
```

Or use the equivalent EF Core CLI command.

---

# 29. Run the API

From the repository root:

```bash
dotnet run --project Wallet.Api
```

The API will start using its configured HTTP/HTTPS endpoints.

Swagger is available in the development environment.

---

# 30. Run the Worker

Open a second terminal:

```bash
dotnet run --project Wallet.Worker
```

The worker contains:

```text
Outbox Publisher
Service Bus Consumer
Withdrawal Processing
Payment Processing
```

---

# 31. Run the React Client

Navigate to:

```bash
cd wallet-client
```

Install dependencies:

```bash
npm install
```

Run:

```bash
npm run dev
```

The development client normally runs at:

```text
http://localhost:5173
```

The API URL is configured using:

```text
VITE_API_BASE_URL
```

Example:

```text
VITE_API_BASE_URL=https://localhost:7143
```

The exact API port depends on the local launch configuration.

---

# 32. Initial Wallet

A known wallet is seeded for local testing.

```text
Wallet ID:
11111111-1111-1111-1111-111111111111

Currency:
ZAR

Initial Balance:
R10,000.00
```

The seed operation is idempotent.

If the wallet already exists, it is not recreated.

---

# 33. API Endpoints

## Get Balance

```http
GET /api/wallet-accounts/{walletAccountId}
```

Example:

```http
GET /api/wallet-accounts/11111111-1111-1111-1111-111111111111
```

Example response:

```json
{
  "walletAccountId": "11111111-1111-1111-1111-111111111111",
  "balance": 10000.00,
  "currency": "ZAR"
}
```

---

# 34. Withdraw Funds

```http
POST /api/wallet-accounts/{walletAccountId}/withdraw
```

Request:

```json
{
  "amount": 100.00
}
```

Header:

```http
Idempotency-Key: 4f0f2c6d-6f13-4a39-b3d6-9e6c9f0f8f3c
```

Example response:

```json
{
  "walletAccountId": "11111111-1111-1111-1111-111111111111",
  "withdrawnAmount": 100.00,
  "remainingBalance": 9900.00,
  "currency": "ZAR"
}
```

---

# 35. Transaction History

```http
GET /api/wallet-accounts/{walletAccountId}/transactions
```

Example:

```http
GET /api/wallet-accounts/11111111-1111-1111-1111-111111111111/transactions
```

---

# 36. Example Withdrawal Flow

A successful withdrawal follows this sequence:

```text
Client
  |
  | POST /withdraw
  v
Wallet API
  |
  v
Validate request
  |
  v
Load WalletAccount
  |
  v
Check balance
  |
  v
Withdraw
  |
  +----------------+
  |                |
  v                v
Transaction      Outbox
  |                |
  +-------+--------+
          |
          v
      DB Commit
          |
          v
   Outbox Publisher
          |
          v
     Service Bus
          |
          v
 Withdrawal Worker
          |
          v
 Payment Processor
```

---

# 37. Assumptions

The following assumptions were made for the assessment implementation:

1. Each wallet uses a single currency.
2. The seeded wallet uses ZAR.
3. Wallet creation is not required for the core assessment flow.
4. A known wallet is seeded for demonstration and testing.
5. Authentication and authorization are outside the core assessment scope.
6. Deposits and transfers are outside the core assessment scope.
7. The wallet balance is maintained as a decimal monetary value.
8. PostgreSQL is the authoritative source of wallet balances.
9. Service Bus is used for asynchronous downstream processing.
10. The Service Bus Emulator is used for local development.
11. External banking/payment infrastructure is represented by an abstraction and local implementation.
12. The React client is a demonstration client rather than the primary focus of the solution.
13. Production deployment is considered separately from the local assessment environment.

---

# 38. Trade-offs

## Transactional Outbox vs Direct Messaging

Directly publishing to Service Bus from the API would be simpler.

However, it introduces a reliability gap:

```text
Database update succeeds
        |
        v
Service Bus publish fails
        |
        v
Event lost
```

The Transactional Outbox pattern adds implementation complexity but provides better reliability.

---

## Synchronous vs Asynchronous Processing

A synchronous payment call would be simpler.

However, asynchronous processing provides:

- Retry capability
- Failure isolation
- Dead-letter handling
- Decoupling
- Better scalability

The solution therefore uses Service Bus for downstream processing.

---

## Optimistic vs Pessimistic Concurrency

Pessimistic locking could serialize wallet operations.

Optimistic concurrency was selected because wallet updates are short-lived and conflicts can be handled using a version check.

This avoids unnecessarily holding database locks.

---

## Service Bus Emulator vs Azure Service Bus

The emulator allows the complete messaging workflow to be demonstrated locally without requiring an Azure subscription.

The trade-off is that the emulator is not a complete replacement for the production Azure environment.

---

## Extra Architecture vs Assessment Simplicity

The assessment requires a relatively small wallet application.

Additional capabilities such as:

- Outbox
- Service Bus
- Idempotency
- Concurrency
- Worker processing

increase complexity.

They were included to demonstrate production-oriented engineering decisions while keeping the mandatory wallet operations simple and clearly identifiable.

---

# 39. Known Limitations

1. Authentication and authorization are not currently implemented.
2. Wallet creation is not part of the primary assessment flow.
3. Deposits are not implemented.
4. Transfers are not implemented.
5. The local payment processor does not connect to a real banking/payment provider.
6. The Service Bus Emulator is used for local development.
7. Production Azure deployment is not required to run the assessment locally.
8. The current payment implementation is a local simulation.
9. A real payment provider would require its own idempotency mechanism.
10. The current outbox publisher is designed for the assessment/local environment and would require stronger message claiming/locking when multiple publisher instances operate concurrently in production.
11. Redis caching is not currently required for the core application.
12. The seeded wallet is intended for demonstration and testing rather than production use.
13. The current payment-processing state model would need additional recovery/reconciliation states for a real external payment integration.

---

# 40. Potential Improvements

## Authentication and Authorization

Integrate Microsoft Entra ID.

```text
Client
   |
   v
API Management
   |
   v
JWT Validation
   |
   v
Wallet API
```

Wallet accounts could then be associated with authenticated users.

---

## Azure API Management

Use Azure API Management for:

- Gateway functionality
- Authentication policies
- Rate limiting
- API versioning
- Request policies
- Backend routing
- API lifecycle management

---

## Azure Key Vault

Store production secrets and sensitive configuration in Azure Key Vault.

Managed identities would be preferred over long-lived credentials.

---

## Azure Container Apps

Deploy the API and background workers as containerized workloads.

```text
Azure Container Apps
        |
        +-- Wallet API
        |
        +-- Outbox Publisher
        |
        +-- Withdrawal Worker
```

---

## Azure Database for PostgreSQL

Use a managed Azure PostgreSQL service for production persistence.

---

## Redis

Redis could be introduced for suitable read-heavy operations.

For example:

```text
GET balance
     |
     v
Redis cache
   /     \
 HIT     MISS
  |        |
  v        v
Return   PostgreSQL
           |
           v
       Update cache
```

PostgreSQL would remain the authoritative financial source.

---

## Application Insights

Application Insights could provide centralized monitoring for:

- HTTP requests
- Exceptions
- Database dependencies
- Service Bus operations
- Worker processing
- Performance
- Distributed tracing

---

## Real Payment Provider

Replace the local payment processor with a real external implementation:

```text
IPaymentProcessor
       |
       v
ExternalPaymentProcessor
       |
       v
Bank / Payment Provider
```

The external payment provider should support an idempotency reference so that retries cannot accidentally create duplicate payments.

---

## Outbox Claiming

For multiple worker instances, the outbox publisher could be enhanced with atomic message claiming/locking.

This would prevent two publisher instances from attempting to publish the same pending outbox message concurrently.

---

# 41. Testing

The solution contains automated unit and integration tests.

Testing includes areas such as:

- Successful withdrawal
- Insufficient funds
- Missing wallet
- Idempotent withdrawal
- Idempotency conflict
- Different idempotency keys
- Transaction creation
- Event publishing
- Database persistence
- Optimistic concurrency
- Withdrawal event processing
- Payment processing
- Payment processing idempotency
- Failure handling

Run all tests with:

```bash
dotnet test
```

---

# 42. Continuous Integration

GitHub Actions are used to automatically build and test the solution.

The CI pipeline performs:

```text
Checkout
   |
   v
Install .NET
   |
   v
Restore
   |
   v
Build
   |
   v
Unit Tests
```

The purpose is to catch build or test regressions before changes are merged.

---

# 43. Development Approach

The application was developed incrementally using vertical slices.

The main progression was:

```text
1. Solution structure
        |
2. Clean Architecture
        |
3. Wallet domain
        |
4. PostgreSQL persistence
        |
5. Balance API
        |
6. Withdrawal
        |
7. Transaction records
        |
8. Idempotency
        |
9. Optimistic concurrency
        |
10. Transactional Outbox
        |
11. Azure Service Bus Emulator
        |
12. Outbox Publisher
        |
13. Service Bus Worker
        |
14. Retry / DLQ
        |
15. Payment processing abstraction
        |
16. Payment processing idempotency
        |
17. Automated tests
        |
18. React demonstration client
        |
19. CI automation
```

Each major capability was tested before moving to the next stage.

---

# 44. Assessment Scope vs Production Extensions

## Core assessment functionality

```text
✓ Get wallet balance
✓ Withdraw funds
✓ Validate sufficient funds
✓ Prevent negative balance
✓ Update wallet balance
✓ Record withdrawal transaction
✓ Publish withdrawal event
✓ Automated testing
✓ Local runnable solution
✓ Documentation
```

## Production-oriented extensions

```text
✓ Clean Architecture
✓ Idempotency
✓ Optimistic concurrency
✓ Transaction history
✓ Transactional Outbox
✓ Azure Service Bus
✓ Background Worker
✓ Retry handling
✓ Dead Letter Queue
✓ Payment processor abstraction
✓ Downstream payment idempotency
✓ Correlation IDs
✓ Structured logging
✓ Centralized error handling
✓ React client
✓ CI automation
```

The additional Azure services such as API Management, Key Vault, Container Apps, Azure PostgreSQL, Redis and Application Insights are identified as potential production improvements rather than prerequisites for the assessment.

---

# 45. AI Usage

AI tools were used as development assistants during implementation.

The primary AI tool used was **ChatGPT**.

AI assistance was used for:

- Architecture exploration
- Clean Architecture design
- .NET implementation guidance
- EF Core configuration
- PostgreSQL configuration
- Dependency injection troubleshooting
- Azure Service Bus implementation
- Worker Service implementation
- Retry and Dead Letter Queue design
- Idempotency design
- Concurrency considerations
- Unit test design
- Integration test design
- Code review
- Error analysis
- Documentation

AI-generated suggestions and code were reviewed, adapted and tested during implementation.

AI was not treated as an authoritative source. Technical decisions were evaluated against the assessment requirements, the application's architecture and actual runtime behaviour.

---

# 46. AI Usage Artifacts

The following are representative examples of prompts used during development.

## Architecture

```text
Design a Clean Architecture structure for a .NET wallet API
that supports balance retrieval, withdrawals, transactions and
withdrawal events while keeping the domain independent of
infrastructure.
```

---

## Entity Framework

```text
How should optimistic concurrency be implemented for a wallet
balance using EF Core and PostgreSQL?
```

---

## Transactional Outbox

```text
Explain how to implement a transactional outbox pattern in
ASP.NET Core with PostgreSQL so that a wallet update and
withdrawal event are persisted atomically.
```

---

## Azure Service Bus

```text
Explain how to implement an Azure Service Bus consumer in a
.NET Worker Service using manual message completion, retry
handling and dead-letter processing.
```

---

## Dependency Injection

```text
Why can't a singleton Service Bus consumer depend directly on
a scoped application service, and how should dependency scopes
be handled in a background worker?
```

---

## Testing

```text
Create unit tests for a withdrawal event processor that verify
successful processing, transient payment failures and permanent
payment validation failures.
```

---

## Idempotency

```text
How can a downstream Service Bus consumer prevent duplicate
payment processing when Service Bus provides at-least-once
delivery?
```

---

## Architectural Review

AI assistance was also used interactively to identify design problems during implementation.

For example, an initial repository abstraction was placed in the Infrastructure layer while an Application service needed to consume it.

This was identified as an undesirable dependency direction.

The design was changed so that:

```text
Application
    |
    +-- Defines repository interface
              ^
              |
Infrastructure
    +-- Implements repository
```

This preserves the intended Clean Architecture dependency direction.

---

# 47. Responsible AI Development

AI-generated code was treated as a starting point rather than automatically accepted implementation.

The development process included:

1. Generate or discuss an approach with AI.
2. Review the proposed architecture.
3. Compare the proposal against the assessment requirements.
4. Implement the change.
5. Build the solution.
6. Run automated tests.
7. Run the application locally.
8. Validate runtime behaviour.
9. Correct architectural or implementation issues when discovered.

Examples of issues discovered and corrected during development include:

- Dependency injection lifetime mismatches
- Application/Infrastructure dependency direction
- Worker dependency registration
- Database connection configuration
- EF Core package requirements
- Service Bus message handling behaviour

This workflow ensured that AI assistance supported developer decision-making rather than replacing it.

---

# 48. Local Development Checklist

Before running the complete system:

```text
[ ] Docker Desktop running
[ ] PostgreSQL container running
[ ] Service Bus Emulator running
[ ] Database migrations applied
[ ] API running
[ ] Worker running
[ ] React client running
```

Then verify:

```text
[ ] GET balance works
[ ] Withdrawal succeeds with sufficient funds
[ ] Insufficient withdrawal is rejected
[ ] Balance is updated
[ ] Transaction is created
[ ] Outbox message is created
[ ] Outbox publisher sends the event
[ ] Service Bus consumer receives the event
[ ] Payment processor processes the event
[ ] Message is completed
```

---

# 49. Repository

The complete source code, commit history and documentation are maintained in the Git repository.

```text
<repository-url>
```

---

# 50. License

This project was developed as part of a technical assessment.
