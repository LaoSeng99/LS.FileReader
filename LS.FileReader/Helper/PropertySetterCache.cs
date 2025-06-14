using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LS.FileReader.Tests")]
namespace LS.FileReader.Helper
{
    internal static class PropertySetterCache
    {
        private static readonly ConcurrentDictionary<string, Action<object, object?>> _setterCache = new ConcurrentDictionary<string, Action<object, object?>>();

        internal static Action<object, object?> GetOrCreateSetter(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException(nameof(propertyInfo));

            var key = $"{propertyInfo.DeclaringType!.FullName}.{propertyInfo.Name}";

            return _setterCache.GetOrAdd(key, _ =>
            {
                var targetType = propertyInfo.DeclaringType!;
                var propertyType = propertyInfo.PropertyType;

                var targetParam = Expression.Parameter(typeof(object), "target");
                var valueParam = Expression.Parameter(typeof(object), "value");

                var targetCast = Expression.Convert(targetParam, targetType);
                var valueCast = Expression.Convert(valueParam, propertyType);

                var propertySetter = propertyInfo.GetSetMethod();
                if (propertySetter == null)
                    throw new InvalidOperationException($"Property '{propertyInfo.Name}' does not have a setter.");

                var body = Expression.Call(targetCast, propertySetter, valueCast);

                var lambda = Expression.Lambda<Action<object, object?>>(body, targetParam, valueParam);
                return lambda.Compile();
            });
        }
    }
}
