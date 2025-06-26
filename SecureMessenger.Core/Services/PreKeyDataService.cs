using SecureMessenger.Core.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;

namespace SecureMessenger.Core.Services
{
    public class PreKeyDataService
    {
        private readonly string _connectionString;

        public PreKeyDataService()
        {
            // This builds the absolute path to the database, which is a robust method.
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectRoot = Path.GetFullPath(Path.Combine(baseDirectory, @"..\..\.."));
            string dbFilePath = Path.Combine(projectRoot, "MessengerDatabase.mdf");
            _connectionString = $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={dbFilePath};Integrated Security=True;Connect Timeout=30";
        }

        // Stores a batch of pre-keys for a user.
        public void StorePreKeys(string username, List<PreKey> preKeys)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                // Using a loop to insert multiple keys. In a high-performance system, a bulk insert would be used.
                foreach (var key in preKeys)
                {
                    string sql = "INSERT INTO PreKeys (Username, KeyId, PublicKey) VALUES (@Username, @KeyId, @PublicKey)";
                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@KeyId", key.KeyId);
                        command.Parameters.AddWithValue("@PublicKey", key.PublicKey);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        // Fetches one available pre-key for a given user.
        public PreKey GetOnePreKey(string username)
        {
            PreKey key = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                // Select the top available key.
                string sql = "SELECT TOP 1 * FROM PreKeys WHERE Username = @Username";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            key = new PreKey
                            {
                                Id = (int)reader["Id"],
                                Username = (string)reader["Username"],
                                KeyId = (int)reader["KeyId"],
                                PublicKey = (byte[])reader["PublicKey"]
                            };
                        }
                    }
                }
            }
            return key;
        }

        // Deletes a pre-key after it has been used for a session.
        public void DeletePreKey(int preKeyId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = "DELETE FROM PreKeys WHERE Id = @PreKeyId";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@PreKeyId", preKeyId);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}