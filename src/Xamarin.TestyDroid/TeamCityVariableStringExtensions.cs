using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.TestyDroid
{
    public static class TeamCityVariableStringExtensions
    {
        public static string EscapeTeamCitySpecialCharacters(this string variableValue)
        {
            // need to perform escaping on the vairbale value

            //            Character	Should be escaped as
            //  ' (apostrophe)	|'
            //  \n (line feed)	|n
            //  \r (carriage return)	|r
            //  \u0085 (next line)	|x
            //  \u2028 (line separator)	|l
            //  \u2029 (paragraph separator)	|p
            //  | (vertical bar)	||
            //  [ (opening bracket)	|[	 
            //  ] (closing bracket)	|]

            // Escape pipes first.
            variableValue = EscapeTeamCitySpecialCharacter(variableValue, '|');

            variableValue = EscapeTeamCitySpecialCharacter(variableValue, '\'');
            variableValue = EscapeTeamCityLineFeed(variableValue);
            variableValue = EscapeTeamCityCarriageReturn(variableValue);
            variableValue = EscapeTeamCitySpecialCharacter(variableValue, '\u0085', "|x");
            variableValue = EscapeTeamCitySpecialCharacter(variableValue, '\u2028', "|l");
            variableValue = EscapeTeamCitySpecialCharacter(variableValue, '\u2029', "|p");

            variableValue = EscapeTeamCitySpecialCharacter(variableValue, '[');
            variableValue = EscapeTeamCitySpecialCharacter(variableValue, ']');
            return variableValue;
        }

        private static string EscapeTeamCitySpecialCharacter(string variableValue, char charToEscape, string replaceWith = "")
        {
            string escapeString = new string(charToEscape, 1);
            if (string.IsNullOrEmpty(replaceWith))
            {
                variableValue = variableValue.Replace(escapeString, "|'" + escapeString);
            }
            else
            {
                variableValue = variableValue.Replace(escapeString, replaceWith);
            }
            return variableValue;
        }

        private static string EscapeTeamCityLineFeed(string variableValue)
        {
            var newLineChar = '\n';
            var escapeString = new string(newLineChar, 1);
            variableValue = variableValue.Replace(escapeString, "|n");
            return variableValue;
        }

        private static string EscapeTeamCityCarriageReturn(string variableValue)
        {
            char newLineChar = '\r';
            var escapeString = new string(newLineChar, 1);
            variableValue = variableValue.Replace(escapeString, "|r");
            return variableValue;
        }

    }
}

