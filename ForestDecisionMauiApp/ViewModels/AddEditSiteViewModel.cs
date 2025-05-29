// ViewModels/AddEditSiteViewModel.cs
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ForestDecisionMauiApp.Models;
using ForestDecisionMauiApp.Services;
using System; // For Guid

namespace ForestDecisionMauiApp.ViewModels
{
    // 这个特性允许我们从导航中接收参数
    [QueryProperty(nameof(ExistingSiteId), "siteId")]
    public partial class AddEditSiteViewModel : ObservableObject
    {
        private readonly DatabaseService _dbService;
        private MonitoringSite _originalSiteForEdit; // 用于存储编辑前的原始对象

        [ObservableProperty]
        private string _pageTitle;

        [ObservableProperty]
        private bool _isEditMode;

        // --- 用于绑定的监测点属性 ---
        [ObservableProperty]
        private string _siteID;

        [ObservableProperty]
        private string _locationDescription;

        [ObservableProperty]
        private AgeClass _selectedAgeClass;

        [ObservableProperty]
        private int _siteIndex;

        [ObservableProperty]
        private PlotType _selectedPlotType;

        [ObservableProperty]
        private string _areaHectaresStr; // 用字符串接收输入，便于验证和转换

        [ObservableProperty]
        private string _errorMessage;


        public ObservableCollection<AgeClass> AgeClasses { get; }
        public ObservableCollection<PlotType> PlotTypes { get; }

        private string _existingSiteId;
        public string ExistingSiteId
        {
            get => _existingSiteId;
            set
            {
                _existingSiteId = Uri.UnescapeDataString(value ?? string.Empty); // 解码URL参数
                // 在这里不能直接调用异步方法，或者需要特殊处理
                // 最好是在页面 OnAppearing 或 ViewModel 的初始化命令中加载
                // 我们将创建一个异步加载方法，由页面调用或命令触发
            }
        }

        public AddEditSiteViewModel(DatabaseService dbService)
        {
            _dbService = dbService;
            AgeClasses = new ObservableCollection<AgeClass>(Enum.GetValues(typeof(AgeClass)).Cast<AgeClass>().Where(ac => ac != AgeClass.Undefined));
            PlotTypes = new ObservableCollection<PlotType>(Enum.GetValues(typeof(PlotType)).Cast<PlotType>().Where(pt => pt != PlotType.Undefined));
            PageTitle = "新增监测点"; // 默认标题
        }

        // 这个方法应该在 ViewModel 被激活后 (例如页面 OnAppearing) 调用
        public async Task InitializeAsync()
        {
            if (!string.IsNullOrWhiteSpace(ExistingSiteId))
            {
                await LoadSiteForEditAsync(ExistingSiteId);
            }
            else
            {
                IsEditMode = false;
                PageTitle = "新增监测点";
                // 为新增模式设置默认值或清空字段
                SiteID = "SITE-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(); // 建议的ID格式
                LocationDescription = string.Empty;
                SelectedAgeClass = AgeClasses.FirstOrDefault();
                SiteIndex = 0; // 或者一个合理的默认值，如12
                SelectedPlotType = PlotTypes.FirstOrDefault();
                AreaHectaresStr = string.Empty;
                ErrorMessage = string.Empty;
            }
        }


        private async Task LoadSiteForEditAsync(string siteId)
        {
            IsEditMode = true;
            PageTitle = "编辑监测点";
            _originalSiteForEdit = await Task.Run(() => _dbService.GetMonitoringSiteById(siteId));

            if (_originalSiteForEdit != null)
            {
                SiteID = _originalSiteForEdit.SiteID;
                LocationDescription = _originalSiteForEdit.LocationDescription;
                SelectedAgeClass = _originalSiteForEdit.AgeClass;
                SiteIndex = _originalSiteForEdit.SiteIndex;
                SelectedPlotType = _originalSiteForEdit.PlotType;
                AreaHectaresStr = _originalSiteForEdit.AreaHectares?.ToString() ?? string.Empty;
                ErrorMessage = string.Empty;
            }
            else
            {
                ErrorMessage = "加载监测点数据失败。";
                await Application.Current.MainPage.DisplayAlert("错误", ErrorMessage, "OK");
                await Shell.Current.GoToAsync(".."); // 返回上一页
            }
        }

        [RelayCommand]
        private async Task SaveSiteAsync()
        {
            ErrorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(SiteID))
            {
                ErrorMessage = "监测点ID不能为空。";
                await Application.Current.MainPage.DisplayAlert("验证错误", ErrorMessage, "OK");
                return;
            }
            if (string.IsNullOrWhiteSpace(LocationDescription))
            {
                ErrorMessage = "位置描述不能为空。";
                await Application.Current.MainPage.DisplayAlert("验证错误", ErrorMessage, "OK");
                return;
            }
            if (SiteIndex <= 0)
            {
                ErrorMessage = "立地指数必须大于0。";
                await Application.Current.MainPage.DisplayAlert("验证错误", ErrorMessage, "OK");
                return;
            }

            double? area = null;
            if (!string.IsNullOrWhiteSpace(AreaHectaresStr))
            {
                if (double.TryParse(AreaHectaresStr, out double parsedArea) && parsedArea >= 0)
                {
                    area = parsedArea;
                }
                else
                {
                    ErrorMessage = "面积（公顷）必须是一个有效的非负数字。";
                    await Application.Current.MainPage.DisplayAlert("验证错误", ErrorMessage, "OK");
                    return;
                }
            }

            var site = new MonitoringSite
            {
                SiteID = this.SiteID,
                LocationDescription = this.LocationDescription,
                AgeClass = this.SelectedAgeClass,
                SiteIndex = this.SiteIndex,
                PlotType = this.SelectedPlotType,
                AreaHectares = area
            };

            bool success = false;
            if (IsEditMode)
            {
                success = await Task.Run(() => _dbService.UpdateMonitoringSite(site));
            }
            else
            {
                // 检查ID是否已存在 (仅在新增模式下)
                var existing = await Task.Run(() => _dbService.GetMonitoringSiteById(site.SiteID));
                if (existing != null)
                {
                    ErrorMessage = $"监测点ID '{site.SiteID}' 已存在。";
                    await Application.Current.MainPage.DisplayAlert("保存失败", ErrorMessage, "OK");
                    return;
                }
                success = await Task.Run(() => _dbService.AddMonitoringSite(site));
            }

            if (success)
            {
                await Application.Current.MainPage.DisplayAlert("成功", $"监测点数据已{(IsEditMode ? "更新" : "保存")}。", "OK");
                // 通知 MainDataViewModel 刷新列表 (可以通过 MessagingCenter 或其他方式)
                // 这里简单地返回，让 MainDataPage 的 OnAppearing 逻辑去刷新
                await Shell.Current.GoToAsync(".."); // 返回上一页
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("失败", $"未能{(IsEditMode ? "更新" : "保存")}监测点数据。", "OK");
            }
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            await Shell.Current.GoToAsync(".."); // 返回上一页
        }
    }
}