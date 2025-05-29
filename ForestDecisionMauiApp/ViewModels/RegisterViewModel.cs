// ViewModels/RegisterViewModel.cs
using System.Collections.ObjectModel; // 用于 ObservableCollection
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ForestDecisionMauiApp.Models; // 引用 UserRole 枚举
using ForestDecisionMauiApp.Services;
using ForestDecisionMauiApp.Views; // 用于可能的导航

namespace ForestDecisionMauiApp.ViewModels
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly UserService _userService;

        [ObservableProperty]
        private string _username;

        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private string _confirmPassword;

        [ObservableProperty]
        private string _fullName;

        // 用于 Picker 选择角色
        public ObservableCollection<UserRole> Roles { get; }

        [ObservableProperty]
        private UserRole _selectedRole;

        [ObservableProperty]
        private string _registerMessage;

        public RegisterViewModel(UserService userService)
        {
            _userService = userService;
            // 初始化角色列表，用于 Picker 控件
            Roles = new ObservableCollection<UserRole>(Enum.GetValues(typeof(UserRole)).Cast<UserRole>());
            SelectedRole = Roles.FirstOrDefault(); // 默认选择第一个角色
        }

        [RelayCommand]
        private async Task Register()
        {
            if (string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(ConfirmPassword) ||
                string.IsNullOrWhiteSpace(FullName))
            {
                RegisterMessage = "所有字段都不能为空。";
                await Application.Current.MainPage.DisplayAlert("注册失败", RegisterMessage, "OK");
                return;
            }

            if (Password != ConfirmPassword)
            {
                RegisterMessage = "两次输入的密码不匹配。";
                await Application.Current.MainPage.DisplayAlert("注册失败", RegisterMessage, "OK");
                return;
            }

            // 调用现有的 UserService 中的 RegisterUser 方法
            var user = _userService.RegisterUser(Username, Password, FullName, SelectedRole);

            if (user != null)
            {
                RegisterMessage = "用户注册成功！现在您可以登录了。";
                await Application.Current.MainPage.DisplayAlert("注册成功", RegisterMessage, "OK");
                // 注册成功后，可以导航回登录页面
                // "//" 表示绝对路由，回到 Shell 的根级路由
                await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
            }
            else
            {
                // UserService 中的 RegisterUser 方法内部应该已经通过 Console.WriteLine 打印了具体错误
                // 对于 UI，我们给出一个通用提示，或者可以从 UserService 返回更具体的错误信息
                RegisterMessage = "注册失败，请检查输入信息或用户名是否已存在。";
                await Application.Current.MainPage.DisplayAlert("注册失败", RegisterMessage, "OK");
            }
        }

        [RelayCommand]
        private async Task GoBackToLogin()
        {
            // 导航回登录页面
            await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
            // 或者使用相对导航 ".." 如果层级正确
            // await Shell.Current.GoToAsync("..");
        }
    }
}