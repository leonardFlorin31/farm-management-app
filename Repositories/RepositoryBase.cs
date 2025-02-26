namespace licenta.Repositories;
using System.Data.SqlClient;

public abstract class RepositoryBase
{
    private readonly string _connectionString;
    
    protected RepositoryBase()
    {
        _connectionString = "Server=DESKTOP-9HFOFLP\\SQLEXPRESS01; Database=LicentaDB; Integrated Security=True";
    }
    
    protected SqlConnection GetConnection()
    {
        return new SqlConnection(_connectionString);
    }
}