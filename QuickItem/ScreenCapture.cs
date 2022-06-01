using System;
using System.Drawing;
using System.Runtime.InteropServices;

public class ScreenCapture
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

    public ScreenCapture()
    {
    }

    public static Bitmap Capture()
    {
        Rectangle bounds = GetForegroundWindowRect();

        //CursorPosition = new Point(Cursor.Position.X - rect.Left, Cursor.Position.Y - rect.Top);

        if (bounds.Width == 0 || bounds.Height == 0)
        {
            return null;
        }

        var result = new Bitmap(bounds.Width, bounds.Height);

        using (var g = Graphics.FromImage(result))
        {
            g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
        }

        return result;
    }

    public static Bitmap Capture(Size sizeRequirement, Rectangle roi)
    {
        Rectangle bounds = GetForegroundWindowRect();

        if (bounds.Width != sizeRequirement.Width || bounds.Height != sizeRequirement.Height)
        {
            return null;
        }

        var result = new Bitmap(roi.Width, roi.Height);

        using (var g = Graphics.FromImage(result))
        {
            g.CopyFromScreen(new Point(bounds.Left + roi.Left, bounds.Top + roi.Top), Point.Empty, roi.Size);
        }

        return result;
    }

    public static Bitmap Capture(OpenCvSharp.Size sizeRequirement, OpenCvSharp.Rect roi)
    {
        Size drawingSizeRequirement = new Size(sizeRequirement.Width, sizeRequirement.Height);
        Rectangle drawingRoi = new Rectangle(roi.X, roi.Y, roi.Width, roi.Height);
        return Capture(drawingSizeRequirement, drawingRoi);
    }

    public static Rectangle GetForegroundWindowRect()
    {
        var foregroundWindowsHandle = GetForegroundWindow();
        var rect = new Rect();
        GetWindowRect(foregroundWindowsHandle, ref rect);

        return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
    }

    public static OpenCvSharp.Rect GetForegroundWindowRectCv()
    {
        var drawingRect = GetForegroundWindowRect();
        return new OpenCvSharp.Rect(drawingRect.X, drawingRect.Y, drawingRect.Width, drawingRect.Height);
    }
}
