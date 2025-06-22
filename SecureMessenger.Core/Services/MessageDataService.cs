// In SecureMessenger.Core/Services/MessageDataService.cs
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

            // We build the connection string and remove "User Instance=true"
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
                                Timestamp = (DateTime)reader["Timestamp"]
                            });
                        }
                    }
                }
            }
            return messages;
        }
    }
}