using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastracture.Base
{
    public class UserProfileBase:StorageBaseModel
    {
        public int ID { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string MiddelName { get; set; } = string.Empty;
        public string Sex { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
    }
}
