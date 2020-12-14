using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization.Configuration;
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

        private async Task LoadFiles()
        {
            txtReport.Text += $"{DateTime.Now} loading files, this may take some time ...";
            var zipFiles = new List<string>();
            await Task.Run(() =>
            {
                zipFiles = Directory.GetFiles(rootPath, "*", SearchOption.AllDirectories).ToList();
            });
            txtReport.Text += $"{DateTime.Now} Found {zipFiles.Count} files";
        }


        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            txtReport.Text = string.Empty;
        }
    }
}
