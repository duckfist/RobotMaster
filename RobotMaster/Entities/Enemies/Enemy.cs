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


    public abstract class Enemy : Entity
    {
        protected static Texture2D mm10EnemySprites;
        protected static Texture2D mm9EnemySprites;

        protected AnimatedSprite animatedSprite;
        protected Dictionary<String, Animation> animations;
        protected AttributePair HP;

        public bool IsActive { get; private set; }
        public bool IsSpawned { get; private set; }
        public bool IsDying { get; set; }

        protected DebugRectangle debugRectangle;
        protected Texture2D debugRectTex;
        protected TimeSpan DamageFlashDuration = TimeSpan.FromMilliseconds(50);
        protected DateTime DamageFlashStart = DateTime.Now;
        protected Vector2 defaultPosition;
        public Vector2 DefaultPosition { get { return defaultPosition; } set { defaultPosition = value; } }
        public EnemyState State;

        public virtual FloatRect Bounds { get { return new FloatRect(Position, Size.X, Size.Y); } }
        public virtual float Width { get { return Size.X; } }
        public virtual float Height { get { return Size.Y; } }

        public abstract Vector2 Size { get; }

        public Enemy(Vector2 position)
            : base(position)
        {
            DefaultPosition = position;
            
            // Start in "OffscreenNotSpawned" state
            IsActive = false;
            IsSpawned = false;
            State = EnemyState.OffscreenNotSpawned;

            LoadAnimations();

            // Debug
            debugRectangle = new DebugRectangle((int)Width, (int)Height);
            debugRectTex = DebugRectangle.GetRectTexture((int)Width, (int)Height);
        }

        public static void LoadContent(ContentManager Content)
        {
            // Load bullet images from sprite sheet
            mm10EnemySprites = Content.Load<Texture2D>(@"Sprites\Sprites_mm10MinorEnemies");
            mm9EnemySprites = Content.Load<Texture2D>(@"Sprites\Sprites_mm9enemies");
        }

        public override void Update(GameTime gameTime)
        {
            switch (State)
            {
                case EnemyState.Activated:
                    // Walked away from enemy, deactivate it.
                    if (!IsOnScreen)
                    {
                        Deactivate();
                    }
                    // It is also possible that the enemy is killed. This is handled in the derived class.
                    break;
                case EnemyState.Deactivated:
                    // Deactivated enemy (at spawn position) is out of range, despawn it.
                    if (!IsOnScreen)
                    {
                        State = EnemyState.OffscreenNotSpawned;
                        IsSpawned = false;
                    }
                    break;
                case EnemyState.OffscreenNotSpawned:
                    if (IsOnScreen)
                    {
                        Activate();
                    }
                    break;
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (Session.DebugHitboxes)
            {
                debugRectangle.Draw(spriteBatch, Position, Color.Red, debugRectTex);
            }
        }

        public virtual void BulletCollisions(List<Bullet> bullets)
        {
            if (IsActive && !IsDying)
            {
                foreach (Bullet b in bullets)
                {
                    // Collision against the enemy
                    if (b.InUse && b.HitTest(Bounds, HP.CurrentValue))
                    {
                        // TODO: Solar Blaze hit limiter
                        HP.Deplete(b.Damage);

                        // Killed!
                        if (HP.CurrentValue <= 0.0f)
                        {
                            // TODO: death animation
                            IsDying = true;
                            State = EnemyState.DeathAnimate;
                            animatedSprite.Position = this.Position;
                            animatedSprite.IsAnimating = true;  
                            animatedSprite.CurrentAnimation = "Death";
                            animatedSprite.SetCurrentFrame("Death", 0);
                            animatedSprite.FrameTimer = TimeSpan.Zero;
                        }
                        // "Flash" to indicate damage has been dealt
                        else
                        {
                            DamageFlashStart = DateTime.Now;
                        }
                    }
                }
            }
        }

        public virtual void Deactivate()
        {
            Position = defaultPosition;
            State = EnemyState.Deactivated;
            IsActive = false;
        }

        public virtual void Activate()
        {
            State = EnemyState.Activated;
            IsActive = true;
        }

        
        public virtual bool IsOnScreen
        {
            get
            {
                return ((Position.X < Session.Camera.Position.X + Engine.ViewportWidth) &&
                    (Position.X + Width > Session.Camera.Position.X) &&
                    (Position.Y + Height > Session.Camera.Position.Y) &&
                    (Position.Y < Session.Camera.Position.Y + Engine.ViewportHeight) &&
                    !Session.IsScrolling);
            }
        }
        
        public abstract void LoadAnimations();

        public static Rectangle ExplodeContentRect0 = new Rectangle(621, 261, 12, 12);
        public static Rectangle ExplodeContentRect1 = new Rectangle(636, 261, 12, 12);
        public static Rectangle ExplodeContentRect2 = new Rectangle(647, 261, 12, 12);
        public static Rectangle[] ExplodeContentRects = { ExplodeContentRect0, ExplodeContentRect1, ExplodeContentRect2 };

        public static Enemy CreateEnemy(EnemyTypes enemyType, int tileX, int tileY)
        {
            Vector2 pos = new Vector2(tileX * Engine.TileWidth, tileY * Engine.TileHeight);

            switch (enemyType)
            {
                case EnemyTypes.MedusaHead:
                    // Spawn in the center of the destination tile
                    return new MedusaHead(
                        Engine.TileSizeVector/2
                        - MedusaHead.SIZE/2
                        + pos);
                case EnemyTypes.SmallJumper:
                    // Spawn in bottom-center of destination tile
                    return new SmallJumper(
                        new Vector2(Engine.TileSizeVector.X/2, Engine.TileSizeVector.Y)
                        - new Vector2(SmallJumper.SIZE.X/2, SmallJumper.SIZE.Y)
                        + pos);
                case EnemyTypes.Zoomer:
                    // Spawn in bottom-center of destination tile
                    return new Zoomer(
                        new Vector2(Engine.TileSizeVector.X / 2, Engine.TileSizeVector.Y)
                        - new Vector2(Zoomer.SIZE.X / 2, Zoomer.SIZE.Y)
                        + pos);
                case EnemyTypes.CeilingSpider:
                    // Spawn in upper-center of destination tile
                    return new JumpSpider(
                        new Vector2(Engine.TileSizeVector.X / 2, -1)
                        - new Vector2(Zoomer.SIZE.X / 2, 0)
                        + pos,
                        JumpSpiderType.Small,
                        true
                        );
                case EnemyTypes.TestBoss:
                    // Spawn at very top of screen on the right side
                    tileX = tileX - tileX % Engine.ViewportWidthTiles;
                    tileY = tileY - tileY % Engine.ViewportHeightTiles;
                    tileX = tileX + 12;
                    tileY = tileY + 1;
                    pos = new Vector2(tileX * Engine.TileWidth, tileY * Engine.TileHeight);

                    return new TestBoss(
                        new Vector2(Engine.TileSizeVector.X / 2, Engine.TileSizeVector.Y)
                        - new Vector2(TestBoss.SIZE.X / 2, TestBoss.SIZE.Y)
                        + pos);
                default:
                    throw new ArgumentException("Bad enemy type.");
            }

        }
    }

    public class SerializedEnemy
    {
        float x;
        float y;
        int type;
    }

    public enum EnemyTypes
    {
        MedusaHead = 0,
        SmallJumper = 1,
        Zoomer = 2,
        CeilingSpider = 3,
        TestBoss = 4,
    }

    public enum EnemyState
    {
        Activated,              // Spawned, Active, OnScreen
        OffscreenNotSpawned,    // NOT Spawned, NOT Active, NOT OnScreen
        Deactivated,            // Spanwed, NOT Active, OnScreen
        DeathAnimate            // Pre-deactivation. Spawned, Active, OnScreen
    }

    

}
