using SecureMessenger.Core.Models;
using System;
using System.Data.SqlClient;
using System.IO;

namespace SecureMessenger.Core.Services
{
    public class UserDataService
    {
        // This is the connection string for your LocalDB file.
        // Note the |DataDirectory|, which is a special placeholder for your app's running directory.
        // ADD THIS NEW FIELD (it's not readonly anymore)
        private string _connectionString;

        // ADD THIS CONSTRUCTOR
        public UserDataService()
        {
            // Get the absolute path to the directory where your .exe is running (e.g., the bin\Debug\net8.0-windows folder)
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Combine that path with your database file name to get the full path
            string dbFilePath = System.IO.Path.Combine(baseDirectory, "MessengerDatabase.mdf");

            // Build the new connection string using the full, explicit path
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