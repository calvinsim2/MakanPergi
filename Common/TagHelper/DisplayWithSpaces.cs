using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Common.TagHelper
{
    public class DisplayWithSpaces : DisplayNameAttribute
    {
        public DisplayWithSpaces([CallerMemberName] string memberName = "")
        {
            Regex r = new Regex(@"(?<=[A-Z])(?=[A-Z][a-z]) | (?<=[^A-Z])(?=[A-Z]) | (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);
            DisplayNameValue = r.Replace(memberName, " ");
        }
    }
}
