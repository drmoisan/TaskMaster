# Final QC Stage 1a — CSharpier Format

- Task: `[P2-T1]`
- Issue: #418
- Evidence series: `2026-08-05T05-00`
- Toolchain pass: **1**
- Timestamp: 2026-08-05T00-06

## Command

```
dotnet tool run csharpier format .
```

Run from the repository root.

```
EXIT_CODE: 0
```

Verbatim output:

```
Formatted 1467 files in 1335ms.
```

## Files reformatted: 0

The `Formatted 1467 files` line reports the number of files csharpier **processed**, not the number it
**changed**. The authoritative measure of whether any file was rewritten is the working tree itself, and
it was used here rather than inferred from the tool's wording.

```
Command: git diff --numstat -- SVGControl.Test/SVGControl.Test.csproj SVGControl.Test/packages.config
Output:  5	0	SVGControl.Test/SVGControl.Test.csproj
         1	0	SVGControl.Test/packages.config
```

Both figures are **identical** to the pre-format state recorded in
`evidence/other/scope-guard.2026-08-05T05-00.md` (5/0 and 1/0). Neither file was rewritten.

```
Command: git diff --stat
Output:
 SVGControl.Test/SVGControl.Test.csproj             |  5 +++
 SVGControl.Test/packages.config                    |  1 +
 .../remediation-plan.2026-08-05T05-00.md           | 36 +++++++++++-----------
 3 files changed, 24 insertions(+), 18 deletions(-)
```

The changed-file set is unchanged: the same three paths, no new entry. The plan file's figure moved from
34 to 36 changed lines only because `[P1-T7]` was checked off between the two measurements (18 check-offs
now, so 18 insertions + 18 deletions; 6 + 18 = 24 insertions, matching the total).

```
Command: grep -c "Was not formatted" <output>
Output:  0
```

**Reformatted count: 0.** No non-conformance was reported and no file changed, so the loop does **not**
restart at this task.

## The `packages.config` entry survived the formatter unreflowed

This is the specific outcome `[P1-T2]` flagged for verification, because `packages.config` is **not**
csharpier-exempt: `.csharpierignore` excludes `*.csproj`, `*.props`, and `*.targets` but not
`packages.config`, and 19 entries in that file are already visibly csharpier-reflowed across multiple
lines.

Post-format state, read from disk:

```
5-  <package id="Castle.Core" version="5.2.1" targetFramework="net481" />
6:  <package id="ExCSS" version="4.3.2" targetFramework="net481" />
7-  <package id="FluentAssertions" version="8.10.0" targetFramework="net481" />
```

The new `ExCSS` entry remains a **single line** at line 6, in its alphabetical position between
`Castle.Core` and `FluentAssertions`. It was not reflowed.

The reason is width, not exemption, exactly as `[P1-T2]` predicted: the entry is 63 characters of element
text (65 including its two-space indent), measured at `[P1-T2]`, against an in-file single-line precedent
that survives to at least 97 characters (`System.Diagnostics.DiagnosticSource` at
`SVGControl.Test/packages.config:120`). `[P1-T2]`'s contingency — "if `[P2-T1]` reflows it anyway, the
reflowed form is correct and this task's acceptance is re-evaluated against the post-format file" — did
**not** need to be invoked, so `[P1-T2]`'s original one-added-line acceptance stands as recorded.

## Expected-versus-measured

This cycle modifies no `.cs` file, so the expected reformatted count was zero. The measured count is
zero. No file changed, so no identification of a changed file and no loop restart is required.

## Output Summary

`EXIT_CODE: 0`. `Formatted 1467 files in 1335ms`, with **0 files reformatted** — verified against the
working tree rather than inferred from the tool's wording: `git diff --numstat` returns the identical
5/0 and 1/0 figures for the two functional files, `git diff --stat` shows the identical three-path
changed set, and `grep -c "Was not formatted"` returns 0. The single-line `ExCSS` entry in
`SVGControl.Test/packages.config` survived unreflowed at 65 characters. Stage 1a of toolchain pass 1 is
clean and the loop proceeds to `[P2-T2]` without restart.
