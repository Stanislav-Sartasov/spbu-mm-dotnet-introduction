using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTrees
{
    public class AccessorGenerator<TGiven, TReaching>
    {
        private struct Accessor
        {
            public readonly string fieldName;
            public readonly int index;

            public Accessor(string fieldName, int index)
            {
                this.fieldName = fieldName;
                this.index = index;
            }
        }
        public Func<TGiven, TReaching> generateAccessor(TGiven instance, string path)
        {
            // Lambda input
            var initial_value = Expression.Parameter(typeof(TGiven));

            //Variables and expressions for lambda block
            var blockExpressions = new List<Expression>();
            var blockVariables = new List<ParameterExpression>();

            var currentInstance = initial_value;
            Type currentClass = typeof(TGiven);

            //Output value
            var result = Expression.Label(typeof(TReaching));
            Expression resultExpression = Expression.Label(result, Expression.Constant(null, typeof(TReaching)));

            //Iterative filling of block variables and expression
            foreach (Accessor part in this.splitPath(path))
            {
                var currentField = currentClass.GetField(part.fieldName);
                if (currentField is null)
                {
                    throw new KeyNotFoundException("Wrong path");
                }

                var currentFieldClass = currentField.FieldType;
                var nextFieldClass = currentFieldClass.GetElementType();

                var BlockVar = Expression.Parameter(nextFieldClass);

                //If current position is null -- return null
                var checkNullExpression = Expression.IfThen(
                Expression.Equal(currentInstance, Expression.Constant(null, currentClass)),
                Expression.Return(result, Expression.Constant(null, typeof(TReaching))));

                blockExpressions.Add(checkNullExpression);

                var GetArrayExpression = Expression.Field(currentInstance, part.fieldName);
                var GetIndexExpression = Expression.Constant(part.index);

                //If index is not in range, return null, otherwise continue building
                var BlockExpr = Expression.IfThenElse(
                                    Expression.GreaterThan(Expression.ArrayLength(GetArrayExpression), GetIndexExpression),
                                    Expression.Assign(BlockVar, Expression.ArrayIndex(GetArrayExpression, GetIndexExpression)),
                                    Expression.Return(result, Expression.Constant(null, typeof(TReaching))));

                blockExpressions.Add(BlockExpr);
                blockVariables.Add(BlockVar);

                currentClass = nextFieldClass;
                currentInstance = BlockVar;

            }

            //Output generation expressions
            var finalExpression = Expression.Return(result, Expression.Convert(currentInstance, typeof(TReaching)));
            blockExpressions.Add(finalExpression);
            blockExpressions.Add(resultExpression);

            return Expression.Lambda<Func<TGiven, TReaching>>(Expression.Block(blockVariables, blockExpressions), initial_value).Compile();
            //Console.WriteLine(i.DynamicInvoke(instance));
        }

        private List<Accessor> splitPath(string path)
        {
            var result = new List<Accessor>();
            foreach (string part in path.Split('.'))
            {
                try
                {
                    var tmp = part.Split('[');
                    string fieldName = tmp[0];
                    int index = Int32.Parse(tmp[1].Replace("]", ""));
                    result.Add(new Accessor(fieldName, index));
                }
                catch (IndexOutOfRangeException)
                {
                    throw new KeyNotFoundException("Wrong path");
                }
            }
            return result;
        }
    }


}
