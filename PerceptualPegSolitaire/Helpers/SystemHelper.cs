//SystemHelper.cs

/* Developed for Intel(R) Perceptual Computing Challenge 2013
 * by Prabu Arumugam (cincoutprabu@gmail.com)
 * http://www.codeding.com/
 * 2013-Aug-15
*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Win32;

using PerceptualPegSolitaire.Entities;

namespace PerceptualPegSolitaire.Helpers
{
    class SystemHelper
    {
        [DllImport("kernel32.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool SetDllDirectory(string pathName);

        #region Methods

        public static void SetNativeDllPath()
        {
            try
            {
                var folderPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Native\" + (Environment.Is64BitProcess ? "x64" : "x86"));
                string dllPath = Path.Combine(folderPath, "libpxcclr.dll");
                if (Directory.Exists(folderPath) && File.Exists(dllPath))
                {
                    //Assembly.Load(dllPath);
                    //SetDllDirectory(path);
                }
            }
            catch (Exception exception)
            {
                Log.Error(exception);
            }
        }

        public static void RegisterComDll(string dllFileName)
        {
            try
            {
                var dllPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Native\" + dllFileName);
                //var dllPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), dllFileName);

                ProcessStartInfo process = new ProcessStartInfo("regsvr32", "/s " + dllPath);
                process.UseShellExecute = false;
                process.CreateNoWindow = true;
                process.RedirectStandardOutput = true;
                Process.Start(process);
            }
            catch (Exception exception)
            {
                Log.Error(exception);
            }
        }

        public static void SetLoadOnStartup()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (key.GetValue(Constants.APP_NAME) == null)
            {
                string processFileName = Process.GetCurrentProcess().MainModule.FileName;
                if (!processFileName.ToLower().Contains("vshost") && !Debugger.IsAttached) //ignore if running from Visual Studio
                {
                    string appPath = "\"" + Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"PerceptualPegSolitaire.exe") + "\" /minimized /regrun";
                    key.SetValue(Constants.APP_NAME, appPath);
                }
            }

            //to remove app from startup
            //key.DeleteValue(Constants.APP_ID, false);
        }

        public static TimeSpan GetSystemUpTime()
        {
            using (var counter = new PerformanceCounter("System", "System Up Time"))
            {
                counter.NextValue();
                TimeSpan uptime = TimeSpan.FromSeconds(counter.NextValue());
                return uptime;
            }
        }

        #endregion
    }
}
