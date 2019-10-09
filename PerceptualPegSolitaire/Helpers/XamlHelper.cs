//XamlHelper.cs

/* Developed for Intel(R) Perceptual Computing Challenge 2013
 * by Prabu Arumugam (cincoutprabu@gmail.com)
 * http://www.codeding.com/
 * 2013-Aug-17
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

using PerceptualPegSolitaire.Entities;

namespace PerceptualPegSolitaire.Helpers
{
    public class XamlHelper
    {
        #region Methods

        public static void DoDoubleAnimation(UIElement control, DependencyProperty property, int duration, double from, double to, EventHandler callback)
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = from;
            doubleAnimation.To = to;
            doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(duration));

            Storyboard storyboard = new Storyboard();
            Storyboard.SetTarget(doubleAnimation, control);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(property));
            storyboard.Children.Add(doubleAnimation);

            if (callback != null)
            {
                storyboard.Completed += callback;
            }
            storyboard.Begin();
        }

        public void FadeTo(UIElement control, int duration, double targetOpacity)
        {
            DoDoubleAnimation(control, UIElement.OpacityProperty, duration, control.Opacity, targetOpacity, null);
        }

        public static void HideWithAnimation(UIElement control)
        {
            control.Opacity = 1.0;
            DoDoubleAnimation(control, UIElement.OpacityProperty, Constants.AnimationSpeed, control.Opacity, 0.0, new EventHandler((object sender, EventArgs e) =>
            {
                control.Visibility = Visibility.Hidden;
            }));
        }

        public static void ShowWithAnimation(UIElement control)
        {
            control.Opacity = 0.0;
            control.Visibility = Visibility.Visible;
            DoDoubleAnimation(control, UIElement.OpacityProperty, Constants.AnimationSpeed, control.Opacity, 1.0, new EventHandler((object sender, EventArgs e) =>
            {
                //control.Visibility = Visibility.Visible;
            }));
        }

        public static List<DependencyObject> All(DependencyObject container)
        {
            ChildrenFinder childrenFinder = new ChildrenFinder();
            childrenFinder.All(container);
            return (childrenFinder.Children);
        }

        public static List<DependencyObject> ByType<T>(DependencyObject container)
        {
            ChildrenFinder childrenFinder = new ChildrenFinder();
            childrenFinder.FindByType<T>(container);
            return (childrenFinder.Children);
        }

        #endregion
    }
}
