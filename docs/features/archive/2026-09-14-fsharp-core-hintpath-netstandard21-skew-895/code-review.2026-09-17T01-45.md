# Code Review — Issue #895 (FSharp.Core HintPath netstandard2.1 skew)

- **Issue:** #895
- **Branch:** `bug/fsharp-core-hintpath-netstandard21-skew-895` at `7b865c4d`
- **Base:** `origin/main` at `91746d2e` (after explicit fetch; equals the merge base)
- **Reviewer:** feature-review agent
- **Timestamp:** 2026-09-17T01-45
- **Files reviewed:** 7 source paths (4 project files, 2 new test files, 1 test file comment-only edit); 0 production `.cs` files
- **Disposition:** **APPROVE.** 0 blocking findings, 0 medium findings, 4 low findings, 4 informational notes.

Verification note: every claim below was re-derived on the execution worktree with git, grep, file hashing,
a direct `PEReader` read of the package and deployed binaries, and direct reads of the on-disk TRX and build
logs, unless labelled evidence-attested.

---

## Executive Summary

The change is small, exact and well-evidenced. The fix itself is three one-segment edits inside existing
`<HintPath>` elements (`-U0` hunks `@@ -52 +52 @@`, `@@ -259 +259 @@`, `@@ -42 +42 @@`), each replacing
`netstandard2.1` with `netstandard2.0` and touching no other character. The two new test classes are
read-only, deterministic, parallel-safe, and detect the defect at the two layers where it exists: the source
(HintPath values) and the deployed binaries (the `netstandard` assembly reference of each copied
`FSharp.Core.dll`). Both were observed failing on the unfixed tree in exactly the rows the defect statement
predicts, and passing after the fix. The comment-only correction keeps an existing XML `<remarks>` block
truthful after the fix.

The three caller-flagged items were assessed:

- **(a)** P3-T2 / P4-T15 acceptance literals (22 / `13 9` / 9) were a prediction error in the plan's R1
  preflight arithmetic; measured 18 / `11 7` / 7 is correct and re-derived here. The substantive claim
  (comment-only) holds: zero non-`///` changed lines.
- **(b)** `CSC_OUT=2` is one `csc.exe` command line plus one `BuildResponseFile =` echo per project;
  re-derived for all fifteen projects in both build logs. One compiler invocation per project.
- **(c)** Projecting offenders into the `because` argument of `BeEmpty` is sound test design and does not
  weaken the assertion.

