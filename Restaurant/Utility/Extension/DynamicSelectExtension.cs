using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Restaurant.Utility
{
    public static class DynamicSelectExtension
    {
        public static IQueryable<TTarget> SelectTo<TSource, TTarget>(
            this IQueryable<TSource> source,
            Dictionary<string, string> propertyMappings = null)
            where TTarget : class, new()
        {
            var selector = BuildSelector<TSource, TTarget>(propertyMappings ?? new Dictionary<string, string>());
            return source.Select(selector);
        }

        private static Expression<Func<TSource, TTarget>> BuildSelector<TSource, TTarget>(
            Dictionary<string, string> propertyMappings)
            where TTarget : class, new()
        {
            var sourceParam = Expression.Parameter(typeof(TSource), "x");
            var targetType = typeof(TTarget);

            var newTarget = Expression.New(targetType);
            var bindings = BuildMemberBindings<TSource, TTarget>(sourceParam, propertyMappings);
            var memberInit = Expression.MemberInit(newTarget, bindings);

            return Expression.Lambda<Func<TSource, TTarget>>(memberInit, sourceParam);
        }

        private static List<MemberBinding> BuildMemberBindings<TSource, TTarget>(
            ParameterExpression sourceParam,
            Dictionary<string, string> propertyMappings)
        {
            var bindings = new List<MemberBinding>();
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);

            var sourceProps = sourceType.GetProperties();
            var targetProps = targetType.GetProperties().Where(p => p.CanWrite);

            foreach (var targetProp in targetProps)
            {
                // Try direct name match first
                var sourceProp = sourceProps.FirstOrDefault(p => p.Name == targetProp.Name);

                // If no direct match, check property mappings
                if (sourceProp == null)
                {
                    var mappedSourceName = propertyMappings.FirstOrDefault(kvp => kvp.Value == targetProp.Name).Key;
                    if (!string.IsNullOrEmpty(mappedSourceName))
                    {
                        sourceProp = sourceProps.FirstOrDefault(p => p.Name == mappedSourceName);
                    }
                }

                if (sourceProp == null) continue;

                Expression binding = null;

                // Handle direct property mapping (same type)
                if (targetProp.PropertyType == sourceProp.PropertyType)
                {
                    binding = Expression.Property(sourceParam, sourceProp);
                }
                // Handle collection mapping
                else if (IsGenericCollection(targetProp.PropertyType) && IsGenericCollection(sourceProp.PropertyType))
                {
                    binding = BuildCollectionSelect(sourceParam, sourceProp, targetProp, propertyMappings);
                }

                if (binding != null)
                {
                    bindings.Add(Expression.Bind(targetProp, binding));
                }
            }

            return bindings;
        }

        private static Expression BuildCollectionSelect(
            ParameterExpression sourceParam,
            PropertyInfo sourceProp,
            PropertyInfo targetProp,
            Dictionary<string, string> propertyMappings)
        {
            var sourceItemType = sourceProp.PropertyType.GetGenericArguments()[0];
            var targetItemType = targetProp.PropertyType.GetGenericArguments()[0];

            var itemParam = Expression.Parameter(sourceItemType, "oi");
            var itemBindings = BuildItemMemberBindings(itemParam, sourceItemType, targetItemType, propertyMappings);

            var itemNew = Expression.New(targetItemType);
            var itemInit = Expression.MemberInit(itemNew, itemBindings);
            var itemLambda = Expression.Lambda(itemInit, itemParam);

            var sourceCollection = Expression.Property(sourceParam, sourceProp);

            var selectMethod = typeof(Enumerable).GetMethods()
                .First(m => m.Name == "Select" && m.GetParameters().Length == 2)
                .MakeGenericMethod(sourceItemType, targetItemType);

            var selectCall = Expression.Call(selectMethod, sourceCollection, itemLambda);

            var toListMethod = typeof(Enumerable).GetMethod("ToList")
                .MakeGenericMethod(targetItemType);

            return Expression.Call(toListMethod, selectCall);
        }

        private static List<MemberBinding> BuildItemMemberBindings(
            ParameterExpression itemParam,
            Type sourceItemType,
            Type targetItemType,
            Dictionary<string, string> propertyMappings)
        {
            var bindings = new List<MemberBinding>();
            var sourceProps = sourceItemType.GetProperties();
            var targetProps = targetItemType.GetProperties().Where(p => p.CanWrite);

            foreach (var targetProp in targetProps)
            {
                // Try direct match first
                var sourceProp = sourceProps.FirstOrDefault(p =>
                    p.Name == targetProp.Name &&
                    p.PropertyType == targetProp.PropertyType);

                // Check mappings if no direct match
                if (sourceProp == null)
                {
                    var mappedSourceName = propertyMappings.FirstOrDefault(kvp => kvp.Value == targetProp.Name).Key;
                    if (!string.IsNullOrEmpty(mappedSourceName))
                    {
                        sourceProp = sourceProps.FirstOrDefault(p =>
                            p.Name == mappedSourceName &&
                            p.PropertyType == targetProp.PropertyType);
                    }
                }

                if (sourceProp != null)
                {
                    var propertyAccess = Expression.Property(itemParam, sourceProp);
                    bindings.Add(Expression.Bind(targetProp, propertyAccess));
                }
            }

            return bindings;
        }

        private static bool IsGenericCollection(Type type)
        {
            return type.IsGenericType &&
                   typeof(IEnumerable).IsAssignableFrom(type) &&
                   type != typeof(string);
        }
    }
}
