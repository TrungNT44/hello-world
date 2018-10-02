namespace Med.Service.Common.File
{
    using App.Common;
    using App.Common.Providers;
    using Med.Entity.Common;
    using global::System.Collections.Generic;
    using global::System;
    using global::System.Drawing;

    public interface IFileService
    {
        IList<FileUploadResponse> UploadFiles(List<MultipartFormDataMemoryStreamProvider.FileInfo> fileData);
        string GetPhotoAsBase64(Guid id);
        FileUpload Get(Guid id);
        Bitmap GetThumbnail(Guid id, ThumbnailSize medium);
    }
}
