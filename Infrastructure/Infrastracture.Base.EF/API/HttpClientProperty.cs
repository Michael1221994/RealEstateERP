using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastracture.Base.EF.API
{
    public class HttpClientProperty
    {
        public HttpClientProperty()
        {

        }
        public string BaseUrl { get; set; }
        public string ApiKey { get; set; }
        public string Entity { get; set; }
    }
}
