using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RobotMaster.Entities
{
    public struct AttributePair
    {
        #region Fields and Properties

        float currentValue;
        float maximumValue;

        public float CurrentValue { get { return currentValue; } }
        public float MaximumValue { get { return maximumValue; } }

        public static AttributePair Zero { get { return new AttributePair(); } }

        #endregion

        #region Constructors

        public AttributePair(float maxValue)
        {
            currentValue = maxValue;
            maximumValue = maxValue;
        }
        public AttributePair(float maxValue, float currentValue)
        {
            this.currentValue = currentValue;
            maximumValue = maxValue;
        }

        #endregion

        #region Methods

        public void ReplenishAll()
        {
            currentValue = maximumValue;
        }

        public void Replenish(float value)
        {
            if (value < 0) return;
            currentValue += value;
            if (currentValue > maximumValue)
                currentValue = maximumValue;
        }

        public void Deplete(float value)
        {
            if (value < 0) return;
            currentValue -= value;
            if (currentValue < 0)
                currentValue = 0;
        }

        public void SetCurrent(float value)
        {
            if (value < 0) value = 0f;
            currentValue = value;
            if (currentValue > maximumValue)
                currentValue = maximumValue;
        }

        public void SetMaximum(float value)
        {
            if (value < 0) value = 0f;
            maximumValue = value;
            if (currentValue > maximumValue)
                currentValue = maximumValue;
        }

        #endregion
    }
}
