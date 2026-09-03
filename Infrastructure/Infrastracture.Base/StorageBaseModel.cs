using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastracture.Base
{
    public class StorageBaseModel
    {
        public StorageBaseModel()
        {
            this.IsActive = true;
            this.IsDeleted = false;
            this.CreatedOn = DateTime.Now.ToUniversalTime();
        }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
