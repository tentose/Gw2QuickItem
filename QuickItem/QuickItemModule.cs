using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
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
        private const string OPERATING_DIRECTORY = "quickitems";
        private const string ITEM_ICON_DIRECTORY = "itemicons";
        private const string GROUPS_DIRECTORY = "groups";
        private const string LAYOUTS_DIRECTORY = "layouts";
        private const string NATIVE_DIRECTORY = "dlls";

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

        public GroupCollection GroupCollection { get; private set; }
        public LayoutCollection LayoutCollection { get; private set; }
        
        public LayoutContainer LayoutContainer { get; private set; }

        private string _operatingDirectory;
        private Gw2Sharp.Gw2Client _gw2sharpClientForRender;
        private CornerIcon _searchIcon;
        private ManagementWindow _window;

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
                IconName = "Something",
                Icon = ContentsManager.GetTexture(@"Textures\CornerIcon.png"),
                HoverIcon = ContentsManager.GetTexture(@"Textures\CornerIconHover.png"),
                LoadingMessage = "maybe",
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

            GroupCollection = new GroupCollection();
            GroupCollection.InitializeFromDisk();

            LayoutCollection = new LayoutCollection();
            LayoutCollection.InitializeFromDisk();

            LayoutContainer = new LayoutContainer(GroupCollection)
            {
                ActiveLayout = LayoutCollection[0],
                Parent = GameService.Graphics.SpriteScreen,
            };

            _window = new ManagementWindow();

            var nativeDllPath = Path.Combine(NativeDirectory, "ItemFinder.dll");
            if (!File.Exists(nativeDllPath))
            {
                using (var inStream = ContentsManager.GetFileStream(@"dll\ItemFinder.dll"))
                {
                    using (var outStream = File.OpenWrite(nativeDllPath))
                    {
                        inStream.CopyTo(outStream);
                    }
                }
            }


            using (var inStream = ContentsManager.GetFileStream(@"Textures\itemmask.png"))
            {
                using (var outStream = File.OpenWrite(Path.Combine(ItemIconDirectory, "itemmask.png")))
                {
                    inStream.CopyTo(outStream);
                }
            }

            //OpenCvSharp.Internal.WindowsLibraryLoader.Instance.AdditionalPaths.Add(NativeDirectory);
            ItemFinderNative.Instance.SetGw2Window(GameService.GameIntegration.Gw2Instance.Gw2WindowHandle);

            var renderConnection = new Gw2Sharp.Connection();
            renderConnection.RenderCacheMethod = new Gw2Sharp.WebApi.Caching.MemoryCacheMethod();
            _gw2sharpClientForRender = new Gw2Sharp.Gw2Client(renderConnection);
            RenderClient = _gw2sharpClientForRender.WebApi.Render;

            _searchIcon.Click += _searchIcon_Click;
        }

        private void _searchIcon_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            _window.ToggleWindow();
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            GlobalSettings = new GlobalSettings(settings);
        }
    }
}
