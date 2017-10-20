using System.Collections.Generic;

namespace Database
{
    public static class SqlBuilderRepository
    {
        public static void CreateBuilder(Dictionary<string, object> filters, string orderBy, int offset,
            int pageSize, string pagedQuery, string totalQuery, out SqlBuilder.Template selectTemplate, out SqlBuilder.Template countTemplate, bool useOr = false, bool skipWhere = false)
        {
            var builder = new SqlBuilder();

            selectTemplate = builder.AddTemplate(pagedQuery, new { offset, total = pageSize });
            countTemplate = builder.AddTemplate(totalQuery);
            if (!skipWhere)
            {
                foreach (KeyValuePair<string, object> kvp in filters)
                {
                    if (kvp.Value is int)
                    {
                        if (useOr)
                        {
                            builder.OrWhere($"{kvp.Key} = {(int)kvp.Value}");
                        }
                        else
                        {
                            builder.Where($"{kvp.Key} = {(int)kvp.Value}");
                        }
                    }
                    else if (kvp.Value is string)
                    {
                        if (useOr)
                        {
                            builder.OrWhere(kvp.Value.ToString().Contains("NOT NULL")
                                ? $"{kvp.Key} IS NOT NULL"
                                : $"{kvp.Key} LIKE '%{kvp.Value}%'");
                        }
                        else
                        {
                            builder.Where(kvp.Value.ToString().Contains("NOT NULL")
                                ? $"{kvp.Key} IS NOT NULL"
                                : $"{kvp.Key} LIKE '%{kvp.Value}%'");
                        }
                    }
                    else if (kvp.Value is bool)
                    {
                        if (useOr)
                        {
                            builder.OrWhere($"{kvp.Key} = '{kvp.Value}'");
                        }
                        else
                        {
                            builder.Where($"{kvp.Key} = '{kvp.Value}'");
                        }
                    }
                }
            }

            builder.OrderBy(orderBy);
        }
    }
}