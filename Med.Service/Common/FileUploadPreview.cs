namespace Med.Service.Common
{
    using App.Common.Data;
    using App.Common.Mapping;
    using global::System;
    using Med.Entity.Common;

    public class FileUploadPreview : BaseEntity, IMappedFrom<FileUpload>
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public string Ext { get; set; }
        public Guid ParentId { get; set; }
        public long Size { get; set; }
    }
}
