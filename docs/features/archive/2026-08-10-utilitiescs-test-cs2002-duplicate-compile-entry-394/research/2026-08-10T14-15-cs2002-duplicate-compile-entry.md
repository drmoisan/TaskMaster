# Research: UtilitiesCS.Test CS2002 duplicate `<Compile Include>` entry (Issue #394)

- Feature: `utilitiescs-test-cs2002-duplicate-compile-entry-394`
- Epic: `build-ci-coverage-gate-fidelity` (wave 0, complexity band C1)
- Timestamp: 2026-08-10T14-15
- Scope: read-only research. No source file was modified while producing this artifact.

## Current State Analysis

`UtilitiesCS.Test/UtilitiesCS.Test.csproj` is a legacy non-SDK project (`ToolsVersion="15.0"`,
`packages.config`, `TargetFrameworkVersion=v4.8.1`) with 973 lines. The single `<Compile>`
`<ItemGroup>` spans lines 72-529 (452 `<Compile Include>` items). Both duplicate occurrences of
`OutlookObjects\Folder\PercentageFormatterTests.cs` were read directly and are bare, self-closing
items with no child elements, no `Condition`, no metadata:

```
304:    <Compile Include="OutlookObjects\Folder\PercentageFormatterTests.cs" />
...
356:    <Compile Include="OutlookObjects\Folder\PercentageFormatterTests.cs" />
```

They sit inside the same `<ItemGroup>` (72-529), confirming the issue's correction of the
potential entry's "two `<ItemGroup>`s" hypothesis. `PercentageFormatterTests.cs` itself
(`UtilitiesCS.Test/OutlookObjects/Folder/PercentageFormatterTests.cs`) contains a `[TestClass]`
named `PercentageFormatterTests` in namespace `UtilitiesCS.Test.OutlookObjects.Folder` with exactly
7 `[TestMethod]` members and 0 `[DataTestMethod]`/`[DataRow]`:

`FormatPercent_Zero_ReturnsZeroPercent`, `FormatPercent_One_ReturnsHundredPercent`,
`FormatPercent_TypicalValue_RoundsToWholePercent`, `FormatPercent_RoundsDownBelowMidpoint`,
`FormatPercent_AtMidpoint_RoundsAwayFromZero`, `FormatPercent_SmallMidpoint_RoundsAwayFromZero`,
`FormatPercent_Null_ReturnsEmptyString`.

The worktree used for this research (`.claude\worktrees\agent-acf594f593ecc4eac`) has **no
`packages/` directory and no `bin/`/`obj/` output** for `UtilitiesCS.Test` (confirmed via `Glob`;
zero matches for `packages/*`, `UtilitiesCS.Test/bin/Debug/*`, `UtilitiesCS.Test/obj/Debug/*`).
Any executor working in this worktree starts from a genuinely cold state and must run
`nuget restore` before any build. This is documented ground state, not a probe result requiring a
build.

## Q1 — Which occurrence should be removed

Both items are confirmed textually identical, bare, and un-conditioned (verified by direct file
read, not by inference). Because MSBuild's `<Compile>` item group is an unordered set as far as
the C# compiler is concerned (`csc.exe` does not assign semantic meaning to source-file order —
type/member resolution is order-independent), removing either occurrence produces a bit-identical
compiled assembly. The only difference between the two choices is diff size and readability.

**Recommendation: delete the second occurrence, at line 356. Keep the first, at line 304.**

Justification:
- This is the minimal-churn choice: it changes exactly one line (a pure deletion) and leaves the
  surrounding context — line 303 (`FolderSuggestionTreeStateTests.cs`) and line 305
  (`FolderProbabilityAdapterTests.cs`) at the first occurrence, and the block from line 349
  (`FolderConverterTests.cs`) through 357 (`FolderNodeViewModelTests.cs`) around the second
  occurrence — otherwise undisturbed.
- It matches the fix already recommended by the independently authored, closely related potential
  entry `docs/features/potential/promoted/2026-08-08-utilitiescs-test-duplicate-percentageformattertests-compile-entry.md`
  (promoted to a separate GitHub issue, #510; see Q6 for the cross-tracking note), which states:
  "remove the second `<Compile Include>` entry only."
- Retaining the lower line number keeps the file's existing narrative order (the block of
  `OutlookObjects\Folder\*Tests.cs` entries added first) intact, which is the more natural read for
  a future diff/blame.

