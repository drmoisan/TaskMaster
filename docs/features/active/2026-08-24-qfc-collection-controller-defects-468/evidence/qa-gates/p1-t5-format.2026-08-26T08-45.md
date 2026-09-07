# [P1-T5] Format gate (CSharpier)

Timestamp: 2026-08-26T08-45

Command (mutating, owned path only):
`pwsh -NoProfile -Command "Set-Location '<WS>'; dotnet tool run csharpier format QuickFiler/Controllers/QfcCollectionController.cs"`

Command (read-only, repository-wide):
`pwsh -NoProfile -Command "Set-Location '<WS>'; dotnet tool run csharpier check ."`

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

### Mutating pass — scoped to the owned file

```
Formatted 1 files in 1729ms.
```

Exit code 0.

The mutating invocation takes an **explicit owned-path argument**
(`QuickFiler/Controllers/QfcCollectionController.cs`), never a bare `.`, so it structurally cannot
rewrite any file outside this plan's ownership scope. In particular it cannot touch
`QuickFiler/Controllers/KbdActions.cs` (D2) or any sibling epic child's file.

**"Formatted 1 files" is CSharpier's count of files *processed*, not files *modified*.** The pass
changed nothing, confirmed three ways:

| Check | Before format | After format |
|---|---|---|
| `git diff --stat` | `241 deletions(-)`, 0 insertions | `241 deletions(-)`, 0 insertions |
| Line count | 2108 | 2108 |
| Encoding | UTF-8 with BOM, CRLF | UTF-8 with BOM, CRLF |

The P1-T2 deletion therefore left the file already CSharpier-clean, which is the expected outcome of
a pure whole-member deletion that removes complete lines and their blank separators without reflowing
any surviving construct. The UTF-8 BOM and CRLF line terminators are preserved.

### Read-only verification pass — repository-wide

```
Checked 1520 files in 7884ms.
```

**EXIT_CODE: 0.**

Files reported as needing formatting: **0**. CSharpier 1.2.6 reports each unformatted file on its own
line as `Error <path> - Was not formatted`; the output is a single line containing no such entry.

### Comparison with the P0-T11 baseline

| Metric | P0-T11 baseline | P1-T5 | Delta |
|---|---|---|---|
| Files checked | 1520 | 1520 | 0 |
| Files needing formatting | 0 | **0** | 0 |
| `csharpier check .` exit code | 0 | **0** | 0 |

The file count is unchanged at 1520 because P1-T2 deleted lines from an existing file and created or
removed no file. The tree was formatting-clean at baseline and remains formatting-clean, so this gate
is measured against a real zero rather than an unknown.

### Acceptance verification

`dotnet tool run csharpier check .` reports `EXIT_CODE: 0`.

Result: PASS. Toolchain step 1 (Formatting) is green; the loop proceeds to step 2 (P1-T6, analyzers).
