using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using RobotMaster.GameComponents;

namespace RobotMaster.Controls
{
    public abstract class Control
    {
        #region Field Region

        protected string name;
        protected string text;
        protected Vector2 size;
        protected Vector2 position;
        protected object value;
        protected bool hasFocus;
        protected bool enabled;
        protected bool visible;
        protected bool tabStop;
        protected SpriteFont spriteFont;
        protected Color color;
        protected string type;

        #endregion

        #region Event Region

        public event EventHandler Selected;

        #endregion

        #region Property Region

        public virtual string Name { get { return name; } set { name = value; } }
        public virtual string Text { get { return text; } set { text = value; } }
        public virtual Vector2 Size { get { return size; } set { size = value; } }
        public virtual Vector2 Position { get { return position; } set { position = value; } }
        public virtual object Value { get { return value; } set { this.value = value; } }
        public virtual bool HasFocus { get { return hasFocus; } set { hasFocus = value; } }
        public virtual bool IsEnabled { get { return enabled; } set { enabled = value; } }
        public virtual bool IsVisible { get { return visible; } set { visible = value; } }
        public virtual bool TabStop { get { return tabStop; } set { tabStop = value; } } // Whether or not the control is selectable
        public virtual SpriteFont SpriteFont { get { return spriteFont; } set { spriteFont = value; } }
        public virtual Color Color { get { return color; } set { color = value; } }
        public string Type { get { return type; } set { type = value; } }

        #endregion

        #region Constructor Region

        public Control()
        {
        }

        #endregion

        #region Abstract Methods

        public abstract void Update(GameTime gameTime);
        public abstract void Draw(SpriteBatch spriteBatch);
        public abstract void HandleInput(PlayerIndex playerIndex);

        #endregion

        #region Virtual Methods

        protected virtual void OnSelected(EventArgs e)
        {
            if (Selected != null)
            {
                Selected(this, e);
            }
        }

        #endregion

    }
}
