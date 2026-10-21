# P1-T12 — CSharpier format and check

Timestamp: 2026-09-07T01-20

## Enumeration pass (pre-format)

Command: `dotnet tool run csharpier check .`
EXIT_CODE: 1
ExpectedExitCode: 1

Run from `<repo-root>` before the format pass so the exact set of files the format pass would
rewrite is observed rather than inferred. CSharpier's `format` subcommand is write-mode and exits 0
whether or not it rewrote anything, and its summary line reports files **scanned**, not files
rewritten, so the pre-format `check` enumeration is the only observation that identifies the rewrite
set.

Output Summary: `Checked 1593 files in 6783ms.` Seven files reported as `Was not formatted`:

- `QuickFiler.Test/Controllers/QfcDatamodelRethrowTests.cs` — line endings
- `TaskMaster/Ribbon/RibbonCommandBoundary.cs` — line endings
- `TaskMaster.Test/Ribbon/RibbonCommandBoundaryTests.cs` — line endings
- `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` — line endings
- `UtilitiesCS.Test/Extensions/DfDeedleRequiredColumnValidationTests.cs` — line endings
- `UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs` — line endings
- `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` — argument list at the repaired direct call
  collapsed to a single line

All seven are paths this change creates or modifies. No file outside this change's paths was
reported.

## Format pass

Command: `dotnet tool run csharpier format .`
EXIT_CODE: 0

Output Summary: `Formatted 1593 files in 6660ms.` That line reports the number of files scanned and
does not distinguish a clean run from a repairing one, which is why the before-and-after tree
observation below is recorded.

## Before-and-after tree observation

Command: git status --porcelain --untracked-files=all -- . ":(exclude).claude" ":(exclude)docs"

Before the format pass (13 entries):

```
 M QuickFiler.Test/QuickFiler.Test.csproj
 M TaskMaster.Test/TaskMaster.Test.csproj
 M TaskMaster/TaskMaster.csproj
 M UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs
 M UtilitiesCS.Test/UtilitiesCS.Test.csproj
 M UtilitiesCS/Extensions/DfDeedle.cs
 M UtilitiesCS/UtilitiesCS.csproj
?? QuickFiler.Test/Controllers/QfcDatamodelRethrowTests.cs
?? TaskMaster.Test/Ribbon/RibbonCommandBoundaryTests.cs
?? TaskMaster/Ribbon/RibbonCommandBoundary.cs
?? UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs
?? UtilitiesCS.Test/Extensions/DfDeedleRequiredColumnValidationTests.cs
?? UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs
```

After the format pass: identical, the same 13 entries with the same status codes. The format pass
introduced no new modified or untracked path.

Verdict: the format pass rewrote seven files, every one of them a path this change creates or
modifies. It rewrote no other file.

## Subset assertion (conditional clause — inactive)

P0-T5 recorded `EXIT_CODE: 0` for `dotnet tool run csharpier check .` with an empty pre-existing
unformatted set. The conditional clause in P1-T12 that applies only where P0-T5 recorded a non-zero
exit is therefore inactive. The whole-tree pass is a no-op outside this change's paths, and the
observation above confirms that outcome rather than assuming it.

## Verification pass (post-format)

Command: `dotnet tool run csharpier check .`
EXIT_CODE: 0

Output Summary: success-case summary line quoted verbatim: `Checked 1593 files in 6873ms.` The line
begins with the literal `Checked ` and ends with the literal `ms.`, and no file was reported as
unformatted.

## Acceptance

The check command recorded `EXIT_CODE: 0` and its success-case summary line is quoted verbatim
above. P1-T12 acceptance satisfied.
