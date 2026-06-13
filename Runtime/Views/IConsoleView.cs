using System;
using System.Collections.Generic;

namespace Ousiron.Console
{
    public interface IConsoleView
    {
        bool IsEnabled { get; set; }
        string Input { get;}
        string InputPrediction { get; set; }
        Action<string> OnChangedInput { get; set; }
        
        
        void EnterWarning(string msg);
        void EnterLog(string msg = null);
        void EnterError(string msg);
        void ClearInput();
        string GetPrediction(int id);
        void ClearPredictionList();
        int GetPredictionsCount();
        void SetInput(string input);
        void SetSelectedPredictedCommandID(int id);
        void SetFocusOnInput();
        void SetPredictionList(List<string> predictionList);
        void ClearConsole();
    }
}