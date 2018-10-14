using System.Web.Http;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Med.Service.Base;
using Med.Service.Impl.Delivery;

namespace MedMan
{
    public class WebMvcInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            //container.Register(
            //    Classes
            //        .FromAssemblyContaining(typeof(DeliveryNoteService))
            //        .BasedOn<BaseService>()
            //        .LifestylePerWebRequest()
            //    );
        }
    }
}