using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Application.Exceptions
{
    public class WalletConcurrencyException : Exception
    {
        public WalletConcurrencyException()
            : base("The wallet was modified by another request. Please retry.")
        {
        }
    }
}
