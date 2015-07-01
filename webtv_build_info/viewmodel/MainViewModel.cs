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
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.IO;

namespace webtv_build_info
{
    /// <summary>
    /// The ViewModel class for the main window (build info dialog).
    /// </summary>
    class MainViewModel
    {
        #region Delegates
        private delegate void void_call();
        #endregion

        #region Fields
        public MainWindow main_window;
        public string new_build_filename;
        public uint calculated_code_checksum;
        public uint calculated_romfs_checksum;
        #endregion

        #region Events
        private RelayCommand _cancel_command;
        public ICommand cancel_command
        {
            get
            {
                if (_cancel_command == null)
                {
                    _cancel_command = new RelayCommand(x => on_cancel_click(), x => true);
                }

                return _cancel_command;
            }
        }

        private RelayCommand _load_build_image;
        public ICommand load_build_image
        {
            get
            {
                if (_load_build_image == null)
                {
                    _load_build_image = new RelayCommand(x => on_load_build_image(), x => true);
                }

                return _load_build_image;
            }
        }

        /// <summary>
        /// UI callback that's used after you click on the "Canel" button to close the dialog.
        /// </summary>
        public void on_cancel_click()
        {
            close_window();
        }

        /// <summary>
        /// UI callback that's used after you click the "Find" button in the UI to update the build image path. 
        /// </summary>
        public void on_load_build_image()
        {
            System.Windows.Forms.OpenFileDialog file_dialog = new System.Windows.Forms.OpenFileDialog();
            file_dialog.Filter = "ROM Image Files (*.o, *.img, *.bin, *.ima)|*.o;*.img;*.bin;*.ima|All Files (*.*)|*.*";
            file_dialog.Title = "Select WebTV ROM Image";

            if (file_dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                new_build_filename = file_dialog.FileName;

                set_new_build_info();
            }
        }
        #endregion

        #region UI update methods
        /// <summary>
        /// Resets the UI so that all the build information is in its default state.
        /// </summary>
        /// <param name="pane_prefix">The prefix to use when looking up UI objects.  This is only useful if you have multiple panes of build information in one dialog.</param>
        public void reset_build_page(string pane_prefix)
        {
            if (pane_prefix == "new")
            {
                ((TextBox)this.main_window.FindName(pane_prefix + "_file_path")).Text = "";
                ((TextBox)this.main_window.FindName(pane_prefix + "_file_size")).Text = "-";
                ((TextBox)this.main_window.FindName(pane_prefix + "_file_size")).Foreground = Brushes.Black;
                ((TextBox)this.main_window.FindName(pane_prefix + "_build_collation")).Text = "-";
            }

            ((TextBox)this.main_window.FindName(pane_prefix + "_build_number")).Text = "-";
            ((TextBox)this.main_window.FindName(pane_prefix + "_build_flags")).Text = "-";
            ((TextBox)this.main_window.FindName(pane_prefix + "_build_code_checksum")).Text = "-";
            ((TextBox)this.main_window.FindName(pane_prefix + "_build_code_checksum")).Foreground = Brushes.Black;
            ((TextBox)this.main_window.FindName(pane_prefix + "_build_length")).Text = "-";
            ((TextBox)this.main_window.FindName(pane_prefix + "_build_code_length")).Text = "-";
            ((TextBox)this.main_window.FindName(pane_prefix + "_build_jump_offset")).Text = "-";
            ((TextBox)this.main_window.FindName(pane_prefix + "_build_base")).Text = "-";
            ((TextBox)this.main_window.FindName(pane_prefix + "_build_romfs_base")).Text = "-";
            ((TextBox)this.main_window.FindName(pane_prefix + "_build_romfs_checksum")).Text = "-";
            ((TextBox)this.main_window.FindName(pane_prefix + "_build_romfs_checksum")).Foreground = Brushes.Black;
            ((TextBox)this.main_window.FindName(pane_prefix + "_build_calculated_romfs_checksum")).Text = "-";
            ((TextBox)this.main_window.FindName(pane_prefix + "_build_romfs_size")).Text = "-";
        }

