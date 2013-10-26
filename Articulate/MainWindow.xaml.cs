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
using System.Reactive.Concurrency;
using System.Threading;
using SierraLib.Translation;
using System.IO;
using System.Globalization;

namespace Articulate
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : MetroWindow, IDisposable
	{
		NotifyIcon ni;		
		Stack<IDisposable> RxSubscriptions = new Stack<IDisposable>();

		AutoResetEvent PushToTalkRelease;

		public MainWindow()
		{
			InitializeComponent();

			PushToTalkRelease = new AutoResetEvent(false);
			Logic = new Core();

			TranslationManager.Instance.DefaultLanguage = new CultureInfo("en");
			TranslationManager.Instance.CurrentLanguage = new CultureInfo(Logic.Configuration.Language ?? "en");

			ni = new System.Windows.Forms.NotifyIcon();

			ni.Icon = Properties.Resources.Main;
			ni.Visible = true;
			ni.Text = "Articulate";
			ni.DoubleClick += (sender, args) =>
				{
					this.Show();
					this.WindowState = WindowState.Normal;
				};
						
			Logic.Keybinder.KeysPressed += OnKeysPressed;
			Logic.Keybinder.KeysReleased += OnKeysReleased;


			#region Rx Event Handlers

			var ConfidenceEvent = Observable.FromEventPattern<RoutedPropertyChangedEventArgs<double>>(ConfidenceMargin, "ValueChanged");

			RxSubscriptions.Push(ConfidenceEvent.Skip(1).Distinct().Sample(TimeSpan.FromMilliseconds(500)).Subscribe(args =>
			{
				Logic.Configuration.ConfidenceMargin = (int)args.EventArgs.NewValue;

				if (Logic != null)
					Logic.Recognizer.ConfidenceMargin = (int)args.EventArgs.NewValue;
			}));

			RxSubscriptions.Push(ConfidenceEvent.Skip(1).Distinct().Sample(TimeSpan.FromMilliseconds(50)).ObserveOnDispatcher().Subscribe(args =>
			{
				ConfidenceMarginNumber.Content = Math.Floor(args.EventArgs.NewValue).ToString();
			}));
			
			RxSubscriptions.Push(SettingsFlyout.ToObservable<bool>(Flyout.IsOpenProperty).Skip(1).Distinct().ObserveOn(ThreadPoolScheduler.Instance).Subscribe(args =>
			{
				if (!args) Logic.Configuration.Save();
			}));

			RxSubscriptions.Push(LanguagesFlyout.ToObservable<bool>(Flyout.IsOpenProperty).Skip(1).Distinct().ObserveOn(ThreadPoolScheduler.Instance).Subscribe(args =>
			{
				if (!args) Logic.Configuration.Save();
			}));
			#endregion
		}

		#region Public Properties

		public Core Logic
		{ get; private set; }

		#endregion

		#region MVVM Properties

		public static DependencyProperty ArticulateStateProperty = DependencyProperty.Register("State", typeof(string), typeof(MainWindow), new PropertyMetadata("LOADING..."));
		public string State
		{
			get { return (string)GetValue(ArticulateStateProperty); }
			set { Dispatcher.Invoke(() => SetValue(ArticulateStateProperty, value)); }
		}

		public static DependencyProperty ArticulateErrorMessageProperty = DependencyProperty.Register("ErrorMessage", typeof(string), typeof(MainWindow), new PropertyMetadata(""));
		public string ErrorMessage
		{
			get { return (string)GetValue(ArticulateErrorMessageProperty); }
			set { Dispatcher.Invoke(() => SetValue(ArticulateErrorMessageProperty, value)); }
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
			// Load translations			
			try
			{
				using (var enStream = new MemoryStream(Properties.Resources.en))
					TranslationManager.Instance.Translations.Add(new FileBasedTranslation(CultureInfo.GetCultureInfo("en"), enStream));

				foreach (var file in new DirectoryInfo(Environment.CurrentDirectory).GetFiles("*.slt"))
				{
					using (var fs = file.OpenRead())
						TranslationManager.Instance.Translations.Add(new FileBasedTranslation(CultureInfo.GetCultureInfo(System.IO.Path.GetFileNameWithoutExtension(file.Name)), fs));
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);

				ErrorFlyout.IsOpen = true;
				ErrorMessage = ex.Message;

				// Failed to load a translation file
#if !DEBUG
				App.HandleError(ex);
#endif
			}

			ListenMode.SelectedIndex = (int)Logic.Configuration.Mode;

			ConfidenceMargin.Value = Logic.Configuration.ConfidenceMargin;
			ConfidenceMarginNumber.Content = Logic.Configuration.ConfidenceMargin;
			

			if (!Logic.Configuration.Applications.Any())
				Logic.Configuration.Applications.AddRange(new[] {
					"arma",
					"arma2",
					"arma2oa",
					"takeonh",
					"arma3"
				});

			LanguageList.ItemsSource = TranslationManager.Instance.Translations.Select(x => x.Culture.DisplayName);
			LanguageList.SelectedItem = TranslationManager.Instance.CurrentLanguage.DisplayName;

			Task.Factory.StartNew(LoadRecognizer);
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			if (ni != null)
				ni.Visible = false;

			while (RxSubscriptions.Any())
				RxSubscriptions.Pop().Dispose();
		}

		#endregion

		#region Recognition

		private void LoadRecognizer()
		{
			// something happened with the setup of the VoiceRecognizer (no mic, etc.)
			if (Logic.Recognizer.State == VoiceRecognizer.VoiceRecognizerState.Error)
			{
				Dispatcher.Invoke(() =>
				{
					State = "state_error".Translate("FAILED");
					ErrorMessage = Logic.Recognizer.SetupError;
					ErrorFlyout.IsOpen = true;
				});
			}
			else
			{
				Logic.Recognizer.ConfidenceMargin = Logic.Configuration.ConfidenceMargin;
				Logic.Recognizer.EndSilenceTimeout = Logic.Configuration.EndCommandPause;

				Logic.Recognizer.MonitoredExecutables = Logic.Configuration.Applications;

				Logic.Recognizer.CommandAccepted += recognizer_CommandAccepted;
				Logic.Recognizer.CommandRejected += recognizer_CommandRejected;


				Enabled = Logic.Configuration.Mode == Articulate.ListenMode.Continuous || Logic.Configuration.Mode == Articulate.ListenMode.PushToIgnore;
			}
		}

		void recognizer_CommandAccepted(object sender, CommandDetectedEventArgs e)
		{
			Trace.WriteLine("Accepted command: " + e.Phrase + " " + e.Confidence);

			Dispatcher.Invoke(() => LastCommand.Content = e.Phrase);

			if (Logic.Configuration.Mode == Articulate.ListenMode.PushToArm) Enabled = false;
		}

		void recognizer_CommandRejected(object sender, CommandDetectedEventArgs e)
		{
			Trace.WriteLine("Rejected command: " + e.Phrase + " " + e.Confidence);

			Dispatcher.Invoke(() => LastCommand.Content = "state_recognition_failed".Translate("What was that?"));

			// TODO: Decide whether or not Push To Arm should keep trying until it gets a match
			if (Logic.Configuration.Mode == Articulate.ListenMode.PushToArm) Enabled = false;
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
			ErrorFlyout.IsOpen = false;
			LoadRecognizer();
		}

		private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
			e.Handled = true;
		}

		private void AdvancedSettings_Click(object sender, RoutedEventArgs e)
		{
			new AdvancedSettings(Logic).ShowDialog();

			Enabled = Logic.Configuration.Mode == Articulate.ListenMode.Continuous || Logic.Configuration.Mode == Articulate.ListenMode.PushToIgnore;
			ListenMode.SelectedIndex = (int)Logic.Configuration.Mode;
		}

		#endregion

		#region PTT

		public bool Enabled
		{
			get { return Logic.Recognizer.State == VoiceRecognizer.VoiceRecognizerState.Listening || Logic.Recognizer.State == VoiceRecognizer.VoiceRecognizerState.ListeningOnce; }
			set
			{
				if (Logic.Recognizer == null) return;

				if (value)
				{
					if (Logic.Configuration.Mode == Articulate.ListenMode.PushToArm) Logic.Recognizer.ListenOnce();
					else Logic.Recognizer.StartListening();

					Dispatcher.Invoke(() => State = "state_online".Translate("LISTENING"));
				}
				else
				{
					Logic.Recognizer.StopListening();
					Dispatcher.Invoke(() => State = "state_offline".Translate("OFFLINE"));
				}
			}
		}
		
		void OnKeysPressed(object sender, CompoundKeyBind e)
		{
			if (Logic.Configuration.Mode == Articulate.ListenMode.Continuous) return;

			PushToTalkRelease.Set();

			if (Enabled && Logic.Configuration.Mode == Articulate.ListenMode.PushToArm)
				Enabled = false;
			else
				Enabled = Logic.Configuration.Mode == Articulate.ListenMode.PushToTalk || Logic.Configuration.Mode == Articulate.ListenMode.PushToArm;
		}
	
		void OnKeysReleased(object sender, CompoundKeyBind e)
		{
			if (Logic.Configuration.Mode == Articulate.ListenMode.PushToArm) return; // Don't disable if we're armed
			if (Logic.Configuration.Mode == Articulate.ListenMode.Continuous) return;

			PushToTalkRelease.Reset();

			ThreadPool.RegisterWaitForSingleObject(PushToTalkRelease, (state, completed) =>
			{
				if (completed)
					Dispatcher.Invoke(() =>
					{
						Enabled = Logic.Configuration.Mode == Articulate.ListenMode.Continuous || Logic.Configuration.Mode == Articulate.ListenMode.PushToIgnore;
					});
			}, null, 500, true);
		}

		#endregion

		#region Settings 

		private void ListenMode_Selected(object sender, RoutedEventArgs e)
		{
			Logic.Configuration.Mode = (Articulate.ListenMode)(ListenMode.SelectedIndex);

			Enabled = Logic.Configuration.Mode == Articulate.ListenMode.Continuous || Logic.Configuration.Mode == Articulate.ListenMode.PushToIgnore;
		}

		private void Languages_Click(object sender, RoutedEventArgs e)
		{
			LanguagesFlyout.IsOpen = true;
		}

		private void Languages_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var translation = TranslationManager.Instance.Translations.Find(x => x.Culture.DisplayName == LanguageList.SelectedItem.ToString());

			Logic.Configuration.Language = (TranslationManager.Instance.CurrentLanguage = translation.Culture).Name;
		}

		#endregion

		#region IDispose Implementation
		public void Dispose()
		{
			if (ni != null)
			{
				ni.Dispose();
				ni = null;
			}

			if (Logic != null)
			{
				Logic.Dispose();
				Logic = null;
			}
		}
		#endregion

	}
}
