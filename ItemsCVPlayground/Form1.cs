using OpenCvSharp;
using System.Diagnostics;
using Point = OpenCvSharp.Point;

namespace ItemsCVPlayground
{
    public partial class Form1 : Form
    {
        System.Windows.Forms.Timer timer;
        ScreenCapture screenCapture;

        Dictionary<string, Marker> boundsMarkers = new Dictionary<string, Marker>();
        Dictionary<string, Marker> markers = new Dictionary<string, Marker>();
        Marker randomTemplate;

        public Form1()
        {
            InitializeComponent();
            timer = new System.Windows.Forms.Timer();
            screenCapture = new ScreenCapture();

            boundsMarkers["upperleft"] = new Marker()
            {
                Img = new Mat(@"Markers\upper-left.png").CvtColor(ColorConversionCodes.BGR2GRAY),
                Mask = new Mat(@"Markers\upper-left-mask.png").CvtColor(ColorConversionCodes.BGR2GRAY),
            };
            boundsMarkers["upperright"] = new Marker()
            {
                Img = new Mat(@"Markers\upper-right.png").CvtColor(ColorConversionCodes.BGR2GRAY),
                Mask = new Mat(@"Markers\upper-right-mask.png").CvtColor(ColorConversionCodes.BGR2GRAY),
            };
            boundsMarkers["lowerleft"] = new Marker()
            {
                Img = new Mat(@"Markers\lower-left.png").CvtColor(ColorConversionCodes.BGR2GRAY),
                //Mask = new Mat(@"Markers\lower-left-mask.png").CvtColor(ColorConversionCodes.BGR2GRAY),
            };
            boundsMarkers["lowerright"] = new Marker()
            {
                Img = new Mat(@"Markers\lower-right.png").CvtColor(ColorConversionCodes.BGR2GRAY),
                Mask = new Mat(@"Markers\lower-right-mask.png").CvtColor(ColorConversionCodes.BGR2GRAY),
            };


            Mat trans_mat = new Mat(2, 3, MatType.CV_32FC1);
            trans_mat.Set(0, 0, 1f);
            trans_mat.Set(0, 1, 0f);
            trans_mat.Set(0, 2, -3f);
            trans_mat.Set(1, 0, 0f);
            trans_mat.Set(1, 1, 1f);
            trans_mat.Set(1, 3, 0f);

            markers["normalcat"] = new Marker()
            {
                Img = new Mat(@"Markers\normalcat.png").Resize(new OpenCvSharp.Size(62, 62), 0, 0, InterpolationFlags.Linear).CvtColor(ColorConversionCodes.BGR2GRAY),
                Mask = new Mat(@"Markers\itemmask.png").Resize(new OpenCvSharp.Size(62, 62), 0, 0, InterpolationFlags.Linear).CvtColor(ColorConversionCodes.BGR2GRAY),
            };
            markers["unitedcat"] = new Marker()
            {
                Img = new Mat(@"Markers\unitedcat.png").Resize(new OpenCvSharp.Size(62, 62), 0, 0, InterpolationFlags.Linear).CvtColor(ColorConversionCodes.BGR2GRAY),
                Mask = new Mat(@"Markers\itemmask.png").Resize(new OpenCvSharp.Size(62, 62), 0, 0, InterpolationFlags.Linear).CvtColor(ColorConversionCodes.BGR2GRAY),
            };
            markers["supcat"] = new Marker()
            {
                Img = new Mat(@"Markers\supcat.png").Resize(new OpenCvSharp.Size(62, 62), 0, 0, InterpolationFlags.Linear).CvtColor(ColorConversionCodes.BGR2GRAY),
                Mask = new Mat(@"Markers\itemmask.png").Resize(new OpenCvSharp.Size(62, 62), 0, 0, InterpolationFlags.Linear).CvtColor(ColorConversionCodes.BGR2GRAY),
            };

            randomTemplate = new Marker()
            {
                Img = new Mat(@"Markers\material.png").CvtColor(ColorConversionCodes.BGR2GRAY).Resize(new OpenCvSharp.Size(62, 62), 0, 0, InterpolationFlags.Linear),
            };
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Mat original = new Mat(@"Sample\Full2.png").CvtColor(ColorConversionCodes.BGR2GRAY);
            //CalculateDiffToRandomTemplate(original);
            FindCornersThenLookInSmallRoiScaled(ref original);
            Cv2.ImShow("debug", original);

            var frameTime = stopwatch.ElapsedMilliseconds;
            timingLabel.Text = frameTime.ToString();
        }

        private void captureButton_Click(object sender, EventArgs e)
        {
            timer.Interval = 3000;
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            var windowSize = new System.Drawing.Size(2560, 1440);
            Rectangle roi = new Rectangle(System.Drawing.Point.Empty, windowSize);

            Stopwatch stopwatch = Stopwatch.StartNew();

            var bmp = screenCapture.Capture(windowSize, roi);
            if (bmp != null)
            {
                var bits = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                try
                {
                    var original = new Mat(bits.Height, bits.Width, MatType.CV_8UC3, bits.Scan0, bits.Stride).CvtColor(ColorConversionCodes.BGR2GRAY);

                    //FindCornersThenLookInSmallRoi(original);
                    //FindOneThenExtendWithKnownDistance(original);
                    //CalculateFilter2DThenFindResult(original);
                    CalculateDiffToRandomTemplate(original);

                    Cv2.ImShow("debug", original);
                }
                finally
                {
                    bmp.UnlockBits(bits);
                }
            }

            var frameTime = stopwatch.ElapsedMilliseconds;
            timingLabel.Text = frameTime.ToString();
        }

