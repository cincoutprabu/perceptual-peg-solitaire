//UIHelper.cs

/* Developed for Intel(R) Perceptual Computing Challenge 2013
 * by Prabu Arumugam (cincoutprabu@gmail.com)
 * http://www.codeding.com/
 * 2013-Aug-16
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;

using PerceptualPegSolitaire.Entities;

namespace PerceptualPegSolitaire.Helpers
{
    class UIHelper
    {
        #region Methods

        public static void ShowInformation(string text)
        {
            ShowInformation(text, false);
        }

        public static void ShowInformation(string text, bool autoClose)
        {
            //MessageBox.Show(text, Constants.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Information);

            //Run(() =>
            //{
            MessageWindow window = new MessageWindow();
            window.AutoClose = autoClose;
            window.MessageTextBlock.Text = text;
            window.ShowDialog();
            //}, 200);
        }

        public static void ShowError(string text)
        {
            MessageBox.Show(text, Constants.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        //runs the action after specified duration
        public static void Run(Action action, long milliSeconds)
        {
            var timer = new DispatcherTimer();
            timer.Tick += delegate(object sender, EventArgs e)
            {
                timer.Stop();
                action.Invoke();
            };
            timer.Interval = TimeSpan.FromMilliseconds(milliSeconds);
            timer.Start();
        }

        #endregion
    }
}
