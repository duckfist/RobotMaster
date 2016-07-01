using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

using RobotMaster.Mathematics;
using RobotMaster.GameComponents;

using Microsoft.Xna.Framework.Content;

namespace RobotMaster.Entities
{
    // General construct for a visible, interactive game object
    public abstract class Entity
    {
        public static Texture2D TextureObjects;
        public static float Gravity = 0.15f;

        public Vector2 PosPreviousFrame;
        public Vector2 position = Vector2.Zero;

        public bool IsCollidable = true;

        public Vector2 Position
        {
            get
            {
                return position;
            }
            set
            {
                PosPreviousFrame = position;
                position = value;
            }

        }

        public Entity(Vector2 position)
        {
            this.Position = position;
        }

        public static void LoadContent(ContentManager Content)
        {
            TextureObjects = Content.Load<Texture2D>(@"Sprites\Sprites_mm10Objects");
            MeterDisplay.LoadContent();
        }

        public abstract void Update(GameTime gameTime);

        public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);
    }
}
