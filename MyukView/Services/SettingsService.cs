using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using MyukView.Models;

namespace MyukView.Services
{
    /// <summary>
    /// 애플리케이션 설정 관리 서비스
    /// </summary>
    public class SettingsService
    {
        private const string SETTINGS_FILE = "settings.json";
        private readonly string _settingsPath;

        public SettingsService()
        {
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MyukView"
            );

            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            _settingsPath = Path.Combine(appDataPath, SETTINGS_FILE);
        }

        /// <summary>
        /// 설정 로드
        /// </summary>
        public async Task<AppSettings> LoadSettingsAsync()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    string json = await File.ReadAllTextAsync(_settingsPath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    return settings ?? new AppSettings();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"설정 로드 실패: {ex.Message}");
            }

            return new AppSettings();
        }

        /// <summary>
        /// 설정 저장
        /// </summary>
        public async Task SaveSettingsAsync(AppSettings settings)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string json = JsonSerializer.Serialize(settings, options);
                await File.WriteAllTextAsync(_settingsPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"설정 저장 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 설정 초기화
        /// </summary>
        public async Task ResetSettingsAsync()
        {
            var defaultSettings = new AppSettings();
            await SaveSettingsAsync(defaultSettings);
        }
    }
}
