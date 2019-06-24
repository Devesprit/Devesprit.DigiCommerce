using System;
using System.Configuration;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel;

namespace Devesprit.FileServer
{
    public partial class UserValidator: UserNamePasswordValidator
    {
        public override void Validate(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                throw new SecurityTokenException("Username and password required");

            if (!userName.Equals(ConfigurationManager.AppSettings["ServiceAdminUserName"], StringComparison.OrdinalIgnoreCase) ||
                password != ConfigurationManager.AppSettings["ServiceAdminPassword"])
                throw new FaultException($"Wrong username ({userName}) or password");
        }
    }
}