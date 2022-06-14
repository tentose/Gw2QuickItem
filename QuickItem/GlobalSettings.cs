﻿using Blish_HUD.Input;
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
        public SettingCollection Settings { get; private set; }

        public SettingEntry<string> ActiveLayout { get; private set; }
        public SettingEntry<KeyBinding> InventoryKeybind { get; private set; }
        public SettingEntry<int> WaitForInventoryOpen { get; private set; }
        public SettingEntry<float> SearchImageScale { get; private set; }
        public SettingEntry<float> SearchAcceptThreshold { get; private set; }
        public SettingEntry<bool> OutputDebugImages { get; private set; }

        public GlobalSettings(SettingCollection settings)
        {
            Settings = settings;

            InventoryKeybind = settings.DefineSetting(
                                "InventoryKeybind",
                                new KeyBinding(Microsoft.Xna.Framework.Input.Keys.I),
                                () => Strings.Settings_InventoryKeybind_Name,
                                () => Strings.Settings_InventoryKeybind_Description);
            InventoryKeybind.Value.BlockSequenceFromGw2 = false;
            InventoryKeybind.Value.Enabled = true;

            WaitForInventoryOpen = settings.DefineSetting("WaitForInventoryOpen",
                                    300,
                                    () => Strings.Settings_WaitForInventoryOpen_Name,
                                    () => Strings.Settings_WaitForInventoryOpen_Description);
            WaitForInventoryOpen.SetRange(100, 1000);

            SearchImageScale = settings.DefineSetting("SearchImageScale",
                                0.6f,
                                () => Strings.Settings_SearchImageScale_Name,
                                () => Strings.Settings_SearchImageScale_Description);
            SearchImageScale.SetRange(0.3f, 1.0f);

            SearchAcceptThreshold = settings.DefineSetting("SearchAcceptThreshold",
                                0.1f,
                                () => Strings.Settings_SearchAcceptThreshold_Name,
                                () => Strings.Settings_SearchAcceptThreshold_Description);
            SearchAcceptThreshold.SetRange(0.01f, 0.2f);

            OutputDebugImages = settings.DefineSetting("OutputDebugImages",
                                    false,
                                    () => Strings.Settings_OutputDebugImages_Name,
                                    () => Strings.Settings_OutputDebugImages_Description);

            ActiveLayout = settings.DefineSetting("ActiveLayout",
                                "",
                                () => Strings.Settings_ActiveLayout_Name,
                                () => Strings.Settings_ActiveLayout_Description);
            ActiveLayout.SetDisabled();
        }
    }
}
