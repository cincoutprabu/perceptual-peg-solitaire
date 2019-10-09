//VoiceData.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PerceptualPegSolitaire.Entities
{
    public class VoiceData
    {
        #region Properties

        public string Key { get; set; }
        public Dictionary<string, uint> Commands { get; set; }

        #endregion

        #region Constructors

        public VoiceData()
        {
            this.Key = string.Empty;
            this.Commands = new Dictionary<string, uint>();
        }

        public VoiceData(string key)
        {
            this.Key = key;
            this.Commands = new Dictionary<string, uint>();
        }

        public VoiceData(string key, string text)
        {
            this.Key = key;
            this.Commands = new Dictionary<string, uint>();
            this.Commands.Add(text, 100);
        }

        #endregion
    }

}
