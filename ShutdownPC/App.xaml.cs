using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ShutdownPC
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Directory.SetCurrentDirectory($"C:\\Users\\{Environment.UserName}\\AppData\\Local\\Temp");
            StreamReader file = new StreamReader("CurryPCShutdownTimeTemp.CF");
            int time = int.Parse(file.ReadToEnd());
            file.Close();
            if (time != 0)
            {
                var result = MessageBox.Show("Отменить выключение компьютера?",
                    "Shutdown PC",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.Yes);
                if (result == MessageBoxResult.Yes)
                {
                    Process p = new Process();
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.FileName = "cmd.exe";
                    p.StartInfo.Arguments = $"/c shutdown /a";
                    p.StartInfo.CreateNoWindow = true;
                    p.Start();
                    MessageBox.Show("Отключение компьютера отменено", "Shutdown PC");
                }
            }
        }
    }
}
