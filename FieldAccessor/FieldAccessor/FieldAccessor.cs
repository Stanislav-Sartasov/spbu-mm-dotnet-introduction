using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FieldAccessor
{
    public class FieldAccessor
    {
        public static Func<Dictionary<int, List<Queue<int>>>, int, int, int?> GetSaveField() {

            var collection = Expression.Parameter(typeof(Dictionary<int, List<Queue<int>>>), "collection");
            var key = Expression.Parameter(typeof(int), "key");
            var listIndex = Expression.Parameter(typeof(int), "index");

            var result = Expression.Variable(typeof(int?), "res");
            
            var trueConst = Expression.Constant(true);

            var list = Expression.Variable(typeof(List<Queue<int>>), "list");
            var queue = Expression.Variable(typeof(Queue<int>), "queue");

            var checkKey = Expression.Call(collection, collection.Type.GetMethod("ContainsKey", new[] { typeof(int)}), key);
            var listSize = Expression.Property(list, list.Type.GetProperty("Count"), null);
            var checkQueue = Expression.Property(queue, queue.Type.GetProperty("Count"), null);

            return Expression.Lambda<Func<Dictionary<int, List<Queue<int>>>, int, int, int?>>(
                Expression.Block(
                new[] { result, list, queue},
                 Expression.IfThenElse(Expression.Equal(checkKey, trueConst),
                    Expression.Block(
                        //get list
                        Expression.Assign(list, Expression.Property(collection, collection.Type.GetProperty("Item"), key)),
                        Expression.IfThenElse(Expression.LessThan(listIndex, listSize),
                            Expression.Block(
                                //get queue
                                Expression.Assign(queue, Expression.Property(list, list.Type.GetProperty("Item"), listIndex)),
                                Expression.IfThenElse(Expression.GreaterThan(checkQueue, Expression.Constant(0)),
                                     Expression.Assign(result, 
                                         Expression.New(
                                             typeof(int?).GetConstructor(new[] { typeof(int) }),
                                             Expression.Call(queue, queue.Type.GetMethod("Dequeue"), null))),
                                     Expression.Assign(result, Expression.Constant(null, typeof(int?))))
                            ),//else (index is invalid)
                            Expression.Assign(result, Expression.Constant(null, typeof(int?))))
                        ),//else (key is invalid)
                    Expression.Assign(result, Expression.Constant(null, typeof(int?)))),
                result
                ),
                new ParameterExpression[] { collection, key, listIndex }
            ).Compile();
        }
    }
}
