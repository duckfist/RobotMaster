using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Threading;


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;

using RobotMaster.Mathematics;
using RobotMaster.TileEngine;
using RobotMaster.Entities;

using DebuggerHost;

namespace RobotMaster.GameComponents
{
    public class Session
    {
        #region Fields and Properties

        private static readonly Session instance = new Session();
        public static Session Instance { get { return instance; } }
        public static Game1 GameRef;
        public static Camera Camera;
        public static Engine Engine;
        public static TileMap CurrentMap;
        public static Room CurrentRoom { get { return CurrentMap.CurrentRoom; } }
        public static PlayerIndex? PlayerInControl;
        public static StorageDevice StorageDevice = null;
        public static SaveData GameData = new SaveData();
        public static Level Level;
        public static MeterDisplay HPMeter;
        public static MeterDisplay WeaponMeter;

        public static MegaMan MegaMan;
        //public static List<Enemy> Enemies = new List<Enemy>();
        public static bool DebugMode = true;
        public static bool DebugPause = false;
        public static bool DebugHitboxes = false;
        public static bool DebugFrameIsAdvancing = false;
        public static Thread thDebug;

        public static Vector2 ScrollDestination = Vector2.Zero;
        public static bool IsScrolling = false;

        public static Texture2D TexEnemies;
        public static Texture2D TexObjects;
        public static Texture2D TexObstacles;
        public static Effect Shader;

        //public static AudioEngine audioEngine;
        //public static WaveBank waveBank;
        //public static SoundBank soundBank;

        #endregion

        #region Constructors

        private Session() { }

        #endregion

        #region Methods
        
        public void Initialize(Game1 game)
        {
            
            GameRef = (Game1)game;
            Camera = new Camera();
            Engine = new Engine(16, 16, game);
            
        }

        public static void DrawMap(SpriteBatch spriteBatch)
        {
            CurrentMap.Draw(spriteBatch);
            Tileset.Tick(); // Animate all tiles
        }

        public static void LoadContent(ContentManager Content)
        {
            //TexEnemies = Content.Load<Texture2D>(@"Sprites\Enemies");
            //TexObjects = Content.Load<Texture2D>(@"Sprites\Objects");
            //TexObstacles = Content.Load<Texture2D>(@"Sprites\Obstacles");

            // Basic audio engine
            //audioEngine = new AudioEngine("Content\\Audio\\bgm.xgs");
            //waveBank = new WaveBank(audioEngine, "Content\\Audio\\Wave Bank.xwb");
            //soundBank = new SoundBank(audioEngine, "Content\\Audio\\Sound Bank.xsb");
        }

        public static void ChangeRooms(Exit exit)
        {
            // Clean up the current room
            foreach (Bullet bullet in Session.MegaMan.Bullets)
            {
                bullet.InUse = false;
            }

            // begin scrolling routine
            Session.Camera.ScrollDirection = exit.Direction;
            Session.CurrentMap.CurrentRoomNum = exit.Destination;
            switch (exit.Direction)
            {
                case Direction.Left:
                    Session.Camera.Destination = Session.Camera.Position - new Vector2(Engine.ViewportWidth,0);
                    break;
                case Direction.Up:
                    Session.Camera.Destination = Session.Camera.Position - new Vector2(0, Engine.ViewportHeight);
                    break;
                case Direction.Right:
                    Session.Camera.Destination = Session.Camera.Position + new Vector2(Engine.ViewportWidth, 0);
                    break;
                case Direction.Down:
                    Session.Camera.Destination = Session.Camera.Position + new Vector2(0, Engine.ViewportHeight);
                    break;
            }
            Session.IsScrolling = true;  // TODO: Change this to some routine to automatically move camera.
                                         // For now, the Camera's update takes care of it when this is true.
        }

        public static void SetShaderColor(Weapon CurrentWeapon)
        {
            switch (CurrentWeapon)
            {
                case Weapon.MEGA_BUSTER:
                    //Shader.Parameters["MegaMan1"].SetValue(new Color(0, 112, 236).ToVector4());
                    //Shader.Parameters["MegaMan2"].SetValue(new Color(0, 232, 216).ToVector4());
                    //Shader.Parameters["Weapon"].SetValue(new Color(252, 228, 160).ToVector4());
                    break;
                case Weapon.TRIPLE_BLADE:
                    //Shader.Parameters["MegaMan1"].SetValue(new Color(184, 0, 184).ToVector4());
                    //Shader.Parameters["MegaMan2"].SetValue(new Color(248, 248, 248).ToVector4());
                    //Shader.Parameters["Weapon"].SetValue(new Color(184, 0, 184).ToVector4());
                    break;
                case Weapon.WATER_SHIELD:
                    //Shader.Parameters["MegaMan1"].SetValue(new Color(0, 112, 232).ToVector4());
                    //Shader.Parameters["MegaMan2"].SetValue(new Color(168, 224, 248).ToVector4());
                    //Shader.Parameters["Weapon"].SetValue(new Color(0, 112, 232).ToVector4());
                    break;
                case Weapon.SOLAR_BLAZE:
                    //Shader.Parameters["MegaMan1"].SetValue(new Color(200, 72, 8).ToVector4());
                    //Shader.Parameters["MegaMan2"].SetValue(new Color(240, 184, 56).ToVector4());
                    //Shader.Parameters["Weapon"].SetValue(new Color(200, 72, 8).ToVector4());
                    break;
            }
        }

