using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Homework
{
    internal static class Program
    {
        public static void Main()
        {
            var a = new A
            {
                B = new B
                {
                    C = new C
                    {
                        Value = "42"
                    }
                }
            };

            var anotherA = new A
            {
                B = new B()
            };

            var access = Create<A, string>(new []{"B", "C", "Value"});

            Console.WriteLine($"The meaning of life, universe and everything is {access(a)}");
            Console.WriteLine($"My good New Year spirit is {access(anotherA) ?? "<null>"}");
        }

        [NotNull]
        private static Func<TRoot, TResult> Create<TRoot, TResult>([NotNull] IEnumerable<string> properties)
        {
            var parameter = Expression.Parameter(typeof(TRoot));
            var exit = Expression.Label(typeof(TResult));

            var locals = new List<ParameterExpression>();
            var expressions = new List<Expression>();

            var currentVariable = parameter;
            var currentType = typeof(TRoot);
            foreach (string propertyName in properties)
            {
                var field = currentType.GetField(propertyName).NotNull();
                var nextVariable = Expression.Variable(field.FieldType);
                locals.Add(nextVariable);
                expressions.Add(Expression.IfThenElse(
                    Expression.NotEqual(currentVariable, Expression.Constant(null)),
                    Expression.Assign(nextVariable, Expression.Field(currentVariable, field)),
                    Expression.Return(exit, Expression.Constant(null, typeof(TResult)))
                ));

                currentVariable = nextVariable;
                currentType = nextVariable.Type;
            }

            expressions.Add(Expression.Return(exit, currentVariable));
            expressions.Add(Expression.Label(exit, Expression.Constant(null, typeof(TResult))));
            return Expression.Lambda<Func<TRoot, TResult>>(Expression.Block(locals, expressions), parameter).Compile();
        }

        private class A
        {
            public B B;
        }

        private class B
        {
            public C C;
        }

        private class C
        {
            public string Value;
        }

        [NotNull]
        public static T NotNull<T>([CanBeNull] this T value) where T : class
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return value;
        }
    }
}
