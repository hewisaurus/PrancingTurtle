using System.Collections.Generic;

namespace Database.Helpers
{
    public static class SqlBuilderRepository
    {
        public static void CreateBuilder(Dictionary<string, object> filters, string orderBy,
            int page, int pageSize, string pagedQuery, string totalQuery,
            out SqlBuilder.Template selectTemplate, out SqlBuilder.Template countTemplate)
        {
            var builder = new SqlBuilder();

            //MSSQL
            //var start = (page - 1) * pageSize + 1;
            //var finish = page * pageSize;
            //MYSQL
            var offset = (page - 1) * pageSize;
            var total = pageSize;

            //MSSQL
            //selectTemplate = builder.AddTemplate(pagedQuery, new { start, finish });
            //MYSQL
            selectTemplate = builder.AddTemplate(pagedQuery, new { offset, total });
            countTemplate = builder.AddTemplate(totalQuery);

            foreach (KeyValuePair<string, object> kvp in filters)
            {
                if (kvp.Value is int )
                {
                    builder.Where(string.Format("{0} = {1}", kvp.Key, (int)kvp.Value));
                }
                else if (kvp.Value is long)
                {
                    builder.Where(string.Format("{0} = {1}", kvp.Key, (long)kvp.Value));
                }
                else if (kvp.Value is string)
                {
                    builder.Where(kvp.Value.ToString().Contains("NOT NULL")
                        ? string.Format("{0} IS NOT NULL", kvp.Key)
                        : string.Format("{0} LIKE '%{1}%'", kvp.Key, kvp.Value));
                }
                else if (kvp.Value is bool)
                {
                    builder.Where(string.Format("{0} = '{1}'", kvp.Key, kvp.Value));
                }
            }

            builder.OrderBy(orderBy);
        }
    }
}
