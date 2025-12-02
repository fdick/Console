using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Ousiron.Console
{
    public class Console
    {
        private List<CommandBase> _commands;
        private ConsoleView _view;
        private CommandBase _predictionCommand;
        private int _lastCommandsCashCapacity;
        private List<string> _lastCommandsCash;
        private int _lastCommandCashIterator = 0;
        private int _predictedListIterator = -1;
        private const string CLEAR_ID = "clear";


        public Console(List<CommandBase> commands, ConsoleView view, int lastCommandsCashCapacity = 10)
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
                if (_view.Input == _view.PredictionList[_predictedListIterator])
                    EnterCommand();
                else
                {
                    var inp = _view.PredictionList[_predictedListIterator];
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
            var predictListCount = _view.PredictionList.Count;
            if (predictListCount == 0)
                return;

            if (_predictedListIterator >= predictListCount)
                _predictedListIterator = 0;

            _predictedListIterator++;
            if (_predictedListIterator >= predictListCount)
                _predictedListIterator = 0;

            _view.SelectedPredictedCommandID = _predictedListIterator;
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
            _view.SelectedPredictedCommandID = _predictedListIterator;

            if (string.IsNullOrEmpty(input))
            {
                _view.InputPrediction = string.Empty;
                _view.PredictionList.Clear();
                _predictionCommand = null;
                return;
            }

            var executiveMatches = GetCommands(input);
            if (executiveMatches == null || executiveMatches.Length == 0)
            {
                _view.InputPrediction = string.Empty;
                _view.PredictionList.Clear();
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

            foreach (CommandBase c in _commands)
            {
                if (c.Id != props[0])
                    continue;

                //command without argument
                if (c is Command dc)
                {
                    if (_view.Input != CLEAR_ID)
                        _view.EnterLog(_view.Input);

                    dc.Invoke();
                    return true;
                }

                //command with 1 argument
                else if (c is Command<int> onec)
                {
                    if (props.Length != 2)
                        return false;

                    if (!int.TryParse(props[1], out var arg))
                        return false;

                    if (_view.Input != CLEAR_ID)
                        _view.EnterLog(_view.Input);

                    onec.Invoke(arg);
                    return true;
                }

                //command with 2 arguments
                else if (c is Command<int, int> twoc)
                {
                    if (props.Length != 3)
                        return false;

                    if (!int.TryParse(props[1], out var arg1))
                        return false;
                    if (!int.TryParse(props[2], out var arg2))
                        return false;

                    if (_view.Input != CLEAR_ID)
                        _view.EnterLog(_view.Input);

                    twoc.Invoke(arg1, arg2);
                    return true;
                }
            }

            return false;
        }

        private CommandBase[] GetCommands(string input)
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

        private List<CommandBase> DefaultCommandsList()
        {
            return new List<CommandBase>()
            {
                new Command("clear", "Clear the console", "clear", () => { _view.ClearConsole(); }),
                new Command<int>("picture", "Draw a picture by ID. <id> is integer value.", "picture <id>", (id) =>
                {
                    switch (id)
                    {
                        case 0:
                            _view.EnterLog("8I____________,8'_____88__________`P888\"");
                            _view.EnterLog("8I___________,8I______88____________\"8ba,.");
                            _view.EnterLog("(8,_________,8P'______88______________88\"\"8bma,.");
                            _view.EnterLog("_8I________,8P'_______88,______________\"8b___\"\"P8ma,");
                            _view.EnterLog("_(8,______,8d\"________`88,_______________\"8b_____`\"8a");
                            _view.EnterLog("__8I_____,8dP_________,8X8,________________\"8b.____:8b");
                            _view.EnterLog("__(8____,8dP'__,I____,8XXX8,________________`88,____8)");
                            _view.EnterLog("___8,___8dP'__,I____,8XxxxX8,_____I,_________8X8,__,8");
                            _view.EnterLog("___8I___8P'__,I____,8XxxxxxX8,_____I,________`8X88,I8");
                            _view.EnterLog("___I8,__\"___,I____,8XxxxxxxxX8b,____I,________8XXX88I,");
                            _view.EnterLog("___`8I______I'__,8XxxxxxxxxxxxXX8____I________8XXxxXX8,");
                            _view.EnterLog("____8I_____(8__,8XxxxxxxxxxxxxxxX8___I________8XxxxxxXX8,");
                            _view.EnterLog("___,8I_____I[_,8XxxxxxxxxxxxxxxxxX8__8________8XxxxxxxxX8,");
                            _view.EnterLog("___d8I,____I[_8XxxxxxxxxxxxxxxxxxX8b_8_______(8XxxxxxxxxX8,");
                            _view.EnterLog("___888I____`8,8XxxxxxxxxxxxxxxxxxxX8_8,_____,8XxxxxxxxxxxX8");
                            _view.EnterLog("___8888,____\"88XxxxxxxxxxxxxxxxxxxX8)8I____.8XxxxxxxxxxxxX8");
                            _view.EnterLog("__,8888I_____88XxxxxxxxxxxxxxxxxxxX8_`8,__,8XxxxxxxxxxxxX8\"");
                            _view.EnterLog("__d88888_____`8XXxxxxxxxxxxxxxxxxX8'__`8,,8XxxxxxxxxxxxX8\"");
                            _view.EnterLog("__888888I_____`8XXxxxxxxxxxxxxxxX8'____\"88XxxxxxxxxxxxX8\"");
                            _view.EnterLog("__88888888bbaaaa88XXxxxxxxxxxxXX8)______)8XXxxxxxxXX8\"");
                            _view.EnterLog("__8888888I,_``\"\"\"\"\"\"8888888888888888aaaaa8888XxxxxXX8\"");
                            _view.EnterLog("__(8888888I,______________________.__```\"\"\"\"\"88888P\"");
                            _view.EnterLog("___88888888I,___________________,8I___8,_______I8\"");
                            _view.EnterLog("____\"\"\"88888I,________________,8I'____\"I8,____;8\"");
                            _view.EnterLog("___________`8I,_____________,8I'_______`I8,___8)");
                            _view.EnterLog("____________`8I,___________,8I'__________I8__:8'");
                            _view.EnterLog("_____________`8I,_________,8I'___________I8__:8");
                            _view.EnterLog("______________`8I_______,8I'_____________`8__(8");
                            _view.EnterLog("_______________8I_____,8I'________________8__(8;");
                            break;
                        case 1:
                            _view.EnterLog("_____________________________________¶¶___________");
                            _view.EnterLog("________________________________¶1¶1111111¶_______");
                            _view.EnterLog("________¶¶111¶_______________¶¶¶¶111111111¶¶¶1____");
                            _view.EnterLog("_____¶1¶¶¶¶¶111111¶_________¶¶¶1¶¶¶11111111¶1¶¶___");
                            _view.EnterLog("___¶¶¶1¶1111111111¶¶1______¶¶1¶¶¶1111111111111¶¶__");
                            _view.EnterLog("__¶¶1¶¶1111111111111¶¶_____¶¶¶1¶¶¶¶1111111111111¶_");
                            _view.EnterLog("__¶¶_¶1111111111111111¶¶___¶¶¶¶¶¶11¶111111111111¶_");
                            _view.EnterLog("_11_¶11111111111111111¶¶_____¶¶¶¶__¶111111111111¶¶");
                            _view.EnterLog("¶¶¶¶1111111111111111¶¶¶¶_____1¶¶__11111111111111¶¶");
                            _view.EnterLog("¶¶¶¶11111111111¶¶¶¶¶¶¶______1¶1¶¶1111111111111111¶");
                            _view.EnterLog("¶¶1¶1111111111111¶¶¶¶¶¶_____¶¶¶¶¶¶11111111111111¶¶");
                            _view.EnterLog("¶¶11111111111111111111111¶¶___¶¶¶¶¶¶1111111111¶¶¶_");
                            _view.EnterLog("_1¶111111111111111111¶¶¶¶¶¶____¶¶¶¶11111111111¶1__");
                            _view.EnterLog("__¶¶11111111111111111¶¶¶_____¶¶¶1111111111111¶1___");
                            _view.EnterLog("___¶¶¶111111111111¶1¶¶¶____1¶¶111¶1111111¶11¶1____");
                            _view.EnterLog("____1¶¶¶11111111111¶¶¶¶111¶¶¶¶111111111¶11¶¶¶_____");
                            _view.EnterLog("______¶¶¶¶1111111111111¶¶¶¶1¶¶¶¶¶¶¶¶11¶11¶¶_______");
                            _view.EnterLog("_______¶¶¶¶¶11111111111¶111¶___¶¶¶111¶1¶¶¶________");
                            _view.EnterLog("_________¶¶¶¶¶¶111111111111¶__¶¶¶111¶¶¶1__________");
                            _view.EnterLog("____________1¶¶¶¶¶11111111¶¶_¶¶¶¶111¶¶____________");
                            _view.EnterLog("______________¶¶¶¶¶¶¶1111111_¶¶¶11¶¶1_____________");
                            _view.EnterLog("_________________1¶¶¶¶¶¶1111¶¶¶1¶¶¶¶______________");
                            _view.EnterLog("____________________¶¶¶¶¶¶1¶¶¶¶¶1¶________________");
                            _view.EnterLog("_______________________¶1¶¶¶1¶¶¶__________________");
                            _view.EnterLog("___________________________11¶____________________");
                            break;
                        case 2:
                            _view.EnterLog("______$$_____________________ $$");
                            _view.EnterLog("____$$$__$__________________$__$$$");
                            _view.EnterLog("___$$$___$$________________$$___$$$");
                            _view.EnterLog("___$$$$$$$$________________$$$$$$$$");
                            _view.EnterLog("____$$$$$$__________________$$$$$$");
                            _view.EnterLog("_____$$$$____$$0$$$$$0$$$____$$$$");
                            _view.EnterLog("_______$$__$$$$$$$$$$$$$$$$__$$");
                            _view.EnterLog("___$$___$$$$$$$$$$$$$$$$$$$$$$___$$");
                            _view.EnterLog("_$$__$$__$$$$$$$$$$$$$$$$$$$$__$$__$$");
                            _view.EnterLog("$______$$$$$$$$$$$$$$$$$$$$$$$$______$");
                            _view.EnterLog("$__$$$____$$$$$$$$$$$$$$$$$$____$$$__$");
                            _view.EnterLog("__$___$$$$_$$$$$$$$$$$$$$$$_$$$$___$");
                            _view.EnterLog("_$_________$_$$$$$$$$$$$$_$_________$");
                            _view.EnterLog("_$______$$$________________$$$______$");
                            _view.EnterLog("_______$______________________$");
                            _view.EnterLog("______$________________________$");
                            _view.EnterLog("______$_______________________ _$");
                            break;
                        case 3:
                            _view.EnterLog("___?$$$?________________");
                            _view.EnterLog("__$$$$$$$_####______####_");
                            _view.EnterLog("___*$$$$$$?####___########");
                            _view.EnterLog("_____*$$$$$$$$$$$##########");
                            _view.EnterLog("_____$$$$$$$$$$$$$##########");
                            _view.EnterLog("______$$$$$$$$$$$$$##########");
                            _view.EnterLog("______$$$$$$$$$$_$$$##########");
                            _view.EnterLog("______$$$$$$$$$$##$$$##########");
                            _view.EnterLog("_______$$$$$$$$$_##$$##########");
                            _view.EnterLog("______$$$$$$$$$$___$$#########");
                            _view.EnterLog("_____$_$$$$$$$$$$__$$_########");
                            _view.EnterLog("___$$__$$$$$$$$$$_$$$__######");
                            _view.EnterLog("______$$$$$$$$$$__$$$___#####");
                            _view.EnterLog("______$$$$$$$$$___$$____####");
                            _view.EnterLog("______$$$$$$$$$_________###");
                            _view.EnterLog("______$$$$$$$$__________##");
                            _view.EnterLog("_______$$$$$$___________##");
                            _view.EnterLog("_______$$$$$$______________");
                            _view.EnterLog("_______$$$$$$$$____________");
                            _view.EnterLog("_______$$$$$$$$____________");
                            _view.EnterLog("_______$$$$_$$$$___________");
                            _view.EnterLog("_______$$$$_$$$$___________");
                            _view.EnterLog("_______$$$___$$$$__________");
                            _view.EnterLog("__???$$$$$$_??$$$$__________");
                            break;
                        case 4:
                            _view.EnterLog("_____Sexy?Sex");
                            _view.EnterLog(" ____?Sexy?Sexy");
                            _view.EnterLog(" ___y?Sexy?Sexy?");
                            _view.EnterLog(" ___?Sexy?Sexy?S");
                            _view.EnterLog(" ___?Sexy?Sexy?S");
                            _view.EnterLog(" __?Sexy?Sexy?Se");
                            _view.EnterLog(" _?Sexy?Sexy?Se");
                            _view.EnterLog(" _?Sexy?Sexy?Se");
                            _view.EnterLog(" _?Sexy?Sexy?Sexy?");
                            _view.EnterLog(" ?Sexy?Sexy?Sexy?Sexy");
                            _view.EnterLog(" ?Sexy?Sexy?Sexy?Sexy?Se");
                            _view.EnterLog(" ?Sexy?Sexy?Sexy?Sexy?Sex");
                            _view.EnterLog(" _?Sexy?__?Sexy?Sexy?Sex");
                            _view.EnterLog(" ___?Sex____?Sexy?Sexy?");
                            _view.EnterLog(" ___?Sex_____?Sexy?Sexy");
                            _view.EnterLog(" ___?Sex_____?Sexy?Sexy");
                            _view.EnterLog(" ____?Sex____?Sexy?Sexy");
                            _view.EnterLog(" _____?Se____?Sexy?Sex");
                            _view.EnterLog(" ______?Se__?Sexy?Sexy");
                            _view.EnterLog(" _______?Sexy?Sexy?Sex");
                            _view.EnterLog(" ________?Sexy?Sexy?sex");
                            _view.EnterLog(" _______?Sexy?Sexy?Sexy?Se");
                            _view.EnterLog(" _______?Sexy?Sexy?Sexy?Sexy?");
                            _view.EnterLog(" _______?Sexy?Sexy?Sexy?Sexy?Sexy");
                            _view.EnterLog(" _______?Sexy?Sexy?Sexy?Sexy?Sexy?S");
                            _view.EnterLog(" ________?Sexy?Sexy____?Sexy?Sexy?se");
                            _view.EnterLog(" _________?Sexy?Se_______?Sexy?Sexy?");
                            _view.EnterLog(" _________?Sexy?Se_____?Sexy?Sexy?");
                            _view.EnterLog(" _________?Sexy?S____?Sexy?Sexy");
                            _view.EnterLog(" _________?Sexy?S_?Sexy?Sexy");
                            _view.EnterLog(" ________?Sexy?Sexy?Sexy");
                            _view.EnterLog(" ________?Sexy?Sexy?S");
                            _view.EnterLog(" ________?Sexy?Sexy");
                            _view.EnterLog(" _______?Sexy?Se");
                            _view.EnterLog(" _______?Sexy?");
                            _view.EnterLog(" ______?Sexy?");
                            _view.EnterLog(" ______?Sexy?");
                            _view.EnterLog(" ______?Sexy?");
                            _view.EnterLog(" ______?Sexy");
                            _view.EnterLog(" ______?Sexy");
                            _view.EnterLog(" _______?Sex");
                            _view.EnterLog(" _______?Sex");
                            _view.EnterLog(" _______?Sex");
                            _view.EnterLog(" ______?Sexy");
                            _view.EnterLog(" ______?Sexy");
                            _view.EnterLog(" _______Sexy");
                            _view.EnterLog(" _______ Sexy?");
                            _view.EnterLog(" ________SexY");
                            break;
                        case 5:
                            _view.EnterLog("211_1_1____2¶¶¶¶66¶¶¶¶¶¶¶88¶88__¶888¶¶2________126");
                            _view.EnterLog("__________¶¶¶¶¶12¶¶¶¶¶8¶¶¶866¶612¶8¶¶¶6_1_________");
                            _view.EnterLog("211_1___6¶¶¶¶¶¶¶¶¶¶¶¶¶¶866¶¶66¶822¶¶8886681_______");
                            _view.EnterLog("1111____2¶¶¶¶¶¶¶¶¶¶¶¶¶¶8688¶¶8_8¶2168¶61126_______");
                            _view.EnterLog("21___268¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶8¶¶¶¶¶¶68¶6_188__666______");
                            _view.EnterLog("11__12¶¶¶¶¶¶228¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶68¶2_26¶2_2861____");
                            _view.EnterLog("1__16¶¶¶¶¶6___¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶61¶¶_¶¶6¶____122__");
                            _view.EnterLog("1__8¶¶¶¶¶6____6¶¶¶¶8¶¶¶¶¶¶¶¶¶¶¶¶218¶18¶61¶1______1");
                            _view.EnterLog("2186¶¶¶¶2_1____28682¶8¶8¶¶¶¶¶¶¶¶¶_68266¶1_2¶1_____");
                            _view.EnterLog("6862¶¶¶¶__1_____6___26_16¶8¶¶¶8__¶211612¶8_¶¶81886");
                            _view.EnterLog("¶88¶¶¶¶81_1____12________________¶¶1_11_6¶_2¶¶2___");
                            _view.EnterLog("6¶¶¶¶¶¶6__2____1________________2_88_2_1_¶1¶8661__");
                            _view.EnterLog("1_¶¶¶¶¶2__8______________________1¶22____¶688¶¶12_");
                            _view.EnterLog("_2¶¶¶¶¶8__21_1_11_____________1__¶__11_1_¶81¶¶¶1__");
                            _view.EnterLog("_2¶¶¶¶¶¶1_2621_1________________18___26__2¶2_¶¶___");
                            _view.EnterLog("28_¶¶¶¶¶1__262_____1_______1_____2___1611_8¶126___");
                            _view.EnterLog("8__¶¶¶¶¶2_282_______________1____2___62_2__2¶¶6___");
                            _view.EnterLog("__2¶8¶8¶6_6¶¶¶8182______________622__16__12__82___");
                            _view.EnterLog("__¶2_2¶¶¶_6¶¶¶¶¶¶¶¶12__61_2¶¶¶¶¶¶¶¶66_6___¶¶21112_");
                            _view.EnterLog("1_81__8¶¶__6¶¶268¶¶¶¶61¶¶¶¶¶¶¶2_18¶¶1__1_2¶¶¶2_1_1");
                            _view.EnterLog("8_22__1¶¶1__6¶¶¶¶¶¶¶¶_____¶¶¶¶¶¶¶_1¶____1¶¶¶¶8____");
                            _view.EnterLog("1_22__1¶¶8____6¶¶¶2__________81___6______¶¶¶¶¶2___");
                            _view.EnterLog("_______¶¶¶2_______________________2______626¶¶6___");
                            _view.EnterLog("_______8¶¶6_______16__________________1__¶¶¶¶8¶81_");
                            _view.EnterLog("_____686¶¶¶¶2___12______________________¶¶¶¶8¶¶¶86");
                            _view.EnterLog("___2¶¶¶¶8¶¶¶6_____26_______________16___¶¶¶¶¶¶¶¶¶6");
                            _view.EnterLog("86¶¶¶¶¶¶¶¶¶¶¶¶221__2¶881__________22_____¶¶¶¶¶128¶");
                            _view.EnterLog("¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶882_16_____¶¶¶21____1_1¶¶¶¶¶228¶");
                            _view.EnterLog("¶¶¶¶¶¶¶¶8¶¶¶¶8¶¶¶8¶¶¶¶¶¶¶¶¶62¶¶¶8___2___¶¶¶¶¶¶86¶¶");
                            _view.EnterLog("¶¶¶¶¶¶¶¶¶¶¶¶¶8162__6¶¶¶¶¶¶66_______6____¶¶¶¶¶¶66¶¶");
                            _view.EnterLog("¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶66___88¶88________¶¶____¶¶¶¶¶¶862¶¶");
                            _view.EnterLog("¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶¶6¶1____________¶¶¶1___¶¶¶¶¶¶¶¶626¶");
                            _view.EnterLog("¶¶¶¶¶¶¶8¶¶¶¶¶2¶8¶¶¶2_________2¶81___2¶¶¶¶¶¶¶¶86618");
                            _view.EnterLog("¶¶¶¶¶¶8¶¶¶¶¶¶2¶¶¶¶8¶¶¶8¶622¶¶¶_____6¶¶¶¶¶8¶¶¶8288¶");
                            _view.EnterLog("¶¶¶¶¶¶¶¶¶¶¶8¶¶8888¶628¶¶¶¶¶2¶¶6____1¶¶¶¶¶8¶¶¶8¶¶¶¶");
                            _view.EnterLog("¶¶¶¶¶¶¶¶6¶¶8¶¶2666866¶¶¶¶¶¶_¶¶¶____6¶¶¶¶¶8¶¶68¶¶8¶");
                            _view.EnterLog("¶¶¶¶¶¶¶¶68¶6¶¶668628¶¶¶¶¶¶82¶¶¶¶__1¶¶¶¶¶62¶¶68¶¶88");
                            _view.EnterLog("¶¶¶¶¶¶¶¶6¶¶¶¶¶¶6666¶¶¶8¶2118¶¶¶¶6_8¶¶¶¶¶128¶6¶¶¶¶¶");
                            _view.EnterLog("¶¶¶¶¶¶¶¶¶¶¶88¶¶862¶¶¶8¶¶2826¶¶¶¶¶¶¶¶66¶¶16¶¶¶¶¶¶¶8");
                            _view.EnterLog("¶¶¶¶¶¶¶¶8¶¶¶2¶¶¶8¶¶¶28¶¶¶¶_2¶¶¶8¶¶¶22¶¶618¶¶¶¶¶¶¶¶");
                            _view.EnterLog("¶¶¶¶¶¶¶¶82¶¶28¶¶¶¶8_28¶¶¶2__¶881_¶618¶¶12¶¶¶¶¶¶¶¶¶");
                            break;
                        case 6:
                            _view.EnterLog(@$"/***");
                            _view.EnterLog(@$" *    __/$$$$$$____________________$$_____________________________");
                            _view.EnterLog(@$" *    _/$$____$$__________________|__/_____________________________");
                            _view.EnterLog(@$" *    |_$$__\_$$_/$$___/$$__/$$$$$$$_$$__/$$$$$$__/$$$$$$__/$$$$$$$_");
                            _view.EnterLog(@$" *    |_$$__|_$$|_$$__|_$$_/$$_____/|_$$_/$$____$$/$$____$$_$$____$$");
                            _view.EnterLog(@$" *    |_$$__|_$$|_$$__|_$$|__$$$$$$__$$_|$$__\__/_$$_____$$/$$____$$");
                            _view.EnterLog(@$" *    |_$$__|_$$|_$$__|_$$_\______$$_$$_|$$_____|_$$____$$_$$____$$");
                            _view.EnterLog(@$" *    |__$$$$$$/|__$$$$$$/_/$$$$$$$/|_$$|_$$_____|__$$$$$$/|_$$____$$");
                            _view.EnterLog(@$" *    _\______/__\______/_|_______/_|__/|__/______\______/_|__/__|__/");
                            _view.EnterLog(@$" *    _______________________________________________________________");
                            break;
                        default:
                            _view.EnterLog(_view.UNDEFINED_ARG);
                            break;
                    }
                }),
                new Command("help", "Show a help window", "help", () =>
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
            };
        }
    }
}