// ViewModels/AddEditUserViewModel.cs
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ForestDecisionMauiApp.Models;
using ForestDecisionMauiApp.Services;

namespace ForestDecisionMauiApp.ViewModels;

[QueryProperty(nameof(UserId), "userId")]
public partial class AddEditUserViewModel : ObservableObject
{
    private readonly UserService _userService;
    private User _userToEdit;

    [ObservableProperty]
    private string _pageTitle;

    [ObservableProperty]
    private bool _isEditMode;

    // --- 用于绑定的用户属性 ---
    [ObservableProperty]
    private string _username;
    [ObservableProperty]
    private string _password;
    [ObservableProperty]
    private string _confirmPassword; // <-- 新增
    [ObservableProperty]
    private string _fullName;
    [ObservableProperty]
    private UserRole _selectedRole;
    [ObservableProperty]
    private string _errorMessage;

    public ObservableCollection<UserRole> Roles { get; }
    public string UserId { get; set; }

    public AddEditUserViewModel(UserService userService)
    {
        _userService = userService;
        Roles = new ObservableCollection<UserRole>(Enum.GetValues(typeof(UserRole)).Cast<UserRole>());
    }

    public async Task InitializeAsync()
    {
        IsEditMode = !string.IsNullOrWhiteSpace(UserId);
        PageTitle = IsEditMode ? "编辑用户" : "新增用户";
        ErrorMessage = string.Empty;

        if (IsEditMode)
        {
            // 编辑模式
            _userToEdit = await Task.Run(() => _userService.GetUserById(UserId));

            if (_userToEdit != null)
            {
                Username = _userToEdit.Username;
                FullName = _userToEdit.FullName;
                SelectedRole = _userToEdit.Role;
                Password = string.Empty;
                ConfirmPassword = string.Empty;
            }
            else
            {
                ErrorMessage = "加载用户信息失败，找不到该用户。";
                await Application.Current.MainPage.DisplayAlert("错误", ErrorMessage, "OK");
                await Shell.Current.GoToAsync("..");
            }
        }
        else
        {
            // 新增模式
            Username = string.Empty;
            FullName = string.Empty;
            SelectedRole = Roles.FirstOrDefault();
            Password = string.Empty;
            ConfirmPassword = string.Empty;
        }
    }

    [RelayCommand]
    private async Task SaveUserAsync()
    {
        ErrorMessage = string.Empty;
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(FullName))
        {
            ErrorMessage = "用户名和全名不能为空。";
            await Application.Current.MainPage.DisplayAlert("验证错误", ErrorMessage, "OK");
            return;
        }

        if (IsEditMode)
        {
            // --- 编辑用户的逻辑 ---
            _userToEdit.FullName = FullName;
            _userToEdit.Role = SelectedRole;

            bool success = await Task.Run(() => _userService.UpdateUser(_userToEdit));
            await Application.Current.MainPage.DisplayAlert(success ? "成功" : "失败", "用户信息已更新。", "好的");
        }
        else
        {
            // --- 新增用户的逻辑 ---
            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "新增用户时密码不能为空。";
                await Application.Current.MainPage.DisplayAlert("验证错误", ErrorMessage, "OK");
                return;
            }
            if (Password != ConfirmPassword)
            {
                ErrorMessage = "两次输入的密码不匹配。";
                await Application.Current.MainPage.DisplayAlert("验证错误", ErrorMessage, "OK");
                return;
            }

            var newUser = _userService.RegisterUser(Username, Password, FullName, SelectedRole);
            await Application.Current.MainPage.DisplayAlert(newUser != null ? "成功" : "失败", "新用户已创建。", "好的");
        }

        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}