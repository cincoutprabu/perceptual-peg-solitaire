//ChooseAudioDevice.xaml.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;

using PerceptualPegSolitaire.BusinessLogic;

namespace PerceptualPegSolitaire
{
    public partial class ChooseAudioDevice : Window
    {
        #region Constructors

        public static ChooseAudioDevice Instance = null;
        public ChooseAudioDevice()
        {
            Instance = this;
            InitializeComponent();
            this.MouseDown += new MouseButtonEventHandler(ChooseAudioDevice_MouseDown);
            this.Closing += new System.ComponentModel.CancelEventHandler(ChooseAudioDevice_Closing);

            //start load-devices-thread
            OkButton.IsEnabled = false;
            DevicesListBox.Focus();

            DevicesListBox.Items.Clear();
            DevicesListBox.Items.Add("Loading...");

            var thread = new Thread(LoadDevices);
            thread.Start();
        }

        #endregion

        #region Control-Events

        void ChooseAudioDevice_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        void ChooseAudioDevice_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!VoiceTracking.Instance.Stopped)
            {
                //VoiceTracking.Instance.Stop();
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (DevicesListBox.SelectedIndex < 0)
            {
                MessageBox.Show("Please choose a Microphone for Voice-Control.");
                return;
            }
            string audioInputDevice = DevicesListBox.SelectedItem.ToString();

            if (VoiceTracking.Instance.VoiceModule == null || VoiceTracking.Instance.VoiceLanguage < 0)
            {
                MessageBox.Show("Voice Capture Module/Language not found. Voice-Control feature will be turned off.");
                audioInputDevice = "None";
            }

            //start voice-tracking
            DevicesListBox.Opacity = 0.5;
            DevicesListBox.IsHitTestVisible = false;
            OkButton.IsEnabled = false;

            if (audioInputDevice.Equals("None"))
            {
                this.Hide();
                new MainWindow().Show();
            }
            else
            {
                VoiceTracking.Instance.Start();
            }
        }

        #endregion

        #region Methods

        public void CloseWindow()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.Hide();
                new MainWindow().Show();
            }));
        }

        #endregion

        #region Internal-Methods

        private void LoadDevices()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                //load audio-devices
                VoiceTracking.Instance = new VoiceTracking();
                VoiceTracking.Instance.VoiceEvent += VoiceTracking.Instance_VoiceEvent;
                VoiceTracking.Instance.Initialize();
                var deviceList = VoiceTracking.Instance.GetAudioCaptureDevices();

                DevicesListBox.Items.Clear();
                DevicesListBox.Items.Add("None");
                deviceList.ForEach(obj => DevicesListBox.Items.Add(obj));

                //choose the device that has 'Creative' word in the name
                var creativeDevice = deviceList.FirstOrDefault(obj => obj.ToLower().Contains("creative"));
                if (creativeDevice != null) DevicesListBox.SelectedItem = creativeDevice;
                else DevicesListBox.SelectedIndex = 0; //choose 'None' by default

                //load voice-module and language
                var moduleList = VoiceTracking.Instance.GetVoiceModules();
                if (moduleList.Count > 0)
                {
                    VoiceTracking.Instance.VoiceModule = moduleList[0];

                    var languageList = VoiceTracking.Instance.GetVoiceLanguages();
                    if (languageList.Count > 0)
                    {
                        VoiceTracking.Instance.VoiceLanguage = 0;
                    }
                }

                OkButton.IsEnabled = true;
            }));
        }

        #endregion
    }
}
