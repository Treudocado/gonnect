using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GOnnect.OutlookAddIn
{
    [ComVisible(true)]
    [Guid("A3D2629C-32F1-48E7-BD24-AC02E70427E8")]
    [ProgId("GOnnect.OutlookAddIn")]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public sealed class Connect : IDTExtensibility2, IRibbonExtensibility
    {
        public const string ClassId = "A3D2629C-32F1-48E7-BD24-AC02E70427E8";
        public const string ProgrammaticId = "GOnnect.OutlookAddIn";

        private object outlookApplication;

        public void OnConnection(
            object application,
            ExtConnectMode connectMode,
            object addInInstance,
            ref Array custom)
        {
            outlookApplication = application;
        }

        public void OnDisconnection(ExtDisconnectMode removeMode, ref Array custom)
        {
            outlookApplication = null;
        }

        public void OnAddInsUpdate(ref Array custom)
        {
        }

        public void OnStartupComplete(ref Array custom)
        {
        }

        public void OnBeginShutdown(ref Array custom)
        {
            outlookApplication = null;
        }

        public string GetCustomUI(string ribbonId)
        {
            return string.Equals(
                ribbonId,
                "Microsoft.Outlook.Explorer",
                StringComparison.OrdinalIgnoreCase)
                ? RibbonMarkup.Explorer
                : null;
        }

        public bool GetMenuVisible(object control)
        {
            object contact;
            if (!TryGetSelectedContact(out contact))
            {
                return false;
            }

            ComDispatch.Release(contact);
            return true;
        }

        public string GetPhoneMenuContent(object control)
        {
            object contact;
            if (!TryGetSelectedContact(out contact))
            {
                return RibbonXmlBuilder.BuildMenuContent(
                    new PhoneNumberEntry[0],
                    GetControlId(control));
            }

            try
            {
                return RibbonXmlBuilder.BuildMenuContent(
                    PhoneNumberProvider.GetEntries(contact),
                    GetControlId(control));
            }
            finally
            {
                ComDispatch.Release(contact);
            }
        }

        public void DialPhoneNumber(object control)
        {
            try
            {
                var number = ComDispatch.GetStringProperty(control, "Tag");
                var startInfo = new ProcessStartInfo
                {
                    FileName = TelephoneUri.Create(number),
                    UseShellExecute = true
                };
                Process.Start(startInfo);
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    "Die Rufnummer konnte nicht an GOnnect übergeben werden.\r\n\r\n"
                        + exception.Message,
                    "GOnnect",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private bool TryGetSelectedContact(out object contact)
        {
            contact = null;
            object explorer = null;
            object selection = null;
            object selectedItem = null;

            try
            {
                if (outlookApplication == null)
                {
                    return false;
                }

                explorer = ComDispatch.InvokeMethod(outlookApplication, "ActiveExplorer");
                selection = ComDispatch.GetProperty(explorer, "Selection");

                var count = Convert.ToInt32(
                    ComDispatch.GetProperty(selection, "Count"),
                    CultureInfo.InvariantCulture);
                if (count != 1)
                {
                    return false;
                }

                selectedItem = ComDispatch.InvokeMethod(selection, "Item", 1);
                var messageClass = ComDispatch.GetStringProperty(selectedItem, "MessageClass");
                if (!messageClass.StartsWith("IPM.Contact", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                contact = selectedItem;
                selectedItem = null;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                ComDispatch.Release(selectedItem);
                ComDispatch.Release(selection);
                ComDispatch.Release(explorer);
            }
        }

        private static string GetControlId(object control)
        {
            var controlId = ComDispatch.GetStringProperty(control, "Id");
            return string.IsNullOrWhiteSpace(controlId) ? "DialMenu" : controlId;
        }
    }
}
