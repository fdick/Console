using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Ousiron.Console
{
    public class Console : IDisposable
    {
        private List<ICommand> _commands;
        private IConsoleView _view;
        private ICommand _predictionCommand;
        private int _lastCommandsCashCapacity;
        private List<string> _lastCommandsCash;
        private int _lastCommandCashIterator = 0;
        private int _predictedListIterator = -1;


        public Console(List<ICommand> commands, IConsoleView view, int lastCommandsCashCapacity = 10)
        {
            _commands = DefaultCommandsList();
            _commands.AddRange(commands);
            _view = view;
            _lastCommandsCashCapacity = lastCommandsCashCapacity;
            view.OnChangedInput += OnChangedInput;
            _lastCommandsCash = new List<string>();
        }

        public void OnEnterPressed()
        {
            if (!_view.IsEnabled)
                return;

            _lastCommandCashIterator = 0;

            void EnterCommand()
            {
                if (TryHandleInput(_view.Input))
                    AddCommandNameToCash(_view.Input);
                else
                    _view.EnterWarning($"Command is not recognized <{_view.Input}>");

                _view.ClearInput();
            }

            if (_predictedListIterator < 0)
            {
                EnterCommand();
            }
            else
            {
                if (_view.Input == _view.GetPrediction(_predictedListIterator))
                {
                    EnterCommand();
                }
                else
                {
                    var inp = _view.GetPrediction(_predictedListIterator);
                    inp = inp.Split(' ')[0];
                    _view.SetInput(inp);
                }
            }
        }

        public void OnArrowUpPressed()
        {
            if (!_view.IsEnabled)
                return;
            if (_lastCommandsCash.Count == 0)
                return;

            if (_lastCommandCashIterator >= _lastCommandsCash.Count)
                _lastCommandCashIterator = 0;

            _view.SetInput(_lastCommandsCash[_lastCommandCashIterator]);

            _lastCommandCashIterator++;
            if (_lastCommandCashIterator >= _lastCommandsCashCapacity ||
                _lastCommandCashIterator >= _lastCommandsCash.Count)
                _lastCommandCashIterator = 0;
        }

        public void OnArrowDownPressed()
        {
            if (!_view.IsEnabled)
                return;
            var predictListCount = _view.GetPredictionsCount();
            if (predictListCount == 0)
                return;

            if (_predictedListIterator >= predictListCount)
                _predictedListIterator = 0;

            _predictedListIterator++;
            if (_predictedListIterator >= predictListCount)
                _predictedListIterator = 0;

            _view.SetSelectedPredictedCommandID(_predictedListIterator);
        }

        public void OnTabPressed()
        {
            if (!_view.IsEnabled)
                return;
            if (string.IsNullOrEmpty(_view.InputPrediction))
                return;

            if (_predictionCommand == null)
                return;
            _view.SetInput(_predictionCommand.Id);
            _view.SetFocusOnInput();
        }

        private void OnChangedInput(string input)
        {
            _predictedListIterator = -1;
            _view.SetSelectedPredictedCommandID(_predictedListIterator);

            if (string.IsNullOrEmpty(input))
            {
                _view.InputPrediction = string.Empty;
                _view.ClearPredictionList();
                _predictionCommand = null;
                return;
            }

            var executiveMatches = GetCommands(input);
            if (executiveMatches == null || executiveMatches.Length == 0)
            {
                _view.InputPrediction = string.Empty;
                _view.ClearPredictionList();
                _predictionCommand = null;
                return;
            }

            _predictionCommand = executiveMatches[0];
            //show input prediction
            _view.InputPrediction = executiveMatches[0].Format;
            //show prediction list
            var predList = executiveMatches.Select(x => x.Format).ToList();

            _view.SetPredictionList(predList);
        }

        private void AddCommandNameToCash(string commandName)
        {
            //drop command for cashing if last cashed command is same
            if (_lastCommandsCash.Count != 0 && _lastCommandsCash[0] == commandName)
                return;
            if (_lastCommandsCash.Count == _lastCommandsCashCapacity)
                _lastCommandsCash.RemoveAt(_lastCommandsCash.Count - 1);

            _lastCommandsCash.Insert(0, commandName);
        }

        private bool TryHandleInput(string inputText)
        {
            if (string.IsNullOrEmpty(inputText))
                return false;
            if (inputText.Contains('?'))
            {
                var ind = inputText.IndexOf('?');
                var inp = inputText.Remove(ind, inputText.Length - ind);
                var com = GetCommands(inp);
                if (com.Length == 0)
                    return false;

                _view.EnterLog($"{com[0].Format} - {com[0].Description}");
                return true;
            }


            var props = inputText.Split(' ');
            if (props.Length == 0)
                return false;

            foreach (ICommand c in _commands)
            {
                if (c.Id != props[0])
                    continue;

                if (c.ParamQuantity != props.Length - 1)
                    continue;

                object[] param = new object[props.Length - 1];
                for (int i = 1; i < props.Length; i++)
                {
                    param[i - 1] = props[i];
                }

                c.Execute(param);
                return true;
            }

            return false;
        }

        private ICommand[] GetCommands(string input)
        {
            try
            {
                Regex executiveRegex = new Regex(@$"^{input}\w*");
                return _commands.Where(x => executiveRegex.IsMatch(x.Id)).ToArray();
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private List<ICommand> DefaultCommandsList()
        {
            return new List<ICommand>()
            {
                new Command("clear", "Clear the console", "clear", 0, (param) => { _view.ClearConsole(); }),
                new Command("help", "Show a help window", "help", 0, (param) =>
                {
                    _view.EnterLog();
                    _view.EnterLog("1. To see command description, type {command name}?. For example: clear?");
                    _view.EnterLog("2. To finish an existing command press [Tab] key when typing any command.");
                    _view.EnterLog("3. To return a last typed correct command, press [Up Arrow] key.");
                    _view.EnterLog("4. To select a predicted command in the below window, press [Down Arrow] key.");
                    _view.EnterLog();
                    _view.EnterLog("Default commands:");
                    _view.EnterLog($"- clear");
                    _view.EnterLog($"- picture");
                    _view.EnterLog($"- help");
                }),
                new Command("picture", "Show an image", "picture", 1, param =>
                {
                    if (!int.TryParse(param[0] as string, out int id))
                        return;


                    var file = Resources.Load<TextAsset>("pictures");
                    var all = file.text;
                    var splited = all.Split(@"\b");

                    if (id >= splited.Length || id < 0)
                        return;

                    var pic = splited[id];
                    var pixels = pic.Split('\n');

                    foreach (var line in pixels)
                    {
                        _view.EnterLog(line);
                    }

                    _view.EnterLog();
                }),
            };
        }

        public void Dispose()
        {
            _commands.Clear();
            _commands = null;
            
            _view.OnChangedInput -= OnChangedInput;
            
            _lastCommandsCash.Clear();
            _lastCommandsCash = null;
        }
    }
}