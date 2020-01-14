using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;
using Autofac.Extras.DynamicProxy;
using Devesprit.Core.Localization;
using Devesprit.Data.Enums;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.DigiCommerce.Controllers;
using Devesprit.Services.Currency;
using Devesprit.Services.Invoice;
using Devesprit.Services.Localization;
using Devesprit.Services.MemoryCache;
using Devesprit.Services.Users;
using Devesprit.WebFramework.ActionFilters;

namespace Devesprit.DigiCommerce.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Intercept(typeof(MethodCache))]
    public partial class AdministrationController : BaseController
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IUsersService _usersService;
        private readonly IUserGroupsService _userGroupsService;
        private readonly ILocalizationService _localizationService;

        public AdministrationController(IInvoiceService invoiceService, 
            IUsersService usersService,
            IUserGroupsService userGroupsService,
            ILocalizationService localizationService)
        {
            _invoiceService = invoiceService;
            _usersService = usersService;
            _userGroupsService = userGroupsService;
            _localizationService = localizationService;
        }

        public virtual ActionResult Index()
        {
            return View();
        }
    }
}