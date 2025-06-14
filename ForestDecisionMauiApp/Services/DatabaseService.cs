// Services/DatabaseService.cs
using Microsoft.Data.Sqlite; // 引入SQLite命名空间
using System;
using System.IO;
using ForestDecisionMauiApp.Models; // 确保引用 User 模型
using System.Collections.Generic; // For GetAllUsers (if you implement it)
using System.Diagnostics;

namespace ForestDecisionMauiApp.Services
{
    public class DatabaseService
    {
        private readonly string _databasePath;
        private static readonly string _tempLogPath = Path.Combine(Path.GetTempPath(), "ForestApp_StartupLog.txt"); // 临时日志文件
        public string DatabasePath => _databasePath;// 一个公共属性或方法来获取数据库路径，以便ViewModel使用




        // --- 构造函数、初始化、版本管理---

        // 构造函数和初始化
        public DatabaseService()
        {
            WriteTempLog("DatabaseService constructor - Entered.");
            try
            {
                // 定义数据库文件的存储位置
                string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                WriteTempLog($"AppDataFolder: {appDataFolder}");

                string appSpecificFolder = Path.Combine(appDataFolder, "ForestDecisionSystemApp");
                WriteTempLog($"AppSpecificFolder path: {appSpecificFolder}");

                Directory.CreateDirectory(appSpecificFolder); // 确保文件夹存在
                WriteTempLog("AppSpecificFolder created or already exists.");

                _databasePath = Path.Combine(appSpecificFolder, "forest_system.db");
                WriteTempLog($"DatabasePath: {_databasePath}");

                InitializeDatabase(); //初始化数据库表
                WriteTempLog("InitializeDatabase - Call completed.");

                // ApplyMigrations(); // 应用版本升级数据迁移
                WriteTempLog("ApplyMigrations - Call completed.");
            }
            catch (Exception ex)
            {
                WriteTempLog($"FATAL ERROR in DatabaseService constructor: {ex.ToString()}");
                // 在这里重新抛出异常或处理，以确保应用知道初始化失败
                // 对于静默失败，这里的日志是关键
                throw;
            }
            WriteTempLog("DatabaseService constructor - Exited successfully.");
        }

