using SecureMessenger.Core.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;

namespace SecureMessenger.Core.Services
{
    public class UserDataService
    {
        private readonly string _connectionString;

        public UserDataService()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectRoot = Path.GetFullPath(Path.Combine(baseDirectory, @"..\..\.."));
            string dbFilePath = Path.Combine(projectRoot, "MessengerDatabase.mdf");
            _connectionString = $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={dbFilePath};Integrated Security=True;Connect Timeout=30";
        }

        public bool UserExists(string username)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT COUNT(1) FROM Users WHERE Username = @Username";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    return (int)command.ExecuteScalar() > 0;
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
                                IdentityPublicKey = (byte[])reader["IdentityPublicKey"],
                                EncryptedIdentityKey = (byte[])reader["EncryptedIdentityKey"],
                                PrivateKeyNonce = (byte[])reader["PrivateKeyNonce"],
                                PrivateKeyAuthTag = (byte[])reader["PrivateKeyAuthTag"]
                            };
                        }
                    }
                }
            }
            return null;
        }

        public bool CreateUser(User user)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = @"INSERT INTO Users (Username, PasswordHash, Salt, IdentityPublicKey, EncryptedIdentityKey, PrivateKeyNonce, PrivateKeyAuthTag)
                             VALUES (@Username, @PasswordHash, @Salt, @IdentityPublicKey, @EncryptedIdentityKey, @PrivateKeyNonce, @PrivateKeyAuthTag)";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Username", user.Username);
                    command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                    command.Parameters.AddWithValue("@Salt", user.Salt);
                    command.Parameters.AddWithValue("@IdentityPublicKey", user.IdentityPublicKey);
                    command.Parameters.AddWithValue("@EncryptedIdentityKey", user.EncryptedIdentityKey);
                    command.Parameters.AddWithValue("@PrivateKeyNonce", user.PrivateKeyNonce);
                    command.Parameters.AddWithValue("@PrivateKeyAuthTag", user.PrivateKeyAuthTag);

                    return command.ExecuteNonQuery() > 0;
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

        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                // This single query gets all the user data we need.
                string sql = "SELECT * FROM Users";
                using (var command = new SqlCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new User
                            {
                                Id = (int)reader["Id"],
                                Username = (string)reader["Username"],
                                PasswordHash = (string)reader["PasswordHash"],
                                Salt = (byte[])reader["Salt"],
                                IdentityPublicKey = (byte[])reader["IdentityPublicKey"],
                                EncryptedIdentityKey = (byte[])reader["EncryptedIdentityKey"],
                                PrivateKeyNonce = (byte[])reader["PrivateKeyNonce"],
                                PrivateKeyAuthTag = (byte[])reader["PrivateKeyAuthTag"]
                            });
                        }
                    }
                }
            }
            return users;
        }
    }
}