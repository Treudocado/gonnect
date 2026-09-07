using System;

namespace GOnnect.OutlookAddIn
{
    internal static class TelephoneUri
    {
        public static string Create(string number)
        {
            if (string.IsNullOrWhiteSpace(number))
            {
                throw new ArgumentException("Die Rufnummer ist leer.", nameof(number));
            }

            return "tel:" + Uri.EscapeDataString(number.Trim());
        }
    }
}

