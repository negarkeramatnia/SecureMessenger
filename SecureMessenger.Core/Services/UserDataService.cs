using SecureMessenger.Core.Models;
using System;
using System.Data.SqlClient;
using System.IO;

namespace SecureMessenger.Core.Services
{
    public class UserDataService
    {
        private string _connectionString;

        public UserDataService()
        {
            // This finds the path to your running .exe (e.g., in the bin\Debug\net8.0-windows folder)
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // This goes up three levels to find your SecureMessenger.UI project folder
            string projectRoot = Path.GetFullPath(Path.Combine(baseDirectory, @"..\..\.."));

            // This combines the project path with your database file name
            string dbFilePath = Path.Combine(projectRoot, "MessengerDatabase.mdf");

            // Build the new connection string. Notice we have REMOVED "User Instance=true".
            _connectionString = $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={dbFilePath};Integrated Security=True;Connect Timeout=30";
        }

        public bool UserExists(string username)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                // Use parameterized queries to prevent SQL Injection
                string sql = "SELECT COUNT(1) FROM Users WHERE Username = @Username";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    int count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        public User GetUserByUsername(string username)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT * FROM Users WHERE Username = @Username";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                Id = (int)reader["Id"],
                                Username = (string)reader["Username"],
                                PasswordHash = (string)reader["PasswordHash"],
                                Salt = (byte[])reader["Salt"],
                                PublicKey = (byte[])reader["PublicKey"],
                                EncryptedPrivateKey = (byte[])reader["EncryptedPrivateKey"],
                                PrivateKeyNonce = (byte[])reader["PrivateKeyNonce"],
                                PrivateKeyAuthTag = (byte[])reader["PrivateKeyAuthTag"]
                            };
                        }
                    }
                }
            }
            return null; // User not found
        }

        public bool CreateUser(User user)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = @"INSERT INTO Users (Username, PasswordHash, Salt, PublicKey, EncryptedPrivateKey, PrivateKeyNonce, PrivateKeyAuthTag)
                     VALUES (@Username, @PasswordHash, @Salt, @PublicKey, @EncryptedPrivateKey, @PrivateKeyNonce, @PrivateKeyAuthTag)";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Username", user.Username);
                    command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                    command.Parameters.AddWithValue("@Salt", user.Salt);
                    command.Parameters.AddWithValue("@PublicKey", user.PublicKey);
                    command.Parameters.AddWithValue("@EncryptedPrivateKey", user.EncryptedPrivateKey);
                    command.Parameters.AddWithValue("@PrivateKeyNonce", user.PrivateKeyNonce);
                    command.Parameters.AddWithValue("@PrivateKeyAuthTag", user.PrivateKeyAuthTag);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }
        public List<string> GetAllUsernames()
        {
            var usernames = new List<string>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT Username FROM Users ORDER BY Username ASC";
                using (var command = new SqlCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            usernames.Add((string)reader["Username"]);
                        }
                    }
                }
            }
            return usernames;
        }
    }
}