using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Media.Effects;

namespace RocketBookWall
{
    /// <summary>
    /// Interaction logic for PDFViewItem.xaml
    /// </summary>
    public partial class PDFViewItem : Window
    {
        private bool ForceUpdate = false;
        private DateTime LastUpdate = new DateTime();
        private bool IsDoneLoading = false;
        private StartWindow StartWindowRef;
        private ParseWindowData.SaveData WindowData;
        public bool ShuttingDown = false;

        public PDFViewItem(StartWindow _StartWindowRef, ParseWindowData.SaveData _WindowData)
        {
            InitializeComponent();

            StartWindowRef = _StartWindowRef;
            WindowData = _WindowData;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            WindowData.Folder = TargetFolder.Text;
            ParseWindowData.UpdateListItem(WindowData, StartWindowRef.WindowData);
            ForceUpdate = true;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (IsDoneLoading)
            {
                WindowData.Height = this.ActualHeight;
                WindowData.Width = this.ActualWidth;
                ParseWindowData.UpdateListItem(WindowData, StartWindowRef.WindowData);
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TargetFolder.Text = WindowData.Folder;
            this.Width = WindowData.Width;
            this.Height = WindowData.Height;
            this.Left = WindowData.X;
            this.Top = WindowData.Y;
            IsDoneLoading = true;
            await UpdatePDFTask();
        }

        private async Task UpdatePDFTask()
        {
            while (true)
            {
                if (Directory.Exists(TargetFolder.Text))
                {
                    this.Title = "RocketBookWall - " + TargetFolder.Text.Substring(TargetFolder.Text.LastIndexOf('\\') + 1);

                    string[] Files = Directory.GetFiles(TargetFolder.Text);
                    if (Files.Length != 0)
                    {
                        string NewestFile = "";
                        foreach (string file in Files)
                        {
                            if (NewestFile == "")
                                NewestFile = file;
                            if (File.GetLastWriteTime(file) > File.GetLastWriteTime(NewestFile))
                                NewestFile = file;
                        }

                        if (LastUpdate != File.GetLastWriteTime(NewestFile))
                        {
                            LastUpdate = File.GetLastWriteTime(NewestFile);
                            pdfWebViewer.Navigate(new Uri(NewestFile));
                        }
                    }
                }

                await Delay(10);
            }
        }

        private async Task Delay(int Seconds)
        {
            for (int i = 0; i < Seconds; i++)
            {
                await Task.Delay(1000);
                if (ForceUpdate)
                {
                    ForceUpdate = false;
                    break;
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsDoneLoading)
            {
                WindowData.Folder = TargetFolder.Text;
                WindowData.Height = this.ActualHeight;
                WindowData.Width = this.ActualWidth;
                WindowData.X = this.Left;
                WindowData.Y = this.Top;
                ParseWindowData.UpdateListItem(WindowData, StartWindowRef.WindowData);

                if (!ShuttingDown)
                    StartWindowRef.CloseAllWindows(this);
            }
        }

        public static async Task FadeInObject(UIElement Item)
        {
            Item.Visibility = Visibility.Visible;
            for (float i = 0; i < 1; i += 0.1f)
            {
                Item.Opacity = i;
                await Task.Delay(10);
            }
            Item.Opacity = 1;
        }

        public static async Task FadeOutObject(UIElement Item)
        {
            for (float i = 1; i > 0; i -= 0.1f)
            {
                Item.Opacity = i;
                await Task.Delay(10);
            }
            Item.Opacity = 0;
            Item.Visibility = Visibility.Hidden;
        }

        private void AddNewButton_Click(object sender, RoutedEventArgs e)
        {
            StartWindowRef.AddNewWindow();
        }

        private void YesRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            ParseWindowData.RemoveListItem(WindowData, StartWindowRef.WindowData);
            IsDoneLoading = false;
            this.Close();
        }

        private async void NoDontRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            await FadeOutObject(AreYouSureGrid);
            await FadeInObject(pdfWebViewer);
        }

        private async void RemoveThisButton_Click(object sender, RoutedEventArgs e)
        {
            await FadeOutObject(pdfWebViewer);
            await FadeInObject(AreYouSureGrid);
        }
    }
}
