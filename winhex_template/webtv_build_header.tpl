template "ubergeek03: WebTV Build Header"
description "Used to show build parameters.  Use at offset 0."
big-endian

begin
	section "First Instruction"
		hex 4 "Jump Instruction"
		hex 4 "Pre-jump Instruction"
	endsection

	section "General Information"
		uint32 "Code Checksum"
		uint32 "Build Size (DWORDs)"
		uint32 "Code Checksum Size (DWORDs)"
		uint32 "Build Number"
	endsection

	section "Heap Information"
		hex 4 "Heap Data Address"
		uint32 "Heap Data Size (DWORDs)"
		uint32 "Heap Free Size"
	endsection

	section "Build Load Information"
		hex 4 "ROMFS Address"
		uint32 "-"
		uint32 "-"
		hex 4 "Build Base Address"
		hex 4 "Build Flags"
	endsection

	section "Compression Information"
		uint32 "Heap Data Size (Shrunk)"
		hex 4 "RAM Code Address"
		uint32 "RAM Code Size (Shrunk,DWORDs)"
	endsection
end