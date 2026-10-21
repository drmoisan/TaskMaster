# Issue Update Mirror — Issue #895

Timestamp: 2026-09-17T01-32
Task: [P5-T7]
WORKTREE-LEAF: agent-a8bc4dc5978785885

PostedAs: unknown

Posting is outside the scope of this execution run. The text below is the exact text intended for
issue #895; it has not been posted, and no GitHub URL or `IssueUpdatedAt` value exists yet.

EXIT_CODE: 0
ExpectedExitCode: 0

---

## Intended text

### Fix

All six `FSharp.Core` `HintPath` entries in the solution now select the `lib\netstandard2.0`
flavour of `FSharp.Core.11.0.100`. Three one-line value edits were made, and nothing else in those
files changed:

- `QuickFiler/QuickFiler.csproj` line 52
- `QuickFiler.Test/QuickFiler.Test.csproj` line 259
- `ToDoModel/ToDoModel.csproj` line 42

The three already-correct entries in `UtilitiesCS/UtilitiesCS.csproj`,
`UtilitiesCS.Test/UtilitiesCS.Test.csproj` and `ToDoModel.Test/ToDoModel.Test.csproj` are
byte-identical to `origin/main`. No `app.config`, `packages.config`, `.props`, `.targets`,
runsettings or `coverage.config` file changed, and the #879 `netstandard` redirect in
`TaskMaster/app.config` is untouched.

Because the netstandard2.1 flavour's own assembly-reference table names `netstandard,
Version=2.1.0.0`, an identity that does not exist on .NET Framework, any output directory that
received that copy held an unloadable assembly. All six `Reference` elements carry the identical
identity `FSharp.Core, Version=11.0.0.0`, so ResolveAssemblyReferences saw no conflict and which
flavour reached a transitive output directory was last-writer-wins build ordering. Aligning all six
removes the unloadable flavour from every path the repository controls.

### Regression tests

Two new MSTest classes were added under `TaskMaster.Test/Bootstrap/`, registered by two
`<Compile Include>` items in `TaskMaster.Test/TaskMaster.Test.csproj`. Both were observed failing on
the unfixed tree before the fix was applied, and passing after.

**Shape A, static assertion over source** — `FSharpCoreHintPathAlignmentTests`, with
`SolutionHasExactlySixFSharpCoreHintPaths` and `EveryFSharpCoreHintPath_SelectsNetstandard20`. It
enumerates every project file from the repository root, skipping directories whose name begins with
a dot (which excludes the nested agent worktrees) and `packages`, `bin`, `obj` and `node_modules`.
It asserts both the count of six and the flavour, so a tree with all six moved to netstandard2.1
would fail rather than pass on mere agreement.

- Observed failing: `evidence/regression-testing/expect-fail-shape-a.2026-09-16T23-27.md` — the
  count test passed with six while the flavour test failed naming exactly the three misaligned
  project files and none of the three correct ones.
- Observed passing: `evidence/regression-testing/pass-after-shape-a.2026-09-16T23-27.md` — 2 of 2.

**Shape B, deployed-binary assertion** — `FSharpCoreDeployedIdentityTests`, with
`DeployedFSharpCore_ReferencesNetstandard20` carrying one literal data row per enumerated output
directory (fifteen rows, each with a bracketed display name so a failure names the directory) and
the positive control `Detector_OnPackageNetstandard21Binary_Reports21`. It reads each deployed
`FSharp.Core.dll` with `PEReader` and `MetadataReader` and asserts the `netstandard` reference is
2.0.0.0 with no 2.1.0.0 reference anywhere. No assembly is loaded into any application domain,
which is why fifteen same-identity files can be read in one process. The control reads the package's
own netstandard2.1 binary and asserts 2.1.0.0, so a reader that could not see a 2.1.0.0 reference at
all cannot pass every directory vacuously.

- Observed failing: `evidence/regression-testing/expect-fail-shape-b.2026-09-16T23-27.md` — the
  `[QuickFiler]`, `[QuickFiler.Test]` and `[ToDoModel]` rows failed, each naming 2.1.0.0, while the
  positive control passed. No additional rows failed on that build.
- Observed passing: `evidence/regression-testing/pass-after-shape-b.2026-09-16T23-27.md` — 16 of 16.

Both expect-fail and pass-after Shape B runs followed a whole-solution `/t:Rebuild` whose log
contained no `Skipping target "CoreCompile"` line for any project, so neither run read stale output.

Neither class carries `[DoNotParallelize]`; both are read-only over files, hold no shared mutable
state and use no clock, RNG, process, application domain or temporary file. They ran under the
standard `Workers=0` / `ClassLevel` runsettings, unchanged.

### Comment-only correction

The `<remarks>` block on `ProbeApplicationBase` in
`TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs` previously stated as present fact
that the `QuickFiler.Test` output directory deploys the flavour referencing the unsatisfiable
`netstandard 2.1.0.0` identity. That is false after this fix. The block now describes the post-fix
state, retains the directory as the historically failing root, and records that the discriminating
power of the class has moved to the display-name tests. Every changed line in that file is an XML
documentation comment line; no test method, assertion, attribute, constant, or the assertion message
at lines 206-207, changed. Recorded at
`evidence/other/scope-boundary-diff.2026-09-16T23-27.md` under the `AC5 Comment-Only Diff:` heading.

Related expected side effect, recorded rather than fixed:
`AfterInstall_DeedleTypeInitializerSucceeds` in the #879 harness now passes with or without the #879
installer, because `QuickFiler.Test` deploys the netstandard2.0 flavour and the Deedle invocation no
longer requests the 2.1.0.0 identity. The #879 negative control
`NegativeControl_WithoutInstall_Netstandard21Throws` binds that display name directly and is
unaffected; it was observed passing after the fix.

### Toolchain

Full C# loop, run in order and completed in a single clean pass:

- CSharpier check: exit 0, `Checked 1641 files in 5450ms.`, no file rewritten.
- Analyzer rebuild: exit 0, 0 warnings, 0 errors, no skipped `CoreCompile`.
- Nullable rebuild: exit 0, 0 warnings, 0 errors, no skipped `CoreCompile`.
- Tests with coverage: exit 0, 7311 total, 7311 passed, 0 failed. Repository-wide line rate
  0.858678 over 65616 valid lines, against a pre-change baseline of 0.858708 over the same 65616.
  No production `.cs` file changed, so there is no changed-line coverage obligation to measure.

### Acceptance Criteria Status
- Source: docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/spec.md
- Total AC items: 5
- Checked off (delivered): 5
- Remaining (unchecked): 0
- Items remaining: none

### Follow-ups, not opened by this change

1. `ToDoModel.Test/packages.config` carries no `FSharp.Core` and no `Deedle` entry although
   `ToDoModel.Test/ToDoModel.Test.csproj` lines 92-96 carry HintPaths for both. The next version
   bump of either package would leave that project pointing at a folder that no longer exists.
2. `scripts/vscode/Sync-PackageReferences.ps1` lines 13-19 rank `netstandard2.1` ahead of
   `netstandard2.0` in its TFM fallback search, which is inverted for a `net481` project. There is
   no in-scope trigger now that all six HintPaths resolve, and Shape A would fail if the skew were
   reintroduced, but the ordering is worth correcting with accompanying Pester coverage.

---

## Acceptance

- The artifact exists and carries `Timestamp:` and `PostedAs: unknown`: yes.
- It carries the five evidence paths by filename: `expect-fail-shape-a`, `expect-fail-shape-b`,
  `pass-after-shape-a`, `pass-after-shape-b` and `scope-boundary-diff`: yes, all five appear in the
  intended text above.
