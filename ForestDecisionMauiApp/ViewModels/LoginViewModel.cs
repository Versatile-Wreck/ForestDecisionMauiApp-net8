// ViewModels/LoginViewModel.cs
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel; // Install CommunityToolkit.Mvvm NuGet package
using CommunityToolkit.Mvvm.Input;
using ForestDecisionMauiApp.Services;
using ForestDecisionMauiApp.Views; // For navigation

namespace ForestDecisionMauiApp.ViewModels
{
    public partial class LoginViewModel : ObservableObject // For INotifyPropertyChanged
    {
        private readonly UserService _userService;
        // No need to inject Navigation here, Shell handles it or pass it from Page

        [ObservableProperty] // Generates Username property with change notification
        private string _username;

        [ObservableProperty] // Generates Password property with change notification
        private string _password;

        [ObservableProperty]
        private string _loginMessage;

        public LoginViewModel(UserService userService)
        {
            _userService = userService;
        }

        [RelayCommand] // Generates LoginCommand
        private async Task Login()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                LoginMessage = "Username and password cannot be empty.";
                await Application.Current.MainPage.DisplayAlert("Login Failed", LoginMessage, "OK");
                return;
            }

            var user = _userService.LoginUser(Username, Password); // Your existing LoginUser method
            if (user != null)
            {
                LoginMessage = $"欢迎 {user.FullName}!";
                await Application.Current.MainPage.DisplayAlert("登录成功", LoginMessage, "OK");

                // 显示主界面的 TabBar
                if (Shell.Current is AppShell appShellInstance)
                {
                    appShellInstance.SetMainUITabBarVisibility(true);
                }

                // 导航到 MainDataPage
                await Shell.Current.GoToAsync($"//AppMainContent/{nameof(MainDataPage)}");
            }
            else
            {
                LoginMessage = "无效的用户名或密码。";
                await Application.Current.MainPage.DisplayAlert("登录失败", LoginMessage, "OK");
            }
        }

        [RelayCommand]
        private async Task GoToRegister()
        {
            // Navigate to the registration page
            await Shell.Current.GoToAsync(nameof(RegisterPage));
        }
    }
}