// Services/DatabaseService.cs
using Microsoft.Data.Sqlite; // 引入SQLite命名空间
using System;
using System.IO;
using ForestDecisionMauiApp.Models; // 确保引用 User 模型
using System.Collections.Generic; // For GetAllUsers (if you implement it)

namespace ForestDecisionMauiApp.Services
{
    public class DatabaseService
    {
        private readonly string _databasePath;

        public DatabaseService()
        {
            // 定义数据库文件的存储位置
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appSpecificFolder = Path.Combine(appDataFolder, "ForestDecisionSystemApp");
            Directory.CreateDirectory(appSpecificFolder); // 确保文件夹存在
            _databasePath = Path.Combine(appSpecificFolder, "forest_system.db");

            InitializeDatabase();
        }

        private string GetConnectionString()
        {
            return $"Data Source={_databasePath}";
        }

        // Services/DatabaseService.cs
        // ... (保留 InitializeDatabase 方法上半部分创建 Users 表的代码) ...

        private void InitializeDatabase()
        {
            using (var connection = new SqliteConnection(GetConnectionString()))
            {
                connection.Open();

                // 开启外键约束支持 (如果每个连接都需要，或者在连接字符串中指定)
                // var pragmaCmd = connection.CreateCommand();
                // pragmaCmd.CommandText = "PRAGMA foreign_keys = ON;";
                // pragmaCmd.ExecuteNonQuery();

                var command = connection.CreateCommand();
                // 创建 Users 表 (已存在)
                command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Users (
                UserID TEXT PRIMARY KEY,
                Username TEXT UNIQUE NOT NULL,
                PasswordHash TEXT NOT NULL,
                FullName TEXT,
                Role TEXT NOT NULL,
                CreatedAt TEXT NOT NULL
            );
        ";
                command.ExecuteNonQuery();

                // 新增：创建 MonitoringSites 表
                command.CommandText = @"
            CREATE TABLE IF NOT EXISTS MonitoringSites (
                SiteID TEXT PRIMARY KEY,
                LocationDescription TEXT,
                AgeClass TEXT,
                SiteIndex INTEGER,
                PlotType TEXT,
                AreaHectares REAL
            );
        ";
                command.ExecuteNonQuery();

                // 新增：创建 SoilNutrientReadings 表
                command.CommandText = @"
            CREATE TABLE IF NOT EXISTS SoilNutrientReadings (
                ReadingID TEXT PRIMARY KEY,
                SiteID TEXT NOT NULL,
                Timestamp TEXT NOT NULL,
                NitrogenTotal REAL,
                PhosphorusTotal REAL,
                PotassiumTotal REAL,
                NitrogenAvailable REAL,
                PhosphorusAvailable REAL,
                PotassiumAvailable REAL,
                Unit TEXT,
                DataSource TEXT,
                FOREIGN KEY (SiteID) REFERENCES MonitoringSites(SiteID) ON DELETE CASCADE 
            ); 
        ";
                // ON DELETE CASCADE 表示如果 MonitoringSites 中的一个记录被删除，
                // 所有相关的 SoilNutrientReadings 记录也会被自动删除。
                command.ExecuteNonQuery();
            }
        }

        public bool AddUser(User user)
        {
            try
            {
                using (var connection = new SqliteConnection(GetConnectionString()))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        INSERT INTO Users (UserID, Username, PasswordHash, FullName, Role, CreatedAt)
                        VALUES ($userID, $username, $passwordHash, $fullName, $role, $createdAt);
                    ";
                    command.Parameters.AddWithValue("$userID", user.UserID);
                    command.Parameters.AddWithValue("$username", user.Username);
                    command.Parameters.AddWithValue("$passwordHash", user.PasswordHash);
                    command.Parameters.AddWithValue("$fullName", user.FullName ?? (object)DBNull.Value); // 处理可能的 null
                    command.Parameters.AddWithValue("$role", user.Role.ToString());
                    command.Parameters.AddWithValue("$createdAt", user.CreatedAt.ToString("o")); // "o" for round-trip ISO 8601

                    command.ExecuteNonQuery();
                    return true;
                }
            }
            catch (SqliteException ex)
            {
                // 特别处理 UNIQUE constraint violation (用户名已存在)
                if (ex.SqliteErrorCode == 19 && ex.Message.ToLower().Contains("unique constraint failed: users.username"))
                {
                    Console.WriteLine($"数据库错误: 用户名 '{user.Username}' 已存在。");
                }
                else
                {
                    Console.WriteLine($"数据库错误 (AddUser): {ex.Message}");
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"一般错误 (AddUser): {ex.Message}");
                return false;
            }
        }

