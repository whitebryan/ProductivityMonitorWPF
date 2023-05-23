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
	/// Interaction logic for ProcessesPage.xaml
	/// </summary>
	public partial class ProcessesPage : Page
	{
		MainWindow mainWin;

		public ProcessesPage()
		{
			InitializeComponent();
			mainWin = (MainWindow)Application.Current.MainWindow;
			initalizeProccesses();
		}

		//Reading all current proccesses into the list
		private void initalizeProccesses()
		{
			AddedProccesses.Items.Clear();
			ProccessSelectBox.Items.Clear();

			var allProccesses = Process.GetProcesses();

			foreach (Process p in allProccesses)
			{
				if (!ProccessSelectBox.Items.Contains(p.ProcessName))
				{
					ProccessSelectBox.Items.Add(p.ProcessName);
				}
			}

			foreach (string prosName in mainWin.trackedProccesses)
			{
				AddedProccesses.Items.Add(prosName);
			}
		}

		//Add process to be tracked
		private void AddProccess_Click(object sender, RoutedEventArgs e)
		{
			AddedProccesses.Items.Add(ProccessSelectBox.SelectedItem);
			if (!mainWin.trackedProccesses.Contains(ProccessSelectBox.SelectedItem))
			{
				mainWin.trackedProccesses.Add(ProccessSelectBox.SelectedItem.ToString());
			}
		}

		//Remove process from being tracked
		private void RemoveAddedButton_Click(object sender, RoutedEventArgs e)
		{
			mainWin.trackedProccesses.Remove(AddedProccesses.SelectedItem.ToString());
			AddedProccesses.Items.Remove(AddedProccesses.SelectedItem);
		}
	}
}
