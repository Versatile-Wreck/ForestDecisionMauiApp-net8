// Views/AddEditSitePage.xaml.cs
using ForestDecisionMauiApp.ViewModels;

namespace ForestDecisionMauiApp.Views;

public partial class AddEditSitePage : ContentPage
{
    private readonly AddEditSiteViewModel _viewModel;
    public AddEditSitePage(AddEditSiteViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    // 当页面出现时，调用 ViewModel 的初始化方法
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync(); // 加载数据或设置新增模式
    }
}