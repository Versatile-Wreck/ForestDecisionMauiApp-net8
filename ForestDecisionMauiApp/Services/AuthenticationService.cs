// Services/AuthenticationService.cs
using ForestDecisionMauiApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;

public partial class AuthenticationService : ObservableObject
{
    [ObservableProperty]
    private User _currentUser;

    public bool IsAdmin => CurrentUser?.Role == UserRole.Administrator;
    public bool IsResearcher => CurrentUser?.Role == UserRole.Researcher;
    public bool IsOperator => CurrentUser?.Role == UserRole.Operator;

    public void Login(User user)
    {
        CurrentUser = user;
    }

    public void Logout()
    {
        CurrentUser = null;
    }
}