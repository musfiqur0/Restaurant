using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Restaurant.Utility
{
    public static class DynamicMapper
    {
        public static void Map<TSource, TTarget>(TSource source, TTarget target)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (target == null) throw new ArgumentNullException(nameof(target));

            var visited = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);
            MapInternal(source, target, visited);
        }

        private static void MapInternal(object source, object target, Dictionary<object, object> visited)
        {
            if (source == null || target == null) return;

            // avoid mapping same source twice (prevent cycles)
            if (!visited.ContainsKey(source))
                visited[source] = target;

            var sourceType = source.GetType();
            var targetType = target.GetType();

            var sourceProps = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var targetProps = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);

            foreach (var sProp in sourceProps)
            {
                if (!sProp.CanRead) continue;

                // find target property by name (case-sensitive) and writable
                var tProp = targetProps.FirstOrDefault(p => p.Name == sProp.Name && p.CanWrite);
                if (tProp == null) continue;

                var sValue = sProp.GetValue(source);
                if (sValue == null)
                {
                    // set null if target accepts null
                    if (!tProp.PropertyType.IsValueType || Nullable.GetUnderlyingType(tProp.PropertyType) != null)
                        tProp.SetValue(target, null);
                    continue;
                }

                var sPropType = sProp.PropertyType;
                var tPropType = tProp.PropertyType;

                // Simple types (primitive, string, decimal, DateTime, enum, nullable of these)
                if (IsSimpleType(sPropType) && IsSimpleType(tPropType))
                {
                    // try direct assignment when possible (covers nullable conversions)
                    if (tPropType.IsAssignableFrom(sPropType))
                    {
                        tProp.SetValue(target, sValue);
                    }
                    else
                    {
                        try
                        {
                            var converted = Convert.ChangeType(sValue, Nullable.GetUnderlyingType(tPropType) ?? tPropType);
                            tProp.SetValue(target, converted);
                        }
                        catch
                        {
                            // ignore conversion errors - leave default
                        }
                    }
                    continue;
                }

                // Handle collections (but not string)
                if (typeof(IEnumerable).IsAssignableFrom(sPropType) && sPropType != typeof(string))
                {
                    // get source enumerable
                    var sEnum = (sValue as IEnumerable);
                    if (sEnum == null) continue;

                    // get target element type
                    Type tElementType = GetCollectionElementType(tPropType);
                    if (tElementType == null) continue;

                    // create a target collection instance (List<TElement> when target is interface)
                    object tCollection;
                    if (tPropType.IsInterface || tPropType.IsAbstract)
                    {
                        var listType = typeof(List<>).MakeGenericType(tElementType);
                        tCollection = Activator.CreateInstance(listType)!;
                    }
                    else
                    {
                        try
                        {
                            tCollection = Activator.CreateInstance(tPropType)!;
                        }
                        catch
                        {
                            // fallback to List<T>
                            var listType = typeof(List<>).MakeGenericType(tElementType);
                            tCollection = Activator.CreateInstance(listType)!;
                        }
                    }

                    // get Add method
                    var addMethod = tCollection.GetType().GetMethod("Add");
                    foreach (var sElem in sEnum)
                    {
                        if (sElem == null) continue;

                        // If this source element was already mapped, reuse mapped target
                        if (visited.TryGetValue(sElem, out var existing))
                        {
                            addMethod?.Invoke(tCollection, new[] { existing });
                            continue;
                        }

                        // create target element instance
                        object tElem;
                        try
                        {
                            tElem = Activator.CreateInstance(tElementType)!;
                        }
                        catch
                        {
                            // if cannot create instance, skip
                            continue;
                        }

                        visited[sElem] = tElem; // mark before recursive mapping (prevents cycles)
                        MapInternal(sElem, tElem, visited);
                        addMethod?.Invoke(tCollection, new[] { tElem });
                    }

                    // assign collection to target property (works when List<T> assignable to ICollection<T>)
                    tProp.SetValue(target, tCollection);
                    continue;
                }

                // Complex object mapping (non-collection)
                {
                    // If source object already mapped -> reuse mapped target instance
                    if (visited.TryGetValue(sValue, out var knownTarget))
                    {
                        if (tProp.PropertyType.IsAssignableFrom(knownTarget.GetType()))
                            tProp.SetValue(target, knownTarget);
                        continue;
                    }

                    // create instance of target property type
                    object nestedTarget;
                    try
                    {
                        nestedTarget = Activator.CreateInstance(tPropType)!;
                    }
                    catch
                    {
                        // cannot construct target; try to set if assignable
                        if (tPropType.IsAssignableFrom(sValue.GetType()))
                        {
                            tProp.SetValue(target, sValue);
                        }
                        continue;
                    }

                    visited[sValue] = nestedTarget;
                    MapInternal(sValue, nestedTarget, visited);
                    tProp.SetValue(target, nestedTarget);
                }
            }
        }

        private static bool IsSimpleType(Type t)
        {
            t = Nullable.GetUnderlyingType(t) ?? t;
            return t.IsPrimitive
                || t.IsEnum
                || t == typeof(string)
                || t == typeof(decimal)
                || t == typeof(DateTime)
                || t == typeof(DateTimeOffset)
                || t == typeof(Guid)
                || t == typeof(TimeSpan);
        }

        private static Type? GetCollectionElementType(Type collectionType)
        {
            // if generic IEnumerable<T> or ICollection<T> etc.
            if (collectionType.IsArray) return collectionType.GetElementType();

            var ifaces = new[] { collectionType }.Concat(collectionType.GetInterfaces());
            foreach (var iface in ifaces)
            {
                if (iface.IsGenericType)
                {
                    var gen = iface.GetGenericTypeDefinition();
                    if (gen == typeof(IEnumerable<>) || gen == typeof(ICollection<>) || gen == typeof(IList<>))
                        return iface.GetGenericArguments()[0];
                }
            }

            // if non-generic IEnumerable, we cannot determine element type
            return null;
        }

        // small comparer to use reference equality for Dictionary keys (so different objects with same values are distinct)
        private class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();
            public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);
            public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
    }
}
