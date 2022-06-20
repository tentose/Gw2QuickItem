using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Settings;
using Blish_HUD.Settings.UI.Views;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickItem
{
    public enum IconSize
    {
        Ant = 20,
        Tiny = 36,
        Small = 45,
        Normal = 50,
        Large = 56,
        Larger = 62,
        Largest = 74,
    }

    public class GroupConfigPresenter : Presenter<GroupConfigView, ItemGroupInfo>
    {
        private SettingEntry<IconSize> _iconSizeSetting;

        public List<SettingEntry> GroupSettings { get; private set; } = new List<SettingEntry>();

        public GroupConfigPresenter(GroupConfigView view, ItemGroupInfo model) : base(view, model)
        {
        }

        protected override Task<bool> Load(IProgress<string> progress)
        {
            var nameSetting = SettingEntry<string>.InitSetting(Model.Name);
            nameSetting.GetDisplayNameFunc = () => "Name";
            nameSetting.GetDescriptionFunc = () => "Group name";
            GroupSettings.Add(nameSetting);
            nameSetting.SettingChanged += NameSetting_SettingChanged;

            // Use the first item as the icon size of the group
            var groupIconSize = IconSize.Larger;
            var firstItem = Model.Items.FirstOrDefault();
            if (firstItem != null)
            {
                groupIconSize = FindClosestSize(firstItem.IconSize);
            }
            _iconSizeSetting = SettingEntry<IconSize>.InitSetting(groupIconSize);
            _iconSizeSetting.GetDisplayNameFunc = () => "Icon Size";
            _iconSizeSetting.GetDescriptionFunc = () => "Icon size";
            GroupSettings.Add(_iconSizeSetting);
            _iconSizeSetting.SettingChanged += IconSizeSetting_SettingChanged;

            View.AddItemClicked += View_AddItemClicked;
            View.AddToLayoutClicked += View_AddToLayoutClicked;

            return base.Load(progress);
        }

        private IconSize FindClosestSize(int size)
        {
            var minDiff = int.MaxValue;
            IconSize minValue = IconSize.Larger;
            foreach (var iconSize in (IconSize[])Enum.GetValues(typeof(IconSize)))
            {
                var absDiff = Math.Abs(size - (int)iconSize);
                if (absDiff < minDiff)
                {
                    minDiff = absDiff;
                    minValue = iconSize;
                }
            }
            return minValue;
        }

        private void View_AddToLayoutClicked(object sender, EventArgs e)
        {
            QuickItemModule.Instance.LayoutContainer.ActiveLayout.Groups.Add(new GroupPosition() { GroupId = Model.Guid });
        }

        private void IconSizeSetting_SettingChanged(object sender, Blish_HUD.ValueChangedEventArgs<IconSize> e)
        {
            foreach (var item in Model.Items)
            {
                item.IconSize = (int)(e.NewValue);
            }
            UpdateView();
        }

        private void NameSetting_SettingChanged(object sender, Blish_HUD.ValueChangedEventArgs<string> e)
        {
            Model.Name = e.NewValue;
        }

        private void View_AddItemClicked(object sender, EventArgs e)
        {
            Model.Items.Add(new ItemIconInfo()
            {
                IconSize = (int)_iconSizeSetting.Value,
            });
            UpdateView();
        }

        protected override void UpdateView()
        {
            // Settings
            this.View.GroupSettings.ClearChildren();
            foreach (var setting in GroupSettings)
            {
                IView settingView;

                if ((settingView = SettingView.FromType(setting, this.View.GroupSettings.Width)) != null)
                {
                    var settingContainer = new ViewContainer()
                    {
                        WidthSizingMode = SizingMode.Fill,
                        HeightSizingMode = SizingMode.AutoSize,
                        Parent = this.View.GroupSettings,
                    };
                    settingContainer.Show(settingView);
                }
            }

            // Group editor
            this.View.GroupEditor.ClearChildren();
            var group = new ItemIconGroup()
            {
                GroupInfo = Model,
                Parent = this.View.GroupEditor,
                AllowActivation = false,
                AllowEdit = true,
                DragMode = GroupDragMode.Item,
                Location = new Point(50, 50),
            };
        }
    }
}
