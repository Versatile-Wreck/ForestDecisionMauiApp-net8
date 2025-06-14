// Services/UserService.cs
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ForestDecisionMauiApp.Models;
using System.Diagnostics;

namespace ForestDecisionMauiApp.Services
{

    /* 
    主要负责：
            用户注册：创建新用户账户
            用户登录：验证用户身份
            密码安全：对密码进行加密处理
    */
    public class UserService
    {
        // 依赖注入的 DatabaseService 实例
        private readonly DatabaseService _dbService; // 依赖 DatabaseService

        public UserService(DatabaseService dbService)
        {
            _dbService = dbService;
        }




        // -----用户注册----- //

        public User RegisterUser(string username, string password, string fullName, UserRole role)
        {
            // 首先检查数据库中用户名是否存在
            if (_dbService.GetUserByUsername(username) != null)
            {
                Debug.WriteLine("错误：用户名已存在。"); // DatabaseService AddUser 也会处理，但这里可以提前检查
                return null;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                Debug.WriteLine("错误：密码不能为空。");
                return null;
            }

            // 动态确定下一个 UserID (简单示例，生产环境可能有更复杂逻辑)
            // 为了简单起见，我们可以在这里生成一个 GUID 或者基于现有用户数量（如果 GetAllUsers 存在）
            // 对于此示例，让 UserID 为 GUID
            var newUser = new User
            {
                UserID = "U-" + Guid.NewGuid().ToString().Substring(0, 8), // 更唯一的ID
                Username = username,
                PasswordHash = HashPassword(password),
                FullName = fullName,
                Role = role,
                CreatedAt = DateTime.UtcNow // 使用 UTC 时间
            };

            if (_dbService.AddUser(newUser))
            {
                Debug.WriteLine("用户注册成功！");
                return newUser;
            }
            else
            {
                // AddUser 内部会打印更具体的数据库错误
                Debug.WriteLine("用户注册失败。");
                return null;
            }
        }





        // -----用户登录----- //

        public async Task<User> LoginUser(string username, string password)
        {
            var user = await _dbService.GetUserByUsername(username);

            if (user == null)
            {
                // GetUserByUsername 内部不打印 "用户名不存在"，所以这里可以打印
                Debug.WriteLine("错误：用户名不存在。");
                return null;
            }

            string hashedPassword = HashPassword(password);
            if (user.PasswordHash == hashedPassword)
            {
                Debug.WriteLine($"欢迎回来, {user.FullName}!");
                return user;
            }
            else
            {
                Debug.WriteLine("错误：密码不正确。");
                return null;
            }
        }




        // -----密码安全----- //

        // 使用 SHA256 哈希算法对密码进行加密
        private string HashPassword(string password)
        {
            // (保留之前的 HashPassword 方法)
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }



        // -----其他用户管理方法----- //
        public List<User> GetAllUsers()
        {
            return _dbService.GetAllUsers();
        }

        public bool UpdateUser(User user)
        {
            return _dbService.UpdateUser(user);
        }

        public bool DeleteUser(string userId)
        {
            return _dbService.DeleteUser(userId);
        }
        public async Task<User> GetUserByUsername(string username)
        {
            return await _dbService.GetUserByUsername(username);
        }
        public User GetUserById(string userId)
        {
            return _dbService.GetUserById(userId);
        }
    }
}