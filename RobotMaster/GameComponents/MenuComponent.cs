using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
//using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace RobotMaster.GameComponents
{
    public class MenuComponent
    {
        string[] menuItems;
        int selectedIndex;
        float width;
        float height;
        SpriteFont spriteFont;

        public Vector2 Position { get; set; }           // position where menu is drawn on screen
        public Color NormalColor { get; set; }
        public Color HighlightColor { get; set; }
        public float Width { get { return width; } }
        public float Height { get { return height; } }
        public int SelectedIndex { get { return selectedIndex; } }
        

        public MenuComponent(SpriteFont spriteFont, string[] items)
        {
            this.spriteFont = spriteFont;
            SetMenuItems(items);

            NormalColor = Color.White;
            HighlightColor = Color.Red;
        }

        public void SetMenuItems(string[] items)
        {
            menuItems = (String[])items.Clone();
            MeasureMenu();
        }

        private void MeasureMenu()
        {
            width = 0;
            height = 0;
            foreach (string s in menuItems)
            {
                if (width < spriteFont.MeasureString(s).X)
                {
                    width = spriteFont.MeasureString(s).X;
                }
                height += spriteFont.LineSpacing;
            }
        }

        public void Update()
        {
            if (InputManager.KeyPressed(Keys.Up) ||
                InputManager.ButtonPressed(PlayerIndex.One, Buttons.DPadUp) ||
                InputManager.ButtonPressed(PlayerIndex.One, Buttons.LeftThumbstickUp))
            {
                selectedIndex--;
                if (selectedIndex < 0)
                {
                    selectedIndex = menuItems.Length - 1;
                }
            }
            if (InputManager.KeyPressed(Keys.Down) ||
                InputManager.ButtonPressed(PlayerIndex.One, Buttons.DPadDown) ||
                InputManager.ButtonPressed(PlayerIndex.One, Buttons.LeftThumbstickDown))
            {
                selectedIndex++;
                if (selectedIndex >= menuItems.Length)
                {
                    selectedIndex = 0;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 menuPosition = Position;
            for (int i = 0; i < menuItems.Length; i++)
            {
                if (i == selectedIndex)
                {
                    spriteBatch.DrawString(spriteFont, menuItems[i], menuPosition, HighlightColor);
                }
                else
                {
                    spriteBatch.DrawString(spriteFont, menuItems[i], menuPosition, NormalColor);
                }
                menuPosition.Y += spriteFont.LineSpacing;
            }
        }
    }
}
