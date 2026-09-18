using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Application.Exceptions
{
    public class DuplicateIdempotencyKeyException : Exception
    {
        public DuplicateIdempotencyKeyException()
            : base("The idempotency key already exists.")
        {
        }
    }
}
