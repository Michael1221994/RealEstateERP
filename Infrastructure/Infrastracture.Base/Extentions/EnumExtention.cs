using System;

namespace Infrastracture.Base.Extentions
{

    public static class EnumExtensions
    {
        /// <summary>
        /// Converts a string to a specified enum type.
        /// </summary>
        /// <typeparam name="T">The type of the enum.</typeparam>
        /// <param name="value">The string to convert.</param>
        /// <returns>The enum value corresponding to the string.</returns>
        /// <exception cref="ArgumentException">Thrown if the string is not a valid enum value.</exception>
        public static T ToEnum<T>(this string value) where T : struct, Enum
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(value));
            }

            if (Enum.TryParse<T>(value, true, out var result))
            {
                return result;
            }

            throw new ArgumentException($"'{value}' is not a valid value for enum type {typeof(T).Name}.");
        }
    }
}
