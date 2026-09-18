using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Application.Features.WalletAccounts.Withdraw.Payment
{
    public class PaymentValidationException : Exception
    {
        public PaymentValidationException(string message)
            : base(message)
        {
        }
    }
}
