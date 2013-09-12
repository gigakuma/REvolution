using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using REvolution.Core.Symbols;
using System.Text.RegularExpressions;

namespace TestCases
{
    class Program
    {
        static void Main(string[] args)
        {
            SymbolManager manager = new SymbolManager();
            //manager.RegisterSymbol("year");
            //manager.RegisterSymbol("month");
            //manager.RegisterSymbol("day");
            //manager["year"].CreateExpression().Pattern = "[0-2]\\d{3}";
            //manager["month"].CreateExpression().Pattern = "\\d|1[0-2]";
            //manager["day"].CreateExpression().Pattern = "\\d|[12]\\d|3[01]";
            //manager["day"].Default.ChangeLabel("case1");
            //manager.RegisterSymbol("date");
            //manager["date"].CreateExpression().Pattern = "(?@year/0){1,2}-(?@month/0)-(?@day/case1)";
            //Expression date_d = manager["date"].Default;
            //date_d.Parse();
            //date_d.Link();
            
            char a = '\b';
            Regex regex = new Regex("^((?>[a-zA-Z\\d!#$%&'*+\\-/=?^_`{|}~]+\x20*|\"((?=[\x01-\x7f])[^\"\\\\]|\\\\[\\x01-\\x7f])*\"\\x20*)*(?<angle><))?((?!\\.)(?>\\.?[a-zA-Z\\d!#$%&'*+\\-/=?^_`{|}~]+)+|\"((?=[\\x01-\\x7f])[^\"\\\\]|\\\\[\\x01-\\x7f])*\")@(((?!-)[a-zA-Z\\d\\-]+(?<!-)\\.)+[a-zA-Z]{2,}|\\[(((?(?<!\\[)\\.)(25[0-5]|2[0-4]\\d|[01]?\\d?\\d)){4}|[a-zA-Z\\d\\-]*[a-zA-Z\\d]:((?=[\\x01-\\x7f])[^\\\\[\\]]|\\\\[\\x01-\\x7f])+)\\])(?(angle)>)$");
            Symbol year = manager.RegisterSymbol("year");
            Symbol month = manager.RegisterSymbol("month");
            Symbol day = manager.RegisterSymbol("day");
            Symbol hour = manager.RegisterSymbol("hour");
            Symbol minute = manager.RegisterSymbol("minute");
            Symbol second = manager.RegisterSymbol("second");
            year.CreateExpression().Pattern = "(?:[1-9][0-9]*)?[0-9]{4}";
            month.CreateExpression().Pattern = "1[0-2]|0[1-9]";
            day.CreateExpression().Pattern = "3[0-1]|0[1-9]|[1-2][0-9]";
            hour.CreateExpression().Pattern = "2[0-3]|[0-1][0-9]";
            minute.CreateExpression().Pattern = "[0-5][0-9]";
            second.CreateExpression().Pattern = "([0-5][0-9])(\\.[0-9]+)?";

            Symbol date = manager.RegisterSymbol("date");
            Symbol time = manager.RegisterSymbol("time");
            date.CreateExpression().Pattern = "(?@year)-(?@month)-(?@day)";
            time.CreateExpression().Pattern = "(?@hour)-(?@minute)-(?@second)";

            Symbol dt = manager.RegisterSymbol("date_time");
            dt.CreateExpression().Pattern = "(?@date)T(?@time)Z";

            dt.Default.Parse();
            dt.Default.Link();
            string str = dt.Default.Generate();
            
            //Regex regex = new Regex(str, RegexOptions.IgnorePatternWhitespace);
            Console.Write(str);
        }
    }
}
