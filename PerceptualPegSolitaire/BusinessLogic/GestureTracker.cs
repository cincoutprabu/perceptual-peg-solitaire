//GestureTracker.cs

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

using PerceptualPegSolitaire.Entities;
using PerceptualPegSolitaire.Helpers;

namespace PerceptualPegSolitaire.BusinessLogic
{
    public enum CamEvent
    {
        FAILED_TO_CREATE_SDK_SESSION,
        FAILED_TO_LOAD_GESTURE_RECOGNITION,
        FAILED_TO_LOCATE_CAPTURE_MODULE,
        DEVICE_DISCONNECTED,
        DEVICE_FAILED,
        DEVICE_RECONNECTED,
        HOVERING_OUTSIDE
    }

    class GestureTracker : IDisposable
    {
        #region Fields

        public event Action<CamEvent> OnError;
        public event Action<CamEvent> OnNotify;
        public event Action<PXCMGesture.Alert> OnAlert;
        public event Action<Point> OnMovement;
        public event Action<PXCMGesture.GeoNode.Openness, uint> OnOpenClose;
        public event Action<PXCMGesture.Gesture> OnGesture;

        bool _tracking;
        PXCMGesture.GeoNode.Openness _previousOpenness = PXCMGesture.GeoNode.Openness.LABEL_OPENNESS_ANY;

        #endregion

        #region Constructors

        public GestureTracker()
        {
            _tracking = true;
        }

        #endregion

        #region Methods

        public void Start()
        {
            //create session
            PXCMSession session;
            pxcmStatus status = PXCMSession.CreateInstance(out session);
            if (IsError(status))
            {
                OnError(CamEvent.FAILED_TO_CREATE_SDK_SESSION);
                return;
            }

            //create gesture-module
            PXCMBase gestureBase;
            status = session.CreateImpl(PXCMGesture.CUID, out gestureBase);
            if (IsError(status))
            {
                OnError(CamEvent.FAILED_TO_LOAD_GESTURE_RECOGNITION);
                session.Dispose();
                return;
            }

            //create gesture-profile
            PXCMGesture gesture = (PXCMGesture)gestureBase;
            PXCMGesture.ProfileInfo profileInfo;
            status = gesture.QueryProfile(0, out profileInfo);
            profileInfo.activationDistance = 70;

            //setup gesture-capture
            UtilMCapture capture = new UtilMCapture(session);
            status = capture.LocateStreams(ref profileInfo.inputs);
            if (IsError(status))
            {
                OnError(CamEvent.FAILED_TO_LOCATE_CAPTURE_MODULE);
                gesture.Dispose();
                capture.Dispose();
                session.Dispose();
                return;
            }

            status = gesture.SetProfile(ref profileInfo);
            status = gesture.SubscribeAlert(this.OnAlertHandler);
            status = gesture.SubscribeGesture(100, this.OnGesureHandler);

            //start capture of frames
            bool device_lost = false;
            PXCMImage[] images = new PXCMImage[PXCMCapture.VideoStream.STREAM_LIMIT];
            PXCMScheduler.SyncPoint[] syncPoints = new PXCMScheduler.SyncPoint[2];

            while (_tracking)
            {
                status = capture.ReadStreamAsync(images, out syncPoints[0]);
                if (IsError(status))
                {
                    if (status == pxcmStatus.PXCM_STATUS_DEVICE_LOST)
                    {
                        if (!device_lost) OnError(CamEvent.DEVICE_DISCONNECTED);
                        device_lost = true;
                        continue;
                    }
                    OnError(CamEvent.DEVICE_FAILED);
                    break;
                }
                if (device_lost)
                {
                    OnNotify(CamEvent.DEVICE_RECONNECTED);
                    device_lost = false;
                }

                status = gesture.ProcessImageAsync(images, out syncPoints[1]);
                if (IsError(status)) break;

                PXCMScheduler.SyncPoint.SynchronizeEx(syncPoints);
                if (syncPoints[0].Synchronize(0) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
                {
                    PXCMGesture.GeoNode data;
                    status = gesture.QueryNodeData(0, PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_PRIMARY, out data);
                    if (!IsError(status))
                    {
                        if (ShapeHelper.IsPointInsideRect(data.positionImage.x, data.positionImage.y, Constants.FoVWindow))
                        {
                            //adjust the point to field-of-view window
                            Point cameraPoint = new Point(data.positionImage.x - Constants.FoVWindow.X, data.positionImage.y - Constants.FoVWindow.Y);
                            //cameraPoint = ShapeHelper.RotatePoint(cameraPoint, Constants.FoVCenter, Constants.RotationAngle);
                            OnMovement(cameraPoint);

                            if (data.opennessState != _previousOpenness)
                            {
                                OnOpenClose(data.opennessState, data.openness);
                                _previousOpenness = data.opennessState;
                            }
                        }
                        else
                        {
                            OnNotify(CamEvent.HOVERING_OUTSIDE);
                        }
                    }
                }

                foreach (PXCMScheduler.SyncPoint p in syncPoints) if (p != null) p.Dispose();
                foreach (PXCMImage img in images) if (img != null) img.Dispose();
            }

            if (gesture != null) gesture.Dispose();
            if (capture != null) capture.Dispose();
            if (session != null) session.Dispose();
        }

        public void Stop()
        {
            _tracking = false;
        }

        public void ClearOpenness()
        {
            _previousOpenness = PXCMGesture.GeoNode.Openness.LABEL_OPENNESS_ANY;
        }

        #endregion

        #region Events

        void OnAlertHandler(ref PXCMGesture.Alert alertData)
        {
            this.OnAlert(alertData);
        }

        void OnGesureHandler(ref PXCMGesture.Gesture gestureData)
        {
            this.OnGesture(gestureData);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Stop();
        }

        #endregion

        #region Helper-Methods

        public static bool IsError(pxcmStatus status)
        {
            return status < pxcmStatus.PXCM_STATUS_NO_ERROR;
        }

        #endregion
    }
}
