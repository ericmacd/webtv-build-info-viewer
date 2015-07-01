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
using System.Threading.Tasks;

namespace webtv_build_info
{
    /// <summary>
    /// Class that provides utility functions to convert a byte length number into a string.
    /// </summary>
    class BytesToString
    {
        #region Human-readable string methods
        /// <summary>
        /// Converts a byte length number into a human-readable string scaled and prefixed using the IEC standard (powers of 2).
        /// </summary>
        static public String bytes_to_iec(ulong bytes)
        {
            string[] units = { "B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB" };

            if (bytes == 0)
            {
                return "0 " + units[0];
            }
            else
            {
                int unit_index = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
                double resoled_bytes = Math.Round(bytes / Math.Pow(1024, unit_index), 1);
                return resoled_bytes.ToString() + " " + units[unit_index];
            }
        }
        #endregion
    }
}
