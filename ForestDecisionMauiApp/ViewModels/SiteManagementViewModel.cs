// ViewModels/SiteManagementViewModel.cs
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
    public partial class SiteManagementViewModel : ObservableObject
    {
        private readonly DatabaseService _dbService;
        private readonly DecisionService _decisionService;

        private readonly AuthenticationService _authService;

        // 为UI绑定创建权限属性
        public bool CanAddSite => _authService.IsAdmin || _authService.IsResearcher;
        public bool CanEditSite => _authService.IsAdmin || _authService.IsResearcher;
        public bool CanDeleteSite => _authService.IsAdmin;

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

        // 修改命令的 CanExecute 逻辑
        private bool CanExecuteEditOperation() => SelectedSite != null && CanEditSite;
        private bool CanExecuteDeleteOperation() => SelectedSite != null && CanDeleteSite;



        public bool CanAdd => _authService.IsAdmin || _authService.IsResearcher;
        public bool CanEdit => _authService.IsAdmin || _authService.IsResearcher;
        public bool CanDelete => _authService.IsAdmin;



        // 修改构造函数以注入 DecisionService
        public SiteManagementViewModel(DatabaseService dbService, DecisionService decisionService, AuthenticationService authService)
        {
            _dbService = dbService;
            _decisionService = decisionService; // <-- 注入 DecisionService
            _authService = authService; // 保存实例

            _authService.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(AuthenticationService.CurrentUser))
                {
                    // 当用户登录或登出时，刷新所有权限相关的属性和命令
                    OnPropertyChanged(nameof(CanAdd));
                    OnPropertyChanged(nameof(CanEdit));
                    OnPropertyChanged(nameof(CanDelete));
                    AddNewSiteCommand.NotifyCanExecuteChanged();
                    EditSiteCommand.NotifyCanExecuteChanged();
                    DeleteSiteCommand.NotifyCanExecuteChanged();
                }
            };

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
               // IsDetailsPanelVisible = true; // <-- 新增：当选中一项时，显示面板
                _ = LoadReadingsForSiteAsync(newValue.SiteID); // 使用 _ 丢弃 Task，表示不等待完成 // 这个方法执行后会更新 LatestReadingForDecision
            }
            else
            {
               // IsDetailsPanelVisible = false; // <-- 新增：当没有选中项时，隐藏面板
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
        [RelayCommand(CanExecute = nameof(CanAddSite))]
        private async Task AddNewSiteAsync()
        {
            // 导航到新增页面，不带参数
            await Shell.Current.GoToAsync(nameof(AddEditSitePage));
        }

        // --- 修改后的命令 ---
        private bool CanExecuteSiteOperation() => SelectedSite != null;

        [RelayCommand(CanExecute = nameof(CanExecuteEditOperation))] // 使用 CanExecute
        private async Task EditSiteAsync()
        {
            // SelectedSite 已通过 CanExecute 检查，确保不为 null
            await Shell.Current.GoToAsync($"{nameof(AddEditSitePage)}?siteId={Uri.EscapeDataString(SelectedSite.SiteID)}");
        }

        [RelayCommand(CanExecute = nameof(CanExecuteDeleteOperation))] // 使用 CanExecute
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

    }
}