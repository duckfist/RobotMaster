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
    public abstract class Bullet : Entity
    {
        protected static Texture2D bulletTexture;
        protected static Texture2D weaponsTexture;

        public bool InUse { get; set; }
        

        public Bullet(Vector2 position)
            : base(position)
        {
            InUse = false;
        }

        public Bullet()
            : base(new Vector2(128, 128))
        {
            InUse = false;
        }

        public static void LoadContent(ContentManager Content)
        {
            // Load bullet images from sprite sheet
            bulletTexture = Content.Load<Texture2D>(@"Sprites\Sprites_mm2Items");
            weaponsTexture = Content.Load<Texture2D>(@"Sprites\Sprites_mm10Weapons");

            // Load all bullet types for mega man
            MegaBuster.Load();
            TripleBlade.Load();
            SolarBlaze.Load();
            WaterShield.Load();

            // Load all bullet types for enemies
            MedusaBullet.Load();
        }

        #region Inhertible Members

        public override void Update(GameTime gameTime)
        {
            if (!IsOnScreen && InUse)
            {
                // Reset so player can shoot again
                InUse = false;
            }
        }

        public abstract float Damage { get; }
        public abstract bool IsOnScreen { get; }
        public abstract bool HitTest(FloatRect rect, float HP = 0f);

        public static Rectangle ExplodeContentRect0 = new Rectangle(166, 494, 12, 12);
        public static Rectangle ExplodeContentRect1 = new Rectangle(181, 261, 12, 12);
        public static Rectangle ExplodeContentRect2 = new Rectangle(192, 261, 12, 12);
        public static Rectangle[] ExplodeContentRects = { ExplodeContentRect0, ExplodeContentRect1, ExplodeContentRect2 };

        #endregion
    }
}
