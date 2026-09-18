using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Application.Features.WalletAccounts.Withdraw.Payment
{
    public class PaymentProcessingException : Exception
    {
        public PaymentProcessingException(string message)
            : base(message)
        {
        }

        public PaymentProcessingException(
            string message,
            Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
