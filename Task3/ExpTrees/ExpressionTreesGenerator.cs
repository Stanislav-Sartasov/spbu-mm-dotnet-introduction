using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpTrees
{
    public class ExpressionTreesGenerator
    {
        public static Func<List<Queue<SortedList<int, int>>>, int?> GenerateLambda()
        {
            ParameterExpression collection = Expression.Parameter(typeof(List<Queue<SortedList<int, int>>>), "collection");
            ParameterExpression queue = Expression.Variable(typeof(Queue<SortedList<int, int>>), "queue");
            ParameterExpression sortedList = Expression.Variable(typeof(SortedList<int, int>), "sortedList");
            ParameterExpression result = Expression.Variable(typeof(int?), "result");

            ConstantExpression i_list = Expression.Constant(0, typeof(int));
            ConstantExpression k_sortedList = Expression.Constant(0, typeof(int));
            ConstantExpression nullConstant = Expression.Constant(null, typeof(object));

            PropertyInfo listIter = collection.Type.GetProperty("Item");
            PropertyInfo sortedListIter = sortedList.Type.GetProperty("Item");

            Expression getQueue = Expression.Assign(queue, Expression.Property(collection, listIter, i_list));
            Expression getSortedDict = Expression.Assign(sortedList, Expression.Call(queue, "Peek", null));
            Expression getResult = Expression.Assign(result, Expression.New(typeof(int?).GetConstructor(new[] { typeof(int) }),
                                                             Expression.Property(sortedList, sortedListIter, k_sortedList)));

            var lambda = Expression.Lambda<Func<List<Queue<SortedList<int, int>>>, int?>>(
                Expression.Block(
                    new ParameterExpression[]
                    {
                        result,
                        queue,
                        sortedList,
                    },
                    Expression.IfThen(
                        Expression.NotEqual(collection, nullConstant),
                        Expression.Block(
                            getQueue,
                            Expression.IfThen(
                                Expression.NotEqual(queue, nullConstant),
                                Expression.Block(
                                    getSortedDict,
                                    Expression.IfThen(
                                        Expression.NotEqual(sortedList, nullConstant),
                                        getResult
                                    )
                                )
                            )
                        )
                    ),
                    result
                ),
                new ParameterExpression[] { collection }
            );
            return lambda.Compile();

        }
    }
}
