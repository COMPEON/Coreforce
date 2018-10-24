using System;
using System.Linq.Expressions;

namespace Compos.Coreforce.Extensions
{
    public static class ExpressionExtension
    {
        public static MemberExpression GetMemberExpression<T>(this Expression<Func<T, object>> expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            var unaryExpression = expression.Body as UnaryExpression;

            if (memberExpression != null)
                return memberExpression;

            return unaryExpression != null ? unaryExpression.Operand as MemberExpression : null;
        }
    }
}