Nothing in this branch changes production behaviour at runtime; the change alters which binary is copied
at build time, and both flavours expose the same assembly identity and public surface.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Low | `TaskMaster.Test/Bootstrap/FSharpCoreDeployedIdentityTests.cs` | :180-192 | `NotContain(Netstandard21)` is evaluated over the versions of *all* referenced assemblies, not only the `netstandard` reference. A future FSharp.Core build that referenced some other assembly at 2.1.0.0 would fail this row for an unrelated reason. | Optional: filter by `reference.Name == NetstandardAssemblyName` before `NotContain`, or leave as-is since it mirrors the spec text "contains no reference at version 2.1.0.0". | The first assertion (`ContainSingle().Which.Should().Be(2.0.0.0)`) already fully decides the `netstandard` question; the second is a belt-and-braces check whose breadth is slightly wider than its name suggests. | Re-derived: the netstandard2.0 flavour references only `netstandard=2.0.0.0` and `System.Runtime.Numerics=4.0.1.0`, so the assertion cannot false-positive today. |
| Low | `TaskMaster.Test/Bootstrap/FSharpCoreDeployedIdentityTests.cs` | :94 | `[DataTestMethod]` is the legacy attribute spelling; MSTest 3+ treats `[TestMethod]` with `[DataRow]` as canonical. | Optional tidy-up in a later touch of the file; not worth a toolchain restart on this branch. | Consistency with newer MSTest guidance. | Analyzer gate produced zero diagnostics, so no rule currently flags it. |
| Low | `TaskMaster.Test/Bootstrap/FSharpCoreHintPathAlignmentTests.cs` | :101 | The directory walk skips every name beginning with `.`, which is a superset of the AC1 literal list (`.git`, `.claude`) and also excludes `.dotnet-sdk`, `.vs`, `.github`. | None required. Keep; the method's `<remarks>` (:76-80) explains the intent. | A project file under a dot-prefixed directory would be invisible to Shape A; none exists, and the count-of-six test would expose an over-exclusion rather than hide it. | Re-derived: `CSPROJ_FILES=18` unfiltered equals filtered in the P0-T9 census; grep census finds exactly six FSharp.Core HintPaths. |
| Low | `TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs` | whole file | 470 of 500 lines after the comment-only edit (466 at base). | Plan a split (for example, move `ChildDomainBindProbe` helpers or the negative-control tests) before the next substantive edit. | 30 lines of headroom under the repository's hard 500-line cap. | Re-derived by `wc -l`. |
| Info | `TaskMaster.Test/Bootstrap/FSharpCoreHintPathAlignmentTests.cs` | :196-209 | Item (c): offenders are projected into the `because` argument of `BeEmpty`. | None; this is the correct design. | `BeEmpty` fails iff the collection is non-empty regardless of the reason text; FluentAssertions' renderer names only one representative element, so the projection is what makes the message name every misaligned file. `string.Join` over an empty list on the passing path is side-effect free. | The P1-T6 TRX `<Message>` on disk lists all three offenders in the reason and one in the `{...}` segment. |
| Info | `TaskMaster.Test/Bootstrap/FSharpCoreDeployedIdentityTests.cs` | :49-56, :209-243 | Shape B depends on a completed whole-solution build and a restored `packages/` directory; on a partial or absent build it throws `InvalidOperationException` naming the path rather than skipping. | None; fail-loud is the right behaviour and matches the #879 harness convention. | A silent skip would make fifteen rows vacuous. CI builds the full solution before running the coverage runner. | `.github/workflows/_mstest-coverage.yml:82` (`msbuild … /t:Build` on the solution) precedes `:94` (`Invoke-MSTestWithCoverage.ps1`). |
| Info | `TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs` | :22, :30 | `[DoNotParallelize]` count is 2, identical at base (placed by #879). Nothing was added or removed by this change; the two new classes carry none. | None. | Constraint block 1 (no serialisation added) is satisfied. | Re-derived by grep; `post-format-sweep` records 0, 0, 2. |
| Info | `QuickFiler/QuickFiler.csproj`, `QuickFiler.Test/QuickFiler.Test.csproj`, `ToDoModel/ToDoModel.csproj` | :52, :259, :42 | Each file differs from `origin/main` on exactly one line, the HintPath value; the three already-correct HintPaths (`UtilitiesCS`, `UtilitiesCS.Test`, `ToDoModel.Test`) are byte-identical to base. No `<Analyzer Include>` line was touched (issue #898 remains separate). | None. | AC3 as worded. | Re-derived: six-file numstat prints exactly three `1 1` lines. |

## 1. Correctness of the fix

### 1.1 The mechanism is the right one

Both package flavours carry the assembly identity `FSharp.Core, Version=11.0.0.0, Culture=neutral,
PublicKeyToken=b03f5f7f11d50a3a`, so ResolveAssemblyReferences cannot distinguish them and the copy that
reaches a transitive output directory is whichever project's `_CopyFilesMarkedCopyLocal` ran last. Read
directly with `PEReader` on this worktree: the `lib/netstandard2.0` binary references `netstandard 2.0.0.0`
and the `lib/netstandard2.1` binary references `netstandard 2.1.0.0`. Only the former identity exists on
.NET Framework 4.8.1, so aligning all six HintPaths on `netstandard2.0` removes the unloadable flavour from
every input the build can copy. No binding redirect, app.config or packages.config change is needed because
all fifteen `FSharp.Core` redirects are on the assembly-version axis, which both flavours share; re-derived
by confirming none of those files is in the diff.

### 1.2 The post-fix state was independently confirmed, not inferred

Without running any test, this reviewer hashed every deployed `FSharp.Core.dll` in the fifteen enumerated
`bin/Debug` directories against the two package binaries: all fifteen match the netstandard2.0 SHA-256 and
none matches the netstandard2.1 SHA-256. `SVGControl`, `SVGControl.Test` and `VBFunctions` hold no copy, as
the research predicted. This is the same property Shape B asserts, established by a different mechanism
(content hash rather than metadata read).

### 1.3 Detection moved to the right layer

Before this change, nothing detected the skew: the #879 probes were green on an unfixed tree because they
were rooted at a directory that happened to receive the netstandard2.0 copy. Shape A now fails at the
project file with no build, and Shape B fails at each deployed binary after a build. Neither suffices alone
(Shape A cannot see what the build deployed to the six flip-capable directories; Shape B cannot run without
a build), and the spec says so explicitly.

## 2. Test design

### 2.1 `FSharpCoreHintPathAlignmentTests` (Shape A)

- `FindRepositoryRoot` (:47-69) walks up from `AppDomain.CurrentDomain.BaseDirectory` to the first
  directory holding `TaskMaster.sln` and throws with the start directory when none is found. From a nested
  agent worktree the first hit is that worktree's own root, which is the intended behaviour.
- `DiscoverFSharpCoreHintPaths` (:83-138) is an explicit stack-based walk with the dot-prefix and named
  exclusions, `XDocument.Load` per project file, `LocalName == "HintPath"` matching (namespace-agnostic,
  correct for both old-style and SDK-style project XML), and ordinal ordering so results do not depend on
  enumeration order.
- The count test (:154-172) is what makes the flavour test non-vacuous, and the flavour test (:178-210)
  is what makes the count test meaningful. Both are needed and both exist.
- `ExpectedHintPathSuffix` uses backslashes (`@"\lib\netstandard2.0\FSharp.Core.dll"`), matching every
  HintPath in the tree; a forward-slash HintPath would be reported as an offender, which is a conservative
  failure rather than a silent pass.

### 2.2 `FSharpCoreDeployedIdentityTests` (Shape B)

- `ReadAssemblyReferences` (:44-72) opens a read-only `FileStream` with `FileShare.Read`, wraps it in
  `PEReader`, and iterates `MetadataReader.AssemblyReferences`. No assembly is loaded into any AppDomain,
  which is what allows fifteen same-identity files to be inspected in one process. Both `using`s are
  correct; `PEReader` takes ownership of the stream by default and double-dispose is safe.
- Fifteen literal `[DataRow]` entries with bracketed `DisplayName`s mean a failure names its directory and
  `[QuickFiler]` is distinguishable from `[QuickFiler.Test]` in a TRX (the expect-fail evidence matched with
  ordinal `Contains` for exactly this reason).
- The positive control (:209-243) derives the `lib/netstandard2.1` path from the first discovered
  HintPath rather than hard-coding the package version, so a package bump does not silently disable the
  control.

### 2.3 Parallelism and determinism

Both classes are read-only over files, hold no static mutable state, use no clock, RNG, process,
AppDomain, temporary file or sleep, and carry no `[DoNotParallelize]`. They ran under the unchanged
`Workers=0` / `ClassLevel` runsettings in both the scoped runs and the 7311-test full suite with zero
failures. Constraint block 1 is satisfied.

## 3. Comment-only correction

`NetstandardBindChildDomainTests.cs:363-375`: the `<remarks>` on `ProbeApplicationBase` no longer asserts
that the `QuickFiler.Test` output directory deploys the 2.1.0.0-referencing flavour. It now describes the
directory as the historically failing root, states the post-fix invariant, and names the display-name tests
as the carriers of discriminating power. Re-derived: every changed line begins with `///`; the assertion
string at line 206 is untouched; `unsatisfiable` occurrences dropped from 2 to 1 (the survivor at line 281
remains true because that test binds the display name directly).

## 4. Project-file edits

Three files: one line each, value only. `TaskMaster.Test.csproj`: two `Compile Include` insertions placed
directly after `Bootstrap\AddInEagerInstallShapeTests.cs`, preserving indentation and order; the project
lists sources explicitly, so without this registration the two classes would not exist. CSharpier does not
reach project files (`.csharpierignore` excludes `*.csproj`), so the four-space/CRLF preservation was the
editor's responsibility and the `-U0` hunks confirm no collateral change.

## 5. Evidence quality

The 28 committed evidence artifacts follow the plan's fixed token and the evidence schema (`Timestamp:`,
`Command:`, `EXIT_CODE:`, `ExpectedExitCode:`). Two artifacts and the loop-closure record carry explicit
`NOT AS WRITTEN` / deviation sections instead of silently restating the plan literals; that transparency is
what makes rulings (a) and (b) possible. No absolute host path, account name or machine name appears in any
committed artifact (grep over the full diff).

## 6. Residual risks and follow-ups (not blocking)

1. `ToDoModel.Test/packages.config` lacks `FSharp.Core` and `Deedle` entries although its project file
   carries HintPaths for both (verified on disk). On the next bump of either package that project would
   fail with CS0006. Should be filed as its own issue.
2. `scripts/vscode/Sync-PackageReferences.ps1:13-19` ranks `netstandard2.1` ahead of `netstandard2.0`
   (verified on disk). Shape A would catch a reintroduced skew, but the order is inverted for net481 and
   should be corrected with Pester coverage under its own issue.
3. Issue #898 (Meziantou analyzer HintPath skew) was worked around at P0-T3 by materialising 3.0.203 into
   the git-ignored `packages/` directory; no project file was touched (re-derived from the diff).

## 7. Disposition

**APPROVE.** The change is correct, minimal, fully evidenced and independently confirmed. No code change
is requested on this branch; the four low findings are optional tidy-ups for a later touch of the files.
