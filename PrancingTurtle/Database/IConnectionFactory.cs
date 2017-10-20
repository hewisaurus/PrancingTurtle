using System.Data.Common;

namespace Database
{
    public interface IConnectionFactory
    {
        DbConnection Create();
        string GetConnectionString();
    }
}
