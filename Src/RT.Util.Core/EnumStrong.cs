using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RT.Util
{
    /// <summary>
    /// Provides generic versions of some of the static methods of the <see cref="Enum"/> class.
    /// </summary>
    public static class EnumStrong
    {
        /// <summary>Returns the enum value corresponding to the specified string.</summary>
        /// <typeparam name="T">The enum type from which to retrieve the value.</typeparam>
        /// <param name="value">The string value for which to return the corresponding enum value.</param>
        public static T Parse<T>(string value) where T : struct
        {
            return (T) Enum.Parse(typeof(T), value);
        }

        /// <summary>Returns the enum value corresponding to the specified string.</summary>
        /// <typeparam name="T">The enum type from which to retrieve the value.</typeparam>
        /// <param name="value">A string containing the name or value to convert.</param>
        /// <param name="ignoreCase">If true, ignore case; otherwise, regard case.</param>
        public static T Parse<T>(string value, bool ignoreCase) where T : struct
        {
            return (T) Enum.Parse(typeof(T), value, ignoreCase);
        }

        /// <summary>Finds the enum value corresponding to the specified string.</summary>
        /// <typeparam name="T">The enum type from which to retrieve the value.</typeparam>
        /// <param name="value">A string containing the name or value to convert.</param>
        /// <param name="result">Variable receiving the converted value.</param>
        /// <returns>True if the value was successfully converted; false otherwise.</returns>
        public static bool TryParse<T>(string value, out T result) where T : struct
        {
            try
            {
                result = (T) Enum.Parse(typeof(T), value);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        /// <summary>Finds the enum value corresponding to the specified string.</summary>
        /// <typeparam name="T">The enum type from which to retrieve the value.</typeparam>
        /// <param name="value">A string containing the name or value to convert.</param>
        /// <param name="result">Variable receiving the converted value.</param>
        /// <param name="ignoreCase">If true, ignore case; otherwise, regard case.</param>
        /// <returns>True if the value was successfully converted; false otherwise.</returns>
        public static bool TryParse<T>(string value, out T result, bool ignoreCase) where T : struct
        {
            try
            {
                result = (T) Enum.Parse(typeof(T), value, ignoreCase);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        /// <summary>Finds the enum value corresponding to the specified string.</summary>
        /// <typeparam name="T">The enum type from which to retrieve the value.</typeparam>
        /// <param name="value">A string containing the name or value to convert.</param>
        /// <returns>The value if it was successfully converted; null otherwise.</returns>
        public static T? TryParse<T>(string value) where T : struct
        {
            T result;
            return EnumStrong.TryParse<T>(value, out result) ? result : (T?) null;
        }

        /// <summary>Finds the enum value corresponding to the specified string.</summary>
        /// <typeparam name="T">The enum type from which to retrieve the value.</typeparam>
        /// <param name="value">A string containing the name or value to convert.</param>
        /// <param name="ignoreCase">If true, ignore case; otherwise, regard case.</param>
        /// <returns>The value if it was successfully converted; null otherwise.</returns>
        public static T? TryParse<T>(string value, bool ignoreCase) where T : struct
        {
            T result;
            return EnumStrong.TryParse<T>(value, out result, ignoreCase) ? result : (T?) null;
        }

        /// <summary>Returns the set of enum values from the specified enum type.</summary>
        /// <typeparam name="T">The enum type from which to retrieve the values.</typeparam>
        /// <returns>A strongly-typed array containing the enum values from the specified type.</returns>
        public static T[] GetValues<T>()
        {
            return (T[]) Enum.GetValues(typeof(T));
        }
    }
}
