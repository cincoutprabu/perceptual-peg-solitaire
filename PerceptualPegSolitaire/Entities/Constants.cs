//Constants.cs

/* Developed for Intel(R) Perceptual Computing Challenge 2013
 * by Prabu Arumugam (cincoutprabu@gmail.com)
 * http://www.codeding.com/
 * 2013-Aug-13
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace PerceptualPegSolitaire.Entities
{
    public enum BallControlGesture
    {
        CloseHand,
        ThumbUp
    }

    static class Constants
    {
        public static string APP_NAME = "Perceptual PegSolitaire";
        public const string LOG_FILENAME = "PerceptualPegSolitaire-Log.txt";
        public const string LOG_DATE_FORMAT = "yyyy-MMM-dd HH:mm:ss";
        public const string DATA_FILENAME = "PegSolitaire-PlayerData.xml";

        public static Size CameraFoV = new Size(320, 240);
        //ignore much of FoV at bottom (to avoid struggle to control bottom-pebbles bcoz of elbow hitting the belly)
        public static Rect FoVWindow = new Rect(80, 60, 160, 80);
        public static Point FoVCenter = new Point(120, 40);
        public static Size NormalizedFoV = new Size(80, 60);
        public static Size CanvasSize = Size.Empty;
        public static double RotationAngle = 0.0;
        public static Point IndexFingerMargin = new Point(62, 2);

        public static BallControlGesture ControlGesture = BallControlGesture.CloseHand;
        public static int CurrentLevel = 1;
        public static int MaxLevels = 9;
        public static int MaxRemainingPebbles = 4;
        public static int AnimationSpeed = 600;
    }
}
