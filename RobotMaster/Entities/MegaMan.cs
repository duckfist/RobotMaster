using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

using RobotMaster.TileEngine;
using RobotMaster.GameComponents;
using RobotMaster.Mathematics;

namespace RobotMaster.Entities
{
    public enum Weapon
    {
        MEGA_BUSTER,
        WATER_SHIELD,
        SOLAR_BLAZE,
        CHILL_SPIKE,
        WHEEL_CUTTER,
        COMMANDO_BOMB,
        TRIPLE_BLADE,
        REBOUND_STRIKER,
        THUNDER_WOOL,
    }

    public class MegaMan 
    {
        #region Fields and Properties

        public AttributePair HitPoints;

        public static float JumpVelocity = 5.5f;
        public static float WalkVelocity = 1.5f;
        public static float ClimbVelocity = 1.5f;
        public static float Width = 14.0f;
        public static float Height = 22.0f;
        public static float Acceleration = 0.28f;
        public static float MaxFallVelocity = 8.0f;
        protected static AnimatedSprite animatedSprite;
        public static Texture2D mmPalettes;
        protected static Texture2D debugRectTex;
        protected static DebugRectangle debugRectangle;
        protected static Dictionary<String, Animation> animations;
        protected static Dictionary<String, Vector2> spriteOffset;

        public Vector2 PosPreviousFrame;
        public Vector2 Position;
        public Vector2 Velocity;
        public Vector2 VelocityExternal;
        public bool IsJumping;
        public bool IsAbleToJump;   // Necessary to allow immediate jumping after special events
        public bool IsFalling;
        public bool IsFacingLeft;
        public bool IsShooting;
        public bool IsClimbing;
        //public bool IsAbleToClimb;
        protected bool isOnGround;
        protected float previousBottom;

        public bool[] AvailableWeapons = new bool[9];
        public AttributePair[] WeaponEnergy = new AttributePair[9]
        {   
            new AttributePair(0),
            new AttributePair(28),
            new AttributePair(28),
            new AttributePair(0),
            new AttributePair(0),
            new AttributePair(0),
            new AttributePair(28),
            new AttributePair(0),
            new AttributePair(0),
        };
        public Weapon CurrentWeapon = Weapon.MEGA_BUSTER;
        public List<Bullet> Bullets;
        public List<MegaBuster> Bullets_MegaBuster;
        public List<TripleBlade> Bullets_TripleBlade;
        public List<SolarBlaze> Bullets_SolarBlaze;
        public List<WaterShield> Bullets_WaterShield;
        protected DateTime shootAnimationCooldown = DateTime.MinValue;


        public int NumBulletsActive
        {
            get
            {
                int i = 0;
                foreach (Bullet bullet in Bullets)
                    if (bullet.InUse) i++;
                return i;
            }
        }
        public int ScreenLocationInRoom
        {
            get
            {
                return (int)(PositionInRoom.X / Engine.ViewportWidth);
            }
        }
        public Vector2 PositionInRoom
        {
            get
            {
                return Position - Session.CurrentRoom.Position;
            }
        }
        public bool IsOnGround
        {
            get { return isOnGround; }
        }
        public FloatRect HitBox
        {
            get { return new FloatRect(Position, Width, Height); }
        }

        #endregion

        public MegaMan(Vector2 SpawnPoint)
        {
            Velocity = new Vector2();
            VelocityExternal = new Vector2();
            Position = SpawnPoint;
            PosPreviousFrame = new Vector2(128, 20);
            IsJumping = false;
            IsFalling = false;
            IsFacingLeft = false;
            IsShooting = false;
            IsAbleToJump = false;
            IsClimbing = false;

            HitPoints = new AttributePair(28);

            // Configure all possible bullets being used
            Bullets = new List<Bullet>();
            Bullets_MegaBuster = new List<MegaBuster>();
            Bullets_TripleBlade = new List<TripleBlade>();
            Bullets_SolarBlaze = new List<SolarBlaze>();
            Bullets_WaterShield = new List<WaterShield>();

            Bullets_MegaBuster.Add(new MegaBuster());
            Bullets_MegaBuster.Add(new MegaBuster());
            Bullets_MegaBuster.Add(new MegaBuster());
            foreach (MegaBuster buster in Bullets_MegaBuster)
                Bullets.Add(buster);

            Bullets_TripleBlade.Add(new TripleBlade());
            foreach (TripleBlade blade in Bullets_TripleBlade)
                Bullets.Add(blade);

            Bullets_WaterShield.Add(new WaterShield());
            foreach (WaterShield wshield in Bullets_WaterShield)
                Bullets.Add(wshield);            

            Bullets_SolarBlaze.Add(new SolarBlaze());
            foreach (SolarBlaze solar in Bullets_SolarBlaze)
                Bullets.Add(solar);

            AvailableWeapons[(int)Weapon.MEGA_BUSTER] = true;
            AvailableWeapons[(int)Weapon.SOLAR_BLAZE] = true;
            AvailableWeapons[(int)Weapon.TRIPLE_BLADE] = true;
            AvailableWeapons[(int)Weapon.WATER_SHIELD] = true;
        }