        public static void SaveGame(string gamename)
        {
            // Open a storage container
            IAsyncResult result = StorageDevice.BeginOpenContainer("MegaMan10x", null, null);

            // Wait for WaitHandle to be signaled
            result.AsyncWaitHandle.WaitOne();

            StorageContainer container = StorageDevice.EndOpenContainer(result);

            // Close the wait handle.
            result.AsyncWaitHandle.Close();

            //string filename = "savegame.sav";
            if (container.FileExists(gamename))
                container.DeleteFile(gamename);
            FileStream stream = new FileStream(gamename, FileMode.Create);
            XmlSerializer serializer = new XmlSerializer(typeof(SaveData));
            serializer.Serialize(stream, GameData);
            stream.Close();
            container.Dispose();
        }

        public static CollisionResult CollisionCheckTilemap(Vector2 Position, Vector2 PreviousPosition, FloatRect Hitbox)
        {
            CollisionResult result = new CollisionResult(Position);
            FloatRect PreviousHitbox = new FloatRect(PreviousPosition, Hitbox.Width, Hitbox.Height);

            int leftTile = (int)Math.Floor(Hitbox.Left / (float)Engine.TileWidth);
            int rightTile = (int)Math.Ceiling(Hitbox.Right / (float)Engine.TileWidth) - 1;
            int topTile = (int)Math.Floor(Hitbox.Top / (float)Engine.TileHeight);
            int bottomTile = (int)Math.Ceiling(Hitbox.Bottom / (float)Engine.TileHeight) - 1;

            // Grab each potentially colliding tile
            List<Tile> left = new List<Tile>();
            List<Tile> right = new List<Tile>();
            List<Tile> down = new List<Tile>();
            List<Tile> up = new List<Tile>();
            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    Tile tile = Session.CurrentMap.GetTileAtIndex(new Point(x, y), 0);
                    if (tile.Collision != TileCollision.Passable && tile.Collision != TileCollision.Ladder)
                    {
                        if (x == leftTile) left.Add(tile);
                        else if (x == rightTile) right.Add(tile);
                        if (y == topTile) up.Add(tile);
                        else if (y == bottomTile) down.Add(tile);
                        tile.StandingOn = true;
                    }
                }
            }

            // For each potentially colliding tile,
            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    Tile tile = Session.CurrentMap.GetTileAtIndex(new Point(x, y), 0);
                    TileCollision collision = tile.Collision;

                    // If this tile is collidable,
                    if (collision != TileCollision.Passable)
                    {
                        // Add to list of collided tiles
                        result.Tiles.Add(tile);

                        // Determine collision depth (with direction) and magnitude.
                        FloatRect tileBounds = Engine.GetTileRect(x, y);

                        // Determine displacement to resolve collision
                        Vector2 depth = FloatRect.GetIntersectionDepth(Hitbox, tileBounds);
                        Vector2 depthCompare = depth;
                        if (depth != Vector2.Zero)
                        {
                            Vector2 midpoint = MegaMath.Midpoint(result.NewPosition, PreviousPosition);
                            Vector2 prevPos = PreviousPosition;
                            Vector2 newPos = midpoint;
                            for (int i = 0; i < 15; i++)
                            {
                                if (tileBounds.Intersects(new FloatRect(newPos, Hitbox.Width, Hitbox.Height)))
                                {
                                    Vector2 newDepth = FloatRect.GetIntersectionDepth(new FloatRect(newPos, Hitbox.Width, Hitbox.Height), tileBounds);
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
                            if (absDepthCompareY < absDepthCompareX ||
                                collision == TileCollision.Platform ||
                                collision == TileCollision.Ladder)       // Y-axis collision
                            {
                                // If we crossed the top of a tile, we are on the ground.
                                if (PreviousHitbox.Bottom <= tileBounds.Top)
                                {
                                    result.HasLanded = true;
                                }
                                // If we crossed the bottom, we have hit the ceiling.
                                if (PreviousHitbox.Top <= tileBounds.Bottom)
                                {
                                    result.HasHitCeiling = true;
                                }

                                // Ignore platforms, unless we are on the ground.
                                if (collision == TileCollision.Impassable ||
                                    collision == TileCollision.ConveyorLeft ||
                                    collision == TileCollision.ConveyorRight || 
                                    result.HasLanded)
                                {
                                    // Resolve the collision along the Y axis.
                                    result.NewPosition = new Vector2(Position.X, Position.Y + depth.Y);

                                    // Perform further collisions with the new bounds.
                                    Hitbox = new FloatRect(result.NewPosition, Hitbox.Width, Hitbox.Height);
                                }
                            }
                            // Left/Right collision
                            else if (collision == TileCollision.Impassable ||
                                    collision == TileCollision.ConveyorLeft ||
                                    collision == TileCollision.ConveyorRight) // Ignore platforms.
                            {
                                // Resolve the collision along the X axis.
                                result.NewPosition = new Vector2(Position.X + depth.X, Position.Y);

                                // Perform further collisions with the new bounds.
                                Hitbox = new FloatRect(result.NewPosition, Hitbox.Width, Hitbox.Height);
                            }
                        } // end if depth != 0

                    } // end if collision != passable

                } // end for all X tiles

            } // end for all Y tiles

