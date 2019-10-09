//FaceTracking.cs

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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Threading;

namespace PerceptualPegSolitaire.BusinessLogic
{
    class FaceTracking
    {
        const string DeviceName = "DepthSense Device 325V2";
        const string ModuleName = "Face Analysis (Intel)";

        #region Fields

        private bool stopped = false;

        public event Action<Bitmap> ImageAvailable;
        public event Action<FaceData> FaceAvailable;
        public event Action FaceNotAvailable;

        #endregion

        #region Methods

        public void Start()
        {
            PXCMSession session;
            pxcmStatus status = PXCMSession.CreateInstance(out session);
            if (status < pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                MessageBox.Show("Unable to create Session for FaceTracking.");
            }

            new Thread(DoTracking).Start();
            Thread.Sleep(5);

            session.Dispose();
        }

        public void Stop()
        {
            stopped = true;
        }

        #endregion

        #region Private-Methods

        private void DoTracking()
        {
            FacePipeline pp = new FacePipeline(1);
            pp.QueryCapture().SetFilter(DeviceName);

            //set modules
            pp.EnableFaceLocation(ModuleName);
            pp.EnableFaceLandmark(ModuleName);

            //initialize
            if (pp.Init())
            {
                while (!stopped)
                {
                    if (!pp.AcquireFrame(true)) break;
                    if (!pp.IsDisconnected())
                    {
                        PXCMFaceAnalysis ft = pp.QueryFace();
                        DisplayPicture(pp.QueryImage(PXCMImage.ImageType.IMAGE_TYPE_COLOR));
                        DisplayLocation(ft);
                    }
                    pp.ReleaseFrame();
                }
            }
            else
            {
                MessageBox.Show("Init Failed");
            }

            pp.Close();
            pp.Dispose();
        }

        private void DisplayPicture(PXCMImage image)
        {
            PXCMImage.ImageData data;
            if (image.AcquireAccess(PXCMImage.Access.ACCESS_READ, PXCMImage.ColorFormat.COLOR_FORMAT_RGB32, out data) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                PXCMImage.ImageInfo imageInfo = image.imageInfo;
                var bmp = data.ToBitmap(imageInfo.width, imageInfo.height);
                if (ImageAvailable != null) ImageAvailable(bmp);
                image.ReleaseAccess(ref data);
            }
        }

        private void DisplayLocation(PXCMFaceAnalysis ft)
        {
            for (uint i = 0; ; i++)
            {
                int fid; ulong ts;
                if (ft.QueryFace(i, out fid, out ts) < pxcmStatus.PXCM_STATUS_NO_ERROR) break;

                //retrieve face location data
                PXCMFaceAnalysis.Detection.Data rData;
                var ftd = ft.DynamicCast<PXCMFaceAnalysis.Detection>(PXCMFaceAnalysis.Detection.CUID);
                var rStatus = ftd.QueryData(fid, out rData);

                //Retrieve face landmark data
                PXCMFaceAnalysis.Landmark.ProfileInfo pinfo;
                var ftl = ft.DynamicCast<PXCMFaceAnalysis.Landmark>(PXCMFaceAnalysis.Landmark.CUID);
                ftl.QueryProfile(out pinfo);
                var lData = new PXCMFaceAnalysis.Landmark.LandmarkData[(int)(pinfo.labels & PXCMFaceAnalysis.Landmark.Label.LABEL_SIZE_MASK)];
                var lStatus = ftl.QueryLandmarkData(fid, pinfo.labels, lData);

                //if (rStatus >= pxcmStatus.PXCM_STATUS_NO_ERROR && lStatus >= pxcmStatus.PXCM_STATUS_NO_ERROR)
                if (rStatus < pxcmStatus.PXCM_STATUS_NO_ERROR || lStatus < pxcmStatus.PXCM_STATUS_NO_ERROR)
                {
                    if (FaceNotAvailable != null) FaceNotAvailable();
                }
                else
                {
                    //build facedata
                    FaceData faceData = new FaceData();
                    faceData.Rect = new Rect(rData.rectangle.x, rData.rectangle.y, rData.rectangle.w, rData.rectangle.h);
                    faceData.Points = lData.Select(p => new System.Windows.Point(p.position.x, p.position.y)).ToList();
                    if (FaceAvailable != null) FaceAvailable(faceData);
                }
            }
        }

        #endregion
    }

    class FaceData
    {
        #region Properties

        public Rect Rect { get; set; }
        public List<System.Windows.Point> Points { get; set; }

        #endregion

        #region Constructors

        public FaceData()
        {
            Rect = Rect.Empty;
            Points = new List<System.Windows.Point>();
        }

        #endregion
    }

    //extend UtilMPipeline to override landmark-configuration
    class FacePipeline : UtilMPipeline
    {
        private uint profileIndex;

        #region Constructors

        public FacePipeline(uint pidx)
            : base()
        {
            profileIndex = pidx;
        }

        #endregion

        #region Methods

        public override void OnFaceLandmarkSetup(ref PXCMFaceAnalysis.Landmark.ProfileInfo finfo)
        {
            PXCMFaceAnalysis.Landmark ftl = QueryFace().DynamicCast<PXCMFaceAnalysis.Landmark>(PXCMFaceAnalysis.Landmark.CUID);
            ftl.QueryProfile(profileIndex, out finfo);
        }

        #endregion
    }
}
