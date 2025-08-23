using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace MultiTool
{
    public partial class MainWindow : Window
    {
        private string? ytDlpPath;

        public MainWindow()
        {
            InitializeComponent();
            ExtractYtDlp();
        }

        private void ExtractYtDlp()
        {
            string tempDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MultiTool");
            if (!Directory.Exists(tempDir))
                Directory.CreateDirectory(tempDir);

            ytDlpPath = Path.Combine(tempDir, "yt-dlp.exe");

            if (!File.Exists(ytDlpPath))
            {
                var assembly = Assembly.GetExecutingAssembly();

                string[] resourceNames = assembly.GetManifestResourceNames();

                string? resourceName = null;
                foreach (string name in resourceNames)
                {
                    if (name.Contains("yt-dlp.exe") || name.EndsWith("yt-dlp.exe"))
                    {
                        resourceName = name;
                        break;
                    }
                }

                if (resourceName == null)
                {
                    resourceName = "MultiTool.Resources.yt-dlp.exe";
                }

                using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        using (FileStream fileStream = new FileStream(ytDlpPath, FileMode.Create, FileAccess.Write))
                        {
                            stream.CopyTo(fileStream);
                        }
                    }
                    else
                    {
                        string availableResources = string.Join("\n", resourceNames);
                        MessageBox.Show($"Resource wasn't found!\nAvailable resources:\n{availableResources}",
                                      "ERROR!", MessageBoxButton.OK, MessageBoxImage.Error);
                        ytDlpPath = null;
                        return;
                    }
                }
            }
        }

        private async void Download_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ytDlpPath) || !File.Exists(ytDlpPath))
            {
                StatusText.Text = "Error: yt-dlp wasn't found on ur COMPUTER!";
                return;
            }

            string url = UrlBox.Text.Trim();
            if (string.IsNullOrEmpty(url))
            {
                StatusText.Text = "You forgot to enter the lint!";
                return;
            }

            string downloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Downloads");
            if (!Directory.Exists(downloadPath))
                Directory.CreateDirectory(downloadPath);

            try
            {
                StatusText.Text = "Started...";
                string arguments = $"-o \"{Path.Combine(downloadPath, "video_%(id)s.%(ext)s")}\" \"{url}\"";

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = ytDlpPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (Process proc = new Process { StartInfo = psi })
                {
                    proc.Start();
                    string output = await proc.StandardOutput.ReadToEndAsync();
                    string error = await proc.StandardError.ReadToEndAsync();
                    await proc.WaitForExitAsync();

                    if (proc.ExitCode == 0)
                        StatusText.Text = $"Video downloaded and saved in: {downloadPath}";
                    else
                        StatusText.Text = $"Error: {error}";
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error: {ex.Message}";
            }
        }

        private void SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog();
            if (dialog.ShowDialog() == true)
            {
                string selectedPath = dialog.FolderName;
                StatusText.Text = $"Selected folder is: {selectedPath}";
            }
        }
    }
}