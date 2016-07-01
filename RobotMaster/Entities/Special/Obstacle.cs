using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RobotMaster.Mathematics;
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


    public abstract class Obstacle : Entity
    {
        protected static Texture2D mm10Objects;

        protected AnimatedSprite animatedSprite;
        protected Dictionary<String, Animation> animations;

        protected Vector2 defaultPosition;
        public Vector2 DefaultPosition { get { return defaultPosition; } set { defaultPosition = value; } }
        
        public virtual FloatRect Bounds { get { return new FloatRect(Position, Size.X, Size.Y); } }
        public virtual float Width { get { return Size.X; } }
        public virtual float Height { get { return Size.Y; } }
        public virtual Vector2 Size { get { return new Vector2(); } }

        public Obstacle(Vector2 position)
            : base(position)
        {
            DefaultPosition = position;
        }

        public static void LoadContent(ContentManager Content)
        {
            mm10Objects = Content.Load<Texture2D>(@"Sprites\Sprites_mm10Objects");
        }

        public static Obstacle CreateObstacle(ObstacleTypes type, int tileX, int tileY)
        {
            Vector2 pos = new Vector2(tileX * Engine.TileWidth, tileY * Engine.TileHeight);

            switch (type)
            {
                case ObstacleTypes.RisingPlatform:
                    // Spawn in the center of the destination tile
                    return new RisingPlatform(pos);
                case ObstacleTypes.FallingPlatform:
                    return new RisingPlatform(pos, true);
                case ObstacleTypes.BossDoorVert:
                    return new BossDoor(pos, true);
                case ObstacleTypes.BossDoorHoriz:
                    return new BossDoor(pos, false);
                default:
                    throw new ArgumentException("Bad type.");
            }
        }

        public override void Update(GameTime gameTime)
        {
            // TODO: State machine
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
        }

        public virtual void NotifyPlayerCollision() { }
        public virtual void NotifyPlayerStanding() { }

        public abstract bool IsOnScreen { get; }
        public abstract void LoadAnimations();

    }

    public enum ObstacleTypes
    {
        RisingPlatform = 0,
        FallingPlatform = 1,
        BossDoorVert = 2,
        BossDoorHoriz = 3,
    }
}
