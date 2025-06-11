// ViewModels/UserManagementViewModel.cs
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ForestDecisionMauiApp.Models;
using ForestDecisionMauiApp.Services;
using ForestDecisionMauiApp.Views;

namespace ForestDecisionMauiApp.ViewModels;

public partial class UserManagementViewModel : ObservableObject
{
    private readonly UserService _userService;
    public ObservableCollection<User> Users { get; } = new();

    [ObservableProperty]
    private bool _isBusy;

    public UserManagementViewModel(UserService userService)
    {
        _userService = userService;
    }

    [RelayCommand]
    private async Task LoadUsersAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            Users.Clear();
            var users = await Task.Run(() => _userService.GetAllUsers());
            foreach (var user in users)
            {
                Users.Add(user);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteUserAsync(User user)
    {
        if (user == null) return;

        bool confirm = await Application.Current.MainPage.DisplayAlert("确认删除", $"确定要删除用户 {user.Username} 吗？", "是", "否");
        if (confirm)
        {
            bool success = await Task.Run(() => _userService.DeleteUser(user.UserID));
            if (success)
            {
                Users.Remove(user);
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("错误", "删除用户失败。", "OK");
            }
        }
    }

    // 我们暂时不实现新增和编辑用户的页面，只先搭建框架
    [RelayCommand]
    private async Task AddUserAsync()
    {
        // 导航到新增页面，不带参数
        await Shell.Current.GoToAsync(nameof(AddEditUserPage));
    }

    [RelayCommand]
    private async Task EditUserAsync(User user)
    {
        if (user == null) return;
        // 导航到编辑页面，并传递 UserID
        await Shell.Current.GoToAsync($"{nameof(AddEditUserPage)}?userId={user.UserID}");
    }
}