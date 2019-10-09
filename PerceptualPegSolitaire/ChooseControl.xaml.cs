//ChooseControl.xaml.cs

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
    public partial class ChooseControl : Window
    {
        #region Constructors

        public ChooseControl()
        {
            InitializeComponent();

            this.IsVisibleChanged += new DependencyPropertyChangedEventHandler(ChooseControl_IsVisibleChanged);
        }

        #endregion

        #region Control-Events

        void ChooseControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation()
            {
                From = this.IsVisible ? 0 : 1,
                To = this.IsVisible ? 1 : 0,
                Duration = TimeSpan.FromMilliseconds(Constants.AnimationSpeed)
            };
            this.BeginAnimation(Window.OpacityProperty, animation);
        }

        private void ClosedHandContainer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Constants.ControlGesture = BallControlGesture.CloseHand;
            LoadMainWindow();
        }

        private void ThumbUpContainer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Constants.ControlGesture = BallControlGesture.ThumbUp;
            LoadMainWindow();
        }

        #endregion

        #region Internal-Methods

        private void LoadMainWindow()
        {
            new MainWindow().Show();
            this.Visibility = System.Windows.Visibility.Hidden;
        }

        #endregion
    }
}
