using Blish_HUD.Input;
using Blish_HUD.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickItem
{
    public class GlobalSettings
    {
        public SettingEntry<string> ActiveLayout { get; private set; }
        public SettingEntry<KeyBinding> VisibilityHotkey { get; private set; }
        public SettingEntry<double> SearchImageScale { get; private set; }
        public SettingEntry<double> SearchAcceptThreshold { get; private set; }

        public GlobalSettings(SettingCollection settings)
        {
            VisibilityHotkey = settings.DefineSetting(
                                "VisibilityHotkey",
                                new KeyBinding(),
                                () => Strings.Settings_VisibilityHotkey_Name,
                                () => Strings.Settings_VisibilityHotkey_Description);
            VisibilityHotkey.Value.BlockSequenceFromGw2 = true;
            VisibilityHotkey.Value.Enabled = true;

            ActiveLayout = settings.DefineSetting("ActiveLayout",
                                "",
                                () => Strings.Settings_ActiveLayout_Name,
                                () => Strings.Settings_ActiveLayout_Description);

            SearchImageScale = settings.DefineSetting("SearchImageScale",
                                0.6,
                                () => Strings.Settings_SearchImageScale_Name,
                                () => Strings.Settings_SearchImageScale_Description);

            SearchAcceptThreshold = settings.DefineSetting("SearchAcceptThreshold",
                    0.1,
                    () => Strings.Settings_SearchAcceptThreshold_Name,
                    () => Strings.Settings_SearchAcceptThreshold_Description);
        }
    }
}
