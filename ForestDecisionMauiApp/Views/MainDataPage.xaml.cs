// Views/MainDataPage.xaml.cs
using ForestDecisionMauiApp.ViewModels;

namespace ForestDecisionMauiApp.Views;

public partial class MainDataPage : ContentPage
{
  //  private bool _isInitialLoad = true; // 标志是否是初次加载

    public MainDataPage(MainDataViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MainDataViewModel vm)
        {
            // 每次页面出现时都尝试加载/刷新数据
            // 或者，你可以设置一个标志位，仅在从 AddEditSitePage 返回时才强制刷新
            if (vm.LoadSitesCommand.CanExecute(null))
            {
                await vm.LoadSitesCommand.ExecuteAsync(null);
            }
        }
    }
}