        #region Methods

        public static void LoadContent(ContentManager contentRef)
        {
            animations = new Dictionary<string, Animation>();
            spriteOffset = new Dictionary<string,Vector2>();
            debugRectangle = new DebugRectangle((int)Width, (int)Height);
            debugRectTex = DebugRectangle.GetRectTexture((int)Width, (int)Height);
            //mmPalettes = contentRef.Load<Texture2D>(@"Sprites\megaman_sprites");

            Texture2D playerTexture = contentRef.Load<Texture2D>(@"Sprites\Sprites_mm2mega");
            animations.Add("WalkLeft", new Animation(mm_walkleft, mm_walkleft_offsets));
            animations.Add("WalkRight", new Animation(mm_walkright, mm_walkright_offsets));
            animations.Add("WalkShootLeft", new Animation(mm_walkshootleft, mm_walkshootleft_offsets));
            animations.Add("WalkShootRight", new Animation(mm_walkshootright, mm_walkshootright_offsets));
            animations.Add("StandLeft", new Animation(mm_standleft, mm_standleft_offset));
            animations.Add("StandRight", new Animation(mm_standright, mm_standright_offset));
            animations.Add("JumpLeft", new Animation(mm_jumpleft, mm_jumpleft_offset));
            animations.Add("JumpRight", new Animation(mm_jumpright, mm_jumpright_offset));
            animations.Add("JumpShootLeft", new Animation(mm_jumpshootleft, mm_jumpshootleft_offset));
            animations.Add("JumpShootRight", new Animation(mm_jumpshootright, mm_jumpshootright_offset));
            animations.Add("StandShootLeft", new Animation(mm_standshootleft, mm_standshootleft_offset));
            animations.Add("StandShootRight", new Animation(mm_standshootright, mm_standshootright_offset));
            animations.Add("Climb", new Animation(mm_climb, mm_climb_offsets));
            animations.Add("ClimbShootLeft", new Animation(mm_climbshootleft, mm_climbshootleftoffset));
            animations.Add("ClimbShootRight", new Animation(mm_climbshootright, mm_climbshootrightoffset));
            animatedSprite = new AnimatedSprite(playerTexture, animations);
            animatedSprite.CurrentAnimation = "StandRight";
            animatedSprite.IsAnimating = true;

            
        }

        public void LockToViewport()
        {
            // TODO: This will need to change to accomodate screen scrolling and pit death
            Position.X = MathHelper.Clamp(Position.X, Session.CurrentRoom.Position.X, Session.CurrentRoom.Position.X + Session.CurrentRoom.Size.X - Width);
            Position.Y = MathHelper.Clamp(Position.Y, Session.CurrentRoom.Position.Y, Session.CurrentRoom.Position.Y + Session.CurrentRoom.Size.Y - Height);
        }

        /// <summary>
        /// Fast-weapon switch to the next weapon in the cycle of available weapons.
        /// </summary>
        /// <param name="next">True to cycle forwards, false to cycle back.</param>
        public void NextWeapon(bool next)
        {
            if (next)
            {
                // Start at the next weapon
                int i = ((int)CurrentWeapon + 1) % AvailableWeapons.Length;

                // Loop through all possible weapons
                while (true)
                {
                    // Change to weapon if available
                    if (AvailableWeapons[i])
                    {
                        ChangeWeapons((Weapon)i);
                        break;
                    }
                    i = (i + 1) % AvailableWeapons.Length; // Wrap back
                    if (i == (int)CurrentWeapon) break;    // None found
                }
            }

            else
            {
                int i = (int)CurrentWeapon - 1;
                i = (i < 0) ? AvailableWeapons.Length - 1: i; // Wrap back
                while (true)
                {
                    if (AvailableWeapons[i])
                    {
                        ChangeWeapons((Weapon)i);
                        break;
                    }
                    i = (--i < 0) ? AvailableWeapons.Length - 1 : i; // Wrap back
                    if (i == (int)CurrentWeapon) break;    // None found
                }
            }
        }

        public void ChangeWeapons(Weapon weapon)
        {
            CurrentWeapon = weapon;
            Session.SetShaderColor(CurrentWeapon);
        }

