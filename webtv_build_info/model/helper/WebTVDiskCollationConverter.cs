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
using System.IO;

namespace webtv_build_info
{
    #region Disk collation type list
    /// <summary>
    /// Specifies the collation (byte-swap) type.  
    /// Depending on the processor mode and disk controller read/write method, data on the hard drive isn't always ready to be handled directly on a PC.
    /// So this helps us tag the data so it can be converted properly.
    /// </summary>
    public enum DiskByteTransform
    {
        // No conversion is needed to read on a PC.  Read and write as-is.
        NOSWAP = 0,

        // 16-bit little endien
        // Older WebTV boxes. [1234=>2143]
        BIT16SWAP = 1,

        // 16-bit little endien step with 32-bit little endien step
        // UltimateTV boxes (possibly other WinCE-based boxes). [1234=>3412]
        BIT1632SWAP = 3,

        // 32-bit little endien
        // No boxes known but support is here. [1234=>4321]
        BIT32SWAP = 2
    };
    #endregion

    /// <summary>
    /// Helps us identify and convert the disk collation (byte-swap) type.
    /// </summary>
    public class WebTVDiskCollationConverter
    {
        #region Fields
        public DiskByteTransform byte_transform = DiskByteTransform.NOSWAP;
        #endregion

        #region Detection methods
        /// <summary>
        /// Identify which disk collation (byte-swap) type based on a known build header format.
        /// We're using the jump instruction that should always be the first byte in a hard-drive based build.
        /// </summary>
        /// <param name="reader">The data stream to read the build from.</param>
        /// <param name="build_offset">The offset in the data stream that the build starts.</param>
        public DiskByteTransform detect_build_byte_transform(Stream reader, uint build_offset = 0x880600)
        {
            try
            {
                byte[] build_test_data = new byte[4];

                reader.Seek(build_offset, SeekOrigin.Begin);
                reader.Read(build_test_data, 0, 4);

                // This tests to see if there's a WebTV build.
                // WebTV builds always start with a jump instruction (0x10).
                // We test using 3 different disk collations.
                if (build_test_data[0] == 0x10 // (US)
                || build_test_data[0] == 0xA8) // (possible JP)
                {
                    return DiskByteTransform.NOSWAP;
                }
                else if (build_test_data[1] == 0x10  // (US 16-bit swap)
                        || build_test_data[1] == 0xA8) // (possible JP 16-bit swap)
                {
                    return DiskByteTransform.BIT16SWAP;
                }
                else if (build_test_data[2] == 0x10  // (US 16+32-bit swap)
                        || build_test_data[2] == 0xA8) // (possible JP 16+32-bit swap)
                {
                    return DiskByteTransform.BIT1632SWAP;
                }
                else
                {
                    return DiskByteTransform.NOSWAP;
                }
            }
            catch
            {
                return DiskByteTransform.NOSWAP;
            }
        }
        #endregion

        #region Conversion methods
        /// <summary>
        /// Convert a 4-byte data array from one disk collation type to another.
        /// </summary>
        /// <param name="from_transform">The disk collation type of from_data.</param>
        /// <param name="from_data">The data to convert.</param>
        /// <param name="offset">The offset in from_data to start converting (we will only convert 4 bytes after this offset).</param>
        /// <param name="to_transform">The disk collation type to convert from_data to.</param>
        private void convert_byte_group(DiskByteTransform from_transform, ref byte[] from_data, ulong offset, DiskByteTransform to_transform)
        {
            byte byte1 = from_data[offset];
            byte byte2 = from_data[offset + 1];
            byte byte3 = from_data[offset + 2];
            byte byte4 = from_data[offset + 3];

            if ((from_transform == DiskByteTransform.NOSWAP && to_transform == DiskByteTransform.BIT16SWAP)
            || (from_transform == DiskByteTransform.BIT16SWAP && to_transform == DiskByteTransform.NOSWAP)
            || (from_transform == DiskByteTransform.BIT32SWAP && to_transform == DiskByteTransform.BIT1632SWAP)
            || (from_transform == DiskByteTransform.BIT1632SWAP && to_transform == DiskByteTransform.BIT32SWAP))
            {
                from_data[offset] = byte2;
                from_data[offset + 1] = byte1;
                from_data[offset + 2] = byte4;
                from_data[offset + 3] = byte3;
            }
            else if ((from_transform == DiskByteTransform.NOSWAP && to_transform == DiskByteTransform.BIT1632SWAP)
                || (from_transform == DiskByteTransform.BIT16SWAP && to_transform == DiskByteTransform.BIT32SWAP)
                || (from_transform == DiskByteTransform.BIT32SWAP && to_transform == DiskByteTransform.BIT16SWAP)
                || (from_transform == DiskByteTransform.BIT1632SWAP && to_transform == DiskByteTransform.NOSWAP))
            {
                from_data[offset] = byte3;
                from_data[offset + 1] = byte4;
                from_data[offset + 2] = byte1;
                from_data[offset + 3] = byte2;
            }
            else if ((from_transform == DiskByteTransform.NOSWAP && to_transform == DiskByteTransform.BIT32SWAP)
                || (from_transform == DiskByteTransform.BIT32SWAP && to_transform == DiskByteTransform.NOSWAP)
                || (from_transform == DiskByteTransform.BIT16SWAP && to_transform == DiskByteTransform.BIT1632SWAP)
                || (from_transform == DiskByteTransform.BIT1632SWAP && to_transform == DiskByteTransform.BIT16SWAP))
            {
                from_data[offset] = byte4;
                from_data[offset + 1] = byte3;
                from_data[offset + 2] = byte2;
                from_data[offset + 3] = byte1;
            }
        }

        /// <summary>
        /// Convert data from one disk collation type to another.  This uses the stored collation type as the from_transform.
        /// </summary>
        /// <param name="from_data">The data to convert.</param>
        /// <param name="offset">The offset in from_data to start converting.</param>
        /// <param name="size">The length of data from offset to convert in from_data.  Must be divisible by 4.</param>
        /// <param name="to_transform">The disk collation type to convert from_data to.</param>
        public void convert_bytes(ref byte[] from_data, ulong offset, uint size, DiskByteTransform to_transform = DiskByteTransform.NOSWAP)
        {
            this.convert_bytes(this.byte_transform, ref from_data, offset, size, to_transform);
        }

        /// <summary>
        /// Convert data from one disk collation type to another.
        /// </summary>
        /// <param name="from_transform">The disk collation type of from_data.</param>
        /// <param name="from_data">The data to convert.</param>
        /// <param name="offset">The offset in from_data to start converting.</param>
        /// <param name="size">The length of data from offset to convert in from_data.  Must be divisible by 4.</param>
        /// <param name="to_transform">The disk collation type to convert from_data to.</param>
        public void convert_bytes(DiskByteTransform from_transform, ref byte[] from_data, ulong offset, uint size, DiskByteTransform to_transform = DiskByteTransform.NOSWAP)
        {
            if ((size % 4) > 0)
            {
                throw new DataMisalignedException("Convert buffer length must be divisible by 4.");
            }

            for (ulong i = offset; i < size; i += 4)
            {
                this.convert_byte_group(from_transform, ref from_data, i, to_transform);
            }
        }
        #endregion
    }
}
