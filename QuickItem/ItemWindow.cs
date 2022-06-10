using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickItem
{
    internal class ItemWindow : StandardWindow
    {
        private StandardButton m_useButton;
        private StandardButton m_calibrateButton;
        private Point m_position = Point.Zero;

        private ItemIconGroup m_group = null;

        public ItemWindow() : base(QuickItemModule.Instance.ContentsManager.GetTexture("Textures/155985.png"), new Rectangle(40, 26, 300, 300), new Rectangle(70, 71, 350, 350))
        {
            Parent = GameService.Graphics.SpriteScreen;
            Id = $"{nameof(ItemWindow)}_{nameof(QuickItemModule)}_5f05a7af-8a00-45d4-87c2-511cddb418fc";
            

            m_useButton = new StandardButton()
            {
                Text = "use item",
                Parent = this,
                Location = new Point(0, 0),
            };
            m_useButton.Click += M_button_Click;
            //m_useButton.LeftMouseButtonReleased += M_useButton_LeftMouseButtonReleased;

            m_calibrateButton = new StandardButton()
            {
                Text = "calibrate",
                Parent = this,
                Location = new Point(0, 100),
            };
            m_calibrateButton.Click += M_calibrateButton_Click;
        }



        private void M_calibrateButton_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            Blish_HUD.GameService.Input.Mouse.LeftMouseButtonReleased += Mouse_LeftMouseButtonReleased;
        }

        private void Mouse_LeftMouseButtonReleased(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            Blish_HUD.GameService.Input.Mouse.LeftMouseButtonReleased -= Mouse_LeftMouseButtonReleased;
            m_position = e.MousePosition;
            m_calibrateButton.Text = String.Format("{0},{1}", m_position.X, m_position.Y);

            if (m_group == null)
            {
                m_group = new ItemIconGroup()
                {
                    Parent = GameService.Graphics.SpriteScreen,
                };

                var item = new ItemIcon()
                {
                    Parent = m_group,
                };
                var item2 = new ItemIcon()
                {
                    Parent = m_group,
                };
                var item3 = new ItemIcon()
                {
                    Parent = m_group,
                };
                var item4 = new ItemIcon()
                {
                    Parent = m_group,
                };
                var item5 = new ItemIcon()
                {
                    Parent = m_group,
                };
                var item6 = new ItemIcon()
                {
                    Parent = m_group,
                };
            }
            else
            {
                m_group.Parent = null;
                m_group = null;
            }
        }

        private void M_button_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            CanClose = false;
            Title = "";

            //if (m_group != null)
            //{
            //    var data = ItemIconGroup.Serialize(m_group);
            //    var path = Path.Combine(QuickItemModule.Instance.GroupsDirectory, "group.json");
            //    File.WriteAllText(path, data);
            //}
            //else
            //{
            //    var path = Path.Combine(QuickItemModule.Instance.GroupsDirectory, "group.json");
            //    var data = File.ReadAllText(path);
            //    m_group = ItemIconGroup.Deserialize(data);
            //    m_group.Parent = GameService.Graphics.SpriteScreen;

            //    m_group.Invalidate();
            //}

            Task.Run(async () =>
            {
                //Blish_HUD.Controls.Intern.Keyboard.Stroke(Blish_HUD.Controls.Extern.VirtualKeyShort.KEY_I);
                await Task.Delay(500);
                //Blish_HUD.Controls.Intern.Mouse.DoubleClick(Blish_HUD.Controls.Intern.MouseButton.LEFT, m_position.X, m_position.Y);
                //Blish_HUD.Controls.Intern.Keyboard.Stroke(Blish_HUD.Controls.Extern.VirtualKeyShort.KEY_I);
            });
        }

        private void M_useButton_LeftMouseButtonReleased(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            Task.Run(async () =>
            {
                Blish_HUD.Controls.Intern.Keyboard.Stroke(Blish_HUD.Controls.Extern.VirtualKeyShort.KEY_I);
                await Task.Delay(250);
                Blish_HUD.Controls.Intern.Mouse.DoubleClick(Blish_HUD.Controls.Intern.MouseButton.LEFT, m_position.X, m_position.Y);
                Blish_HUD.Controls.Intern.Mouse.Release(Blish_HUD.Controls.Intern.MouseButton.LEFT, m_position.X, m_position.Y);
                await Task.Delay(10);
                Blish_HUD.Controls.Intern.Keyboard.Stroke(Blish_HUD.Controls.Extern.VirtualKeyShort.KEY_I);
            });
        }
    }
}