        public void Shoot()
        {
            switch (CurrentWeapon)
            {
                case Weapon.MEGA_BUSTER:
                    foreach (MegaBuster bullet in Bullets_MegaBuster)
                    {
                        if (!bullet.InUse)
                        {
                            bullet.Activate(IsFacingLeft, HitBox);
                            shootAnimationCooldown = DateTime.Now;
                            //Session.soundBank.PlayCue("buster");
                            return;
                        }
                    }
                    break;
                case Weapon.WATER_SHIELD:
                    foreach (WaterShield bullet in Bullets_WaterShield)
                    {
                        if (!bullet.InUse && WeaponEnergy[(int)CurrentWeapon].CurrentValue > 0)
                        {
                            bullet.Activate(IsFacingLeft/*, this*/);
                            shootAnimationCooldown = DateTime.Now;
                            WeaponEnergy[(int)CurrentWeapon].Deplete(4);
                            return;
                        }
                        else if (bullet.ReadyToLaunch)
                        {
                            bullet.Launch();
                        }
                    }
                    break;
                case Weapon.SOLAR_BLAZE:
                    foreach (SolarBlaze bullet in Bullets_SolarBlaze)
                    {
                        if (!bullet.InUse && WeaponEnergy[(int)CurrentWeapon].CurrentValue > 0)
                        {
                            bullet.Activate(IsFacingLeft, HitBox);
                            shootAnimationCooldown = DateTime.Now;
                            WeaponEnergy[(int)CurrentWeapon].Deplete(2);
                            return;
                        }
                    }
                    break;
                case Weapon.CHILL_SPIKE:
                    break;
                case Weapon.WHEEL_CUTTER:
                    break;
                case Weapon.COMMANDO_BOMB:
                    break;
                case Weapon.TRIPLE_BLADE:
                    foreach (TripleBlade bullet in Bullets_TripleBlade)
                    {
                        if (!bullet.InUse && WeaponEnergy[(int)CurrentWeapon].CurrentValue > 0)
                        {
                            bullet.Activate(IsFacingLeft, IsJumping || IsFalling, HitBox);
                            shootAnimationCooldown = DateTime.Now;
                            WeaponEnergy[(int)CurrentWeapon].Deplete(1);
                            return;
                        }
                    }
                    break;
                case Weapon.REBOUND_STRIKER:
                    break;
                case Weapon.THUNDER_WOOL:
                    break;
            }


        }

        #endregion Methods

        #region Updating

        public virtual void Udpate(GameTime gameTime)
        {

            if (!Session.IsScrolling)
            {
                // Update Mega Man's position
                if (!IsClimbing)
                {
                    Velocity.Y = MathHelper.Clamp(Velocity.Y + Acceleration, -MaxFallVelocity, MaxFallVelocity);
                }
                PosPreviousFrame = Position;
                Position += Velocity;

                // Keep Mega in the screen bounds
                LockToViewport();

                // Check and respond to tiles
                HandleTileCollisions();

                // Try to exit the room.
                CheckExits();

                // Respond to special platforms and other objects
                HandleSpecialCollisions();



                // Has run into a wall, or is standing still
                if (Position.X == PosPreviousFrame.X)
                    Velocity.X = 0;

                if (IsClimbing)
                {
                }
                // Player is on the ground
                else if (Math.Abs(Position.Y - PosPreviousFrame.Y) < 0.001)
                //if (Position.Y == PosPreviousFrame.Y)
                {
                    Velocity.Y = 0;

                    IsFalling = (IsJumping) ? true : false;
                    IsJumping = false;

                    // Force player to release jump key before allowing another jump
                    if (!InputManager.KeyDown(Keys.A))
                    {
                        IsAbleToJump = true;
                    }
                }
                else
                {
                    // Just started a jump, or has walked off ledge
                    if (!IsFalling && !IsJumping)
                        IsJumping = true;

                    // Has reached the peak of the jump
                    if (IsJumping && Velocity.Y > 0.0f)
                    {
                        IsJumping = false;
                        IsFalling = true;
                    }
                    // Force player to release jump key before allowing another jump
                    IsAbleToJump = false;
                }

                // Apply external velocity after handling collisions and other movement states
                Position += VelocityExternal;
                VelocityExternal = Vector2.Zero;

                // Fix bad collision value rounding error
                double round = Math.Round((double)Position.Y, 4);
                Position.Y = (float)round;

                // Shooting animation
                IsShooting = ((DateTime.Now - shootAnimationCooldown).TotalMilliseconds > 300) ? false : true;
            }
            else
            {
                // Mega man's auto-scroll velocity
                float scrollSpeed = 0.75f;
                switch (Session.Camera.ScrollDirection)
                {
                    case Direction.Left:
                        Position.X -= scrollSpeed;
                        break;
                    case Direction.Up:
                        Position.Y -= scrollSpeed;
                        break;
                    case Direction.Right:
                        Position.X += scrollSpeed;
                        break;
                    case Direction.Down:
                        Position.Y += scrollSpeed;
                        break;
                }
                
            }

            animatedSprite.Position = new Vector2((float)Math.Ceiling(Position.X), (float)Math.Ceiling(Position.Y));
            animatedSprite.Update(gameTime);

            UpdateBullets(gameTime);
        }

