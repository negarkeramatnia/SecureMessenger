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
                // The SQL query now uses the new columns
                string sql = @"INSERT INTO Messages (RecipientUsername, TargetPreKeyId, SenderIdentityKey, SenderEphemeralKey, Ciphertext, Timestamp, IsEdited)
                     VALUES (@Recipient, @PreKeyId, @IdentityKey, @EphemeralKey, @Ciphertext, @Timestamp, @IsEdited)";
                using (var command = new SqlCommand(sql, connection))
                {
                    // The parameters now match the properties in your new Message.cs
                    command.Parameters.AddWithValue("@Recipient", message.RecipientUsername);
                    command.Parameters.AddWithValue("@PreKeyId", message.TargetPreKeyId);
                    command.Parameters.AddWithValue("@IdentityKey", message.SenderIdentityKey);
                    command.Parameters.AddWithValue("@EphemeralKey", message.SenderEphemeralKey);
                    command.Parameters.AddWithValue("@Ciphertext", message.Ciphertext);
                    command.Parameters.AddWithValue("@Timestamp", message.Timestamp);
                    command.Parameters.AddWithValue("@IsEdited", message.IsEdited);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
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
                            // Map the new database columns to the new Message object properties
                            messages.Add(new Message
                            {
                                Id = (int)reader["Id"],
                                RecipientUsername = (string)reader["RecipientUsername"],
                                TargetPreKeyId = (int)reader["TargetPreKeyId"],
                                SenderIdentityKey = (byte[])reader["SenderIdentityKey"],
                                SenderEphemeralKey = (byte[])reader["SenderEphemeralKey"],
                                Ciphertext = (byte[])reader["Ciphertext"],
                                Timestamp = (DateTime)reader["Timestamp"],
                                IsEdited = (bool)reader["IsEdited"]
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
                            return new Message
                            {
                                Id = (int)reader["Id"],
                                RecipientUsername = (string)reader["RecipientUsername"],
                                TargetPreKeyId = (int)reader["TargetPreKeyId"],
                                SenderIdentityKey = (byte[])reader["SenderIdentityKey"],
                                SenderEphemeralKey = (byte[])reader["SenderEphemeralKey"],
                                Ciphertext = (byte[])reader["Ciphertext"],
                                Timestamp = (DateTime)reader["Timestamp"],
                                IsEdited = (bool)reader["IsEdited"]
                            };
                        }
                    }
                }
            }
            return null; // Message not found
        }
    }
}