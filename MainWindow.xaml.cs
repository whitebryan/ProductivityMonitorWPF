using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json;


namespace ProductivityMonitorWPF
{
	//Settings struct for saving/loading
	public struct Settings
	{
		public string productiveColor;
		public string unproductiveColor;
		public List<string> proccesses;
	};


	public partial class MainWindow : Window
	{
		public List<string> trackedProccesses = new List<string>();
		public WindowGrabber windowGrabber = new WindowGrabber();
		public bool homeButtonEnabled = true;
		public bool processesButtonEnabled = true;
		public bool settingsButtonEnabled = true;

		public SolidColorBrush productiveColor = new SolidColorBrush(Colors.Blue);
		public SolidColorBrush unproductiveColor = new SolidColorBrush(Colors.Red);


		private Stopwatch stopwatch = new Stopwatch();
		private CancellationTokenSource cancelToken = new CancellationTokenSource();
		

		public MainWindow()
		{
			Application.Current.MainWindow = this;
			InitializeComponent();
			updateButtons("Home");
			ThreadPool.QueueUserWorkItem(new WaitCallback(continuousCheck), cancelToken.Token);//new System.Threading.Thread(continuousCheck,);
			Background = new SolidColorBrush(Colors.Black);
			loadSettings();
		}

		public void ChangePage(string pageToGoTo)
		{
			Page page = null;
			switch (pageToGoTo)
			{
				//Create page to change to and show/hide timer for simplicity
				case "Home":
					page = new Page();
					TimerCanvas.Visibility = Visibility.Visible;
					break;
				case "Processes":
					page = new ProcessesPage();
					TimerCanvas.Visibility = Visibility.Hidden;
					break;
				case "Settings":
					page = new SettingsPage();
					TimerCanvas.Visibility = Visibility.Hidden;
					break;
			}

			if(page != null)
			{
				PageFrame.Navigate(page);
				updateButtons(pageToGoTo);
			}
		}

		//Activate/Deactive buttons while on their page
		private void updateButtons(string newPage)
		{
			switch (newPage)
			{
				case "Home":
					homeButtonEnabled = false;
					processesButtonEnabled = true;
					settingsButtonEnabled = true;
					break;
				case "Processes":
					homeButtonEnabled = true;
					processesButtonEnabled = false;
					settingsButtonEnabled = true;
					break;
				case "Settings":
					homeButtonEnabled = true;
					processesButtonEnabled = true;
					settingsButtonEnabled = false;
					break;
			}
		}

		private void Button_Clicked(object sender, RoutedEventArgs e)
		{
			ChangePage(((Button)sender).Tag.ToString() ?? "Null");
		}

		//Updating via the timer always
		private void continuousCheck(object? obj)
		{
			if(obj == null)
			{
				return;
			}

			while (true)
			{
				WindowInfo info = windowGrabber.GetActivewindowInfo();

				string curWindow = info.WindowFullName;
				string[] windowNameSplit = curWindow.Split('-');
				curWindow = windowNameSplit.Length > 1 ? windowNameSplit[windowNameSplit.Length - 1] : curWindow;

				this.Dispatcher.Invoke(() =>
				{
					CurWindowText.Text = info.WindowFullName;
				});

				if (trackedProccesses.Count > 0 && !trackedProccesses.Contains(info.ProccessName))
				{
					this.Dispatcher.Invoke(() =>
					{
						Background = unproductiveColor;
					});
					workingTimer(false);
				}
				else if (trackedProccesses.Count > 0)
				{
					this.Dispatcher.Invoke(() =>
					{
						Background = productiveColor;
					});
					workingTimer(true);
				}
				else
				{
					this.Dispatcher.Invoke(() =>
					{
						Background = new SolidColorBrush(Colors.Black);
					});
					workingTimer(false);
				}
			}
		}

		//Changing timer text and pausing if needed
		private void workingTimer(bool shouldRun)
		{
			if (shouldRun)
			{
				if (!stopwatch.IsRunning)
				{
					this.Dispatcher.Invoke(() =>
					{
						stopwatch.Start();
					});
				}
				TimeSpan timespan = TimeSpan.FromSeconds(stopwatch.Elapsed.TotalSeconds);
				this.Dispatcher.Invoke(() =>
				{
					ElapsedTime.Content = $"{timespan:hh\\:mm\\:ss}";
				});
			}
			else
			{
				this.Dispatcher.Invoke(() =>
				{
					stopwatch.Stop();
				});
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			saveSettings();
			cancelToken.Cancel();
		}

		public void loadSettings()
		{
			var folder = Environment.SpecialFolder.LocalApplicationData;
			var path = Environment.GetFolderPath(folder);
			string settingsPath = System.IO.Path.Combine(path, "productivityMonitorSettings.txt");
			if(File.Exists(settingsPath))
			{
				var curSettings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingsPath));
				productiveColor = ((SolidColorBrush)new BrushConverter().ConvertFrom(curSettings.productiveColor));
				unproductiveColor = (SolidColorBrush)new BrushConverter().ConvertFrom(curSettings.unproductiveColor);
				trackedProccesses = curSettings.proccesses;
			}
		}

		public void saveSettings()
		{
			var folder = Environment.SpecialFolder.LocalApplicationData;
			var path = Environment.GetFolderPath(folder);
			string settingsPath = System.IO.Path.Combine(path, "productivityMonitorSettings.txt");

			Settings newSettings = new Settings();
			newSettings.productiveColor = productiveColor.ToString();
			newSettings.unproductiveColor = unproductiveColor.ToString();
			newSettings.proccesses = trackedProccesses;

			string jsonString = JsonConvert.SerializeObject(newSettings);
			File.WriteAllText(settingsPath, jsonString);
		}
	}
}