        private void CheckExits()
        {
            for (int i = 0; i < Session.CurrentRoom.Exits.Count; i++)
            {
                Exit theExit = Session.CurrentRoom.Exits[i];

                // Check this exit only if Mega is in the right screen
                if (theExit.Screen != ScreenLocationInRoom) continue;

                bool exit = false;
                switch (theExit.Direction)
                {
                    case Direction.Left:
                        if (Position.X < Session.CurrentRoom.Position.X + Engine.TileWidth)
                            exit = true;
                            // Exit Left
                        break;
                    case Direction.Up:
                        if (Position.Y < Session.CurrentRoom.Position.Y + Engine.TileHeight)
                            exit = true;
                            // Exit Up
                        break;
                    case Direction.Right:
                        if (Position.X + Width >
                            Session.CurrentRoom.Position.X + Session.CurrentRoom.Size.X - Engine.TileWidth)
                            exit = true;
                            // Exit Right
                        break;
                    case Direction.Down:
                        if (Position.Y + Height >
                            Session.CurrentRoom.Position.Y + Session.CurrentRoom.Size.Y - Engine.TileHeight)
                            exit = true;
                            // Exit Down
                        break;
                }
                if (exit)
                {
                    Session.ChangeRooms(theExit);
                    break;
                }
            }
        }

        public void TryGrabLadder(bool up)
        {
            if (IsClimbing) return;

            int topTile = (int)Math.Floor(HitBox.Top / (float)Engine.TileHeight);
            int bottomTile = (int)Math.Ceiling(HitBox.Bottom / (float)Engine.TileHeight);
            int tileX = (int)(Math.Round(HitBox.CenterX) / (double)(Engine.TileWidth));
            int tileY = (int)(Math.Round(HitBox.CenterY) / (double)(Engine.TileWidth));

            // Grab in mid-air
            if (IsFalling && up)
            {
                TileCollision collision = Session.CurrentMap.GetCollision(tileX, topTile, 0);
                if (collision == TileCollision.Ladder)
                {
                    // Attach to ladder
                    Position.X = Engine.CellToVector(new Point(tileX, tileY)).X + 1.0f;
                    Velocity.X = 0.0f;
                    IsFalling = false;
                    IsJumping = false;
                    IsClimbing = true;
                    IsAbleToJump = false;
                }
            }
            // Grab from on the ground
            else
            {
                if (up)
                {
                    TileCollision collision = Session.CurrentMap.GetCollision(tileX, topTile, 0);
                    if (collision == TileCollision.Ladder && !IsFalling)
                    {
                        // Attach to ladder
                        Position.X = Engine.CellToVector(new Point(tileX, tileY)).X + 1.0f;
                        Position.Y -= 2.0f;
                        Velocity.X = 0.0f;
                        IsClimbing = true;
                        IsAbleToJump = false;
                    }
                }
                else
                {
                    TileCollision collision = Session.CurrentMap.GetCollision(tileX, bottomTile, 0);
                    if (collision == TileCollision.Ladder && !IsFalling)
                    {
                        // Attach to ladder
                        Position.X = Engine.CellToVector(new Point(tileX, tileY)).X + 1.0f;
                        Position.Y += 14.0f;
                        Velocity.X = 0.0f;
                        IsClimbing = true;
                        IsAbleToJump = false;
                    }
                }
            }

            //for (int y = topTile; y <= bottomTile; ++y)
            //{
            //    TileCollision collision = Session.CurrentMap.GetCollision(x, y, 0);
            //    if (collision == TileCollision.Ladder)
            //    {
            //        Position.X = Engine.CellToVector(new Point(x, y)).X;
            //        IsClimbing = true;
            //        IsAbleToClimb = false;
            //        IsFalling = false;
            //        IsJumping = false;
            //        break;
            //    }
            //}
        }

        public void HandleTileCollisions()
        {
            FloatRect _hitBox = HitBox;

            int leftTile = (int)Math.Floor(HitBox.Left / (float)Engine.TileWidth);
            int rightTile = (int)Math.Ceiling(HitBox.Right / (float)Engine.TileWidth) - 1;
            int topTile = (int)Math.Floor(HitBox.Top / (float)Engine.TileHeight);
            int bottomTile = (int)Math.Ceiling(HitBox.Bottom / (float)Engine.TileHeight) - 1;

            if (IsClimbing)
            {
                int centerX = (int)Math.Floor(HitBox.CenterX / (float)Engine.TileWidth);
                int centerY = (int)Math.Floor(HitBox.CenterY / (float)Engine.TileHeight);
                TileCollision top = Session.CurrentMap.GetCollision(centerX, topTile, 0);
                TileCollision center = Session.CurrentMap.GetCollision(centerX, centerY, 0);
                TileCollision bottom = Session.CurrentMap.GetCollision(centerX, bottomTile, 0);
                if (bottom == TileCollision.Impassable ||
                    bottom == TileCollision.ConveyorLeft ||
                    bottom == TileCollision.ConveyorRight)
                {
                    // Bottom of ladder against the ground
                    IsClimbing = false;
                    Position.Y = (float)Math.Round(Engine.CellToVector(new Point(centerX, bottomTile)).Y - 22.0f);
                    PosPreviousFrame = Position;
                    return;
                }
                else if (bottom == TileCollision.Passable && top == TileCollision.Passable)
                {
                    // Bottom of ladder against empty space
                    IsClimbing = false;
                    return;
                }
                else if (center == TileCollision.Passable && bottom == TileCollision.Ladder)
                {
                    // Top of ladder
                    IsClimbing = false;
                    Velocity.Y = 0.0f;
                    Position.Y = (float)Math.Round(Engine.CellToVector(new Point(centerX, centerY)).Y - 6.0f);
                    PosPreviousFrame = Position;
                    previousBottom = Position.Y + Height;
                    return;
                }
                //return;
            }

            isOnGround = false;

            // Grab each potentially colliding tile
            List<Tile> left = new List<Tile>();
            List<Tile> right = new List<Tile>();
            List<Tile> down = new List<Tile>();
            List<Tile> up = new List<Tile>();
            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    TileCollision collision = Session.CurrentMap.GetCollision(x, y, 0);
                    if (collision != TileCollision.Passable && collision != TileCollision.Ladder)
                    {
                        if (x == leftTile)
                            left.Add(Session.CurrentMap.GetTileAtIndex(new Point(x, y), 0));
                        else if (x == rightTile)
                            right.Add(Session.CurrentMap.GetTileAtIndex(new Point(x, y), 0));
                        if (y == topTile)
                            up.Add(Session.CurrentMap.GetTileAtIndex(new Point(x, y), 0));
                        else if (y == bottomTile)
                            down.Add(Session.CurrentMap.GetTileAtIndex(new Point(x, y), 0));
                        Session.CurrentMap.GetTileAtIndex(new Point(x, y), 0).StandingOn = true;
                    }
                }
            }

