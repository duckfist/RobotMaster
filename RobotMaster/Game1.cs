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


using RobotMaster.GameScreens;
using RobotMaster.GameComponents;

namespace RobotMaster
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        #region Fields and Properties

        public static int WINDOW_WIDTH = 256;
        public static int WINDOW_HEIGHT = 224;

        public Session Session = Session.Instance;

        public static GraphicsDeviceManager graphics;
        InputManager inputManager;

        // Default to 256x224
        public static Matrix Scale = Matrix.Identity;

        public static SpriteBatch SpriteBatch;
        public TitleScreen TitleScreen;
        public StartScreen StartScreen;
        public GamePlayScreen GamePlayScreen;
        public StageSelectScreen StageSelectScreen;
        public PauseScreen PauseScreen;
        public ScreenManager ScreenManager;

        private static IntPtr gfxHandle;

        #endregion

        public Game1(IntPtr handle) : this()
        {
            gfxHandle = handle;
            graphics.PreparingDeviceSettings += OnPrepareDeviceSettings;
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;
            graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
            graphics.IsFullScreen = false;
            Window.Title = "Mega Man 10x";

            this.Exiting += new EventHandler<EventArgs>(Session.Session_Exiting);
            
            #if XBOX360            
            Components.Add(new GamerServicesComponent(this));
            #endif

            Session.Initialize(this);

            inputManager = new InputManager(this);
            ScreenManager = new ScreenManager(this);

            // Stuff added to Components will have Initalize, Draw, and Update called as usual
            Components.Add(inputManager);
            Components.Add(ScreenManager);

            // Make all screens, set first active screen the start screen
            TitleScreen = new TitleScreen(this, ScreenManager);
            StartScreen = new StartScreen(this, ScreenManager);
            GamePlayScreen = new GamePlayScreen(this, ScreenManager);
            StageSelectScreen = new StageSelectScreen(this, ScreenManager);
            PauseScreen = new PauseScreen(this, ScreenManager);
            ScreenManager.ChangeScreens(TitleScreen);

            // IsFixedTimeStep = false; // lol super speed
            graphics.SynchronizeWithVerticalRetrace = false;
        }

        void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            // Make changes to handle the new window size.            
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        /// 
        protected override void Initialize()
        {
            base.Initialize();

        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        /// 
        
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            float scale = (float)graphics.GraphicsDevice.Viewport.Width / 256.0f;
            Scale = Matrix.CreateScale(scale, scale, 1);
            
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            //if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            //    this.Exit();

            // Setup the debug window, feeding it the gameTime object
            if (Session.thDebug == null && Session.DebugMode)
            {
                Session.SetupDebugger(gameTime);
            }

            // Change the window size
            if (InputManager.KeyDown(Keys.OemPlus))
            {
                graphics.PreferredBackBufferHeight = 896;
                graphics.PreferredBackBufferWidth = 1024;
                float scale = (float)graphics.GraphicsDevice.Viewport.Width / 256.0f;
                Scale = Matrix.CreateScale(scale, scale, 1);
                graphics.ApplyChanges();
                
            }
            if (InputManager.KeyDown(Keys.OemMinus))
            {
                graphics.PreferredBackBufferHeight = 224;
                graphics.PreferredBackBufferWidth = 256;
                float scale = (float)graphics.GraphicsDevice.Viewport.Width / 256.0f;
                Scale = Matrix.CreateScale(scale, scale, 1);
                graphics.ApplyChanges();
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Gray);
            
            //SpriteBatch.Begin();
            base.Draw(gameTime);
            //SpriteBatch.End();
        }

        private void OnPrepareDeviceSettings(object sender, PreparingDeviceSettingsEventArgs args)
        {
            args.GraphicsDeviceInformation.PresentationParameters.DeviceWindowHandle = gfxHandle;
        }
    }
}