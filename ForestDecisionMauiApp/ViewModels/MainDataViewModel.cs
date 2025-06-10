// ViewModels/MainDataViewModel.cs
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ForestDecisionMauiApp.Models;
using ForestDecisionMauiApp.Services;
using ForestDecisionMauiApp.Views; // 用于可能的导航
using CommunityToolkit.Maui.Storage; // <--- 添加 using for FileSaver
using System.IO;                   // <--- 添加 using for FileStream and Path
using System.Threading;            // <--- 添加 using for CancellationToken
using Microsoft.Data.Sqlite; // <--- 确保引入 Microsoft.Data.Sqlite
using System.Diagnostics;

namespace ForestDecisionMauiApp.ViewModels
{
    public partial class MainDataViewModel : ObservableObject
    {
        private readonly DatabaseService _dbService;
        private readonly DecisionService _decisionService;

        // 用于在 XAML 中绑定监测点列表
        public ObservableCollection<MonitoringSite> Sites { get; } = new();

        // 用于在 XAML 中绑定选中监测点的养分读数列表
        public ObservableCollection<SoilNutrientReading> SelectedSiteReadings { get; } = new();

        [ObservableProperty]
        private MonitoringSite _selectedSite; // 当前选中的监测点

        [ObservableProperty]
        private bool _isBusy; // 用于指示是否正在加载数据

        // 用于在 XAML 中绑定决策建议列表
        public ObservableCollection<DecisionRecommendation> Recommendations { get; } = new();

        [ObservableProperty]
        private SoilNutrientReading _latestReadingForDecision; // 保存用于决策的最新读数

        // 修改构造函数以注入 DecisionService
        public MainDataViewModel(DatabaseService dbService, DecisionService decisionService)
        {
            _dbService = dbService;
            _decisionService = decisionService; // <-- 注入 DecisionService
        }

        // 当 SelectedSite 属性变化时调用的方法 (由 ObservableProperty 自动生成部分方法)
        partial void OnSelectedSiteChanged(MonitoringSite oldValue, MonitoringSite newValue)
        {
            // 如果旧值和新值相同，或者新值是null但旧值也是null，则不执行任何操作
            if (EqualityComparer<MonitoringSite>.Default.Equals(oldValue, newValue))
                return;

            Recommendations.Clear(); // 清空旧的建议
            LatestReadingForDecision = null; // 清空旧的读数

            if (newValue != null)
            {
                // 异步加载选中监测点的养分数据
                _ = LoadReadingsForSiteAsync(newValue.SiteID); // 使用 _ 丢弃 Task，表示不等待完成 // 这个方法执行后会更新 LatestReadingForDecision
            }
            else
            {
                SelectedSiteReadings.Clear();
            }

            // 手动通知命令的 CanExecute 状态可能已改变
            // 这会使绑定到这些命令的按钮的 IsEnabled 状态刷新
            EditSiteCommand.NotifyCanExecuteChanged();
            DeleteSiteCommand.NotifyCanExecuteChanged();

        }

