using System;

namespace Ousiron.Console
{
    public class Command : ICommand
    {
        public string Id { get; }
        public string Description { get; }
        public string Format { get; }
        public Action<object[]> Action { get; }
        public int ParamQuantity { get; }

        public Command(string id, string description, string format, int paramQuantity, Action<object[]> action)
        {
            Id = id;
            Format = format;
            Description = description;
            Action = action;
            ParamQuantity = paramQuantity;
        }

        public void Execute(params object[] param)
        {
            Action?.Invoke(param);
        }
    }
}