//MessageWindow.xaml.cs

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
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;

using PerceptualPegSolitaire.Entities;
using PerceptualPegSolitaire.Helpers;

namespace PerceptualPegSolitaire
{
    public partial class MessageWindow : Window
    {
        #region Properties

        private bool _autoClose = false;
        public bool AutoClose
        {
            get
            {
                return _autoClose;
            }
            set
            {
                _autoClose = value;
                this.OkButton.Visibility = _autoClose ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        #endregion

        #region Constructors

        public MessageWindow()
        {
            this.DataContext = this;
            InitializeComponent();

            this.IsVisibleChanged += new DependencyPropertyChangedEventHandler(MessageWindow_IsVisibleChanged);
            this.Activated += new EventHandler(MessageWindow_Activated);
        }

        void MessageWindow_Activated(object sender, EventArgs e)
        {
            if (this.AutoClose)
            {
                UIHelper.Run(() =>
                {
                    this.Visibility = Visibility.Hidden;
                    this.Close();
                }, 2000);
            }
        }

        #endregion

        #region Control-Events

        void MessageWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation()
            {
                From = this.IsVisible ? 0.95 : 1,
                To = this.IsVisible ? 0.95 : 0,
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
