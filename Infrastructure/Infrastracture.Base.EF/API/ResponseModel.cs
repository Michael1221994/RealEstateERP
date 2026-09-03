using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastracture.Base.EF.API
{
    public class ResultModel<T>
    {
        public string Error { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
        public string Error2 { get; set; }
        public string Message2 { get; set; }
        public bool Success2 { get; set; }
        public dynamic Payload { get; set; }
        public int? StatusCode { get; set; }
        public string EventState { get; set; }
        public T Value { get; set; }
    }
}
