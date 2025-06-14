using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using LS.FileReader.Attributes;

[assembly: InternalsVisibleTo("LS.FileReader.Tests")]
namespace LS.FileReader.Helper
{
    internal static class PropertyMappingCache
    {
        internal static Dictionary<string, (PropertyInfo Prop, Action<object, object?> Setter, Type Type)> Build<T>() where T : new()
        {
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var dict = new Dictionary<string, (PropertyInfo, Action<object, object?>, Type)>(StringComparer.OrdinalIgnoreCase);

            foreach (var prop in props)
            {
                var setter = PropertySetterCache.GetOrCreateSetter(prop);
                var type = prop.PropertyType;

                var attr = prop.GetCustomAttribute<HeaderColumnAttribute>();
                if (attr != null)
                {
                    foreach (var alias in attr.Aliases)
                    {
                        if (!string.IsNullOrWhiteSpace(alias))
                            dict[alias.Trim()] = (prop, setter, type);
                    }
                }

                dict[prop.Name] = (prop, setter, type);
            }

            return dict;
        }
    }
}
