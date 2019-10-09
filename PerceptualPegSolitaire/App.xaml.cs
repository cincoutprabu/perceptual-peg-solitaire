//App.xaml.cs

/* Developed for Intel(R) Perceptual Computing Challenge 2013
 * by Prabu Arumugam (cincoutprabu@gmail.com)
 * http://www.codeding.com/
 * 2013-Aug-12
*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using System.Diagnostics;
using System.Reflection;

using PerceptualPegSolitaire.Entities;
using PerceptualPegSolitaire.Helpers;

namespace PerceptualPegSolitaire
{
    public partial class App : Application
    {
        #region App-Events

        protected override void OnStartup(StartupEventArgs e)
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            if (IsAlreadyRunning())
            {
                UIHelper.ShowError(Constants.APP_NAME + " is already running.");
                Application.Current.Shutdown();
                return;
            }

            //SystemHelper.SetNativeDllPath();
            //SystemHelper.SetLoadOnStartup();

            if (!AllLevels.LoadLevels())
            {
                UIHelper.ShowError("ERROR while reading PlayerData XML." + Environment.NewLine + "Press OK to quit.");
                Application.Current.Shutdown();
                return;
            }

            base.OnStartup(e);
        }

        private bool IsAlreadyRunning()
        {
            Process thisProc = Process.GetCurrentProcess();
            if (Process.GetProcessesByName(thisProc.ProcessName).Length > 1)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// TODO: Pending Tasks
        /// 1) Close About window for 'Close' voice command.
        /// 2) Wave-hand gesture for closing About window. Hide hand-image while About window is in focus.
        /// 3) Implement FaceTracking (show pause/continue window when no face is detected for a considerable amount of time).
        /// </summary>

        #endregion

        #region Events

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Error(e.Exception);
            UIHelper.ShowError("APPLICATION UNHANDLED ERROR:" + Environment.NewLine + e.Exception.Message);
            Application.Current.Shutdown();
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string errorMessage = string.Empty;
            if (e.ExceptionObject is Exception)
            {
                Log.Error((Exception)e.ExceptionObject);
                errorMessage = ((Exception)e.ExceptionObject).Message;
            }
            else if (e.ExceptionObject != null)
            {
                errorMessage = "CurrentDomain_UnhandledException: " + e.ExceptionObject.ToString();
                Log.Write(errorMessage);
            }

            UIHelper.ShowError("DOMAIN UNHANDLED ERROR:" + Environment.NewLine + errorMessage);
            Application.Current.Shutdown();
        }

        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.ToLower().Contains(".resources")) return null;
            Log.Write("AssemblyResolve failed for: " + args.Name);

            //get dll-folder path
            string basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string folderPath = Path.Combine(basePath, @"Native\" + (Environment.Is64BitProcess ? "x64" : "x86"));

            //return new assembly
            string assemblyPath = Path.Combine(folderPath, new AssemblyName(args.Name).Name + ".dll");
            if (File.Exists(assemblyPath))
            {
                Log.Write("New assembly-path: " + assemblyPath);

                Assembly assembly = Assembly.LoadFrom(assemblyPath);
                return assembly;
            }
            return null;
        }

        #endregion
    }
}
