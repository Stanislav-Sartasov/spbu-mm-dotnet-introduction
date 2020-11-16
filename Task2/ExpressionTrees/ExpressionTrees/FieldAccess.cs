using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpressionTrees
{
    public class FieldAccess<TObject, TField>
    {
        private readonly string[] _path;
        private readonly Func<TObject, TField> _accessFunc;
            
        public string[] FieldPath => _path;
        public Func<TObject, TField> AccessFunc => _accessFunc;

        public FieldAccess(string[] path)
        {
            if (path.Length <= 0)
                throw new Exception($"Passed empty property path for {typeof(TObject)}:{typeof(TField)}");
            
            _path = path;
            _accessFunc = Generate();
        }

        public TField Access(TObject source)
        {
            return _accessFunc(source);
        }

        private Func<TObject, TField> Generate()
        {
            var assignments = new List<Expression>();
            var sources = new List<Expression>();
            var sourceTypes = new List<Type>();
            var localVars = new List<ParameterExpression>();
            
            var inputParam = Expression.Parameter(typeof(TObject), "subject");
            
            var currentSourceType = typeof(TObject);
            var currentSource = inputParam;
            
            foreach (var fieldName in _path)
            {
                var fieldInfo = currentSourceType.GetField(fieldName);
                
                if (fieldInfo is null)
                    throw new Exception($"No such field {fieldName} in the object {currentSourceType}");

                var access = Expression.Field(currentSource, fieldName);
                var target = Expression.Parameter(fieldInfo.FieldType, fieldName);
                var assign = Expression.Assign(target, access);
                
                sources.Add(currentSource);
                sourceTypes.Add(currentSourceType);
                assignments.Add(assign);

                currentSource = target;
                currentSourceType = fieldInfo.FieldType;
                
                localVars.Add(target);
            }

            var result = currentSource;
            var nullConst = Expression.Constant(null);
            var body = (Expression) Expression.Empty();
            
            for (var i = sources.Count - 1; i >= 0; i--)
            {
                var source = sources[i];
                var sourceType = sourceTypes[i];
                var assign = assignments[i];
                

                bool IsNullable(Type type) => !type.IsValueType;
                
                if (IsNullable(sourceType))
                    body = Expression.IfThen(Expression.NotEqual(source, nullConst), Expression.Block(assign, body));
                else
                    body = Expression.Block(assign, body);
            }

            body = Expression.Block(localVars, body, result);

            var fun = Expression.Lambda<Func<TObject, TField>>(body, inputParam).Compile();
            return Expression.Lambda<Func<TObject,TField>>(body, inputParam).Compile();
        }
    }
}