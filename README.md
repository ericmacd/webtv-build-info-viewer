## WebTV (MSNTV) Build Information Viewer

This is tool that displays ROMFS and build properties by reading a build image file.

This mainly only works on builds built for boxes that have a hard drive.

Builds for boxes I know should work:

- **WebTV Plus v1**: INT-200, INT-W200B, MAT-972, SIS-100, RW-2000, RW-2001
- **Japan**: INT-WJ200, INT-WJ300
- **Dish Network**: Dishplayer 7100, Dishplayer 7200
- **DirectTV (UltimateTV)**: SAT-W60, DWD490RE

Build properties shown:

- **Build Number**: The client version number shown on the 411 page.
- **Build Flags**: What kind of build image this is (debug build, satellite build, Windows CE image, if the build has raw or compressed heap data).
- **Found Code Checksum**: The code checksum in the file.
- **Actual Code Checksum**: The calculated code checksum.  If this doesn't match the found checksum, the build will be rejected by the box.
- **File Size**: The file size of the build image file.  This is identical to what you see in Windows Explorer.
- **Build Size**: The build size according to the build header.  This includes the build code and data sections but not the ROMFS section.
- **Code Checksum Size**: The size of the code section.  This will always be smaller than the build size.  The code section is used to calculate the checksum.
- **RAM Code Address**: For compressed builds, this specifies the address to the start of the compressed code in the file.
- **RAM Code Size (Shrunk)**: This specifies the size of the compressed code in the file.
- **Jump Offset (start)**: The offset in the build image file where the main code starts executing.
- **Build Base**: The RAM address where the build is loaded.
- **ROMFS Base**: The RAM address where the bottom of the ROMFS section is loaded.
- **Found ROMFS Checksum**: The ROMFS checksum found in the build image file.
- **Actual ROMFS Checksum**: The calculated ROMFS checksum.  If this doesn't match the found ROMFS checksum then the box will reject this build.
- **Initial Heap Size**: The initial heap size the box will allocate.
- **Heap Data Address**: The initial heap data address in the build image file.
- **Heap Data Size (Shrunk)**: If the build flags specifies that we have compressed initial heap data then this will be the size of the compressed data.
- **Heap Data Size (Expanded)**: This is the size of the uncompressed heap once it's expanded into memory.

![Screenshot](https://raw.githubusercontent.com/ericmacd/webtv-build-info-viewer/master/webtv_build_info/view/static/images/screenshot1.png)

If you use WinHex, there's a WinHex template that displays the build image header information in `winhex_template/webtv_build_header.tpl`
