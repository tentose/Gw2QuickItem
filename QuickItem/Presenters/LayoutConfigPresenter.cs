using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
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
    public class LayoutConfigPresenter : Presenter<LayoutConfigView, LayoutInfo>
    {
        public List<SettingEntry> LayoutSettings { get; private set; } = new List<SettingEntry>();

        public LayoutConfigPresenter(LayoutConfigView view, LayoutInfo model) : base(view, model)
        {
        }

        protected override Task<bool> Load(IProgress<string> progress)
        {
            var nameSetting = SettingEntry<string>.InitSetting(Model.Name);
            nameSetting.GetDisplayNameFunc = () => "Name";
            nameSetting.GetDescriptionFunc = () => "Layout name";
            LayoutSettings.Add(nameSetting);
            nameSetting.SettingChanged += NameSetting_SettingChanged;

            var keybindSetting = SettingEntry<KeyBinding>.InitSetting(Model.KeyBind);
            keybindSetting.GetDisplayNameFunc = () => "Keybind";
            keybindSetting.GetDescriptionFunc = () => "Keybind to activate this layout";
            //LayoutSettings.Add(keybindSetting);
            // Keybinding SettingEntry doesn't fire SettingChanged events. Watch the view's event as a workaround
            //keybindSetting.SettingChanged += KeybindSetting_SettingChanged;

            // Set the current active layout to this layout
            QuickItemModule.Instance.LayoutContainer.ActiveLayout = Model;

            return base.Load(progress);
        }

        private void KeyBindingSettingView_ValueChanged(object sender, Blish_HUD.ValueEventArgs<KeyBinding> e)
        {
            Model.KeyBind = e.Value;
        }

        private void NameSetting_SettingChanged(object sender, Blish_HUD.ValueChangedEventArgs<string> e)
        {
            Model.Name = e.NewValue;
        }

        protected override void UpdateView()
        {
            // Settings
            this.View.LayoutSettings.ClearChildren();
            foreach (var setting in LayoutSettings)
            {
                IView settingViewInterface = null;
                if (setting.SettingType == typeof(KeyBinding))
                {
                    // Keybinding settings need special handling as they don't fire SettingChanged events
                    // Watch for ValueChanged on the view instead.
                    var settingView = new KeybindingSettingView(setting as SettingEntry<KeyBinding>, this.View.LayoutSettings.Width);
                    settingView.ValueChanged += KeyBindingSettingView_ValueChanged;
                    settingViewInterface = settingView;
                }
                else
                {
                    settingViewInterface = SettingView.FromType(setting, this.View.LayoutSettings.Width);
                }

                if (settingViewInterface != null)
                {
                    var settingContainer = new ViewContainer()
                    {
                        WidthSizingMode = SizingMode.Fill,
                        HeightSizingMode = SizingMode.AutoSize,
                        Parent = this.View.LayoutSettings,
                    };
                    settingContainer.Show(settingViewInterface);
                }
            }
        }
    }
}
