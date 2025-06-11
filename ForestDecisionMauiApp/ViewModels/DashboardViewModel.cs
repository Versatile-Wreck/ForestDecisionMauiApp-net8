// ViewModels/DashboardViewModel.cs
using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ForestDecisionMauiApp.Services;

namespace ForestDecisionMauiApp.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly DatabaseService _dbService;

        [ObservableProperty]
        private int _totalSitesCount;

        [ObservableProperty]
        private int _warningSitesCount;

        [ObservableProperty]
        private DateTime? _latestDataTimestamp;

        [ObservableProperty]
        private bool _isBusy;

        public DashboardViewModel(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        [RelayCommand]
        private async Task LoadDashboardDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                // 这些获取计数的逻辑需要在 DatabaseService 中实现
                // 为了简化，我们先使用 GetAllMonitoringSites，将来可以优化为直接 Count 查询
                var sites = await Task.Run(() => _dbService.GetAllMonitoringSites());
                TotalSitesCount = sites?.Count ?? 0;

                // 这里的“预警站点”逻辑是示例，需要 DecisionService 支持
                // 暂时先用一个假数据
                WarningSitesCount = sites?.Count > 0 ? new Random().Next(0, sites.Count / 2) : 0;

                // 获取最新数据时间 (示例)
                // 最新一条养分数据的 Timestamp
                // 最新一条监测点的创建/修改时间
                // 暂时用一个假数据
                LatestDataTimestamp = DateTime.Now;

            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("错误", $"无法加载仪表盘数据: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}