            // For each potentially colliding tile,
            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    // If this tile is collidable,
                    TileCollision collision = Session.CurrentMap.GetCollision(x, y, 0);
                    if (collision != TileCollision.Passable)
                    {
                        // Determine collision depth (with direction) and magnitude.
                        FloatRect tileBounds = Engine.GetTileRect(x, y);

                        // Determine displacement to resolve collision
                        Vector2 depth = FloatRect.GetIntersectionDepth(_hitBox, tileBounds);
                        Vector2 depthCompare = depth;
                        if (depth != Vector2.Zero)
                        {
                            // TODO: Binary search - retest collision at the midpoint of previous position and current position a few times.
                            Vector2 midpoint = MegaMath.Midpoint(Position, PosPreviousFrame);
                            Vector2 prevPos = PosPreviousFrame;
                            Vector2 newPos = midpoint;
                            for (int i = 0; i < 15; i++)
                            {
                                if (tileBounds.Intersects(new FloatRect(newPos, HitBox.Width, HitBox.Height)))
                                {
                                    Vector2 newDepth = FloatRect.GetIntersectionDepth(new FloatRect(newPos, HitBox.Width, HitBox.Height), tileBounds);
                                    midpoint = MegaMath.Midpoint(newPos, prevPos);
                                    newPos = midpoint;
                                    if (newDepth == Vector2.Zero)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        depthCompare = newDepth;
                                        float proportion = Math.Abs(depthCompare.X) / Math.Abs(depthCompare.Y);
                                        if (proportion > 100 || proportion < 0.01) break;
                                    }
                                }
                                else
                                {
                                    Vector2 initPos = prevPos;
                                    prevPos = newPos;
                                    newPos = MegaMath.Midpoint(newPos, newPos + 2 * (newPos - initPos));
                                }

                            }


                            float absDepthX = Math.Abs(depth.X);
                            float absDepthY = Math.Abs(depth.Y);
                            float absDepthCompareX = Math.Abs(depthCompare.X);
                            float absDepthCompareY = Math.Abs(depthCompare.Y);
                            
                            // Resolve the collision along the shallow axis.
                            // Up/Down collision

                            if (absDepthCompareY < absDepthCompareX ||
                                collision == TileCollision.Platform ||
                                collision == TileCollision.Ladder)       // Y-axis collision
                            {
                                // If we crossed the top of a tile, we are on the ground.
                                if (previousBottom <= tileBounds.Top)
                                {
                                    if (collision == TileCollision.Ladder)
                                    {
                                        // Only land on a ladder if it's the topmost ladder tile and not already climbing
                                        if (!IsClimbing &&
                                            Session.CurrentMap.GetCollision(x, y - 1, 0) == TileCollision.Passable)
                                        {
                                            isOnGround = true;
                                        }
                                    }
                                    else
                                    {
                                        isOnGround = true;
                                    }
                                }

                                // Ignore platforms, unless we are on the ground.
                                if (collision == TileCollision.Impassable ||
                                    collision == TileCollision.ConveyorLeft ||
                                    collision == TileCollision.ConveyorRight ||
                                    IsOnGround)
                                {
                                    // Resolve the collision along the Y axis.
                                    Position = new Vector2(Position.X, Position.Y + depth.Y);

                                    // Perform further collisions with the new bounds.
                                    _hitBox = HitBox;
                                }

                                // Apply conveyor velocities
                                if (IsOnGround)
                                {
                                    // TODO: Grab the tile directly beneath mega man.
                                    if (collision == TileCollision.ConveyorLeft)
                                        VelocityExternal = new Vector2(-Tile.ConveyorVelocity, 0);
                                    else if (collision == TileCollision.ConveyorRight)
                                        VelocityExternal = new Vector2(Tile.ConveyorVelocity, 0);
                                }
                            }
                            // Left/Right collision
                            else if (collision == TileCollision.Impassable||
                                    collision == TileCollision.ConveyorLeft ||
                                    collision == TileCollision.ConveyorRight) // Ignore platforms.
                            {
                                // Resolve the collision along the X axis.
                                Position = new Vector2(Position.X + depth.X, Position.Y);

                                // Perform further collisions with the new bounds.
                                _hitBox = HitBox;
                            }
                        } // end if depth != 0

                    } // end if collision != passable

                } // end for all X tiles

            } // end for all Y tiles


            // Save the new bounds bottom.
            previousBottom = _hitBox.Bottom;


        }

        public void HandleSpecialCollisions() 
        {
            FloatRect PreviousHitbox = HitBox;

            foreach (Obstacle p in Session.Level.Obstacles)
            {
                // If solid object and collision detected
                if (p.Bounds.Intersects(HitBox))
                {
                    if (!p.IsCollidable) continue;

                    // Fire event on obstacle
                    p.NotifyPlayerCollision();

                    // Handle typical solid collisions
                    Vector2 depth = FloatRect.GetIntersectionDepth(HitBox, p.Bounds);
                    Vector2 depthCompare = depth;

                    if (depth != Vector2.Zero)
                    {
                        Vector2 midpoint = MegaMath.Midpoint(Position, PosPreviousFrame);
                        Vector2 prevPos = PosPreviousFrame;
                        Vector2 newPos = midpoint;

                        for (int i = 0; i < 15; i++)
                        {
                            if (p.Bounds.Intersects(new FloatRect(newPos, HitBox.Width, HitBox.Height)))
                            {
                                Vector2 newDepth = FloatRect.GetIntersectionDepth(new FloatRect(newPos, HitBox.Width, HitBox.Height), p.Bounds);
                                midpoint = MegaMath.Midpoint(newPos, prevPos);
                                newPos = midpoint;
                                if (newDepth == Vector2.Zero)
                                {
                                    break;
                                }
                                else
                                {
                                    depthCompare = newDepth;
                                    float proportion = Math.Abs(depthCompare.X) / Math.Abs(depthCompare.Y);
                                    if (proportion > 100 || proportion < 0.01) break;
                                }
                            }
                            else
                            {
                                Vector2 initPos = prevPos;
                                prevPos = newPos;
                                newPos = MegaMath.Midpoint(newPos, newPos + 2 * (newPos - initPos));
                            }
                        } // end for

                        float absDepthX = Math.Abs(depth.X);
                        float absDepthY = Math.Abs(depth.Y);
                        float absDepthCompareX = Math.Abs(depthCompare.X);
                        float absDepthCompareY = Math.Abs(depthCompare.Y);

                        // Resolve the collision along the shallow axis.
                        if (absDepthCompareY < absDepthCompareX) // Up/Down collision
                        {
                            // If we crossed the top of a tile, we are on the ground.
                            if (PreviousHitbox.Bottom <= p.Bounds.Top)
                            {
                                isOnGround = true;
                            }

                            // Resolve the collision along the Y axis.
                            Position = new Vector2(Position.X, Position.Y + depth.Y);

                            // Notify platform that we are standing on it
                            if (Position.Y < p.Bounds.Top)
                            {
                                p.NotifyPlayerStanding();
                            }
                        }
                        else // Left/Right collision
                        {
                            // Resolve the collision along the X axis.
                            Position = new Vector2(Position.X + depth.X, Position.Y);
                        }
                    }
                }
            } // end foreach
        }

        public void UpdateBullets(GameTime gameTime)
        {
            foreach (Bullet bullet in Bullets)
            {
                bullet.Update(gameTime);
            }
        }

        #endregion

        #region Drawing

        public void DrawBullets(GameTime gameTime, SpriteBatch spriteBatch)
        {
            foreach (Bullet bullet in Bullets)
            {
                bullet.Draw(gameTime, spriteBatch);
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (IsClimbing)
            {
                if (IsShooting)
                {
                    if (IsFacingLeft)
                    {
                        animatedSprite.CurrentAnimation = "ClimbShootLeft";
                        animatedSprite.SetCurrentFrame("Climb", 0);
                    }
                    else
                    {
                        animatedSprite.CurrentAnimation = "ClimbShootRight";
                        animatedSprite.SetCurrentFrame("Climb", 1);
                    }
                }
                else
                {
                    animatedSprite.CurrentAnimation = "Climb";
                }

                if (Velocity.Y == 0.0f)
                    animatedSprite.FrameTimer = TimeSpan.Zero;
            }
            // Jumping/Falling
            else if (IsFalling || IsJumping)
            {
                if (IsFacingLeft)
                {
                    if (IsShooting)
                    {
                        animatedSprite.CurrentAnimation = "JumpShootLeft";
                    }
                    else
                    {
                        animatedSprite.CurrentAnimation = "JumpLeft";
                    }
                }
                else
                {
                    if (IsShooting)
                    {
                        animatedSprite.CurrentAnimation = "JumpShootRight";
                    }
                    else
                    {
                        animatedSprite.CurrentAnimation = "JumpRight";
                    }
                }
            }
            // On the ground
            else
            {

                // Walking left
                if (Velocity.X < -0.001f)
                {
                    if (IsShooting)
                    {
                        // Sync walking and walkshoot animations
                        if (animatedSprite.CurrentAnimation == "WalkLeft")
                        {
                            int frame = animatedSprite.Frame;
                            TimeSpan frameTime = animatedSprite.FrameTimer;
                            animatedSprite.CurrentAnimation = "WalkShootLeft";
                            animatedSprite.Frame = frame;
                            animatedSprite.FrameTimer = frameTime;
                        }
                        else
                        {
                            animatedSprite.CurrentAnimation = "WalkShootLeft";
                        }
                    }
                    else
                    {
                        // Sync walking and walkshoot animations
                        if (animatedSprite.CurrentAnimation == "WalkShootLeft")
                        {
                            int frame = animatedSprite.Frame;
                            TimeSpan frameTime = animatedSprite.FrameTimer;
                            animatedSprite.CurrentAnimation = "WalkLeft";
                            animatedSprite.Frame = frame;
                            animatedSprite.FrameTimer = frameTime;
                        }
                        else
                        {
                            animatedSprite.CurrentAnimation = "WalkLeft";
                        }
                    }
                }
                // Walking right
                else if (Velocity.X > 0.001f)
                {
                    if (IsShooting)
                    {
                        // Sync walking and walkshoot animations
                        if (animatedSprite.CurrentAnimation == "WalkRight")
                        {
                            int frame = animatedSprite.Frame;
                            TimeSpan frameTime = animatedSprite.FrameTimer;
                            animatedSprite.CurrentAnimation = "WalkShootRight";
                            animatedSprite.Frame = frame;
                            animatedSprite.FrameTimer = frameTime;
                        }
                        else
                        {
                            animatedSprite.CurrentAnimation = "WalkShootRight";
                        }
                    }
                    else
                    {
                        // Sync walking and walkshoot animations
                        if (animatedSprite.CurrentAnimation == "WalkShootRight")
                        {
                            int frame = animatedSprite.Frame;
                            TimeSpan frameTime = animatedSprite.FrameTimer;
                            animatedSprite.CurrentAnimation = "WalkRight";
                            animatedSprite.Frame = frame;
                            animatedSprite.FrameTimer = frameTime;
                        }
                        else
                        {
                            animatedSprite.CurrentAnimation = "WalkRight";
                        }
                    }
                }
                // Standing still
                else
                {
                    if (IsFacingLeft)
                    {
                        if (IsShooting)
                        {
                            animatedSprite.CurrentAnimation = "StandShootLeft";
                        }
                        else
                        {
                            animatedSprite.CurrentAnimation = "StandLeft";
                        }
                    }
                    else
                    {
                        if (IsShooting)
                        {
                            animatedSprite.CurrentAnimation = "StandShootRight";
                        }
                        else
                        {
                            animatedSprite.CurrentAnimation = "StandRight";
                        }
                    }
                }
            }
            
            animatedSprite.Draw(spriteBatch);

            // Draw weapon energy
            if (CurrentWeapon != Weapon.MEGA_BUSTER)
            {
                Session.WeaponMeter.Draw(spriteBatch, WeaponEnergy[(int)CurrentWeapon]);
            }

            if (Session.DebugHitboxes)
            {
                debugRectangle.Draw(spriteBatch, Position, Color.Red, debugRectTex);
            }

            DrawBullets(gameTime, spriteBatch);
        }
        #endregion

        #region Sprite Sheet Frame Rectangles
        public static Rectangle mm_walkleft_0 = new Rectangle(43, 38, 22, 22);
        public static Vector2 mm_walkleft_offset0 = new Vector2(-1,0);
        public static Rectangle mm_walkleft_1 = new Rectangle(66, 36, 16, 24);
        public static Vector2 mm_walkleft_offset1 = new Vector2(1, -2);
        public static Rectangle mm_walkleft_2 = new Rectangle(84, 38, 24, 22);
        public static Vector2 mm_walkleft_offset2 = new Vector2(-5, 0);
        public static Rectangle mm_walkright_0 = new Rectangle(208, 38, 22, 22);
        public static Vector2 mm_walkright_offset0 = new Vector2(-7, 0);
        public static Rectangle mm_walkright_1 = new Rectangle(190, 36, 16, 24);
        public static Vector2 mm_walkright_offset1 = new Vector2(-3,-2);
        public static Rectangle mm_walkright_2 = new Rectangle(164, 38, 24, 22);
        public static Vector2 mm_walkright_offset2 = new Vector2(-5, 0);
        public static Rectangle mm_walkshootleft_0 = new Rectangle(68, 96, 29, 22);
        public static Vector2 mm_walkshootleft_offset0 = new Vector2(-9, 0);
        public static Rectangle mm_walkshootright_0 = new Rectangle(175, 96, 29, 22);
        public static Vector2 mm_walkshootright_offset0 = new Vector2(-6, 0);
        public static Rectangle mm_walkshootleft_1 = new Rectangle(38, 94, 26, 24);
        public static Vector2 mm_walkshootleft_offset1 = new Vector2(-9, -2);
        public static Rectangle mm_walkshootright_1 = new Rectangle(208, 94, 26, 24);
        public static Vector2 mm_walkshootright_offset1 = new Vector2(-3, -2);
        public static Rectangle mm_walkshootleft_2 = new Rectangle(5, 96, 30, 22);
        public static Vector2 mm_walkshootleft_offset2 = new Vector2(-9, 0);
        public static Rectangle mm_walkshootright_2 = new Rectangle(237, 96, 30, 22);
        public static Vector2 mm_walkshootright_offset2 = new Vector2(-7, 0);

        public static Rectangle mm_standleft = new Rectangle(46, 10, 22, 24);
        public static Vector2 mm_standleft_offset = new Vector2(-3, -2);
        public static Rectangle mm_standright = new Rectangle(205, 10, 22, 24);
        public static Vector2 mm_standright_offset = new Vector2(-4, -2);
        public static Rectangle mm_standshootleft = new Rectangle(100, 94, 31, 24);
        public static Vector2 mm_standshootleft_offset = new Vector2(-13, -2);
        public static Rectangle mm_standshootright = new Rectangle(141, 94, 31, 24);
        public static Vector2 mm_standshootright_offset = new Vector2(-4, -2);
        public static Rectangle mm_jumpleft = new Rectangle(105, 62, 26, 30);
        public static Vector2 mm_jumpleft_offset = new Vector2(-6, -1);
        public static Rectangle mm_jumpright = new Rectangle(141, 62, 26, 30);
        public static Vector2 mm_jumpright_offset = new Vector2(-6, -1);
        public static Rectangle mm_jumpshootleft = new Rectangle(44, 62, 29, 30);
        public static Vector2 mm_jumpshootleft_offset = new Vector2(-9, -1);
        public static Rectangle mm_jumpshootright = new Rectangle(199, 62, 29, 30);
        public static Vector2 mm_jumpshootright_offset = new Vector2(-6, -1);

        public static Rectangle mm_climb0 = new Rectangle(86, 120, 16, 29);
        public static Vector2 mm_climb_offset0 = new Vector2(-1, -2);
        public static Rectangle mm_climb1 = new Rectangle(170, 120, 16, 29);
        public static Vector2 mm_climb_offset1 = new Vector2(-1, -2);
        public static Rectangle mm_climbshootleft = new Rectangle(35, 120, 24, 29);
        public static Vector2 mm_climbshootleftoffset = new Vector2(-9, -2);
        public static Rectangle mm_climbshootright = new Rectangle(213, 120, 24, 29);
        public static Vector2 mm_climbshootrightoffset = new Vector2(-1, -2);

        public static Rectangle[] mm_walkleft = { mm_walkleft_0, mm_walkleft_1, mm_walkleft_2, mm_walkleft_1 };
        public static Rectangle[] mm_walkright = { mm_walkright_0, mm_walkright_1, mm_walkright_2, mm_walkright_1 };
        public static Rectangle[] mm_walkshootleft = { mm_walkshootleft_0, mm_walkshootleft_1, mm_walkshootleft_2, mm_walkshootleft_1 };
        public static Rectangle[] mm_walkshootright = { mm_walkshootright_0, mm_walkshootright_1, mm_walkshootright_2, mm_walkshootright_1 };
        public static Rectangle[] mm_climb = { mm_climb0, mm_climb1 };
        public static Vector2[] mm_walkleft_offsets = { mm_walkleft_offset0, mm_walkleft_offset1, mm_walkleft_offset2, mm_walkleft_offset1 };
        public static Vector2[] mm_walkright_offsets = { mm_walkright_offset0, mm_walkright_offset1, mm_walkright_offset2, mm_walkright_offset1 };
        public static Vector2[] mm_walkshootleft_offsets = { mm_walkshootleft_offset0, mm_walkshootleft_offset1, mm_walkshootleft_offset2, mm_walkshootleft_offset1 };
        public static Vector2[] mm_walkshootright_offsets = { mm_walkshootright_offset0, mm_walkshootright_offset1, mm_walkshootright_offset2, mm_walkshootright_offset1 };
        public static Vector2[] mm_climb_offsets = { mm_climb_offset0, mm_climb_offset1 };
        
        #endregion
    }

    public enum AnimationState
    {
    }


}

