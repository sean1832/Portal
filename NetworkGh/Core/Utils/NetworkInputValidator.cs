using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NetworkGh.Core.Utils
{
    internal static class NetworkInputValidator
    {
        public static (bool, string) IsEndpointValid(string endpointName)
        {
            Regex validRegex = new Regex(@"^/[a-zA-Z0-9_/]+$", RegexOptions.Compiled);
            if (string.IsNullOrEmpty(endpointName))
                return (false, "Empty or null");
            if (!endpointName.StartsWith("/"))
                return (false, "Not start with '/'");
            if (endpointName.Length < 2)
                return (false, "Less than 2 character");
            if (endpointName.EndsWith("/"))
                return (false, "End with '/'");
            if (endpointName.Contains("//"))
                return (false, "Contains '//'");
            if (endpointName.Contains(" "))
                return (false, "Contains space");
            if (!validRegex.IsMatch(endpointName))
                return (false, "Contain illegal characters");

            return (true, "");
        }
    }
}
