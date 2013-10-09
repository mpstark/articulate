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
		public AdvancedSettings(Settings settings)
		{
			Settings = settings;

			InitializeComponent();
		}

		public Settings Settings
		{ get; private set; }

		void Window_Loaded(object sender, RoutedEventArgs e)
		{

		}

		void Window_Closing(object sender, RoutedEventArgs e)
		{

		}
	}
}
