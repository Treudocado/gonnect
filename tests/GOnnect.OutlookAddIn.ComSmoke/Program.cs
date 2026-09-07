using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GOnnect.OutlookAddIn.ComSmoke
{
    internal static class Program
    {
        private const string ProgrammaticId = "GOnnect.OutlookAddIn";

        private static int Main()
        {
            object instance = null;
            try
            {
                var comType = Type.GetTypeFromProgID(ProgrammaticId, true);
                instance = Activator.CreateInstance(comType);
                Assert(Marshal.IsComObject(instance),
                    "COM activation did not return a COM proxy.");

                var extensibility = (IDTExtensibility2)instance;
                Array custom = new object[0];
                extensibility.OnConnection(
                    new ComTestApplication(),
                    ExtConnectMode.Startup,
                    null,
                    ref custom);

                var ribbon = (IRibbonExtensibility)instance;
                var xml = ribbon.GetCustomUI("Microsoft.Outlook.Explorer");
                Assert(xml != null && xml.Contains("GOnnectDialContactMenu"),
                    "The COM Ribbon callback returned no Outlook menu.");

                extensibility.OnDisconnection(ExtDisconnectMode.UserClosed, ref custom);
                Console.WriteLine("Independent COM activation test passed.");
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

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }

    internal enum ExtConnectMode
    {
        Startup = 1
    }

    internal enum ExtDisconnectMode
    {
        UserClosed = 1
    }

    [ComImport]
    [Guid("B65AD801-ABAF-11D0-BB8B-00A0C90F2744")]
    [TypeLibType(TypeLibTypeFlags.FDual | TypeLibTypeFlags.FDispatchable)]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    internal interface IDTExtensibility2
    {
        [DispId(1)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void OnConnection(
            [In, MarshalAs(UnmanagedType.IDispatch)] object application,
            [In] ExtConnectMode connectMode,
            [In, MarshalAs(UnmanagedType.IDispatch)] object addInInstance,
            [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)]
            ref Array custom);

        [DispId(2)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void OnDisconnection(
            [In] ExtDisconnectMode removeMode,
            [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)]
            ref Array custom);

        [DispId(3)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void OnAddInsUpdate(
            [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)]
            ref Array custom);

        [DispId(4)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void OnStartupComplete(
            [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)]
            ref Array custom);

        [DispId(5)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void OnBeginShutdown(
            [In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)]
            ref Array custom);
    }

    [ComImport]
    [Guid("000C0396-0000-0000-C000-000000000046")]
    [TypeLibType(TypeLibTypeFlags.FDual | TypeLibTypeFlags.FDispatchable)]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    internal interface IRibbonExtensibility
    {
        [DispId(1)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [return: MarshalAs(UnmanagedType.BStr)]
        string GetCustomUI([In, MarshalAs(UnmanagedType.BStr)] string ribbonId);
    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDispatch)]
    public sealed class ComTestApplication
    {
    }
}
