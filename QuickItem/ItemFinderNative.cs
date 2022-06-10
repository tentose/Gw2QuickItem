using Blish_HUD;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QuickItem
{

    /*
     *
     *
typedef void(CALLBACK * LogCallback)(LogLevel, PCWSTR);

ITEMFINDER_API HRESULT InitializeItemFinder(LogCallback logCallback);

ITEMFINDER_API HRESULT SetWindow(HWND hwndGame);

ITEMFINDER_API HRESULT AddMarker(uint32_t id, PCWSTR path, PCWSTR maskPath, SearchMode mode);
ITEMFINDER_API HRESULT RemoveMarker(uint32_t id);

ITEMFINDER_API HRESULT SetParameters(uint32_t itemSize, double searchScale);
ITEMFINDER_API HRESULT InvalidateSession();
ITEMFINDER_API HRESULT FindMarker(uint32_t id, double threshold, Point* markerPosition);
     * 
     */



    public class ItemFinderNative
    {
        private static readonly Logger Logger = Logger.GetLogger<QuickItemModule>();

        enum SearchMode
        {
            ToGray,
            RedOnly,
            BlueOnly,
            GreenOnly,
            Count,
        }

        enum LogLevel
        {
            Error,
            Warn,
            Info,
            Verbose,
        }

        [StructLayout(LayoutKind.Sequential)]
        struct Point
        {
            public int X;
            public int Y;
        }

        private delegate void LogCallback(LogLevel level, [MarshalAs(UnmanagedType.LPWStr)] string message);

        private static void LogString(LogLevel level, string message)
        {
            switch (level)
            {
                case LogLevel.Error:
                    Logger.Error(message);
                    break;
                case LogLevel.Warn:
                    Logger.Warn(message);
                    break;
                case LogLevel.Info:
                    Logger.Info(message);
                    break;
                case LogLevel.Verbose:
                    Logger.Debug(message);
                    break;
            }
        }

        [DllImport("Kernel32.dll")]
        private static extern IntPtr LoadLibrary(string path);

        [DllImport("ItemFinder.dll", CharSet = CharSet.Unicode, PreserveSig = false, SetLastError = false, ExactSpelling = true)]
        private static extern void InitializeItemFinder(LogCallback logCallback);

        [DllImport("ItemFinder.dll", CharSet = CharSet.Unicode, PreserveSig = false, SetLastError = false, ExactSpelling = true)]
        private static extern void SetWindow(IntPtr hwnd);

        [DllImport("ItemFinder.dll", CharSet = CharSet.Unicode, PreserveSig = false, SetLastError = false, ExactSpelling = true)]
        private static extern void AddMarker(uint id, string path, string maskPath, SearchMode mode);

        [DllImport("ItemFinder.dll", CharSet = CharSet.Unicode, PreserveSig = false, SetLastError = false, ExactSpelling = true)]
        private static extern void RemoveMarker(uint id);

        [DllImport("ItemFinder.dll", CharSet = CharSet.Unicode, PreserveSig = false, SetLastError = false, ExactSpelling = true)]
        private static extern void SetParameters(uint itemSize, double searchScale);

        [DllImport("ItemFinder.dll", CharSet = CharSet.Unicode, PreserveSig = false, SetLastError = false, ExactSpelling = true)]
        private static extern void InvalidateSession();

        [DllImport("ItemFinder.dll", CharSet = CharSet.Unicode, PreserveSig = false, SetLastError = false, ExactSpelling = true)]
        private static extern void FindMarker(uint id, double threshold, out Point markerPosition);

        private static ItemFinderNative _instance = null;
        public static ItemFinderNative Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ItemFinderNative();
                }
                return _instance;
            }
        }

        private uint _itemSize = 62;
        private double _searchScale = 0.6;
        private double _searchThreshold = 0.1;
        private SortedSet<uint> _items = new SortedSet<uint>();
        private LogCallback callbackInstance;

        private ItemFinderNative()
        {
            // Force load the native dll with the known (but dynamic) path first so DllImport can find it later without having to search on disk
            var dllPath = Path.Combine(QuickItemModule.Instance.NativeDirectory, "ItemFinder.dll");
            var module = LoadLibrary(dllPath);

            // Keep a reference to the delegate so it doesn't get GC'ed
            callbackInstance = LogString;
            InitializeItemFinder(callbackInstance);

            QuickItemModule.Instance.GlobalSettings.SearchAcceptThreshold.SettingChanged += SearchAcceptThreshold_SettingChanged;
            QuickItemModule.Instance.GlobalSettings.SearchImageScale.SettingChanged += SearchImageScale_SettingChanged;
            GameService.Gw2Mumble.UI.UISizeChanged += UI_UISizeChanged;

            _itemSize = UiSizeToItemSize(GameService.Gw2Mumble.UI.UISize);
            _searchScale = QuickItemModule.Instance.GlobalSettings.SearchImageScale.Value;
            _searchThreshold = QuickItemModule.Instance.GlobalSettings.SearchAcceptThreshold.Value;

            UpdateSettings();
        }

        private uint UiSizeToItemSize(Gw2Sharp.Mumble.Models.UiSize uiSize)
        {
            uint itemSize = 62;
            switch (uiSize)
            {
                case Gw2Sharp.Mumble.Models.UiSize.Larger:
                    itemSize = 62;
                    break;
                case Gw2Sharp.Mumble.Models.UiSize.Large:
                    itemSize = 56;
                    break;
                case Gw2Sharp.Mumble.Models.UiSize.Normal:
                    itemSize = 50;
                    break;
                case Gw2Sharp.Mumble.Models.UiSize.Small:
                    itemSize = 45;
                    break;
            }

            return itemSize;
        }

        private void UI_UISizeChanged(object sender, ValueEventArgs<Gw2Sharp.Mumble.Models.UiSize> e)
        {
            _itemSize = UiSizeToItemSize(e.Value);
            UpdateSettings();
        }

        private void SearchImageScale_SettingChanged(object sender, ValueChangedEventArgs<double> e)
        {
            _searchScale = e.NewValue;
            UpdateSettings();
        }

        private void SearchAcceptThreshold_SettingChanged(object sender, ValueChangedEventArgs<double> e)
        {
            _searchThreshold = e.NewValue;
        }

        public void SetGw2Window(IntPtr hwnd)
        {
            SetWindow(hwnd);
        }

        public void AddItem(int assetId)
        {
            uint uassetId = (uint)assetId;
            if (!_items.Contains(uassetId) && assetId != 0)
            {
                AddMarker(uassetId, ContentService.Content.DatAssetCache.GetLocalTexturePath(assetId), Path.Combine(QuickItemModule.Instance.ItemIconDirectory, "itemmask.png"), SearchMode.ToGray);
                _items.Add(uassetId);
            }
        }

        public void RemoveItem(int assetId)
        {
            uint uassetId = (uint)assetId;
            if (_items.Contains(uassetId))
            {
                RemoveMarker(uassetId);
                _items.Remove(uassetId);
            }
        }
        public void ClearItems()
        {
            foreach (var id in _items)
            {
                RemoveMarker(id);
            }
            _items.Clear();
        }

        public void UpdateSettings()
        {
            SetParameters(_itemSize, _searchScale);
        }

        public Microsoft.Xna.Framework.Point? FindItem(int assetId)
        {
            Logger.Info($"Looking for item asset ID {assetId}");
            InvalidateSession();
            uint uassetId = (uint)assetId;
            
            Microsoft.Xna.Framework.Point? xnaPoint = null;
            if (_items.Contains(uassetId))
            {
                Point point = new Point();
                FindMarker(uassetId, _searchThreshold, out point);

                if (point.X >= 0)
                {
                    xnaPoint = new Microsoft.Xna.Framework.Point((int)(point.X / _searchScale + _itemSize / 2), (int)(point.Y / _searchScale + _itemSize / 2));
                    Logger.Info($"Found item asset ID {assetId} at {xnaPoint.Value.X},{xnaPoint.Value.Y}");
                }
                else
                {
                    Logger.Info($"Didn't find item asset ID {assetId}");
                }
            }
            else
            {
                Logger.Warn($"Item asset ID {assetId} not registered");
            }

            return xnaPoint;
        }
    }
}
