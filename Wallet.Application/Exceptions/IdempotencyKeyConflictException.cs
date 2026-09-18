using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Application.Exceptions
{
    public class IdempotencyKeyConflictException : Exception
    {
        public IdempotencyKeyConflictException()
            : base(
                "The idempotency key has already been used with a different request.")
        {
        }
    }
}
