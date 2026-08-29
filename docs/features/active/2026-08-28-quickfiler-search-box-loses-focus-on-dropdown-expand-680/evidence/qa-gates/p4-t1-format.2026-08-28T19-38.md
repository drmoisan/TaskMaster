Timestamp: 2026-08-28T19-38
Command: dotnet tool run csharpier check . ; dotnet tool run csharpier format . ; dotnet tool run csharpier check .
EXIT_CODE: 0
Output Summary: PRE_FORMAT_CHECK_EXIT = 0 ("Checked 1560 files in 4246ms" — no drift; this is the
restarted pass after the P2-T1 test file needed one reflow in the prior iteration). Format command
output: "Formatted 1560 files in 1255ms" (write-mode pass; formatted count is a processed count, not a
changed count). Post-format check EXIT_CODE 0 ("Checked 1560 files in 4142ms"). git status --porcelain
captured immediately before and immediately after the format command is byte-identical (diff exit 0),
confirming the format command changed no file in this pass. This pass counts as final for the format
gate.
