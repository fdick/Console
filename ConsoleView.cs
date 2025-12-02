using System;
using System.Collections.Generic;
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

    public class ConsoleView : MonoBehaviour
    {
        public bool IsEnabled { get; private set; } = false;

        [SerializeField] private int _consoleCapacity = 150;
        [SerializeField] private int _maxPredictionListCount = 10;
        public readonly string UNDEFINED_ARG = "Undefined argument.";
        private string ARROW_DOWN;
        private string ARROW_UP;


        private float _consoleHeight;
        private int _consoleHeightPercent = 40;
        private float _textFieldHeight;
        private float _predictionWindowRecordHeight;
        private float _predictionWindowRecordWidth;
        private float _leftRightBorderWidth;
        private GUIStyle _fontStyle;
        private Vector2 _scroll;
        private Queue<LogView> _logs;
        private string _lastInput;
        private bool _needUpdateTextFieldCursorPosition = false;
        private bool _needUpdateScrollPosition = false;
        private bool _needSetFocusOnInput = true;
        private Texture2D _blackTexture;
        private Texture2D _redTexture;
        private int _fontSize = 100;
        private int _fontSizeDelta = 10;
        private int _fontSizeIterator = 10;
        private int[] _fontSizeIteratorBoards = new int[2] { 8, 12 };


        private bool _isExpanded = false;

        public string Input { get; private set; }
        public string InputPrediction { get; set; }
        public List<string> PredictionList { get; set; } = new List<string>();
        public int SelectedPredictedCommandID { get; set; } = -1;
        public Action<string> OnChangedInput { get; set; }

        private void Awake()
        {
            _logs = new Queue<LogView>(_consoleCapacity);
            _lastInput = Input;


            _blackTexture = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
            _blackTexture.SetPixel(0, 0, new Color(0, 0, 0, 0.8f));
            _blackTexture.Apply(); // not sure if this is necessary
            _redTexture = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
            _redTexture.SetPixel(0, 0, new Color(1, 0, 0, 1f));
            _redTexture.Apply(); // not sure if this is necessary

            ARROW_DOWN = '\u25bc'.ToString();
            ARROW_UP = '\u25b2'.ToString();
            
        }

        private void OnGUI()
        {
            if (!IsEnabled)
                return;
                
            GUI.skin.settings.selectionColor = Color.red;

            var h = Screen.height;
            var w = Screen.width;
            _consoleHeight = GetPercentFromValue(h, _consoleHeightPercent);
            _textFieldHeight = GetPercentFromValue(h, 3);
            _predictionWindowRecordHeight = GetPercentFromValue(h, 2);
            _leftRightBorderWidth = GetPercentFromValue(w, 1);
            _predictionWindowRecordWidth = GetPercentFromValue(w, 25);


            var boxStyle = new GUIStyle(GUIStyle.none);
            boxStyle.normal.background = _blackTexture;

            GUI.Box(new Rect(0, 0, w, _consoleHeight), "", boxStyle);

            //for text field
            _fontStyle = new GUIStyle(GUIStyle.none);
            _fontStyle.fontSize = w / _fontSize;
            _fontStyle.normal.textColor = Color.white;
            _fontStyle.normal.background = _blackTexture;

            GUI.backgroundColor = new Color(1, 0, 0, 1f);

            InputArea();

            //when closing the console was typed ` symbol
            if (!string.IsNullOrEmpty(Input) && Input.Contains('`'))
                Input = Input.Remove(Input.IndexOf('`'));

            InputPredictionCommand();

            if (_lastInput != Input)
                OnChangedInput?.Invoke(Input);

            ShowConsoleBody();
            PredictionWindow();

            if (_needSetFocusOnInput)
            {
                SetFocusOnInput();
                _needSetFocusOnInput = false;
            }

            if (_needUpdateTextFieldCursorPosition)
            {
                UpdateTextFieldCursorPosition();
                _needUpdateTextFieldCursorPosition = false;
            }

            if (_needUpdateScrollPosition)
            {
                MoveScrollPositionToEnd();
                _needUpdateScrollPosition = false;
            }

            _lastInput = Input;
        }

        private void InputArea()
        {
            var addableOffset = 10;
            var inputFieldRect = new Rect(
                _leftRightBorderWidth,
                _consoleHeight - _textFieldHeight,
                Screen.width - _leftRightBorderWidth * 2 - _textFieldHeight * 5,
                _textFieldHeight);

            var btnRect = new Rect(
                inputFieldRect.width + _textFieldHeight,
                inputFieldRect.y,
                _textFieldHeight,
                _textFieldHeight);

            var btnRect2 = new Rect(
                btnRect.x + _textFieldHeight,
                inputFieldRect.y,
                _textFieldHeight,
                _textFieldHeight);

            var btnRect3 = new Rect(
                btnRect2.x + _textFieldHeight,
                inputFieldRect.y,
                _textFieldHeight * 2,
                _textFieldHeight);


            GUI.SetNextControlName("input");
            Input = GUI.TextField(inputFieldRect, Input, _fontStyle);

            //limit string length
            int maxLength = 100;
            if (Input?.Length > maxLength)
            {
                Input = Input.Remove(maxLength - 1, Input.Length - maxLength);
            }


            GUILayout.BeginHorizontal();
            if (GUI.Button(btnRect, "-"))
            {
                ChangeFontSize(true);
            }


            if (GUI.Button(btnRect2, "+"))
            {
                ChangeFontSize(false);
            }

            if (!_isExpanded)
            {
                if (GUI.Button(btnRect3, ARROW_DOWN))
                {
                    _consoleHeightPercent = 70;
                    _isExpanded = true;
                }
            }
            else
            {
                if (GUI.Button(btnRect3, ARROW_UP))
                {
                    _consoleHeightPercent = 40;
                    _isExpanded = false;
                }
            }

            GUILayout.EndHorizontal();
        }

        private void ChangeFontSize(bool increase)
        {
            if (increase)
            {
                if (_fontSizeIterator == _fontSizeIteratorBoards[1])
                    return;

                _fontSizeIterator++;
                _fontSize = _fontSizeIterator * _fontSizeDelta;
            }
            else
            {
                if (_fontSizeIterator == _fontSizeIteratorBoards[0])
                    return;
                _fontSizeIterator--;
                _fontSize = _fontSizeIterator * _fontSizeDelta;
            }
        }

        private void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }


        public void SetInput(string text)
        {
            Input = text;
            _needUpdateTextFieldCursorPosition = true;
            _needSetFocusOnInput = true;
        }

        public void SetFocusOnInput()
        {
            GUI.FocusControl("input");
            var editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            editor.OnFocus();
            editor.MoveTextEnd();
            editor.selectIndex =
                0; //cursor selected end position… it will selecting the text from 0 to 56 (cursorindex)
        }

        public void SetPredictionList(List<string> list)
        {
            if (list.Count > _maxPredictionListCount)
            {
                var count = list.Count - _maxPredictionListCount;
                list.RemoveRange(_maxPredictionListCount, count);
            }

            PredictionList = list;
        }

        public void SwitchConsole(bool on)
        {
            IsEnabled = on;
            if (!on)
                _needSetFocusOnInput = true;
        }

        public void ClearConsole() => _logs.Clear();

        public void ClearInput()
        {
            Input = String.Empty;
            _needUpdateScrollPosition = true;
        }

        public void EnterLog(string log = null)
        {
            SetLogToConsole(log, Color.white);
        }

        public void EnterWarning(string warning)
        {
            SetLogToConsole(warning, Color.yellow);
        }

        public void EnterError(string error)
        {
            SetLogToConsole(error, Color.red);
        }

        private void UpdateTextFieldCursorPosition()
        {
            TextEditor te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            if (te != null)
            {
                te.MoveTextEnd();
            }
        }

        private void MoveScrollPositionToEnd()
        {
            _scroll.y += _textFieldHeight * _consoleCapacity;
        }

        private void ShowConsoleBody()
        {
            // var viewport = new Rect(_leftRightBorderWidth, 0, Screen.width - _leftRightBorderWidth * 2 - 20,
            //     _predictionWindowRecordHeight * _logs.Count - _textFieldHeight - 10);

            GUI.SetNextControlName("scroll");
            _scroll = GUILayout.BeginScrollView(
                _scroll,
                GUILayout.Width(Screen.width - _leftRightBorderWidth * 2),
                GUILayout.Height(_consoleHeight - _textFieldHeight - 10));


            Queue<LogView> copy = new Queue<LogView>(_logs);
            int i = 0;
            string str = String.Empty;

            var style = new GUIStyle(_fontStyle)
            {
                normal =
                {
                    background = null
                },
                richText = true,
                wordWrap = true
            };


            while (copy.Count != 0)
            {
                var l = copy.Dequeue();
                str += GetColoredText(l) + "\n";
                i++;
            }

            GUILayout.TextArea(str, style);
            GUILayout.EndScrollView();
        }

        private string GetColoredText(LogView log)
        {
            string clr = ColorUtility.ToHtmlStringRGBA(log.Color);
            var str = @$"<color=#{clr}> {log.LogText}</color>";

            return str;
        }

        private void InputPredictionCommand()
        {
            var inputFieldRect = new Rect(_leftRightBorderWidth, _consoleHeight - _textFieldHeight,
                Screen.width - _leftRightBorderWidth * 2, _textFieldHeight);

            var style = new GUIStyle(_fontStyle);
            style.normal.textColor = new Color(1f, 1f, 1f, 0.55f);
            style.normal.background = null;
            GUI.Label(inputFieldRect, InputPrediction, style);
        }

        private void HandleLog(string logString, string stacktrace, LogType type)
        {
            switch (type)
            {
                case LogType.Error:

                    EnterError(logString);
                    break;
                case LogType.Warning:
                    // EnterWarning(logString);
                    break;
                case LogType.Log:
                    EnterLog(logString);
                    break;
            }
        }

        private float GetPercentFromValue(float value, int percent)
        {
            percent = Math.Clamp(percent, 0, 100);
            if (percent == 100)
                return value;
            else if (percent == 0)
                return 0;

            return (value * percent) / 100;
        }

        private void SetLogToConsole(string text, Color color)
        {
            if (text == null)
                text = string.Empty;
            var date = $"[{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}]";
            var t = $"{date} {text}";
            if (_logs.Count == _consoleCapacity)
                _logs.Dequeue();
            _logs.Enqueue(new LogView(t, color));
        }

        private void PredictionWindow()
        {
            if (PredictionList.Count == 0)
                return;
            var predictionList = PredictionList.Count;
            if (predictionList > _maxPredictionListCount)
                predictionList = _maxPredictionListCount;

            var viewport = new Rect(_leftRightBorderWidth, _consoleHeight, _predictionWindowRecordWidth,
                predictionList * _predictionWindowRecordHeight);
            GUI.Box(viewport, "");

            for (var i = 0; i < predictionList; i++)
            {
                var style = new GUIStyle(_fontStyle);
                style.normal.textColor = new Color(1f, 1f, 1f, 0.7f);
                style.normal.background = null;
                var selectedStyle = new GUIStyle(_fontStyle);
                selectedStyle.normal.textColor = new Color(1f, 1f, 1f, 1f);

                GUI.Label(
                    new Rect(viewport.x, (viewport.y + _predictionWindowRecordHeight * i), viewport.width,
                        _predictionWindowRecordHeight),
                    PredictionList[i],
                    i == SelectedPredictedCommandID ? selectedStyle : style);
            }
        }
    }
}