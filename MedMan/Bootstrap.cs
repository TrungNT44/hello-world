using App.Common.DI;
using App.Common.Authorize;
using MedMan.Share.Authorize;
using App.Common.Base;

namespace MedMan
{
    public class Bootstrap : App.Common.Tasks.BaseTask<IBaseContainer>, IBootstrapper
    {
        public Bootstrap():base(AppBase.Instance.AppType){}
        public override void Execute(IBaseContainer context)
        {
            base.Execute(context);
        }  

        public override void RegisterForWeb(IBaseContainer context)
        {
            context.RegisterSingleton<IUserLoginAuthorization, UserLoginAuthorization>();
        }
        public override void RegisterForNonWeb(IBaseContainer context)
        {
            context.RegisterSingleton<IUserLoginAuthorization, UserLoginAuthorization>();
        }
    }
}