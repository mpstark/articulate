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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

namespace Articulate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        NotifyIcon ni;
        VoiceRecognizer recognizor;

        public MainWindow()
        {
            InitializeComponent();

            ni = new System.Windows.Forms.NotifyIcon();

            ni.Icon = Articulate.Properties.Resources.Main;
            ni.Visible = true;
            ni.Text = "Articulate for Arma 3";
            ni.DoubleClick +=
                delegate(object sender, EventArgs args)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            ni.Visible = false;
            ni.Dispose();
            ni = null;

            if (recognizor != null)
                recognizor.Dispose();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            recognizor = new VoiceRecognizer();

            // something happened with the setup of the VoiceRecognizer (no mic, etc.)
            if (!recognizor.IsSetup)
            {
                System.Windows.MessageBox.Show("Is there a valid default input device?\n\n" + recognizor.SetupError);
                this.Close();
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
