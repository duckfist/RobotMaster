using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RobotMaster.GameComponents;
using RobotMaster.TileEngine;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace RobotMaster.Entities
{
    public abstract class MMEffect : Entity
    {
        public bool IsActive; // Determine when effect no longer needs to be drawn

        protected Texture2D debugRectTex;
        protected DebugRectangle debugRectangle;

        public MMEffect(Vector2 Position)
            : base(Position)
        {
            IsActive = true;
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsActive)
            {
                Session.Level.Effects.Remove(this);
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            
        }

        public abstract void LoadAnimations();

        public virtual bool IsOnScreen
        {
            get
            {
                return ((Position.X < Session.Camera.Position.X + Engine.ViewportWidth) &&
                    (Position.X > Session.Camera.Position.X) &&
                    (Position.Y > Session.Camera.Position.Y) &&
                    (Position.Y < Session.Camera.Position.Y + Engine.ViewportHeight) &&
                    !Session.IsScrolling);
            }
        }

    }
}