            return result;
        }

        #region Debugging

        public static void SetupDebugger(GameTime gameTime)
        {
            // Launch debugger thread window and message loop
            thDebug = new Thread(() =>
                {
                    //reset.Set();
                    Debugger.Show(HandleDebuggerEvents);
                });
            thDebug.SetApartmentState(ApartmentState.STA);
            thDebug.Start();

            Thread.Sleep(10);

            // Launch debugger updater thread
            Thread th = new Thread(() => 
                {
                    UpdateDebugger(gameTime);
                    //reset.WaitOne();
                });
            th.Start();
            
        }

        public static void UpdateDebugger(GameTime gameTime)
        {
            while (DebugMode)
            {
                try
                {
                    DebugInfo debugInfo = new DebugInfo();
                    debugInfo.ScreenStack = GameRef.ScreenManager.ToString();
                    debugInfo.ElapsedGameTime = gameTime.ElapsedGameTime;
                    debugInfo.TotalGameTime = gameTime.TotalGameTime;
                    debugInfo.IsRunningSlowly = gameTime.IsRunningSlowly;

                    Debugger.WriteGameDebug(debugInfo);
                    //Debugger.IsBaseGameEnabled = true;
                }
                catch (Exception e)
                {
                    //Debugger.IsBaseGameEnabled = false;
                }

                try
                {
                    DebugInfo debugInfo = new DebugInfo();
                    debugInfo.megaX = MegaMan.Position.X;
                    debugInfo.megaY = MegaMan.Position.Y;
                    debugInfo.megaRight = MegaMan.HitBox.Right;
                    debugInfo.megaDown = MegaMan.HitBox.Bottom;
                    debugInfo.cameraX = Camera.Position.X;
                    debugInfo.cameraY = Camera.Position.Y;
                    debugInfo.velocityX = MegaMan.Velocity.X;
                    debugInfo.velocityY = MegaMan.Velocity.Y;
                    debugInfo.IsAbleToJump = MegaMan.IsAbleToJump;
                    debugInfo.IsFalling = MegaMan.IsFalling;
                    debugInfo.IsClimbing = MegaMan.IsClimbing;
                    debugInfo.IsJumping = MegaMan.IsJumping;
                    debugInfo.RoomNum = CurrentRoom.RoomNumber;

                    Debugger.WriteGameplayScreenDebug(debugInfo);
                    //Debugger.IsGameplayScreenEnabled = true;
                }
                catch (Exception e)
                {
                    //Debugger.IsGameplayScreenEnabled = false;
                }

                Thread.Sleep(10);
            }
        }

        public static void HandleDebuggerEvents(object sender, MMDebugger.MMDebugEventArgs args)
        {
            if (args.RestartLevel == true)
            {
                // Restart the current level
                Session.CurrentMap.CurrentRoomNum = 0;
                Point spawnP = new Point(Session.CurrentMap.CurrentRoom.RoomLocation.X * Engine.ViewportWidthTiles + Session.CurrentMap.SpawnTile.X,
                    Session.CurrentMap.CurrentRoom.RoomLocation.Y * Engine.ViewportHeightTiles + Session.CurrentMap.SpawnTile.Y);
                Vector2 spawn = Engine.CellToVector(spawnP);
                spawn.X += (Engine.TileWidth - (MegaMan.Width / 2f));
                spawn.Y += ((Engine.TileHeight * 2) - MegaMan.Height);
                Session.MegaMan.Position = spawn;
                IsScrolling = false;
            }
            if (args.PauseLevel == true)
            {
                DebugPause = true;
            }
            else if (args.PauseLevel == false)
            {
                DebugPause = false;
            }
            if (args.DrawHitboxes == true)
            {
                DebugHitboxes = true;
            }
            else if (args.DrawHitboxes == false)
            {
                DebugHitboxes = false;
            }
        }
        #endregion

        public static void Session_Exiting(object sender, EventArgs e)
        {
            // Shutdown debugger window and message loop thread
            if (DebugMode)
            {
                Debugger.Exit(thDebug);
            }

            // Stop the debug updater
            DebugMode = false;
        }

        #endregion

    }

    public class CollisionResult
    {
        public List<Tile> Tiles = new List<Tile>();
        public Vector2 NewPosition = Vector2.Zero;
        public bool HasLanded = false;
        public bool HasHitCeiling = false;

        public bool HasCollided
        {
            get
            {
                return (Tiles.Count > 0);
            }
        }

        public CollisionResult(Vector2 Pos)
        {
            this.NewPosition = Pos;
        }
        public CollisionResult() { }
    }
}
