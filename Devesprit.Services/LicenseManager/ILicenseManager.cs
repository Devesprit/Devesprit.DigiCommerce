using System.Collections.Generic;

namespace Devesprit.Services.LicenseManager
{
    public partial interface ILicenseManager
    {
        ILicenseGenerator FindLicenseGeneratorById(string id);
        List<ILicenseGenerator> GetAvailableLicenseGenerators();
    }
}
