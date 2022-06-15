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
    /// <summary>
    /// Changed to refresh description on float and int setting views. This is a hack to show the current value on the slider.
    /// </summary>
    public class InternalSettingsView : SettingView<SettingCollection>
    {

        private FlowPanel _settingFlowPanel;

        private readonly SettingCollection _settings;

        private bool _lockBounds = true;

        public bool LockBounds
        {
            get => _lockBounds;
            set
            {
                if (_lockBounds == value) return;

                _lockBounds = value;

                UpdateBoundsLocking(_lockBounds);
            }
        }

        private ViewContainer _lastSettingContainer;

        public InternalSettingsView(SettingEntry<SettingCollection> setting, int definedWidth = -1) : base(setting, definedWidth)
        {
            _settings = setting.Value;
        }
        public InternalSettingsView(SettingCollection settings, int definedWidth = -1)
            : this(new SettingEntry<SettingCollection>() { Value = settings }, definedWidth) { /* NOOP */ }

        private void UpdateBoundsLocking(bool locked)
        {
            if (_settingFlowPanel == null) return;

            _settingFlowPanel.ShowBorder = !locked;
            _settingFlowPanel.CanCollapse = !locked;
        }

        protected override void BuildSetting(Container buildPanel)
        {
            _settingFlowPanel = new FlowPanel()
            {
                Size = buildPanel.Size,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(5, 2),
                OuterControlPadding = new Vector2(10, 15),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                AutoSizePadding = new Point(0, 15),
                Parent = buildPanel
            };

            foreach (var setting in _settings.Where(s => s.SessionDefined))
            {
                IView settingView;

                if ((settingView = SettingView.FromType(setting, _settingFlowPanel.Width)) != null)
                {
                    _lastSettingContainer = new ViewContainer()
                    {
                        WidthSizingMode = SizingMode.Fill,
                        HeightSizingMode = SizingMode.AutoSize,
                        Parent = _settingFlowPanel
                    };

                    _lastSettingContainer.Show(settingView);

                    if (settingView is SettingsView subSettingsView)
                    {
                        subSettingsView.LockBounds = false;
                    }
                    else if (settingView is FloatSettingView floatView)
                    {
                        SubscribeDescriptionChangeOnValueChange(floatView, setting);
                    }
                    else if (settingView is IntSettingView intView)
                    {
                        SubscribeDescriptionChangeOnValueChange(intView, setting);
                    }
                }
            }

            UpdateBoundsLocking(_lockBounds);
        }

        private static void SubscribeDescriptionChangeOnValueChange<T>(SettingView<T> settingView, SettingEntry setting)
        {
            settingView.ValueChanged += (sender, args) =>
            {
                settingView.Description = setting.Description;
            };
        }

        protected override void RefreshDisplayName(string displayName)
        {
            _settingFlowPanel.Title = displayName;
        }

        protected override void RefreshDescription(string description)
        {
            _settingFlowPanel.BasicTooltipText = description;
        }

        protected override void RefreshValue(SettingCollection value) { /* NOOP */ }
    }
}
