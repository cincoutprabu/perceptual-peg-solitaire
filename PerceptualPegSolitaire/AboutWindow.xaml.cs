//AboutWindow.xaml.cs

/* Developed for Intel(R) Perceptual Computing Challenge 2013
 * by Prabu Arumugam (cincoutprabu@gmail.com)
 * http://www.codeding.com/
 * 2013-Aug-19
*/

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
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using PerceptualPegSolitaire.Entities;

namespace PerceptualPegSolitaire
{
    public partial class AboutWindow : Window
    {
        #region Constructors

        public AboutWindow()
        {
            InitializeComponent();

            this.IsVisibleChanged += new DependencyPropertyChangedEventHandler(AboutWindow_IsVisibleChanged);
        }

        #endregion

        #region Control-Events

        void AboutWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation()
            {
                From = this.IsVisible ? 0 : 1,
                To = this.IsVisible ? 0.92 : 0,
                Duration = TimeSpan.FromMilliseconds(Constants.AnimationSpeed)
            };
            this.BeginAnimation(Window.OpacityProperty, animation);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion
    }
}
