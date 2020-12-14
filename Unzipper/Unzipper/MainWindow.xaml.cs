using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

using Ookii.Dialogs.Wpf;



namespace Unzipper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string rootPath = string.Empty;
        private readonly Stopwatch stopwatch = new Stopwatch();
        private long elapsed;
        private int maxValue;
        private long totalElapsed = 0;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

        }


        private string text;

        public string Text
        {
            get => text;
            set
            {
                text = value;
                OnPropertyChanged("Text");
            }
        }

        private string progress;

        public string Progress
        {
            get => progress;
            set
            {
                progress = value;
                OnPropertyChanged("Progress");
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        private async void BtnBrowse_Click(object sender, RoutedEventArgs e) //the only time you may use async void
        {
            try
            {
                var dialog = new VistaFolderBrowserDialog();
                dialog.Description = "Select parent folder with zip files";
                dialog.UseDescriptionForTitle = true;

                if ((bool)dialog.ShowDialog())
                {
                    txtBrowse.Text = dialog.SelectedPath;
                    rootPath = dialog.SelectedPath;
                    await LoadFiles();
                }
            }
            catch (Exception exception)
            {
                Text = $"{DateTime.Now} Exception thrown {exception} \n";

            }
        }

        private async Task LoadFiles()
        {
            try
            {
                Text = $"{DateTime.Now} loading files, this may take some time ... \n";
                var zipFiles = new List<string>();
                await Task.Run(() =>
                {
                    zipFiles = Directory.GetFiles(rootPath, "*.zip", SearchOption.AllDirectories).ToList();
                });

                Text = $"{DateTime.Now} Found {zipFiles.Count} files \n";
                Prg_progress.Maximum = zipFiles.Count;
                Prg_progress.Minimum = 0;

                maxValue = zipFiles.Count;

                Text = $"{DateTime.Now} starting extracting... \n";
                Extract(zipFiles);
            }
            catch (Exception exception)
            {
                 Text = $"{DateTime.Now} Exception thrown {exception} \n";
            }
        }

        private void Extract(List<string> zipFiles)
        {
            try
            {
                foreach (var file in zipFiles)
                {
                    stopwatch.Start();
                    //extract same place create folder with same name
                    string destinationPath = file.Remove(file.Length - 4, 4); // the same path extract zip in a folder with same name
                    if (!Directory.Exists(destinationPath))
                    {
                        ZipFile.ExtractToDirectory(file, destinationPath);
                        elapsed = stopwatch.ElapsedMilliseconds;
                        //update progressbar
                        Prg_progress.Value++;
                        stopwatch.Reset();
                    }
                    else
                    {
                        txtReport.Text += $"{DateTime.Now} Destination already exists for file {file} \n";
                        Prg_progress.Value++;
                    }
                    // only extract if the directory does not exists. if the directory exists it will throw an io exception

                }
            }
            catch (Exception exception)
            {
                Text = $"{DateTime.Now} Exception thrown {exception} \n";
            }
        }


        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            Text = string.Empty;
        }

        private void Prg_progress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //update estimated time
            double currentProgressValue = Prg_progress.Value;
            long lastElapsedTime = elapsed;
            
            totalElapsed += (lastElapsedTime/1000); // in seconds

            long currentLevel = (long) ((totalElapsed / currentProgressValue) * (maxValue - currentProgressValue));
            Progress = $"Estimated: {currentLevel / 1000} Min"; // in min
        }
    }
}
