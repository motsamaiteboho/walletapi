using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wallet.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NameIdempotencyUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_idempotency_records_WalletAccountId_IdempotencyKey",
                table: "idempotency_records",
                newName: "ux_idempotency_wallet_key");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "ux_idempotency_wallet_key",
                table: "idempotency_records",
                newName: "IX_idempotency_records_WalletAccountId_IdempotencyKey");
        }
    }
}
