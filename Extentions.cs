using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public static class Extentions
    {
        public static string FirstCharToUpper(this string input) =>
           input switch
           {
               null => throw new ArgumentNullException(nameof(input)),
               "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
               _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
           };
    }
}
