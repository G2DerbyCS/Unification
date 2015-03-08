using System;

namespace Unification.Models
{
    /// <summary>
    /// An EventArgs class for indicating value change.
    /// </summary>
    /// <typeparam name="T">Type specifier.</typeparam>
    public class ValueChangingEventArgs<T> : EventArgs
    {
        public readonly T FutureValue;
        public readonly T CurrentValue;

        public ValueChangingEventArgs(T CurrentValue, T FutureValue)
        {
            this.CurrentValue = CurrentValue;
            this.FutureValue  = FutureValue;
        }
    }
}
