using System;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using App.Common;
using log4net;
using MedMan.App_Start;
using Med.Web.Extensions;
using sThuoc.Filter;
using System.Web.Http;
using App.Common.Session;
using Med.Common;
using System.Web.Helpers;
using Castle.Windsor;
using Castle.Windsor.Installer;
using App.Common.DI;

namespace MedMan
{
    public class MvcApplication : HttpApplication
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MvcApplication));
        private static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["SimpleSecurityConnection"].ConnectionString;

        void Application_Error(Object sender, EventArgs e)
        {
            var ex = Server.GetLastError().GetBaseException();

            Log.Error("App_Error", ex);
        }

        private void InitializeServiceLocator()
        {
            IWindsorContainer container = (IWindsorContainer)IoC.Container.Instance;
            container.Install(FromAssembly.This());
            GlobalConfiguration.Configuration.DependencyResolver = new App.Common.DI.DependencyResolver(container.Kernel);
        }

        protected void Application_Start()
        {
            MainApp.Instance.Initialize(ApplicationType.MVC, this, MedConstants.WebAppName);
            AreaRegistration.RegisterAllAreas();
            AntiForgeryConfig.SuppressIdentityHeuristicChecks = true;
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            //First register WebApi router
            GlobalConfiguration.Configure(WebApiConfig.Register);
            //and register Default MVC Route after
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            WebSecurity.Register();
            // InitializeServiceLocator();
            SessionStateInstaller.Register();
            //System.Data.SqlClient.SqlDependency.Start(ConnectionString);
            ModelBinders.Binders.Add(typeof(DateTime?), new DateTimeBinder());
            MainApp.Instance.StartApp();
            //MainApp.Instance.NotifyUsersUpdateNewestFeatures();
        }

        protected void Application_End(object sender, EventArgs e)
        {
            System.Data.SqlClient.SqlDependency.Stop(ConnectionString);
        }
    }
}
