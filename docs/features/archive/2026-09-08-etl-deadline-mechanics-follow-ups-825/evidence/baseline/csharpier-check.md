# Baseline — CSharpier Format Check

Timestamp: 2026-09-09T16-34

Command: dotnet tool run csharpier check .

EXIT_CODE: 0

CheckedFiles: 1622
UnformattedFileList: none

Output Summary: CSharpier 1.2.6 printed a single summary line, "Checked 1622 files in 4433ms.", and
reported no unformatted file. The exit code is 0, so the tree carries no pre-existing formatter
drift. This matters for the P9-T4 Write Set accounting: P8-T1 runs a repository-wide mutating
`csharpier format .`, and any file it repaired would enter the branch diff. With zero drift at
baseline, the only files that pass can rewrite are the ones this plan itself edits, so the diff
remains confined to the eleven Write Set paths and this feature's own folder.