        // 初始化数据库，创建必要的表和初始数据
        private void InitializeDatabase()
        {
            WriteTempLog("InitializeDatabase - Entered.");
            using (var connection = new SqliteConnection(GetConnectionString()))
            {
                WriteTempLog("InitializeDatabase - Connection string obtained.");
                connection.Open();
                WriteTempLog("InitializeDatabase - Connection opened.");

                // ... (你的所有 CREATE TABLE 和 INSERT OR IGNORE INTO AppDbInfo 语句)
                // 你可以在每个关键的 command.ExecuteNonQuery() 之后也添加 WriteTempLog
                var command = connection.CreateCommand();
                command.CommandText = "CREATE TABLE IF NOT EXISTS AppDbInfo (Key TEXT PRIMARY KEY, Value TEXT);";
                command.ExecuteNonQuery();
                WriteTempLog("InitializeDatabase - AppDbInfo table ensured.");
                // ... (其他表的创建日志)

                // 开启外键约束支持 (如果每个连接都需要，或者在连接字符串中指定)
                // var pragmaCmd = connection.CreateCommand();
                // pragmaCmd.CommandText = "PRAGMA foreign_keys = ON;";
                // pragmaCmd.ExecuteNonQuery();

                // 版本信息表 (AppDbInfo) 用于存储应用程序的版本信息或其他元数据
                command.CommandText = @"
            CREATE TABLE IF NOT EXISTS AppDbInfo (
                Key TEXT PRIMARY KEY,
                Value TEXT
            );
        ";
                command.ExecuteNonQuery();

                // 设置初始版本号 (如果表是新创建的)
                command.CommandText = "INSERT OR IGNORE INTO AppDbInfo (Key, Value) VALUES ('SchemaVersion', '1');";
                command.ExecuteNonQuery();


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
            WriteTempLog("InitializeDatabase - Exited successfully.");
        }

        //数据库版本管理（迁移）
        private void ApplyMigrations()
        {
            WriteTempLog("ApplyMigrations - Entered.");

            using (var connection = new SqliteConnection(GetConnectionString()))
            {
                connection.Open();
                int currentVersion = GetCurrentSchemaVersion(connection); // 在 ApplyMigrations 的开头获取版本号，然后在这里传递连接

                Debug.WriteLine($"当前数据库 Schema 版本: {currentVersion}");
                
                // 确保在事务中执行迁移
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        if (currentVersion < 2) // 迁移到版本 2
                        {
                            Debug.WriteLine("应用迁移：版本 1 -> 2");
                            var migrateCmd_1_2 = connection.CreateCommand();
                            migrateCmd_1_2.Transaction = transaction;
                            migrateCmd_1_2.CommandText = @"
                          ALTER TABLE MonitoringSites ADD COLUMN LastInspectedDate TEXT;
                          ALTER TABLE Users ADD COLUMN Email TEXT;
                      "; // 示例：添加新列
                            migrateCmd_1_2.ExecuteNonQuery();
                            SetSchemaVersion(connection, transaction, 2);
                            currentVersion = 2; // 更新当前版本以便后续迁移
                            Debug.WriteLine("迁移到版本 2 成功。");
                        }

                        if (currentVersion < 3) // 迁移到版本 3
                        {
                            Debug.WriteLine("应用迁移：版本 2 -> 3");
                            var migrateCmd_2_3 = connection.CreateCommand();
                            migrateCmd_2_3.Transaction = transaction;
                            // 示例：创建新表，并从旧表迁移数据（如果需要）
                            migrateCmd_2_3.CommandText = @"
                          CREATE TABLE IF NOT EXISTS SiteNotes (
                              NoteID TEXT PRIMARY KEY,
                              SiteID TEXT NOT NULL,
                              NoteText TEXT,
                              CreatedAt TEXT NOT NULL,
                              FOREIGN KEY (SiteID) REFERENCES MonitoringSites(SiteID) ON DELETE CASCADE
                          );
                      ";
                            migrateCmd_2_3.ExecuteNonQuery();
                            SetSchemaVersion(connection, transaction, 3);
                            currentVersion = 3;
                            Debug.WriteLine("迁移到版本 3 成功。");
                        }
                        // 添加更多版本的迁移逻辑...

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"数据库迁移失败: {ex.Message}");
                        transaction.Rollback();
                        // 关键错误，可能需要通知用户或记录日志
                        // 不应继续运行可能依赖新schema的应用代码
                        throw; // 重新抛出异常，让应用知道初始化失败
                    }
                }
            }

