//Log.cs

/* Developed for Intel(R) Perceptual Computing Challenge 2013
 * by Prabu Arumugam (cincoutprabu@gmail.com)
 * http://www.codeding.com/
 * 2013-Aug-11
*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PerceptualPegSolitaire.Entities;

namespace PerceptualPegSolitaire.Helpers
{
    class Log
    {
        #region Methods

        public static void Clear()
        {
            try
            {
                string path = GetLogFilePath();
                File.WriteAllText(path, "");
            }
            catch (Exception)
            {
            }
        }

        public static string Read()
        {
            try
            {
                string path = GetLogFilePath();
                return File.ReadAllText(path);
            }
            catch (Exception)
            {
            }

            return string.Empty;
        }

        public static void Write(string text)
        {
            try
            {
                string path = GetLogFilePath();
                string line = DateTime.Now.ToString(Constants.LOG_DATE_FORMAT) + " " + text + Environment.NewLine;
                File.AppendAllText(path, line);
            }
            catch (Exception)
            {
            }
        }

        public static void Error(Exception exception)
        {
            //Write("ERROR: " + exception.Message + Environment.NewLine + exception.StackTrace);
            Write("ERROR: " + string.Join(Environment.NewLine, new string[] { exception.Message, exception.InnerException == null ? string.Empty : exception.InnerException.Message }));
        }

        private static string GetLogFilePath()
        {
            //return @"C:\PRABU\PegSolitaire-Log.txt";
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.LOG_FILENAME);
        }

        #endregion
    }
}
