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
using SierraLib.GlobalHooks;

namespace Articulate
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : MetroWindow, IDisposable
	{
		NotifyIcon ni;
		VoiceRecognizer recognizer;

		Settings settings;

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


			settings = Articulate.Settings.Load();

			#region Rx Event Handlers

			Observable.FromEventPattern<RoutedPropertyChangedEventArgs<double>>(ConfidenceMargin, "ValueChanged")
				.Throttle(TimeSpan.FromMilliseconds(500)).Subscribe(args =>
				{
					if (recognizer != null)
						recognizer.ConfidenceMargin = (int)args.EventArgs.NewValue;

					settings.ConfidenceMargin = (int)args.EventArgs.NewValue;
					Task.Factory.StartNew(() => settings.Save());
				});

			#endregion
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

			PTTKey.Content = settings.PTTKey.ToString();
			PushToIgnore.IsChecked = settings.PushToIgnore;

			recognizer = new VoiceRecognizer();

			// something happened with the setup of the VoiceRecognizer (no mic, etc.)
			if (!recognizer.IsSetup)
			{
				State = "FAILED";
				ErrorMessage = recognizer.SetupError;
				ErrorFlyout.IsOpen = true;
			}
			else
			{
				State = "LISTENING...";
				ConfidenceMargin.Value = settings.ConfidenceMargin;
				Enabled = settings.PushToIgnore || settings.PTTKey == System.Windows.Forms.Keys.None;
			}
			
			HookManager.KeyDown += HookManager_KeyDown;
			HookManager.KeyUp += HookManager_KeyUp;
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			if (ni != null)
				ni.Visible = false;
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

		#region PTT
				
		public bool Enabled
		{
			get { return recognizer.Enabled; }
			set
			{
				recognizer.Enabled = value;
				if (value)
					State = "LISTENING...";
				else
					State = "WAITING...";
			}
		}

		void HookManager_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if(settings.PTTKey == System.Windows.Forms.Keys.None || e.KeyCode != settings.PTTKey) return;

			Enabled = settings.PushToIgnore;
		}

		void HookManager_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (ListeningForNewPTT)
			{
				ListeningForNewPTT = false;

				if (e.KeyCode == System.Windows.Forms.Keys.Escape)				
					settings.PTTKey = System.Windows.Forms.Keys.None;
				else
					settings.PTTKey = e.KeyCode;

				PTTKey.Content = e.KeyCode.ToString();

				Enabled = settings.PushToIgnore || settings.PTTKey == System.Windows.Forms.Keys.None;
				Task.Factory.StartNew(() => settings.Save());

				return;
			}

			if (settings.PTTKey == System.Windows.Forms.Keys.None || e.KeyCode != settings.PTTKey) return;

			Enabled = !settings.PushToIgnore;
		}

		#endregion

		#region Settings

		private bool ListeningForNewPTT = false;

		private void PTTKey_Click(object sender, RoutedEventArgs e)
		{
			ListeningForNewPTT = true;
		}

		private void PushToIgnore_Checked(object sender, RoutedEventArgs e)
		{
			settings.PushToIgnore = PushToIgnore.IsChecked ?? false;
			Enabled = settings.PushToIgnore || settings.PTTKey == System.Windows.Forms.Keys.None;
			Task.Factory.StartNew(() => settings.Save());
		}

		#endregion

		public void Dispose()
		{
			if (recognizer != null)
			{
				recognizer.Dispose();
				recognizer = null;
			}

			if (ni != null)
			{
				ni.Dispose();
				ni = null;
			}
		}
	}
}
