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
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

namespace Articulate
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : MetroWindow, IDisposable
	{
		NotifyIcon ni;
		VoiceRecognizer recognizer;
		
		public MainWindow()
		{
			InitializeComponent();

			ni = new System.Windows.Forms.NotifyIcon();

			ni.Icon = Properties.Resources.Main;
			ni.Visible = true;
			ni.Text = "Articulate";
			ni.DoubleClick += (sender, args) =>
				{
					this.Show();
					this.WindowState = WindowState.Normal;
				};

            Observable.FromEventPattern<RoutedPropertyChangedEventArgs<double>>(ConfidenceMargin, "ValueChanged")
			    .Throttle(TimeSpan.FromMilliseconds(500)).Subscribe(args =>
				{
					if (recognizer != null)
						recognizer.ConfidenceMargin = (int)args.EventArgs.NewValue;
				});
		}


		#region MVVM Properties

		public static DependencyProperty ArticulateStateProperty = DependencyProperty.Register("State", typeof(string), typeof(MainWindow), new PropertyMetadata("LOADING..."));
		public string State
		{
			get { return (string)GetValue(ArticulateStateProperty); }
			set { SetValue(ArticulateStateProperty, value); }
		}

		public static DependencyProperty ArticulateErrorMessageProperty = DependencyProperty.Register("ErrorMessage", typeof(string), typeof(MainWindow), new PropertyMetadata(""));
		public string ErrorMessage
		{
			get { return (string)GetValue(ArticulateErrorMessageProperty); }
			set { SetValue(ArticulateErrorMessageProperty, value); }
		}

		#endregion

		#region Window Events

		protected override void OnStateChanged(EventArgs e)
		{
			if (WindowState == System.Windows.WindowState.Minimized)
				this.Hide();

			base.OnStateChanged(e);
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			recognizer = new VoiceRecognizer();

			// something happened with the setup of the VoiceRecognizer (no mic, etc.)
			if (recognizer.State == VoiceRecognizer.VoiceRecognizerState.Error)
			{
				State = "FAILED";
				ErrorMessage = recognizer.SetupError;
				ErrorFlyout.IsOpen = true;
			}
			else
			{
				State = "LISTENING...";
				ConfidenceMargin.Value = 90;
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

		private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
			e.Handled = true;
		}

		#endregion

		#region Settings
		
		#endregion

        #region IDispose Implementation
        public void Dispose()
		{
			if (recognizer != null)
			{
				recognizer.Dispose();
				recognizer = null;
			}

			if (ni != null)
			{
				ni.Visible = false;
				ni.Dispose();
				ni = null;
			}
        }
        #endregion
    }
}
