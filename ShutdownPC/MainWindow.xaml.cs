using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace ShutdownPC
{
    public partial class MainWindow : Window
    {
        string version = "1.4";
        public string pressed = "all";
        public int time = 0;
        public float timeMultiply = 1;
        public bool isNeedShutdown = false;
        public int totaltime = 0;
        public bool isNeedRestart = false;
        public bool isPauseThreads = false;
        void Program()
        {
            VersionLabel.Content = $"ver - {version}";
            LabelInfo.Visibility = Visibility.Hidden;
            DisableButtons();
            Thread RefreshLabelTimeThread = new Thread(RefreshLabelTime);
            RefreshLabelTimeThread.Start();
            Thread TimeCounterThread = new Thread(TimeCounter);
            TimeCounterThread.Start();
            Thread ProgressInfoThread = new Thread(RefreshProgressInfo);
            ProgressInfoThread.Start();
/*            Thread RefreshTimeFileThread = new Thread(RefreshTimeFile);
            RefreshTimeFileThread.Start();*/
            Closing += MainWindow_Closing;
        }

        public MainWindow()
        {
            InitializeComponent();
            Directory.SetCurrentDirectory($"C:\\Users\\{Environment.UserName}\\AppData\\Local\\Temp");
            Program();
        }
        private void SetTimeToTextBoxes(TextBox[] Temp, int plusHour = 0, int plusMin = 0, int plusSec = 0)
        {
            DateTime dateTime = DateTime.Now;
            if ((dateTime.Hour + plusHour) == 24)
            {
                Temp[0].Text = "00";
            }
            else
            {
                Temp[0].Text = SetTimeWithZero((dateTime.Hour + plusHour).ToString());
            }
            Temp[1].Text = SetTimeWithZero((dateTime.Minute + plusMin).ToString());
            Temp[2].Text = SetTimeWithZero((dateTime.Second + plusSec).ToString());
        }
        private void ClearTextBoxes()
        {
            TextBox[] Temp = new TextBox[3] { TextBoxHour, TextBoxMin, TextBoxSec };
            foreach (TextBox item in Temp)
            {
                item.Text = "0";
            }
        }
        private void textBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }
        private void DisableButtons(bool type = false, bool all = false)
        {
            if (all)
            {
                RadioButtonOffAtTime.IsEnabled = type;
                RadioButtonWaitTime.IsEnabled = type;
            }
            LeftHour.IsEnabled = type;
            LeftMin.IsEnabled = type;
            LeftSec.IsEnabled = type;
            TextBoxHour.IsEnabled = type;
            TextBoxMin.IsEnabled = type;
            TextBoxSec.IsEnabled = type;
        }
        private void RadioButtonWaitTime_Checked(object sender, RoutedEventArgs e)
        {
            pressed = "Left";
            DisableButtons(true);
            ClearTextBoxes();
            EnterButton.IsEnabled = true;
            ChangeMaxLenTextBoxes(4);
        }

        private void RadioButtonOffAtTime_Checked(object sender, RoutedEventArgs e)
        {
            pressed = "Right";
            DisableButtons(true);
            ClearTextBoxes();
            SetTimeToTextBoxes(new TextBox[] { TextBoxHour, TextBoxMin, TextBoxSec }, 1);
            EnterButton.IsEnabled = true;
            ChangeMaxLenTextBoxes(2);
        }
        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            DisableButtons(false, true);
            CancelButton.IsEnabled = true;
            EnterButton.IsEnabled = false;
            if (pressed == "Left")
            {
                time = (int.Parse(TextBoxHour.Text) * 3600) + (int.Parse(TextBoxMin.Text) * 60) + int.Parse(TextBoxSec.Text);
                Shutdown(time);
            }
            if (pressed == "Right")
            {
                if (int.Parse(TextBoxHour.Text) > 23)
                {
                    TextBoxHour.Text = "23";
                }
                if (int.Parse(TextBoxMin.Text) > 59)
                {
                    TextBoxMin.Text = "59";
                }
                if (int.Parse(TextBoxSec.Text) > 59)
                {
                    TextBoxSec.Text = "59";
                }
                DateTime CurrentDate = DateTime.Now;
                DateTime NeedToShutdown = new DateTime(CurrentDate.Year, CurrentDate.Month, CurrentDate.Day,
                     int.Parse(TextBoxHour.Text), int.Parse(TextBoxMin.Text), int.Parse(TextBoxSec.Text));
                if (NeedToShutdown < CurrentDate)
                {
                    NeedToShutdown.AddDays(1);
                    TimeSpan Temp = NeedToShutdown - CurrentDate;
                    time = 86400 + (int)Temp.TotalSeconds;
                }
                else
                {
                    TimeSpan Temp = NeedToShutdown - CurrentDate;
                    time = (int)Temp.TotalSeconds;
                }
            }
            totaltime = time;
            isNeedShutdown = true;
            LabelInfo.Visibility = Visibility.Visible;
        }
        private void ChangeMaxLenTextBoxes(int len)
        {
            TextBox[] Temp = new TextBox[] { TextBoxHour, TextBoxMin, TextBoxSec };
            foreach (TextBox item in Temp)
            {
                item.MaxLength = len;
            }
        }
        private void RefreshLabelTime()
        {
            while (true)
            {
                if (isPauseThreads)
                {
                    Thread.Sleep((int)(125 * timeMultiply));
                    continue;
                }
                int[] temp = SplitTime(time);
                Dispatcher.Invoke(
                    () => LabelInfo.Content = $"Осталось: {temp[0]} часов, {temp[1]} минут, {temp[2]} секунд", DispatcherPriority.Normal);
                Thread.Sleep((int)(125 * timeMultiply));
            }
        }
        private void RefreshProgressInfo()
        {
            while (true)
            {
                if (isPauseThreads)
                {
                    Thread.Sleep((int)(125 * timeMultiply));
                    continue;
                }
                float neednumber = 0;
                if (totaltime > 0)
                {
                    neednumber = 100 - (((float)time / (float)totaltime) * 100);
                }
                else
                {
                    neednumber = 0;
                }
                Dispatcher.Invoke(() => ProgressInfo.Value = neednumber, DispatcherPriority.Normal);
                Thread.Sleep((int)(125 * timeMultiply));
            }
        }
        private void TimeCounter()
        {
            while (true)
            {
                if (isPauseThreads)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                if (time != 0)
                {
                    time -= 1;
                }
                else
                {
                    if (isNeedShutdown)
                    {
                        Shutdown(0);
                    }
                }
                Thread.Sleep(1000);
            }
        }
        private int[] SplitTime(int seconds)
        {
            int[] temp = new int[3];
            int hour = seconds / 3600;
            seconds -= (hour * 3600);
            int min = seconds / 60;
            seconds -= (min * 60);
            temp[0] = hour;
            temp[1] = min;
            temp[2] = seconds;
            return temp;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            //Shutdown(0, false);
            DisableButtons(true, true);
            EnterButton.IsEnabled = true;
            CancelButton.IsEnabled = false;
            isNeedShutdown = false;
            time = 0;
            totaltime = 0;
        }
        private void Shutdown(int time = 0, bool needShutdown = true)
        {
            if (isNeedShutdown)
            {
                if (needShutdown)
                {
                    if (!isNeedRestart)
                    {
                        CMDcommand($"shutdown -s -f -t {time}");
                    }
                    else
                    {
                        CMDcommand($"shutdown -r -f -t {time}");
                    }
                }
            }
            if (!needShutdown)
            {
                System.Diagnostics.Process.Start("cmd", "/c shutdown /a");
            }
        }
        bool _IsAnimating = false;
        private void ProgressInfo_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_IsAnimating)
                return;

            _IsAnimating = true;

            DoubleAnimation doubleAnimation = new DoubleAnimation
                (e.OldValue, e.NewValue, new Duration(TimeSpan.FromSeconds(0.3)), FillBehavior.Stop);
            doubleAnimation.Completed += Db_Completed;

            ((ProgressBar)sender).BeginAnimation(ProgressBar.ValueProperty, doubleAnimation);

            e.Handled = true;
        }
        private void Db_Completed(object sender, EventArgs e)
        {
            _IsAnimating = false;
        }
        private void CMDcommand(string command)
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = "/c " + command;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            Environment.Exit(0);
        }

        private string SetTimeWithZero(string text)
        {
            string needreturn = text;
            if (text.Length == 1)
            {
                needreturn = $"0{text}";
            }
            return needreturn;
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //Shutdown(0, false);
            isNeedRestart = true;
            //Shutdown(time, true);
        }
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            //Shutdown(0, false);
            isNeedRestart = false;
            //Shutdown(time, true);
        }
        private void RefreshTimeFile()
        {
            while (true)
            {
                if (isNeedRestart || isNeedShutdown)
                {
                    StreamWriter file = new StreamWriter("CurryPCShutdownTimeTemp.CF");
                    file.Write(time.ToString());
                    file.Close();
                }
                else
                {
                    StreamWriter file = new StreamWriter("CurryPCShutdownTimeTemp.CF");
                    file.Write("0");
                    file.Close();
                }
                Thread.Sleep((int)(500f * timeMultiply));
            }
        }
        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            isPauseThreads = true;
            if (isNeedShutdown)
            {
                MessageBox.Show("Выключение компьютера отменено");
            }
            Environment.Exit(0);
        }
    }
}