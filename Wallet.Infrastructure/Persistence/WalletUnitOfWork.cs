using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Application.Abstractions;
using Wallet.Application.Exceptions;

namespace Wallet.Infrastructure.Persistence
{
    public class WalletUnitOfWork : IUnitOfWork
    {
        private readonly WalletDbContext _context;

        public WalletUnitOfWork(
            WalletDbContext context)
        {
            _context = context;
        }

        public async Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.SaveChangesAsync(
                    cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new WalletConcurrencyException();
            }
            catch (DbUpdateException exception)when (IsIdempotencyConstraintViolation(exception))
            {
                throw new DuplicateIdempotencyKeyException();
            }
        }
        private static bool IsIdempotencyConstraintViolation( DbUpdateException exception)
        {
            return exception.InnerException is PostgresException postgresException
                && postgresException.SqlState ==
                   PostgresErrorCodes.UniqueViolation
                && postgresException.ConstraintName ==
                   "ux_idempotency_wallet_key";
        }
    }
}
