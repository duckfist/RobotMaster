using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace RobotMaster.GameComponents
{
    /// <summary>
    /// The InputManager handles all input in the game, allowing access to any input
    /// device from anywhere in the game.  There is only one instance of this class.
    /// </summary>
    public class InputManager : GameComponent
    {

        public static KeyboardState keyboardState;
        public static KeyboardState lastKeyboardState;

        public static GamePadState[] gamePadStates;
        public static GamePadState[] lastGamePadStates;

        public InputManager(Game game)
            : base(game)
        {
            gamePadStates = new GamePadState[4];        // 4 controllers
            lastGamePadStates = new GamePadState[4];
            foreach (PlayerIndex index in Enum.GetValues(typeof(PlayerIndex)))
            {
                gamePadStates[(int)index] = GamePad.GetState(index);
            }

            keyboardState = Keyboard.GetState();
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            // Save previous frame's keyboard values, get new values
            lastGamePadStates = (GamePadState[])gamePadStates.Clone(); // Array copy
            foreach (PlayerIndex index in Enum.GetValues(typeof(PlayerIndex)))
            {
                gamePadStates[(int)index] = GamePad.GetState(index);
            }

            // Save previous frame's keyboard values, get new values
            lastKeyboardState = keyboardState;
            keyboardState = Keyboard.GetState();

            base.Update(gameTime);
        }

        #region Keyboard Region


        public static KeyboardState KeyboardState
        {
            get { return keyboardState; }
        }

        public static KeyboardState LastKeyboardState
        {
            get { return lastKeyboardState; }
        }

        public static bool KeyReleased(Keys key)
        {
            return keyboardState.IsKeyUp(key) && lastKeyboardState.IsKeyDown(key);
        }

        public static bool KeyPressed(Keys key)
        {
            return keyboardState.IsKeyDown(key) && lastKeyboardState.IsKeyUp(key);
        }

        public static bool KeyDown(Keys key)
        {
            return keyboardState.IsKeyDown(key);
        }

        #endregion

        #region Game Pad Region

        public static GamePadState[] GamePadStates
        {
            get { return gamePadStates; }
        }
        public static GamePadState[] LastGamePadStates
        {
            get { return lastGamePadStates; }
        }
        public static bool ButtonReleased(PlayerIndex index, Buttons button)
        {
            return gamePadStates[(int)index].IsButtonUp(button) &&
                lastGamePadStates[(int)index].IsButtonDown(button);
        }
        public static bool ButtonPressed(PlayerIndex index, Buttons button)
        {
            return gamePadStates[(int)index].IsButtonDown(button) &&
                lastGamePadStates[(int)index].IsButtonUp(button);
        }

        public static bool ButtonDown(PlayerIndex index, Buttons button)
        {
            return gamePadStates[(int)index].IsButtonDown(button);
        }

        #endregion

        #region Nullable Method Regions

        public static bool WasReleased(PlayerIndex? playerInControl, Buttons button, Keys key, out PlayerIndex playerIndex)
        {
            if (playerInControl.HasValue)
            {
                playerIndex = playerInControl.Value;
                if (KeyReleased(key) || ButtonReleased(playerInControl.Value, button))
                    return true;
                else
                    return false;
            }
            else
            {
                return (WasReleased(PlayerIndex.One, button, key, out playerIndex) ||
                    WasReleased(PlayerIndex.Two, button, key, out playerIndex) ||
                    WasReleased(PlayerIndex.Three, button, key, out playerIndex) ||
                    WasReleased(PlayerIndex.Four, button, key, out playerIndex));
            }
        }

        public static bool WasPressed(PlayerIndex? playerInControl, Buttons button, Keys key, out PlayerIndex playerIndex)
        {
            if (playerInControl.HasValue)
            {
                playerIndex = playerInControl.Value;
                if (KeyPressed(key) || ButtonPressed(playerInControl.Value, button))
                    return true;
                else
                    return false;
            }
            else
            {
                return (WasPressed(PlayerIndex.One, button, key, out playerIndex) ||
                    WasPressed(PlayerIndex.Two, button, key, out playerIndex) ||
                    WasPressed(PlayerIndex.Three, button, key, out playerIndex) ||
                    WasPressed(PlayerIndex.Four, button, key, out playerIndex));
            }
        }

        #endregion

        #region General Methods

        public static void Flush()
        {
            lastKeyboardState = keyboardState;
            lastGamePadStates = (GamePadState[])gamePadStates.Clone();
        }

        #endregion
    }
}
