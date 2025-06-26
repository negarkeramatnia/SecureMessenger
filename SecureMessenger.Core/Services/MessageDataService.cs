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

        // This is the new constructor that builds the correct path.
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
                string sql = @"INSERT INTO Messages (SenderUsername, RecipientUsername, Ciphertext, Nonce, AuthTag, EncryptedMessageKeyForSender, EncryptedMessageKeyForRecipient, Timestamp)
                             VALUES (@Sender, @Recipient, @Cipher, @Nonce, @Tag, @KeySender, @KeyRecipient, @Timestamp)";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Sender", message.SenderUsername);
                    command.Parameters.AddWithValue("@Recipient", message.RecipientUsername);
                    command.Parameters.AddWithValue("@Cipher", message.Ciphertext);
                    command.Parameters.AddWithValue("@Nonce", message.Nonce);
                    command.Parameters.AddWithValue("@Tag", message.AuthTag);
                    command.Parameters.AddWithValue("@KeySender", message.EncryptedMessageKeyForSender);
                    command.Parameters.AddWithValue("@KeyRecipient", message.EncryptedMessageKeyForRecipient);
                    command.Parameters.AddWithValue("@Timestamp", message.Timestamp);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        public List<Message> GetConversation(string user1, string user2)
        {
            var messages = new List<Message>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = @"SELECT * FROM Messages 
                             WHERE (SenderUsername = @User1 AND RecipientUsername = @User2) 
                                OR (SenderUsername = @User2 AND RecipientUsername = @User1) 
                             ORDER BY Timestamp ASC";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@User1", user1);
                    command.Parameters.AddWithValue("@User2", user2);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            messages.Add(new Message
                            {
                                Id = (int)reader["Id"],
                                SenderUsername = (string)reader["SenderUsername"],
                                RecipientUsername = (string)reader["RecipientUsername"],
                                Ciphertext = (byte[])reader["Ciphertext"],
                                Nonce = (byte[])reader["Nonce"],
                                AuthTag = (byte[])reader["AuthTag"],
                                EncryptedMessageKeyForSender = (byte[])reader["EncryptedMessageKeyForSender"],
                                EncryptedMessageKeyForRecipient = (byte[])reader["EncryptedMessageKeyForRecipient"],
                                Timestamp = (DateTime)reader["Timestamp"],
                                // --- ADDED THIS LINE ---
                                IsEdited = (bool)reader["IsEdited"]
                            });
                        }
                    }
                }
            }
            return messages;
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
                                SenderUsername = (string)reader["SenderUsername"],
                                RecipientUsername = (string)reader["RecipientUsername"],
                                Ciphertext = (byte[])reader["Ciphertext"],
                                Nonce = (byte[])reader["Nonce"],
                                AuthTag = (byte[])reader["AuthTag"],
                                EncryptedMessageKeyForSender = (byte[])reader["EncryptedMessageKeyForSender"],
                                EncryptedMessageKeyForRecipient = (byte[])reader["EncryptedMessageKeyForRecipient"],
                                Timestamp = (DateTime)reader["Timestamp"],
                                IsEdited = (bool)reader["IsEdited"]
                            };
                        }
                    }
                }
            }
            return null; // Message not found
        }
        public bool UpdateMessage(int messageId, byte[] newCiphertext, byte[] newNonce, byte[] newAuthTag)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = @"UPDATE Messages 
                             SET Ciphertext = @Cipher, Nonce = @Nonce, AuthTag = @Tag, IsEdited = 1 
                             WHERE Id = @MessageId";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@MessageId", messageId);
                    command.Parameters.AddWithValue("@Cipher", newCiphertext);
                    command.Parameters.AddWithValue("@Nonce", newNonce);
                    command.Parameters.AddWithValue("@Tag", newAuthTag);
                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        public bool DeleteMessage(int messageId, string currentUsername)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                // We only allow the sender to delete the message.
                string sql = "DELETE FROM Messages WHERE Id = @MessageId AND SenderUsername = @Username";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@MessageId", messageId);
                    command.Parameters.AddWithValue("@Username", currentUsername);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }
    }
}