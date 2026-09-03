using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastracture.Base.EF.Minio
{
    public class MinioServerConfigurationModel
    {
        /// <summary>
        /// 	<Domain-name> or <ip:port> of your object storage
        /// </summary>
        public string EndPoint { get; set; } = string.Empty;
        /// <summary>
        /// User ID that uniquely identifies your account
        /// </summary>
        public string AccessKey { get; set; } = string.Empty;
        /// <summary>
        /// Password to your account
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;
        /// <summary>
        /// Enable/disable HTTPS support (default=true)
        /// </summary>
        public bool IsSecure { get; set; } = true; 
    }
}
