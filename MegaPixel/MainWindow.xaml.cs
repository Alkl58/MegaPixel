﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MegaPixel
{
    public partial class MainWindow : Window
    {
        public string imageOutput, encoder, allSettingsLibavif, allSettingsWebp, allSettingsJpegxl, allSettingsMozjpeg, allSettingsEct, allSettingsCavif;
        public string tempInput;
        public int workerCount, imageChunksCount;
        public bool imageOutputSet, wrongFormat, subFolders;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void setParams()
        {
            // Set Encoder
            encoder = ComboBoxEncoder.Text;
            // Set Workercount
            workerCount = int.Parse(TextBoxWorkerCount.Text);
            imageChunksCount = 0;
            subFolders = CheckBoxBatchSubfolders.IsChecked == true;
            // Count number of images
            foreach (var file in ListBoxImagesToConvert.Items)
            {
                imageChunksCount += 1;
            }
            // Set Encoder Params
            if (ComboBoxEncoder.SelectedIndex == 0) { SetLibavifParams(false); }
            if (ComboBoxEncoder.SelectedIndex == 1) { SetCavifParams(false); }
            if (ComboBoxEncoder.SelectedIndex == 2) { SetWebpParams(false); }
            if (ComboBoxEncoder.SelectedIndex == 3) { SetJpegxlParams(false); }
            if (ComboBoxEncoder.SelectedIndex == 5) { SetMozjpegParams(false); }
            if (ComboBoxEncoder.SelectedIndex == 6) { SetEctParams(false); }
        }

        private void SetLibavifParams(bool temp)
        {
            // Avifenc Settings
            if (CheckBoxCustomSettings.IsChecked == true && temp == false)
            {
                // Custom Settings
                allSettingsLibavif = TextBoxCustomSettings.Text;
            }
            else
            {
                if (CheckBoxAvifLossless.IsChecked == true)
                {
                    allSettingsLibavif = " --lossless ";                                        // Lossless
                    allSettingsLibavif += " --speed " + ComboBoxAvifSpeed.Text;                 // Speed
                    allSettingsLibavif += " --jobs " + TextBoxAvifThreads.Text;                 // Threads
                    allSettingsLibavif += " --depth " + ComboBoxAvifDepth.Text;                 // Bit-Depth
                    allSettingsLibavif += " --yuv " + ComboBoxAvifColorFormat.Text;             // Color Space
                    allSettingsLibavif += " --range " + ComboBoxAvifColorRange.Text;            // Color Range
                    allSettingsLibavif += " --tilerowslog2 " + ComboBoxAvifTileRows.Text;       // Tile Rows
                    allSettingsLibavif += " --tilecolslog2 " + ComboBoxAvifTileColumns.Text;    // Tile Columns
                }
                else
                {
                    allSettingsLibavif = " --min " + TextBoxAvifMinQ.Text;                      // Min-Q
                    allSettingsLibavif += " --max " + TextBoxAvifMaxQ.Text;                     // Max-Q
                    allSettingsLibavif += " --speed " + ComboBoxAvifSpeed.Text;                 // Speed
                    allSettingsLibavif += " --jobs " + TextBoxAvifThreads.Text;                 // Threads
                    allSettingsLibavif += " --depth " + ComboBoxAvifDepth.Text;                 // Bit-Depth
                    allSettingsLibavif += " --yuv " + ComboBoxAvifColorFormat.Text;             // Color Space
                    allSettingsLibavif += " --range " + ComboBoxAvifColorRange.Text;            // Color Range
                    allSettingsLibavif += " --tilerowslog2 " + ComboBoxAvifTileRows.Text;       // Tile Rows
                    allSettingsLibavif += " --tilecolslog2 " + ComboBoxAvifTileColumns.Text;    // Tile Columns
                }
            }
        }

        private void SetCavifParams(bool temp)
        {
            // Cavif Settings
            if (CheckBoxCustomSettings.IsChecked == true && temp == false)
            {
                // Custom Settings
                allSettingsCavif = TextBoxCustomSettings.Text;
            }
            else
            {
                allSettingsCavif = "--quality=" + TextBoxJpegxlQuality.Text + " --speed=" + ComboBoxCavifSpeed.Text;
            }
        }

        private void SetWebpParams(bool temp)
        {
            // WebP Settings
            if (CheckBoxCustomSettings.IsChecked == true && temp == false)
            {
                // Custom Settings
                allSettingsWebp = TextBoxCustomSettings.Text;
            }
            else
            {
                if (CheckBoxWebpNearLossless.IsChecked == true)
                {
                    // Near Lossless
                    allSettingsWebp = " -preset " + ComboBoxWebpPreset.Text;                // Preset
                    allSettingsWebp += " -near_lossless " + TextBoxWebpNearLossless.Text;   // Near Lossless
                    allSettingsWebp += " -z " + ComboBoxWebpLosslessPreset.Text;            // Lossless Preset
                }
                else if (CheckBoxAvifLossless.IsChecked == true)
                {
                    // Lossless
                    allSettingsWebp = " -preset " + ComboBoxWebpPreset.Text;                // Preset
                    allSettingsWebp += " -lossless ";                                       // Lossless
                    allSettingsWebp += " -m " + ComboBoxWebpSpeed.SelectedIndex;            // Speed
                }
                else
                {
                    allSettingsWebp = " -preset " + ComboBoxWebpPreset.Text;                // Preset
                    allSettingsWebp += " -q " + TextBoxWebpQuality.Text;                    // Quality
                    allSettingsWebp += " -m " + ComboBoxWebpSpeed.SelectedIndex;            // Speed
                }
                if (CheckBoxWebpMultiThreading.IsChecked == true)
                {
                    allSettingsWebp += " -mt";                                              // Multi-Threading
                }
            }
        }

        private void SetJpegxlParams(bool temp)
        {
            // JpegXl Settings
            if (CheckBoxCustomSettings.IsChecked == true && temp == false)
            {
                // Custom Settings
                allSettingsJpegxl = TextBoxCustomSettings.Text;
            }
            else
            {
                allSettingsJpegxl = "-q " + TextBoxJpegxlQuality.Text + " -s " + ComboBoxJpegxlSpeed.Text;
            }
        }

        private void SetMozjpegParams(bool temp)
        {
            // Mozjpeg Settings
            if (CheckBoxCustomSettings.IsChecked == true && temp == false)
            {
                // Custom Settings
                allSettingsMozjpeg = TextBoxCustomSettings.Text;
            }
            else
            {
                string quant;
                if(CheckBoxMozjpegQuant.IsChecked == true) 
                { 
                    quant = " -quant-table " + ComboBoxMozjpegQuantTable.SelectedIndex; 
                } else 
                { 
                    quant = ""; 
                }
                allSettingsMozjpeg = "-quality " + TextBoxWebpQuality.Text + " -" + ComboBoxMozjpegTune.Text + " -smooth " + TextBoxMozjpegSmoothing.Text + quant;
            }
        }

        private void SetEctParams(bool temp)
        {
            // ECT Settings
            if (CheckBoxCustomSettings.IsChecked == true && temp == false)
            {
                allSettingsEct = TextBoxCustomSettings.Text;
            }
            else
            {
                allSettingsEct = "-" + ComboBoxEctCompressionLevel.Text;
            }
        }

        private void AutoRemoveNonJpeg()
        {
            List<string> list = new List<string>();
            foreach (var element in ListBoxImagesToConvert.Items)
            {
                if (Path.GetExtension(element.ToString()) != ".jpg" && Path.GetExtension(element.ToString()) != ".jpeg")
                {
                    list.Add(element.ToString());
                }
            }
            foreach (var element in list)
            {
                ListBoxImagesToConvert.Items.Remove(element);
            }
        }

        private void ListBoxImagesToConvert_Drop(object sender, DragEventArgs e)
        {
            //Allows to drag and drop files directly into the ListBox
            List<string> filepaths = new List<string>();
            foreach (var s in (string[])e.Data.GetData(DataFormats.FileDrop, false))
            {
                if (Directory.Exists(s))
                {
                    if (CheckBoxBatchSubfolders.IsChecked == false)
                    {
                        //Add files from folder
                        filepaths.AddRange(Directory.GetFiles(s));
                    }
                    else
                    {
                        //Add files from folder and subfolder
                        tempInput = s;
                        string[] allfiles = Directory.GetFiles(s, "*.*", SearchOption.AllDirectories);
                        foreach (var file in allfiles)
                        {
                            if (Path.GetExtension(file.ToString()) == ".jpg" || Path.GetExtension(file.ToString()) == ".jpeg" || Path.GetExtension(file.ToString()) == ".png")
                            {
                                filepaths.Add(file);
                            }
                        }
                    }
                }
                else
                {
                    //Add filepath
                    filepaths.Add(s);
                }
            }
            foreach (string fileName in filepaths)
            {
                ListBoxImagesToConvert.Items.Add(fileName);            
            }
        }

        private async void ButtonStartEncoding_Click(object sender, RoutedEventArgs e)
        {
            setParams();
            try
            {
                ProgressBar.Value = 0;
                ProgressBar.Maximum = imageChunksCount;
                LabelProgressbar.Content = "0 / " + imageChunksCount;
                await Task.Run(() => ParallelEncode());
                if (CheckBoxPlayFinishedSound.IsChecked == true)
                {
                    SoundPlayer playSound = new SoundPlayer(Properties.Resources.finished);
                    playSound.Play();
                }
            }
            catch { }
        }

        private void ButtonSaveTo_Click(object sender, RoutedEventArgs e)
        {
            // Set Output Path
            System.Windows.Forms.FolderBrowserDialog browseOutputFolder = new System.Windows.Forms.FolderBrowserDialog();
            if (browseOutputFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                imageOutput = browseOutputFolder.SelectedPath;
                LabelOutput.Content = imageOutput;
                imageOutputSet = true;
            }
        }

        private void CheckBoxCustomSettings_Checked(object sender, RoutedEventArgs e)
        {
            // Set Custom Settings Box
            if (ComboBoxEncoder.SelectedIndex == 0) { SetLibavifParams(true); TextBoxCustomSettings.Text = allSettingsLibavif; }
            if (ComboBoxEncoder.SelectedIndex == 1) { SetCavifParams(true); TextBoxCustomSettings.Text = allSettingsCavif; }
            if (ComboBoxEncoder.SelectedIndex == 2) { SetWebpParams(true); TextBoxCustomSettings.Text = allSettingsWebp; }
            if (ComboBoxEncoder.SelectedIndex == 3) { SetJpegxlParams(true); TextBoxCustomSettings.Text = allSettingsJpegxl; }
            if (ComboBoxEncoder.SelectedIndex == 5) { SetMozjpegParams(true); TextBoxCustomSettings.Text = allSettingsMozjpeg; }
            if (ComboBoxEncoder.SelectedIndex == 6) { SetEctParams(true); TextBoxCustomSettings.Text = allSettingsEct; }
        }

        private void ButtonOpenSource_Click(object sender, RoutedEventArgs e)
        {
            // Set Input
            if (CheckBoxBatchEncoding.IsChecked == false)
            {
                // Single File Input
                OpenFileDialog openImageFileDialog = new OpenFileDialog();
                openImageFileDialog.Filter = "Image Files|*.png;*.jpg;|All Files|*.*";
                Nullable<bool> result = openImageFileDialog.ShowDialog();
                if (result == true) { ListBoxImagesToConvert.Items.Add(openImageFileDialog.FileName); ListBoxImagesToConvert.SelectedIndex = 0; }
            }
            else
            {
                // Batch Input
                if (CheckBoxBatchSubfolders.IsChecked == false)
                {
                    // Batch without Subfolders
                    System.Windows.Forms.FolderBrowserDialog browseSourceFolder = new System.Windows.Forms.FolderBrowserDialog();
                    if (browseSourceFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        DirectoryInfo queueFiles = new DirectoryInfo(browseSourceFolder.SelectedPath);
                        foreach (var file in queueFiles.GetFiles())
                        {
                            ListBoxImagesToConvert.Items.Add(file.FullName);
                        }
                        if (ComboBoxEncoder.SelectedIndex == 4)
                        {
                            foreach (var element in ListBoxImagesToConvert.Items)
                            {
                                if (Path.GetExtension(element.ToString()) != ".jpg" && Path.GetExtension(element.ToString()) != ".jpeg") { wrongFormat = true; }
                            }
                            if (wrongFormat)
                            {

                                if (MessageBox.Show("You have elements in the Queue, which are not jpg/jpeg.\n\nMozJpeg only reduces file sizes of JPEG images!\n\nAuto-Remove non jpeg/jpg files from List?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                                {
                                    AutoRemoveNonJpeg();
                                }
                            }
                        }
                        wrongFormat = false;
                    }
                }
                else
                {
                    // Batch with Subfolders
                    System.Windows.Forms.FolderBrowserDialog browseSourceFolder = new System.Windows.Forms.FolderBrowserDialog();
                    if (browseSourceFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        tempInput = browseSourceFolder.SelectedPath;
                        string[] allfiles = Directory.GetFiles(browseSourceFolder.SelectedPath, "*.*", SearchOption.AllDirectories);
                        foreach (var file in allfiles)
                        {
                            if (Path.GetExtension(file.ToString()) == ".jpg" || Path.GetExtension(file.ToString()) == ".jpeg" || Path.GetExtension(file.ToString()) == ".png")
                            {
                                ListBoxImagesToConvert.Items.Add(file);
                            }
                        }
                    }
                }
            }

        }

        private void ButtonRemoveFromList_Click(object sender, RoutedEventArgs e)
        {
            //Removes every selected item from the listbox
            var selected = ListBoxImagesToConvert.SelectedItems.Cast<Object>().ToArray();
            foreach (var item in selected) ListBoxImagesToConvert.Items.Remove(item);
        }

        private void ButtonOpenGithubPage_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/Alkl58/MegaPixel");
        }

        private void ButtonOpenDiscord_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://discord.gg/HSBxne3");
        }

        private void ButtonOpenReddit_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.reddit.com/user/Al_kl");
        }

        private void ComboBoxEncoder_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ComboBoxEncoder.SelectedIndex == 4)
            {
                foreach (var element in ListBoxImagesToConvert.Items)
                {
                    if (Path.GetExtension(element.ToString()) != ".jpg"  && Path.GetExtension(element.ToString()) != ".jpeg") { wrongFormat = true; }
                }
                if (wrongFormat)
                {

                    if (MessageBox.Show("You have elements in the Queue, which are not jpg/jpeg.\n\nMozJpeg only reduces file sizes of JPEG images!\n\nAuto-Remove non jpeg/jpg files from List?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        AutoRemoveNonJpeg();
                    }
                }
            }
            wrongFormat = false;
        }

        private void CheckImageOutput(string Path)
        {
            // Removes Finished Items from List
            if (CheckBoxRemoveFinishedItems.IsChecked == true)
            {
                if (File.Exists(Path))
                {
                    try
                    {
                        ListBoxImagesToConvert.Items.Remove(Path);
                    } catch { }
                }
            }
        }

        private void ParallelEncode()
        {
            using (SemaphoreSlim concurrencySemaphore = new SemaphoreSlim(workerCount))
            {
                List<Task> tasks = new List<Task>();
                foreach (var items in ListBoxImagesToConvert.Items)
                {
                    string imageOutputTemp;
                    if (imageOutputSet == false)
                    {
                        imageOutputTemp = items.ToString() + "_converted.";
                    }
                    else
                    {
                        string imageName = Path.GetFileNameWithoutExtension(items.ToString());
                        if (subFolders == true)
                        {
                            int n = tempInput.Length;
                            string sub = items.ToString().Substring(n, items.ToString().Length - n); // \a\anime-pictures.net-628630.jpg
                            string tempFileName = Path.GetFileName(items.ToString());
                            n = tempFileName.Length;
                            sub = sub.Remove(sub.Length - n);
                            if (Directory.Exists(imageOutput + sub) == false)
                            {
                                Directory.CreateDirectory(imageOutput + sub);
                            }
                            imageOutputTemp = Path.Combine(imageOutput + sub, imageName + ".");
                        }
                        else
                        {
                            imageOutputTemp = Path.Combine(imageOutput, imageName + ".");
                        }
                    }

                    concurrencySemaphore.Wait();
                    var t = Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            Process process = new Process();
                            ProcessStartInfo startInfo = new ProcessStartInfo();
                            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            startInfo.UseShellExecute = true;
                            startInfo.FileName = "cmd.exe";
                            
                            switch (encoder)
                            {
                                case "avif (aom)":
                                    startInfo.WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Encoders", "avif");
                                    startInfo.Arguments = "/C avifenc.exe " + '\u0022' + items + '\u0022' + " " + allSettingsLibavif + " " + '\u0022' + imageOutputTemp + "avif" + '\u0022';
                                    break;
                                case "cavif (rav1e)":
                                    startInfo.WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Encoders", "cavif");
                                    startInfo.Arguments = "/C cavif.exe " + allSettingsCavif + " -o " + '\u0022' + imageOutputTemp + "avif" + '\u0022' + " " + '\u0022' + items + '\u0022';
                                    break;
                                case "webp":
                                    startInfo.WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Encoders", "webp");
                                    startInfo.Arguments = "/C cwebp.exe " + allSettingsWebp + " " + '\u0022' + items + '\u0022' + " -o " + '\u0022' + imageOutputTemp + "webp" + '\u0022';
                                    break;
                                case "jpegxl":
                                    startInfo.WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Encoders", "jpegxl");
                                    startInfo.Arguments = "/C cjpegxl.exe " + '\u0022' + items + '\u0022' + " " + '\u0022' + imageOutputTemp + "jxl" + '\u0022' + " " + allSettingsJpegxl;
                                    break;
                                case "jpegxl decoder":
                                    startInfo.WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Encoders", "jpegxl");
                                    startInfo.Arguments = "/C djpegxl.exe " + '\u0022' + items + '\u0022' + " " + '\u0022' + imageOutputTemp + "png" + '\u0022';
                                    break;
                                case "mozjpeg":
                                    startInfo.WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Encoders", "mozjpeg");
                                    startInfo.Arguments = "/C cjpeg.exe " + allSettingsMozjpeg + " -outfile " + '\u0022' + imageOutputTemp + "jpg" + '\u0022' + " " + '\u0022' + items + '\u0022';
                                    break;
                                case "ect":
                                    startInfo.WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Encoders", "ect");
                                    startInfo.Arguments = "/C ect.exe " + allSettingsEct + " " + '\u0022' + items + '\u0022';
                                    break;
                                default:
                                    break;
                            }
                            //Console.WriteLine(startInfo.Arguments);
                            process.StartInfo = startInfo;
                            process.Start();
                            process.WaitForExit();
                            LabelProgressbar.Dispatcher.Invoke(() => CheckImageOutput(items.ToString()), DispatcherPriority.Background);
                        }
                        finally
                        {
                            concurrencySemaphore.Release();
                            ProgressBar.Dispatcher.Invoke(() => ProgressBar.Value += 1, DispatcherPriority.Background);
                            LabelProgressbar.Dispatcher.Invoke(() => LabelProgressbar.Content = ProgressBar.Value + " / " + imageChunksCount.ToString(), DispatcherPriority.Background);
                        }
                    });
                    tasks.Add(t);

                }
                Task.WaitAll(tasks.ToArray());
            }
        }
    }
}
