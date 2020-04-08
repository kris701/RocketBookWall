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
using System.Reflection;
using System.IO;
using System.Windows.Resources;

namespace RocketBookWall
{
    /// <summary>
    /// Interaction logic for StartWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        public class SaveData
        {
            public double Width { get; set; }
            public double Height { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public string Folder { get; set; }
        }

        public List<SaveData> WindowData = new List<SaveData>();
        public List<PDFViewItem> WindowReferences = new List<PDFViewItem>();

        public StartWindow()
        {
            InitializeComponent();

            System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
            StreamResourceInfo sri = Application.GetResourceStream(new Uri("/logo.ico", UriKind.Relative));
            if (sri != null)
            {
                using (Stream s = sri.Stream)
                    ni.Icon = new System.Drawing.Icon(s);
            }
            ni.Visible = true;
            ni.Text = "RocketBookWall";
            ni.Click += ShowHideWindows;
            this.Show();
        }

        private void ShowHideWindows(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.WindowState = WindowState.Normal;
                foreach (PDFViewItem Window in WindowReferences)
                {
                    Window.WindowState = WindowState.Normal;
                    Window.Activate();
                }
            }
            else
            {
                this.WindowState = WindowState.Minimized;
                foreach (PDFViewItem Window in WindowReferences)
                {
                    Window.WindowState = WindowState.Minimized;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string[] CutA = Properties.Settings.Default.WindowData.Split(';');

            foreach (string Cut in CutA)
            {
                SaveDataToList(Cut);
            }

            foreach (SaveData Data in WindowData)
            {
                PDFViewItem NewWindow = new PDFViewItem(this, Data);
                WindowReferences.Add(NewWindow);
                NewWindow.Show();
            }

            if (WindowData.Count == 0)
            {
                AddNewWindow();
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }

        public void AddNewWindow()
        {
            SaveData saveData = new SaveData();
            saveData.Width = 300;
            saveData.Height = 420;
            saveData.X = 10;
            saveData.Y = 10;
            saveData.Folder = "";

            WindowData.Add(saveData);

            PDFViewItem NewWindow = new PDFViewItem(this, saveData);
            WindowReferences.Add(NewWindow);
            NewWindow.Show();
        }

        private void SaveDataToList(string Data)
        {
            if (Data != "")
            {
                string[] SplitData = Data.Split(',');

                SaveData saveData = new SaveData();

                if (SplitData.Length == 5)
                {
                    saveData.Width = Convert.ToDouble(SplitData[0]);
                    saveData.Height = Convert.ToDouble(SplitData[1]);
                    saveData.X = Convert.ToDouble(SplitData[2]);
                    saveData.Y = Convert.ToDouble(SplitData[3]);
                    saveData.Folder = SplitData[4];
                }

                WindowData.Add(saveData);
            }
        }

        public void UpdateListItem(SaveData Item)
        {
            WindowData[WindowData.IndexOf(Item)].Width = Item.Width;
            WindowData[WindowData.IndexOf(Item)].Height = Item.Height;
            WindowData[WindowData.IndexOf(Item)].X = Item.X;
            WindowData[WindowData.IndexOf(Item)].Y = Item.Y;
            WindowData[WindowData.IndexOf(Item)].Folder = Item.Folder;
        }

        public void RemoveListItem(SaveData Item)
        {
            WindowData.Remove(Item);
        }

        private void SaveDataToSettings()
        {
            string OutString = "";
            foreach (SaveData Data in WindowData)
            {
                OutString += Data.Width + "," + Data.Height + "," + Data.X + "," + Data.Y + "," + Data.Folder + ";";
            }
            Properties.Settings.Default.WindowData = OutString;
            Properties.Settings.Default.Save();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CloseAllWindows(null);
        }

        public void CloseAllWindows(PDFViewItem SenderWindow)
        {
            if (SenderWindow != null)
            {
                foreach (PDFViewItem Window in WindowReferences)
                {
                    if (Window != SenderWindow)
                    {
                        Window.ShuttingDown = true;
                        Window.Close();
                    }
                }

                SaveDataToSettings();

                this.Close();
            }
        }
    }
}
