// Models/User.cs
using System;

namespace ForestDecisionMauiApp.Models
{
    public class User
    {
        public string UserID { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; } // 新增：存储密码的哈希值
        public string FullName { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; } // 新增：用户创建时间

        public override string ToString()
        {
            return $"User ID: {UserID}, Username: {Username}, Role: {Role}, Created: {CreatedAt.ToShortDateString()}";
        }
    }
}