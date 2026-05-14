using System;
using System.Collections.Generic;

namespace Source.Shared.Reactive.Events
{
    public readonly struct ValueChangedEvent<T> : IEquatable<ValueChangedEvent<T>>
    {
        public T OldValue { get; }
        public T NewValue { get; }

        public ValueChangedEvent(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public bool Equals(ValueChangedEvent<T> other)
        {
            return EqualityComparer<T>.Default.Equals(OldValue, other.OldValue) &&
                   EqualityComparer<T>.Default.Equals(NewValue, other.NewValue);
        }

        public override bool Equals(object obj)
        {
            return obj is ValueChangedEvent<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(OldValue, NewValue);
        }

        public static bool operator ==(ValueChangedEvent<T> left, ValueChangedEvent<T> right) => left.Equals(right);
        
        public static bool operator !=(ValueChangedEvent<T> left, ValueChangedEvent<T> right) => !left.Equals(right);
    }
}