using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastracture.Base
{
    public interface IPDFGenerator
    {
        byte[] GeneratePdfFromRazorTemplate<T>(string templateContent, T model);

    }
}
