﻿using System.Web.Mvc;

namespace Devesprit.DigiCommerce
{ 
    public partial class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}