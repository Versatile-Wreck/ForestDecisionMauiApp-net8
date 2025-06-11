// Views/UserManagementPage.xaml.cs

using ForestDecisionMauiApp.ViewModels;
using ForestDecisionMauiApp.Models; // 确保引用了 UserModel

namespace ForestDecisionMauiApp.Views;


public partial class UserManagementPage : ContentPage
{
    private readonly UserManagementViewModel _viewModel;
    public UserManagementPage(UserManagementViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // 每次进入页面时都加载用户列表
        _viewModel.LoadUsersCommand.Execute(null);
    }
}