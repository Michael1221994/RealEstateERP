using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastracture.Base
{
    public interface ISMSAPI
    {
        Task<Response<bool>> SendSMS(string phone, string message, Guid transactionId);
    }
}