The document-order evaluation reasoning in the task prompt is correct as a general MSBuild fact,
but is not what drives this recommendation — for this specific fix, document order is irrelevant
to build correctness. The reason to prefer deleting line 356 is diff minimality and consistency
with the existing recommendation on file, not any load-bearing evaluation-order effect.

## Q2 — Exact command to reproduce CS2002 (highest priority)

### Direct empirical evidence found in repo evidence artifacts

Two prior feature evidence files in this repository capture exactly the CS2002-vs-incremental-build
behavior this question asks about, on this same duplicate, using this same command family:

1. `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/baseline/phase0-baseline-msbuild-analyzers.md`
   (2026-08-08T16-08): first build after a fresh `nuget.exe restore TaskMaster.sln` (171 packages
   restored), using
   `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
   (invoked as
   `MSBuild.exe TaskMaster.sln -t:Build -p:Configuration=Debug "-p:Platform=Any CPU" -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true -nologo -v:minimal`).
   EXIT_CODE 0. Output explicitly lists "1x CS2002 duplicate-Compile-item warning in
   UtilitiesCS.Test (pre-existing, previously logged as latent/out-of-scope)". This was a genuine
   cold compile (CoreCompile ran) because there was no prior build output.

2. `docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/qa-gates/phase2-final-msbuild-analyzers.md`
   (2026-08-08T16-58): the **same command** (`/t:Build`, same flags), run later in the same
   session, against an already-built tree where `UtilitiesCS.Test` had not changed. Output states:
   "the one CS2002 UtilitiesCS.Test warning present at baseline **did not re-emit here** because
   this incremental build did not recompile that unchanged project."

This is a direct, on-repo, dated demonstration of the exact hazard flagged in the task prompt:
**`/t:Build` is not a reliable fail-before capture command for this defect.** It only surfaces
CS2002 when `UtilitiesCS.Test`'s `CoreCompile` actually runs, which depends on incremental-build
state that a planner cannot assume, and which a same-session "before" and "after" pair executed
with `/t:Build` could silently fail to distinguish (a second `/t:Build` after the first would emit
zero CS2002 lines even before the fix, producing a false "no CS2002" reading that has nothing to do
with the fix).

### Recommended reproduction command

Force a genuine recompile with `/t:Rebuild`, exactly as CI already does for its own
`TreatWarningsAsErrors` step (`.github/workflows/ci.yml` lines 103-116, which the file's own
inline comment justifies: "MSBuild's incremental up-to-date check does not invalidate on this
command-line property change alone, so a plain `/t:Build` would silently skip recompilation").

Two viable forms, both confirmed workable by direct evidence read in this repository (not by
assumption):

**A. Single project (fastest, recommended for the fail-before/post-fix pair):**

```
msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU
```

Confirmed workable: `docs/features/active/2026-08-08-wpf-dispatcher-yield-test-order-dependent-508/evidence/regression-testing/fail-before.2026-08-08T16-26.md`
built this exact project directly with `/t:Build` (EXIT_CODE 0, 6 warnings, 7.24s) and recorded a
**critical, non-obvious platform-spelling hazard**:

> "a direct csproj build requires the project-level platform name `AnyCPU`; the solution-level
> `Any CPU` spelling fails `_CheckForInvalidConfigurationAndPlatform` with 'The
> BaseOutputPath/OutputPath property is not set'. The first attempt used `Any CPU` and errored;
> the retry with `AnyCPU` succeeded."

This is the opposite convention from the solution-level command (`"/p:Platform=Any CPU"` with a
space, as used by CI and `CLAUDE.md`). **The executor must use `/p:Platform=AnyCPU` (no space)
when invoking MSBuild against the `.csproj` directly**, and `"/p:Platform=Any CPU"` (with a space)
only when invoking against `TaskMaster.sln`. Using the wrong spelling for the target produces a
build configuration error, not a CS2002-relevant result, and would corrupt a fail-before capture.

Building the single `.csproj` directly does not require the `.sln`: the project's own
`<ProjectReference>` items (`..\TaskMaster\TaskMaster.csproj`, `..\UtilitiesCS\UtilitiesCS.csproj`,
lines 910-917) let MSBuild resolve and build those dependencies transitively. This was exercised
successfully in the cited evidence file.

**B. Whole solution (matches CI/CLAUDE.md syntax exactly, more expensive):**

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```

This is CI's literal step-2 command (`.github/workflows/ci.yml:113-115`) and was run successfully
against this repository on 2026-08-08 (see Q3 below) with CS2002 present in the output.