        private async Task LoadReadingsForSiteAsync(string siteId)
        {
            IsBusy = true;
            SelectedSiteReadings.Clear();
            Recommendations.Clear();
            LatestReadingForDecision = null;
            try
            {
                var readings = await Task.Run(() => _dbService.GetSoilReadingsBySiteId(siteId));
                if (readings != null)
                {
                    foreach (var reading in readings)
                    {
                        SelectedSiteReadings.Add(reading);
                    }
                    // 获取最新的读数用于决策 (假设 GetSoilReadingsBySiteId 返回的是按时间降序排列的)
                    LatestReadingForDecision = SelectedSiteReadings.FirstOrDefault();
                    if (LatestReadingForDecision != null && SelectedSite != null)
                    {
                        // 当最新读数加载完毕后，自动获取决策建议
                        GenerateDecisionRecommendations();
                    }
                }
            }
            catch (Exception ex)
            {
                // 显示错误消息
                await Application.Current.MainPage.DisplayAlert("加载错误", $"无法加载养分读数: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // 这个方法不再是命令，而是在数据加载后被调用
        private void GenerateDecisionRecommendations()
        {
            if (SelectedSite == null || LatestReadingForDecision == null)
            {
                // Debug.WriteLine("无法生成建议：监测点或最新读数为空。");
                Recommendations.Clear(); // 清空旧的建议
                // 可以添加一个默认的提示信息到Recommendations列表
                Recommendations.Add(new DecisionRecommendation { RecommendationText = "请先选择一个监测点并确保有最新的养分数据。", Severity = RecommendationSeverity.Info });
                return;
            }

            IsBusy = true;
            Recommendations.Clear();
            try
            {
                var newRecommendations = _decisionService.GenerateRecommendations(SelectedSite, LatestReadingForDecision);
                foreach (var rec in newRecommendations)
                {
                    Recommendations.Add(rec);
                }
            }
            catch (Exception ex)
            {
                Application.Current.MainPage.DisplayAlert("决策错误", $"生成决策建议时出错: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }   



        [RelayCommand] // 命令用于刷新或初次加载监测点数据
        public async Task LoadSitesAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            var previouslySelectedSiteId = SelectedSite?.SiteID; // 保存之前的选择ID

            Sites.Clear();
            SelectedSiteReadings.Clear(); // 清除旧的读数
            Recommendations.Clear();
            LatestReadingForDecision = null;
            SelectedSite = null; // 清除之前的选择

            try
            {
                var sitesFromDb = await Task.Run(() => _dbService.GetAllMonitoringSites());
                if (sitesFromDb != null)
                {
                    foreach (var site in sitesFromDb)
                    {
                        Sites.Add(site);
                    }
                    // 默认选中第一个（如果列表不为空）
                    // SelectedSite = Sites.FirstOrDefault(); // 这会触发 OnSelectedSiteChanged
                    // (可选) 尝试恢复之前的选择
                    if (!string.IsNullOrEmpty(previouslySelectedSiteId))
                    {
                        SelectedSite = Sites.FirstOrDefault(s => s.SiteID == previouslySelectedSiteId);
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("加载错误", $"无法加载监测点: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task Logout()
        {
            if (Shell.Current is AppShell appShellInstance)
            {
                appShellInstance.SetMainUITabBarVisibility(false);
            }
            await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
        }


        // ViewModels/MainDataViewModel.cs
        // ... (保留现有属性和方法) ...

        // 新增命令
        [RelayCommand]
        private async Task AddNewSiteAsync()
        {
            // 导航到新增页面，不带参数
            await Shell.Current.GoToAsync(nameof(AddEditSitePage));
        }

        // --- 修改后的命令 ---
        private bool CanExecuteSiteOperation() => SelectedSite != null;

        [RelayCommand(CanExecute = nameof(CanExecuteSiteOperation))] // 使用 CanExecute
        private async Task EditSiteAsync()
        {
            // SelectedSite 已通过 CanExecute 检查，确保不为 null
            await Shell.Current.GoToAsync($"{nameof(AddEditSitePage)}?siteId={Uri.EscapeDataString(SelectedSite.SiteID)}");
        }

        [RelayCommand(CanExecute = nameof(CanExecuteSiteOperation))] // 使用 CanExecute
        private async Task DeleteSiteAsync()
        {
            // SelectedSite 已通过 CanExecute 检查，确保不为 null
            bool confirmed = await Application.Current.MainPage.DisplayAlert(
                "确认删除",
                $"您确定要删除监测点 '{SelectedSite.SiteID} ({SelectedSite.LocationDescription})' 吗？\n\n注意：相关的养分读数数据也将一并被删除。",
                "是的，删除",
                "取消");

            if (confirmed)
            {
                IsBusy = true;
                bool success = await Task.Run(() => _dbService.DeleteMonitoringSite(SelectedSite.SiteID));
                IsBusy = false;

                if (success)
                {
                    await Application.Current.MainPage.DisplayAlert("成功", "监测点已删除。", "OK");
                    await LoadSitesAsync(); // 重新加载列表
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("失败", "删除监测点失败。", "OK");
                }
            }
        }

        // 在 LoadSitesAsync 成功加载数据后，可以考虑清除 SelectedSite，
        // 避免在列表刷新后仍然保持对一个可能已不存在（如果被其他方式删除）的项的选择。
        // 或者在 OnAppearing 中总是调用 LoadSitesAsync 来刷新列表。

        // MainDataPage.xaml.cs 中的 OnAppearing 方法现在应该更积极地刷新数据
        // public async Task RefreshDataAsync() { await LoadSitesAsync(); } // 新增一个公共方法

        [RelayCommand]
        private async Task BackupDatabaseAsync()
        {
            IsBusy = true;
            try
            {
                string currentDbPath = _dbService.DatabasePath; // 从 DatabaseService 获取当前数据库路径
                if (string.IsNullOrWhiteSpace(currentDbPath) || !File.Exists(currentDbPath))
                {
                    await Application.Current.MainPage.DisplayAlert("备份错误", "无法找到当前的数据库文件。", "好的");
                    IsBusy = false;
                    return;
                }

                var suggestedFileName = $"ForestDB_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.db";

                // 1. 让用户选择备份文件的保存位置和名称，我们只需要目标路径
                //    这里我们仍然可以使用 FileSaver 来获取用户选择的路径，但我们不会给它流。
                //    或者，我们需要一个能返回“另存为”路径的对话框。
                //    对于 Windows，可以考虑使用特定平台的API，或者 CommunityToolkit 中的 FolderPicker + 手动输入文件名。
                //    为了简化，我们先假设能通过某种方式获取到 `targetBackupFilePath`。
                //    一个简单的方式是，我们先创建一个临时的空文件流，让FileSaver帮我们获取路径。

                string targetBackupFilePath = null;
                using (var dummyStream = new MemoryStream()) // 创建一个虚拟的空流
                {
                    // 使用 FileSaver 来获取用户选择的保存路径
                    var fileSaverResult = await FileSaver.Default.SaveAsync(suggestedFileName, dummyStream, CancellationToken.None);
                    if (fileSaverResult.IsSuccessful && !string.IsNullOrWhiteSpace(fileSaverResult.FilePath))
                    {
                        targetBackupFilePath = fileSaverResult.FilePath;
                    }
                    else
                    {
                        string errorMessage = fileSaverResult.Exception?.Message ?? "用户取消了操作或未能选择文件路径。";
                        if (!fileSaverResult.IsSuccessful && string.IsNullOrWhiteSpace(fileSaverResult.FilePath)) // 用户取消
                        {
                            // 用户取消，静默处理或给一个温和提示
                            Debug.WriteLine("用户取消了备份操作。");
                            IsBusy = false;
                            return;
                        }
                        await Application.Current.MainPage.DisplayAlert("选择路径失败", $"未能获取备份文件保存路径: {errorMessage}", "好的");
                        IsBusy = false;
                        return;
                    }
                }

                // 确保我们确实有了一个路径
                if (string.IsNullOrWhiteSpace(targetBackupFilePath))
                {
                    IsBusy = false;
                    return;
                }

                // 2. 执行 SQLite 在线备份
                //    注意：这里的源连接不应在 using 块内立即关闭，BackupDatabase 需要它保持打开。
                //    DatabaseService 应该提供一个获取活动连接的方法，或者我们在这里创建一个新的源连接。
                //    为了简单，我们假设 DatabaseService 内部的连接管理是可靠的，
                //    我们直接使用其连接字符串创建源和目标连接。

                // 清理连接池，确保我们拿到的是最新的状态或者避免冲突
                SqliteConnection.ClearAllPools();
                await Task.Delay(200); // 短暂等待

                using (var sourceConnection = new SqliteConnection($"Data Source={currentDbPath}"))
                {
                    await sourceConnection.OpenAsync(); // 异步打开

                    // 创建到目标备份文件的连接
                    using (var destinationConnection = new SqliteConnection($"Data Source={targetBackupFilePath}"))
                    {
                        await destinationConnection.OpenAsync();

                        // 执行在线备份
                        // 从 sourceConnection 备份到 destinationConnection
                        sourceConnection.BackupDatabase(destinationConnection);
                        // BackupDatabase 方法是同步的，如果数据量大，可以考虑 Task.Run
                        // await Task.Run(() => sourceConnection.BackupDatabase(destinationConnection));

                        // destinationConnection 会在 using 结束时自动关闭，将数据写入文件
                    }
                    // sourceConnection 会在 using 结束时自动关闭
                }

                await Application.Current.MainPage.DisplayAlert("备份成功", $"数据库已成功备份到: {targetBackupFilePath}", "好的");

            }
            catch (Exception ex)
            {
                // 特别处理 SqliteException 中的文件锁定错误 (SQLite Error 5: 'database is locked')
                if (ex is SqliteException sqliteEx && sqliteEx.SqliteErrorCode == 5)
                {
                    await Application.Current.MainPage.DisplayAlert("备份失败", $"数据库文件当前被锁定，无法备份。请稍后再试或重启应用。\n错误: {sqliteEx.Message}", "好的");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("备份异常", $"备份过程中发生错误: {ex.Message}", "好的");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task RestoreDatabaseAsync()
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "恢复数据警告",
                "恢复数据将会覆盖当前所有数据，并且操作不可逆！恢复成功后，建议重启应用程序以确保所有更改生效。\n\n您确定要继续吗？",
                "是的，恢复",
                "取消");

            if (!confirm) return;

            try
            {
                var pickOptions = new PickOptions
                {
                    PickerTitle = "请选择数据库备份文件 (.db)",
                    // 文件类型筛选器在 Windows 上可能需要特定设置，或者接受所有文件然后验证后缀
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".db" } }, // 示例：仅显示 .db 文件
                    { DevicePlatform.macOS, new[] { "db" } }, // Mac 的 UTI
                    // 其他平台可以类似添加
                })
                };

                var result = await FilePicker.Default.PickAsync(pickOptions);
                if (result != null)
                {
                    if (result.FileName.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
                    {
                        IsBusy = true; // 可能需要一段时间，显示忙碌指示
                        bool success = await _dbService.RestoreDatabaseAsync(result.FullPath);
                        IsBusy = false;

                        if (success)
                        {
                            await Application.Current.MainPage.DisplayAlert(
                                "恢复成功",
                                "数据库已从备份文件恢复。\n\n强烈建议您现在重启应用程序以确保数据正确加载和应用所有迁移。",
                                "好的");
                            // 在这里，理想情况下应用应该关闭或提示用户手动关闭并重启。
                            // 例如: Application.Current.Quit(); (但这可能过于突然)
                        }
                        // RestoreDatabaseAsync 内部会处理部分失败提示
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("文件无效", "请选择一个有效的 .db 数据库备份文件。", "好的");
                    }
                }
            }
            catch (Exception ex)
            {
                IsBusy = false;
                await Application.Current.MainPage.DisplayAlert("恢复异常", $"恢复过程中发生错误: {ex.Message}", "好的");
            }
        }

    }
}