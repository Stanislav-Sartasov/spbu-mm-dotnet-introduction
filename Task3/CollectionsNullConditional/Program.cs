using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace CollectionsNullConditional
{
    class Program
    {
        static Func<List<Stack<Dictionary<int, int>>>, int?> GetCompiledLambda()
        {
            ParameterExpression coll = Expression.Parameter(typeof(List<Stack<Dictionary<int, int>>>), "coll");

            ConstantExpression listConstIndex = Expression.Constant(0, typeof(int));
            ConstantExpression dictConstKey = Expression.Constant(0, typeof(int));
            ConstantExpression nullConst = Expression.Constant(null, typeof(object));

            ParameterExpression resultVar = Expression.Variable(typeof(int?), "result");

            ParameterExpression stackVar = Expression.Variable(typeof(Stack<Dictionary<int, int>>), "stack");
            ParameterExpression dictVar  = Expression.Variable(typeof(Dictionary<int, int>), "dict");

            PropertyInfo listIndexer = coll.Type.GetProperty("Item");
            PropertyInfo dictIndexer = dictVar.Type.GetProperty("Item");

            // stack = coll[listConstIndex]
            Expression assignStackVar = Expression.Assign( 
                stackVar,
                Expression.Property(coll, listIndexer, listConstIndex)
            );
            // dict = stack.Peek()
            Expression assignDictVar = Expression.Assign(
                dictVar,
                Expression.Call(stackVar, "Peek", null)
            );
            // result = new int?( dict[dictConstKey] )
            Expression assignResultVar = Expression.Assign(
                resultVar,
                Expression.New(
                    typeof(int?).GetConstructor(new[]{ typeof(int) }), 
                    Expression.Property(dictVar, dictIndexer, dictConstKey))
            );

            var lambda = Expression.Lambda<Func<List<Stack<Dictionary<int, int>>>, int?>>
            (
                Expression.Block
                (
                    // constants and variables
                    new ParameterExpression[] 
                    {
                        resultVar, stackVar, dictVar
                    },
                    // if coll is not null
                    Expression.IfThen
                    (
                        Expression.NotEqual(coll, nullConst),
                        Expression.Block
                        (
                            assignStackVar,
                            // if stack is not null
                            Expression.IfThen
                            (
                                Expression.NotEqual(stackVar, nullConst),
                                Expression.Block
                                (
                                    assignDictVar,
                                    // if dict is not null
                                    Expression.IfThen
                                    (
                                        Expression.NotEqual(dictVar, nullConst),
                                        assignResultVar
                                    )
                                )
                            )
                        )
                    ),
                    resultVar
                ),
                new ParameterExpression[]{ coll }
            );

            return lambda.Compile();
        }

        static void Main(string[] args)
        {
            #region default
            Console.WriteLine($"All path components are Generic Collections (option 3).{Environment.NewLine}");

            var collFull = new List<Stack<Dictionary<int, int>>>();
            collFull.Add(new Stack<Dictionary<int, int>>());
            collFull[0].Push(new Dictionary<int, int>());
            collFull[0].Peek().Add(0, 1);

            int? b = collFull?[0]?.Peek()?[0];
            Debug.Assert(b.HasValue && b.Value == 1);
            Console.WriteLine("Collections are not null. Value: " + b.Value);

            List<Stack<Dictionary<int, int>>> collNullList = null;

            var collNullStack = new List<Stack<Dictionary<int, int>>>();
            collNullStack.Add(null);

            var collNullDict = new List<Stack<Dictionary<int, int>>>();
            collNullDict.Add(new Stack<Dictionary<int, int>>());
            collNullDict[0].Push(null);

            bool collNullListHas = (collNullList?[0]?.Peek()?[0]).HasValue;
            bool collNullStackHas = (collNullStack?[0]?.Peek()?[0]).HasValue;
            bool collNullDictHas = (collNullDict?[0]?.Peek()?[0]).HasValue;
            Debug.Assert(!collNullListHas);
            Debug.Assert(!collNullStackHas);
            Debug.Assert(!collNullDictHas);
            Console.WriteLine("List  is null. HasValue: " + collNullListHas);
            Console.WriteLine("Stack is null. HasValue: " + collNullStackHas);
            Console.WriteLine("Dict  is null. HasValue: " + collNullDictHas);
            
            Console.WriteLine();
            #endregion

            var compiled = GetCompiledLambda();

            int? b_l = compiled(collFull);
            Debug.Assert(b_l.HasValue && b_l.Value == 1);
            Console.WriteLine("Expr: Collections are not null. Value: " + b_l.Value);

            bool collNullListHas_l = (collNullList?[0]?.Peek()?[0]).HasValue;
            bool collNullStackHas_l = (collNullStack?[0]?.Peek()?[0]).HasValue;
            bool collNullDictHas_l = (collNullDict?[0]?.Peek()?[0]).HasValue;
            Debug.Assert(!collNullListHas_l);
            Debug.Assert(!collNullStackHas_l);
            Debug.Assert(!collNullDictHas_l);
            Console.WriteLine("Expr: List  is null. HasValue: " + collNullListHas_l);
            Console.WriteLine("Expr: Stack is null. HasValue: " + collNullStackHas_l);
            Console.WriteLine("Expr: Dict  is null. HasValue: " + collNullDictHas_l);
        }
    }
}
