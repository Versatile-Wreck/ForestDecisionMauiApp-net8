// Views/UserManagementPage.xaml.cs

using ForestDecisionMauiApp.ViewModels;
using ForestDecisionMauiApp.Models; // ȷ�������� UserModel

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
        // ÿ�ν���ҳ��ʱ�������û��б�
        _viewModel.LoadUsersCommand.Execute(null);
    }
}