using System.Data;
using System.Data.SqlClient;
using System.Net;
using licenta.Model;

namespace licenta.Repositories;

public class UserRepository : RepositoryBase, IUserRepository
{

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
        UserModel user = null;
        using(var connection=GetConnection())
        using (var command = new SqlCommand())
        {
            connection.Open();
            command.Connection = connection;
            command.CommandText = "select * from [User] where Username=@Username";
            command.Parameters.Add("@Username", SqlDbType.VarChar).Value = username;
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    user = new UserModel()
                    {
                        Id = reader["Id"].ToString(),
                        Username = reader["Username"].ToString(),
                        Password = string.Empty,
                        FirstName = reader["Name"].ToString(),
                        LastName = reader["LastName"].ToString(),
                        Email = reader["Email"].ToString(),
                    };
                }
                else
                {
                    user = null;
                }
            }
        }
        return user;
    }

    public IEnumerable<UserModel> GetAllUsers()
    {
        throw new NotImplementedException();
    }
}
