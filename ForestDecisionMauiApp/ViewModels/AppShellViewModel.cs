// ViewModels/AppShellViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ForestDecisionMauiApp.Views;

namespace ForestDecisionMauiApp.ViewModels
{
    public partial class AppShellViewModel : ObservableObject
    {
        private readonly AuthenticationService _authService; // 新增

        public bool IsAdmin => _authService.IsAdmin;

        public AppShellViewModel(AuthenticationService authService) // 修改构造函数
        {
            _authService = authService;
            // 监听 CurrentUser 变化，以刷新 IsAdmin 属性，从而更新UI
            _authService.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(AuthenticationService.CurrentUser))
                {
                    OnPropertyChanged(nameof(IsAdmin));
                }
            };
        }

        [RelayCommand]
        private async Task Logout()
        {
            _authService.Logout(); // **登出时，清空当前用户**
            // 导航回登录页面。使用绝对路由清空导航堆栈。
            await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
        }
    }
}