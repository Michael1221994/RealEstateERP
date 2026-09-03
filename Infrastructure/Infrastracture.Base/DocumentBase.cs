using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SES.WINSSAS.Common.Enums;
namespace Infrastracture.Base
{
    public class DocumentRequestBase
    {
        /// <summary>
        /// Serves as object name for s3 storage
        /// </summary>
        public Guid DocumentIdentifier { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string Bucket { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;

    }
    public class DocumentBase
    {
        public Guid DocumentIdentifier { get; set; }
        public string Folder { get; set; } = string.Empty;
        public string Extension { get; set; }= string.Empty;
        public string Bucket { get; set; } = string.Empty;
        public byte[] Content { get; set; }
        public string ContentType { get; set; } = "application/pdf";
        public DocumentTypeEnum DocumentTypeEnum { get; set; }
        public Dictionary<string, string> Metadata { get; set; }=new Dictionary<string, string>();
    }
}
