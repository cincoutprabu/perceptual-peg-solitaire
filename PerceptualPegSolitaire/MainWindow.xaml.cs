//MainWindow.xaml.cs

/* Developed for Intel(R) Perceptual Computing Challenge 2013
 * by Prabu Arumugam (cincoutprabu@gmail.com)
 * http://www.codeding.com/
 * 2013-Aug-12
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using PerceptualPegSolitaire.Entities;
using PerceptualPegSolitaire.Helpers;
using PerceptualPegSolitaire.BusinessLogic;

namespace PerceptualPegSolitaire
{
    public partial class MainWindow : Window
    {
        PegSolitaire _boardControl;
        GestureTracker _tracker;
        bool _isHandActive = false;

        #region Constructors

        public static MainWindow Instance;
        public MainWindow()
        {
            Instance = this;
            InitializeComponent();
            LoadMenu();

            UIHelper.Run(() =>
            {
                //Constants.CanvasSize = new Size(Board.ActualWidth, Board.ActualHeight);
                //Board.HighlightCells(ref BoardCanvas);

                //MessageBox.Show(BoardContainer.ActualWidth + "," + BoardContainer.ActualHeight);
                //MessageBox.Show(LevelsContainer.ActualWidth + "," + LevelsContainer.ActualHeight);
                //MessageBox.Show(CanvasContainer.ActualWidth + "," + CanvasContainer.ActualHeight);
                //MessageBox.Show(BoardCanvas.ActualWidth + "," + BoardCanvas.ActualHeight);

                //UIHelper.ShowInformation("Hi");

                Task.Factory.StartNew(() => InitializeGestureTracker(), TaskCreationOptions.LongRunning);
            }, 500);
        }

        #endregion

        #region Methods

        public void CloseApp()
        {
            VoiceTracking.Instance.Stop();
            ChooseAudioDevice.Instance.Close();
            Application.Current.Shutdown();
        }

        public void ExecuteVoiceCommand(string command)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.TitleTextBlock.Text = command;

                switch (command)
                {
                    case "Close":
                        MainWindow.Instance.CloseApp();
                        break;
                    case "Back":
                        if (_boardControl != null) //if in game-screen
                        {
                            BackImage_MouseDown(null, null);
                        }
                        break;
                    case "Ok":
                        _tracker_OnOpenClose(PXCMGesture.GeoNode.Openness.LABEL_CLOSE, 0); //simulate hand-close gesture
                        break;
                    case "No":
                        if (_boardControl != null) //if in game-screen
                        {
                            _boardControl.DoUndo();
                        }
                        break;
                }
            }));
        }

        public void LevelSelected()
        {
            _boardControl = new PegSolitaire();
            BoardPlaceholder.Content = _boardControl;
            LoadBoard();
        }

        public void LevelCompleted(Level level, bool advanceToNextLevel)
        {
            AllLevels.SaveLevels();
            AllLevels.LoadLevels();

            if (advanceToNextLevel)
            {
                if (level.Number == Constants.MaxLevels)
                {
                    LoadMenu();
                }
                else
                {
                    Constants.CurrentLevel = level.Number + 1;
                    LevelSelected();
                }
            }
        }

        private void LoadMenu()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                LevelPlaceholder.Content = new LevelSelect();
                //LevelSelectControl.PopulateLevels();

                XamlHelper.HideWithAnimation(BoardContainer);
                XamlHelper.HideWithAnimation(BackImage);
                XamlHelper.HideWithAnimation(RefreshImage);

                XamlHelper.ShowWithAnimation(LevelsContainer);
                XamlHelper.ShowWithAnimation(CloseImage);
                XamlHelper.ShowWithAnimation(AboutImage);

                _boardControl = null;
            }));
        }

        private void LoadBoard()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                XamlHelper.HideWithAnimation(LevelsContainer);
                XamlHelper.HideWithAnimation(CloseImage);
                XamlHelper.HideWithAnimation(AboutImage);

                XamlHelper.ShowWithAnimation(BoardContainer);
                XamlHelper.ShowWithAnimation(BackImage);
                XamlHelper.ShowWithAnimation(RefreshImage);
            }));
        }

        #endregion

        #region Internal-Methods

        private void InitializeGestureTracker()
        {
            try
            {
                _tracker = new GestureTracker();
                _tracker.OnError += new Action<CamEvent>(_tracker_OnError);
                _tracker.OnNotify += new Action<CamEvent>(_tracker_OnNotify);
                _tracker.OnAlert += new Action<PXCMGesture.Alert>(_tracker_OnAlert);
                _tracker.OnMovement += new Action<Point>(_tracker_OnMovement);
                _tracker.OnOpenClose += new Action<PXCMGesture.GeoNode.Openness, uint>(_tracker_OnOpenClose);
                _tracker.OnGesture += new Action<PXCMGesture.Gesture>(_tracker_OnGesture);
                _tracker.Start();
            }
            catch (Exception exception)
            {
                UIHelper.ShowError("GESTURE TRACKER INITIALIZATION ERROR: " + exception.Message);
            }
        }

        private void AddLog(string text)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                LogContainer.Children.Add(new TextBlock() { Text = text, Margin = new Thickness(1) });
            }));
        }

        private void SetLog(string text)
        {
            ClearLog();
            AddLog(text);
        }

        private void ClearLog()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                LogContainer.Children.Clear();
            }));
        }

        private void ShowHand()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                HandImage.Visibility = Visibility.Visible;
                //HandPointer.Visibility = Visibility.Visible;
            }));
        }

        private void HideHand()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                HandImage.Visibility = Visibility.Hidden;
                HandPointer.Visibility = Visibility.Hidden;
            }));
        }

        private void CheckFieldOfView(PXCMGesture.Alert.Label alert)
        {
            if (alert == PXCMGesture.Alert.Label.LABEL_GEONODE_ACTIVE)
            {
                _isHandActive = true;
            }
            else if (alert == PXCMGesture.Alert.Label.LABEL_GEONODE_INACTIVE ||
                     alert == PXCMGesture.Alert.Label.LABEL_FOV_BLOCKED)
            {
                _isHandActive = false;
                ClearHover();
                HideHand();
            }

            //if (_isHandActive) ShowHand();
            //else HideHand();
        }

        private void ClearHover()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                if (_boardControl != null)
                {
                    if (_currentPebblePoint != null && _currentPebblePoint.Row >= 0 && _currentPebblePoint.Column >= 0)
                    {
                        if (_boardControl._currentGameMode != GameMode.TargetSelection)
                        {
                            _boardControl.PebbleUnHovered(_currentPebblePoint.Row, _currentPebblePoint.Column);
                        }
                    }
                }
            }));
        }

        #endregion

        #region Control-Events

        bool _rotating = false;

        private void BoardContainer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //_rotating = true;
        }

        private void BoardContainer_MouseMove(object sender, MouseEventArgs e)
        {
            if (_rotating)
            {
                RotateTransform transform = new RotateTransform();

                Point p = e.GetPosition(this);
                Point center = new Point(BoardContainer.ActualWidth / 2.0, BoardContainer.ActualHeight / 2.0);
                double radians = Math.Atan((p.Y - center.Y) / (p.X - center.X));
                transform.Angle = radians * 180 / Math.PI;

                //apply a 180 degree shift when X is negative so that we can rotate all of the way around
                if (p.X - center.X < 0) transform.Angle += 180;

                BoardContainer.RenderTransform = transform;
                Constants.RotationAngle = transform.Angle;
            }
        }

        private void BoardContainer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _rotating = false;
        }

        private void CloseImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.CloseApp();
            }));
        }

        private void AboutImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                UIHelper.Run(() =>
                {
                    new AboutWindow().ShowDialog();
                }, 100);
            }));
        }

        private void BackImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            LoadMenu();
        }

        private void RefreshImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _boardControl.ReStart();
        }

        #endregion

        #region Gesture-Events

        void _tracker_OnError(CamEvent e)
        {
            //AddLog(e.ToString() + ",");

            if (e == CamEvent.DEVICE_DISCONNECTED)
            {
                _isHandActive = false;
            }

            if (e != CamEvent.DEVICE_DISCONNECTED || e != CamEvent.DEVICE_FAILED)
            {
                _tracker.Stop();
                //_tracker.Dispose();
            }
        }

        void _tracker_OnNotify(CamEvent e)
        {
            //AddLog("[Notification: " + e + "]");

            //if (_boardControl == null) return;

            if (e == CamEvent.DEVICE_RECONNECTED)
            {
                //InitializeGestureTracker();
            }

            if (e == CamEvent.HOVERING_OUTSIDE)
            {
                ClearHover();
                HideHand();
            }
        }

        void _tracker_OnAlert(PXCMGesture.Alert alertData)
        {
            if (_boardControl == null)
            {
                //AddLog("[Alert: " + alertData.label + "]");
                //return;
            }

            CheckFieldOfView(alertData.label);
            _tracker.ClearOpenness();

            //AddLog("[Alert: " + alertData.label + ", HandActive: " + _isHandActive.ToString() + "]");
        }

        Position _previousPebblePoint = null;
        Position _currentPebblePoint = null;
        double _boardX, _boardY;
        void _tracker_OnMovement(Point cameraPoint)
        {
            //if (_boardControl == null) return;
            if (!_isHandActive) return;

            //normalize the point i.e. reduce the spatial coordinates to a small window (to reduce movement flicker?)
            double normalizedX = (cameraPoint.X / Constants.FoVWindow.Width) * Constants.NormalizedFoV.Width;
            double normalizedY = (cameraPoint.Y / Constants.FoVWindow.Height) * Constants.NormalizedFoV.Height;

            //find the cell/pebble of current hand position
            _boardX = BoardCanvas.ActualWidth - ((normalizedX / Constants.NormalizedFoV.Width) * BoardCanvas.ActualWidth);
            _boardY = (normalizedY / Constants.NormalizedFoV.Height) * BoardCanvas.ActualHeight;
            //_boardX += Constants.IndexFingerMargin.X; _boardY += Constants.IndexFingerMargin.Y;

            //string text = string.Empty;
            //text += "Camra Pt: " + cameraPoint + ",";
            //text += "Board Sz: " + _boardControl.ActualWidth + "," + _boardControl.ActualHeight + ",";
            //text += "Board Pt: " + (int)boardX + "," + (int)boardY;
            //text += "Pbbl Pos: " + _currentPebblePoint.Row + "," + _currentPebblePoint.Column;

            this.Dispatcher.Invoke(new Action(() =>
            {
                //TitleTextBlock.Text = text;

                if (_boardX < 0 || _boardX > BoardCanvas.ActualWidth || _boardY < 0 || _boardX > BoardCanvas.ActualHeight)
                {
                    HideHand();
                }
                else
                {
                    ShowHand();
                    HandImage.SetValue(Canvas.LeftProperty, _boardX);
                    HandImage.SetValue(Canvas.TopProperty, _boardY);
                    HandPointer.SetValue(Canvas.LeftProperty, _boardX);
                    HandPointer.SetValue(Canvas.TopProperty, _boardY);
                }

                if (_boardControl != null)
                {
                    if (_previousPebblePoint != null && _previousPebblePoint.Row >= 0 && _previousPebblePoint.Column >= 0)
                    {
                        if (_boardControl._currentGameMode != GameMode.TargetSelection)
                        {
                            _boardControl.PebbleUnHovered(_previousPebblePoint.Row, _previousPebblePoint.Column);
                        }
                    }

                    _currentPebblePoint = _boardControl.FindPebbleUnderXY(_boardX + Constants.IndexFingerMargin.X, _boardY + Constants.IndexFingerMargin.Y);
                    //_currentPebblePoint = _boardControl.FindPebbleUnderXY(_boardX, _boardY);
                    if (_currentPebblePoint != null && _currentPebblePoint.Row >= 0 && _currentPebblePoint.Column >= 0)
                    {
                        if (_boardControl._currentGameMode != GameMode.TargetSelection)
                        {
                            _boardControl.PebbleHovered(_currentPebblePoint.Row, _currentPebblePoint.Column);
                            _previousPebblePoint = _currentPebblePoint;
                        }
                    }
                }
                else
                {
                    LevelSelect.Instance.CheckLevelUnderXY(_boardX + Constants.IndexFingerMargin.X, _boardY + Constants.IndexFingerMargin.Y, false);
                }
            }));
        }

        void _tracker_OnOpenClose(PXCMGesture.GeoNode.Openness opennessState, uint openness)
        {
            //AddLog("[OpenState: " + opennessState + "]");

            //if (_boardControl == null) return;
            //if (Constants.ControlGesture == BallControlGesture.ThumbUp) return;

            if (opennessState == PXCMGesture.GeoNode.Openness.LABEL_CLOSE)
            {
                if (_boardControl != null)
                {
                    ActivateCurrentPebble();
                }
                else
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        LevelSelect.Instance.CheckLevelUnderXY(_boardX + Constants.IndexFingerMargin.X, _boardY + Constants.IndexFingerMargin.Y, true);
                    }));
                }
            }
        }

        private DateTime _lastGestureAt = default(DateTime);

        void _tracker_OnGesture(PXCMGesture.Gesture gestureData)
        {
            //AddLog("[Gesture: " + gestureData.label + "]");

            //ignore consecutive gestures
            if (_lastGestureAt != default(DateTime))
            {
                TimeSpan span = DateTime.Now.Subtract(_lastGestureAt);
                if (Math.Abs(span.TotalSeconds) < 3.0) return;
            }

            switch (gestureData.label)
            {
                case PXCMGesture.Gesture.Label.LABEL_HAND_WAVE:
                    {
                        _lastGestureAt = DateTime.Now;

                        if (_boardControl != null)
                        {
                            BackImage_MouseDown(null, null);
                        }
                        else
                        {
                            CloseImage_MouseDown(null, null);
                        }
                    }
                    break;
                case PXCMGesture.Gesture.Label.LABEL_POSE_THUMB_UP:
                    {
                        _lastGestureAt = DateTime.Now;

                        //if (_boardControl == null)
                        {
                            AboutImage_MouseDown(null, null);
                            return;
                        }

                        if (Constants.ControlGesture == BallControlGesture.ThumbUp)
                        {
                            ActivateCurrentPebble();
                        }
                    }
                    break;
                case PXCMGesture.Gesture.Label.LABEL_POSE_THUMB_DOWN:
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            _boardControl.DoUndo();
                        }));
                    }
                    break;
            }
        }

        private void ActivateCurrentPebble()
        {
            if (_currentPebblePoint != null && _currentPebblePoint.Row >= 0 && _currentPebblePoint.Column >= 0)
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    if (_boardControl._currentGameMode == GameMode.TargetSelection && _boardControl.IsEmptyPebble(_currentPebblePoint.Row, _currentPebblePoint.Column))
                    {
                        _boardControl.PebbleActivated(_currentPebblePoint);
                    }
                    else if (_boardControl.IsValidPebble(_currentPebblePoint.Row, _currentPebblePoint.Column))
                    {
                        _boardControl.PebbleActivated(_currentPebblePoint);
                    }
                }));
            }
        }

        #endregion
    }
}
