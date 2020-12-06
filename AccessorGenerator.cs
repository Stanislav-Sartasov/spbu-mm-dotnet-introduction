using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Generator
{
    public static class AccessorGenerator
    {
        public static Func<TObject, TProperty> GenerateAccessor<TObject, TProperty>(string pathToProperty)
        {
            string[] properties = pathToProperty.Split('.');

            LambdaExpression accessor = GenerateAccesorRecursive<TProperty>(typeof(TObject), properties.Skip(1));
            return Expression.Lambda<Func<TObject, TProperty>>(accessor.Body, accessor.Parameters).Compile();
        }

        private static LambdaExpression GenerateAccesorRecursive<TProperty>(Type targetType, IEnumerable<string> propertyNames)
        {
            if (propertyNames.Count() == 0)
            {
                throw new ArgumentException("Path must contain at least one property");
            }

            ParameterExpression parameter = Expression.Parameter(targetType, targetType.Name);
            PropertyInfo currentProperty = targetType.GetProperty(propertyNames.First()) 
                ?? throw new ArgumentException($"Property {propertyNames.First()} was not found in type {targetType.Name}");

            if (propertyNames.Count() == 1)
            {
                return Expression.Lambda(Expression.Property(parameter, currentProperty), parameter);
            }

            ParameterExpression propertyContent = Expression.Parameter(currentProperty.PropertyType, propertyNames.First());
            ParameterExpression result = Expression.Parameter(typeof(TProperty), propertyNames.Last());

            LambdaExpression accessor =
                Expression.Lambda(
                    Expression.Block(
                        new ParameterExpression[] { propertyContent, result },
                        Expression.Assign(result, Expression.Constant(null, typeof(TProperty))),
                        Expression.Assign(propertyContent, Expression.Property(parameter, currentProperty)),
                        Expression.IfThen(
                            Expression.NotEqual(propertyContent, Expression.Constant(null, currentProperty.PropertyType)),
                            Expression.Assign(result, Expression.Invoke(
                                GenerateAccesorRecursive<TProperty>(currentProperty.PropertyType, propertyNames.Skip(1)), propertyContent))),
                        result), parameter);

            return accessor;
        }
    }
}
