using System;

namespace Ousiron.Console
{
    public interface ICommand
    {
        public string Id { get; }
        public string Description { get; }
        public string Format { get; }
        public Action<object[]> Action { get; }
        public int ParamQuantity { get; }

        public void Execute(params object[] param);
    }
}