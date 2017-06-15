namespace GoogleDataCollection.Logging
{
    public abstract class LogCategory
    {
        public abstract string Name { get; protected set; }

        public override string ToString()
        {
            return $"{ Name }";
        }
    }
}
