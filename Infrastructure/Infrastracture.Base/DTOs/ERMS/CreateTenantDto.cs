using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastracture.Base.DTOs.ERMS
{
    public class CreateTenantDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string Code { get; set; }
        public string? Tag { get; set; }
        public bool? IsActive { get; set; }
    }
}
