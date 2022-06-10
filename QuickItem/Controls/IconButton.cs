using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Blish_HUD.Content;
using Blish_HUD.Input;
using Blish_HUD.Graphics;
using Blish_HUD.Controls;
using Blish_HUD;

namespace QuickItem.Controls
{
    public class IconButton : Control
    {
        private const float DARKEN_MULTIPLIER = 0.8f;

        private Color m_multiplierWhenNotHovered = Color.White;
        private bool m_darkenUnlessHovered = false;
        public bool DarkenUnlessHovered
        {
            get => m_darkenUnlessHovered;
            set
            {
                m_darkenUnlessHovered = value;
                m_multiplierWhenNotHovered = m_darkenUnlessHovered ? Color.White * DARKEN_MULTIPLIER : Color.White;
            }
        }

        private AsyncTexture2D m_icon;
        public AsyncTexture2D Icon
        {
            get => m_icon;
            set => SetProperty(ref m_icon, value);
        }

        private AsyncTexture2D m_hoverIcon;
        public AsyncTexture2D HoverIcon
        {
            get => m_hoverIcon;
            set => SetProperty(ref m_hoverIcon, value);
        }

        public IconButton()
        {
        }

        public IconButton(AsyncTexture2D icon) : this()
        {
            m_icon = icon;
        }

        public IconButton(AsyncTexture2D icon, AsyncTexture2D hoverIcon) : this(icon)
        {
            m_hoverIcon = hoverIcon;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            Content.PlaySoundEffectByName(@"button-click");

            base.OnClick(e);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (m_icon != null)
            {
                if (this.MouseOver && this.Enabled)
                {
                    spriteBatch.DrawOnCtrl(this, m_hoverIcon ?? m_icon, bounds);
                }
                else
                {
                    spriteBatch.DrawOnCtrl(this, m_icon, bounds, m_multiplierWhenNotHovered);
                }
            }
        }
    }
}
