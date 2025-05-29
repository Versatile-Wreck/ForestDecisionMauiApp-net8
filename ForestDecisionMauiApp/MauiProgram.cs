// MauiProgram.cs
using Microsoft.Extensions.Logging;
using ForestDecisionMauiApp.Services; // Add this
using ForestDecisionMauiApp.ViewModels; // We'll create these soon
using ForestDecisionMauiApp.Views;      // We'll create these soon
using ForestDecisionMauiApp.Services;

namespace ForestDecisionMauiApp;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Register Services (Singleton or Transient as appropriate)
        // DatabaseService should ideally be singleton to manage one DB connection/file.
        builder.Services.AddSingleton<DatabaseService>();
        // UserService can be transient or scoped if it doesn't hold long-lived state,
        // or singleton if it primarily orchestrates calls to DatabaseService.
        // For simplicity here, let's make it singleton too.
        builder.Services.AddSingleton<UserService>();

        builder.Services.AddSingleton<DecisionService>(); // <-- 新增对 DecisionService 的注册

        // Register ViewModels (typically Transient)
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>(); // You'll add this later
        builder.Services.AddTransient<MainDataViewModel>(); // You'll add this later

        // Register Pages (typically Transient)
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>(); // You'll add this later
        builder.Services.AddTransient<MainDataPage>();  // You'll add this later


        builder.Services.AddTransient<AddEditSiteViewModel>(); // AddEditSiteViewModel 通常是 Transient
        builder.Services.AddTransient<AddEditSitePage>();
        // ...

        return builder.Build();
    }
}