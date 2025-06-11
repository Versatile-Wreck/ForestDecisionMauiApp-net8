// ViewModels/SiteDetailViewModel.cs
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using ForestDecisionMauiApp.Models;
using ForestDecisionMauiApp.Services;


namespace ForestDecisionMauiApp.ViewModels
{
    [QueryProperty(nameof(SiteId), "siteId")]
    public partial class SiteDetailViewModel : ObservableObject
    {
        private readonly DatabaseService _dbService;
        private readonly DecisionService _decisionService;

        [ObservableProperty]
        private string _siteId;

        [ObservableProperty]
        private string _pageTitle = "加载中...";

        [ObservableProperty]
        private MonitoringSite _siteDetails;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private bool _isReady = false; // 控制页面内容是否可见


        // <-- 新增一个属性来控制 CollectionView 的高度 -->
        [ObservableProperty]
        private double _recommendationsHeight;


        public ObservableCollection<SoilNutrientReading> HistoricalReadings { get; } = new();
        public ObservableCollection<DecisionRecommendation> CurrentRecommendations { get; } = new();

        public SiteDetailViewModel(DatabaseService dbService, DecisionService decisionService)
        {
            _dbService = dbService;
            _decisionService = decisionService;
        }

        // 当 SiteId 属性被导航参数设置时，这个方法会被自动调用
        async partial void OnSiteIdChanged(string value)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            if (string.IsNullOrEmpty(SiteId) || IsBusy)
                return;

            IsBusy = true;
            IsReady = false;
            try
            {
                // 清空旧数据
                HistoricalReadings.Clear();
                CurrentRecommendations.Clear();

                // 加载监测点基础数据
                SiteDetails = await Task.Run(() => _dbService.GetMonitoringSiteById(SiteId));
                if (SiteDetails == null)
                {
                    await Application.Current.MainPage.DisplayAlert("错误", "找不到指定的监测点数据。", "确定");
                    await Shell.Current.GoToAsync(".."); // 返回上一页
                    return;
                }

                PageTitle = $"详情: {SiteDetails.SiteID}";

                // 加载历史读数
                var readings = await Task.Run(() => _dbService.GetSoilReadingsBySiteId(SiteId));
                foreach (var reading in readings)
                {
                    HistoricalReadings.Add(reading);
                }

                // 基于最新读数生成决策建议
                // ViewModels/SiteDetailViewModel.cs -> LoadDataAsync 方法内

                // ...
                // 基于最新读数生成决策建议
                var latestReading = HistoricalReadings.FirstOrDefault();
                CurrentRecommendations.Clear(); // 确保先清空
                if (latestReading != null)
                {
                    var recommendations = _decisionService.GenerateRecommendations(SiteDetails, latestReading);
                    foreach (var rec in recommendations)
                    {
                        CurrentRecommendations.Add(rec);
                    }
                }
                else
                {
                    // **新增：当没有任何养分读数时，添加一条提示信息**
                    CurrentRecommendations.Add(new DecisionRecommendation
                    {
                        Severity = RecommendationSeverity.Info,
                        RecommendationText = "该监测点尚无养分读数数据，无法生成决策建议。",
                        Basis = "数据缺失"
                    });
                }

                // **新增：根据建议的数量计算并设置高度**
                // 假设每个条目大约高 90 个单位 (包括 Frame 的 Padding 和 Margin)
                // 你可以根据你的 ItemTemplate 的实际观感调整这个数字
                const double singleItemHeight = 90;
                RecommendationsHeight = CurrentRecommendations.Count * singleItemHeight;


            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("加载失败", $"加载详细数据时出错: {ex.Message}", "确定");
            }
            finally
            {
                IsBusy = false;
                IsReady = true; // 数据加载完毕，显示内容
            }
        }
    }
}