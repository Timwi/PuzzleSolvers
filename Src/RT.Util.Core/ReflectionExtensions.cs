using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RT.Util.ExtensionMethods
{
    /// <summary>Provides extension methods on types involved in the Reflection API.</summary>
#if EXPORT_UTIL
    public
#endif
    static class ReflectionExtensions
    {
        /// <summary>
        ///     Determines whether the current type is, derives from, or implements the specified generic type, and determines
        ///     that type’s generic type parameters.</summary>
        /// <param name="type">
        ///     The current type.</param>
        /// <param name="typeToFind">
        ///     A generic type definition for a base type of interface, e.g. <c>typeof(ICollection&lt;&gt;)</c> or
        ///     <c>typeof(IDictionary&lt;,&gt;)</c>.</param>
        /// <param name="typeParameters">
        ///     Receives an array containing the generic type parameters of the generic type.</param>
        /// <returns>
        ///     <c>true</c> if the current type is, derives from or implements the specified generic type.</returns>
        public static bool TryGetGenericParameters(this Type type, Type typeToFind, out Type[] typeParameters)
        {
            typeParameters = null;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeToFind)
            {
                typeParameters = type.GetGenericArguments();
                return true;
            }

            if (typeToFind.IsInterface)
            {
                var implements = type.FindInterfaces((ty, obj) => ty.IsGenericType && ty.GetGenericTypeDefinition() == typeToFind, null).FirstOrDefault();
                if (implements == null)
                    return false;

                typeParameters = implements.GetGenericArguments();
                return true;
            }

            foreach (var baseType in type.SelectChain(t => t.BaseType))
            {
                if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeToFind)
                {
                    typeParameters = baseType.GetGenericArguments();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Returns all fields contained in the specified type, including private fields inherited from base classes.</summary>
        /// <param name="type">
        ///     The type to return all fields of.</param>
        /// <returns>
        ///     An <see cref="IEnumerable&lt;FieldInfo&gt;"/> containing all fields contained in this type, including private
        ///     fields inherited from base classes.</returns>
        public static IEnumerable<FieldInfo> GetAllFields(this Type type)
        {
            IEnumerable<FieldInfo> fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var baseType = type.BaseType;
            return (baseType == null) ? fields : GetAllFields(baseType).Concat(fields);
        }

        /// <summary>
        ///     Returns all properties contained in the specified type, including private properties inherited from base
        ///     classes.</summary>
        /// <param name="type">
        ///     The type to return all properties of.</param>
        /// <returns>
        ///     An <see cref="IEnumerable&lt;PropertyInfo&gt;"/> containing all properties contained in this type, including
        ///     private properties inherited from base classes.</returns>
        public static IEnumerable<PropertyInfo> GetAllProperties(this Type type)
        {
            IEnumerable<PropertyInfo> properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var baseType = type.BaseType;
            return (baseType == null) ? properties : GetAllProperties(baseType).Concat(properties);
        }

        /// <summary>
        ///     Indicates whether one or more instance of the specified attribute type is applied to this member.</summary>
        /// <typeparam name="T">
        ///     The type of attribute to search for.</typeparam>
        /// <param name="member">
        ///     Member whose custom attributes to search.</param>
        /// <param name="inherit">
        ///     Specifies whether to search this member's inheritance chain to find the attributes.</param>
        public static bool IsDefined<T>(this MemberInfo member, bool inherit = false)
        {
            return member.IsDefined(typeof(T), inherit);
        }

        /// <summary>
        ///     Indicates whether one or more instance of the specified attribute type is applied to this parameter.</summary>
        /// <typeparam name="T">
        ///     The type of attribute to search for.</typeparam>
        /// <param name="parameter">
        ///     Parameter whose custom attributes to search.</param>
        public static bool IsDefined<T>(this ParameterInfo parameter)
        {
            return parameter.IsDefined(typeof(T), false /* This argument is ignored */);
        }

        /// <summary>Determines whether a property has a public getter.</summary>
        public static bool HasPublicGetter(this PropertyInfo Property)
        {
            var meth = Property.GetGetMethod();
            return meth != null && meth.IsPublic;
        }

        /// <summary>Determines whether a property is static.</summary>
        public static bool IsStatic(this PropertyInfo Property)
        {
            var meth = Property.GetGetMethod();
            return meth == null ? Property.GetSetMethod().IsStatic : meth.IsStatic;
        }

        /// <summary>
        ///     Returns the equivalent of <c>default(T)</c> for a <c>Type</c> object. For reference or nullable types, this is
        ///     <c>null</c>, while for value types, it is the default value (e.g. <c>false</c>, <c>0</c>, etc.).</summary>
        /// <param name="type">
        ///     The type to retrieve the default value for.</param>
        public static object GetDefaultValue(this Type type)
        {
            if (!type.IsValueType)
                return null;
            // This works for nullables too
            return Activator.CreateInstance(type);
        }
    }
}
