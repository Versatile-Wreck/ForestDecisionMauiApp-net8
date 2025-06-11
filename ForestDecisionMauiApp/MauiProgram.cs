// MauiProgram.cs
using Microsoft.Extensions.Logging;
using ForestDecisionMauiApp.Services; // Add this
using ForestDecisionMauiApp.ViewModels; // We'll create these soon
using ForestDecisionMauiApp.Views;      // We'll create these soon
using ForestDecisionMauiApp.Services;
using CommunityToolkit.Maui;
using Syncfusion.Maui.Core.Hosting;
using CommunityToolkit.Maui.Views;

namespace ForestDecisionMauiApp;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {

        string syncfusionLicenseKey = "Ngo9BigBOggjHTQxAR8/V1NNaF1cWWhPYVJxWmFZfVtgd19FY1ZQRmYuP1ZhSXxWdkNiWX9dc3dURGZaWUR9XUs=";
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncfusionLicenseKey);

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit() // <--- 初始化 Community Toolkit
            .ConfigureSyncfusionCore()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // --- Services ---
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<UserService>();
        builder.Services.AddSingleton<DecisionService>();
        builder.Services.AddSingleton<AuthenticationService>();

        // --- ViewModels ---
        builder.Services.AddSingleton<AppShellViewModel>(); // Shell ViewModel 最好是单例
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<DashboardViewModel>(); // 新增
        builder.Services.AddTransient<SiteManagementViewModel>(); // 重命名
        builder.Services.AddTransient<AddEditSiteViewModel>();
        builder.Services.AddTransient<SiteDetailViewModel>();
        builder.Services.AddTransient<UserManagementViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<AddEditUserViewModel>();
        
        // ... (以后还会有 SettingsViewModel 等)

        // --- Pages/Views ---
        builder.Services.AddSingleton<AppShell>(); // Shell 是单例
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<DashboardPage>(); // 新增
        builder.Services.AddTransient<SiteManagementPage>(); // 重命名
        builder.Services.AddTransient<AddEditSitePage>();
        builder.Services.AddTransient<SettingsPage>(); // 新增
        builder.Services.AddTransient<SiteDetailPage>();
        builder.Services.AddTransient<UserManagementPage>();
        builder.Services.AddTransient<AddEditUserPage>();
        // ...

        return builder.Build();
    }
}