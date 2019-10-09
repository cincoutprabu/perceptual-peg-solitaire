//PegSolitaire.xaml.cs

/* Developed for Intel(R) Perceptual Computing Challenge 2013
 * by Prabu Arumugam (cincoutprabu@gmail.com)
 * http://www.codeding.com/
 * 2013-Aug-12
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using PerceptualPegSolitaire.Entities;
using PerceptualPegSolitaire.Helpers;

namespace PerceptualPegSolitaire
{
    public enum PebbleState
    {
        Invalid = -1,
        Empty = 0,
        Valid = 1
    }

    public enum GameMode
    {
        Play = 1,           //indicates normal gaming mode
        TargetSelection = 2 //indicates that a target pebble selection is in progress
    }

    public partial class PegSolitaire : UserControl
    {
        #region Fields

        private Level _level;
        private Board _board;

        private string _holeImagePath = "/Images/hole.png";
        private string _highlightedImagePath = "Images/Balls/ball3.png";
        private Dictionary<string, string> _pebbleImagePaths = new Dictionary<string, string>()
        { 
            {"ball1", "Images/Balls/ball1.png"},
            {"ball2", "Images/Balls/ball2.png"},
        };
        private string _currentTheme = "ball1";

        private double _holeOpacity = 1.0;
        private double _normalPebbleOpacity = 1.0;
        private double _targetPositionOpacity = 0.5;
        private double _immovablePebbleOpacity = 0.4;
        private double _otherPebblesOpacity = 0.2;

        public GameMode _currentGameMode = GameMode.Play;
        private Position _currentPosition = null;
        private List<Position> _currentPossibleTargetPositions = null;

        #endregion

        #region Constructors

        public PegSolitaire()
        {
            this.DataContext = this;
            InitializeComponent();
            InitializeBoard();
            RenderBoard();
            CheckGameOver();
        }

        #endregion

        #region Properties

        public List<string> AllThemes
        {
            get
            {
                return _pebbleImagePaths.Keys.ToList();
            }
        }

        #endregion

        #region Internal-Methods

        private void InitializeBoard()
        {
            //set grid rows & columns
            BoardGrid.RowDefinitions.Clear();
            for (int row = 0; row < Board.Rows; row++)
            {
                BoardGrid.RowDefinitions.Add(new RowDefinition());
            }
            BoardGrid.ColumnDefinitions.Clear();
            for (int column = 0; column < Board.Columns; column++)
            {
                BoardGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            //initialize board
            _level = AllLevels.LevelList.FirstOrDefault(obj => obj.Number == Constants.CurrentLevel);
            _board = _level.GetBoard();

            LevelTextBlock.Text = _level.Number.ToString();
        }

        private List<ContentControl> _pebbleControls = new List<ContentControl>();
        public void RenderBoard()
        {
            BoardGrid.Children.Clear();
            _pebbleControls.Clear();

            for (int row = 0; row < Board.Rows; row++)
            {
                for (int column = 0; column < Board.Columns; column++)
                {
                    if (!IsInvalidPebble(row, column)) //if not invalid position
                    {
                        bool addQTextBlock = false;

                        string pebbleImagePath = _holeImagePath;
                        Cursor pebbleCursor = Cursors.Arrow;
                        double pebbleOpacity = _holeOpacity;
                        bool pebbleHitTest = false;

                        if (IsValidPebble(row, column)) //if there is a pebble in this position
                        {
                            pebbleImagePath = _pebbleImagePaths[_currentTheme];
                            pebbleCursor = Cursors.Hand;
                            pebbleOpacity = _normalPebbleOpacity;
                            pebbleHitTest = true;
                        }

                        //if game is in target-selection-mode, disable moving other pebbles
                        if (_currentGameMode == GameMode.TargetSelection)
                        {
                            pebbleOpacity = _otherPebblesOpacity;
                            pebbleHitTest = false;
                            pebbleCursor = Cursors.Arrow;

                            if (IsTargetPosition(new Position(row, column)))
                            {
                                pebbleImagePath = _highlightedImagePath;
                                pebbleOpacity = _normalPebbleOpacity;
                                pebbleHitTest = true;
                                pebbleCursor = Cursors.Hand;

                                addQTextBlock = true;
                            }
                            if (row == _currentPosition.Row && column == _currentPosition.Column)
                            {
                                //pebbleImagePath = _highlightedImagePath;
                                pebbleOpacity = _normalPebbleOpacity;
                            }
                        }

                        Image pebbleImage = new Image();
                        pebbleImage.Source = new BitmapImage(new Uri(pebbleImagePath, UriKind.Relative));
                        pebbleImage.Stretch = Stretch.Uniform;
                        pebbleImage.Margin = new Thickness(2);
                        if (pebbleImagePath.Equals(_holeImagePath))
                        {
                            //pebbleImage.RenderTransform = new ScaleTransform(0.95, 0.95);
                        }

                        ContentControl pebbleControl = new ContentControl();
                        pebbleControl.Name = "PebbleControl_" + row + "_" + column;
                        pebbleControl.Tag = new Position(row, column);
                        pebbleControl.Content = pebbleImage;
                        //pebbleControl.Style = (Style)FindResource("PebbleButtonStyle");
                        //pebbleControl.Background = new SolidColorBrush(Colors.Transparent);
                        //pebbleControl.BorderBrush = new SolidColorBrush(Colors.Transparent);
                        //pebbleControl.BorderThickness = new Thickness(0);
                        pebbleControl.Cursor = pebbleCursor;
                        pebbleControl.Opacity = pebbleOpacity;
                        pebbleControl.IsHitTestVisible = pebbleHitTest;
                        pebbleControl.IsTabStop = false;
                        pebbleControl.SetValue(Grid.RowProperty, row);
                        pebbleControl.SetValue(Grid.ColumnProperty, column);
                        pebbleControl.MouseEnter += new MouseEventHandler(pebbleControl_MouseEnter);
                        pebbleControl.MouseLeave += new MouseEventHandler(pebbleControl_MouseLeave);
                        //pebbleControl.Click += new RoutedEventHandler(pebbleControl_Click);
                        pebbleControl.MouseDown += new MouseButtonEventHandler(pebbleControl_MouseDown);
                        BoardGrid.Children.Add(pebbleControl);
                        _pebbleControls.Add(pebbleControl);

                        if (addQTextBlock)
                        {
                            TextBlock qTextBlock = new TextBlock();
                            qTextBlock.Text = "?";
                            qTextBlock.Foreground = new SolidColorBrush(Colors.Black);
                            qTextBlock.IsHitTestVisible = false;
                            qTextBlock.FontSize = 52;
                            qTextBlock.FontWeight = FontWeights.ExtraBold;
                            qTextBlock.Cursor = Cursors.Hand;
                            qTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
                            qTextBlock.VerticalAlignment = VerticalAlignment.Center;
                            qTextBlock.SetValue(Grid.RowProperty, row);
                            qTextBlock.SetValue(Grid.ColumnProperty, column);
                            BoardGrid.Children.Add(qTextBlock);
                        }
                    }
                }
            }
            BoardGrid.UpdateLayout();
        }

        private PebbleState GetPebbleState(int row, int column)
        {
            return (PebbleState)_board[row][column];
        }

        private bool IsInvalidPebble(int row, int column)
        {
            return (GetPebbleState(row, column) == PebbleState.Invalid);
        }

        public bool IsEmptyPebble(int row, int column)
        {
            return (GetPebbleState(row, column) == PebbleState.Empty);
        }

        public bool IsValidPebble(int row, int column)
        {
            return (GetPebbleState(row, column) == PebbleState.Valid);
        }

        public void PebbleHovered(int row, int column)
        {
            if (IsValidPebble(row, column))
            {
                ContentControl pebbleControl = GetPebbleControl(row, column);
                if (pebbleControl == null) return;
                Image pebbleImage = (Image)pebbleControl.Content;

                //highlight possible positions
                List<Position> possiblePositions = FindPossibleTargetPositions(row, column);
                if (possiblePositions.Count == 0) //if this pebble is not movable, indicate thru cursor and opacity
                {
                    pebbleControl.Cursor = Cursors.Arrow;
                    pebbleControl.Opacity = _immovablePebbleOpacity;
                }
                else
                {
                    //highlight current-pebble and possible-target-positions
                    pebbleImage.Source = new BitmapImage(new Uri(_highlightedImagePath, UriKind.Relative));
                    foreach (Position possiblePosition in possiblePositions)
                    {
                        HighlightTargetPosition(possiblePosition.Row, possiblePosition.Column);
                    }
                }
            }
        }

        public void PebbleUnHovered(int row, int column)
        {
            if (IsValidPebble(row, column))
            {
                ContentControl pebbleControl = GetPebbleControl(row, column);
                if (pebbleControl == null) return;
                Image pebbleImage = (Image)pebbleControl.Content;
                pebbleImage.Source = new BitmapImage(new Uri(_pebbleImagePaths[_currentTheme], UriKind.Relative));

                //dehighlight possible positions
                List<Position> possiblePositions = FindPossibleTargetPositions(row, column);
                if (possiblePositions.Count == 0)
                {
                    pebbleControl.Opacity = _normalPebbleOpacity;
                }
                else
                {
                    foreach (Position possiblePosition in possiblePositions)
                    {
                        DehighlightTargetPosition(possiblePosition.Row, possiblePosition.Column);
                    }
                }
            }
        }

        public void PebbleActivated(Position position)
        {
            //if game is in target-selection-mode, and a target-pebble is clicked, make a valid-move now
            if (_currentGameMode == GameMode.TargetSelection)
            {
                if (IsTargetPosition(position))
                {
                    Position targetPosition = FindTargetPosition(position);
                    MakeValidMove(_currentPosition, targetPosition);

                    _currentGameMode = GameMode.Play;
                    RenderBoard();
                }
                return;
            }

            List<Position> possiblePositions = FindPossibleTargetPositions(position.Row, position.Column);
            if (possiblePositions.Count == 1) //if there is only one target pebble, replace it
            {
                Position targetPosition = possiblePositions[0];
                MakeValidMove(position, targetPosition);
            }
            else if (possiblePositions.Count > 1) //if more than possible target positions, turn on target-selection mode
            {
                _currentGameMode = GameMode.TargetSelection;
                _currentPosition = position;
                _currentPossibleTargetPositions = possiblePositions;
                RenderBoard();
            }
        }

        public void HighlightCells(ref Canvas canvas)
        {
            double cellWidth = this.ActualWidth / Board.Columns;
            double cellHeight = this.ActualHeight / Board.Rows;

            double cellX = 0;
            for (int column = 0; column < Board.Columns; column++, cellX += cellWidth)
            {
                double cellY = 0;
                for (int row = 0; row < Board.Rows; row++, cellY += cellHeight)
                {
                    Rectangle rect = new Rectangle();
                    rect.Stroke = new SolidColorBrush(Colors.Green);
                    rect.Width = cellWidth;
                    rect.Height = cellHeight;
                    rect.SetValue(Canvas.LeftProperty, cellX);
                    rect.SetValue(Canvas.TopProperty, cellY);
                    canvas.Children.Add(rect);
                }
            }
        }

        public Position FindPebbleUnderXY(double boardX, double boardY)
        {
            double cellWidth = this.ActualWidth / Board.Columns;
            double cellHeight = this.ActualHeight / Board.Rows;

            double cellX = 0;
            for (int column = 0; column < Board.Columns; column++, cellX += cellWidth)
            {
                double cellY = 0;
                for (int row = 0; row < Board.Rows; row++, cellY += cellHeight)
                {
                    Rect cellRect = new Rect(cellX, cellY, cellWidth, cellHeight);
                    if (ShapeHelper.IsPointInsideRect(boardX, boardY, cellRect))
                    {
                        return new Position(row, column);
                    }
                }
            }

            return new Position(-1, -1);
        }

        private void HighlightTargetPosition(int row, int column)
        {
            ContentControl pebbleControl = GetPebbleControl(row, column);
            if (pebbleControl != null)
            {
                Image pebbleImage = (Image)pebbleControl.Content;
                pebbleImage.Source = new BitmapImage(new Uri(_highlightedImagePath, UriKind.Relative));
                pebbleControl.Opacity = _targetPositionOpacity;
            }
        }

        private void DehighlightTargetPosition(int row, int column)
        {
            ContentControl pebbleControl = GetPebbleControl(row, column);
            if (pebbleControl != null)
            {
                Image pebbleImage = (Image)pebbleControl.Content;
                pebbleImage.Source = new BitmapImage(new Uri(_holeImagePath, UriKind.Relative));
                pebbleControl.Opacity = _holeOpacity;
            }
        }

        private List<Position> FindPossibleTargetPositions(int row, int column)
        {
            List<Position> positionList = new List<Position>();

            //check top
            if (row > 1)
            {
                if (IsValidPebble(row - 1, column) && IsEmptyPebble(row - 2, column))
                {
                    positionList.Add(new Position(row - 2, column, Direction.Up));
                }
            }

            //check bottom
            if (row < Board.Rows - 2)
            {
                if (IsValidPebble(row + 1, column) && IsEmptyPebble(row + 2, column))
                {
                    positionList.Add(new Position(row + 2, column, Direction.Down));
                }
            }

            //check left
            if (column > 1)
            {
                if (IsValidPebble(row, column - 1) && IsEmptyPebble(row, column - 2))
                {
                    positionList.Add(new Position(row, column - 2, Direction.Left));
                }
            }

            //check right
            if (column < Board.Columns - 2)
            {
                if (IsValidPebble(row, column + 1) && IsEmptyPebble(row, column + 2))
                {
                    positionList.Add(new Position(row, column + 2, Direction.Right));
                }
            }

            return positionList;
        }

        private void RemoveMiddlePebble(Position position)
        {
            if (position.Direction == Direction.Up)
            {
                _board[position.Row + 1][position.Column] = (int)PebbleState.Empty;
            }
            else if (position.Direction == Direction.Down)
            {
                _board[position.Row - 1][position.Column] = (int)PebbleState.Empty;
            }
            else if (position.Direction == Direction.Left)
            {
                _board[position.Row][position.Column + 1] = (int)PebbleState.Empty;
            }
            else if (position.Direction == Direction.Right)
            {
                _board[position.Row][position.Column - 1] = (int)PebbleState.Empty;
            }
        }

        private void MakeValidMove(Position currentPosition, Position targetPosition)
        {
            SaveForUndo();

            //remove current-pebble
            _board[currentPosition.Row][currentPosition.Column] = (int)PebbleState.Empty;

            //remove middle-pebble
            RemoveMiddlePebble(targetPosition);

            //remove target-pebble
            _board[targetPosition.Row][targetPosition.Column] = (int)PebbleState.Valid;

            RenderBoard();
            CheckGameOver();
        }

        private bool IsTargetPosition(Position position)
        {
            return (FindTargetPosition(position) != null);
        }

        private Position FindTargetPosition(Position position)
        {
            if (_currentPossibleTargetPositions != null)
            {
                foreach (Position targetPosition in _currentPossibleTargetPositions)
                {
                    if (position.Row == targetPosition.Row && position.Column == targetPosition.Column)
                    {
                        return targetPosition;
                    }
                }
            }
            return null;
        }

        private void CheckGameOver()
        {
            //find number of remaining pebbles
            int remainingPebbles = 0;
            for (int row = 0; row < Board.Rows; row++)
            {
                for (int column = 0; column < Board.Columns; column++)
                {
                    if (_board[row][column] == 1)
                    {
                        remainingPebbles += 1;
                    }
                }
            }
            RemainingPebblesTextBlock.Text = remainingPebbles.ToString();

            //find target positions of all valid-pebbles
            int totalTargetPositions = 0;
            for (int row = 0; row < Board.Rows; row++)
            {
                for (int column = 0; column < Board.Columns; column++)
                {
                    if (IsValidPebble(row, column))
                    {
                        List<Position> targetPositions = FindPossibleTargetPositions(row, column);
                        totalTargetPositions += targetPositions.Count;
                    }
                }
            }

            //if no more moves, then game-over
            bool gameOver = totalTargetPositions == 0;
            if (gameOver)
            {
                if (remainingPebbles > Constants.MaxRemainingPebbles)
                {
                    if (_level.BestPebbleCount == 0)
                    {
                        UIHelper.ShowInformation(string.Format("Game Over! You have to reduce the number of pebbles to {0} or less to advance to Level {1}. All the BEST!", Constants.MaxRemainingPebbles, _level.Number + 1));
                    }
                    else
                    {
                        UIHelper.ShowInformation(string.Format("Game Over! But try again, your previous best was {0}.", _level.BestPebbleCount));
                    }
                }
                else
                {
                    if (_level.Number < Constants.MaxLevels)
                    {
                        var nextLevel = AllLevels.LevelList.First(obj => obj.Number == _level.Number + 1);
                        if (!nextLevel.Unlocked) //its ok to congratulate only once for a level
                        {
                            nextLevel.Unlocked = true;
                            UIHelper.ShowInformation(string.Format("CONGRATS! You have unlocked Level {0}.", nextLevel.Number));
                        }
                    }
                    else
                    {
                        if (_level.BestPebbleCount == 0) //congratulate only once
                        {
                            UIHelper.ShowInformation("GREAT! You have mastered ALL the boards!!.");
                        }
                    }

                    if (_level.BestPebbleCount == 0)
                    {
                        _level.BestPebbleCount = remainingPebbles;
                        MainWindow.Instance.LevelCompleted(_level, true);
                    }
                    else if (remainingPebbles < _level.BestPebbleCount) //avoid overriding previous best
                    {
                        _level.BestPebbleCount = remainingPebbles;
                        UIHelper.ShowInformation(string.Format("NEW RECORD!"));
                        MainWindow.Instance.LevelCompleted(_level, false);
                    }
                    else if (remainingPebbles > _level.BestPebbleCount)
                    {
                        UIHelper.ShowInformation(string.Format("CONGRATS! But try again, your previous best was {0}.", _level.BestPebbleCount));
                    }
                    else
                    {
                        UIHelper.ShowInformation(string.Format("CONGRATS! You have equalled your previous record!"));
                    }
                }
            }
        }

        private ContentControl GetPebbleControl(int row, int column)
        {
            ContentControl pebbleControl = _pebbleControls.FirstOrDefault(obj => obj.Name.Equals("PebbleControl_" + row + "_" + column));
            //ContentControl pebbleControl = (ContentControl)BoardContainer.FindName("PebbleControl_" + row + "_" + column);
            return pebbleControl;
        }

        public void ReStart()
        {
            InitializeBoard();
            RenderBoard();
            CheckGameOver();

            _cacheBoard = null;
            UndoImage.Visibility = Visibility.Hidden;
        }

        #endregion

        #region Events

        void pebbleControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (_currentGameMode == GameMode.TargetSelection) return;

            ContentControl senderControl = (ContentControl)sender;
            Position position = (Position)senderControl.Tag;
            PebbleHovered(position.Row, position.Column);

            e.Handled = true;
        }

        void pebbleControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_currentGameMode == GameMode.TargetSelection) return;

            ContentControl senderControl = (ContentControl)sender;
            Position position = (Position)senderControl.Tag;
            PebbleUnHovered(position.Row, position.Column);

            e.Handled = true;
        }

        void pebbleControl_Click(object sender, RoutedEventArgs e)
        {
            ContentControl senderControl = (ContentControl)sender;
            Position position = (Position)senderControl.Tag;
            PebbleActivated(position);

            e.Handled = true;
        }

        void pebbleControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            pebbleControl_Click(sender, e);

            e.Handled = true;
        }

        private void UndoImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DoUndo();
        }

        #endregion

        #region Undo-Methods

        private Board _cacheBoard;

        private void SaveForUndo()
        {
            if (_cacheBoard == null) _cacheBoard = new Board();
            else _cacheBoard.Clear();

            _cacheBoard = _board.CreateCopy();
            UndoImage.Visibility = Visibility.Visible;
        }

        public void DoUndo()
        {
            if (_cacheBoard == null) return;

            _board.Clear();
            _board.AddRange(_cacheBoard);
            RenderBoard();

            UndoImage.Visibility = Visibility.Hidden;
        }

        #endregion
    }
}
