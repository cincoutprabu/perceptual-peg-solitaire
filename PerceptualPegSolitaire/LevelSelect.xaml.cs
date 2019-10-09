//LevelSelect.xaml.cs

/* Developed for Intel(R) Perceptual Computing Challenge 2013
 * by Prabu Arumugam (cincoutprabu@gmail.com)
 * http://www.codeding.com/
 * 2013-Aug-18
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

using PerceptualPegSolitaire.Entities;
using PerceptualPegSolitaire.Helpers;

namespace PerceptualPegSolitaire
{
    public partial class LevelSelect : UserControl, INotifyPropertyChanged
    {
        #region Properties

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Level> Levels { get; set; }

        #endregion

        #region Constructors

        public static LevelSelect Instance;
        public LevelSelect()
        {
            Instance = this;
            InitializeComponent();

            this.DataContext = this;
            PopulateLevels();
        }

        #endregion

        #region Internal-Methods

        public void PopulateLevels()
        {
            this.Levels = new ObservableCollection<Level>();

            foreach (int levelNumber in Enumerable.Range(1, Constants.MaxLevels))
            {
                var level = AllLevels.LevelList.FirstOrDefault(obj => obj.Number == levelNumber);
                this.Levels.Add(level);
            }
        }

        #endregion

        #region Methods

        public void CheckLevelUnderXY(double boardX, double boardY, bool clicked)
        {
            foreach (int level in levelControls.Keys)
            {
                Grid levelGrid = levelControls[level];
                Point location = levelGrid.TranslatePoint(new Point(0, 0), MainWindow.Instance.BoardCanvas);
                Rect levelRect = new Rect(location.X, location.Y, levelGrid.ActualWidth, levelGrid.ActualHeight);
                if (ShapeHelper.IsPointInsideRect(boardX, boardY, levelRect))
                {
                    if (clicked)
                    {
                        LevelGrid_MouseDown(levelGrid, null);
                    }
                    else
                    {
                        LevelGrid_MouseEnter(levelGrid, null);
                    }
                }
                else if (!clicked)
                {
                    LevelGrid_MouseLeave(levelGrid, null);
                }
            }
        }

        #endregion

        #region Control-Events

        private Dictionary<int, Grid> levelControls = new Dictionary<int, Grid>();

        private void LevelGrid_Loaded(object sender, RoutedEventArgs e)
        {
            Grid levelGrid = (Grid)sender;
            Level level = (Level)levelGrid.DataContext;

            if (!levelControls.ContainsKey(level.Number))
            {
                levelControls.Add(level.Number, levelGrid);
            }
        }

        private void LevelGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            Grid levelGrid = (Grid)sender;
            Border levelBorder = (Border)levelGrid.FindName("LevelBorder");
            levelBorder.BorderBrush = new SolidColorBrush(Colors.LightGray);
        }

        private void LevelGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            Grid levelGrid = (Grid)sender;
            Border levelBorder = (Border)levelGrid.FindName("LevelBorder");
            levelBorder.BorderBrush = new SolidColorBrush(Colors.Chocolate);
        }

        private void LevelGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement senderControl = (FrameworkElement)sender;
            Level level = (Level)senderControl.DataContext;

            if (!level.Unlocked)
            {
                //UIHelper.ShowInformation(string.Format("You have to complete Level {0} to play this Level.", level.Number - 1), true);
                return;
            }

            Constants.CurrentLevel = level.Number;
            MainWindow.Instance.LevelSelected();
        }

        #endregion
    }
}
