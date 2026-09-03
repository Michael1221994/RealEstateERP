using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastracture.Base.BaseEntities
{
    public class JwtSettings
    {
        public string Name { get; set; }
        public string Issuer { get; set; } = string.Empty;
        public string AudienceId { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public int ExpirationMinutes { get; set; } = 60;

    }
}
