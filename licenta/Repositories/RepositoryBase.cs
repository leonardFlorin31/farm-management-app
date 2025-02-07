namespace licenta.Repositories;
using System.Data.SqlClient;

public abstract class RepositoryBase
{
    private readonly string _connectionString;
    
    protected RepositoryBase(string connectionString)
    {
        _connectionString = "Server=(local); Database=licenta; Integrated Security=True";
    }
    
    protected SqlConnection GetConnection()
    {
        return new SqlConnection(_connectionString);
    }
}