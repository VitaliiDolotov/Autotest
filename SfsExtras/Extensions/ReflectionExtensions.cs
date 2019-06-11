using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SfsExtras.Extensions
{
    public static class ReflectionExtensions
    {
        public static T GetFirstDecoration<T>(this ICustomAttributeProvider attributeProvider) where T : Attribute
        {
            object[] attributes = attributeProvider.GetCustomAttributes(typeof(T), true);
            return (T)attributes.FirstOrDefault();
        }

        public static MemberInfo ResolveMember<TModel, TProperty>(TModel model, Expression<Func<TModel, TProperty>> expression)
        {
            if (!(expression.Body is MemberExpression))
                throw new ArgumentException(
                    "Expression passed to this method should be of type MemberExpression, for example: c => c.Property");

            var memberExpression = (MemberExpression)expression.Body;
            return memberExpression.Member;
        }
    }
}
