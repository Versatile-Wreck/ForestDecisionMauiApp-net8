// Views/SiteManagementPage.xaml.cs
using ForestDecisionMauiApp.ViewModels;
using ForestDecisionMauiApp.Models; // 确保引用了 MonitoringSite
using Syncfusion.Maui.DataGrid; // <-- 新增 using 声明，为了方便使用 SfDataGrid 类型
using System.ComponentModel; // <-- 确保有这个 using


namespace ForestDecisionMauiApp.Views;

public partial class SiteManagementPage : ContentPage
{
    //  private bool _isInitialLoad = true; // 标志是否是初次加载

    private readonly SiteManagementViewModel _viewModel;

    public SiteManagementPage(SiteManagementViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {

        // 加载数据逻辑保持不变
        if (_viewModel.LoadSitesCommand.CanExecute(null))
        {
            _viewModel.LoadSitesCommand.Execute(null);
        }
    }


    // --- 新增的事件处理方法 ---
    private void OnGridSelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
    {
        if (BindingContext is not SiteManagementViewModel viewModel)
        {
            return;
        }

        // 1. e.AddedRows 包含的是实际的数据对象（类型为 object）
        var selectedItem = e.AddedRows.FirstOrDefault();

        // 2. 直接将选中的 object 转换为 MonitoringSite 类型
        if (selectedItem is MonitoringSite selectedSite)
        {
            // 3. 将正确转换后的对象赋值给 ViewModel
            viewModel.SelectedSite = selectedSite;
        }
        else
        {
            // 4. 如果没有选中项或类型不匹配，则清空选择
            viewModel.SelectedSite = null;
        }
    }

    // **新增：处理双击事件的方法**
    private async void OnGridCellDoubleTapped(object sender, DataGridCellDoubleTappedEventArgs e)
    {
        // e.RowData 包含被双击单元格所在的行的数据对象
        if (e.RowData is MonitoringSite siteToView)
        {
            // 导航到详情页，并把 SiteID 作为参数传递过去
            await Shell.Current.GoToAsync($"{nameof(SiteDetailPage)}?siteId={siteToView.SiteID}");
        }
    }

}