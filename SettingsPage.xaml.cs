using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace ProductivityMonitorWPF
{
	/// <summary>
	/// Interaction logic for SettingsPage.xaml
	/// </summary>
	public partial class SettingsPage : Page
	{
		public SettingsPage()
		{
			InitializeComponent();
			ProductivePicker.SelectedColor = ((MainWindow)Application.Current.MainWindow).productiveColor.Color;
			UnproducitvePicker.SelectedColor = ((MainWindow)Application.Current.MainWindow).unproductiveColor.Color;
			SaveTimeCheckbox.IsChecked = ((MainWindow)Application.Current.MainWindow).shouldSaveTime;
		}

		private void ClrPcker_Background_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
		{
			System.Windows.Media.Color newColor = ProductivePicker.SelectedColor ?? Colors.Black;
			((MainWindow)Application.Current.MainWindow).productiveColor = new SolidColorBrush(newColor);
        }

		private void ClrPcker_Background_Copy_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
		{
			System.Windows.Media.Color newColor = UnproducitvePicker.SelectedColor ?? Colors.Black;
			((MainWindow)Application.Current.MainWindow).unproductiveColor = new SolidColorBrush(newColor);
		}

		private void CheckBox_Checked(object sender, RoutedEventArgs e)
		{
			((MainWindow)Application.Current.MainWindow).shouldSaveTime = SaveTimeCheckbox.IsChecked ?? false;
		}
    }
}
