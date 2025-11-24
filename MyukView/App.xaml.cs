using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MyukView
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 명령줄 인자로 파일이 전달된 경우
            if (e.Args.Length > 0)
            {
                string filePath = e.Args[0];
                if (System.IO.File.Exists(filePath))
                {
                    // MainWindow에 파일 경로 전달
                    var mainWindow = new MainWindow(filePath);
                    mainWindow.Show();
                    return;
                }
            }

            // 기본 실행
            var defaultWindow = new MainWindow();
            defaultWindow.Show();
        }
    }
}
