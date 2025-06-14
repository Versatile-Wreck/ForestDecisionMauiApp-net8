// ViewModels/DataManagementViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ForestDecisionMauiApp.Interfaces;
using ForestDecisionMauiApp.Services;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ForestDecisionMauiApp.ViewModels
{
    public partial class DataManagementViewModel : ObservableObject
    {
        private readonly DatabaseService _dbService;
        private readonly IFileSaveService _fileSaveService;

        [ObservableProperty]
        private bool _isBusy;

        public DataManagementViewModel(DatabaseService dbService, IFileSaveService fileSaveService)
        {
            _dbService = dbService;
            _fileSaveService = fileSaveService;
        }

        [RelayCommand]
        private async Task BackupDatabaseAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var suggestedFileName = $"ForestDB_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.db";

                // 1. 调用平台特定服务，弹出“另存为”对话框，获取用户选择的保存路径
                string targetFilePath = await _fileSaveService.SaveFileAsync(suggestedFileName, ".db");

                // 2. 如果用户选择了路径 (没有取消)
                if (!string.IsNullOrWhiteSpace(targetFilePath))
                {
                    // 3. 执行在线备份
                    bool success = await _dbService.BackupDatabaseAsync(targetFilePath);

                    if (success)
                    {
                        await Application.Current.MainPage.DisplayAlert("备份成功", $"数据库已成功备份到:\n{targetFilePath}", "好的");
                    }
                    // 失败的提示已经在 DatabaseService 内部通过 DisplayAlert 处理了
                }
                else
                {
                    Debug.WriteLine("备份操作被用户取消。");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("备份异常", $"备份过程中发生错误: {ex.Message}", "好的");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task RestoreDatabaseAsync()
        {
            if (IsBusy) return;

            // 1. 弹出严重警告，让用户确认操作
            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "恢复数据警告",
                "此操作将用备份文件完全覆盖当前所有数据，且操作不可逆！\n\n恢复成功后，强烈建议您重启应用程序。\n\n您确定要继续吗？",
                "是的，恢复",
                "取消");

            if (!confirm) return;

            IsBusy = true;
            try
            {
                // 2. 使用 MAUI Essentials 的 FilePicker 让用户选择备份文件
                var pickOptions = new PickOptions
                {
                    PickerTitle = "请选择数据库备份文件 (.db)",
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.WinUI, new[] { ".db" } },
                        { DevicePlatform.macOS, new[] { "db" } },
                    })
                };

                var result = await FilePicker.Default.PickAsync(pickOptions);
                if (result != null)
                {
                    if (result.FileName.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
                    {
                        // 3. 调用服务执行恢复操作
                        bool success = await _dbService.RestoreDatabaseAsync(result.FullPath);

                        if (success)
                        {
                            await Application.Current.MainPage.DisplayAlert(
                                "恢复成功",
                                "数据库已从备份文件恢复。\n\n请立即重启应用程序以确保数据正确加载。",
                                "好的");
                            // 实际生产应用中，可以考虑在这里强制退出应用
                            // Application.Current.Quit();
                        }
                        // 失败的提示已在 DatabaseService 内部处理
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("文件无效", "请选择一个有效的 .db 数据库备份文件。", "好的");
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("恢复异常", $"恢复过程中发生错误: {ex.Message}", "好的");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}