### Restore prerequisite

`nuget restore` (or `nuget.exe restore TaskMaster.sln`) is a hard prerequisite in a fresh worktree.
This worktree currently has no `packages/` directory (`Glob` on `packages/*` returned zero
matches), so the first build attempt without restoring will fail with NuGet-missing-package errors
(`MSB3202`/similar), as separately documented in
`docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/baseline/phase0-baseline-msbuild-analyzers.md`
("the first attempt failed with MSB1008/NuGet-restore errors ... on this fresh worktree"). Restore
is solution-scoped even when only the single test project will subsequently be built, because
`packages.config` restore populates one shared `packages/` folder used by all projects' relative
`..\packages\...` HintPaths.

### Verbosity / file logger

No elevated verbosity or `/fl` file logger is required to see CS2002: it is reported by `csc.exe`
as a normal build warning line, and MSBuild always surfaces warnings/errors regardless of
`/v:` level (verbosity controls informational noise, not diagnostics). The cited evidence used
`-v:minimal` and still captured the CS2002 warning in its summary count. A file logger
(`/fl /flp:LogFile=<path>;Verbosity=normal`) is still good practice for durable evidence capture
(consistent with the way this repository's other evidence artifacts paste raw command + output),
but it is not required for the warning to appear.

### What could not be verified without a build

This researcher did not execute any build (per the read-only mandate and because no shell/Bash
tool is available to this agent in this session — only `Read`, `Grep`, `Glob`, `WebFetch`, `Write`,
`Edit`). The two evidence-file findings above are prior, dated, already-captured build outputs from
this same repository; they were not re-run in this session. The executor at Phase 0 must:
- Confirm `nuget restore` succeeds in the target worktree/branch before either build.
- Actually execute the `/t:Rebuild` command above and capture its literal output as the fail-before
  artifact (do not treat this research's citations as a substitute for a fresh capture).
- Locate `msbuild.exe` via `vswhere.exe` (per `.github/workflows/ci.yml:124-129` for
  `vstest.console.exe`, and consistent with the fixed path
  `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe` recorded
  in the two 2026-08-08 evidence files above) since `msbuild`/`vstest` are not on `PATH` in this
  environment per established repo convention.

## Q3 — Does `/p:TreatWarningsAsErrors=true` promote CS2002 to an error?

**No — confirmed by direct, dated, empirical evidence, not by inference.**

`docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/evidence/qa-gates/rebuild-warnings-as-errors.2026-08-08T17-45.md`
records an actual run of CI's exact command against this repository:

```
Command: MSBuild.exe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
EXIT_CODE: 0
Output Summary: Build succeeded. 0 Error(s), 6 Warning(s). Warnings are pre-existing and unrelated
to this remediation: 5x System.Reactive.PackagesConfigCheck.targets packages.config-migration
notices ... and 1x CS2002 duplicate-Compile-item warning in UtilitiesCS.Test/UtilitiesCS.Test.csproj
(latent, tracked out of scope per prior sessions).
```

This is a genuine full `/t:Rebuild` (forces `CoreCompile` for every project, including
`UtilitiesCS.Test`), with `/p:TreatWarningsAsErrors=true` set, exit code 0, and CS2002 explicitly
enumerated as a **warning**, not an error, in the summary. Since this run post-dates the duplicate
(which is confirmed present at the epic's base commit `edf3d34c` and multiple earlier commits back
to at least `003c5715`), this is direct proof that CS2002 is **not** currently promoted by this
repository's `TreatWarningsAsErrors` gate.

**Search for an explicit suppression mechanism (to rule out a `NoWarn`/`WarningsNotAsErrors`
explanation) found none:**
- Grepped every `*.csproj` in the repository for `NoWarn|WarningsNotAsErrors|TreatWarningsAsErrors|2002`:
  the only hits are unrelated `<!-- Issue #181 -->` comments about analyzer severities being set to
  `suggestion` in `.editorconfig`; none reference `2002` or configure `NoWarn`/`WarningsNotAsErrors`.
- No `Directory.Build.props` exists at the repository root (`Glob` returned no match).
- `.editorconfig` was grepped for `NoWarn|WarningsNotAsErrors|2002` and `dotnet_diagnostic.CS2002`:
  no matches.
- `docs/research/2026-08-10-parallel-bug-flighting-and-surface-blockers.md` was grepped for
  `CS2002|MSB3105|multiple times`: no matches (i.e., that research document does not independently
  discuss a suppression mechanism either).

**Most likely mechanism (stated with appropriate hedging — could not be independently confirmed
against Roslyn/MSBuild source in this session; a `WebFetch` attempt against the Roslyn resource
file did not return the specific diagnostic and is inconclusive):** CS2002 ("Source file specified
multiple times") is emitted by the C# compiler's command-line/source-file-list processing, before a
`Compilation` object exists — i.e., it is a compiler-*driver* diagnostic rather than a normal
semantic/syntax diagnostic produced during compilation. The `/warnaserror` (`TreatWarningsAsErrors`)
mechanism promotes diagnostics that flow through the compilation's `GeneralDiagnosticOption`
filtering; command-line diagnostics reported ahead of that filtering step are not subject to it.
This is consistent with, and was the working hypothesis already recorded in, both `issue.md`'s
Impact/Severity section and the task prompt's own framing of Q3. The empirical fact (not promoted)
is verified; the causal mechanism (why) is a reasonable, evidence-consistent, but not
independently-source-verified explanation.

**Consequence for the issue's own risk framing:** issue.md states the duplicate "would break the
build if warning-promotion rules changed," flagging sibling feature `csharp-toolchain-gate-fidelity-512`
as a live consideration. Feature 512's scope (per the epic) is to fix the `/t:Build`-vs-`/t:Rebuild`
documentation gap, the `/p:Nullable=enable` false-positive gap, and the `csharpier` command-syntax
gap in `CLAUDE.md`/`.claude/rules/csharp.md`/`.claude/skills/csharp-qa-gate/SKILL.md` — it does not
add or change any `NoWarn`/`WarningsNotAsErrors`/`TreatWarningsAsErrors` *value*, only which command
and target is documented. Since CS2002's non-promotion is structural (a compiler-driver diagnostic
outside the `/warnaserror` filtering path), **feature 512's changes would not alter this outcome**:
CS2002 would still not be promoted to an error under a corrected/documented `TreatWarningsAsErrors`
gate. The severity-inflating premise in issue.md's Impact section is not supported by evidence found
in this research; the duplicate remains a low-severity warning-noise defect regardless of 512's
outcome.

## Q4 — Duplicate sweep across the whole project file

Full item-type sweep performed by reading the complete file (see: lines 1-72 header/imports,
72-529 `<Compile>`, 530-535 `<EmbeddedResource>`, 536-548 three `<None>` item groups, 549-908
`<Reference>`, 909-918 `<ProjectReference>`, 919-930 `<BootstrapperPackage>`, 931-934 and 958-970
`<Analyzer>`/`<AdditionalFiles>`). No `PackageReference` items exist anywhere in the file (this is
a `packages.config`-style legacy project; PackageReference is not used).

| Item type | Count | Duplicates found |
|---|---|---|
| `Compile` | 452 | **1** — `OutlookObjects\Folder\PercentageFormatterTests.cs` at lines 304 and 356 (already established; confirmed by direct read) |
| `EmbeddedResource` | 1 | none |
| `None` | 7 (across three separate `<ItemGroup>`s: 5 + 1 + 1) | none — all 7 filenames distinct (`app.config`, `packages.config`, `Resources\AbstractCube.svg`, `Resources\pplkey.json`, `test.runsettings`, `Resources\AboutBox.png`, `Resources\EmailHtmlBodyWithDownloadableLinks.html`) |
| `Reference` | ~114 | none — every `Include` name (the token before the first comma) is distinct; manually enumerated the full list during the read and found no repeated assembly name |
| `ProjectReference` | 2 | none (`TaskMaster.csproj`, `UtilitiesCS.csproj`) |
| `BootstrapperPackage` | 2 | none |
| `Analyzer` | 9 (2 in one group + 7 in another) | none |
| `AdditionalFiles` | 1 | none |
| `PackageReference` | 0 | n/a (not used by this project style) |
| `packages.config` `<package>` entries | ~99 | none — read the full file; every `id` attribute is distinct |

**One adjacent anomaly found, out of this issue's scope:** the `Reference` item for `System.Linq`
(lines 842-846) carries a **duplicated child metadata element** — `<Private>True</Private>`
appears twice inside the same `<Reference>` element:

```xml
<Reference Include="System.Linq, Version=4.1.1.0, ...">
  <HintPath>..\packages\System.Linq.4.3.0\lib\net463\System.Linq.dll</HintPath>
  <Private>True</Private>
  <Private>True</Private>
</Reference>
```

This is not a duplicate `<Compile Include>` and not a duplicate `<Reference Include>` (the
`Reference` item itself is not repeated), so it is outside this issue's stated scope ("Exactly one
`<Compile Include>` item ... remains" / "sweep ... for other duplicate `<Compile Include>`
entries"). It is functionally harmless (both values are identical, so there is no ambiguity about
which wins) and does not produce a compiler warning analogous to CS2002. It is flagged here for
completeness per the acceptance criterion's instruction to sweep the whole file, but the plan
should not fix it as part of this change — doing so would violate the issue's own "remove the
duplicate item and nothing else" scope constraint and would introduce unrelated churn. If desired,
it should be raised as a separate, low-priority potential entry.

**Confirmation of the pre-established fact:** the ground-truth summary states `sort | uniq -d` over
all 452 `Compile` `Include` values yields exactly one duplicate. This research did not re-run that
shell command (no shell tool available), but the full manual read of every line in the `Compile`
`<ItemGroup>` (72-529) is consistent with that finding: only `PercentageFormatterTests.cs` was
observed twice.

## Q5 — Test-count baseline mechanics

**Test file and count:** `UtilitiesCS.Test/OutlookObjects/Folder/PercentageFormatterTests.cs`,
namespace `UtilitiesCS.Test.OutlookObjects.Folder`, class `PercentageFormatterTests`, exactly
**7** `[TestMethod]`s (confirmed by direct read; matches the pre-established ground truth). No
`[DataTestMethod]`/`[DataRow]`, so vstest will report exactly 7 discovered/executed tests for this
class both before and after the fix (the fix does not touch the `.cs` file, only the `.csproj`).

**Test assembly location:** `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll`, produced by the
`/t:Rebuild` (or `/t:Build`) command in Q2. Confirmed as the exact path used in prior evidence
(`docs/features/active/2026-08-08-wpf-dispatcher-yield-test-order-dependent-508/evidence/regression-testing/fail-before.2026-08-08T16-26.md`).

**vstest.console.exe location (confirmed, not assumed):**
`C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`
(VSTest version 18.8.0 x64, per the cited evidence file's captured run output). CI locates this
same binary dynamically via `vswhere.exe -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'`
(`.github/workflows/ci.yml:124-129`); the executor should prefer the `vswhere`-driven lookup for
portability but the fixed path above is a confirmed-working fallback on this machine as of
2026-08-08.

**Recommended selection argument:**

```
/TestCaseFilter:"FullyQualifiedName~PercentageFormatterTests"
```

The `~` (contains) operator against `FullyQualifiedName` will match all 7
`UtilitiesCS.Test.OutlookObjects.Folder.PercentageFormatterTests.FormatPercent_*` tests and no
others: a check of every class name in the `Compile` item list found no other type whose name
contains the substring `PercentageFormatterTests` (the neighboring
`FolderProbabilityAdapterTests.cs` and `FolderNodeViewModelTests.cs` do not match). This is
preferable to the `/Tests:<Name1>,<Name2>,...` form used in the cited single-test probe evidence,
because it does not require enumerating all 7 method names and is stable if a method is renamed.

**Hazard — do not glob for the assembly.** Per established repository convention (recorded
separately in project memory), a CI-style recursive `*.Test.dll` search over the full workspace
will also match stale build outputs left under `.claude\worktrees\...` from other agent sessions,
which have caused bogus `AssemblyInit` signature failures in the past. The invocation must name
`UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` explicitly (an absolute or worktree-relative
path), not a glob, exactly as the single-project fail-before evidence above already does.

**Full recommended vstest invocation:**

```
"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" `
    UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll `
    /InIsolation `
    /TestCaseFilter:"FullyQualifiedName~PercentageFormatterTests"
```

Run once against the pre-fix rebuilt assembly (expect: "Total tests: 7", all passed — CS2002 is a
build warning, not a test-discovery or execution defect, so the duplicate `<Compile>` item does not
itself change the test count; the count comparison exists to prove the csproj edit did not
accidentally remove the file from compilation) and once against the post-fix rebuilt assembly
(expect: "Total tests: 7", unchanged).

## Q6 — Risks and non-obvious hazards

- **Compilation order does not matter.** C#/`csc.exe` resolves types and members independently of
  the order in which source files are listed on the command line; removing either duplicate
  `<Compile>` item cannot change compiled output. No further verification of this point is
  necessary beyond the general fact that this is how the C# compiler works.
- **No `.sln`/filters file references item line numbers.** `TaskMaster.sln` references project
  files, not individual items inside them; there is no `.vbproj`/`.filters`/`.sln.filters`
  equivalent in this legacy C# project style that would need updating alongside a line deletion.
- **Line-ending style: CRLF, confirmed.** A `Grep` for the trailing-`\r` pattern (`\r$`) against
  the whole file matched 972 of the file's ~973 lines (the last line is a trailing blank from the
  file's final newline), confirming Windows CRLF line endings throughout. A single-line deletion
  performed as a targeted string replacement (not a full-file rewrite) will not disturb this.
- **BOM status: not conclusively determined with the tools available in this session.** A `Grep`
  for the UTF-8 BOM byte sequence (`\xEF\xBB\xBF`) at the start of the file returned no match, but
  this is inconclusive: `ripgrep` (the engine behind the `Grep` tool) detects and strips a BOM
  before pattern matching by default, so an actual BOM would also produce "no match" for this
  probe. No tool available to this agent (`Read`, `Grep`, `Glob`, `WebFetch`, `Write`, `Edit`) can
  reliably report raw leading bytes. **This does not block the fix**: the executor should perform
  the deletion as a targeted, exact-string single-line replacement (an `Edit`-style operation)
  rather than reading the whole file and writing it back, which preserves whatever encoding/BOM the
  file currently has without needing to determine it first. If the toolchain later reports an
  unexpected encoding diff, the executor should check for BOM loss as the first hypothesis.
- **Two independent tracking artifacts exist for the same underlying defect.** Beyond this issue
  (#394), `docs/features/potential/promoted/2026-08-08-utilitiescs-test-duplicate-percentageformattertests-compile-entry.md`
  documents the identical duplicate and states it was promoted to a separate GitHub issue, **#510**.
  That promoted-potential file itself notes the overlap risk explicitly: "A related potential entry
  already exists at `docs/features/potential/promoted/2026-07-20-utilitiescs-test-cs2002-duplicate-compile-entry.md`.
  Confirm whether that entry covers this same duplicate before opening new work, and consolidate
  rather than duplicating the tracking." This is out of scope for this research (which is limited
  to #394's technical reproduction and fix mechanics), but the orchestrator/planner should be aware
  that closing #394 by removing the line-356 duplicate will also resolve whatever #510 describes,
  and may want to cross-reference or close #510 as a duplicate when #394 merges, to avoid a second
  team member re-doing this same one-line fix later.
- **Direct single-project builds require `/p:Platform=AnyCPU` (no space), not `"/p:Platform=Any CPU"`
  (with a space).** This is the single most actionable hazard in this research: mixing up the two
  spellings between the solution-level and project-level invocations (see Q2) produces a build
  configuration error (`_CheckForInvalidConfigurationAndPlatform`), not useful CS2002 evidence, and
  would look like an unrelated tooling failure if not anticipated.

## Recommendations Summary

1. **Fix:** delete line 356 only (`<Compile Include="OutlookObjects\Folder\PercentageFormatterTests.cs" />`),
   keep line 304. No other line in the file should change.
2. **Fail-before / post-fix build command:**
   `msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU`
   (single project, `AnyCPU` no space), preceded by `nuget restore` if `packages\` does not already
   exist. `/t:Rebuild` is mandatory — `/t:Build` is empirically proven vacuous for this defect in
   this repository (see Q2 evidence). Locate `msbuild.exe` via `vswhere.exe` or the confirmed
   fallback path `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`.
3. **CS2002-vs-`TreatWarningsAsErrors` question:** answered — CS2002 is **not** promoted to an
   error today (direct evidence, 2026-08-08), and sibling feature 512's changes do not alter that
   outcome because the non-promotion is structural to how the diagnostic is emitted, not a
   configurable suppression. The issue's "would break the build if warning-promotion rules
   changed" framing is not supported and should be treated as a low-confidence hypothesis rather
   than a fact when the plan/spec describes severity.
4. **Duplicate sweep:** complete; no other duplicate `Compile`/`None`/`EmbeddedResource`/
   `Reference`/`ProjectReference`/`Analyzer`/`AdditionalFiles`/`BootstrapperPackage`/
   `packages.config` entries exist. One out-of-scope, functionally-harmless duplicate child
   element (`<Private>True</Private>` twice on the `System.Linq` `Reference`) was found and should
   not be fixed in this change.
5. **Test-count evidence:**
   `"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~PercentageFormatterTests"`,
   expected "Total tests: 7" before and after.
