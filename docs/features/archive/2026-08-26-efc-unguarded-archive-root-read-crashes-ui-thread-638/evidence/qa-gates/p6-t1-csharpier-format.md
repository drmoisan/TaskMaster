# [P6-T1] CSharpier format (Issue 638)

Timestamp: 2026-08-29T12-36

Command: `dotnet tool run csharpier format .`

Branch taken: **unscoped**. [P0-T9] recorded `BASELINE_UNFORMATTED_COUNT: 0`, so the command
runs against `.` as written, with no scoping to the two owned files.

EXIT_CODE: 0

Output Summary:

The command is write-mode: its exit code is 0 whether or not it rewrote anything, so this
task is judged on a tree observation — the SHA-256 of the two owned files before and after
— rather than on the exit code.

## Pass 1 (rejected; the loop restarted)

```
BeforeHashes:
  QuickFiler/Controllers/EfcDataModel.cs                        995BB6452CDD1DD012713DC856EEA8F9897401F0F261BCA38166B2A05B2A5E2A
  QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs   81E1AA23B5F25702B56BD71B09D14AAEC992D8535A40ADA7D630C6DB0002B148

AfterHashes:
  QuickFiler/Controllers/EfcDataModel.cs                        995BB6452CDD1DD012713DC856EEA8F9897401F0F261BCA38166B2A05B2A5E2A
  QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs   A20BCF32C44D861F96304152864C7C5D4891CA7F696988E3F2798432987794EB
```

Final summary line: `Formatted 1561 files in 4454ms.` (a processed count, not a changed
count).

The hash of `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` changed, so per
the Phase 6 preamble and `CLAUDE.md` § "After Making Changes" the loop restarted from
[P6-T1]. The change was line-ending normalization: the file was created with LF endings and
CSharpier rewrote it to the CRLF used throughout this worktree under `core.autocrlf=true`.
The line count was 389 before and 389 after, and a post-format measurement confirmed
`LF=389 CRLF=389`, that is, every LF is part of a CRLF. `git status --porcelain -uall`
after the pass named only the three paths in this change's footprint, so no other file was
rewritten.

## Pass 2 (the accepted pass)

```
BeforeHashes:
  QuickFiler/Controllers/EfcDataModel.cs                        995BB6452CDD1DD012713DC856EEA8F9897401F0F261BCA38166B2A05B2A5E2A
  QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs   A20BCF32C44D861F96304152864C7C5D4891CA7F696988E3F2798432987794EB

AfterHashes:
  QuickFiler/Controllers/EfcDataModel.cs                        995BB6452CDD1DD012713DC856EEA8F9897401F0F261BCA38166B2A05B2A5E2A
  QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs   A20BCF32C44D861F96304152864C7C5D4891CA7F696988E3F2798432987794EB
```

Final summary line: `Formatted 1561 files in 1321ms.`

Both hash pairs are equal, so this pass is a fixpoint and the formatting step changed no
file. [P6-T2] through [P6-T5] ran after this pass; the `AfterHashes:` values above are the
ones [P6-T6] compares against.
