using System.Collections.Generic;

namespace Database.Helpers
{
    public class PagedData<T>
    {
        public IEnumerable<T> Data { get; set; }
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public PagedData()
        {
            Data = new List<T>();
            TotalRecords = 0;
        }

        public PagedData(int page, int pageSize)
        {
            PageNumber = page;
            PageSize = pageSize;
        }
    }
}