        private void FindOneThenExtendWithKnownDistance(Mat original)
        {
            const int VERTICAL_SPACING = 11;
            const int HORIZONTAL_SPACING = 10;
            const int ICON_SIZE = 62;

            List<Point> points = null;

            // find first
            foreach (var marker in markers)
            {
                if (points == null)
                {
                    var position = LookForMarker(original, marker.Value, 0.1);

                    if (position.HasValue)
                    {
                        original.DrawMarker(position.Value, Scalar.White, MarkerTypes.Square, 25, 2);
                        Debug.WriteLine(marker.Key + ": " + position.Value);

                        using (Mat sub = original.SubMat(position.Value.Y, position.Value.Y + ICON_SIZE, position.Value.X, position.Value.X + ICON_SIZE))
                        {
                            Cv2.ImShow("first", sub);
                        }

                        if (points == null)
                        {
                            points = new List<Point>();

                            var startX = position.Value.X % (ICON_SIZE + HORIZONTAL_SPACING);
                            var startY = position.Value.Y % (ICON_SIZE + VERTICAL_SPACING);

                            for (int x = startX; x < original.Cols - ICON_SIZE; x += ICON_SIZE + HORIZONTAL_SPACING)
                            {
                                for (int y = startY; y < original.Rows - ICON_SIZE; y += ICON_SIZE + VERTICAL_SPACING)
                                {
                                    points.Add(new Point(x, y));
                                }
                            }

                            points.Remove(position.Value);
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"searching {points.Count}");
                    Point pt = new Point();
                    double minDiff = 10000000;

                    foreach (var point in points)
                    {
                        using (Mat sub = original.SubMat(point.Y, point.Y + ICON_SIZE, point.X, point.X + ICON_SIZE))
                        {
                            var diff = Cv2.Norm(marker.Value.Img, sub, NormTypes.L1);
                            if (diff < minDiff)
                            {
                                minDiff = diff;
                                pt = point;
                                Cv2.ImShow("item", sub);
                            }
                        }
                        
                    }

                    Debug.WriteLine($"mindiff {minDiff}");
                    original.DrawMarker(pt, Scalar.White, MarkerTypes.Square, 25, 2);
                }
            }
        }

        private void FindCornersThenLookInSmallRoi(Mat original)
        {
            var ulPosition = LookForMarker(original, boundsMarkers["upperleft"], 0.1);
            if (ulPosition.HasValue)
            {
                var x = ulPosition.Value.X;
                var y = ulPosition.Value.Y;
                Rect newRoi = new Rect(x, y, original.Width - x, original.Height - y);
                original = original.SubMat(newRoi);
            }

            var lrPosition = LookForMarker(original, boundsMarkers["lowerright"], 0.1);
            if (lrPosition.HasValue)
            {
                var x = lrPosition.Value.X + boundsMarkers["lowerright"].Img.Width;
                var y = lrPosition.Value.Y + boundsMarkers["lowerright"].Img.Height;
                Rect newRoi = new Rect(0, 0, x, y);
                original = original.SubMat(newRoi);
            }

            //var sobelX = original.Sobel(MatType.CV_8UC1, 1, 0);
            //var sobelY = original.Sobel(MatType.CV_8UC1, 0, 1);

            //Cv2.ImShow("x", sobelX);
            //Cv2.ImShow("y", sobelY);

            foreach (var marker in markers)
            {
                var position = LookForMarker(original, marker.Value, 0.1);

                if (position.HasValue)
                {
                    original.DrawMarker(position.Value, Scalar.White, MarkerTypes.Square, 25, 2);
                    Debug.WriteLine(marker.Key + ": " + position.Value);
                }
            }
        }

        private void FindCornersThenLookInSmallRoiScaled(ref Mat original)
        {
            const float scale = 0.5f;

            original = original.Resize(OpenCvSharp.Size.Zero, scale, scale, InterpolationFlags.Linear);
            var markersScaled = markers.Select((pair) => (pair.Key, new Marker()
            {
                Img = pair.Value.Img.Resize(OpenCvSharp.Size.Zero, scale, scale, InterpolationFlags.Linear),
                Mask = pair.Value.Mask?.Resize(OpenCvSharp.Size.Zero, scale, scale, InterpolationFlags.Linear),
            })).ToDictionary(pair => pair.Key, pair => pair.Item2);

            foreach (var marker in markersScaled)
            {
                var position = LookForMarker(original, marker.Value, 0.1);

                if (position.HasValue)
                {
                    original.DrawMarker(position.Value, Scalar.White, MarkerTypes.Square, 25, 2);
                    Debug.WriteLine(marker.Key + ": " + position.Value);
                }
            }
        }

        private void CalculateDiffToRandomTemplate(Mat original)
        {
            Mat intermediate = randomTemplate.Img; // markers["normalcat"].Img;
            Mat originalDiffToRandom = original.MatchTemplate(intermediate, TemplateMatchModes.CCorrNormed);            

            foreach (var marker in markers)
            {
                Mat markerDiffToRandom = marker.Value.Img.MatchTemplate(intermediate, TemplateMatchModes.CCorrNormed);

                var value = markerDiffToRandom.Get<float>(0, 0);

                Mat diffResult = new Mat();
                Cv2.Absdiff(originalDiffToRandom, new Scalar(value), diffResult);

                Cv2.ImShow("marker", marker.Value.Img);
                Cv2.ImShow("diffResult", diffResult * 2000);
                Cv2.ImShow("diffToRandom", originalDiffToRandom);

                Cv2.MinMaxLoc(diffResult, out double minVal, out double maxVal, out Point minLoc, out Point maxLoc);

                original.DrawMarker(minLoc, Scalar.White, MarkerTypes.Square, 25, 2);

                Debug.WriteLine($"{minVal}");

                Cv2.MinMaxLoc(originalDiffToRandom, out double minVal2, out double maxVal2, out Point minLoc2, out Point maxLoc2);
                Debug.WriteLine($"{minVal2}");
            }
        }

        private void CalculateFilter2DThenFindResult(Mat original)
        {
            Mat kernel = new Mat(62, 62, MatType.CV_32FC1, new Scalar(0));
            for (int i = 0; i < 62; i++)
            {
                for (int j = 0; j < 62; j++)
                {
                    //kernel.Set(i, j, (float)(Random.Shared.NextDouble()));
                    kernel.Set(i, j, (float)1);
                }
            }
            //kernel.SetArray(new float[] { 
            //    0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
            //    0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
            //    0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
            //    0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
            //    0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
            //    0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
            //    0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
            //    0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
            //    0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
            //    0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
            //    0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
            //    0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, });
            //kernel.Rectangle(new Point(0, 31), new Point(61, 61), new Scalar(3), -1, LineTypes.Link4);
            //kernel.Rectangle(new Point(31, 31), new Point(61, 61), new Scalar(5), -1, LineTypes.Link4);
            //Scalar mysum = Cv2.Sum(kernel);
            //kernel /= mysum[0];
            Cv2.ImShow("kernel", kernel);

            Mat output = original.Filter2D(MatType.CV_32FC1, kernel, new Point(0, 0), 0, BorderTypes.Isolated);

            //Mat output = new Mat();
            //Cv2.Filter2D(original, output, MatType.CV_32FC1, kernel, new Point(0, 0), 0, BorderTypes.Isolated);
            Cv2.ImShow("filterOutput", output);

            Mat output2 = output / 255.0f;
            Cv2.ImShow("outputnorm", output2);

            foreach (var marker in markers)
            {
                var markerresult = marker.Value.Img.Filter2D(MatType.CV_32FC1, kernel, new Point(0, 0), 0, BorderTypes.Isolated);

                var value = markerresult.Get<float>(0, 0);

                Mat diffResult = new Mat();
                Cv2.Absdiff(output, new Scalar(value), diffResult);

                Cv2.ImShow("diffResult", diffResult / 255f);

                Cv2.MinMaxLoc(diffResult, out double minVal, out double maxVal, out Point minLoc, out Point maxLoc);

                original.DrawMarker(minLoc, Scalar.White, MarkerTypes.Square, 25, 2);

                Debug.WriteLine($"{minVal}");
            }
        }

        private Point? LookForMarker(Mat img, Marker marker, double confidenceThreshold = 0.3)
        {
            Point? markerPosition = null;
            var markerImg = marker.Img;

            //Mat output = img.CvtColor(ColorConversionCodes.BGR2GRAY);

            Mat searchImg = img;


            Mat result = new Mat();
            if (marker.Mask == null)
            {
                Cv2.MatchTemplate(searchImg, markerImg, result, TemplateMatchModes.SqDiffNormed);
            }
            else
            {
                Cv2.MatchTemplate(searchImg, markerImg, result, TemplateMatchModes.SqDiffNormed, marker.Mask);
            }

            Cv2.MinMaxLoc(result, out double minVal, out double maxVal, out Point minLoc, out Point maxLoc);

            //int x = minLoc.X;
            //int y = minLoc.Y;
            //int w = markerImg.Width;
            //int h = markerImg.Height;
            //Rect exclaimRect = new Rect(x, y, w, h);

            if (minVal < confidenceThreshold)
            {
                markerPosition = minLoc;
                Debug.WriteLine(minVal);
            }

            return markerPosition;
        }
    }

    public enum MarkerSearchStrategy
    {
        ToGray,
        RedChannel,
    }

    public class Marker
    {
        public Mat Img { get; set; }
        public Mat Mask { get; set; }

        public MarkerSearchStrategy SearchStrategy { get; set; } = MarkerSearchStrategy.ToGray;
    }
}