using System;
using System.Runtime.InteropServices;

namespace GOnnect.OutlookAddIn.ComSmoke
{
    internal static class Program
    {
        private const string ProgrammaticId = "GOnnect.OutlookAddIn";
        private const int FirstDualInterfaceMethod = 7;
        private const ushort VariantType = 12;

        private static int Main()
        {
            object instance = null;
            try
            {
                var comType = Type.GetTypeFromProgID(ProgrammaticId, true);
                instance = Activator.CreateInstance(comType);

                var unknown = Marshal.GetIUnknownForObject(instance);
                try
                {
                    TestExtensibilityInterface(unknown);
                    TestRibbonInterface(unknown);
                }
                finally
                {
                    Marshal.Release(unknown);
                }

                Console.WriteLine("Independent COM activation and vtable test passed.");
                return 0;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception);
                return 1;
            }
            finally
            {
                if (instance != null && Marshal.IsComObject(instance))
                {
                    Marshal.FinalReleaseComObject(instance);
                }
            }
        }

        private static void TestExtensibilityInterface(IntPtr unknown)
        {
            var interfaceId = new Guid("B65AD801-ABAF-11D0-BB8B-00A0C90F2744");
            var extensibility = QueryInterface(unknown, interfaceId);
            IntPtr application = IntPtr.Zero;
            IntPtr custom = IntPtr.Zero;

            try
            {
                application = Marshal.GetIDispatchForObject(new ComTestApplication());
                custom = SafeArrayCreateVector(VariantType, 0, 0);
                Assert(custom != IntPtr.Zero, "An empty VARIANT SAFEARRAY could not be created.");

                var onConnection = GetVtableDelegate<OnConnectionDelegate>(
                    extensibility,
                    FirstDualInterfaceMethod);
                Marshal.ThrowExceptionForHR(onConnection(
                    extensibility,
                    application,
                    ExtConnectMode.Startup,
                    IntPtr.Zero,
                    ref custom));

                var onDisconnection = GetVtableDelegate<OnDisconnectionDelegate>(
                    extensibility,
                    FirstDualInterfaceMethod + 1);
                Marshal.ThrowExceptionForHR(onDisconnection(
                    extensibility,
                    ExtDisconnectMode.UserClosed,
                    ref custom));
            }
            finally
            {
                if (custom != IntPtr.Zero)
                {
                    SafeArrayDestroy(custom);
                }
                if (application != IntPtr.Zero)
                {
                    Marshal.Release(application);
                }
                Marshal.Release(extensibility);
            }
        }

        private static void TestRibbonInterface(IntPtr unknown)
        {
            var interfaceId = new Guid("000C0396-0000-0000-C000-000000000046");
            var ribbon = QueryInterface(unknown, interfaceId);
            IntPtr ribbonId = IntPtr.Zero;
            IntPtr ribbonXml = IntPtr.Zero;

            try
            {
                ribbonId = Marshal.StringToBSTR("Microsoft.Outlook.Explorer");
                var getCustomUi = GetVtableDelegate<GetCustomUiDelegate>(
                    ribbon,
                    FirstDualInterfaceMethod);
                Marshal.ThrowExceptionForHR(getCustomUi(ribbon, ribbonId, out ribbonXml));

                var xml = ribbonXml == IntPtr.Zero
                    ? null
                    : Marshal.PtrToStringBSTR(ribbonXml);
                Assert(xml != null && xml.Contains("GOnnectDialContactMenu"),
                    "The COM Ribbon callback returned no Outlook menu.");
            }
            finally
            {
                if (ribbonXml != IntPtr.Zero)
                {
                    Marshal.FreeBSTR(ribbonXml);
                }
                if (ribbonId != IntPtr.Zero)
                {
                    Marshal.FreeBSTR(ribbonId);
                }
                Marshal.Release(ribbon);
            }
        }

        private static IntPtr QueryInterface(IntPtr unknown, Guid interfaceId)
        {
            IntPtr interfacePointer;
            Marshal.ThrowExceptionForHR(Marshal.QueryInterface(
                unknown,
                ref interfaceId,
                out interfacePointer));
            Assert(interfacePointer != IntPtr.Zero,
                "COM QueryInterface returned a null pointer for " + interfaceId + ".");
            return interfacePointer;
        }

        private static T GetVtableDelegate<T>(IntPtr interfacePointer, int slot)
            where T : class
        {
            var vtable = Marshal.ReadIntPtr(interfacePointer);
            var method = Marshal.ReadIntPtr(vtable, slot * IntPtr.Size);
            return (T)(object)Marshal.GetDelegateForFunctionPointer(method, typeof(T));
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        [DllImport("oleaut32.dll")]
        private static extern IntPtr SafeArrayCreateVector(
            ushort variantType,
            int lowerBound,
            uint elementCount);

        [DllImport("oleaut32.dll")]
        private static extern int SafeArrayDestroy(IntPtr safeArray);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int OnConnectionDelegate(
            IntPtr self,
            IntPtr application,
            ExtConnectMode connectMode,
            IntPtr addInInstance,
            ref IntPtr custom);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int OnDisconnectionDelegate(
            IntPtr self,
            ExtDisconnectMode removeMode,
            ref IntPtr custom);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int GetCustomUiDelegate(
            IntPtr self,
            IntPtr ribbonId,
            out IntPtr ribbonXml);
    }

    internal enum ExtConnectMode
    {
        Startup = 1
    }

    internal enum ExtDisconnectMode
    {
        UserClosed = 1
    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDispatch)]
    public sealed class ComTestApplication
    {
    }
}
