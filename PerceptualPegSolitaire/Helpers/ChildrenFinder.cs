//ChildrenFinder.cs

/* Developed for Intel(R) Perceptual Computing Challenge 2013
 * by Prabu Arumugam (cincoutprabu@gmail.com)
 * http://www.codeding.com/
 * 2013-Aug-18
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace PerceptualPegSolitaire.Helpers
{
    internal class ChildrenFinder
    {
        #region Properties

        public List<DependencyObject> Children { get; set; }

        #endregion

        #region Constructors

        public ChildrenFinder()
        {
            this.Children = new List<DependencyObject>();
        }

        #endregion

        #region Methods

        public void All(DependencyObject control)
        {
            int childrenCount = VisualTreeHelper.GetChildrenCount(control);
            for (int childIndex = 0; childIndex < childrenCount; childIndex++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(control, childIndex);
                this.Children.Add(child);
                All(child);
            }
        }

        public void FindByType<T>(DependencyObject control)
        {
            int childrenCount = VisualTreeHelper.GetChildrenCount(control);
            for (int childIndex = 0; childIndex < childrenCount; childIndex++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(control, childIndex);
                if (child is T)
                {
                    this.Children.Add(child);
                }
                FindByType<T>(child);
            }
        }

        #endregion
    }
}
