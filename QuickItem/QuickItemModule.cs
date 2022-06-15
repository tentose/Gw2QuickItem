using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Newtonsoft.Json;
using QuickItem.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickItem
{
    [Export(typeof(Module))]
    public class QuickItemModule : Module
    {
        private static readonly Logger Logger = Logger.GetLogger<QuickItemModule>();

        private const string OPERATING_DIRECTORY = "quickitems";
        private const string ITEM_ICON_DIRECTORY = "itemicons";
        private const string GROUPS_DIRECTORY = "groups";
        private const string LAYOUTS_DIRECTORY = "layouts";
        private const string NATIVE_DIRECTORY = "dlls";
        private const string CACHE_DIRECTORY = "cache";
        private const string DEBUG_DIRECTORY = "debug";
        private const string STATIC_ITEMS_FILE_NAME = "all_items.json";
        private const string CACHE_VERSION_FILE_NAME = "cache_version.json";

        private SettingsManager m_settingsManager => this.ModuleParameters.SettingsManager;
        public ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        public DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        public Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;
        public GlobalSettings GlobalSettings { get; private set; }
        public Gw2Sharp.WebApi.Render.IGw2WebApiRenderClient RenderClient { get; private set; }
        public string ItemIconDirectory { get; private set; }
        public string GroupsDirectory { get; private set; }
        public string LayoutsDirectory { get; private set; }
        public string NativeDirectory { get; private set; }
        public string CacheDirectory { get; private set; }
        public string DebugDirectory { get; private set; }

        public GroupCollection GroupCollection { get; private set; }
        public LayoutCollection LayoutCollection { get; private set; }
        
        public LayoutContainer LayoutContainer { get; private set; }

        public ItemClicker Clicker { get; private set; }

        private string _operatingDirectory;
        private Gw2Sharp.Gw2Client _gw2sharpClientForRender;
        private CornerIcon _searchIcon;
        private ManagementWindow _window;

        class ModuleVersion
        {
            public int Major { get; set; }
            public int Minor { get; set; }
            public int Patch { get; set; }
            public ModuleVersion(SemVer.Version v)
            {
                Major = v.Major;
                Minor = v.Minor;
                Patch = v.Patch;
            }
            public ModuleVersion()
            {
            }
        }

        public static QuickItemModule Instance;

        [ImportingConstructor]
        public QuickItemModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            Instance = this;
        }

        protected override async Task LoadAsync()
        {
            await ItemIcon.LoadIconResources();

            _searchIcon = new CornerIcon()
            {
                IconName = Strings.CornerIcon_Name,
                Icon = ContentsManager.GetTexture(@"Textures\CornerIcon.png"),
                HoverIcon = ContentsManager.GetTexture(@"Textures\CornerIconHover.png"),
                LoadingMessage = Strings.CornerIcon_Loading,
            };

            _operatingDirectory = DirectoriesManager.GetFullDirectoryPath(OPERATING_DIRECTORY);

            ItemIconDirectory = Path.Combine(_operatingDirectory, ITEM_ICON_DIRECTORY);
            Directory.CreateDirectory(ItemIconDirectory);

            GroupsDirectory = Path.Combine(_operatingDirectory, GROUPS_DIRECTORY);
            Directory.CreateDirectory(GroupsDirectory);

            LayoutsDirectory = Path.Combine(_operatingDirectory, LAYOUTS_DIRECTORY);
            Directory.CreateDirectory(LayoutsDirectory);

            NativeDirectory = Path.Combine(_operatingDirectory, NATIVE_DIRECTORY);
            Directory.CreateDirectory(NativeDirectory);

            CacheDirectory = Path.Combine(_operatingDirectory, CACHE_DIRECTORY);
            Directory.CreateDirectory(CacheDirectory);

            DebugDirectory = Path.Combine(_operatingDirectory, DEBUG_DIRECTORY);
            Directory.CreateDirectory(DebugDirectory);

            EnsureCacheVersion();

            EnsureFileCopiedFromArchive(@"dll\ItemFinder.dll", Path.Combine(NativeDirectory, "ItemFinder.dll"));
            EnsureFileCopiedFromArchive(@"Textures\itemmask.png", Path.Combine(ItemIconDirectory, "itemmask.png"));

            var localeDir = LocaleToPathString(GameService.Overlay.UserLocale.Value);
            var localeSpecificCacheDirectory = Path.Combine(CacheDirectory, localeDir);
            var staticItemsJsonPath = Path.Combine(localeSpecificCacheDirectory, STATIC_ITEMS_FILE_NAME);

            EnsureFileCopiedFromArchive(Path.Combine(localeDir, STATIC_ITEMS_FILE_NAME), staticItemsJsonPath);
            await StaticItemInfo.Initialize(staticItemsJsonPath);

            GroupCollection = new GroupCollection();
            GroupCollection.InitializeFromDisk();

            LayoutCollection = new LayoutCollection();
            LayoutCollection.InitializeFromDisk();

            string activeLayoutName = GlobalSettings.ActiveLayout.Value;
            var activeLayout = LayoutCollection.Where(layout => layout.Name == activeLayoutName).FirstOrDefault();
            if (activeLayout == null)
            {
                activeLayout = LayoutCollection[0];
            }

            LayoutContainer = new LayoutContainer(GroupCollection)
            {
                ActiveLayout = activeLayout,
                Parent = GameService.Graphics.SpriteScreen,
            };

            Clicker = new ItemClicker();

            _window = new ManagementWindow();

            //OpenCvSharp.Internal.WindowsLibraryLoader.Instance.AdditionalPaths.Add(NativeDirectory);
            ItemFinderNative.Instance.SetGw2Window(GameService.GameIntegration.Gw2Instance.Gw2WindowHandle);

            var renderConnection = new Gw2Sharp.Connection();
            renderConnection.RenderCacheMethod = new Gw2Sharp.WebApi.Caching.MemoryCacheMethod();
            _gw2sharpClientForRender = new Gw2Sharp.Gw2Client(renderConnection);
            RenderClient = _gw2sharpClientForRender.WebApi.Render;

            _searchIcon.Click += _searchIcon_Click;
            _searchIcon.LoadingMessage = null;
        }

        private string LocaleToPathString(Gw2Sharp.WebApi.Locale locale)
        {
            switch (locale)
            {
                case Gw2Sharp.WebApi.Locale.English: return "en";
                case Gw2Sharp.WebApi.Locale.French: return "fr";
                case Gw2Sharp.WebApi.Locale.German: return "de";
                case Gw2Sharp.WebApi.Locale.Spanish: return "es";
                case Gw2Sharp.WebApi.Locale.Chinese: return "zh";
                default: return "en";
            }
        }

        private void _searchIcon_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            _window.ToggleWindow();
        }

        private void EnsureFileCopiedFromArchive(string archivePath, string extractedPath)
        {
            if (!File.Exists(extractedPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(extractedPath));
                using (var inStream = ContentsManager.GetFileStream(archivePath))
                {
                    using (var outStream = File.OpenWrite(extractedPath))
                    {
                        inStream.CopyTo(outStream);
                    }
                }
            }
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            GlobalSettings = new GlobalSettings(settings);
        }

        public override IView GetSettingsView()
        {
            return new ModuleSettingsView(_window.Show);
        }

        private void EnsureCacheVersion()
        {
            bool shouldUpdateNativeDlls = true;
            var cacheVerFilePath = Path.Combine(CacheDirectory, CACHE_VERSION_FILE_NAME);
            try
            {
                if (File.Exists(cacheVerFilePath))
                {
                    var moduleVer = this.Version;
                    var cacheVer = JsonConvert.DeserializeObject<ModuleVersion>(File.ReadAllText(cacheVerFilePath));
                    // Native dlls should be updated with any version change
                    shouldUpdateNativeDlls = cacheVer.Major != moduleVer.Major || cacheVer.Minor != moduleVer.Minor || cacheVer.Patch != moduleVer.Patch;
                }
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Failed to compute cache version");
            }

            if (shouldUpdateNativeDlls)
            {
                Logger.Info("Cache version mismatch. Clearing cache.");
                try
                {
                    Directory.Delete(NativeDirectory, true);
                    Directory.CreateDirectory(NativeDirectory);

                    var currentVer = new ModuleVersion(this.Version);
                    File.WriteAllText(cacheVerFilePath, JsonConvert.SerializeObject(currentVer));
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to clear cache.");
                }
            }
        }
    }
}
