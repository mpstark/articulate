using MahApps.Metro.Controls;
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

namespace Articulate
{
	/// <summary>
	/// Interaction logic for AdvancedSettings.xaml
	/// </summary>
	public partial class AdvancedSettings : MetroWindow
	{
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
			
		}

		private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Logic.Configuration.Save();
		}
	}
}
