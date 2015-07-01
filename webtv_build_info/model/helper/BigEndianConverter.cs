#region Copyright and License Information
/*
 * WebTV (MSNTV) Build Information Viewer
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version
 * 3 of the License, or (at your option) any later version.
 * 
 * Author: Eric MacDonald <ubergeek03@gmail.com>
 * Date: June 30th, 2015
 * 
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace webtv_build_info
{
    /// <summary>
    /// Provides the same functionality as "BitConverter" but returns a byte array in big-endian format.
    /// </summary>
    class BigEndianConverter
    {
        #region Methods that return bytes
        /// <summary>
        /// Returns the specified Boolean value as an array of bytes in big-endian format.
        /// </summary>
        /// <param name="value">A Boolean value.</param>
        /// <returns>An array of bytes with length 1.</returns>
        public static byte[] GetBytes(bool value)
        {
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        /// Returns the specified Unicode character value as an array of bytes in big-endian format.
        /// </summary>
        /// <param name="value">A character to convert.</param>
        /// <returns>An array of bytes with length 2.</returns>
        public static byte[] GetBytes(char value)
        {
            return BitConverter.GetBytes(value);
        }

        /// <summary>
        /// Returns the specified single-precision floating point value as an array of bytes in big-endian format.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 4.</returns>
        public static byte[] GetBytes(float value)
        {
            return BigEndianConverter.ReorderBytes(BitConverter.GetBytes(value), 0, 4);
        }

        /// <summary>
        /// Returns the specified double-precision floating point value as an array of bytes in big-endian format.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 8.</returns>
        public static byte[] GetBytes(double value)
        {
            return BigEndianConverter.ReorderBytes(BitConverter.GetBytes(value), 0, 8);
        }

        /// <summary>
        /// Returns the specified 16-bit signed integer value as an array of bytes in big-endian format.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 2.</returns>
        public static byte[] GetBytes(short value)
        {
            return BigEndianConverter.ReorderBytes(BitConverter.GetBytes(value), 0, 2);
        }

        /// <summary>
        /// Returns the specified 32-bit signed integer value as an array of bytes in big-endian format.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 4.</returns>
        public static byte[] GetBytes(int value)
        {
            return BigEndianConverter.ReorderBytes(BitConverter.GetBytes(value), 0, 4);
        }

        /// <summary>
        /// Returns the specified 64-bit signed integer value as an array of bytes in big-endian format.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 8.</returns>
        public static byte[] GetBytes(long value)
        {
            return BigEndianConverter.ReorderBytes(BitConverter.GetBytes(value), 0, 8);
        }

        /// <summary>
        /// Returns the specified 16-bit unsigned integer value as an array of bytes in big-endian format.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 2.</returns>
        public static byte[] GetBytes(ushort value)
        {
            return BigEndianConverter.ReorderBytes(BitConverter.GetBytes(value), 0, 2);
        }

        /// <summary>
        /// Returns the specified 32-bit unsigned integer value as an array of bytes in big-endian format.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 4.</returns>
        public static byte[] GetBytes(uint value)
        {
            return BigEndianConverter.ReorderBytes(BitConverter.GetBytes(value), 0, 4);
        }

        /// <summary>
        /// Returns the specified 64-bit unsigned integer value as an array of bytes in big-endian format.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 8.</returns>
        public static byte[] GetBytes(ulong value)
        {
            return BigEndianConverter.ReorderBytes(BitConverter.GetBytes(value), 0, 8);
        }
        #endregion

        #region Methods that process bytes
        /// <summary>
        /// Returns a single-precision floating point number converted from four bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="value">An array of bytes.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns></returns>
        public static float ToSingle(byte[] value, int startIndex)
        {
            return BitConverter.ToSingle(BigEndianConverter.ReorderBytes(value, startIndex, 4), 0);
        }

        /// <summary>
        /// Returns a double-precision floating point number converted from eight bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="value">An array of bytes.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns></returns>
        public static double ToDouble(byte[] value, int startIndex)
        {
            return BitConverter.ToDouble(BigEndianConverter.ReorderBytes(value, startIndex, 8), 0);
        }

        /// <summary>
        /// Returns a 16-bit signed integer converted from two bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="value">An array of bytes.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns></returns>
        public static short ToInt16(byte[] value, int startIndex)
        {
            return BitConverter.ToInt16(BigEndianConverter.ReorderBytes(value, startIndex, 2), 0);
        }

        /// <summary>
        /// Returns a 32-bit signed integer converted from four bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="value">An array of bytes.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns></returns>
        public static int ToInt32(byte[] value, int startIndex)
        {
            return BitConverter.ToInt32(BigEndianConverter.ReorderBytes(value, startIndex, 4), 0);
        }

        /// <summary>
        /// Returns a 64-bit signed integer converted from eight bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="value">An array of bytes.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns></returns>
        public static long ToInt64(byte[] value, int startIndex)
        {
            return BitConverter.ToInt64(BigEndianConverter.ReorderBytes(value, startIndex, 8), 0);
        }

        /// <summary>
        /// Returns a 16-bit unsigned integer converted from two bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="value">An array of bytes.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns></returns>
        public static ushort ToUInt16(byte[] value, int startIndex)
        {
            return BitConverter.ToUInt16(BigEndianConverter.ReorderBytes(value, startIndex, 2), 0);
        }

        /// <summary>
        /// Returns a 32-bit unsigned integer converted from four bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="value">An array of bytes.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns></returns>
        public static uint ToUInt32(byte[] value, int startIndex)
        {
            return BitConverter.ToUInt32(BigEndianConverter.ReorderBytes(value, startIndex, 4), 0);
        }

        /// <summary>
        /// Returns a 64-bit unsigned integer converted from eight bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="value">An array of bytes.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns></returns>
        public static ulong ToUInt64(byte[] value, int startIndex)
        {
            return BitConverter.ToUInt64(BigEndianConverter.ReorderBytes(value, startIndex, 8), 0);
        }

        /// <summary>
        /// Takes a little-endian byte array and converts it into a big-endian byte array.  This is necessary so we can use BitConverter which uses little-endian byte arrays.
        /// </summary>
        /// <param name="value">The byte array to convert.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <param name="count">From startIndex, the number of bytes in value to convert.</param>
        private static byte[] ReorderBytes(byte[] value, int startIndex, int count)
        {
            byte[] bytes = new byte[count];

            for (int i = (count - 1); i >= 0; i--)
            {
                bytes[i] = value[startIndex + (count - 1 - i)];
            }

            return bytes;
        }
        #endregion
    }
}
