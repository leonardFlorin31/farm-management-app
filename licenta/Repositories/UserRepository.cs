using System.Data;
using System.Data.SqlClient;
using System.Net;
using licenta.Model;

namespace licenta.Repositories;

public class UserRepository : RepositoryBase, IUserRepository
{
    public UserRepository(string connectionString) : base(connectionString)
    {
    }

    public bool AutenticateUser(NetworkCredential credential)
    {
        bool validUser;
        using(var connection=GetConnection())
        using (var command = new SqlCommand())
        {
            connection.Open();
            command.Connection = connection;
            command.CommandText = "select * from [User] where Username=@Username and Password=@Password";
            command.Parameters.Add("@Username", SqlDbType.VarChar).Value = credential.UserName;
            command.Parameters.Add("@Password", SqlDbType.VarChar).Value = credential.Password;
            validUser = command.ExecuteScalar() == null ? false : true;
        }
        return validUser;
    }

    public void AddUser(UserModel user)
    {
        throw new NotImplementedException();
    }

    public void EditUser(UserModel user)
    {
        throw new NotImplementedException();
    }

    public void RemoveUser(UserModel user)
    {
        throw new NotImplementedException();
    }

    public UserModel GetUserById(int id)
    {
        throw new NotImplementedException();
    }

    public UserModel GetUserByUsername(string username)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<UserModel> GetAllUsers()
    {
        throw new NotImplementedException();
    }
}
