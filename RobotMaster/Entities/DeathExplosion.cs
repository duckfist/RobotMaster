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
    public class DeathExplosion : MMEffect
    {
        List<DeathExplosionParticle> Particles;

        public DeathExplosion(Vector2 Position)
            : base(Position)
        {
            Particles = new List<DeathExplosionParticle>();

            // outer ring
            Particles.Add(new DeathExplosionParticle(Position, MegaMath.UnitVector(0)));
            Particles.Add(new DeathExplosionParticle(Position, MegaMath.UnitVector(Math.PI / 4)));
            Particles.Add(new DeathExplosionParticle(Position, MegaMath.UnitVector(Math.PI / 2)));
            Particles.Add(new DeathExplosionParticle(Position, MegaMath.UnitVector(Math.PI * 3/ 4)));
            Particles.Add(new DeathExplosionParticle(Position, MegaMath.UnitVector(Math.PI)));
            Particles.Add(new DeathExplosionParticle(Position, MegaMath.UnitVector(Math.PI * 5 / 4)));
            Particles.Add(new DeathExplosionParticle(Position, MegaMath.UnitVector(Math.PI * 3 / 2)));
            Particles.Add(new DeathExplosionParticle(Position, MegaMath.UnitVector(Math.PI * 7 / 4)));
            Particles.Add(new DeathExplosionParticle(Position, MegaMath.UnitVector(Math.PI * 2)));

            // inner ring
            Particles.Add(new DeathExplosionParticle(Position, 0.5f * MegaMath.UnitVector(0)));
            Particles.Add(new DeathExplosionParticle(Position, 0.5f * MegaMath.UnitVector(Math.PI / 4)));
            Particles.Add(new DeathExplosionParticle(Position, 0.5f * MegaMath.UnitVector(Math.PI / 2)));
            Particles.Add(new DeathExplosionParticle(Position, 0.5f * MegaMath.UnitVector(Math.PI * 3 / 4)));
            Particles.Add(new DeathExplosionParticle(Position, 0.5f * MegaMath.UnitVector(Math.PI)));
            Particles.Add(new DeathExplosionParticle(Position, 0.5f * MegaMath.UnitVector(Math.PI * 5 / 4)));
            Particles.Add(new DeathExplosionParticle(Position, 0.5f * MegaMath.UnitVector(Math.PI * 3 / 2)));
            Particles.Add(new DeathExplosionParticle(Position, 0.5f * MegaMath.UnitVector(Math.PI * 7 / 4)));
            Particles.Add(new DeathExplosionParticle(Position, 0.5f * MegaMath.UnitVector(Math.PI * 2)));

            LoadAnimations();
            // Debug
            //debugRectangle = new DebugRectangle((int)Width, (int)Height);
            //debugRectTex = DebugRectangle.GetRectTexture((int)Width, (int)Height);
        }

        public static void CreateEffect(Vector2 position)
        {
            Session.Level.Effects.Add(new DeathExplosion(position));
        }

        public override bool IsOnScreen
        {
            get
            {
                foreach (DeathExplosionParticle particle in Particles)
                {
                    if (particle.IsActive) return true;
                }
                return false;
            }
        }

        public override void LoadAnimations()
        {
            foreach (DeathExplosionParticle particle in Particles)
            {
                particle.LoadAnimations();
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (IsActive)
            {
                foreach (DeathExplosionParticle particle in Particles)
                {
                    particle.Update(gameTime);
                }

                if (!IsOnScreen)
                {
                    IsActive = false;
                    
                }
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (IsActive)
            {
                foreach (DeathExplosionParticle particle in Particles)
                {
                    particle.Draw(gameTime, spriteBatch);
                }
            }
            base.Draw(gameTime, spriteBatch);
        }


    }

    public class DeathExplosionParticle : MMEffect
    {
        private static Dictionary<String, Vector2> spriteOffset = new Dictionary<string, Vector2>();
        protected AnimatedSprite animatedSprite;
        protected Dictionary<String, Animation> animations;

        public Vector2 Velocity;

        public DeathExplosionParticle(Vector2 Position, Vector2 Velocity)
            : base(Position)
        {
            IsActive = true;
            this.Velocity = Velocity;
        }

        public override void LoadAnimations()
        {
            animations = new Dictionary<string, Animation>();
            spriteOffset = new Dictionary<string, Vector2>();

            Animation a = new Animation(ContentRects, ContentOffsets);
            a.FramesPerSecond = 30;
            animations.Add("Default", a);

            animatedSprite = new AnimatedSprite(TextureObjects, animations);
            animatedSprite.CurrentAnimation = "Default";
            animatedSprite.IsAnimating = true;
        }


        public override void Update(GameTime gameTime)
        {
            if (IsActive)
            {
                this.Position += this.Velocity;

                if (!IsOnScreen)
                {
                    IsActive = false;
                }

                animatedSprite.Position = this.Position;
                animatedSprite.Update(gameTime);

                base.Update(gameTime);
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (IsActive)
            {
                
                animatedSprite.Draw(spriteBatch);
                base.Draw(gameTime, spriteBatch);
            }
        }

        private static Rectangle ContentRect0 = new Rectangle(66, 0, 24, 24);
        private static Vector2 ContentOffset0 = new Vector2(-12, -12);
        private static Rectangle ContentRect1 = new Rectangle(91, 4, 16, 16);
        private static Vector2 ContentOffset1 = new Vector2(-8, -8);
        private static Rectangle ContentRect2 = new Rectangle(110, 6, 12, 12);
        private static Vector2 ContentOffset2 = new Vector2(-6, -6);
        private static Rectangle ContentRect3 = new Rectangle(128, 7, 10, 10);
        private static Vector2 ContentOffset3 = new Vector2(-5, -5);
        private static Rectangle ContentRect4 = new Rectangle(144, 10, 4, 4);
        private static Vector2 ContentOffset4 = new Vector2(-2, -2);
        private static Rectangle[] ContentRects = { ContentRect0, ContentRect1, ContentRect2, ContentRect3, ContentRect4 };
        private static Vector2[] ContentOffsets = { ContentOffset0, ContentOffset1, ContentOffset2, ContentOffset3, ContentOffset4 };
    }
}
