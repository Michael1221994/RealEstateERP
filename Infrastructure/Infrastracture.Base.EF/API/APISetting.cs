using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastracture.Base.EF.API
{
    public class APISetting
    {
        public string URL { get; set; } = string.Empty;
        public string ClientID { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string ClientSecret { get; set; } =string.Empty;
    }
}
