// AppShell.xaml.cs
using ForestDecisionMauiApp.Views;

namespace ForestDecisionMauiApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // 注册 RegisterPage (LoginPage 和 MainDataPage 由 AppShell.xaml 中的 ShellContent 定义了路由)
        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        Routing.RegisterRoute(nameof(AddEditSitePage), typeof(AddEditSitePage));

        this.Loaded += AppShell_Loaded;
    }

    private async void AppShell_Loaded(object sender, EventArgs e)
    {
        this.Loaded -= AppShell_Loaded; // 避免重复执行

        var mainTabBar = this.FindByName<TabBar>("MainTabBar");
        if (mainTabBar != null)
        {
            mainTabBar.IsVisible = false; // 初始隐藏主内容区域的 TabBar
        }

        // 导航到 LoginPage ShellContent。因为 LoginPage 现在是Shell的直接内容，
        // 使用绝对路由 //LoginPage 是合适的。
        await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
    }

    // 可以添加一个公共方法来控制 TabBar 的可见性，供 ViewModel 调用
    public void SetMainUITabBarVisibility(bool isVisible)
    {
        var mainTabBar = this.FindByName<TabBar>("MainTabBar");
        if (mainTabBar != null)
        {
            mainTabBar.IsVisible = isVisible;
        }
    }
}