using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Application.Abstractions
{
    public sealed record IdempotencyRecord(
        Guid WalletAccountId,
        string IdempotencyKey,
        string RequestHash,
        string ResponsePayload,
        int StatusCode,
        DateTime CreatedAt);
}
