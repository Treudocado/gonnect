using System.Collections.Generic;
using System.Security;
using System.Text;

namespace GOnnect.OutlookAddIn
{
    internal static class RibbonXmlBuilder
    {
        private const string Namespace = "http://schemas.microsoft.com/office/2009/07/customui";

        public static string BuildMenuContent(
            IReadOnlyList<PhoneNumberEntry> entries,
            string controlId)
        {
            var idPrefix = SanitizeId(controlId);
            var xml = new StringBuilder();
            xml.Append("<menu xmlns=\"").Append(Namespace).Append("\">");

            if (entries.Count == 0)
            {
                xml.Append("<button id=\"")
                    .Append(idPrefix)
                    .Append("Empty\" label=\"Keine Rufnummer vorhanden\" enabled=\"false\" />");
            }
            else
            {
                for (var index = 0; index < entries.Count; index++)
                {
                    var entry = entries[index];
                    var label = entry.Label + ": " + entry.Number;
                    xml.Append("<button id=\"")
                        .Append(idPrefix)
                        .Append("Phone")
                        .Append(index)
                        .Append("\" label=\"")
                        .Append(Escape(label))
                        .Append("\" tag=\"")
                        .Append(Escape(entry.Number))
                        .Append("\" imageMso=\"Call\" onAction=\"DialPhoneNumber\" />");
                }
            }

            xml.Append("</menu>");
            return xml.ToString();
        }

        private static string Escape(string value)
        {
            return SecurityElement.Escape(value) ?? string.Empty;
        }

        private static string SanitizeId(string value)
        {
            var result = new StringBuilder("GOnnect");
            foreach (var character in value ?? string.Empty)
            {
                if (char.IsLetterOrDigit(character) || character == '_')
                {
                    result.Append(character);
                }
            }

            return result.ToString();
        }
    }
}

