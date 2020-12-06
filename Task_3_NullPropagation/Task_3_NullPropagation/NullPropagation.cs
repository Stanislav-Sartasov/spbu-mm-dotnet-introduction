using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NullPropagating
{
public static class NullPropagation
{
    public static Func<TSrc, TDst> Wrap<TSrc, TDst>(string path)
    {
        if (path.Trim().Length == 0)
            throw new ArgumentException($"Given path \"{path}\" is blank");
        
        var srcParam = Expression.Parameter(typeof(TSrc));
        var returnTarget = Expression.Label(typeof(TDst));

        var currVar = srcParam;
        var currType = typeof(TSrc);
        var vars = new List<ParameterExpression>();
        var exprs = new List<Expression>();
        foreach (var fieldName in path.Split('.'))
        {
            var field = currType.GetField(fieldName) ??
                        throw new ArgumentException($"{currType} does not contain field \"{fieldName}\"");
            var nextVar = Expression.Variable(field.FieldType);

            // var t_i;
            // if (t_{i-1} != null)
            //   t_i = t_{i-1}.f_{i+1};
            // else
            //   return null;
            vars.Add(nextVar);
            exprs.Add(Expression.IfThenElse(
                Expression.NotEqual(currVar, Expression.Constant(null)),
                Expression.Assign(nextVar, Expression.Field(currVar, field)),
                Expression.Return(returnTarget, Expression.Constant(null, typeof(TDst)))
            ));

            currVar = nextVar;
            currType = field.FieldType;
        }

        exprs.Add(Expression.Return(returnTarget, currVar));

        if (!typeof(TDst).IsAssignableFrom(currType))
            throw new ArgumentException(
                $"Specified return type {typeof(TDst)} is not a supertype of actual return type ${currType}");

        exprs.Add(Expression.Label(returnTarget, Expression.Constant(null, typeof(TDst))));
        return Expression.Lambda<Func<TSrc, TDst>>(Expression.Block(vars, exprs), srcParam).Compile();
    }
}
}