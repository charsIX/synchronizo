﻿using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace synchronizo
{
    public partial class synchronizo_home : Form
    {
        string videoPath;
        int peaksSum = 0;
        public synchronizo_home()
        {
            InitializeComponent();
            //input and output device dropdown menu
            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            var input_devices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            var output_devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            input_comboBox.Items.AddRange(input_devices.ToArray());
            output_comboBox.Items.AddRange(output_devices.ToArray());
            
            //moltiplicator rich text box
            molt_richTextBox.SelectAll();
            molt_richTextBox.SelectionAlignment = HorizontalAlignment.Center;
            molt_richTextBox.DeselectAll();
        }

        private void synchronizo_Load(object sender, EventArgs e)
        {

        }

        private void video_browser_button_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Browse button clicked");

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = @"..\..\resources\video"; //setting default dialog box folder
                //openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.CommonVideos); // opening the browser at the default user video folder
                openFileDialog.Filter = "Video files (*.mp4, *.avi, ...)|*.mp4;*.mov;*.avi;.m4v;*.wmv;*.mpg;*.mpeg;*.mp4v;*.3g2;*.3gp2;*.3gp;*.3gpp"; // filtering available files
                openFileDialog.FilterIndex = 1; // selecting the default filter in the browser (mp4)
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    videoPath = openFileDialog.FileName;
                    Console.WriteLine($"Selected file filepath: {videoPath}");
                }
            }
            
            video_viewer_wmp.URL = videoPath;

            if (video_viewer_wmp.URL != string.Empty)
            {
                noVideo_RichTextBox.Visible = false;
                video_viewer_wmp.Visible = true;
                //starting timers
                Console.WriteLine("Starting timers");
                audio_timer.Enabled = true;
                modeSelectorUpdater_timer.Enabled = true;
                peaksSum_timer.Enabled = true;
            }
        }
        
        private void audio_timer_Tick(object sender, EventArgs e)
        {
            if (input_comboBox.SelectedItem != null)
            {
                var input_device = (MMDevice)input_comboBox.SelectedItem;
                int peakValue = (int)(input_device.AudioMeterInformation.MasterPeakValue);
                input_progressBar.Value = peakValue * 100;
                
                peaksSum += peakValue;
            }
            if (output_comboBox.SelectedItem != null)
            {
                var output_device = (MMDevice)output_comboBox.SelectedItem;
                output_progressBar.Value = (int)(output_device.AudioMeterInformation.MasterPeakValue * 100);
            }

        }

        private void peaksSum_timer_Tick(object sender, EventArgs e)
        {
            //peaksSum max = 320
            //peaksSum min = 0
            if (modeSelector_comboBox.SelectedIndex == 0) //noiser is faster
            {
                if (peaksSum >= 250)
                {
                    video_viewer_wmp.settings.rate = 2;
                }
                else if (peaksSum < 250 && peaksSum >= 150)
                {
                    video_viewer_wmp.settings.rate = 1;
                }
                else if (peaksSum < 150 && peaksSum >= 50)
                {
                    video_viewer_wmp.settings.rate = 0.75;
                }
                else
                {
                    video_viewer_wmp.settings.rate = 0.25;
                }
            }
            else if (modeSelector_comboBox.SelectedIndex == 1)
            {
                if (peaksSum >= 250)
                {
                    video_viewer_wmp.settings.rate = 0.25;
                }
                else if (peaksSum < 250 && peaksSum >= 150)
                {
                    video_viewer_wmp.settings.rate = 0.75;
                }
                else if (peaksSum < 150 && peaksSum >= 50)
                {
                    video_viewer_wmp.settings.rate = 1;
                }
                else
                {
                    video_viewer_wmp.settings.rate = 2;
                }
            }
            else
            {
                video_viewer_wmp.settings.rate = 1;
            }

            Console.Write("Peaksum: ");
            Console.WriteLine(peaksSum);
            Console.Write(video_viewer_wmp.settings.rate);
            Console.WriteLine("X");

            peaksSum = 0;
            modeUpdate_progressBar.Value = 0;
            
            molt_richTextBox.Text = $"{video_viewer_wmp.settings.rate} X";
            molt_richTextBox.SelectAll();
            molt_richTextBox.SelectionAlignment = HorizontalAlignment.Center;
            molt_richTextBox.DeselectAll();
        }

        private void modeSelectorUpdater_timer_Tick(object sender, EventArgs e)
        {
            try
            {
                modeUpdate_progressBar.Value += 10;
            }
            catch (Exception)
            {
                Console.WriteLine("modeUpdate_progressBar.Value += 1; failed");
                Console.WriteLine(modeUpdate_progressBar.Value);
                modeUpdate_progressBar.Value = 100;
            }
        }
    }
}