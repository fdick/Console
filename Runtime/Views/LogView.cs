using UnityEngine;

namespace Ousiron.Console
{
    public class LogView
    {
        public string LogText { get; }
        public Color Color { get; }

        public LogView(string logText, Color color)
        {
            this.LogText = logText;
            this.Color = color;
        }
    }
}