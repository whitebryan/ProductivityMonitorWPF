using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using Xceed.Wpf.Toolkit.Core;

namespace ProductivityMonitorWPF
{
	//Settings struct for saving/loading
	public struct Settings
	{
		public string productiveColor;
		public string unproductiveColor;
		public List<string> proccesses;
		public bool? shouldSaveTime;
		public int? savedTime;
	};


	public partial class MainWindow : Window
	{
		public List<string> trackedProccesses = new List<string>();
		public WindowGrabber windowGrabber = new WindowGrabber();

		//Settings
		public bool shouldSaveTime = false;
		public SolidColorBrush productiveColor = new SolidColorBrush(Colors.Blue);
		public SolidColorBrush unproductiveColor = new SolidColorBrush(Colors.Red);

		private int curOffset = 0;
		private bool timerPaused = false;
		private CustomStopWatch stopwatch = new CustomStopWatch(0);
		private CancellationTokenSource cancelToken = new CancellationTokenSource();
		

		public MainWindow()
		{
			Application.Current.MainWindow = this;
			InitializeComponent();
			updateButtons("Home");
			ThreadPool.QueueUserWorkItem(new WaitCallback(continuousCheck), cancelToken.Token);//new System.Threading.Thread(continuousCheck,);
			Background = new SolidColorBrush(Colors.Black);
			loadSettings();

			string imagePath = timerPaused ? "playButton.png" : "pauseButton.png";
			ImageBrush buttonBrush = new ImageBrush();
			Image image = new Image();
			image.Source = new BitmapImage(new Uri(imagePath, UriKind.Relative));
			buttonBrush.ImageSource = image.Source;
			PauseButton.Background = buttonBrush;
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
					HomeButton.IsEnabled = false;
					ProcessesButton.IsEnabled = true;
					SettingsButton.IsEnabled = true;
					break;
				case "Processes":
					HomeButton.IsEnabled = true;
					ProcessesButton.IsEnabled = false;
					SettingsButton.IsEnabled = true;
					break;
				case "Settings":
					HomeButton.IsEnabled = true;
					ProcessesButton.IsEnabled = true;
					SettingsButton.IsEnabled = false;
					break;
			}

			OnPropertyChanged("IsEnabled");
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
				//Skip if the timer should be paused currently and make sure the background is set to black
				if (timerPaused)
				{
					this.Dispatcher.Invoke(() =>
					{
						Background = new SolidColorBrush(Colors.Black);
					});
					
					if(stopwatch.IsRunning)
					{
						workingTimer(false);
					}

					continue;
				}


				//Grab current active window info
				WindowInfo info = windowGrabber.GetActivewindowInfo();

				string curWindow = info.WindowFullName;
				string[] windowNameSplit = curWindow.Split('-');
				curWindow = windowNameSplit.Length > 1 ? windowNameSplit[windowNameSplit.Length - 1] : curWindow;

				//Set cur window text
				this.Dispatcher.Invoke(() =>
				{
					CurWindowText.Text = info.WindowFullName;
				});

				//Check if its a tracked proccess or not and set color/timer accordingly 
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

				//Check if pause button should be shown
				bool pauseButtonShown = false;
				this.Dispatcher.Invoke(() =>
				{
					pauseButtonShown = PauseButton.IsEnabled;
				});

				if (trackedProccesses.Count > 0 && !pauseButtonShown)
				{
					this.Dispatcher.Invoke(() =>
					{
						PauseButton.IsEnabled = true;
						PauseButton.Visibility = Visibility.Visible;
						OnPropertyChanged("IsEnabled");
					});
				}
				else if(trackedProccesses.Count == 0 && pauseButtonShown)
				{
					this.Dispatcher.Invoke(() =>
					{
						PauseButton.IsEnabled = false;
						PauseButton.Visibility = Visibility.Hidden;
						OnPropertyChanged("IsEnabled");
					});
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
				TimeSpan timespan = TimeSpan.FromMilliseconds(stopwatch.ElapsedMiliseconds);
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
				shouldSaveTime = curSettings.shouldSaveTime ?? false;

				int offset = shouldSaveTime ? curSettings.savedTime ?? 0 : 0;
				stopwatch = new CustomStopWatch(offset);
			}
			else
			{
				stopwatch = new CustomStopWatch(0);
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
			newSettings.shouldSaveTime = shouldSaveTime;
			newSettings.savedTime = shouldSaveTime ? (int)(stopwatch.ElapsedMiliseconds) : curOffset;

			string jsonString = JsonConvert.SerializeObject(newSettings);
			File.WriteAllText(settingsPath, jsonString);
		}

		private void toggleTimer(object sender, RoutedEventArgs e)
		{
			timerPaused = !timerPaused;
			string imagePath = timerPaused ? "playButton.png" : "pauseButton.png";
			ImageBrush buttonBrush = new ImageBrush();
			Image image = new Image();
			image.Source = new BitmapImage(new Uri(imagePath, UriKind.Relative));
			buttonBrush.ImageSource = image.Source;
			PauseButton.Background = buttonBrush;
		}


		//Used to make sure changing a button IsEnabled updates
		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
