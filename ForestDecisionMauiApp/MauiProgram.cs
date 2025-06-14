// MauiProgram.cs
using Microsoft.Extensions.Logging;
using ForestDecisionMauiApp.Services; // Add this
using ForestDecisionMauiApp.ViewModels; // We'll create these soon
using ForestDecisionMauiApp.Views;      // We'll create these soon
using ForestDecisionMauiApp.Services;
using CommunityToolkit.Maui;
using Syncfusion.Maui.Core.Hosting;
using CommunityToolkit.Maui.Views;
using ForestDecisionMauiApp.Interfaces; // <-- 新增

#if WINDOWS
using ForestDecisionMauiApp.Platforms.Windows; // <-- 新增
#endif

namespace ForestDecisionMauiApp;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {

        string syncfusionLicenseKey = "Ngo9BigBOggjHTQxAR8/V1NNaF1cXGNCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXlfcXZXRmZZVUZ+WkdWYUA=";
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

        // ▼▼▼ 新增平台特定服务的注册 ▼▼▼
#if WINDOWS
        builder.Services.AddTransient<IFileSaveService, FileSaveService>();
#endif
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲


        // --- ViewModels ---
        builder.Services.AddSingleton<AppShellViewModel>(); // Shell ViewModel 最好是单例
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<DashboardViewModel>(); 
        builder.Services.AddTransient<SiteManagementViewModel>(); 
        builder.Services.AddTransient<AddEditSiteViewModel>();
        builder.Services.AddTransient<SiteDetailViewModel>();
        builder.Services.AddTransient<UserManagementViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<AddEditUserViewModel>();
        builder.Services.AddTransient<DataManagementViewModel>();
        

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
        builder.Services.AddTransient<DataManagementPage>();
        // ...

        return builder.Build();
    }
}