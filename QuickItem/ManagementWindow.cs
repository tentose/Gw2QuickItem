using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Settings.UI.Views;
using Microsoft.Xna.Framework;
using QuickItem.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickItem
{
    public class ManagementWindow : TabbedWindow2
    {
        public ManagementWindow() : base(QuickItemModule.Instance.ContentsManager.GetTexture(@"Textures\155985.png"),
                                            new Rectangle(35, 36, 930,      640),
                                            new Rectangle(95, 42, 813 + 38, 592)
                                            )
        {
            Title = Strings.SettingsWindow_Title;
            Parent = GameService.Graphics.SpriteScreen;
            Id = $"{nameof(ManagementWindow)}_{nameof(QuickItemModule)}_6f6df496-b40a-4155-b598-f14b6c4c5d0c";
            SavesPosition = true;
            Emblem = QuickItemModule.Instance.ContentsManager.GetTexture(@"Textures\WindowIcon.png");

            Tabs.Add(new Tab(Content.DatAssetCache.GetTextureFromAssetId(1508665), () => new HowToUseView(), Strings.SettingsWindow_InfoTabName));
            Tabs.Add(new Tab(Content.DatAssetCache.GetTextureFromAssetId(156706), () => new InternalSettingsView(QuickItemModule.Instance.GlobalSettings.Settings), Strings.SettingsWindow_SettingsTabName));
            Tabs.Add(new Tab(QuickItemModule.Instance.ContentsManager.GetTexture(@"Textures\Group.png"), () => new GroupsManagementView(QuickItemModule.Instance.GroupCollection), Strings.SettingsWindow_GroupsTabName));
            Tabs.Add(new Tab(QuickItemModule.Instance.ContentsManager.GetTexture(@"Textures\Layout.png"), () => new LayoutsManagementView(QuickItemModule.Instance.LayoutCollection), Strings.SettingsWindow_LayoutsTabName));

            this.Shown += ManagementWindow_Shown;
            this.Hidden += ManagementWindow_Hidden;
        }

        private void ManagementWindow_Hidden(object sender, EventArgs e)
        {
            QuickItemModule.Instance.LayoutContainer.AllowEdit = false;
        }

        private void ManagementWindow_Shown(object sender, EventArgs e)
        {
            QuickItemModule.Instance.LayoutContainer.AllowEdit = true;
        }
    }
}
