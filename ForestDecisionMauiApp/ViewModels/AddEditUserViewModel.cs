// ViewModels/AddEditUserViewModel.cs
using System.Collections.ObjectModel;
using System.Linq;
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
    private string _fullName;
    [ObservableProperty]
    private UserRole _selectedRole;

    public ObservableCollection<UserRole> Roles { get; }

    public string UserId { get; set; }

    [ObservableProperty]
    private string _errorMessage;



    public AddEditUserViewModel(UserService userService)
    {
        _userService = userService;
        Roles = new ObservableCollection<UserRole>(Enum.GetValues(typeof(UserRole)).Cast<UserRole>());
    }

    // ViewModels/AddEditUserViewModel.cs

    // 这个方法应该在 ViewModel 被激活后 (例如页面 OnAppearing) 调用
    public async Task InitializeAsync()
    {
        IsEditMode = !string.IsNullOrWhiteSpace(UserId);
        PageTitle = IsEditMode ? "编辑用户" : "新增用户";
        ErrorMessage = string.Empty;

        if (IsEditMode)
        {
            // 编辑模式
            PageTitle = "编辑用户";
            _userToEdit = await Task.Run(() => _userService.GetUserByUsername(Username)); // 假设可以通过Username获取
                                                                                          // 在实际项目中，通过唯一的 UserId 获取会更可靠。你需要在 DatabaseService 和 UserService 中添加 GetUserById 方法。
                                                                                          // 为了让程序运行，我们先用 GetAllUsers 临时代替。
            if (_userToEdit == null)
            {
                var users = await Task.Run(() => _userService.GetAllUsers());
                _userToEdit = users.FirstOrDefault(u => u.UserID == UserId);
            }

            if (_userToEdit != null)
            {
                Username = _userToEdit.Username;
                FullName = _userToEdit.FullName;
                SelectedRole = _userToEdit.Role;
                // 编辑模式下不应显示或修改密码，除非是“重置密码”功能
                Password = string.Empty;
            }
            else
            {
                ErrorMessage = "加载用户信息失败。";
                await Application.Current.MainPage.DisplayAlert("错误", ErrorMessage, "OK");
                await Shell.Current.GoToAsync("..");
            }
        }
        else
        {
            // 新增模式
            IsEditMode = false;
            PageTitle = "新增用户";
            Username = string.Empty;
            FullName = string.Empty;
            SelectedRole = Roles.FirstOrDefault();
            Password = string.Empty;
        }
    }

    [RelayCommand]
    private async Task SaveUserAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(FullName))
        {
            await Application.Current.MainPage.DisplayAlert("错误", "用户名和全名不能为空。", "好的");
            return;
        }

        if (IsEditMode)
        {
            _userToEdit.FullName = FullName;
            _userToEdit.Role = SelectedRole;
            // 如果需要支持重置密码，可以在这里添加逻辑
            bool success = await Task.Run(() => _userService.UpdateUser(_userToEdit));
            await Application.Current.MainPage.DisplayAlert(success ? "成功" : "失败", "用户信息已更新。", "好的");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(Password))
            {
                await Application.Current.MainPage.DisplayAlert("错误", "新增用户时密码不能为空。", "好的");
                return;
            }
            var newUser = _userService.RegisterUser(Username, Password, FullName, SelectedRole);
            await Application.Current.MainPage.DisplayAlert(newUser != null ? "成功" : "失败", "新用户已创建。", "好的");
        }

        await Shell.Current.GoToAsync(".."); // 返回上一页
    }
}