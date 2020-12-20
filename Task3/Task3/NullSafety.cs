using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NullSafety
{
    public static class NullSafety
    {
        public static Func<TObject, TProperty> SafeGetProperty<TObject, TProperty>(List<string> propertyChain)
        {
            if (propertyChain.Count() <= 1)
            {
                throw new ArgumentException("Property chain argument must contain at least two item");
            }
            
            return (Func<TObject, TProperty>)SafeGetPropertyHelper<TProperty>(typeof(TObject), propertyChain.Skip(1)).Compile();
        }

        private static LambdaExpression SafeGetPropertyHelper<TProperty>(Type type, IEnumerable<string> propertyChain)
        {
            PropertyInfo propertyInfo = type.GetProperty(
                propertyChain.First(),
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static
            );

            if (propertyInfo == null)
            {
                throw new ArgumentException($"Type {type.Name} does not have public property {propertyChain.First()}");
            }

            ParameterExpression currentTypeVar = Expression.Parameter(type, type.Name);

            if (propertyChain.Count() == 1)
            {
                return Expression.Lambda(Expression.Property(currentTypeVar, propertyInfo), currentTypeVar);
            }

            ParameterExpression lastPropertyVar = Expression.Parameter(typeof(TProperty), propertyChain.Last());
            ParameterExpression currentPropertyVar = Expression.Parameter(propertyInfo.PropertyType, propertyChain.First());

            Console.WriteLine(currentPropertyVar.Type);
            Console.WriteLine(lastPropertyVar.Type);

            BlockExpression currentExpressionBlock = Expression.Block(
                new ParameterExpression[] { lastPropertyVar, currentPropertyVar },
                Expression.Assign(currentPropertyVar, Expression.Property(currentTypeVar, propertyInfo)),
                Expression.Assign(lastPropertyVar, Expression.Constant(null, typeof(TProperty))),
                Expression.IfThen(
                    Expression.NotEqual(currentPropertyVar, Expression.Constant(null, propertyInfo.PropertyType)),
                    Expression.Assign(
                        lastPropertyVar, 
                        Expression.Invoke(
                            SafeGetPropertyHelper<TProperty>(propertyInfo.PropertyType, propertyChain.Skip(1)), currentPropertyVar)
                        )
                    ),
                    lastPropertyVar
                );

            return Expression.Lambda(currentExpressionBlock, currentTypeVar);
        }
    }
}
