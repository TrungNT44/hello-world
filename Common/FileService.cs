namespace Med.Service.Impl.Common
{
    using App.Common.Providers;
    using App.Common.Data;
    using App.Common;
    using App.Common.Helpers;
    using App.Common.DI;
    using Med.Service.Common.File;
    using global::System;
    using Med.Entity.Common;
    using Med.Repository.Common;
    using global::System.Drawing;
    using global::System.Collections.Generic;
    using Med.Service.Base;

    internal class FileService : MedBaseService, IFileService
    {
        public FileUpload Get(Guid id)
        {
            IFileRepository repo = IoC.Container.Resolve<IFileRepository>();
            return repo.GetById(id.ToString());
        }

        public string GetPhotoAsBase64(Guid id)
        {
            IFileRepository repo = IoC.Container.Resolve<IFileRepository>();
            FileUpload file = repo.GetById(id.ToString());
            return ImageHelper.ToBase64(file.FileName, file.Content);
        }

        public Bitmap GetThumbnail(Guid id, ThumbnailSize size)
        {
            IFileRepository repo = IoC.Container.Resolve<IFileRepository>();
            FileUpload file = repo.GetById(id.ToString());
            return ImageHelper.GetThumbnail(file.FileName, file.Content, size);
        }

        public IList<FileUploadResponse> UploadFiles(List<MultipartFormDataMemoryStreamProvider.FileInfo> files)
        {
            this.ValidateUploadRequest(files);
            using (IUnitOfWork uow = new UnitOfWork(RepositoryType.MSSQL))
            {
                IFileRepository repo = IoC.Container.Resolve<IFileRepository>();
                IList<FileUploadResponse> filesUploaded = new List<FileUploadResponse>();
                foreach (MultipartFormDataMemoryStreamProvider.FileInfo file in files)
                {
                    FileUpload fileCreated = new Entity.Common.FileUpload(file.FileName, file.ContentType, file.FileSize, file.Content);
                    repo.Add(fileCreated);
                    filesUploaded.Add(ObjectHelper.Convert<FileUploadResponse>(fileCreated));
                }

                uow.Commit();
                return filesUploaded;
            }
        }

        private void ValidateUploadRequest(object files)
        {
        }
    }
}
