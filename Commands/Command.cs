using System;

namespace Ousiron.Console
{
    public class Command : CommandBase
    {
        private Action _commandAction;

        public Command(string id, string description, string format, Action commandAction) : base(id, description,
            format)
        {
            _commandAction = commandAction;
        }

        public void Invoke() => _commandAction?.Invoke();
    }

    public class Command<T> : CommandBase
    {
        private Action<T> _commandAction;

        public Command(string keyFormat, string description, string format,
            Action<T> commandAction) :
            base(keyFormat, description, format)
        {
            _commandAction = commandAction;
        }

        public void Invoke(T arg) => _commandAction?.Invoke(arg);
    }

    public class Command<T1, T2> : CommandBase
    {
        private Action<T1, T2> _commandAction;

        public Command(string keyFormat, string description, string format, Action<T1, T2> commandAction) : base(
            keyFormat, description, format)
        {
            _commandAction = commandAction;
        }

        public void Invoke(T1 arg1, T2 arg2) => _commandAction?.Invoke(arg1, arg2);
    }
}