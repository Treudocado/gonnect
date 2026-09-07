using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;

namespace GOnnect.OutlookAddIn
{
    internal static class ComDispatch
    {
        public static object GetProperty(object target, string propertyName)
        {
            if (target == null)
            {
                return null;
            }

            return target.GetType().InvokeMember(
                propertyName,
                BindingFlags.GetProperty,
                null,
                target,
                null,
                CultureInfo.InvariantCulture);
        }

        public static object InvokeMethod(object target, string methodName, params object[] args)
        {
            if (target == null)
            {
                return null;
            }

            return target.GetType().InvokeMember(
                methodName,
                BindingFlags.InvokeMethod,
                null,
                target,
                args,
                CultureInfo.InvariantCulture);
        }

        public static string GetStringProperty(object target, string propertyName)
        {
            try
            {
                return Convert.ToString(GetProperty(target, propertyName), CultureInfo.InvariantCulture)
                    ?? string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static void Release(object value)
        {
            if (value != null && Marshal.IsComObject(value))
            {
                Marshal.ReleaseComObject(value);
            }
        }
    }
}
