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
    public abstract class Boss : Enemy
    {
        public BossState BossCurrentState;
        protected AnimatedSprite spriteDamageFlash;
        protected float fakeHP = 0;
        protected readonly float fakeHPPerFrame = 0.2f;
        protected readonly int invulnTime = 20;
        protected int invulnCount = 20;

        public Boss(Vector2 position)
            : base(position)
        {
            BossCurrentState = BossState.Inactive;
        }

        public override void Update(GameTime gameTime)
        {
            switch (BossCurrentState)
            {
                case BossState.Inactive:
                    // Wait for player to enter boss room
                    if (IsOnScreen)
                    {
                        Activate();
                    }
                    break;
                case BossState.Intro:
                    break;
                case BossState.Health:
                    break;
                case BossState.Fight:
                    break;
                case BossState.Dead:
                    break;
            }
        }

        public override void LoadAnimations()
        {
            Dictionary<string, Animation> dict = new Dictionary<string, Animation>();
            Animation a = new Animation(new Rectangle[2]{ new Rectangle(232, 20, 24, 24), new Rectangle(0,0,0,0) }, new Vector2[2]{ new Vector2(-6,-2), new Vector2()});
            a.FramesPerSecond = 30;
            dict.Add("Default", a);

            spriteDamageFlash = new AnimatedSprite(mm10EnemySprites, dict);
            spriteDamageFlash.CurrentAnimation = "Default";
            spriteDamageFlash.IsAnimating = true;
        }

        public override void Activate()
        {
            Position = defaultPosition;
            fakeHP = 0;
            BossCurrentState = BossState.Intro;
        }

        public override void Deactivate()
        {
            BossCurrentState = BossState.Inactive;
        }

        public enum BossState
        {
            Inactive,
            Intro,
            Health,
            Fight,
            Dead
        }

    }
}
