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
    /// WebTV build information.  Use WebTVBuildInfo to parse a build image into a WebTVBuild object.
    /// </summary>
    class WebTVBuild
    {
        #region Fields
        public uint jump_instruction { get; set; }
        public uint pre_jump_instruction { get; set; }
        public uint jump_offset { get; set; }
        public uint code_checksum { get; set; }
        public uint dword_length { get; set; }
        public uint dword_code_length { get; set; }
        public uint build_number { get; set; }
        public uint build_flags { get; set; }
        public uint build_base_address { get; set; }
        public uint romfs_base_address { get; set; }
        public uint romfs_checksum { get; set; }
        public uint dword_romfs_size { get; set; }
        public uint heap_data_address { get; set; }
        public uint dword_heap_data_size { get; set; }
        public uint dword_heap_free_size { get; set; }
        public uint dword_heap_compressed_data_size { get; set; }
        public uint code_compressed_address { get; set; }
        public uint dword_code_compressed_size { get; set; }
        // Original classic builds are very different from later builds, so it's necessary to have this flag.
        public bool is_classic_build { get; set; }
        #endregion
    }
}
