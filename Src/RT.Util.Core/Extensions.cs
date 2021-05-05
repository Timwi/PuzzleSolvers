using System;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace RT.Util
{
    /// <summary>Provides a function delegate that accepts only value types as input and return types.</summary>
    public delegate TResult FuncStructStruct<in TInput, TResult>(TInput input)
        where TInput : struct
        where TResult : struct;
    /// <summary>Provides a function delegate that accepts only value types as input and only reference types as return types.</summary>
    public delegate TResult FuncStructClass<in TInput, TResult>(TInput input)
        where TInput : struct
        where TResult : class;
    /// <summary>Provides a function delegate that accepts only reference types as input and only value types as return types.</summary>
    public delegate TResult FuncClassStruct<in TInput, TResult>(TInput input)
        where TInput : class
        where TResult : struct;
    /// <summary>Provides a function delegate that accepts only reference types as input and return types.</summary>
    public delegate TResult FuncClassClass<in TInput, TResult>(TInput input)
        where TInput : class
        where TResult : class;
    /// <summary>Provides a function delegate that accepts only reference types as input.</summary>
    public delegate TResult FuncClass<in TInput, TResult>(TInput input) where TInput : class;
    /// <summary>Provides a function delegate that accepts only value types as input.</summary>
    public delegate TResult FuncStruct<in TInput, TResult>(TInput input) where TInput : struct;

    /// <summary>Provides extension methods that apply to all types.</summary>
    partial class Ut
    {
        /// <summary>Returns null if the input is null, otherwise the result of the specified lambda when applied to the input.</summary>
        /// <typeparam name="TInput">Type of the input value.</typeparam>
        /// <typeparam name="TResult">Type of the result from the lambda.</typeparam>
        /// <param name="input">Input value to check for null.</param>
        /// <param name="lambda">Function to apply the input value to if it is not null.</param>
        public static TResult NullOr<TInput, TResult>(this TInput input, FuncClassClass<TInput, TResult> lambda)
            where TInput : class
            where TResult : class
        {
            return input == null ? null : lambda(input);
        }

        /// <summary>Returns null if the input is null, otherwise the result of the specified lambda when applied to the input.</summary>
        /// <typeparam name="TInput">Type of the input value.</typeparam>
        /// <typeparam name="TResult">Type of the result from the lambda.</typeparam>
        /// <param name="input">Input value to check for null.</param>
        /// <param name="lambda">Function to apply the input value to if it is not null.</param>
        public static TResult? NullOr<TInput, TResult>(this TInput input, FuncClass<TInput, TResult?> lambda)
            where TInput : class
            where TResult : struct
        {
            return input == null ? null : lambda(input);
        }

        /// <summary>Returns null if the input is null, otherwise the result of the specified lambda when applied to the input.</summary>
        /// <typeparam name="TInput">Type of the input value.</typeparam>
        /// <typeparam name="TResult">Type of the result from the lambda.</typeparam>
        /// <param name="input">Input value to check for null.</param>
        /// <param name="lambda">Function to apply the input value to if it is not null.</param>
        public static TResult? NullOr<TInput, TResult>(this TInput input, FuncClassStruct<TInput, TResult> lambda)
            where TInput : class
            where TResult : struct
        {
            return input == null ? null : lambda(input).Nullable();
        }

        /// <summary>Returns null if the input is null, otherwise the result of the specified lambda when applied to the input.</summary>
        /// <typeparam name="TInput">Type of the input value.</typeparam>
        /// <typeparam name="TResult">Type of the result from the lambda.</typeparam>
        /// <param name="input">Input value to check for null.</param>
        /// <param name="lambda">Function to apply the input value to if it is not null.</param>
        public static TResult NullOr<TInput, TResult>(this TInput? input, FuncStructClass<TInput, TResult> lambda)
            where TInput : struct
            where TResult : class
        {
            return input == null ? null : lambda(input.Value);
        }

        /// <summary>Returns null if the input is null, otherwise the result of the specified lambda when applied to the input.</summary>
        /// <typeparam name="TInput">Type of the input value.</typeparam>
        /// <typeparam name="TResult">Type of the result from the lambda.</typeparam>
        /// <param name="input">Input value to check for null.</param>
        /// <param name="lambda">Function to apply the input value to if it is not null.</param>
        public static TResult? NullOr<TInput, TResult>(this TInput? input, FuncStruct<TInput, TResult?> lambda)
            where TInput : struct
            where TResult : struct
        {
            return input == null ? null : lambda(input.Value);
        }

        /// <summary>Returns null if the input is null, otherwise the result of the specified lambda when applied to the input.</summary>
        /// <typeparam name="TInput">Type of the input value.</typeparam>
        /// <typeparam name="TResult">Type of the result from the lambda.</typeparam>
        /// <param name="input">Input value to check for null.</param>
        /// <param name="lambda">Function to apply the input value to if it is not null.</param>
        public static TResult? NullOr<TInput, TResult>(this TInput? input, FuncStructStruct<TInput, TResult> lambda)
            where TInput : struct
            where TResult : struct
        {
            return input == null ? null : lambda(input.Value).Nullable();
        }

        /// <summary>Turns the specified value into a nullable value.</summary>
        /// <typeparam name="TInput">Any non-nullable value type.</typeparam>
        /// <param name="input">Any value.</param>
        /// <returns>The same value cast as nullable.</returns>
        public static TInput? Nullable<TInput>(this TInput input) where TInput : struct
        {
            return (TInput?) input;
        }

        /// <summary>Searches the specified object’s type for a field of the specified name and returns that field’s value.</summary>
        /// <typeparam name="T">Expected type of the field.</typeparam>
        /// <param name="instance">Instance from which to retrieve the field value.</param>
        /// <param name="fieldName">Name of the field to return the value of.</param>
        /// <returns>The value of the field.</returns>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item><description>The field is of a different type than specified.</description></item>
        /// <item><description>There is no field with the specified name.</description></item>
        /// </list>
        /// </exception>
        /// <remarks>This method is intended to be used only for debugging. Do not rely on it in production code.</remarks>
        public static T GetFieldValue<T>(this object instance, string fieldName)
        {
            var field = instance.GetType().GetAllFields().Single(f => f.Name == fieldName);
            if (!typeof(T).IsAssignableFrom(field.FieldType))
                throw new InvalidOperationException("Field is of type {1}, but was expected to be of type {0} (or derived from it).".Fmt(typeof(T).FullName, field.FieldType.FullName));
            return (T) field.GetValue(instance);
        }
    }
}
