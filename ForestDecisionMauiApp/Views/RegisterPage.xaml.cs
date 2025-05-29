// Views/RegisterPage.xaml.cs
using ForestDecisionMauiApp.ViewModels;

namespace ForestDecisionMauiApp.Views;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(RegisterViewModel viewModel) // “¿¿µ◊¢»Î viewModel
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}