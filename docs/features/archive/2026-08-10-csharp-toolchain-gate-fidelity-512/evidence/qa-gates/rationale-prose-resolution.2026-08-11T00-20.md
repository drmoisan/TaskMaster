# AC10 — resolution of rationale prose adjacent to the corrected commands ([P5-T12])

Timestamp: 2026-08-11T00-20
Command: (none — analysis artifact)
EXIT_CODE: (none — analysis artifact)

AC10 requires that factually incorrect rationale prose adjacent to the corrected commands is
**either corrected or explicitly recorded as verified-correct**. Both dispositions are recorded
below.

## 1. Corrected — the `CLAUDE.md` CSharpier-scope claim

**Merge-base text** (`CLAUDE.md`, § C#1 item 1):

> `csharpier` is file-based and formats only `*.cs` without touching project files.

**Corrected by Block R1** ([P3-T1]) to:

> `csharpier` is file-based and does not load the solution or project model, so it cannot rewrite a
> `.csproj` as a side effect of parsing the build graph. It is **not** restricted to `*.cs`:
> CSharpier 1.2.6 also accepts and processes `*.xml` and `packages.config`. `*.csproj`, `*.props` and
> `*.targets` are kept out of the check by `.csharpierignore`, not by any inherent CSharpier
> behavior.

### Measured basis

From `FEATURE/evidence/baseline/baseline-csharpier-replacement-forms.2026-08-10T14-45.md`, the two
direct probes of non-`.cs` files:

```
$ dotnet tool run csharpier check QuickFiler\packages.config
Checked 1 files in 425ms.
EXIT_CODE: 0

$ dotnet tool run csharpier check TaskMaster\Ribbon\RibbonExplorer.xml
Checked 1 files in 444ms.
EXIT_CODE: 0
```

Both non-`.cs` files were **accepted and processed**, not ignored. The clause "formats only `*.cs`"
is therefore factually false.

The second clause, "without touching project files", is **true only because `.csharpierignore`
explicitly lists `*.csproj`, `*.props` and `*.targets`**, not because of any inherent CSharpier
behavior. The corrected text states the mechanism rather than asserting the outcome.

This sentence sits directly adjacent to the defective format command at the enumerated site, so
correcting it is within the epic's authorization to edit "the toolchain command text and its
surrounding rationale at the enumerated sites".

## 2. Reviewed and recorded as verified-correct — left unchanged

| # | Site | Text (abridged) | Disposition |
|---|---|---|---|
| 1 | `CLAUDE.md` § C#1 item 1, the `dotnet format` warning | "Do **not** use `dotnet format` — it loads the solution/project model and can mis-handle legacy VSTO / .NET Framework projects by rewriting `.csproj` files." | **Verified correct.** Retained byte-identical. The projects are legacy non-SDK `packages.config` VSTO projects, exactly the class this warns about. |
| 2 | `CLAUDE.md` § C#1 item 1, formatter-output-wins | "Do not hand-format; if a diff disagrees with `csharpier`, formatter output wins." | **Verified correct.** Retained byte-identical. |
| 3 | `CLAUDE.md` § C#2 item 2 | "Keep nullable reference types enabled." | **Verified correct under a per-file opt-in reading**, and outside the enumerated-site authorization. Design guidance, not a command. Reviewed and left unchanged. |
| 4 | `.claude/rules/csharp.md` § Coding Standards, null-safety bullet | "Keep nullable reference types enabled. Model optional values with nullable annotations and guard clauses." | Same disposition as #3. Reviewed and left unchanged. |
| 5 | `.claude/rules/csharp.md` § Severity-first ordering invariant | "All new analyzer rule severities are configured in `.editorconfig` at `severity = suggestion` ... BEFORE any `<Analyzer Include>` item is wired into a project." | **The invariant itself is verified correct and preserved verbatim.** Only the embedded command string changed ([P4-T4], row 15): `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` -> `msbuild ... /t:Rebuild /m ... /p:TreatWarningsAsErrors=true`. `git diff <MERGE_BASE>` shows exactly one changed line in that section. |
| 6 | `.claude/rules/csharp.md` § Deferred analyzer — SecurityCodeScan.VS2019 (the CS8032 paragraph) | "CS8032 is a compiler warning ... under `/p:TreatWarningsAsErrors=true` it is promoted to an error and breaks the protected nullable build." | **Verified correct and unaffected**, because `/p:TreatWarningsAsErrors=true` is retained by the corrected command. Untouched. |

Items 1 and 2 are the two lines `spec.md` designates as "verified correct and retained verbatim";
both are confirmed byte-identical in `git diff <MERGE_BASE> -- CLAUDE.md` (they appear as unchanged
context lines).

## 3. Known residual — `.csharpierignore`

`.csharpierignore` lines 1-3 carry a comment that repeats the same false premise the corrected
`CLAUDE.md` sentence removes:

```
# CSharpier formats C# source only. Generated coverage and test-result
# artifacts are committed as audit-trail evidence (not source) and must not
# be subject to formatting checks (e.g. trailing-newline rules on tool output).
```

and again at lines 9-11:

```
# Project files (*.csproj/*.props/*.targets) are owned by Visual Studio and are
# not C# source. CSharpier formats C# source only (per CLAUDE.md C#1), so exclude
# project files from the formatting check.
```

**The ignore rules themselves are correct**; only the explanatory comment repeats the false premise.
The file is outside the enumerated documentation sites, so it is **recorded here as a known
residual** and is **folded into the SD1 follow-up issue** filed by [P7-T1] / [P7-T2]. It is not
edited by this feature.

## Output Summary

AC10 is satisfied. The one factually incorrect rationale sentence adjacent to a corrected command
(`CLAUDE.md`'s "formats only `*.cs` without touching project files") is **corrected** by Block R1
against measured `csharpier check` probes of `QuickFiler\packages.config` and
`TaskMaster\Ribbon\RibbonExplorer.xml`, both `EXIT_CODE: 0`. Six further adjacent prose items are
**reviewed and recorded as verified-correct**, four of them left byte-identical and one changed only
in its embedded command string. `.csharpierignore`'s comment is recorded as a known residual folded
into the SD1 follow-up.
