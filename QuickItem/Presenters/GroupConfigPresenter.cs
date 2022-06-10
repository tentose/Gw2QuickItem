﻿using Blish_HUD.Controls;
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
        Tiny = 41,
        Small = 51,
        Normal = 61,
        Large = 71,
    }

    public class GroupConfigPresenter : Presenter<GroupConfigView, ItemGroupInfo>
    {
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

            var iconSizeSetting = SettingEntry<IconSize>.InitSetting(IconSize.Normal);
            iconSizeSetting.GetDisplayNameFunc = () => "Icon Size";
            iconSizeSetting.GetDescriptionFunc = () => "Icon size";
            GroupSettings.Add(iconSizeSetting);
            iconSizeSetting.SettingChanged += IconSizeSetting_SettingChanged;

            View.AddItemClicked += View_AddItemClicked;
            View.AddToLayoutClicked += View_AddToLayoutClicked;

            return base.Load(progress);
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
            Model.Items.Add(new ItemIconInfo());
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
                DragMode = GroupDragMode.Item,
                Location = new Point(50, 50),
            };
        }
    }
}