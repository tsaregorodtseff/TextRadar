using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Data.Entity;
using TextRadarFuzzySearchDemo.Models;

namespace TextRadarFuzzySearchDemo
{
    public class MvcApplication : System.Web.HttpApplication
    {

        public string[] BookText;
        public int BookTextNumberOfPages;

        protected void Application_Start()
        {

            BookText = Models.SearchInBook.СreatePages();
            BookTextNumberOfPages = BookText.Length;

            Application["BookText"] = BookText;
            Application["BookTextNumberOfPages"] = BookTextNumberOfPages;

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

        }
    }
}
