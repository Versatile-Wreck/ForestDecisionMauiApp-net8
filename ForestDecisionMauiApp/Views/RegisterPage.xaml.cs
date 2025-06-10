// Views/RegisterPage.xaml.cs
using ForestDecisionMauiApp.ViewModels;

namespace ForestDecisionMauiApp.Views;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(RegisterViewModel viewModel) // 依赖注入 viewModel
    {
        InitializeComponent(); // 初始化界面组件（自动生成的方法，构造函数中必须首先调用）
        BindingContext = viewModel; // 设置数据绑定上下文为传入的 ViewModel 实例
    }
}