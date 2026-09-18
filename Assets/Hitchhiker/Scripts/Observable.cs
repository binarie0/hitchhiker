using System;
using System.Collections.Generic;
using System.Text;


    internal class Observer<T> where T: IEquatable<T>
    {
        public delegate void OnValueChangedDelegate(T value);

        /// <summary>
        /// The value that exists inside this observer. If this value is changed, then <see cref="ValueChanged"/> is emitted.
        /// </summary>
        internal T Value
        { 
            get
            {
                return value;
            }
            set
            {
                if (!value.Equals(this.value))
                {
                    this.value = value;
                    EmitChanged();
                }
            }
        }
        private T value;

        internal Observer(T initValue = default(T), params OnValueChangedDelegate[] initDelegates)
        {
            value = initValue;
        if (initDelegates != null)
        {
            foreach (var initDelegate in initDelegates) {
                ValueChanged += initDelegate;
            }
        }
            EmitChanged();
        }
            

        /// <summary>
        /// Emitted when the value of an object changes.
        /// </summary>
        internal event OnValueChangedDelegate ValueChanged;

        /// <summary>
        /// Calls a manual update on <see cref="ValueChanged"/>. Helpful for when initializing dependencies.
        /// </summary>
        internal void EmitChanged()
        {
            ValueChanged?.Invoke(this.value);
        }

    /// <summary>
    /// Converts a T value into an Observer instance. Should only be used at initialization time.
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Observer<T>(T value)
    {
        return new Observer<T>(value);
    }

    public static implicit operator T(Observer<T> value)
    {
        return value.value;
    }
}


