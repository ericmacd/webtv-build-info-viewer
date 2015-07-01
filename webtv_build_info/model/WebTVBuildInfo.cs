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
using System.Runtime.InteropServices;

namespace webtv_build_info
{
    #region Build Data Structures
    /// <summary>
    /// ROMFS footer structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = WebTVBuildInfo.ROMFS_FOOTER_SIZE), Serializable]
    public unsafe struct webtv_romfs_footer
    {
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] dword_romfs_size;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] romfs_checksum;
    }

    /// <summary>
    /// Build header structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = WebTVBuildInfo.BUILD_HEADER_SIZE), Serializable]
    public unsafe struct webtv_build_header
    {
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] jump_instruction;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] pre_jump_instruction;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] code_checksum;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] dword_length;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] dword_code_length;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] build_number;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] heap_data_address;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] dword_heap_data_size;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] dword_heap_free_size;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] romfs_base_address;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] unknown1;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] unknown2;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] build_base_address;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] build_flags;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] dword_heap_compressed_data_size;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] code_compressed_address;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] dword_code_compressed_size;
    }
    #endregion

    /// <summary>
    /// Parses a builds header and ROMFS footer from a file stream and constructs a WebTVBuild class.
    /// </summary>
    class WebTVBuildInfo
    {
        #region Constants
        public const int BUILD_HEADER_SIZE = 0x44;
        public const int ROMFS_FOOTER_SIZE = 0x8;
        public const uint NO_ROMFS = 0x4E6F4653; // "NoFS"
        public const int DWORD_SIZE_BYTES = 0x4;

        // The entire WebTV build must fit into the boot partition.
        // 0x2000000 is the largest boot partition I've seen.
        public const uint MAX_BUILD_SIZE = 0x2000000;
        #endregion

        #region Fields
        public Stream reader;
        public WebTVDiskCollationConverter byte_converter = null;

        // Caged reader is used when reading from a disk.  
        // It's when we lock a disk steam into a certain section of the disk and call byte 1 of that reagion offset 0x00.
        private bool is_caged_reader;
        #endregion

        #region Checksum methods (ROMFS and Build Code)
        /// <summary>
        /// Returns the build's actual code checksum based on the code start address and code length in the build's header.
        /// </summary>
        /// <returns></returns>
        public uint calculate_code_checksum()
        {
            var build_header = this.get_build_header();

            var build_length = (BigEndianConverter.ToUInt32(build_header.dword_length, 0) - 3) * WebTVBuildInfo.DWORD_SIZE_BYTES;
            var build_code_length = (BigEndianConverter.ToUInt32(build_header.dword_code_length, 0) - 3) * WebTVBuildInfo.DWORD_SIZE_BYTES;

            if (build_code_length > build_length)
            {
                build_code_length = build_length;
            }

            if (build_code_length > WebTVBuildInfo.MAX_BUILD_SIZE)
            {
                build_code_length = WebTVBuildInfo.MAX_BUILD_SIZE;
            }

            var build_code = new byte[build_code_length];

            reader.Seek(0xC, SeekOrigin.Begin);
            reader.Read(build_code, 0, (int)build_code_length);

            if (byte_converter != null)
            {
                byte_converter.convert_bytes(ref build_code, 0, build_code_length);
            }

            uint checksum = BigEndianConverter.ToUInt32(build_header.jump_instruction, 0);
            for (int i = 0; i < build_code_length; i += 4)
            {
                var build_block = BigEndianConverter.ToUInt32(build_code, i);

                checksum += build_block;
            }

            return checksum;
        }

        /// <summary>
        /// Returns the build's actual ROMFS checksum based on the ROMFS (footer) address and ROMFS length.  ROMFS data is read from the bottom up.
        /// </summary>
        /// <returns></returns>
        public uint calculate_romfs_checksum()
        {
            var build_header = this.get_build_header();

            var romfs_base_address = BigEndianConverter.ToUInt32(build_header.romfs_base_address, 0);

            if (romfs_base_address != WebTVBuildInfo.NO_ROMFS)
            {
                var build_base_address = BigEndianConverter.ToUInt32(build_header.build_base_address, 0);
                var build_length = (BigEndianConverter.ToUInt32(build_header.dword_length, 0) - 3) * WebTVBuildInfo.DWORD_SIZE_BYTES;

                try
                {
                    var romfs_footer = this.get_romfs_footer(build_base_address, romfs_base_address);

                    var romfs_length = (BigEndianConverter.ToUInt32(romfs_footer.dword_romfs_size, 0)) * WebTVBuildInfo.DWORD_SIZE_BYTES;

                    if (romfs_length > build_length)
                    {
                        romfs_length = build_length - WebTVBuildInfo.ROMFS_FOOTER_SIZE;
                    }

                    if (romfs_length > WebTVBuildInfo.MAX_BUILD_SIZE)
                    {
                        romfs_length = WebTVBuildInfo.MAX_BUILD_SIZE - WebTVBuildInfo.ROMFS_FOOTER_SIZE;
                    }

                    var build_code = new byte[romfs_length];

                    var romfs_offset = this.get_romfs_base_offset(build_base_address, romfs_base_address);

                    reader.Seek(romfs_offset - WebTVBuildInfo.ROMFS_FOOTER_SIZE - romfs_length, SeekOrigin.Begin);
                    reader.Read(build_code, 0, (int)romfs_length);

                    if (byte_converter != null)
                    {
                        byte_converter.convert_bytes(ref build_code, 0, romfs_length);
                    }

                    uint checksum = 0;
                    for (int i = 0; i < romfs_length; i += 4)
                    {
                        var build_block = BigEndianConverter.ToUInt32(build_code, i);

                        checksum += build_block;
                    }

                    return checksum;
                }
                catch
                {
                    return 0;
                }

            }

            return 0;
        }
        #endregion

        #region General information methods
        /// <summary>
        /// Parse the build header and ROMFS footer and returns the WebTVBuild object.
        /// </summary>
        /// <returns></returns>
        public WebTVBuild get_build_info()
        {
            var build_header = this.get_build_header();

            bool is_classic_build = false;
            uint jump_instruction = 0;
            uint pre_jump_instruction = 0;
            uint jump_offset = 0;
            uint code_checksum = 0;
            uint dword_length = 0;
            uint dword_code_length = 0;
            uint build_number = 0;
            uint build_flags = 0;
            uint build_base_address = 0;
            uint romfs_base_address = 0;
            uint romfs_checksum = 0;
            uint dword_romfs_size = 0;
            uint heap_data_address = 0;
            uint dword_heap_data_size = 0;
            uint dword_heap_free_size = 0;
            uint dword_heap_compressed_data_size = 0;
            uint code_compressed_address = 0;
            uint dword_code_compressed_size = 0;

            if (build_header.jump_instruction[0] == 0x10)
            {
                jump_instruction = BigEndianConverter.ToUInt32(build_header.jump_instruction, 0);
                pre_jump_instruction = BigEndianConverter.ToUInt32(build_header.pre_jump_instruction, 0);
                code_checksum = BigEndianConverter.ToUInt32(build_header.code_checksum, 0);
                dword_length = BigEndianConverter.ToUInt32(build_header.dword_length, 0);
                dword_code_length = BigEndianConverter.ToUInt32(build_header.dword_code_length, 0);
                build_number = BigEndianConverter.ToUInt32(build_header.build_number, 0);
                build_flags = BigEndianConverter.ToUInt32(build_header.build_flags, 0);
                build_base_address = BigEndianConverter.ToUInt32(build_header.build_base_address, 0);
                romfs_base_address = BigEndianConverter.ToUInt32(build_header.romfs_base_address, 0);
                heap_data_address = BigEndianConverter.ToUInt32(build_header.heap_data_address, 0);
                dword_heap_data_size = BigEndianConverter.ToUInt32(build_header.dword_heap_data_size, 0);
                dword_heap_free_size = BigEndianConverter.ToUInt32(build_header.dword_heap_free_size, 0);
                dword_heap_compressed_data_size = BigEndianConverter.ToUInt32(build_header.dword_heap_compressed_data_size, 0);
                code_compressed_address = BigEndianConverter.ToUInt32(build_header.code_compressed_address, 0);
                dword_code_compressed_size = BigEndianConverter.ToUInt32(build_header.dword_code_compressed_size, 0);

                // Classic builds always start with 0x202020 for whatever reason.  No idea why at this point.
                if (pre_jump_instruction == 0x202020)
                {
                    is_classic_build = true;
                }

                // If the code length is longer than the entire build length then assume there's a problem and set the code length to zero.
                if (dword_code_length > dword_length)
                {
                    dword_code_length = 0;
                }

                // Strip the operation and register information from the MIPS instruction and get the absolute jump offset.
                // We're assuming that the first instruction is always a jump instruction.
                jump_offset = ((jump_instruction & 0xFFFF) << 2) + 4;

                // On UltimateTV builds the ROMFS address is "NoFS", so assume no ROMFS image.
                if (romfs_base_address == WebTVBuildInfo.NO_ROMFS)
                {
                    romfs_base_address = 0;
                }
                else
                {
                    try
                    {
                        var romfs_footer = this.get_romfs_footer(build_base_address, romfs_base_address);

                        romfs_checksum = BigEndianConverter.ToUInt32(romfs_footer.romfs_checksum, 0);
                        dword_romfs_size = BigEndianConverter.ToUInt32(romfs_footer.dword_romfs_size, 0);

                        if (dword_romfs_size > dword_length)
                        {
                            dword_romfs_size = 0;
                        }
                    }
                    catch
                    {
                        romfs_checksum = 0;
                        dword_romfs_size = 0;
                    }
                }
            }

            return new WebTVBuild()
            {
                is_classic_build = is_classic_build,
                jump_instruction = jump_instruction,
                pre_jump_instruction = pre_jump_instruction,
                jump_offset = jump_offset,
                code_checksum = code_checksum,
                dword_length = dword_length,
                dword_code_length = dword_code_length,
                build_number = build_number,
                build_flags = build_flags,
                build_base_address = build_base_address,
                romfs_base_address = romfs_base_address,
                romfs_checksum = romfs_checksum,
                dword_romfs_size = dword_romfs_size,
                heap_data_address = heap_data_address,
                dword_heap_data_size = dword_heap_data_size,
                dword_heap_free_size = dword_heap_free_size,
                dword_heap_compressed_data_size = dword_heap_compressed_data_size,
                code_compressed_address = code_compressed_address,
                dword_code_compressed_size = dword_code_compressed_size
            };
        }
        #endregion

        #region ROMFS methods
        /// <summary>
        /// Get the ROMFS offset in a stream.  It uses the ROMFS address found in the build header and subtracts the base address to get the stream offset.
        /// The ROMFS address found in the build header assumes that the build data is loaded into memory at the base address.
        /// </summary>
        /// <param name="build_base_address">The base memory address that the build image is loaded into.</param>
        /// <param name="romfs_base_address">The ROMFS footer address, it is always below the base memory address.</param>
        /// <returns></returns>
        public uint get_romfs_base_offset(uint build_base_address, uint romfs_base_address)
        {
            if (romfs_base_address > build_base_address)
            {
                return (romfs_base_address - build_base_address);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Parses the ROMFS footer into a webtv_romfs_footer (StructLayout).
        /// </summary>
        /// <param name="build_base_address">The base memory address that the build image is loaded into.</param>
        /// <param name="romfs_base_address">The ROMFS footer address, it is always below the base memory address.</param>
        /// <returns></returns>
        public webtv_romfs_footer get_romfs_footer(uint build_base_address, uint romfs_base_address)
        {
            GCHandle romfs_footer_handle = new GCHandle();

            try
            {
                if (romfs_base_address != WebTVBuildInfo.NO_ROMFS)
                {
                    var romfs_offset = this.get_romfs_base_offset(build_base_address, romfs_base_address);

                    if (romfs_offset > 0 && romfs_offset <= reader.Length)
                    {
                        var romfs_footer_data = new byte[WebTVBuildInfo.ROMFS_FOOTER_SIZE];
                        reader.Seek(romfs_offset - (long)WebTVBuildInfo.ROMFS_FOOTER_SIZE, SeekOrigin.Begin);
                        reader.Read(romfs_footer_data, 0, WebTVBuildInfo.ROMFS_FOOTER_SIZE);

                        if (byte_converter != null)
                        {
                            byte_converter.convert_bytes(ref romfs_footer_data, 0, (uint)WebTVBuildInfo.ROMFS_FOOTER_SIZE);
                        }

                        romfs_footer_handle = GCHandle.Alloc(romfs_footer_data, GCHandleType.Pinned);
                        var romfs_footer_pointer = romfs_footer_handle.AddrOfPinnedObject();

                        return (webtv_romfs_footer)Marshal.PtrToStructure(romfs_footer_pointer, typeof(webtv_romfs_footer));
                    }
                    else
                    {
                        throw new DataMisalignedException("Couldn't seek to ROMFS base because of an invalid address.");
                    }
                }
                else
                {
                    throw new InvalidDataException("No ROMFS available.");
                }
            }
            catch (Exception e)
            {
                throw new InvalidDataException("Couldn't parse WebTV build. " + e.Message);
            }
            finally
            {
                if (romfs_footer_handle.IsAllocated)
                {
                    romfs_footer_handle.Free();
                }
            }
        }

        /// <summary>
        /// Parses the ROMFS footer into a webtv_romfs_footer (StructLayout) using address information from the build image header.
        /// </summary>
        /// <returns></returns>
        public webtv_romfs_footer get_romfs_footer()
        {
            var build_header = this.get_build_header();

            uint build_base_address = BigEndianConverter.ToUInt32(build_header.build_base_address, 0);
            uint romfs_base_address = BigEndianConverter.ToUInt32(build_header.romfs_base_address, 0);

            return this.get_romfs_footer(build_base_address, romfs_base_address);
        }
        #endregion

        #region Build Header methods
        /// <summary>
        /// Parses the build image header into a webtv_build_header (StructLayout).
        /// </summary>
        /// <returns></returns>
        public webtv_build_header get_build_header()
        {
            GCHandle build_header_handle = new GCHandle();

            try
            {
                var build_header_data = new byte[WebTVBuildInfo.BUILD_HEADER_SIZE];
                reader.Seek(0, SeekOrigin.Begin);
                reader.Read(build_header_data, 0, WebTVBuildInfo.BUILD_HEADER_SIZE);

                if (byte_converter != null)
                {
                    byte_converter.convert_bytes(ref build_header_data, 0, (uint)WebTVBuildInfo.BUILD_HEADER_SIZE);
                }

                build_header_handle = GCHandle.Alloc(build_header_data, GCHandleType.Pinned);
                var build_header_pointer = build_header_handle.AddrOfPinnedObject();

                return (webtv_build_header)Marshal.PtrToStructure(build_header_pointer, typeof(webtv_build_header));
            }
            catch (Exception e)
            {
                throw new InvalidDataException("Couldn't parse WebTV build. " + e.Message);
            }
            finally
            {
                if (build_header_handle.IsAllocated)
                {
                    build_header_handle.Free();
                }
            }
        }
        #endregion

        #region Data IO methods
        /// <summary>
        /// Sets the byte collation (byte-swapping method).  This is useful if you're reading from a disk stream where bytes are swapped.
        /// </summary>
        /// <param name="byte_converter">A WebTVDiskCollationConverter type that descibes the byte collation method we should use.</param>
        public void set_converter(WebTVDiskCollationConverter byte_converter)
        {
            this.byte_converter = byte_converter;
        }

        /// <summary>
        /// Close the stream reader.
        /// </summary>
        public void Close()
        {
            if (this.reader != null)
            {
                if (!this.is_caged_reader)
                {
                    this.reader.Close();
                }
                else
                {
                    this.reader.Dispose();
                }
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Construcotor. Open a file stream based on a file name.
        /// </summary>
        /// <param name="file_name">The full path to the build image file.</param>
        public WebTVBuildInfo(string file_name)
        {
            FileStream build_reader = File.Open(file_name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            if (build_reader != null)
            {
                this.reader = build_reader;

                var converter = new WebTVDiskCollationConverter();
                converter.byte_transform = converter.detect_build_byte_transform(this.reader, 0);

                this.set_converter(converter);
            }
            else
            {
                throw new FileLoadException("Couldn't open WebTV build image.");
            }
        }

        /// <summary>
        /// Construcotor. Inject a file stream.  Useful if using disk IO.
        /// </summary>
        /// <param name="reader">Steam object we should use to parse the build information.</param>
        public WebTVBuildInfo(Stream reader)
        {
            this.reader = reader;
        }
        #endregion
    }
}