        public User GetUserByUsername(string username)
        {
            User user = null;
            try
            {
                using (var connection = new SqliteConnection(GetConnectionString()))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        SELECT UserID, Username, PasswordHash, FullName, Role, CreatedAt
                        FROM Users
                        WHERE Username = $username;
                    ";
                    command.Parameters.AddWithValue("$username", username);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                UserID = reader.GetString(0),
                                Username = reader.GetString(1),
                                PasswordHash = reader.GetString(2),
                                FullName = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Role = Enum.Parse<UserRole>(reader.GetString(4), true),
                                CreatedAt = DateTime.Parse(reader.GetString(5)).ToLocalTime() // 'o' format is UTC, convert to Local if needed
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"数据库错误 (GetUserByUsername): {ex.Message}");
            }
            return user;
        }

        // 可选: 如果 UserService 需要一次性加载所有用户 (通常不推荐，直接查询更好)
        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            try
            {
                using (var connection = new SqliteConnection(GetConnectionString()))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT UserID, Username, PasswordHash, FullName, Role, CreatedAt FROM Users;";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new User
                            {
                                UserID = reader.GetString(0),
                                Username = reader.GetString(1),
                                PasswordHash = reader.GetString(2),
                                FullName = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Role = Enum.Parse<UserRole>(reader.GetString(4), true),
                                CreatedAt = DateTime.Parse(reader.GetString(5)).ToLocalTime()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"数据库错误 (GetAllUsers): {ex.Message}");
            }
            return users;
        }

        // Services/DatabaseService.cs
        // ... (保留 Users 相关的方法) ...

        // --- MonitoringSite Methods ---
        public bool AddMonitoringSite(MonitoringSite site)
        {
            try
            {
                using (var connection = new SqliteConnection(GetConnectionString()))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                INSERT INTO MonitoringSites (SiteID, LocationDescription, AgeClass, SiteIndex, PlotType, AreaHectares)
                VALUES ($siteID, $locationDescription, $ageClass, $siteIndex, $plotType, $areaHectares);
            ";
                    command.Parameters.AddWithValue("$siteID", site.SiteID);
                    command.Parameters.AddWithValue("$locationDescription", site.LocationDescription ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$ageClass", site.AgeClass.ToString());
                    command.Parameters.AddWithValue("$siteIndex", site.SiteIndex);
                    command.Parameters.AddWithValue("$plotType", site.PlotType.ToString());
                    command.Parameters.AddWithValue("$areaHectares", site.AreaHectares ?? (object)DBNull.Value);

                    command.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"数据库错误 (AddMonitoringSite - {site.SiteID}): {ex.Message}");
                return false;
            }
        }

        public List<MonitoringSite> GetAllMonitoringSites()
        {
            var sites = new List<MonitoringSite>();
            try
            {
                using (var connection = new SqliteConnection(GetConnectionString()))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT SiteID, LocationDescription, AgeClass, SiteIndex, PlotType, AreaHectares FROM MonitoringSites;";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            sites.Add(new MonitoringSite
                            {
                                SiteID = reader.GetString(0),
                                LocationDescription = reader.IsDBNull(1) ? null : reader.GetString(1),
                                AgeClass = Enum.Parse<AgeClass>(reader.GetString(2), true),
                                SiteIndex = reader.GetInt32(3),
                                PlotType = Enum.Parse<PlotType>(reader.GetString(4), true),
                                AreaHectares = reader.IsDBNull(5) ? (double?)null : reader.GetDouble(5)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"数据库错误 (GetAllMonitoringSites): {ex.Message}");
            }
            return sites;
        }

        // --- SoilNutrientReading Methods ---
        public bool AddSoilNutrientReading(SoilNutrientReading reading)
        {
            try
            {
                using (var connection = new SqliteConnection(GetConnectionString()))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                INSERT INTO SoilNutrientReadings (ReadingID, SiteID, Timestamp, 
                                                NitrogenTotal, PhosphorusTotal, PotassiumTotal,
                                                NitrogenAvailable, PhosphorusAvailable, PotassiumAvailable,
                                                Unit, DataSource)
                VALUES ($readingID, $siteID, $timestamp, 
                        $nitrogenTotal, $phosphorusTotal, $potassiumTotal,
                        $nitrogenAvailable, $phosphorusAvailable, $potassiumAvailable,
                        $unit, $dataSource);
            ";
                    command.Parameters.AddWithValue("$readingID", reading.ReadingID);
                    command.Parameters.AddWithValue("$siteID", reading.SiteID);
                    command.Parameters.AddWithValue("$timestamp", reading.Timestamp.ToString("o"));
                    command.Parameters.AddWithValue("$nitrogenTotal", reading.NitrogenTotal ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$phosphorusTotal", reading.PhosphorusTotal ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$potassiumTotal", reading.PotassiumTotal ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$nitrogenAvailable", reading.NitrogenAvailable ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$phosphorusAvailable", reading.PhosphorusAvailable ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$potassiumAvailable", reading.PotassiumAvailable ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$unit", reading.Unit.ToString());
                    command.Parameters.AddWithValue("$dataSource", reading.DataSource ?? (object)DBNull.Value);

                    command.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"数据库错误 (AddSoilNutrientReading - {reading.ReadingID}): {ex.Message}");
                return false;
            }
        }

        public List<SoilNutrientReading> GetSoilReadingsBySiteId(string siteId)
        {
            var readings = new List<SoilNutrientReading>();
            try
            {
                using (var connection = new SqliteConnection(GetConnectionString()))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                SELECT ReadingID, SiteID, Timestamp, NitrogenTotal, PhosphorusTotal, PotassiumTotal, 
                       NitrogenAvailable, PhosphorusAvailable, PotassiumAvailable, Unit, DataSource 
                FROM SoilNutrientReadings WHERE SiteID = $siteID ORDER BY Timestamp DESC; 
            "; // 通常按时间排序
                    command.Parameters.AddWithValue("$siteID", siteId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            readings.Add(new SoilNutrientReading
                            {
                                ReadingID = reader.GetString(0),
                                SiteID = reader.GetString(1),
                                Timestamp = DateTime.Parse(reader.GetString(2)).ToLocalTime(),
                                NitrogenTotal = reader.IsDBNull(3) ? (double?)null : reader.GetDouble(3),
                                PhosphorusTotal = reader.IsDBNull(4) ? (double?)null : reader.GetDouble(4),
                                PotassiumTotal = reader.IsDBNull(5) ? (double?)null : reader.GetDouble(5),
                                NitrogenAvailable = reader.IsDBNull(6) ? (double?)null : reader.GetDouble(6),
                                PhosphorusAvailable = reader.IsDBNull(7) ? (double?)null : reader.GetDouble(7),
                                PotassiumAvailable = reader.IsDBNull(8) ? (double?)null : reader.GetDouble(8),
                                Unit = Enum.Parse<NutrientUnit>(reader.GetString(9), true),
                                DataSource = reader.IsDBNull(10) ? null : reader.GetString(10)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"数据库错误 (GetSoilReadingsBySiteId for {siteId}): {ex.Message}");
            }
            return readings;
        }

        // 方法用于检查表是否为空，供数据导入逻辑使用
        public bool IsTableEmpty(string tableName)
        {
            try
            {
                using (var connection = new SqliteConnection(GetConnectionString()))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    // 使用参数化查询防止SQL注入，尽管这里表名是硬编码的
                    command.CommandText = $"SELECT COUNT(*) FROM {tableName};";
                    long count = (long)command.ExecuteScalar();
                    return count == 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"检查表 {tableName} 是否为空时出错: {ex.Message}");
                return false; // 出错时保守假设表不为空或无法确定
            }
        }

        // Services/DatabaseService.cs
        // ... (保留现有的构造函数, InitializeDatabase, Users相关方法, AddMonitoringSite, GetAllMonitoringSites, AddSoilNutrientReading, GetSoilReadingsBySiteId, IsTableEmpty 方法)

        // --- 新增和修改 MonitoringSite 的数据库操作方法 ---

        public MonitoringSite GetMonitoringSiteById(string siteId)
        {
            MonitoringSite site = null;
            if (string.IsNullOrWhiteSpace(siteId)) return null;

            try
            {
                using (var connection = new SqliteConnection(GetConnectionString()))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                SELECT SiteID, LocationDescription, AgeClass, SiteIndex, PlotType, AreaHectares 
                FROM MonitoringSites 
                WHERE SiteID = $siteID;
            ";
                    command.Parameters.AddWithValue("$siteID", siteId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            site = new MonitoringSite
                            {
                                SiteID = reader.GetString(0),
                                LocationDescription = reader.IsDBNull(1) ? null : reader.GetString(1),
                                AgeClass = Enum.Parse<AgeClass>(reader.GetString(2), true),
                                SiteIndex = reader.GetInt32(3),
                                PlotType = Enum.Parse<PlotType>(reader.GetString(4), true),
                                AreaHectares = reader.IsDBNull(5) ? (double?)null : reader.GetDouble(5)
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"数据库错误 (GetMonitoringSiteById for {siteId}): {ex.Message}");
            }
            return site;
        }

        public bool UpdateMonitoringSite(MonitoringSite site)
        {
            if (site == null || string.IsNullOrWhiteSpace(site.SiteID)) return false;

            try
            {
                using (var connection = new SqliteConnection(GetConnectionString()))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                UPDATE MonitoringSites SET 
                    LocationDescription = $locationDescription, 
                    AgeClass = $ageClass, 
                    SiteIndex = $siteIndex, 
                    PlotType = $plotType, 
                    AreaHectares = $areaHectares
                WHERE SiteID = $siteID;
            ";
                    command.Parameters.AddWithValue("$locationDescription", site.LocationDescription ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$ageClass", site.AgeClass.ToString());
                    command.Parameters.AddWithValue("$siteIndex", site.SiteIndex);
                    command.Parameters.AddWithValue("$plotType", site.PlotType.ToString());
                    command.Parameters.AddWithValue("$areaHectares", site.AreaHectares ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$siteID", site.SiteID);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"数据库错误 (UpdateMonitoringSite for {site.SiteID}): {ex.Message}");
                return false;
            }
        }

        public bool DeleteMonitoringSite(string siteId)
        {
            if (string.IsNullOrWhiteSpace(siteId)) return false;

            try
            {
                using (var connection = new SqliteConnection(GetConnectionString()))
                {
                    connection.Open();
                    // 确保启用了外键，这样相关的 SoilNutrientReadings 也会被删除 (如果表定义了 ON DELETE CASCADE)
                    var fkCommand = connection.CreateCommand();
                    fkCommand.CommandText = "PRAGMA foreign_keys = ON;"; // 在某些情况下可能需要为每个连接显式启用
                    fkCommand.ExecuteNonQuery();


                    var command = connection.CreateCommand();
                    command.CommandText = "DELETE FROM MonitoringSites WHERE SiteID = $siteID;";
                    command.Parameters.AddWithValue("$siteID", siteId);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"数据库错误 (DeleteMonitoringSite for {siteId}): {ex.Message}");
                return false;
            }
        }


    }
}