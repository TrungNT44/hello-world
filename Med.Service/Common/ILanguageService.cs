namespace Med.Service.Common
{
    using Med.Entity.Common;
    using global::System.Collections.Generic;

    public interface ILanguageService
    {
        void Add(IList<Language> languages);
    }
}