        /// <summary>
        /// The code that stringifies build information and update the UI so it's displayed in the dialog.
        /// </summary>
        /// <param name="build">The WebTVBuildInfo object that contains the header information from the build.</param>
        /// <param name="pane_prefix">The prefix to use when looking up UI objects.  This is only useful if you have multiple panes of build information in one dialog.</param>
        /// <param name="filename">The full file path to the build image.</param>
        /// <param name="file_size_bytes">The size of the build image file.</param>
        /// <param name="file_offset_bytes">The offset within the build image file that the first byte of the build is located.  Useful if using disk image files.</param>
        /// <param name="collation">The format (byte-swap type) of the build image file.</param>
        public void style_build_pane(WebTVBuildInfo build, string pane_prefix, string filename = "", ulong file_size_bytes = 0, ulong file_offset_bytes = 0, DiskByteTransform collation = DiskByteTransform.NOSWAP)
        {
            reset_build_page(pane_prefix);

            var build_header = build.get_build_info();
            this.calculated_romfs_checksum = 0;
            this.calculated_code_checksum = 0;

            if (pane_prefix == "new")
            {
                ((TextBox)this.main_window.FindName(pane_prefix + "_file_path")).Text = filename;
                ((TextBox)this.main_window.FindName(pane_prefix + "_file_size")).Text = BytesToString.bytes_to_iec(file_size_bytes);

                var collation_description = "";
                switch (collation)
                {
                    case DiskByteTransform.BIT16SWAP:
                        collation_description = "16-bit swapped";
                        break;

                    case DiskByteTransform.BIT1632SWAP:
                        collation_description = "16+32-bit swapped";
                        break;

                    case DiskByteTransform.BIT32SWAP:
                        collation_description = "32-bit swapped";
                        break;

                    case DiskByteTransform.NOSWAP:
                    default:
                        collation_description = "no swapping";
                        break;
                }

                ((TextBox)this.main_window.FindName(pane_prefix + "_build_collation")).Text = collation_description;
            }

            var build_flags = new List<string>();

            if ((build_header.build_flags & 0x04) != 0)
            {
                build_flags.Add("Debug");
            }
            if ((build_header.build_flags & 0x20) != 0)
            {
                build_flags.Add("Satellite?");
            }
            if ((build_header.build_flags & 0x10) != 0)
            {
                build_flags.Add("Windows CE?");
            }
            if ((build_header.build_flags & 0x01) != 0)
            {
                build_flags.Add("Compressed Heap Data");
            }
            else
            {
                build_flags.Add("Raw Heap Data");
            }

            if (build_flags.Count > 0)
            {
                ((TextBox)this.main_window.FindName(pane_prefix + "_build_flags")).Text = String.Join(", ", build_flags);
            }
            else
            {
                ((TextBox)this.main_window.FindName(pane_prefix + "_build_flags")).Text = "-";
            }

            if (!build_header.is_classic_build && build_header.build_number > 0 && build_header.dword_code_length > 0 && build_header.jump_offset > 4)
            {
                this.calculated_code_checksum = build.calculate_code_checksum();
            }
            else if (pane_prefix == "new")
            {
                ((CheckBox)this.main_window.FindName(pane_prefix + "_build_use_calculated_code_checksum")).IsChecked = false;
                ((CheckBox)this.main_window.FindName(pane_prefix + "_build_use_calculated_romfs_checksum")).IsChecked = false;
            }

            if (build_header.build_number == 0)
            {
                ((TextBox)this.main_window.FindName(pane_prefix + "_build_number")).Text = "?";
            }
            else
            {
                ((TextBox)this.main_window.FindName(pane_prefix + "_build_number")).Text = build_header.build_number.ToString();
            }

            if (build_header.is_classic_build)
            {
                ((TextBox)this.main_window.FindName(pane_prefix + "_build_code_checksum")).Text = "Classic build?";
            }
            else
            {
                ((TextBox)this.main_window.FindName(pane_prefix + "_build_code_checksum")).Text = build_header.code_checksum.ToString("X");

                if (build_header.jump_offset > 4 && this.calculated_code_checksum != build_header.code_checksum)
                {
                    ((TextBox)this.main_window.FindName(pane_prefix + "_build_code_checksum")).Foreground = Brushes.Red;
                }
            }

            if (build_header.jump_offset == 4)
            {
                ((TextBox)this.main_window.FindName(pane_prefix + "_build_calculated_code_checksum")).Text = "Compressed build?";
            }
            else
            {
                ((TextBox)this.main_window.FindName(pane_prefix + "_build_calculated_code_checksum")).Text = this.calculated_code_checksum.ToString("X");
            }


            if (build_header.is_classic_build || build_header.dword_length == 0)
            {
                ((TextBox)this.main_window.FindName(pane_prefix + "_build_length")).Text = "?";
            }
            else
            {
                ((TextBox)this.main_window.FindName(pane_prefix + "_build_length")).Text = this.get_size_string(build_header.dword_length);
            }

            if (build_header.is_classic_build || build_header.dword_code_length == 0)
            {
                ((TextBox)this.main_window.FindName(pane_prefix + "_build_code_length")).Text = "?";
            }
            else
            {
                ((TextBox)this.main_window.FindName(pane_prefix + "_build_code_length")).Text = this.get_size_string(build_header.dword_code_length);
            }

            ((TextBox)this.main_window.FindName(pane_prefix + "_build_jump_offset")).Text = this.get_offset_string((build_header.build_base_address + build_header.jump_offset), build_header.build_base_address, file_offset_bytes);

            ((TextBox)this.main_window.FindName(pane_prefix + "_build_base")).Text = this.get_offset_string(build_header.build_base_address, build_header.build_base_address, file_offset_bytes);

            if (build_header.romfs_base_address == 0)
            {
                ((TextBox)this.main_window.FindName(pane_prefix + "_build_romfs_base")).Text = "No ROMFS";

                if (pane_prefix == "new")
                {
                    ((CheckBox)this.main_window.FindName(pane_prefix + "_build_use_calculated_romfs_checksum")).IsChecked = false;
                }
            }
            else if(!build_header.is_classic_build)
            {
                this.calculated_romfs_checksum = build.calculate_romfs_checksum();

                ((TextBox)this.main_window.FindName(pane_prefix + "_build_romfs_base")).Text = this.get_offset_string(build_header.romfs_base_address, build_header.build_base_address, file_offset_bytes);

                ((TextBox)this.main_window.FindName(pane_prefix + "_build_romfs_checksum")).Text = build_header.romfs_checksum.ToString("X");
                ((TextBox)this.main_window.FindName(pane_prefix + "_build_calculated_romfs_checksum")).Text = this.calculated_romfs_checksum.ToString("X");
                ((TextBox)this.main_window.FindName(pane_prefix + "_build_romfs_size")).Text = this.get_size_string(build_header.dword_romfs_size);

                if (this.calculated_romfs_checksum != build_header.romfs_checksum)
                {
                    ((TextBox)this.main_window.FindName(pane_prefix + "_build_romfs_checksum")).Foreground = Brushes.Red;
                }
            }

            if (!build_header.is_classic_build)
            {
                ((TextBox)this.main_window.FindName(pane_prefix + "_build_heap_size")).Text = this.get_size_string(build_header.dword_heap_data_size + build_header.dword_heap_free_size);
                ((TextBox)this.main_window.FindName(pane_prefix + "_build_heap_data_offset")).Text = this.get_offset_string(build_header.heap_data_address, build_header.build_base_address, file_offset_bytes);
                ((TextBox)this.main_window.FindName(pane_prefix + "_build_heap_data_compressed_size")).Text = this.get_size_string(build_header.dword_heap_compressed_data_size);
                ((TextBox)this.main_window.FindName(pane_prefix + "_build_heap_data_size")).Text = this.get_size_string(build_header.dword_heap_data_size);

                ((TextBox)this.main_window.FindName(pane_prefix + "_build_code_shrunk_offset")).Text = this.get_offset_string(build_header.code_compressed_address, build_header.build_base_address, file_offset_bytes);
                ((TextBox)this.main_window.FindName(pane_prefix + "_build_code_shrunk_size")).Text = this.get_size_string(build_header.dword_code_compressed_size);
            }
        }

