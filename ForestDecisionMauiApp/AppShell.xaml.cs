// AppShell.xaml.cs
using ForestDecisionMauiApp.Views;
using ForestDecisionMauiApp.ViewModels;

namespace ForestDecisionMauiApp;

public partial class AppShell : Shell
{
    public AppShell(AppShellViewModel viewModel) // 注入 ViewModel
    {
        InitializeComponent();

        // 将 ViewModel 设置为 Shell 的 BindingContext，这样 MenuItem 的命令才能绑定
        BindingContext = viewModel;

        // 注册那些不在 Shell 可视化结构中（不在侧边栏）但需要导航的页面
        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        Routing.RegisterRoute(nameof(AddEditSitePage), typeof(AddEditSitePage));
        Routing.RegisterRoute(nameof(SiteDetailPage), typeof(SiteDetailPage));
        Routing.RegisterRoute(nameof(UserManagementPage), typeof(UserManagementPage)); // <-- 新增这一行
        Routing.RegisterRoute(nameof(AddEditUserPage), typeof(AddEditUserPage));
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