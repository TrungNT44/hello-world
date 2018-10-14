using System;
using Microsoft.Owin;
using Owin;
using Hangfire;
using Hangfire.SqlServer;
using App.Configuration;
using Med.Web.Helpers;
using App.Common.Helpers;

[assembly: OwinStartup(typeof(MedMan.Startup))]

namespace MedMan
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            LogHelper.Info("Configure the Hangfire.");
            GlobalConfiguration.Configuration
               .UseSqlServerStorage(MachineConfig.Instance.Config.HangfireDbConnection.StringValue, new SqlServerStorageOptions { QueuePollInterval = TimeSpan.FromSeconds(1) });

            //app.UseHangfireDashboard();
            app.UseHangfireServer();
        }
    }
}
