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

            ni.Icon = new Icon("Main.ico");
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
            
            recognizor.Dispose();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            recognizor = new VoiceRecognizer();

            // something happened with the setup of the VoiceRecognizer (no mic, etc.)
            if (!recognizor.IsSetup)
            {
                System.Windows.MessageBox.Show("There was a problem starting Articulate. Is there a valid default input device?");
                this.Close();
            }
        }
    }
}
