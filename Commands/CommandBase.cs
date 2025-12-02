namespace Ousiron.Console
{
    public abstract class CommandBase
    {
        public string Id { get; }
        public string Description { get; }
        public string Format { get; }

        public CommandBase(string id, string description, string format)
        {
            Id = id;
            Format = format;
            Description = description;
        }
    }
}