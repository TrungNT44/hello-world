namespace Med.Service.Impl.Common
{
    using Med.Entity.Common;
    using App.Common.Data;
    using App.Common;
    using Med.Service.Common;
    using Med.Repository.Common;
    using global::System.Collections.Generic;

    internal class LanguageService : ILanguageService
    {
        public void Add(IList<Language> languages)
        {
            using (IUnitOfWork uow = new App.Common.Data.UnitOfWork(RepositoryType.MSSQL))
            {
                ILanguageRepository repository = App.Common.DI.IoC.Container.Resolve<ILanguageRepository>();
                foreach (Language item in languages)
                {
                    repository.AddIfNotExist(item);
                }

                uow.Commit();
            }
        }
    }
}
