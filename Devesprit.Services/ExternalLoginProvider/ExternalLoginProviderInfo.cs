using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devesprit.Services.ExternalLoginProvider
{
    public partial class ExternalLoginProviderInfo
    {
        public string ProviderName { get; set; }
        public string ProviderLoginBtnPartialUrl { get; set; }
        public int Order { get; set; }
    }
}
