//VoiceTracking.cs

/* Developed for Intel(R) Perceptual Computing Challenge 2013
 * by Prabu Arumugam (cincoutprabu@gmail.com)
 * http://www.codeding.com/
 * 2013-Aug-18
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading;

using PerceptualPegSolitaire.Entities;

namespace PerceptualPegSolitaire.BusinessLogic
{
    public class VoiceTracking
    {
        #region Fields/Properties

        public static string[] GameCommands = new string[] { "Close", "Back", "Ok", "No" };

        public string AudioInputDevice { get; set; }
        public string VoiceModule { get; set; }
        public uint VoiceLanguage { get; set; }
        public bool Stopped { get; set; }

        PXCMSession session;
        public event Action<VoiceData> VoiceEvent;

        #endregion

        #region Constructors

        public static VoiceTracking Instance = null;
        public VoiceTracking()
        {
        }

        public static void Instance_VoiceEvent(VoiceData data)
        {
            if (data.Key == "ListeningStarted")
            {
                ChooseAudioDevice.Instance.CloseWindow();
                return;
            }
            else if (data.Key == "Alert")
            {
                //
            }
            else if (data.Key == "Command")
            {
                if (data.Commands.Count > 0)
                {
                    var sorted = data.Commands.OrderBy(obj => obj.Value).Reverse();
                    var command = sorted.ElementAt(0).Key;
                    MainWindow.Instance.ExecuteVoiceCommand(command);
                }
            }

            //MessageBox.Show("VoiceEvent: " + key + ", " + values);
        }

        #endregion

        #region Methods

        public void Initialize()
        {
            pxcmStatus status = PXCMSession.CreateInstance(out session);
            if (status < pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                MessageBox.Show("Unable to create Session for VoiceTracking.");
            }
        }

        public List<string> GetAudioCaptureDevices()
        {
            PXCMSession.ImplDesc desc = new PXCMSession.ImplDesc();
            desc.group = PXCMSession.ImplGroup.IMPL_GROUP_SENSOR;
            desc.subgroup = PXCMSession.ImplSubgroup.IMPL_SUBGROUP_AUDIO_CAPTURE;

            var deviceList = new List<string>();
            for (uint i = 0; ; i++)
            {
                PXCMSession.ImplDesc desc1;
                if (session.QueryImpl(ref desc, i, out desc1) < pxcmStatus.PXCM_STATUS_NO_ERROR) break;
                PXCMCapture capture;
                if (session.CreateImpl<PXCMCapture>(ref desc1, PXCMCapture.CUID, out capture) < pxcmStatus.PXCM_STATUS_NO_ERROR) continue;
                for (uint j = 0; ; j++)
                {
                    PXCMCapture.DeviceInfo dinfo;
                    if (capture.QueryDevice(j, out dinfo) < pxcmStatus.PXCM_STATUS_NO_ERROR) break;
                    deviceList.Add(dinfo.name.get());
                }
                capture.Dispose();
            }

            return deviceList;
        }

        public List<string> GetVoiceModules()
        {
            PXCMSession.ImplDesc desc = new PXCMSession.ImplDesc();
            desc.cuids[0] = PXCMVoiceRecognition.CUID;

            var moduleList = new List<string>();
            for (uint i = 0; ; i++)
            {
                PXCMSession.ImplDesc desc1;
                if (session.QueryImpl(ref desc, i, out desc1) < pxcmStatus.PXCM_STATUS_NO_ERROR) break;
                moduleList.Add(desc1.friendlyName.get());
            }

            return moduleList;
        }

        public List<string> GetVoiceLanguages()
        {
            PXCMSession.ImplDesc desc = new PXCMSession.ImplDesc();
            desc.cuids[0] = PXCMVoiceRecognition.CUID;
            desc.friendlyName.set(this.VoiceModule);

            PXCMVoiceRecognition vrec;
            if (session.CreateImpl<PXCMVoiceRecognition>(ref desc, PXCMVoiceRecognition.CUID, out vrec) < pxcmStatus.PXCM_STATUS_NO_ERROR) return null;

            var languageList = new List<string>();
            for (uint i = 0; ; i++)
            {
                PXCMVoiceRecognition.ProfileInfo pinfo;
                if (vrec.QueryProfile(i, out pinfo) < pxcmStatus.PXCM_STATUS_NO_ERROR) break;
                languageList.Add(GetLanguageName(pinfo.language));
            }
            vrec.Dispose();

            return languageList;
        }

        public void Start()
        {
            new Thread(DoTracking).Start();
            Thread.Sleep(5);
        }

        public void Stop()
        {
            this.Stopped = true;
        }

        public void OnVoiceInput(VoiceData data)
        {
            if (VoiceEvent != null)
            {
                VoiceEvent(data);
            }
        }

        #endregion

        #region Internal-Methods

        private void DoTracking()
        {
            VoicePipeline pp = new VoicePipeline(this, this.VoiceLanguage);

            //set audio-source
            pp.QueryCapture().SetFilter(this.AudioInputDevice);

            //set module
            pp.EnableVoiceRecognition(this.VoiceModule);

            //set commands
            pp.SetVoiceCommands(GameCommands);
            //pp.SetVoiceDictation();

            if (pp.Init())
            {
                if (VoiceEvent != null) VoiceEvent(new VoiceData("ListeningStarted"));

                //set audio volume to 0.2
                pp.QueryCapture().device.SetProperty(PXCMCapture.Device.Property.PROPERTY_AUDIO_MIX_LEVEL, 0.2f);

                //recognition-loop
                while (!this.Stopped)
                {
                    if (!pp.AcquireFrame(true)) break;
                    pp.ReleaseFrame();
                }
            }

            pp.Close();
            pp.Dispose();
        }

        #endregion

        #region Helper-Methods

        private string GetLanguageName(PXCMVoiceRecognition.ProfileInfo.Language language)
        {
            switch (language)
            {
                case PXCMVoiceRecognition.ProfileInfo.Language.LANGUAGE_US_ENGLISH: return "US English";
                case PXCMVoiceRecognition.ProfileInfo.Language.LANGUAGE_GB_ENGLISH: return "British English";
                case PXCMVoiceRecognition.ProfileInfo.Language.LANGUAGE_DE_GERMAN: return "Deutsch";
                case PXCMVoiceRecognition.ProfileInfo.Language.LANGUAGE_IT_ITALIAN: return "italiano";
                case PXCMVoiceRecognition.ProfileInfo.Language.LANGUAGE_BR_PORTUGUESE: return "PORTUGUÊS";
                case PXCMVoiceRecognition.ProfileInfo.Language.LANGUAGE_CN_CHINESE: return "中文";
                case PXCMVoiceRecognition.ProfileInfo.Language.LANGUAGE_FR_FRENCH: return "Français";
                case PXCMVoiceRecognition.ProfileInfo.Language.LANGUAGE_JP_JAPANESE: return "日本語";
                case PXCMVoiceRecognition.ProfileInfo.Language.LANGUAGE_US_SPANISH: return "español";
            }
            return null;
        }

        #endregion
    }

    //extend UtilMPipeline to override voice-capture
    class VoicePipeline : UtilMPipeline
    {
        private VoiceTracking _track;
        private uint profileIndex;

        #region Constructors

        public VoicePipeline(VoiceTracking track, uint pidx)
            : base()
        {
            _track = track;
            profileIndex = pidx;
        }

        #endregion

        #region Methods

        public override void OnVoiceRecognitionSetup(ref PXCMVoiceRecognition.ProfileInfo pinfo)
        {
            QueryVoiceRecognition().QueryProfile(profileIndex, out pinfo);
        }

        public override void OnRecognized(ref PXCMVoiceRecognition.Recognition data)
        {
            if (data.label < 0)
            {
                _track.OnVoiceInput(new VoiceData("Dictation", data.dictation));
            }
            else
            {
                //form.ClearScores();
                VoiceData voiceData = new VoiceData("Command");
                for (int i = 0; i < 4; i++)
                {
                    int label = data.nBest[i].label;
                    uint confidence = data.nBest[i].confidence;
                    if (label < 0 || confidence == 0) continue;

                    voiceData.Commands.Add(VoiceTracking.GameCommands[label], confidence);
                    //scores += VoiceTracking.GameCommands[label] + ":" + confidence + ",";
                }
                _track.OnVoiceInput(voiceData);
            }
        }

        public override void OnAlert(ref PXCMVoiceRecognition.Alert data)
        {
            _track.OnVoiceInput(new VoiceData("Alert", data.label.ToString()));
        }

        #endregion
    }
}
