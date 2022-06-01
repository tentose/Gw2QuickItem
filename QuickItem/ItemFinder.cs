using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Point = OpenCvSharp.Point;

namespace QuickItem
{
    public class ItemFinder
    {
        public static Point? FindItem(string itemImagePath)
        {
            var windowSize = new System.Drawing.Size(2560, 1440);
            Rectangle roi = new Rectangle(System.Drawing.Point.Empty, windowSize);
            Point? value = null;

            var bmp = ScreenCapture.Capture(windowSize, roi);
            if (bmp != null)
            {
                var bits = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                try
                {
                    var original = new Mat(bits.Height, bits.Width, MatType.CV_8UC3, bits.Scan0, bits.Stride).Split()[2];//.CvtColor(ColorConversionCodes.BGR2GRAY);
                    var markerImg = new Mat(itemImagePath).Resize(new OpenCvSharp.Size(62, 62), 0, 0, InterpolationFlags.Linear).Split()[2];//.CvtColor(ColorConversionCodes.BGR2GRAY);
                    var markerMask = new Mat(Path.Combine(QuickItemModule.Instance.ItemIconDirectory, "itemmask.png")).Resize(new OpenCvSharp.Size(62, 62), 0, 0, InterpolationFlags.Linear).Split()[2];//.CvtColor(ColorConversionCodes.BGR2GRAY);

                    value = FindCornersThenLookInSmallRoiScaled(ref original, markerImg, markerMask);

                    //Cv2.ImShow("debug", original);
                }
                finally
                {
                    bmp.UnlockBits(bits);
                }
            }

            return value;
        }

        private static Point? FindCornersThenLookInSmallRoiScaled(ref Mat original, Mat markerImg, Mat markerMask)
        {
            const float scale = 0.6f;

            original = original.Resize(OpenCvSharp.Size.Zero, scale, scale, InterpolationFlags.Linear);
            markerImg = markerImg.Resize(OpenCvSharp.Size.Zero, scale, scale, InterpolationFlags.Linear);
            markerMask = markerMask.Resize(OpenCvSharp.Size.Zero, scale, scale, InterpolationFlags.Linear);

            var position = LookForMarker(original, markerImg, markerMask, 0.1);

            if (position.HasValue)
            {
                //original.DrawMarker(position.Value, Scalar.White, MarkerTypes.Square, 25, 2);
                position = position.Value.Add(new Point(markerImg.Size().Width / 2, markerImg.Size().Height / 2));
                position = position.Value.Multiply(1 / scale);
            }

            return position;
        }

        private static Point? LookForMarker(Mat img, Mat markerImg, Mat markerMask, double confidenceThreshold = 0.3)
        {
            Point? markerPosition = null;

            //Mat output = img.CvtColor(ColorConversionCodes.BGR2GRAY);

            Mat searchImg = img;


            Mat result = new Mat();
            Cv2.MatchTemplate(searchImg, markerImg, result, TemplateMatchModes.SqDiffNormed, markerMask);
            Cv2.MinMaxLoc(result, out double minVal, out double maxVal, out Point minLoc, out Point maxLoc);

            //int x = minLoc.X;
            //int y = minLoc.Y;
            //int w = markerImg.Width;
            //int h = markerImg.Height;
            //Rect exclaimRect = new Rect(x, y, w, h);

            if (minVal < confidenceThreshold)
            {
                markerPosition = minLoc;
            }

            return markerPosition;
        }
    }
}
