using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace GOnnect.OutlookAddIn.Tests
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                TestPhoneNumberSelection();
                TestRibbonXmlEscaping();
                TestTelephoneUri();
                TestComContractMetadata();
                Console.WriteLine("All tests passed.");
                return 0;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception.Message);
                return 1;
            }
        }

        private static void TestPhoneNumberSelection()
        {
            var contact = new TestContact
            {
                MobileTelephoneNumber = "0151 123456",
                BusinessTelephoneNumber = "+49 40 1234",
                OtherTelephoneNumber = "0151-123456",
                PagerNumber = "040 9876"
            };

            var entries = PhoneNumberProvider.GetEntries(contact);
            Assert(entries.Count == 3, "Duplicate numbers must be suppressed.");
            Assert(entries[0].Label == "Mobiltelefon", "Mobile number label is wrong.");
            Assert(entries[1].Label == "Geschäftlich", "Business number label is wrong.");
            Assert(entries[2].Label == "Weitere", "Additional number label is wrong.");
            Assert(entries.Select(entry => entry.Number).Contains("040 9876"),
                "Pager field must be exposed as an additional number.");
        }

        private static void TestRibbonXmlEscaping()
        {
            var entries = new[] { new PhoneNumberEntry("Weitere", "+49&123") };
            var xml = RibbonXmlBuilder.BuildMenuContent(entries, "Contact-Menu");

            Assert(xml.Contains("+49&amp;123"), "Phone number must be XML escaped.");
            Assert(xml.Contains("onAction=\"DialPhoneNumber\""),
                "Dial callback is missing.");
            Assert(xml.Contains("id=\"GOnnectContactMenuPhone0\""),
                "Generated control ID is wrong.");
        }

        private static void TestTelephoneUri()
        {
            var uri = TelephoneUri.Create("+49 40 123#");
            Assert(uri == "tel:%2B49%2040%20123%23", "Telephone URI is wrong: " + uri);
        }

        private static void TestComContractMetadata()
        {
            AssertDualInterface(typeof(IDTExtensibility2));
            AssertDualInterface(typeof(IRibbonExtensibility));

            AssertVariantSafeArray("OnConnection", 3);
            AssertVariantSafeArray("OnDisconnection", 1);
            AssertVariantSafeArray("OnAddInsUpdate", 0);
            AssertVariantSafeArray("OnStartupComplete", 0);
            AssertVariantSafeArray("OnBeginShutdown", 0);
        }

        private static void AssertVariantSafeArray(string methodName, int parameterIndex)
        {
            var method = typeof(IDTExtensibility2).GetMethod(methodName);
            Assert(method != null, methodName + " COM method is missing.");
            var customParameter = method.GetParameters()[parameterIndex];
            var marshalAs = (MarshalAsAttribute)Attribute.GetCustomAttribute(
                customParameter,
                typeof(MarshalAsAttribute));

            Assert(customParameter.IsIn, "The custom SAFEARRAY must be marked as input.");
            Assert(marshalAs != null && marshalAs.Value == UnmanagedType.SafeArray,
                "The custom parameter must be marshaled as SAFEARRAY.");
            Assert(marshalAs.SafeArraySubType == VarEnum.VT_VARIANT,
                "The custom SAFEARRAY must contain VARIANT values.");
        }

        private static void AssertDualInterface(Type interfaceType)
        {
            var attribute = (InterfaceTypeAttribute)Attribute.GetCustomAttribute(
                interfaceType,
                typeof(InterfaceTypeAttribute));
            Assert(attribute != null && attribute.Value == ComInterfaceType.InterfaceIsDual,
                interfaceType.Name + " must be declared as a dual COM interface.");
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private sealed class TestContact
        {
            public string MobileTelephoneNumber { get; set; }
            public string BusinessTelephoneNumber { get; set; }
            public string Business2TelephoneNumber { get; set; }
            public string HomeTelephoneNumber { get; set; }
            public string Home2TelephoneNumber { get; set; }
            public string OtherTelephoneNumber { get; set; }
            public string PrimaryTelephoneNumber { get; set; }
            public string CompanyMainTelephoneNumber { get; set; }
            public string AssistantTelephoneNumber { get; set; }
            public string CallbackTelephoneNumber { get; set; }
            public string CarTelephoneNumber { get; set; }
            public string PagerNumber { get; set; }
            public string RadioTelephoneNumber { get; set; }
            public string ISDNNumber { get; set; }
        }
    }

}
