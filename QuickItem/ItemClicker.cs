using Blish_HUD;
using Blish_HUD.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QuickItem
{
    public class ItemClicker
    {
        private static readonly Logger Logger = Logger.GetLogger<QuickItemModule>();

        private GlobalSettings _settings;
        private SemaphoreSlim _finderAccess;

        public ItemClicker()
        {
            _settings = QuickItemModule.Instance.GlobalSettings;
            _finderAccess = new SemaphoreSlim(1, 1);
        }

        public async void ClickItem(int assetId)
        {
            //const float sizeMultiplier = 0.90625f;
            if (!await _finderAccess.WaitAsync(10000))
            {
                Logger.Warn("Timed out waiting for finder access");
                return;
            }

            try
            {
                PressInventoryKey();
                await Task.Delay(_settings.WaitForInventoryOpen.Value);

                var point = ItemFinderNative.Instance.FindItem(assetId);

                if (point.HasValue)
                {
                    var clickPoint = point.Value;
                    Blish_HUD.Controls.Intern.Mouse.DoubleClick(Blish_HUD.Controls.Intern.MouseButton.LEFT, clickPoint.X, clickPoint.Y);
                    Blish_HUD.Controls.Intern.Mouse.Release(Blish_HUD.Controls.Intern.MouseButton.LEFT, clickPoint.X, clickPoint.Y);
                }
                else
                {
                    ScreenNotification.ShowNotification(Strings.Notification_CantFindItem, ScreenNotification.NotificationType.Error);
                }

                await Task.Delay(50);
                PressInventoryKey();
            }
            catch (Exception e)
            {
                Logger.Error(e, "Click item failed");
            }
            finally
            {
                _finderAccess.Release();
            }
        }

        private void PressInventoryKey()
        {
            var primaryKey = _settings.InventoryKeybind.Value.PrimaryKey;
            var modifierKey = _settings.InventoryKeybind.Value.ModifierKeys;

            if (modifierKey != Microsoft.Xna.Framework.Input.ModifierKeys.None)
            {
                Blish_HUD.Controls.Intern.Keyboard.Press((Blish_HUD.Controls.Extern.VirtualKeyShort)modifierKey);
            }

            Blish_HUD.Controls.Intern.Keyboard.Stroke((Blish_HUD.Controls.Extern.VirtualKeyShort)primaryKey);

            if (modifierKey != Microsoft.Xna.Framework.Input.ModifierKeys.None)
            {
                Blish_HUD.Controls.Intern.Keyboard.Release((Blish_HUD.Controls.Extern.VirtualKeyShort)modifierKey);
            }
        }
    }
}
