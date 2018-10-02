using System.Collections.Generic;
namespace Med.ServiceModel.Response
{
    public class PagingResultModel<T> where T : class
    {
        public IEnumerable<T> Results { get; set; }

        public int TotalSize { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public PagingResultModel(IEnumerable<T> results, int totalSize)
        {
            this.Results = results;
            this.TotalSize = totalSize;
        }

        public PagingResultModel(IEnumerable<T> result, int totalSize, int pageSize, int pageIndex)
            : this(result, totalSize)
        {
            this.PageSize = pageSize;
            this.PageIndex = pageIndex;
        }
    }
}
