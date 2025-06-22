using SecureMessenger.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace SecureMessenger.UI
{
    // This is a temporary, in-memory store to simulate a database for UI testing.
    public static class UserStore
    {
        public static List<User> Users { get; private set; } = new List<User>();

        public static void AddUser(User user)
        {
            if (Users.Any(u => u.Username.ToLower() == user.Username.ToLower()))
            {
                // In a real app, this check would happen before trying to add.
                return;
            }
            Users.Add(user);
        }

        public static User GetUserByUsername(string username)
        {
            return Users.FirstOrDefault(u => u.Username.ToLower() == username.ToLower());
        }
    }
}