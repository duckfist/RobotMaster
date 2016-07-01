using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using RobotMaster.GameComponents;

namespace RobotMaster.Controls
{
    public class LeftRightSelector : Control
    {

        #region Fields and Properties

        List<string> items = new List<string>();
        Texture2D leftSelection;
        Texture2D rightSelection;
        Color highlightColor = Color.Red;
        int maxItemWidth;
        int selectedItem;


        public int SelectedIndex
        {
            get { return selectedItem; }
            set { selectedItem = (int)MathHelper.Clamp(value, 0f, items.Count); }
        }

        public string SelectedItem
        {
            get { return Items[selectedItem]; }
        }

        public List<string> Items
        {
            get { return items; }
        }

        #endregion

        #region Constructors

        public LeftRightSelector(SpriteFont spriteFont, Texture2D leftArrow, Texture2D rightArrow)
        {

            SpriteFont = spriteFont;
            this.leftSelection = leftArrow;
            this.rightSelection = rightArrow;
            IsEnabled = true;
            IsVisible = true;
            TabStop = true;
            Color = Color.White;
        }

        #endregion

        #region Methods

        public void SetItems(string[] items, int maxWidth)
        {
            this.items.Clear();
            foreach (string s in items) this.items.Add(s);
            maxItemWidth = maxWidth;
        }

        #endregion

        #region Abstract Methods

        public override void Update(GameTime gameTime)
        {
            
        }

        public override void HandleInput(PlayerIndex playerIndex)
        {
            if (items.Count == 0) return;

            if (InputManager.ButtonReleased(playerIndex, Buttons.LeftThumbstickLeft) ||
                InputManager.ButtonReleased(playerIndex, Buttons.DPadLeft) ||
                InputManager.KeyReleased(Keys.Left))
            {
                selectedItem--;
                if (selectedItem < 0)
                    selectedItem = 0;
            }

            if (InputManager.ButtonReleased(playerIndex, Buttons.LeftThumbstickRight) ||
                InputManager.ButtonReleased(playerIndex, Buttons.DPadRight) ||
                InputManager.KeyReleased(Keys.Right))
            {
                selectedItem++;
                if (selectedItem >= items.Count)
                    selectedItem = items.Count - 1;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Vector2 drawTo = position;

            // Draw left arrow if not the first item
            if (selectedItem > 0)
                spriteBatch.Draw(leftSelection, drawTo, Color.White);
            drawTo.X += leftSelection.Width + 5f;

            // Draw the text for the selected item
            float itemWidth = spriteFont.MeasureString(items[selectedItem]).X;
            float offset = (maxItemWidth - itemWidth) / 2;
            drawTo.X += offset;

            if (hasFocus)
                spriteBatch.DrawString(spriteFont, items[selectedItem], drawTo, highlightColor);
            else
                spriteBatch.DrawString(spriteFont, items[selectedItem], drawTo, Color);
            drawTo.X += -1 * offset + maxItemWidth + 5f;

            // Draw right arrow if not the last item
            if (selectedItem < items.Count - 1)
                spriteBatch.Draw(rightSelection, drawTo, Color.White);
        }

        #endregion

    }
}
