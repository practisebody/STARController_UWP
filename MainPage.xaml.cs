using LCY;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace STARController
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        const int ControlWidth = 200;
        const int ControlHeight = 40;
        const int ControlMargin = 5;


        USocketClient client;

        public MainPage()
        {
            this.InitializeComponent();

            Window.Current.CoreWindow.SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(CoreWindow w, WindowSizeChangedEventArgs e)
        {
            svControl.Height = e.Size.Height - 80;
            svLog.Height = e.Size.Height - 80;
            svStatus.Height = e.Size.Height - 80;
        }

        #region socket

        private enum Statuses
        {
            CONTROL,
            LOG,
            STATUS,
        };
        private Statuses Status { get; set; } = Statuses.CONTROL;

        private void OnConnectClick(object sender, RoutedEventArgs e)
        {
            btnConnect.Visibility = Visibility.Collapsed;
            client = new USocketClient(tbIP.Text, 12345);
            client.Persistent = true;
            client.MessageReceived += OnMessage;
            client.Disconnected += OnDisconnect;
            client.Connect();
            btnDisonnect.Visibility = Visibility.Visible;
        }

        private void OnDisconnectClick(object sender, RoutedEventArgs e)
        {
            btnDisonnect.Visibility = Visibility.Collapsed;
            btnConnect.Visibility = Visibility.Visible;
        }

        private void OnMessage(string s)
        {
            switch (s)
            {
                case "CONTROL":
                    Status = Statuses.CONTROL;
                    spControl.Children.Clear();
                    return;
                case "LOG":
                    Status = Statuses.LOG;
                    LogString = "";
                    return;
                case "STATUS":
                    Status = Statuses.STATUS;
                    StatusString = "";
                    return;
            }
            switch (Status)
            {
                case Statuses.CONTROL:
                    OnInitMessage(s);
                    break;
                case Statuses.LOG:
                    OnLogMessage(s);
                    break;
                case Statuses.STATUS:
                    OnStatusMessage(s);
                    break;
            }
        }

        private void OnInitMessage(string s)
        {
            if (s == "END")
            {
            }
            else
            {
                string[] options = s.Split(';');
                Array.Sort(options);
                foreach (string line in options)
                {
                    string[] pair = line.Split(':');
                    if (pair.Length == 2)
                    {
                        string name = pair[0];
                        string value = pair[1];
                        if (Boolean.TryParse(value, out bool b))
                        {
                            AddBool(name, value);
                        }
                        else
                        {
                            AddString(name, value);
                        }
                    }
                }
                for (int i = 0; i < 10; ++i)
                    AddBool("", "");
            }
        }

        private string LogString = "";

        private void OnLogMessage(string s)
        {
            if (s == "END")
                tbLogText.Text = LogString;
            else
                LogString += s + "\n";
        }

        private string StatusString = "";

        private void OnStatusMessage(string s)
        {
            if (s == "END")
                tbStatusText.Text = StatusString;
            else
                StatusString += s + "\n";
        }

        private void OnDisconnect()
        {
            foreach (UIElement line in spControl.Children)
            {
                StackPanel s = (StackPanel)line;
                foreach (UIElement a in s.Children)
                {
                    if (a.GetType() == typeof(Button))
                        ((Button)a).IsEnabled = false;
                }
            }
        }

        #endregion

        #region GUI

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            string name = (string)((Button)sender).Content;
            string value = "";
            if (FindName(name + "TextBlock") != null)
            {
                TextBlock text = (TextBlock)FindName(name + "TextBlock");
                bool v = !Boolean.Parse(text.Text);
                value = text.Text = v.ToString();
            }
            else if (FindName(name + "TextBox") != null)
            {
                TextBox text = (TextBox)FindName(name + "TextBox");
                value = text.Text;
            }
            string send;
            if (value == "")
                send = String.Format("{{{0}:\"null\"}}", name);
            else
                send = String.Format("{{{0}:\"{1}\"}}", name, value);
            client.Send(send);
        }

        private StackPanel NewLine()
        {
            StackPanel line = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
            };
            spControl.Children.Add(line);
            return line;
        }

        private Button NewButton(string name, int width = ControlWidth, int height = ControlHeight, int margin = ControlMargin)
        {
            Button button = new Button()
            {
                Name = name,
                Content = name,
                Margin = new Thickness(margin),
                Width = width,
                Height = height,
            };
            button.Click += OnButtonClick;
            return button;
        }

        private TextBlock NewTextBlock(string name, string value, int width = ControlWidth, int height = ControlHeight, int margin = ControlMargin)
        {
            TextBlock textblock = new TextBlock()
            {
                Name = name + "TextBlock",
                Text = value,
                Margin = new Thickness(margin),
                Width = width,
                Height = height,
            };
            return textblock;
        }

        private TextBox NewTextBox(string name, string value, int width = ControlWidth, int height = ControlHeight, int margin = ControlMargin)
        {
            TextBox textbox = new TextBox()
            {
                Name = name + "TextBox",
                Text = value,
                Margin = new Thickness(margin),
                Width = width,
                Height = height,
            };
            return textbox;
        }

        private void AddBool(string name, string value)
        {
            StackPanel line = NewLine();
            line.Children.Add(NewButton(name));
            line.Children.Add(NewTextBlock(name, value));
        }

        private void AddString(string name, string value)
        {
            StackPanel line = NewLine();
            line.Children.Add(NewButton(name));
            line.Children.Add(NewTextBox(name, value));
        }

        #endregion
    }
}
