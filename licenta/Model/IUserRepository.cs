using System.Net;

namespace licenta.Model;

public interface IUserRepository
{
    bool AutenticateUser(NetworkCredential credential);
    
    void AddUser(UserModel user);
    void EditUser(UserModel user);
    void RemoveUser(UserModel user);
    
    UserModel GetUserById(int id);
    UserModel GetUserByUsername(string username);
    IEnumerable<UserModel> GetAllUsers();
    
}