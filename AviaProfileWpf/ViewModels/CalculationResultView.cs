using AviaProfileWpf.Models;
using ControlzEx.Standard;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AviaProfileWpf.ViewModels;

/// <summary>
/// Класс групировки CalculationResult по группам с именем ResultsName
/// </summary>
public class CalculationResultView
{
    public string ResultsName { get; set; }

    public ObservableCollection<CalculationResult> Results { get; set; }

    public CalculationResultView(string resultsName, ObservableCollection<CalculationResult> values)
    {
        ResultsName = resultsName;
        Results = values;
    }    
}