        /// <summary>
        /// Fetches the build data from this.new_build_filename and displays it on the main_window.
        /// </summary>
        public void set_new_build_info()
        {
            if (this.new_build_filename != null && this.new_build_filename != "")
            {
                try
                {
                    var build = new WebTVBuildInfo(this.new_build_filename);

                    style_build_pane(build, "new", this.new_build_filename, (ulong)build.reader.Length, 0, build.byte_converter.byte_transform);

                    build.Close();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error! " + e.Message);
                }
            }
        }
        #endregion

        #region Utility methods
        /// <summary>
        /// Thread-safe call to close the build info dialog.
        /// </summary>
        public void close_window()
        {
            if (this.main_window.Dispatcher.CheckAccess() == false)
            {
                var cb = new void_call(this.close_window);

                this.main_window.Dispatcher.Invoke(cb);
            }
            else
            {
                this.main_window.Close();
            }
        }

        /// <summary>
        /// Returns a string showing an offset address in hex.  We will assume the offset is a memory address if the offset is greater than the base address.  
        /// All memory addresses will be converted into file offsets within curly brackets.
        /// </summary>
        /// <param name="offset">The offset address to display.</param>
        /// <param name="build_base">The base memory address the build will load into.</param>
        /// <param name="stream_offset">The offset in a file stream that the first byte of the build begins.  Useful if you're reading from a disk stream.</param>
        /// <returns></returns>
        public string get_offset_string(ulong offset, ulong build_base, ulong stream_offset)
        {
            var offset_string = offset.ToString("X");

            if (offset >= build_base)
            {
                offset_string += " {" + (stream_offset + (offset - build_base)).ToString("X") + "}";
            }

            return offset_string;
        }

        /// <summary>
        /// Returns human-readable size string based on an unsigned integer input.  It displays it in the format "X bytes (hex size)".
        /// </summary>
        /// <param name="dword_size">An unsigned integer to be converted into a human-readable size string</param>
        /// <returns></returns>
        public string get_size_string(uint dword_size)
        { 
            var size = dword_size * WebTVBuildInfo.DWORD_SIZE_BYTES;

            return BytesToString.bytes_to_iec(size) + " (" + dword_size.ToString("X") + ")";
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Construcotor
        /// </summary>
        /// <param name="main_window"></param>
        public MainViewModel(MainWindow main_window)
        {
            this.main_window = main_window;
        }
        #endregion
    }
}
