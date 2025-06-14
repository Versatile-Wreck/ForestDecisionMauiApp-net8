// ViewModels/SettingsViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ForestDecisionMauiApp.Services;
using ForestDecisionMauiApp.Views;

namespace ForestDecisionMauiApp.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly AuthenticationService _authService;

        // 用于在XAML中绑定按钮的可见性
        public bool CanManageUsers => _authService.IsAdmin;

        public SettingsViewModel(AuthenticationService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        private async Task GoToUserManagement()
        {
            // 只有当按钮可见时，这个命令才能被触发，但我们可以在此添加二次检查
            if (!CanManageUsers)
            {
                await Application.Current.MainPage.DisplayAlert("无权限", "只有管理员才能访问用户管理。", "好的");
                return;
            }
            await Shell.Current.GoToAsync(nameof(UserManagementPage));
        }

        [RelayCommand]
        private async Task GoToDataManagement()
        {
            // 这里可以导航到我们之前设计的备份恢复页面
            // await Application.Current.MainPage.DisplayAlert("提示", "数据管理功能（备份/恢复）待整合到专属页面。", "好的");
            await Shell.Current.GoToAsync(nameof(DataManagementPage));
        }
    }
}