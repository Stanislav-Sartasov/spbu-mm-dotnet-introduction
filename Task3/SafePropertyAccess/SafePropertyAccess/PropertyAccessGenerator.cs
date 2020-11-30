using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SafePropertyAccess
{
    public static class PropertyAccessGenerator
    {
        public static Func<TObject, TProperty> Generate<TObject, TProperty>(string[] pathComponents)
        {
            if (pathComponents.Length == 0)
            {
                throw new ArgumentException("Path cannot be empty");
            }

            var sourceType = typeof(TObject);
            var source = Expression.Parameter(sourceType, "source");
            var pathComponentsQueue = new Queue<string>(pathComponents);
            var variables = new List<ParameterExpression>();

            var conditionalQuery = ConstructCondition<TProperty>(pathComponentsQueue, source, sourceType, variables);

            var propertyVar = variables.Last();
            var lambdaBody = Expression.Block(variables.ToArray(), new[] {conditionalQuery, propertyVar});
            var lambdaExpr = Expression.Lambda<Func<TObject, TProperty>>(lambdaBody, new[] {source});

            return lambdaExpr.Compile();
        }

        private static Expression ConstructCondition<TProperty>(Queue<string> pathComponents,
            ParameterExpression source,
            Type sourceType, List<ParameterExpression> variables)
        {
            var componentName = pathComponents.Dequeue();
            var propertyInfo = sourceType.GetProperty(componentName);

            if (propertyInfo is null)
            {
                throw new NullReferenceException($"No {componentName} property in {sourceType}");
            }

            var propertyAccess = Expression.Property(source, propertyInfo);
            var accessCondition = Expression.NotEqual(source, Expression.Constant(null));

            if (pathComponents.Count == 0)
            {
                var resultProperty = Expression.Parameter(typeof(TProperty), componentName);
                var resultAssign =
                    Expression.Assign(resultProperty, Expression.TypeAs(propertyAccess, typeof(TProperty)));

                variables.Add(resultProperty);

                return Expression.IfThen(accessCondition, resultAssign);
            }

            var propertyVar = Expression.Parameter(propertyInfo.PropertyType, componentName);
            var propertyAssign = Expression.Assign(propertyVar, propertyAccess);

            variables.Add(propertyVar);

            var nestedConditionalQuery =
                ConstructCondition<TProperty>(pathComponents, propertyVar, propertyInfo.PropertyType, variables);

            return Expression.IfThen(accessCondition, Expression.Block(propertyAssign, nestedConditionalQuery));
        }
    }
}