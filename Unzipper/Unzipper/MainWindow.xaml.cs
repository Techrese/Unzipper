using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

using System.Threading.Tasks;
using System.Windows;

using Ookii.Dialogs.Wpf;



namespace Unzipper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string rootPath = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
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
                txtReport.Text += $"{DateTime.Now} Exception thrown {exception} \n";

            }
        }

        private async Task LoadFiles()
        {
            try
            {
                txtReport.Text += $"{DateTime.Now} loading files, this may take some time ... \n";
                var zipFiles = new List<string>();
                await Task.Run(() =>
                {
                    zipFiles = Directory.GetFiles(rootPath, "*.zip", SearchOption.AllDirectories).ToList();
                });
                txtReport.Text += $"{DateTime.Now} Found {zipFiles.Count} files \n";
                Extract(zipFiles);
            }
            catch (Exception exception)
            {
                txtReport.Text += $"{DateTime.Now} Exception thrown {exception} \n";
            }
        }

        private void Extract(List<string> zipFiles)
        {
            try
            {
                foreach (var file in zipFiles)
                {
                    //extract same place create folder with same name
                    ZipFile.ExtractToDirectory(file, file.Remove(file.Length-4,4));
                }
            }
            catch (Exception exception)
            {
                txtReport.Text += $"{DateTime.Now} Exception thrown {exception} \n";
            }
        }


        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            txtReport.Text = string.Empty;
        }
    }
}
