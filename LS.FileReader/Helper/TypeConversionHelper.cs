using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LS.FileReader.Tests")]
namespace LS.FileReader.Helper
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class TypeConversionHelper
    {
        internal static bool TryConvert(string input, Type type, out object value)
        {
            value = null;

            if (string.IsNullOrWhiteSpace(input))
            {
                value = GetDefault(type);
                return false;
            }

            try
            {
                if (type == typeof(string)) { value = input; return true; }
                if (type == typeof(int) || type == typeof(int?)) { value = int.Parse(input); return true; }
                if (type == typeof(decimal) || type == typeof(decimal?)) { value = decimal.Parse(input); return true; }
                if (type == typeof(DateTime) || type == typeof(DateTime?)) { value = DateTime.Parse(input); return true; }
                if (type.IsEnum) { value = Enum.Parse(type, input, true); return true; }

                value = Convert.ChangeType(input, type);
                return true;
            }
            catch
            {
                value = GetDefault(type);
                return false;
            }
        }

        private static object GetDefault(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}
