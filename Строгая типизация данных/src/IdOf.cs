namespace Article
{
    public readonly struct IdOf<T>
    {
        public long Id { get; }

        public IdOf(long id) => Id = id;
    }
}
