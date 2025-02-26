using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using licenta.Model;
using System;
using System.Text.Json.Serialization;


namespace licenta.Repositories;

public class UserRepository : IUserRepository
{
    public async Task<UserModel> GetUserByUsernameAsync(string username)
    {
        using (var client = new HttpClient())
        {
            var response = await client.GetAsync($"http://localhost:5035/api/auth/{username}");
            if (response.IsSuccessStatusCode)
            {
                var userJson = await response.Content.ReadAsStringAsync();
                Console.WriteLine(userJson);
                var userDto = JsonSerializer.Deserialize<UserData>(userJson);
                Console.WriteLine($"Id: {userDto.Id}, Type: {userDto.Id.GetType()}");
                Console.WriteLine($"Username: {userDto.Username}, Type: {userDto.Username.GetType()}");
                Console.WriteLine($"Email: {userDto.Email}, Type: {userDto.Email.GetType()}");
                Console.WriteLine($"Name: {userDto.Name}, Type: {userDto.Name.GetType()}");
                Console.WriteLine($"LastName: {userDto.LastName}, Type: {userDto.LastName.GetType()}");
                
                return new UserModel
                {
                    Id = userDto.Id,
                    Username = userDto.Username,
                    Password = string.Empty, // Nu este necesar
                    Name = userDto.Name,
                    LastName = userDto.LastName,
                    Email = userDto.Email,
                };
            }
            return null;
        }
    }

    public bool AutenticateUser(NetworkCredential credential)
    {
        throw new NotImplementedException();
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


public class UserData
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("lastName")]
    public string LastName { get; set; }
}

