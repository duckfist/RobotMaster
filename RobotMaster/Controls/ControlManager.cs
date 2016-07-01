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
    public class ControlManager : List<Control>
    {
        #region Fields and Properties
        int selectedControl = 0;
        #endregion


        #region Constructors
        public ControlManager() : base() { }
        public ControlManager(int capacity) : base(capacity) { }
        public ControlManager(IEnumerable<Control> collection) : base(collection) { }
        #endregion


        #region Methods
        public void Update(GameTime gameTime, PlayerIndex playerIndex)
        {
            if (Count == 0)
                return;
            foreach (Control c in this)
            {
                if (c.IsEnabled)
                    c.Update(gameTime);
                if (c.HasFocus)
                    c.HandleInput(playerIndex);
            }
            if (InputManager.ButtonPressed(playerIndex, Buttons.LeftThumbstickUp) ||
                InputManager.ButtonPressed(playerIndex, Buttons.DPadUp) ||
                InputManager.KeyPressed(Keys.Up))
                PreviousControl();
            if (InputManager.ButtonPressed(playerIndex, Buttons.LeftThumbstickDown) ||
                InputManager.ButtonPressed(playerIndex, Buttons.DPadDown) ||
                InputManager.KeyPressed(Keys.Down))
                NextControl();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Control c in this)
            {
                if (c.IsVisible) c.Draw(spriteBatch);
            }
        }

        public void NextControl()
        {
            if (Count == 0)
                return;
            int currentControl = selectedControl;
            this[selectedControl].HasFocus = false;
            do
            {
                selectedControl++;
                if (selectedControl == Count)
                    selectedControl = 0;
                if (this[selectedControl].TabStop && this[selectedControl].IsEnabled)
                    break;
            } while (currentControl != selectedControl);
            this[selectedControl].HasFocus = true;
        }

        public void PreviousControl()
        {
            if (Count == 0)
                return;
            int currentControl = selectedControl;
            this[selectedControl].HasFocus = false;
            do
            {
                selectedControl--;
                if (selectedControl < 0)
                    selectedControl = Count - 1;
                if (this[selectedControl].TabStop && this[selectedControl].IsEnabled)
                    break;
            } while (currentControl != selectedControl);
            this[selectedControl].HasFocus = true;
        }

        #endregion
    }
}
