using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastracture.Base.BaseEntities
{
    public class FileMetadata
    {
        public Guid? OwnerIdentifier { get; set; }
        public Guid? CaseId { get; set; }
        public Guid? MotionId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string? Extention { get; set; }
        public string? FileName { get; set; }
        public string? GivenName { get; set; }
        public string? Description { get; set; }
    }
    public class FileDTO : FileMetadata
    {
        public string? ContentType { set; get; } = string.Empty;
        public byte[]? File { get; set; }

        public DocumentBase GetDocumentBase()
        {

            return new DocumentBase()
            {
                Content = File,
                ContentType = ContentType,
                Extension = Extention,
            };
        }
    }

}
