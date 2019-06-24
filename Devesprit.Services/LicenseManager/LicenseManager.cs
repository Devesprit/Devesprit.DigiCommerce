using System;
using System.Collections.Generic;
using System.Linq;
using Devesprit.Core.Plugin;

namespace Devesprit.Services.LicenseManager
{
    public partial class LicenseManager : ILicenseManager
    {
        private readonly IPluginFinder _pluginFinder;

        public LicenseManager(IPluginFinder pluginFinder)
        {
            _pluginFinder = pluginFinder;
        }

        public virtual ILicenseGenerator FindLicenseGeneratorById(string id)
        {
            var licenseGenerators = _pluginFinder.GetPlugins<ILicenseGenerator>();
            return licenseGenerators.FirstOrDefault(p =>
                string.Compare(p.LicenseGeneratorServiceId, id, StringComparison.OrdinalIgnoreCase) == 0);
        }

        public virtual List<ILicenseGenerator> GetAvailableLicenseGenerators()
        {
            var licenseGenerators = _pluginFinder.GetPlugins<ILicenseGenerator>();
            return licenseGenerators.ToList();
        }
    }
}