            WriteTempLog("ApplyMigrations - Exited successfully.");
        }






        // --- Users Methods ---

        // 添加用户
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
                    Debug.WriteLine($"数据库错误: 用户名 '{user.Username}' 已存在。");
                }
                else
                {
                    Debug.WriteLine($"数据库错误 (AddUser): {ex.Message}");
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"一般错误 (AddUser): {ex.Message}");
                return false;
            }
        }

        // 通过用户名获取用户信息
        public async Task<User>  GetUserByUsername(string username)
        {
            User user = null;
            try
            {
                using (var connection = new SqliteConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();  // ✅ 异步打开数据库
                    // connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        SELECT UserID, Username, PasswordHash, FullName, Role, CreatedAt
                        FROM Users
                        WHERE Username = $username;
                    ";
                    command.Parameters.AddWithValue("$username", username);

                    //using (var reader = command.ExecuteReader())
                    using (var reader = await command.ExecuteReaderAsync()) // ✅ 异步执行查询
                    {
                        if (await reader.ReadAsync()) // ✅ 异步读取数据
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
                Debug.WriteLine($"数据库错误 (GetUserByUsername): {ex.Message}");
            }
            return user;
        }

        // 通过用户ID获取用户信息
        public User GetUserById(string userId)
        {
            User user = null;
            if (string.IsNullOrWhiteSpace(userId)) return null;

            try
            {
                using (var connection = new SqliteConnection(GetConnectionString()))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                    SELECT UserID, Username, PasswordHash, FullName, Role, CreatedAt
                    FROM Users
                    WHERE UserID = $userID;
                ";
                    command.Parameters.AddWithValue("$userID", userId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                UserID = reader.GetString(0),
                                Username = reader.GetString(1),
                                PasswordHash = reader.GetString(2),
                                FullName = reader.GetString(3),
                                Role = Enum.Parse<UserRole>(reader.GetString(4), true),
                                CreatedAt = DateTime.Parse(reader.GetString(5)).ToLocalTime()
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"数据库错误 (GetUserById for {userId}): {ex.Message}");
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
                Debug.WriteLine($"数据库错误 (GetAllUsers): {ex.Message}");
            }
            return users;
        }

        // 删除用户
        public bool DeleteUser(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return false;
            try
            {
                using (var connection = new SqliteConnection(GetConnectionString()))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "DELETE FROM Users WHERE UserID = $userID;";
                    command.Parameters.AddWithValue("$userID", userId);
                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"数据库错误 (DeleteUser): {ex.Message}");
                return false;
            }
        }

        // 更新用户信息
        public bool UpdateUser(User user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.UserID)) return false;
            try
            {
                using (var connection = new SqliteConnection(GetConnectionString()))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                UPDATE Users SET 
                    FullName = $fullName, 
                    Role = $role 
                WHERE UserID = $userID;";
                    // 注意：通常不在这里直接修改用户名或密码，密码重置应有单独流程
                    command.Parameters.AddWithValue("$fullName", user.FullName);
                    command.Parameters.AddWithValue("$role", user.Role.ToString());
                    command.Parameters.AddWithValue("$userID", user.UserID);
                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"数据库错误 (UpdateUser): {ex.Message}");
                return false;
            }
        }

        



        // --- MonitoringSite Methods ---

        // 获取所有监测点信息
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
                Debug.WriteLine($"数据库错误 (GetAllMonitoringSites): {ex.Message}");
            }
            return sites;
        }

        // 添加监测点信息
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
                Debug.WriteLine($"数据库错误 (AddMonitoringSite - {site.SiteID}): {ex.Message}");
                return false;
            }
        }

        // 删除某监测点信息
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
                Debug.WriteLine($"数据库错误 (DeleteMonitoringSite for {siteId}): {ex.Message}");
                return false;
            }
        }

        // 修改某监测点信息
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
                Debug.WriteLine($"数据库错误 (UpdateMonitoringSite for {site.SiteID}): {ex.Message}");
                return false;
            }
        }

        // 查询指定ID的监测点信息
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
                Debug.WriteLine($"数据库错误 (GetMonitoringSiteById for {siteId}): {ex.Message}");
            }
            return site;
        }






        // --- SoilNutrientReading Methods ---

        // 添加土壤养分读数
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
                Debug.WriteLine($"数据库错误 (AddSoilNutrientReading - {reading.ReadingID}): {ex.Message}");
                return false;
            }
        }

        // 获取指定监测点的所有土壤养分读数
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
                Debug.WriteLine($"数据库错误 (GetSoilReadingsBySiteId for {siteId}): {ex.Message}");
            }
            return readings;
        }







        // --- 数据库管理 ---

        // 获取数据库连接字符串？
        private string GetConnectionString() => $"Data Source={_databasePath}";

        // 获取当前数据库的版本号
        private int GetCurrentSchemaVersion()
        {
            using (var connection = new SqliteConnection(GetConnectionString()))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Value FROM AppDbInfo WHERE Key = 'SchemaVersion';";
                var result = command.ExecuteScalar();
                if (result != null && int.TryParse(result.ToString(), out int version))
                {
                    return version;
                }
                return 0; // 或者 1，取决于你的初始版本定义
            }
        }
        
        // 调整 GetCurrentSchemaVersion 使其能在 ApplyMigrations 内部使用已打开的连接
        private int GetCurrentSchemaVersion(SqliteConnection connection)
        {
            var command = connection.CreateCommand(); // 不需要再打开/关闭连接
            command.CommandText = "SELECT Value FROM AppDbInfo WHERE Key = 'SchemaVersion';";
            var result = command.ExecuteScalar();
            if (result != null && int.TryParse(result.ToString(), out int version))
            {
                return version;
            }
            // 如果 AppDbInfo 表或 SchemaVersion 键不存在，意味着是全新的v1数据库
            // 或者是一个非常早期的、没有版本信息的数据库。
            // InitializeDatabase 应该确保 SchemaVersion 至少为 '1'。
            return 1; // 假设最低版本为1
        }

        // 设置数据库的版本号
        private void SetSchemaVersion(SqliteConnection connection, SqliteTransaction transaction, int version)
        {
            var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "UPDATE AppDbInfo SET Value = $version WHERE Key = 'SchemaVersion';";
            command.Parameters.AddWithValue("$version", version.ToString());
            command.ExecuteNonQuery();
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
                Debug.WriteLine($"检查表 {tableName} 是否为空时出错: {ex.Message}");
                return false; // 出错时保守假设表不为空或无法确定
            }
        }






        // --- 备份与恢复功能 ---
        public async Task<bool> BackupDatabaseAsync(string targetBackupFilePath)
        {
            // 确保目标路径不为空
            if (string.IsNullOrWhiteSpace(targetBackupFilePath))
            {
                Debug.WriteLine("备份路径无效。");
                return false;
            }

            // 确保当前数据库连接已尽可能关闭，以避免文件锁定
            // Microsoft.Data.Sqlite 通常在连接关闭后会释放文件，但连接池可能保持连接。
            // 清理连接池是一种更彻底的方式，但要注意这会影响所有使用该连接字符串的连接。
            SqliteConnection.ClearAllPools(); // 清理所有连接池中的连接

            // 短暂延迟，给操作系统一点时间释放文件锁（可选，但有时有帮助）
            await Task.Delay(200);

            try
            {
                File.Copy(_databasePath, targetBackupFilePath, true); // true 表示如果目标文件已存在则覆盖
                Debug.WriteLine($"数据库成功备份到: {targetBackupFilePath}");
                return true;
            }
            catch (IOException ioEx) // 特别处理IO异常，可能是文件被占用
            {
                Debug.WriteLine($"备份数据库时发生IO错误 (可能文件仍被占用): {ioEx.Message}");
                // 提示用户：如果备份失败，尝试重启应用后再试
                await Application.Current.MainPage.DisplayAlert("备份失败", "文件操作错误，可能数据库文件仍被占用。请尝试重启应用后再进行备份。", "好的");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"备份数据库失败: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("备份失败", $"发生未知错误: {ex.Message}", "好的");
                return false;
            }
        }

        public async Task<bool> RestoreDatabaseAsync(string sourceBackupFilePath)
        {
            if (string.IsNullOrWhiteSpace(sourceBackupFilePath) || !File.Exists(sourceBackupFilePath))
            {
                Debug.WriteLine("无效的备份文件路径或文件不存在。");
                await Application.Current.MainPage.DisplayAlert("恢复失败", "选择的备份文件无效或不存在。", "好的");
                return false;
            }

            // 非常重要：关闭所有到当前数据库的连接并清理连接池
            Debug.WriteLine("正在关闭数据库连接以进行恢复...");
            SqliteConnection.ClearAllPools();

            // GC.Collect(); // 有时可以帮助加速文件锁的释放
            // GC.WaitForPendingFinalizers();
            await Task.Delay(500); // 给与更长的延迟确保文件锁释放

            try
            {
                // 删除 (或重命名) 当前的数据库文件
                if (File.Exists(_databasePath))
                {
                    File.Delete(_databasePath);
                    // 或者重命名: File.Move(_databasePath, _databasePath + ".old_backup_" + DateTime.Now.ToString("yyyyMMddHHmmss"));
                }

                File.Copy(sourceBackupFilePath, _databasePath, true);
                Debug.WriteLine($"数据库成功从 {sourceBackupFilePath} 恢复。");

                // 重要提示：恢复后，应用需要重新初始化数据库连接和状态。
                // 最简单可靠的方式是提示用户重启应用。
                // 或者，如果你的应用设计允许，可以尝试重新初始化 DatabaseService 和相关的 ViewModel。
                return true;
            }
            catch (IOException ioEx)
            {
                Debug.WriteLine($"恢复数据库时发生IO错误 (可能文件仍被占用): {ioEx.Message}");
                await Application.Current.MainPage.DisplayAlert("恢复失败", "文件操作错误，可能数据库文件仍被占用。请尝试重启应用后再进行恢复，或确保没有其他程序正在访问数据库文件。", "好的");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"恢复数据库失败: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("恢复失败", $"发生未知错误: {ex.Message}", "好的");
                return false;
            }
        }






        // --- 日志记录 ---
        private static void WriteTempLog(string message)
        {
            try
            {
                File.AppendAllText(_tempLogPath, $"{DateTime.Now}: {message}\n");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to write temp log: {ex.Message}"); // 在控制台也输出一下，以防万一
            }
        }

    }
}