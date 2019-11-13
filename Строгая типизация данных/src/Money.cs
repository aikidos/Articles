using System;
using Article.Currency;

namespace Article
{
    public readonly struct Money<TCurrency> : IComparable<Money<TCurrency>>
        where TCurrency : struct, ICurrency
    {
        public decimal Amount { get; }

        public Money(decimal amount) => Amount = amount;

        public (string Code, int Number) GetCurrencyInfo()
        {
            var currency = default(TCurrency);

            return (currency.Code, currency.Number);
        }

        public int CompareTo(Money<TCurrency> other) => Amount.CompareTo(other.Amount);

        public bool Equals(Money<TCurrency> other) => Amount == other.Amount;

        public override bool Equals(object? obj) => obj is Money<TCurrency> other && Equals(other);

        public override int GetHashCode() => Amount.GetHashCode();

        public static Money<TCurrency> operator +(Money<TCurrency> a, Money<TCurrency> b) => new Money<TCurrency>(a.Amount + b.Amount);
        public static Money<TCurrency> operator -(Money<TCurrency> a, Money<TCurrency> b) => new Money<TCurrency>(a.Amount - b.Amount);
        public static bool operator >(Money<TCurrency> a, Money<TCurrency> b) => a.Amount > b.Amount;
        public static bool operator <(Money<TCurrency> a, Money<TCurrency> b) => a.Amount < b.Amount;
        public static bool operator ==(Money<TCurrency> a, Money<TCurrency> b) => a.Amount == b.Amount;
        public static bool operator !=(Money<TCurrency> a, Money<TCurrency> b) => a.Amount != b.Amount;
        public static bool operator >=(Money<TCurrency> a, Money<TCurrency> b) => a.Amount >= b.Amount;
        public static bool operator <=(Money<TCurrency> a, Money<TCurrency> b) => a.Amount <= b.Amount;
    }
}