using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FieldNullability
{

    public class DelegateFactory
    {
        public static Func<ObjectT, ResultT> createSafeGetterDelegate<ObjectT, ResultT>(List<string> fieldNames)
        {
            var expressions = new List<Expression>();

            var obj = Expression.Parameter(typeof(ObjectT), "object");
            var result = Expression.Variable(typeof(ResultT), "result"); 
            
            expressions.Add(result);
            expressions.Add(Expression.Assign(result, Expression.Constant(null, typeof(ResultT))));

            var memberExpressions = new List<MemberExpression>();

            Expression currentObj = obj;
            for (int i = 0; i < fieldNames.Count; i++)
            {
                var fieldName = fieldNames[i];
                var accessField = Expression.Field(currentObj, fieldName);
                currentObj = accessField;

                memberExpressions.Add(accessField);                
            }

            var condition = Expression.NotEqual(obj, Expression.Constant(null));


            for (int i = 0; i < memberExpressions.Count - 1; i++) 
            {
                var o = memberExpressions[i];
                var memberType = o.Type;
                condition = Expression.AndAlso(condition, Expression.NotEqual(o, Expression.Constant(null)));
            }

            expressions.Add(
                Expression.IfThen(
                    condition,
                    Expression.Assign(result, Expression.TypeAs(memberExpressions[memberExpressions.Count - 1], typeof(ResultT)))
                )
            ); 

            expressions.Add(result);

            var localVariables = new List<ParameterExpression>() { result };
            var lambdaBody = Expression.Block(localVariables, expressions);
            var lambda = Expression.Lambda<Func<ObjectT, ResultT>>(lambdaBody, obj);

            return lambda.Compile();
        }
    }

    class Program
    {
        public class A 
        {
            public B B;

            public A(B B)
            {
                this.B = B;
            }
        } 

        public class B 
        {
            public int C;

            public B(int C)
            {
                this.C = C;
            }
        }

        static void Main(string[] args)
        {
            // A.B
            var fieldPath = new List<string>{ "B", "C" };
            var safeFieldGetter = DelegateFactory.createSafeGetterDelegate<A, int?>(fieldPath);
            var b = new B(2);
            var a = new A(b);
            var result = safeFieldGetter.Invoke(a);
            if (result == null)
                Console.WriteLine("Field is null");
            else Console.WriteLine($"Field: {result}");
        }
    }
}
