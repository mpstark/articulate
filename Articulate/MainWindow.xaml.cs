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
using MahApps.Metro.Controls;
using System.ComponentModel;

namespace Articulate
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : MetroWindow, INotifyPropertyChanged
	{
		NotifyIcon ni;
		VoiceRecognizer recognizer;

		public event PropertyChangedEventHandler PropertyChanged;

		public MainWindow()
		{
			InitializeComponent();

			ni = new System.Windows.Forms.NotifyIcon();

			ni.Icon = new Icon("Main.ico");
			ni.Visible = true;
			ni.Text = "Articulate";
			ni.DoubleClick += (sender, args) =>
				{
					this.Show();
					this.WindowState = WindowState.Normal;
				};
		}


		#region MVVM Properties

		public static DependencyProperty ArticulateStateProperty = DependencyProperty.Register("State", typeof(string), typeof(MainWindow), new PropertyMetadata("LOADING..."));
		public string State
		{
			get { return (string)GetValue(ArticulateStateProperty); }
			set { SetValue(ArticulateStateProperty, value); }
		}

		public static DependencyProperty ArticulateReloadEnabledProperty = DependencyProperty.Register("ReloadEnabled", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));
		public bool ReloadEnabled
		{
			get { return (bool)GetValue(ArticulateReloadEnabledProperty); }
			set { SetValue(ArticulateReloadEnabledProperty, value); }
		}

		#endregion

		#region Window Events

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

			if (recognizer != null)
				recognizer.Dispose();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			recognizer = new VoiceRecognizer();

			// something happened with the setup of the VoiceRecognizer (no mic, etc.)
			if (!recognizer.IsSetup)
			{
				State = "FAILED";
				ReloadEnabled = true;
			}
			else
			{
				State = "LISTENING...";
				ReloadEnabled = false;
				ConfidenceMargin.Value = recognizer.ConfidenceMargin * 100;
			}
		}

		#endregion

		#region Window Command Buttons

		private void Settings_Click(object sender, RoutedEventArgs e)
		{
			SettingsFlyout.IsOpen = true;
		}

		private void About_Click(object sender, RoutedEventArgs e)
		{
			AboutFlyout.IsOpen = true;
		}

		#endregion

		#region Window Buttons

		private void ReloadRecognizer_Click(object sender, RoutedEventArgs e)
		{
			Window_Loaded(sender, e);
		}

		#endregion

		#region Settings
		
		private void ConfidenceMargin_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if(recognizer != null)
				recognizer.ConfidenceMargin = e.NewValue / 100;
		}

		#endregion
	}
}
