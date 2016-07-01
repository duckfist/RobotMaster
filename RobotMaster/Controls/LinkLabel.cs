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
    public class LinkLabel : Control
    {

        #region Fields and Properties

        Color selectedColor = Color.Red;

        public Color SelectedColor
        {
            get { return selectedColor; }
            set { selectedColor = value; }
        }

        #endregion

        #region Constructor Region

        public LinkLabel(SpriteFont spriteFont)
        {
            SpriteFont = spriteFont;
            IsEnabled = true;
            IsVisible = true;
            TabStop = true;         // Able to receive focus
            HasFocus = false;
            Position = Vector2.Zero;
        }

        #endregion

        #region Abstract Methods

        public override void Update(GameTime gameTime)
        {
            
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (hasFocus)
                spriteBatch.DrawString(SpriteFont, Text, Position, selectedColor);
            else
                spriteBatch.DrawString(SpriteFont, Text, Position, Color);
        }

        public override void HandleInput(PlayerIndex playerIndex)
        {
            if (!HasFocus) return;

            if (InputManager.KeyReleased(Keys.Enter) ||
                InputManager.ButtonReleased(PlayerIndex.One, Buttons.A))
                base.OnSelected(null);
        }

        #endregion

    }
}
