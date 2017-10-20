using System.Collections.Generic;

namespace Common
{
    public class PagedData<T>
    {
        public IEnumerable<T> Data { get; set; }
        public int TotalRecords { get; set; }
        public int PageSize { get; set; }

        public PagedData()
        {
            Data = new List<T>();
            TotalRecords = 0;
        }
    }
}