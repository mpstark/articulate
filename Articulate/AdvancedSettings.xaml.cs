using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
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

namespace Articulate
{
	/// <summary>
	/// Interaction logic for AdvancedSettings.xaml
	/// </summary>
	public partial class AdvancedSettings : MetroWindow
	{
		Stack<IDisposable> RxSubscriptions = new Stack<IDisposable>();

		public AdvancedSettings(Core logic)
		{
			Logic = logic;
			InitializeComponent();
		}

		public Core Logic
		{
			get;
			private set;
		}

		void OnLoaded(object sender, RoutedEventArgs e)
		{
			PTTKeys.ItemsSource = Logic.Configuration.KeyBinds;
			ListenMode.SelectedIndex = (int)Logic.Configuration.Mode;
			EndCommandPause.Value = Logic.Configuration.EndCommandPause;
			EndCommandPauseNumber.Content = Logic.Configuration.EndCommandPause;

			var CommandPauseEvent = Observable.FromEventPattern<RoutedPropertyChangedEventArgs<double>>(EndCommandPause, "ValueChanged");

			RxSubscriptions.Push(CommandPauseEvent.Skip(1).Distinct().Sample(TimeSpan.FromMilliseconds(50)).ObserveOnDispatcher().Subscribe(args =>
			{
				EndCommandPauseNumber.Content = Math.Floor(args.EventArgs.NewValue).ToString();
			}));

			RxSubscriptions.Push(CommandPauseEvent.Skip(1).Distinct().Sample(TimeSpan.FromMilliseconds(500)).Subscribe(args =>
			{
				Logic.Configuration.EndCommandPause = (int)args.EventArgs.NewValue;

				if (Logic != null)
					Logic.Recognizer.EndSilenceTimeout = (int)args.EventArgs.NewValue;
			}));

			Logic.Keybinder.MappingCompleted += OnMappingCompleted;
		}

		private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Logic.Configuration.Save();

			while (RxSubscriptions.Any())
				RxSubscriptions.Pop().Dispose();

			Logic.Keybinder.MappingCompleted -= OnMappingCompleted;
		}


		private void PTTKey_Click(object sender, RoutedEventArgs e)
		{
			if (Logic != null)
			{
				Logic.Keybinder.BeginMapping();
				PTTKey.IsEnabled = false;
				PTTKey.Content = "Press Keys...";
			}
		}

		private void PTTKeys_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (PTTKeys.SelectedItem != null)
			{
				Logic.Configuration.KeyBinds.Remove((CompoundKeyBind)PTTKeys.SelectedItem);

				if (!Logic.Configuration.KeyBinds.Any())
				{
					Logic.Configuration.Mode = Articulate.ListenMode.Continuous;
					ListenMode.SelectedIndex = (int)Articulate.ListenMode.Continuous;
				}
			}
		}
		
		private void ListenMode_Selected(object sender, RoutedEventArgs e)
		{
			Logic.Configuration.Mode = (Articulate.ListenMode)(ListenMode.SelectedIndex);
		}

		void OnMappingCompleted(object sender, IEnumerable<CompoundKeyBind> e)
		{
			PTTKey.IsEnabled = true;
			PTTKey.Content = "Add Key Bind";
		}
	}
}
