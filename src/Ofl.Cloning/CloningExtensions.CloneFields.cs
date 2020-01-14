using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Ofl.Linq;

namespace Ofl.Cloning
{
    public static partial class CloningExtensions
    {
        public static T CloneFields<T>(this T from)
            where T : notnull, new() =>
            from.CloneFields(new T());

        public static T CloneFields<T>(this T from, T to)
            where T : notnull
        {
            // Validate parameters.
            if (from == null) throw new ArgumentNullException(nameof(from));
            if (to == null) throw new ArgumentNullException(nameof(to));

            // Add or create the copier.
            Action<object, object> copier = GetOrAddCopier(typeof(T));

            // Copy.
            copier(from, to);

            // Return to.
            return to;
        }

        private static Action<object, object> GetOrAddCopier(Type key)
        {
            // Validate parameters.
            if (key == null) throw new ArgumentNullException(nameof(key));

            // Lock on the copiers.
            lock (CopyFieldCopiers)
            {
                // Try and get the value.
                if (CopyFieldCopiers.TryGetValue(key, out var copier)) return copier;

                // Set the copier.
                copier = CreateCopier(key);

                // Add it.
                CopyFieldCopiers.Add(key, copier);

                // Return the copier.
                return copier;
            }
        }

        private static readonly IDictionary<Type, Action<object, object>> CopyFieldCopiers = new Dictionary<Type, Action<object, object>>();

        private static Action<object, object> CreateCopier(Type type)
        {
            // Validate parameters.
            Debug.Assert(type != null);

            // The parameter expressions.
            ParameterExpression fromParameterExpression = Expression.Parameter(typeof(object), "from");
            ParameterExpression toParameterExpression = Expression.Parameter(typeof(object), "to");

            // Typed parameter expressions.
            ParameterExpression typedFromParameterExpression = Expression.Variable(type, "f");
            ParameterExpression typedToParameterExpression = Expression.Variable(type, "t");

            // The block expression.
            var block = new List<Expression> {
                // Add the assignment to the variable.
                Expression.Assign(typedFromParameterExpression, Expression.Convert(fromParameterExpression, type)),
                Expression.Assign(typedToParameterExpression, Expression.Convert(toParameterExpression, type))
            };

            // Create the conversions that assign the conversion.
            block.AddRange(
                // NOTE: Used to be
                // from f in type.GetTypeInfo().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                from f in type.GetTypeInfo().DeclaredFields.Where(f => !f.IsStatic)
                select Expression.Assign(Expression.Field(typedToParameterExpression, f), Expression.Field(typedFromParameterExpression, f))
            );

            // Cycle through the p
            return Expression.Lambda<Action<object, object>>(Expression.Block(
                EnumerableExtensions.From(typedFromParameterExpression).Append(typedToParameterExpression),
                block), fromParameterExpression, toParameterExpression).Compile();
        }
    }
}
