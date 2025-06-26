using SecureMessenger.Core.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;

namespace SecureMessenger.Core.Services
{
    public class MessageDataService
    {
        private readonly string _connectionString;

        public MessageDataService()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectRoot = Path.GetFullPath(Path.Combine(baseDirectory, @"..\..\.."));
            string dbFilePath = Path.Combine(projectRoot, "MessengerDatabase.mdf");
            _connectionString = $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={dbFilePath};Integrated Security=True;Connect Timeout=30";
        }

        public bool CreateMessage(Message message)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = @"INSERT INTO Messages (RecipientUsername, TargetPreKeyId, SenderIdentityKey, SenderEphemeralKey, Ciphertext, Timestamp)
                             VALUES (@Recipient, @TargetPreKeyId, @SenderIdentityKey, @SenderEphemeralKey, @Ciphertext, @Timestamp)";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Recipient", message.RecipientUsername);
                    command.Parameters.AddWithValue("@TargetPreKeyId", message.TargetPreKeyId);
                    command.Parameters.AddWithValue("@SenderIdentityKey", message.SenderIdentityKey);
                    command.Parameters.AddWithValue("@SenderEphemeralKey", message.SenderEphemeralKey);
                    command.Parameters.AddWithValue("@Ciphertext", message.Ciphertext);
                    command.Parameters.AddWithValue("@Timestamp", message.Timestamp);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public List<Message> GetMessagesForUser(string username)
        {
            var messages = new List<Message>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT * FROM Messages WHERE RecipientUsername = @Username ORDER BY Timestamp ASC";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            messages.Add(new Message
                            {
                                Id = (int)reader["Id"],
                                RecipientUsername = (string)reader["RecipientUsername"],
                                TargetPreKeyId = (int)reader["TargetPreKeyId"],
                                SenderIdentityKey = (byte[])reader["SenderIdentityKey"],
                                SenderEphemeralKey = (byte[])reader["SenderEphemeralKey"],
                                Ciphertext = (byte[])reader["Ciphertext"],
                                Timestamp = (DateTime)reader["Timestamp"]
                            });
                        }
                    }
                }
            }
            return messages;
        }

        public void DeleteMessage(int messageId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = "DELETE FROM Messages WHERE Id = @MessageId";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@MessageId", messageId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public Message GetMessageById(int messageId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT * FROM Messages WHERE Id = @MessageId";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@MessageId", messageId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // This re-uses the logic from your GetMessagesForUser method
                            return new Message
                            {
                                Id = (int)reader["Id"],
                                RecipientUsername = (string)reader["RecipientUsername"],
                                SenderIdentityKey = (byte[])reader["SenderIdentityKey"],
                                SenderEphemeralKey = (byte[])reader["SenderEphemeralKey"],
                                Ciphertext = (byte[])reader["Ciphertext"],
                                Timestamp = (DateTime)reader["Timestamp"]
                            };
                        }
                    }
                }
            }
            return null; // Message not found
        }
    }
}