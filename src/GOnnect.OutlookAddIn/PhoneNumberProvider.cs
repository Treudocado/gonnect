using System;
using System.Collections.Generic;
using System.Text;

namespace GOnnect.OutlookAddIn
{
    internal sealed class PhoneNumberEntry
    {
        public PhoneNumberEntry(string label, string number)
        {
            Label = label;
            Number = number;
        }

        public string Label { get; }

        public string Number { get; }
    }

    internal static class PhoneNumberProvider
    {
        private sealed class FieldDefinition
        {
            public FieldDefinition(string propertyName, string label)
            {
                PropertyName = propertyName;
                Label = label;
            }

            public string PropertyName { get; }

            public string Label { get; }
        }

        private static readonly FieldDefinition[] Fields =
        {
            new FieldDefinition("MobileTelephoneNumber", "Mobiltelefon"),
            new FieldDefinition("BusinessTelephoneNumber", "Geschäftlich"),
            new FieldDefinition("Business2TelephoneNumber", "Geschäftlich 2"),
            new FieldDefinition("HomeTelephoneNumber", "Privat"),
            new FieldDefinition("Home2TelephoneNumber", "Privat 2"),
            new FieldDefinition("OtherTelephoneNumber", "Weitere"),
            new FieldDefinition("PrimaryTelephoneNumber", "Primär"),
            new FieldDefinition("CompanyMainTelephoneNumber", "Firma"),
            new FieldDefinition("AssistantTelephoneNumber", "Assistenz"),
            new FieldDefinition("CallbackTelephoneNumber", "Rückruf"),
            new FieldDefinition("CarTelephoneNumber", "Autotelefon"),
            new FieldDefinition("PagerNumber", "Weitere"),
            new FieldDefinition("RadioTelephoneNumber", "Weitere"),
            new FieldDefinition("ISDNNumber", "ISDN")
        };

        public static IReadOnlyList<PhoneNumberEntry> GetEntries(object contact)
        {
            var result = new List<PhoneNumberEntry>();
            var seenNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var labelCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var field in Fields)
            {
                var number = ComDispatch.GetStringProperty(contact, field.PropertyName).Trim();
                var normalizedNumber = Normalize(number);
                if (normalizedNumber.Length == 0 || !seenNumbers.Add(normalizedNumber))
                {
                    continue;
                }

                int count;
                labelCounts.TryGetValue(field.Label, out count);
                count++;
                labelCounts[field.Label] = count;

                var label = count == 1 ? field.Label : field.Label + " " + count;
                result.Add(new PhoneNumberEntry(label, number));
            }

            return result;
        }

        private static string Normalize(string number)
        {
            var result = new StringBuilder(number.Length);
            foreach (var character in number)
            {
                if (char.IsDigit(character) || character == '+' || character == '*'
                    || character == '#')
                {
                    result.Append(character);
                }
            }

            return result.ToString();
        }
    }
}

