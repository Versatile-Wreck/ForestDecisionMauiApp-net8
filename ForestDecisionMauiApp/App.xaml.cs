// App.xaml.cs
using System.IO; // 用于文件操作
using System;    // 用于 DateTime 和 Exception
using Microsoft.Maui.ApplicationModel; // 用于 FileSystem
using System.Diagnostics;

namespace ForestDecisionMauiApp;

public partial class App : Application
{
    public App(AppShell appShell)
    {
        InitializeComponent();

        MainPage = appShell;

        // 设置全局未处理异常处理器
        AppDomain.CurrentDomain.UnhandledException += App_UnhandledException;
    }

    private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Exception ex = e.ExceptionObject as Exception;

        // 准备日志信息
        string logMessage = $"{DateTime.Now} [FATAL] Unhandled Exception: {ex?.GetType().FullName}\n" +
                            $"Message: {ex?.Message}\n" +
                            $"StackTrace: {ex?.StackTrace}\n\n";

        if (ex?.InnerException != null)
        {
            logMessage += $"Inner Exception: {ex.InnerException?.GetType().FullName}\n" +
                          $"Message: {ex.InnerException?.Message}\n" +
                          $"StackTrace: {ex.InnerException?.StackTrace}\n\n";
        }

        // 定义日志文件路径 (应用数据目录)
        string logFilePath = Path.Combine(FileSystem.AppDataDirectory, "ForestApp_CrashLog.txt");

        try
        {
            // 将日志信息追加到文件
            File.AppendAllText(logFilePath, logMessage);
            Debug.WriteLine($"CRITICAL ERROR LOGGED TO: {logFilePath}");
        }
        catch (Exception logEx)
        {
            Debug.WriteLine($"Failed to write crash log: {logEx.Message}");
            Debug.WriteLine($"Original crash: {logMessage}");
        }
    }
}
