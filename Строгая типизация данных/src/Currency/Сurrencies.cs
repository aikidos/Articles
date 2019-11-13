namespace Article.Currency
{
    public readonly struct Rub : ICurrency
    {
        public string Code => "RUB";

        public int Number => 643;
    }

    public readonly struct Euro : ICurrency
    {
        public string Code => "EUR";

        public int Number => 978;
    }
}