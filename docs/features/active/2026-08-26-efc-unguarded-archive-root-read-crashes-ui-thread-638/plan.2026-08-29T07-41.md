# 2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread (Plan)

- **Issue:** #638
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-29T11-09
- **Status:** Executed
- **Version:** 1.0
- **Work Mode:** `full-bug`

**Acceptance-criteria source.** Work Mode for issue #638 is `full-bug`, so
`docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/spec.md`
is the sole acceptance-criteria source, per `.claude/skills/acceptance-criteria-tracking/SKILL.md`.
No `user-story.md` exists for #638 and none is to be created.
`docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/issue.md`
is background only; two of its claims are false at this branch head and are corrected in the spec's
Context section.

**Fail-closed evidence rule.** Every baseline command step, every final-QC command step, and every
coverage comparison below names its artifact path. A missing or field-incomplete artifact makes the
corresponding checklist item unchecked and the outcome BLOCKED or INCOMPLETE, never PASS.

**Evidence schema.** Every artifact named below carries `Timestamp:`, `Command:`, `EXIT_CODE:` and
`Output Summary:` per `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. Timestamps use
`yyyy-MM-ddTHH-mm`. Artifact file names are fixed by this plan (no timestamp in the file name) so the
change footprint is enumerable in advance; the ISO-8601 timestamp lives in the `Timestamp:` field.
No artifact this plan writes may contain an absolute filesystem path, an account name or a machine
name, because [P9-T1] commits all of them. This applies in particular to recorded vstest failure
output, whose Debug stack traces carry absolute source paths, and to [P7-T2] in `raw` mode, where the
Cobertura `filename` attribute is absolute: record the exception type and the test's fully qualified
name rather than the stack trace, and record worktree-relative paths computed as
`$_.FullName.Substring((Get-Location).Path.Length)`. The `Command:` field of every artifact whose
command runs a vswhere-resolved executable — [P0-T10], [P0-T11], [P0-T12], [P2-T3], [P3-T14],
[P3-T15], [P4-T6], [P5-T1], [P5-T2], [P6-T3], [P6-T4] and [P6-T5] — records the unresolved
`& "${env:ProgramFiles(x86)}\...\vswhere.exe" ... | Select-Object -First 1` expression together with
the arguments passed to the resolved tool, never the resolved MSBuild or `vstest.console.exe` path,
which is absolute. The clause in [P6-T5] is a specific instance of this rule, not a second rule.

**Evidence location normalization.**
`EVIDENCE_LOCATION_OVERRIDE_REJECTED: docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/coverage/ replaced with docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/ and docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/`.
The spec named an `evidence/coverage/` sub-path in its Repro & Evidence section and in AC17 before
this substitution was applied.
`evidence/coverage/` is not one of the canonical evidence sub-paths enumerated by
`.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. Baseline coverage evidence is written
to `evidence/baseline/` and post-change coverage evidence to `evidence/qa-gates/`. The planner applied
the two spec substitutions during preflight, so `spec.md` already names the canonical sub-paths and
AC17 is satisfiable as written. The planner separately replaced the absolute worktree path in the
spec's Context provenance sentence with a description naming only the branch and the merge-base SHA,
because [P9-T1] commits this feature folder with the
`docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638` pathspec, so a
deferred substitution task ordered after that commit could not prevent the account name from entering
history. Task [P8-T1] verifies both states; it edits no criterion text.

**Base ref.** All anchored diffs in this plan use the merge base
`ecdb1c84ba8541ab67042985919cfed4df768c01`, the `origin/main` commit this worktree branched from.

**Scope decisions already settled by the orchestrator; do not reopen.**

- **D1.** New tests live in `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs`, not in a
  `tests/` mirror tree. `.claude/skills/policy-compliance-order/SKILL.md` ranks `CLAUDE.md` above
  `.claude/rules/general-unit-test.md`; the unit-test policies embedded in `CLAUDE.md` impose no
  `tests/` mirroring requirement; and the General Code Change Policy requires matching existing
  repository style, which for every C# test project here is a sibling `<Project>.Test` project. The
  new file must be registered with an explicit `<Compile Include=... />` entry in
  `QuickFiler.Test/QuickFiler.Test.csproj`, a legacy non-SDK project.
- **D2.** Plan against the acceptance criteria as restated in `spec.md`, not against the issue body.
- **D3.** Scope is the three unguarded reads plus their regression tests. The three non-goals
  recorded in the spec must not be swept in.

**Out of scope for this plan.** Atomic execution reporting, pull-request authoring, CI monitoring and
feature review are performed later by a separate orchestrator. This plan contains no task that
creates a pull request or polls CI.

## Change Footprint

Paths this plan's diff will create or modify:

- `QuickFiler/Controllers/EfcDataModel.cs`
- `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs`
- `QuickFiler.Test/QuickFiler.Test.csproj`
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/spec.md`
  (already edited at plan-authoring time: the planner replaced the two non-canonical coverage evidence
  sub-path references, replaced the absolute worktree path in the Context section's provenance sentence
  with a branch-and-merge-base description, and appended a dated Correction Log entry for each of those
  two substitutions. The executor's remaining edits to
  this file are the AC check-offs in Phase 8 and the header fields in [P8-T23]. The file therefore
  appears in the [P9-T2] footprint check whether or not any Phase 8 check-off is applied.)
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/plan.2026-08-29T07-41.md`
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/issue.md`
  (untracked at plan-authoring time; added to the diff by [P9-T1]'s feature-folder pathspec, unmodified)
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/research/2026-08-29T08-05-archive-root-guard-research.md`
  (untracked at plan-authoring time; added to the diff by [P9-T1]'s feature-folder pathspec, unmodified)
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/phase0-instructions-read.md`
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/p0-t6-dotnet-tool-restore.md`
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/p0-t7-solution-restore.md`
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/p0-t8-dotnet-coverage-probe.md`
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/p0-t9-csharpier-check.md`
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/p0-t10-msbuild-analyzers.md`
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/p0-t11-msbuild-nullable.md`
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/p0-t12-vstest-coverage.md`
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/p0-t12-direct-harness-baseline.md`
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/p1-t4-tree-facts.md`
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/other/p2-t3-seam-compile.md`
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/other/p3-t14-tests-compile.md`
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/regression-testing/p3-t15-regression-fail-before.md`
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/other/p4-t6-fix-compile.md`
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/other/p4-t7-file-size.md`
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/regression-testing/p5-t1-regression-pass-after.md`
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/regression-testing/p5-t2-sentinel-tests.md`
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/other/p5-t3-untouched-tests.md`
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p6-t1-csharpier-format.md`
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p6-t2-csharpier-check.md`
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p6-t3-msbuild-analyzers.md`
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p6-t4-msbuild-nullable.md`
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p6-t5-vstest-coverage.md`
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p6-t6-loop-closure.md`
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p7-t1-coverage-postchange.md`
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p7-t2-coverage-changed-lines.md`
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p7-t3-coverage-delta.md`
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p7-t4-canonical-coverage-artifact.md`
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/other/p8-t2-followup-issue-dossier.md`
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p9-t2-change-footprint.md`
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p9-t3-clean-tree.md`

Paths written by this plan that are **not** part of the diff because they are gitignored:
`artifacts/csharp/coverage.xml` (`.gitignore:57` ignores `artifacts/`), `coverage/coverage.cobertura.xml`
(`.gitignore:144` ignores `coverage/*`), the `TestResults` run directories
(`.gitignore:39` ignores `[Tt]est[Rr]esult*/`), the MSBuild transcripts written to
`TestResults\msbuild\*.log` by [P0-T10], [P0-T11], [P2-T3], [P6-T3] and [P6-T4] (same `.gitignore:39`
rule), and any NuGet package directory materialized under `packages/` by [P0-T7]
(`.gitignore:191` ignores `**/[Pp]ackages/*`).

No other production file is modified. In particular `QuickFiler/Controllers/EfcFormController.cs`,
`QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs`, `TaskMaster/AppGlobals/AppOlObjects.cs`,
`TaskMaster/AppGlobals/ArchiveRootPathGuard.cs`, `QuickFiler/Controllers/EfcSelectionGuard.cs` and
`UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs` are read-only citations here and must remain
untouched.

## Verified facts this plan is built on

Each was re-derived against the working tree of this worktree during plan authoring:

- `QuickFiler/Controllers/EfcDataModel.cs:289`, `:310` and `:328` each contain
  `OlAncestor = Globals.Ol.ArchiveRootPath,` inside an `EmailFilerConfig` object initializer, in
  `MoveToFolderAsync(string, bool, bool, bool, bool)` (`:259-297`), `OpenOlFolderAsync(string)`
  (`:299-316`) and `OpenFsFolderAsync(string)` (`:318-334`) respectively.
- The OneDrive `SpecialFolders` guards sit at `QuickFiler/Controllers/EfcDataModel.cs:277-281`,
  `:301-304` and `:320-323`, each strictly above the archive-root read in its own method.
- The `MailInfo is null` guard is the first statement of `MoveToFolderAsync` at
  `QuickFiler/Controllers/EfcDataModel.cs:267-270`.
- `QuickFiler/Controllers/EfcDataModel.cs` is 423 lines. The static log4net logger is declared at
  `:23-25`, `System.Windows.Forms` is imported at `:10`, and `MessageBox.Show` is already used at
  `:355`.
- `QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs:217` asserts
  `probe.FileSystem.SpecialFoldersAccessCount.Should().Be(2)` inside
  `OpenFolderMethods_DelegateToDataModelWithoutExternalServices` (`:207-218`). The counter is
  incremented by the `SpecialFolders` getter at `:416-423`, declared at `:414`.
  `FakeApplicationGlobals.Ol` returns `null` at `:388`.
- `QuickFiler.Test/QuickFiler.Test.csproj:114-115` registers
  `<Compile Include="Controllers\EfcDataModelIssue614Tests.cs" />` and
  `<Compile Include="Controllers\EfcDataModelTests.cs" />`.
- `QuickFiler/Properties/AssemblyInfo.cs:5` is `[assembly: InternalsVisibleTo("QuickFiler.Test")]`.
- `UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs:15` is `string ArchiveRootPath { get; }`.
- `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs:13-14` declares `UnresolvableRule` and `:16-17`
  declares `CrossStoreRule`; they are thrown at `:44` and `:56`. The class is `internal` in namespace
  `TaskMaster`, so `QuickFiler.Test` cannot reference the constants; the new test file declares its
  own private `const string` copies of both texts.
- `QuickFiler.Test/Controllers/EfcHomeControllerExecuteMovesTests.cs:160-175` is
  `HandleMoveResult_WhenMoveFails_RoutesMessageThroughInjectedAction`, asserting the exact string
  `Cannot move to folderpath Archive/Target` at `:174`.
- `QuickFiler.Test/Controllers/EfcDataModelTests.cs:220-228` is the private `CreateGlobals()` helper
  building a strict `Mock<IApplicationGlobals>` over a strict `Mock<IOlObjects>`; it does **not** set
  up `FS`, so the new test file needs its own globals builder that also stubs
  `IApplicationGlobals.FS` and `IFileSystemFolderPaths.SpecialFolders`.
- `QuickFiler/Controllers/EfcDataModel.cs:235-253` is `TryGetFirstInSelection`, whose
  `catch (System.Exception)` at `:249` returns `null`. A strict `Mock<IOlObjects>` with no `App`
  setup therefore yields `Mail == null` and `MailInfo == null` from the public constructor, without
  throwing.
- `.github/workflows/_mstest-coverage.yml:83` is
  `& $vstestPath $testAssemblies /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`.
- `.github/workflows/_format-check.yml:37` runs `dotnet tool restore` and `:41` runs
  `dotnet csharpier check .`. `dotnet-tools.json:6` pins CSharpier to `1.2.6`.
- `.csharpierignore:4` excludes `**/evidence/**` and `:12` excludes `*.csproj`, so neither the
  evidence artifacts nor the `.csproj` edit is subject to the format gate.
- `scripts/vscode/Invoke-MSTestWithCoverage.ps1:76` appends `/InIsolation` and
  `/TestCaseFilter:TestCategory!=LiveOutlook` to its inner vstest invocation, and `:341` calls
  `Assert-CoberturaLineCoverageThreshold` before `:343` writes the post-processed file.
  `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:487-490` throws when the repo-wide line
  percentage is below 80.
- `.claude/hooks/validate-feature-review-coverage.ps1:253` reads `artifacts/csharp/coverage.xml`
  through `Get-JacocoRepoCoverage`, which at `:229` selects `//counter[@type="LINE"]` and sums the
  `missed` and `covered` attributes; `:313` fails when the repo-wide figure is below `85.0`. The file
  must therefore be JaCoCo-shaped, not Cobertura-shaped.
- This worktree has no `packages/` directory, and `QuickFiler.Test/QuickFiler.Test.csproj:499-503`
  names `Meziantou.Analyzer.3.0.156` and `Roslynator.Analyzers.4.16.0` in `<Analyzer Include>` items
  while `QuickFiler.Test/packages.config` pins `Meziantou.Analyzer` to `3.0.174` (`:10-15`) and
  `Roslynator.Analyzers` to `4.16.1` (`:139-144`). Until that skew is resolved, every `/t:Rebuild`
  fails with `error CS0006`, which would make [P2-T3], [P3-T14], [P4-T6], [P6-T3], [P6-T4], [P6-T5],
  [P0-T12] and [P7-T1] unsatisfiable. [P0-T7] detects and resolves it. `.gitignore:191` ignores
  `**/[Pp]ackages/*`, so the remediation adds nothing to the diff.
- A non-null `MailInfo` is reachable without an Outlook COM fixture only through the harness described
  in [P3-T1]. `QuickFiler/Controllers/EfcDataModel.cs:233` defines `MailInfo` as
  `ConversationResolver?.MailHelper`; the two-argument `ConversationResolver` constructor
  (`QuickFiler/Helper Classes/ConversationResolver.cs:64-68`) does no work; `MailHelper` is publicly
  settable at `QuickFiler/Helper Classes/ConversationResolver.cs:282-286`; and the parameterless
  `MailItemHelper` (`UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs:80-84`) runs
  `InitializeSafeDefaults` (`:167`), which sets `_folderInfo = null` (`:177`), so
  `MailItemHelper.FolderInfo` (`UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Properties.cs:78-82`)
  is `null`. The `ConversationResolver` constructor declared at
  `QuickFiler/Helper Classes/ConversationResolver.cs:70-84` — five parameters, the fifth defaulted,
  which production invokes with four arguments at `QuickFiler/Controllers/EfcDataModel.cs:67` — must
  not be used, because the helper construction inside it at
  `QuickFiler/Helper Classes/ConversationResolver.cs:82`
  installs a `_folderInfo` factory (`UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs:106-111`)
  whose materialization calls `ResolveFolderRoot`
  (`UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Loading.cs:122-130`), reading
  `ArchiveRootPath` a second time at `:124`.
- `scripts/vscode/Invoke-Restore.ps1:27` resolves MSBuild with
  `& $vswherePath -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1`.
  Every vswhere `-find` invocation in this plan carries the same `| Select-Object -First 1` suffix,
  because `-find` can emit more than one matching path.
- `QuickFiler.Test/QuickFiler.Test.csproj:18` is `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>`
  and the project sets no `<LangVersion>`. The absence of that element does not restrict the language
  version: the tree compiles a C# 12 collection expression at
  `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs:133` and a C# 9 `is not null` pattern at
  `QuickFiler/Controllers/EfcDataModel.cs:61`. No task in this plan depends on a specific C# language
  version either way.

## Ordering hazards this plan is sequenced around

1. **Compile-red would mask runtime-red.** The regression tests reference
   `EfcDataModel.UserDiagnosticAction`, which does not exist yet. Written first, they would break the
   whole `QuickFiler.Test` assembly and the fail-before run would be a build failure rather than a
   genuine failing test. Phase 2 therefore declares the seam alone, with no call site, so Phase 3's
   fail-before run is a real runtime `InvalidOperationException`.
2. **A zero-failure gate must not run while `[expect-fail]` tests are red.** The only zero-failure
   assertions in this plan are in Phase 5 and Phase 6, both after the fix lands in Phase 4. Phase 3's
   run asserts an exact failing set, not zero failures.
3. **Guard placement.** The archive-root guard goes strictly **after** the OneDrive `SpecialFolders`
   read in all three methods. Placing it first drops
   `QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs:217` from 2 to 0 and additionally
   raises `NullReferenceException` from the probe's null `Ol` at `:388`, which a
   `catch (InvalidOperationException)` does not absorb.
4. **Anchored diffs need a commit.** The change-footprint gate runs in Phase 9 after the commit task,
   and is paired with a `git status --porcelain` companion so files the plan creates are visible.
5. **Formatter runs before the size audit.** The 500-line audit in Phase 4 is re-confirmed by [P8-T21]
   after the Phase 6 formatting pass, because CSharpier can change line counts.

### Phase 0 — Baseline Capture and Policy Reads

- [x] [P0-T1] Read `CLAUDE.md` in full and record its four embedded policies (General Code Change,
  General Unit Test, C# Code Change, C# Unit Test) plus the `## C# Toolchain (run in this exact order)`
  section. Acceptance: the reader can quote the four toolchain commands verbatim from that section.
- [x] [P0-T2] Read `.claude/rules/general-code-change.md` in full. Acceptance: the 500-line file-size
  limit and the mandatory toolchain loop are recorded in the reader's notes for [P0-T5].
- [x] [P0-T3] Read `.claude/rules/general-unit-test.md` in full. Acceptance: the Coverage Exclusion
  Policy and the Test File Location clause are recorded, together with the note that D1 supersedes the
  `tests/` mirroring clause for C# in this repository because `CLAUDE.md` outranks it.
- [x] [P0-T4] Read `.claude/rules/csharp.md` in full. Acceptance: its contents are recorded in the
  reader's notes for [P0-T5].
- [x] [P0-T5] Write
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/phase0-instructions-read.md`
  containing `Timestamp:`, `Policy Order:` (the four files in the order read), and an explicit list of
  the files read. Acceptance: the file exists and contains all three field labels and four file paths.
- [x] [P0-T6] Run `dotnet tool restore` from the worktree root and write
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/p0-t6-dotnet-tool-restore.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Acceptance: the artifact records
  `EXIT_CODE: 0` and `Output Summary:` names CSharpier version `1.2.6`. If `dotnet` cannot resolve an
  SDK because the repo-local `.dotnet-sdk` directory named in `global.json:7` is absent, first run
  `pwsh -NoProfile -File scripts/vscode/Install-RepoDotNetSdk.ps1`, then retry, and record both
  invocations in `Output Summary:`.
- [x] [P0-T7] Run
  `pwsh -NoProfile -File scripts/vscode/Invoke-Restore.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU"`
  and write
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/p0-t7-solution-restore.md`
  with the four schema fields. This step exists because a fresh worktree has no `packages/` directory
  and an unrestored legacy project reports missing-reference errors as CS0006 build errors, not
  warnings. After the restore returns, confirm the analyzer package wiring resolves.
  The skew is solution-wide and uniform, and was re-derived at plan-authoring time. Every `.csproj`
  in the solution names `Meziantou.Analyzer.3.0.156` and `Roslynator.Analyzers.4.16.0` in its
  `<Analyzer Include>` items, while all sixteen `packages.config` files that carry analyzer entries
  pin `Meziantou.Analyzer` to `3.0.174` and `Roslynator.Analyzers` to `4.16.1`. The other four
  analyzer packages — `MSTest.Analyzers 4.3.3`, `SonarAnalyzer.CSharp 10.32.0.713`,
  `AsyncFixer 2.1.0` and `Microsoft.CodeAnalysis.BannedApiAnalyzers 5.6.0` — already agree between the
  two files and need no action. Every project resolves its analyzers from the single repository-root
  `packages/` directory through a `..\packages\` relative path, so installing the two skewed pairs
  once resolves the whole solution. Run
  `Select-String -Path 'QuickFiler.Test/QuickFiler.Test.csproj' -SimpleMatch 'Meziantou.Analyzer.','Roslynator.Analyzers.' | ForEach-Object { $_.Line.Trim() }`
  and compare the version segments against the `Meziantou.Analyzer` and `Roslynator.Analyzers`
  `version=` values in `QuickFiler.Test/packages.config` (`:10-15` and `:139-144`), then confirm each
  named `packages\<id>.<version>\` directory exists on disk. Two lines of
  `QuickFiler.Test/QuickFiler.Test.csproj` other than its `<Analyzer Include>` items also match the
  first pattern and are excluded from the comparison, which considers `<Analyzer Include>` items
  only: `:3` is an `<Import Project>` and `:490` is an `<Error Condition>` inside
  `EnsureNuGetPackageBuildImports`, and both name the packages.config version `3.0.174` rather than the
  `<Analyzer Include>` version, so a comparison over the raw match list would see both `3.0.174` and
  `3.0.156` for the same id from the same file. Record `ANALYZER_SKEW:` as either `none`
  or the explicit list of `<id>.<version>` pairs, drawn from those two ids only, that an
  `<Analyzer Include>` item names and whose directory is absent. The acceptance below is scoped to
  those two ids, which are the only ids this command inspects. When the list is non-empty, install
  exactly those versions into the gitignored `packages/` directory with
  `nuget install Meziantou.Analyzer -Version 3.0.156 -OutputDirectory packages` and
  `nuget install Roslynator.Analyzers -Version 4.16.0 -OutputDirectory packages`, re-run the existence
  check, and record both the before and after lists in `Output Summary:`.
  Then run `git status --porcelain -- '*.csproj' '*/packages.config' 'packages'` and record its output
  verbatim. Acceptance: the artifact records `EXIT_CODE: 0`, `ANALYZER_SKEW:` resolves to `none` on the
  second check for both `Meziantou.Analyzer` and `Roslynator.Analyzers`, and the
  `git status --porcelain` output is empty, proving the remediation touched no
  tracked file. If the skew cannot be resolved this way, stop and report; do not proceed to [P0-T10]
  with a CS0006 baseline.
- [x] [P0-T8] Probe for the `dotnet-coverage` global tool with
  `Get-Command dotnet-coverage -ErrorAction SilentlyContinue`; if it is absent run
  `dotnet tool install --global dotnet-coverage`. Write
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/p0-t8-dotnet-coverage-probe.md`
  with the four schema fields and a `Resolution:` line reading either `already present` or
  `installed`. Acceptance: the artifact exists and `Output Summary:` records the resolved
  `dotnet-coverage` version string. Do not halt on absence; install and continue.
- [x] [P0-T9] Run `dotnet tool run csharpier check .` from the worktree root and write
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/p0-t9-csharpier-check.md`
  with the four schema fields. `Output Summary:` must quote the tool's final summary line verbatim and
  record `BASELINE_UNFORMATTED_COUNT:` as the count of `Error <path> - Was not formatted.` lines in the
  output. That count is not on the final summary line: `check` ends with `Checked N files in Nms.`,
  whose N is the number of files processed. Record `BASELINE_UNFORMATTED_FILES:` as the explicit list
  of the file paths named by those `Error` lines (or the literal `none`).
  Acceptance: the artifact exists and carries a numeric `BASELINE_UNFORMATTED_COUNT:` value and a
  `BASELINE_UNFORMATTED_FILES:` line. A non-zero baseline is recorded, not repaired here; the count
  governs the branch chosen in [P6-T1] and the list is the comparison set for [P6-T2].
- [x] [P0-T10] Run
  `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  using the MSBuild resolved by
  `& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1`
  — the `| Select-Object -First 1` suffix is required because `-find` can emit several matching paths,
  and `scripts/vscode/Invoke-Restore.ps1:27` resolves MSBuild the same way — teeing console output to
  `TestResults\msbuild\p0-t10.log` (creating `TestResults\msbuild` first; `.gitignore:39` ignores
  `[Tt]est[Rr]esult*/`, so the log is outside the diff), and write
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/p0-t10-msbuild-analyzers.md`
  with the four schema fields. `Output Summary:` must quote MSBuild's own `Warning(s)` and `Error(s)`
  summary lines verbatim and record `BASELINE_ANALYZER_ERRORS:` as the integer from the `Error(s)`
  line. Acceptance: the artifact exists and carries that integer. If the recorded integer is non-zero,
  stop and report before Phase 2; [P6-T3]/[P6-T4] demand `0 Error(s)` and cannot be satisfied from a
  non-zero baseline. Do not invoke
  `scripts/vscode/Invoke-VSBuild.ps1`; it runs `scripts/vscode/Sync-PackageReferences.ps1` over every
  `.csproj` and would rewrite project files outside the change footprint.
- [x] [P0-T11] Run
  `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  using the same vswhere-resolved MSBuild as [P0-T10], including its `| Select-Object -First 1`
  suffix, teeing console output to `TestResults\msbuild\p0-t11.log` (creating `TestResults\msbuild`
  first; `.gitignore:39` ignores `[Tt]est[Rr]esult*/`, so the log is outside the diff), and write
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/p0-t11-msbuild-nullable.md`
  with the four schema fields. `Output Summary:` must quote the `Error(s)` summary line verbatim and
  record `BASELINE_NULLABLE_ERRORS:` as that integer. Acceptance: the artifact exists and carries that
  integer. If the recorded integer is non-zero, stop and report before Phase 2; [P6-T3]/[P6-T4] demand
  `0 Error(s)` and cannot be satisfied from a non-zero baseline. The command must not gain
  `/p:Nullable=enable` and must not substitute `/t:Build`.
- [x] [P0-T12] Run the baseline test suite in coverage mode with
  `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage/coverage.cobertura.xml`
  and write
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/p0-t12-vstest-coverage.md`
  with the four schema fields plus, in `Output Summary:`, the vstest `Total tests`, `Passed`, `Failed`
  and `Skipped` counts observed in this coverage-harness run, a `COVERAGE_HARNESS_FAILURE_SET:` line
  naming every failing test's fully qualified name (or the literal `none`) as observed in this run —
  `BASELINE_FAILURE_SET:` is set later in this task from the second, direct-harness run — and
  `BASELINE_REPO_LINE_COVERAGE_PERCENT:` as the repo-wide line
  percentage. The percentage is read from the `line-rate` attribute of the root `coverage` element of
  `coverage/coverage.cobertura.xml` multiplied by 100. Also record `COVERAGE_XML_MODE:` as
  `koverage-processed` when the script exited 0, and as `raw` when it exited non-zero.
  `scripts/vscode/Invoke-MSTestWithCoverage.ps1:341` asserts the threshold on the post-processed
  content and `:343` writes that content back only afterwards, so a throw leaves the raw
  `dotnet-coverage` root attributes on disk. Those two states have different denominators:
  `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:417-421` removes every non-allowlisted
  `<package>`, including every `.Test` assembly, and `:442-445` recomputes `line-rate`,
  `lines-covered` and `lines-valid` over what remains. Acceptance: the artifact exists and carries a
  numeric `BASELINE_REPO_LINE_COVERAGE_PERCENT:` value, a `COVERAGE_XML_MODE:` value that is one of
  those two literals, and a `COVERAGE_HARNESS_FAILURE_SET:` line. The script
  is expected to exit non-zero when that percentage is below 80, because
  `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:487-490` throws in that case; the raw Cobertura
  file is written by `dotnet-coverage` before that throw, so the numeric value is still readable, but
  it is then a raw figure computed over every instrumented module rather than a post-processed
  first-party figure, which is what `COVERAGE_XML_MODE:` records. When
  the run exits non-zero, record the observed `EXIT_CODE:` and set `ExpectedExitCode: 1`. Justify it by
  quoting, verbatim in `Output Summary:`, the terminating message emitted by
  `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:489`, which reports the measured Cobertura line
  percentage and ends with the literal `is below the required 80% threshold.`. If that literal is
  absent from the run output, the non-zero exit came from another cause: record it without
  `ExpectedExitCode:` and treat the task as REMEDIATION-REQUIRED.
  Run this command through `Start-Process -Wait` or an equivalent detached wrapper; the full suite takes
  longer than a single foreground command window is reliable for.
  Then capture the baseline failure set a second time using exactly the harness [P6-T5] will use, so
  the carve-out list [P6-T5] consumes is commensurable with the run it gates. Resolve
  `vstest.console.exe` and build the assembly list by the rules stated in [P6-T5], then run the
  resolved executable with that list and
  `/EnableCodeCoverage /InIsolation /Logger:trx /ResultsDirectory:TestResults\p0-t12-direct /TestCaseFilter:"TestCategory!=LiveOutlook"`,
  again through `Start-Process -Wait` or an equivalent detached wrapper. Record this second run in its
  own companion artifact
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/p0-t12-direct-harness-baseline.md`,
  not in the artifact above: the evidence schema carries one `EXIT_CODE:` and at most one
  `ExpectedExitCode:` per file, so two runs with different expected exit codes cannot share one
  artifact. The companion carries the four schema fields for the direct-harness command, its
  `Total tests`, `Passed`, `Failed` and `Skipped` counts under `DIRECT_HARNESS_` prefixes, and
  `BASELINE_FAILURE_SET:` set from **this** run, naming every failing test's fully qualified name or
  the literal `none`.
  When this run exits non-zero and every failing test it reports is listed in its own
  `BASELINE_FAILURE_SET:`, record the observed `EXIT_CODE:` and set `ExpectedExitCode: 1`; when it
  exits 0 and `BASELINE_FAILURE_SET:` is the literal `none`, omit `ExpectedExitCode:`. Apply the same
  rule to [P6-T5]'s artifact against the carve-out list it accepts.
  The coverage-harness run above remains the source of
  `BASELINE_REPO_LINE_COVERAGE_PERCENT:` only, and its artifact's `Command:`, `EXIT_CODE:` and
  `ExpectedExitCode:` fields describe that run alone. Also copy `COVERAGE_HARNESS_FAILURE_SET:` into
  the companion and note there any test that appears in one set and not the other, because
  a divergence identifies a test whose outcome depends on the instrumentation path rather than on this
  change. Acceptance additionally requires that the companion artifact exists, carries the four schema
  fields, and carries a `BASELINE_FAILURE_SET:` line produced by the direct-harness run.

### Phase 1 — Scope Lock and Citation Re-derivation

- [x] [P1-T1] Re-derive the three unguarded read sites. Run
  `Select-String -Path 'QuickFiler/Controllers/EfcDataModel.cs' -SimpleMatch 'OlAncestor = Globals.Ol.ArchiveRootPath'`
  and record the matched line numbers. Acceptance: exactly three matches are returned and their line
  numbers are 289, 310 and 328.
- [x] [P1-T2] Re-derive the ordering sentinel. Run
  `Select-String -Path 'QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs' -SimpleMatch 'SpecialFoldersAccessCount.Should().Be(2)'`
  and record the matched line number. Acceptance: exactly one match is returned at line 217.
- [x] [P1-T3] Confirm the new test file is not yet registered. Run
  `Select-String -Path 'QuickFiler.Test/QuickFiler.Test.csproj' -SimpleMatch 'EfcDataModelArchiveRootTests.cs'`.
  Acceptance: zero matches are returned, and
  `Select-String -Path 'QuickFiler.Test/QuickFiler.Test.csproj' -SimpleMatch 'Controllers\EfcDataModelTests.cs'`
  returns exactly one match at line 115, which is the insertion anchor for [P3-T2].
- [x] [P1-T4] Record the pre-change file size with
  `(Get-Content -LiteralPath 'QuickFiler/Controllers/EfcDataModel.cs').Count` and write
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/p1-t4-tree-facts.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` and the four re-derived facts from
  [P1-T1] through [P1-T4]. Acceptance: the artifact records
  `PRECHANGE_EFCDATAMODEL_LINE_COUNT: 423` and `HEADROOM_TO_CAP: 77`. Use `(Get-Content ...).Count`,
  not `Measure-Object -Line`, which reports a different figure for a file with a trailing newline.

### Phase 2 — Diagnostic Seam Declaration

- [x] [P2-T1] Add the injectable user-diagnostic seam to `QuickFiler/Controllers/EfcDataModel.cs`,
  inside the `#region Public Properties` block, which spans
  `QuickFiler/Controllers/EfcDataModel.cs:146-255`. Write the declaration as
  `internal Action<string> UserDiagnosticAction { get; set; } = text => MessageBox.Show(text);`,
  matching `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:23-24`. CSharpier will wrap it
  across two lines at the `=`; that is expected and is not a deviation. `using System;` is already at
  `QuickFiler/Controllers/EfcDataModel.cs:1`, so use `Action<string>` rather than
  `System.Action<string>`. Add an XML doc comment stating that production never assigns it and that
  tests replace it with a capturing delegate. The XML doc comment must not repeat the identifier
  `UserDiagnosticAction`; refer to it as 'this seam'. Do not add any call site in this task.
  Acceptance:
  `Select-String -Path 'QuickFiler/Controllers/EfcDataModel.cs' -SimpleMatch 'UserDiagnosticAction'`
  returns exactly one match.
- [x] [P2-T2] Confirm the seam is declaration-only across the production tree. Run
  `Get-ChildItem -Path 'QuickFiler','TaskMaster','UtilitiesCS','ToDoModel' -Recurse -Filter '*.cs' | Where-Object { $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\bin\\' } | Select-String -SimpleMatch 'UserDiagnosticAction'`.
  `Select-String` has no `-Recurse` parameter, so the file set is enumerated by `Get-ChildItem` and
  piped in; `obj` and `bin` are excluded because a build leaves generated copies of source files there.
  Acceptance: exactly one match is returned, and it is in `QuickFiler/Controllers/EfcDataModel.cs`.
  This proves the seam changes no behavior yet, so Phase 3's red is caused by the missing guard rather
  than by the seam.
- [x] [P2-T3] Rebuild the solution with
  `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"` using the
  vswhere-resolved MSBuild from [P0-T10], including its `| Select-Object -First 1` suffix, teeing
  console output to `TestResults\msbuild\p2-t3.log` (creating `TestResults\msbuild` first;
  `.gitignore:39` ignores `[Tt]est[Rr]esult*/`, so the log is outside the diff), and write
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/other/p2-t3-seam-compile.md`
  with the four schema fields. Acceptance: the artifact records `EXIT_CODE: 0` and `Output Summary:`
  quotes an MSBuild `Error(s)` summary line reading `0 Error(s)`.

### Phase 3 — Regression Tests, Fail Before the Fix

All eleven tests below live in `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs`, use
MSTest attributes, Moq and FluentAssertions only, and must be independent, deterministic, free of any
filesystem, network, COM or live-Outlook dependency, free of temporary files, and free of
`Thread.Sleep` and `Task.Delay`. No test may carry `[TestCategory("LiveOutlook")]`. Each test must be
laid out in explicit Arrange, Act and Assert sections and must carry a short summary comment stating
the scenario and the expected outcome, per `spec.md` Test Strategy and `CLAUDE.md` § UT3.

Every test in this phase except [P3-T9] constructs its subject as the `TestableEfcDataModel` defined in
[P3-T1], which is the only arrangement in this plan that yields a non-null `MailInfo` without an
Outlook COM fixture. [P3-T9] alone constructs a plain
`new EfcDataModel(globals, null, new CancellationTokenSource(), CancellationToken.None)`, because its
scenario requires `MailInfo` to be null; that `CancellationTokenSource` is constructed inline in the
test method with no disposal, matching `QuickFiler.Test/Controllers/EfcDataModelTests.cs:203`.
Where a task below says "arrange as in [P3-T3]", that includes the `TestableEfcDataModel` construction.

- [x] [P3-T1] Create `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` with namespace
  `QuickFiler.Test.Controllers`, a `[TestClass] public class EfcDataModelArchiveRootTests`, two
  private `const string` fields holding verbatim copies of the two rule texts from
  `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs:13-14` and `:16-17` (named `UnresolvableRuleText` and
  `CrossStoreRuleText`, with a comment recording that `ArchiveRootPathGuard` is `internal` to the
  `TaskMaster` assembly and therefore cannot be referenced), a private `const string ArchiveRootLiteral`
  of `\\mailbox@example.com\Archive`, and private helpers that build a strict `Mock<IOlObjects>`, a
  strict `Mock<IApplicationGlobals>` whose `Ol` and `FS` getters are stubbed, and an
  `IFileSystemFolderPaths` stub whose `SpecialFolders` returns a caller-supplied
  `ConcurrentDictionary<string, string>`. Also add a
  `private sealed class TestableEfcDataModel : EfcDataModel` whose constructor takes
  `IApplicationGlobals globals` and chains to
  `base(globals, null, new CancellationTokenSource(), CancellationToken.None)`. Construct that
  `CancellationTokenSource` inline in the `base(...)` argument list and add no disposal for it: no
  `IDisposable` implementation, no instance field, and no `[TestCleanup]` method. This matches the five
  existing constructions at `QuickFiler.Test/Controllers/EfcDataModelTests.cs:35`, `:73`, `:139`,
  `:169` and `:203`, each of which passes `new CancellationTokenSource()` inline with no disposal in
  the same project that [P6-T4] builds. No disposable-tracking diagnostic can fail either build gate on
  it. `SonarAnalyzer.CSharp` (`QuickFiler.Test/QuickFiler.Test.csproj:472`) and `Roslynator.Analyzers`
  (`:500-503`) do ship such rules and may report at suggestion severity; what makes that non-blocking
  is that the complete
  `<Analyzer Include>` set for `QuickFiler.Test/QuickFiler.Test.csproj` is at `:470-472` and
  `:499-506` and contains no `Microsoft.CodeAnalysis.NetAnalyzers` package, and `.editorconfig:27`
  sets `dotnet_analyzer_diagnostic.severity = suggestion` with no `severity = error` entry anywhere in
  that file and no `.globalconfig` in the repository. `QuickFiler.Test/QuickFiler.Test.csproj:498`
  records that intent verbatim. Passing `null` for the
  mail item makes the base constructor call `TryGetFirstInSelection`, whose `catch (System.Exception)`
  at `QuickFiler/Controllers/EfcDataModel.cs:249` absorbs the strict mock's failure on the unstubbed
  `Ol.App`, so `Mail` is null and the base constructor builds no `ConversationResolver`. In the derived
  constructor body assign the `protected` setter:
  `ConversationResolver = new ConversationResolver(globals, null) { MailHelper = new MailItemHelper() };`.
  The two-argument `ConversationResolver` constructor (`QuickFiler/Helper Classes/ConversationResolver.cs:64-68`)
  stores its two fields and does no work; `MailHelper` is a public settable property
  (`QuickFiler/Helper Classes/ConversationResolver.cs:282-286`); and the parameterless `MailItemHelper`
  (`UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs:80-84`) leaves `FolderInfo` null. `MailInfo`
  (`QuickFiler/Controllers/EfcDataModel.cs:233`, `ConversationResolver?.MailHelper`) is therefore
  non-null with no Outlook COM mock other than the strict `Mock<IOlObjects>`. The derived class is
  legal from this assembly because `EfcDataModel` is `internal`
  (`QuickFiler/Controllers/EfcDataModel.cs:21`) and `QuickFiler/Properties/AssemblyInfo.cs:5` grants
  `InternalsVisibleTo("QuickFiler.Test")`; the `ConversationResolver` setter is `protected`
  (`QuickFiler/Controllers/EfcDataModel.cs:216-220`) and is reachable through `this` inside the derived
  constructor. Acceptance: the file exists, contains the literal
  `namespace QuickFiler.Test.Controllers`, contains the literal `class EfcDataModelArchiveRootTests`,
  and contains the literal `class TestableEfcDataModel : EfcDataModel`.
- [x] [P3-T2] Register the new file in `QuickFiler.Test/QuickFiler.Test.csproj` by inserting
  `<Compile Include="Controllers\EfcDataModelArchiveRootTests.cs" />` immediately after the existing
  `<Compile Include="Controllers\EfcDataModelTests.cs" />` entry identified in [P1-T3]. Acceptance:
  `Select-String -Path 'QuickFiler.Test/QuickFiler.Test.csproj' -SimpleMatch 'Controllers\EfcDataModelArchiveRootTests.cs'`
  returns exactly one match.
- [x] [P3-T3] Add `MoveToFolderAsync_WhenArchiveRootIsUnresolvable_ReturnsFalseInsteadOfThrowing`.
  Arrange a strict `Mock<IOlObjects>` whose `ArchiveRootPath` getter throws
  `new InvalidOperationException(UnresolvableRuleText)`, a `SpecialFolders` dictionary containing the
  key `OneDrive`, and a `TestableEfcDataModel` built as defined in [P3-T1], whose `MailInfo` is
  therefore non-null. Act with `moveConversation: false`. Assert the awaited result is `false` and that the
  call does not throw. Acceptance: the test method exists with that exact name and its assert block
  contains a FluentAssertions `Should().BeFalse()` call on the awaited result.
- [x] [P3-T4] Add
  `MoveToFolderAsync_WhenArchiveRootIsCrossStoreUnresolvable_ReturnsFalseInsteadOfThrowing`, identical
  in shape to [P3-T3] except that the getter throws
  `new InvalidOperationException(CrossStoreRuleText)`. This is the second documented throw condition
  required by AC9. Acceptance: the test method exists with that exact name and references
  `CrossStoreRuleText`.
- [x] [P3-T5] Add `OpenOlFolderAsync_WhenArchiveRootIsUnresolvable_ReportsAndReturns`. Arrange as in
  [P3-T3], additionally assigning `dataModel.UserDiagnosticAction` to a capturing delegate that appends
  to a `List<string>`. Act by awaiting `OpenOlFolderAsync("Clients\\North")`. Assert the call does not
  throw and the captured list has exactly one element. Acceptance: the test method exists with that
  exact name and its assert block contains `Should().ContainSingle()` on the captured list.
- [x] [P3-T6] Add `OpenFsFolderAsync_WhenArchiveRootIsUnresolvable_ReportsAndReturns`, the same shape
  as [P3-T5] but awaiting `OpenFsFolderAsync("Clients\\North")`. Acceptance: the test method exists
  with that exact name and its assert block contains `Should().ContainSingle()` on the captured list.
- [x] [P3-T7] Add `ArchiveRootFailureDiagnostic_DoesNotContainTheArchivePathOrMailboxAddress`. Arrange
  as in [P3-T5]. Act by awaiting `OpenOlFolderAsync("Clients\\North")`. Assert the single captured
  message contains neither `mailbox@example.com` nor `ArchiveRootLiteral`, using the redaction
  assertion style of `QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs:58`. Acceptance: the
  test method exists with that exact name and its assert block contains two
  `Should().NotContain(...)` calls.
- [x] [P3-T8] Add `MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce`. Arrange a strict
  `Mock<IOlObjects>` whose `ArchiveRootPath` getter returns `ArchiveRootLiteral`, a `SpecialFolders`
  dictionary containing `OneDrive`, and an `EfcDataModel` whose `MailInfo` is a `MailItemHelper`
  created through its public parameterless constructor, whose `InitializeSafeDefaults` sets
  `_folderInfo = null` (`UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs:167`, `:177`), so
  `MailItemHelper.FolderInfo` (`UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Properties.cs:78-82`)
  returns `null` and no lazy `FolderWrapper` factory runs. `EmailFiler.SortAsync` therefore
  dereferences `FolderInfo!.OlFolder` at
  `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs:133` and raises
  `NullReferenceException` before `MailItemHelper.Loading.cs:124` can perform a second
  `ArchiveRootPath` read. Act with `moveConversation: false`, awaiting the call inside
  `await act.Should().ThrowAsync<NullReferenceException>()`. The `moveConversation` value is
  load-bearing: with `true`, `QuickFiler/Controllers/EfcDataModel.cs:274` dereferences
  `ConversationResolver.ConversationInfo`, which the two-argument `ConversationResolver` used by
  [P3-T1] leaves null, so a `NullReferenceException` is raised at `:274` before the read at `:289` —
  `ThrowAsync<NullReferenceException>()` would still pass while `Times.Once()` would fail. Assert
  `olObjects.VerifyGet(x => x.ArchiveRootPath, Times.Once())`. The `Times.Once()` assertion and the
  asserted exception type are both fixed and must not be weakened or substituted. The
  `TestableEfcDataModel` of [P3-T1] already supplies exactly this mail helper, so this test uses it
  unchanged; the `ConversationResolver` constructor declared at
  `QuickFiler/Helper Classes/ConversationResolver.cs:70-84` — five parameters, the fifth defaulted,
  which production invokes with four arguments at `QuickFiler/Controllers/EfcDataModel.cs:67` — must
  not be used here, because the helper construction inside it at
  `QuickFiler/Helper Classes/ConversationResolver.cs:82` installs a `_folderInfo` factory at
  `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs:106-111` whose materialization reads
  `ArchiveRootPath` a second time and would make `Times.Once()` fail both before and after the fix.
  Acceptance: the test method exists with that exact name and contains the literal `Times.Once()`.
- [x] [P3-T9] Add `MoveToFolderAsync_WhenMailInfoIsNull_ReturnsFalseWithoutReadingArchiveRoot`.
  Arrange a strict `Mock<IOlObjects>` with `ArchiveRootPath` set up to throw and with no `App` setup,
  and construct a plain
  `new EfcDataModel(globals, null, new CancellationTokenSource(), CancellationToken.None)` rather than
  a `TestableEfcDataModel`, so the public constructor's `TryGetFirstInSelection` catch at
  `QuickFiler/Controllers/EfcDataModel.cs:249` yields a null `Mail`, no `ConversationResolver` is
  built, and `MailInfo` is null.
  Act with `moveConversation: false`. Assert the awaited result is `false` and
  `olObjects.VerifyGet(x => x.ArchiveRootPath, Times.Never())`. Acceptance: the test method exists
  with that exact name and contains the literal `Times.Never()`.
- [x] [P3-T10] Add `MoveToFolderAsync_WhenOneDriveIsMissing_ReturnsFalseWithoutReadingArchiveRoot`.
  Arrange as in [P3-T3] but with an empty `SpecialFolders` dictionary. Assert the awaited result is
  `false` and `olObjects.VerifyGet(x => x.ArchiveRootPath, Times.Never())`. This pins the ordering
  constraint from the production side. Acceptance: the test method exists with that exact name and
  contains the literal `Times.Never()`.
- [x] [P3-T11] Add `MoveToFolderAsync_WhenArchiveRootThrowsComException_StillPropagates`. Arrange
  exactly as in [P3-T3] — a `SpecialFolders` dictionary containing the key `OneDrive` and a
  `TestableEfcDataModel` with a non-null `MailInfo` — except that the `ArchiveRootPath` getter throws
  `new System.Runtime.InteropServices.COMException("com failure")`. Act with `moveConversation: false`
  and assert the
  awaited call throws `COMException` rather than returning `false`. Acceptance: the test method exists
  with that exact name, and its assert block contains the literal `ThrowAsync` and the literal
  `COMException`.
- [x] [P3-T12] Add `OpenOlFolderAsync_WhenOneDriveIsMissing_ReturnsWithoutReadingArchiveRoot`.
  Arrange an empty `SpecialFolders` dictionary and a throwing `ArchiveRootPath` getter. Assert the
  awaited call does not throw and `olObjects.VerifyGet(x => x.ArchiveRootPath, Times.Never())`.
  Acceptance: the test method exists with that exact name and contains the literal `Times.Never()`.
- [x] [P3-T13] Add `OpenFsFolderAsync_WhenOneDriveIsMissing_ReturnsWithoutReadingArchiveRoot`, the
  same shape as [P3-T12] but awaiting `OpenFsFolderAsync`. Acceptance: the test method exists with
  that exact name and contains the literal `Times.Never()`.
- [x] [P3-T14] Rebuild the solution with
  `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"` using the
  vswhere-resolved MSBuild from [P0-T10], including its `| Select-Object -First 1` suffix, and write
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/other/p3-t14-tests-compile.md`
  with the four schema fields. Acceptance: the artifact records `EXIT_CODE: 0` and `Output Summary:`
  quotes an MSBuild `Error(s)` summary line reading `0 Error(s)`. A non-zero result here means the new
  tests do not compile and must be corrected before [P3-T15]; the fail-before evidence must be a
  runtime failure, never a build failure.
- [x] [P3-T15] `[expect-fail]` Run only the new test class against the built `QuickFiler.Test`
  assembly with the vswhere-resolved `vstest.console.exe`, resolved with
  `& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1`,
  run against `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` — the Debug|AnyCPU `<OutputPath>` at
  `QuickFiler.Test/QuickFiler.Test.csproj:36` — and the arguments
  `/InIsolation /Logger:trx /ResultsDirectory:TestResults\p3-t15 /TestCaseFilter:"FullyQualifiedName~EfcDataModelArchiveRootTests"`,
  then write
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/regression-testing/p3-t15-regression-fail-before.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, `ExpectedExitCode: 1` and `Output Summary:`. Acceptance:
  the run reports `Total tests: 11`, `Failed: 5` and `Passed: 6`; the five failing tests are exactly
  `MoveToFolderAsync_WhenArchiveRootIsUnresolvable_ReturnsFalseInsteadOfThrowing`,
  `MoveToFolderAsync_WhenArchiveRootIsCrossStoreUnresolvable_ReturnsFalseInsteadOfThrowing`,
  `OpenOlFolderAsync_WhenArchiveRootIsUnresolvable_ReportsAndReturns`,
  `OpenFsFolderAsync_WhenArchiveRootIsUnresolvable_ReportsAndReturns` and
  `ArchiveRootFailureDiagnostic_DoesNotContainTheArchivePathOrMailboxAddress`; and each of those five
  failure messages names `InvalidOperationException`. The other six tests
  (`MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce`,
  `MoveToFolderAsync_WhenMailInfoIsNull_ReturnsFalseWithoutReadingArchiveRoot`,
  `MoveToFolderAsync_WhenOneDriveIsMissing_ReturnsFalseWithoutReadingArchiveRoot`,
  `MoveToFolderAsync_WhenArchiveRootThrowsComException_StillPropagates`,
  `OpenOlFolderAsync_WhenOneDriveIsMissing_ReturnsWithoutReadingArchiveRoot`,
  `OpenFsFolderAsync_WhenOneDriveIsMissing_ReturnsWithoutReadingArchiveRoot`) pin behavior that is
  already correct before the fix and are expected to pass in this run. Give the run its own
  `/ResultsDirectory` so its TRX does not collide with the Phase 5 and Phase 6 runs.

### Phase 4 — Minimal Fix

- [x] [P4-T1] Add a private helper to `QuickFiler/Controllers/EfcDataModel.cs` with the signature
  `private bool TryGetArchiveRoot(out string archiveRoot)`. It reads `Globals.Ol.ArchiveRootPath`
  exactly once inside a `try`, returns `true` on success, and in
  `catch (InvalidOperationException ex)` sets `archiveRoot` to `null`, writes one
  `logger.Warn(...)` entry through the existing static logger at
  `QuickFiler/Controllers/EfcDataModel.cs:23-25` with a message that interpolates no path and no
  mailbox address, and returns `false`. The catch must name `InvalidOperationException` only.
  Acceptance:
  `Select-String -Path 'QuickFiler/Controllers/EfcDataModel.cs' -SimpleMatch 'private bool TryGetArchiveRoot(out string archiveRoot)'`
  returns exactly one match, and
  `Select-String -Path 'QuickFiler/Controllers/EfcDataModel.cs' -SimpleMatch 'catch (InvalidOperationException'`
  returns exactly one match.
- [x] [P4-T2] Route the `MoveToFolderAsync(string, bool, bool, bool, bool)` read through the helper.
  Insert `if (!TryGetArchiveRoot(out var olAncestor)) { return false; }` immediately after the OneDrive
  guard block — the `Globals.FS.SpecialFolders.TryGetValue("OneDrive", out var folderRoot)` block
  ending at `QuickFiler/Controllers/EfcDataModel.cs:281` before [P2-T1]'s insertion shifted it, since
  [P2-T1] adds the seam and its doc comment inside `#region Public Properties` (`:146-255`), above this
  method — and change the
  initializer member to `OlAncestor = olAncestor,`. The `MailInfo is null` guard remains the first
  statement of the method and the OneDrive guard remains above the new guard. Acceptance: within the
  body of `MoveToFolderAsync(string, ...)` the line index of the `SpecialFolders.TryGetValue` call is
  strictly less than the line index of the `TryGetArchiveRoot` call, and the line index of the
  `MailInfo is null` check is strictly less than both.
- [x] [P4-T3] Route the `OpenOlFolderAsync(string)` read through the helper. Insert, immediately after
  the OneDrive guard block — the `Globals.FS.SpecialFolders.TryGetValue("OneDrive", out var oneDrive)`
  block ending at `QuickFiler/Controllers/EfcDataModel.cs:304` before [P2-T1]'s insertion shifted it —
  a guard
  that calls `TryGetArchiveRoot`, invokes `UserDiagnosticAction` exactly once with a redacted message
  on failure, and returns; and change the initializer member to `OlAncestor = olAncestor,`.
  Acceptance: within the body of `OpenOlFolderAsync` the line index of the `SpecialFolders.TryGetValue`
  call is strictly less than the line index of the `TryGetArchiveRoot` call, and exactly one
  `UserDiagnosticAction` invocation appears in that body.
- [x] [P4-T4] Route the `OpenFsFolderAsync(string)` read through the helper, in the same shape as
  [P4-T3], after the OneDrive guard block — the
  `Globals.FS.SpecialFolders.TryGetValue("OneDrive", out var oneDrive)` block ending at
  `QuickFiler/Controllers/EfcDataModel.cs:323` before [P2-T1]'s insertion shifted it. Acceptance:
  within the body of `OpenFsFolderAsync` the
  line index of the `SpecialFolders.TryGetValue` call is strictly less than the line index of the
  `TryGetArchiveRoot` call, and exactly one `UserDiagnosticAction` invocation appears in that body.
- [x] [P4-T5] Prove no unguarded read remains. Run
  `Select-String -Path 'QuickFiler/Controllers/EfcDataModel.cs' -SimpleMatch 'OlAncestor = Globals.Ol.ArchiveRootPath'`
  and
  `Select-String -Path 'QuickFiler/Controllers/EfcDataModel.cs' -SimpleMatch 'Globals.Ol.ArchiveRootPath'`.
  Acceptance: the first returns zero matches; the second returns exactly one match, and that match is
  inside the `TryGetArchiveRoot` body added by [P4-T1]. The single match must be an executable
  statement inside the `TryGetArchiveRoot` body. Any occurrence in a comment, including an XML doc
  comment on `TryGetArchiveRoot`, violates this condition; write the doc comment without naming
  `Globals.Ol.ArchiveRootPath`. Commented-out copies of the old expression are
  prohibited, because they would defeat the first assertion.
- [x] [P4-T6] Rebuild the solution with
  `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"` using the
  vswhere-resolved MSBuild from [P0-T10], including its `| Select-Object -First 1` suffix, and write
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/other/p4-t6-fix-compile.md`
  with the four schema fields. Acceptance: the artifact records `EXIT_CODE: 0` and `Output Summary:`
  quotes an MSBuild `Error(s)` summary line reading `0 Error(s)`.
- [x] [P4-T7] Record the post-fix file size with
  `(Get-Content -LiteralPath 'QuickFiler/Controllers/EfcDataModel.cs').Count` and write
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/other/p4-t7-file-size.md`
  with the four schema fields plus `POSTFIX_EFCDATAMODEL_LINE_COUNT:`. Record the line count of
  `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` with
  `(Get-Content -LiteralPath 'QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs').Count`
  alongside the production count, and record it as `POSTFIX_ARCHIVEROOTTESTS_LINE_COUNT:`. Acceptance:
  both recorded counts are at most 500. If either exceeds 500, the corresponding file must be tightened
  rather than the cap waived; no other file may absorb the overflow, because that would leave the
  change footprint. Budget guidance: at roughly 25 lines per test and roughly 110 lines of shared
  fixtures, `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` lands near 385 lines. If
  tightening cannot bring it under 500, stop and report rather than splitting, because a second test
  file would require an AC18 amendment in `spec.md` and a corresponding change to [P8-T20] and
  [P9-T2].

### Phase 5 — Regression Pass After the Fix

- [x] [P5-T1] Re-run the new test class with the same command, executable resolution, assembly and
  filter as [P3-T15] but with `/ResultsDirectory:TestResults\p5-t1`, and write
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/regression-testing/p5-t1-regression-pass-after.md`
  with the four schema fields. Acceptance: the artifact records `EXIT_CODE: 0` and `Output Summary:`
  records `Total tests: 11`, `Passed: 11` and `Failed: 0`, with each of the eleven test names listed.
- [x] [P5-T2] Run the two ordering-sentinel tests that must keep passing unmodified, using the
  `vstest.console.exe` resolved exactly as in [P3-T15] against the same
  `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` assembly, with
  `/InIsolation /Logger:trx /ResultsDirectory:TestResults\p5-t2 /TestCaseFilter:"FullyQualifiedName~OpenFolderMethods_DelegateToDataModelWithoutExternalServices|FullyQualifiedName~HandleMoveResult_WhenMoveFails_RoutesMessageThroughInjectedAction"`,
  and write
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/regression-testing/p5-t2-sentinel-tests.md`
  with the four schema fields. Acceptance: the artifact records `EXIT_CODE: 0`, `Total tests: 2`,
  `Passed: 2` and `Failed: 0`.
- [x] [P5-T3] Prove the six existing test files named by the spec are unedited. Run
  `git diff --name-only ecdb1c84ba8541ab67042985919cfed4df768c01 -- QuickFiler.Test TaskMaster.Test`
  and, as the companion required for a name-listing diff,
  `git status --porcelain -uall -- QuickFiler.Test TaskMaster.Test`;
  write
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/other/p5-t3-untouched-tests.md`
  with the four schema fields and both outputs verbatim. Acceptance: neither output names
  `QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs`,
  `QuickFiler.Test/Controllers/EfcHomeControllerExecuteMovesTests.cs`,
  `QuickFiler.Test/Controllers/EfcDataModelTests.cs`,
  `QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs`,
  `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` or
  `TaskMaster.Test/AppGlobals/AppOlObjectsArchiveRootValidationTests.cs`; and the union of the two
  outputs names exactly `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` and
  `QuickFiler.Test/QuickFiler.Test.csproj` under those two pathspecs.

### Phase 6 — Final QC Toolchain Loop

The four commands below are the `CLAUDE.md` § "C# Toolchain (run in this exact order)" commands, run
in order. [P6-T1] carries one documented deviation: when [P0-T9] recorded a non-zero baseline, the
format step is scoped to this change's two owned files so pre-existing drift is not swept into the
change footprint, and AC13 is then resolved through [P8-T15]'s remediation branch. If any step fails
or changes a file, restart the loop from [P6-T1].

- [x] [P6-T1] Record `Get-FileHash -Algorithm SHA256` for `QuickFiler/Controllers/EfcDataModel.cs` and
  `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs`, run
  `dotnet tool run csharpier format .`, then record the same two hashes again, and write
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p6-t1-csharpier-format.md`
  with the four schema fields plus the four hash values under `BeforeHashes:` and `AfterHashes:`.
  Acceptance: the artifact records `EXIT_CODE: 0` and both before-and-after hash pairs, so the
  write-mode command is judged on a tree observation rather than on its exit code, which is 0 whether
  or not it rewrote anything. If [P0-T9] recorded `BASELINE_UNFORMATTED_COUNT: 0`, run the command
  against `.` as written; if [P0-T9] recorded a non-zero count, run it against only
  `QuickFiler/Controllers/EfcDataModel.cs` and
  `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` so the pre-existing drift is not swept
  into this change footprint, and record that branch and the baseline count in `Output Summary:`.
- [x] [P6-T2] Run `dotnet tool run csharpier check .` and write
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p6-t2-csharpier-check.md`
  with the four schema fields, quoting the tool's final summary line verbatim in `Output Summary:`.
  Acceptance: the artifact records `EXIT_CODE: 0` when [P0-T9] recorded
  `BASELINE_UNFORMATTED_COUNT: 0`. When [P0-T9] recorded a non-zero count, the acceptance is instead
  that the set of files the check reports — derived as in [P0-T9] from the
  `Error <path> - Was not formatted.` lines, not from the `Checked N files in Nms.` summary line —
  is a subset of the baseline set recorded in [P0-T9] and
  contains neither `QuickFiler/Controllers/EfcDataModel.cs` nor
  `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs`; record that set difference in
  `Output Summary:`.
- [x] [P6-T3] Run
  `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  using the vswhere-resolved MSBuild from [P0-T10], including its `| Select-Object -First 1` suffix,
  teeing console output to `TestResults\msbuild\p6-t3.log` (creating `TestResults\msbuild` first;
  `.gitignore:39` ignores `[Tt]est[Rr]esult*/`, so the log is outside the diff), and write
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p6-t3-msbuild-analyzers.md`
  with the four schema fields. Acceptance: the artifact records `EXIT_CODE: 0`; `Output Summary:`
  quotes an MSBuild `Error(s)` summary line reading `0 Error(s)`; and the tee'd log contains zero
  occurrences of the literal `Skipping target "CoreCompile"`, proving the gate was not vacuous. The
  error count is read from MSBuild's own summary line, not by counting `error CS` occurrences.
- [x] [P6-T4] Run
  `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  using the vswhere-resolved MSBuild from [P0-T10], including its `| Select-Object -First 1` suffix,
  teeing console output to `TestResults\msbuild\p6-t4.log` (creating `TestResults\msbuild` first;
  `.gitignore:39` ignores `[Tt]est[Rr]esult*/`, so the log is outside the diff), and write
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p6-t4-msbuild-nullable.md`
  with the four schema fields. Acceptance: the artifact records `EXIT_CODE: 0`; `Output Summary:`
  quotes an MSBuild `Error(s)` summary line reading `0 Error(s)`; the tee'd log contains zero
  occurrences of the literal `Skipping target "CoreCompile"`; and `Command:` contains neither
  `/p:Nullable=enable` nor `/t:Build`.
- [x] [P6-T5] Run the full suite with `vstest.console.exe` and `/EnableCodeCoverage`. Resolve the
  executable with
  `& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1`.
  The `| Select-Object -First 1` suffix is required for the same reason as in [P0-T10]: `-find` can
  emit several matching paths, and `scripts/vscode/Invoke-Restore.ps1:27` resolves its tool the same way.
  Build the assembly list from `Get-ChildItem -Path . -Recurse -Filter '*.Test.dll'` keeping only paths
  matching `\bin\Debug\` and rejecting paths matching `\obj\` or `\ref\`. Apply the `.claude` worktree
  exclusion to the path **relative to the current worktree root**, computed as
  `$_.FullName.Substring((Get-Location).Path.Length)`, and reject only relative paths matching
  `\.claude\`. Do not test the absolute path for `\.claude\`: this worktree's own root sits under
  `.claude\worktrees`, so an absolute-path filter matches every candidate and yields an empty assembly
  list, which vstest reports as a run with zero failures. Then run the resolved executable with the
  assembly list and `/EnableCodeCoverage /InIsolation /Logger:trx /ResultsDirectory:TestResults\p6-t5 /TestCaseFilter:"TestCategory!=LiveOutlook"`,
  and write
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p6-t5-vstest-coverage.md`
  with the four schema fields plus `DISCOVERED_ASSEMBLY_COUNT:` and the assembly list recorded as
  worktree-relative paths, computed as `$_.FullName.Substring((Get-Location).Path.Length)`; no
  absolute path and no account or machine name may appear in the artifact, which [P9-T1] commits.
  Acceptance: `DISCOVERED_ASSEMBLY_COUNT:` is at least 4 and the list contains a path ending
  `QuickFiler.Test.dll` and a path ending `TaskMaster.Test.dll`; the run reports `Failed: 0` for tests
  whose fully qualified name begins `QuickFiler.` and for tests whose fully qualified name begins
  `TaskMaster.` — both figures derived from the TRX under `TestResults\p6-t5`, not from the console
  `Failed:` total, which is an all-assembly aggregate — except any such test already named in the
  direct-harness `BASELINE_FAILURE_SET:` recorded by [P0-T12] in
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/baseline/p0-t12-direct-harness-baseline.md`
  — each such exception must be listed by name in `Output Summary:` together with its
  baseline occurrence, and no test in `EfcDataModelArchiveRootTests` may appear among them; and no
  executed test carries `[TestCategory("LiveOutlook")]`. Run this through
  `Start-Process -Wait` or an equivalent detached wrapper.
- [x] [P6-T6] Close the toolchain loop. Write
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p6-t6-loop-closure.md`
  recording, for the final accepted pass, the `Timestamp:` of each of [P6-T1] through [P6-T5] and the
  SHA-256 of `QuickFiler/Controllers/EfcDataModel.cs` and
  `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` taken immediately after [P6-T5].
  Acceptance: those two hashes equal the `AfterHashes:` values recorded in [P6-T1], proving no file in
  the change footprint changed between the formatting step and the test step, and the five timestamps
  are non-decreasing.

### Phase 7 — Coverage Measurement and Delta

- [x] [P7-T1] Run
  `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage/coverage.cobertura.xml`
  and write
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p7-t1-coverage-postchange.md`
  with the four schema fields plus `POSTCHANGE_REPO_LINE_COVERAGE_PERCENT:` read from the `line-rate`
  attribute of the root `coverage` element of `coverage/coverage.cobertura.xml` multiplied by 100, and
  `POSTCHANGE_REPO_BRANCH_COVERAGE_PERCENT:` read from the `branch-rate` attribute the same way, and
  `COVERAGE_XML_MODE:` recorded by the same rule as [P0-T12] — `koverage-processed` on exit 0, `raw`
  on a non-zero exit. Acceptance: the artifact carries numeric values for both percentage fields and a
  `COVERAGE_XML_MODE:` value that is one of those two literals. As in [P0-T12], the Cobertura file is
  written before the script's built-in 80 percent assert runs, so a non-zero exit does not
  invalidate the artifact, but it does mean the file carries raw rather than post-processed root
  attributes. When the run exits non-zero, record the observed `EXIT_CODE:` and set
  `ExpectedExitCode: 1`. Justify it by quoting, verbatim in `Output Summary:`, the terminating message
  emitted by `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:489`, which reports the measured
  Cobertura line percentage and ends with the literal `is below the required 80% threshold.`. If that
  literal is absent from the run output, the non-zero exit came from another cause: record it without
  `ExpectedExitCode:` and treat the task as REMEDIATION-REQUIRED.
  Run through `Start-Process -Wait` or an equivalent detached wrapper.
- [x] [P7-T2] Compute coverage for the changed code. From `coverage/coverage.cobertura.xml`, aggregate
  every `class` element whose `filename` attribute resolves to `QuickFiler/Controllers/EfcDataModel.cs`
  — aggregate by `filename`, not by `class`, because a C# async state machine emits its lines under a
  separate generated class and a per-class figure would understate the method.
  In `koverage-processed` mode `Merge-CoberturaClassesByFilename`
  (`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:428`) has already merged them and
  `filename` is repo-relative; in `raw` mode neither is true and `filename` is an absolute path, so
  match on the `QuickFiler/Controllers/EfcDataModel.cs` suffix with the separator normalized. Record
  `COVERAGE_XML_MODE:` copied from [P7-T1]. Derive the changed-line
  set mechanically rather than by judgment: run
  `git diff -U0 ecdb1c84ba8541ab67042985919cfed4df768c01 -- QuickFiler/Controllers/EfcDataModel.cs`,
  take every post-image line number named by a `+` hunk, and intersect that set with the line numbers
  that appear as `<line number=...>` entries under the aggregated `QuickFiler/Controllers/EfcDataModel.cs`
  filename. The intersection is the denominator; a changed line that emits no `<line>` entry is
  non-executable and is excluded. Record the `git diff -U0` hunk headers verbatim in `Output Summary:`
  so the set is re-derivable. Report the covered and valid line counts and the percentage over that
  intersection, and write
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p7-t2-coverage-changed-lines.md`
  with the four schema fields plus `CHANGED_LINE_COVERAGE_PERCENT:`, `CHANGED_LINES_COVERED:`,
  `CHANGED_LINES_VALID:` and `COVERAGE_XML_MODE:`. Acceptance: `CHANGED_LINE_COVERAGE_PERCENT:` is at least 90.0, and the
  artifact lists each changed line number with a covered or uncovered marker so a third party can
  re-derive the figure. The lambda body of [P2-T1]'s default seam value is expected to be uncovered:
  every test replaces the seam before invoking the paths that use it, so no test executes the default.
  List it explicitly among the uncovered lines rather than treating it as a gap to close.
- [x] [P7-T3] Write the coverage delta report to
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p7-t3-coverage-delta.md`
  with the four schema fields plus `BASELINE_REPO_LINE_COVERAGE_PERCENT:` copied from [P0-T12],
  `POSTCHANGE_REPO_LINE_COVERAGE_PERCENT:` copied from [P7-T1], `CHANGED_LINE_COVERAGE_PERCENT:`
  copied from [P7-T2], and `DELTA_REPO_LINE_COVERAGE_POINTS:` as the post-change value minus the
  baseline value. Also copy `COVERAGE_XML_MODE:` from each of [P0-T12] and [P7-T1] as
  `BASELINE_COVERAGE_XML_MODE:` and `POSTCHANGE_COVERAGE_XML_MODE:`. Acceptance: all four numeric
  fields are present and none reads `UNVERIFIED`; the two recorded modes are equal, because a raw
  figure and a post-processed figure are computed over different denominators and their difference
  measures the denominator rather than the change; and `DELTA_REPO_LINE_COVERAGE_POINTS:` is at least
  `-0.50`, the tolerance that absorbs measurement noise from run-to-run instrumentation differences on
  a suite this size. A delta below that tolerance, or a pair of unequal modes, is recorded as
  `REMEDIATION-REQUIRED` rather than as a pass. The repo-wide figure is recorded and
  reported per AC17 and is not itself a blocking threshold; the blocking clauses are the change-scoped
  ones in [P7-T2] and the tolerance in this task.
- [x] [P7-T4] Decide, and record the decision for, the canonical review coverage artifact. Read
  `POSTCHANGE_REPO_LINE_COVERAGE_PERCENT:` and `COVERAGE_XML_MODE:` from [P7-T1]. When
  `COVERAGE_XML_MODE:` reads `raw`, do not create the file and record
  `Decision: NOT WRITTEN — measured figure is a raw denominator that includes test assemblies`,
  because the raw root attributes are computed over every instrumented module and overstate the
  first-party repo-wide figure the downstream hook reads. When it reads `koverage-processed`, write
  `artifacts/csharp/coverage.xml` **only if** the measured value is greater than or equal to 85.0;
  otherwise do not create the file. Write
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p7-t4-canonical-coverage-artifact.md`
  with the four schema fields plus `MEASURED_REPO_LINE_COVERAGE_PERCENT:`, `COVERAGE_XML_MODE:` and a
  `Decision:` line reading exactly one of `WRITTEN`, `NOT WRITTEN — measured figure below 85.0`, or
  `NOT WRITTEN — measured figure is a raw denominator that includes test assemblies`. When the file is
  written — reachable only in `koverage-processed` mode, so the source attributes named below are the
  recomputed first-party ones from
  `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:442-445` — it
  must be JaCoCo-shaped, carrying a root `<report>` element with a
  `<counter type="LINE" missed="…" covered="…"/>` child whose two values are derived from the
  `lines-covered` and `lines-valid` attributes of the root `coverage` element of
  `coverage/coverage.cobertura.xml`. The hook reads the file with `Get-JacocoRepoCoverage` at
  `.claude/hooks/validate-feature-review-coverage.ps1:253`, which selects `//counter[@type="LINE"]` at
  `:229` and sums the `missed` and `covered` attributes, so a Cobertura-shaped file yields no counters
  and the hook reads nothing. Record the derivation, both source attribute values, and both emitted
  counter values in `Output Summary:`, so a third party can re-derive the figure. Acceptance: the
  artifact exists, carries a numeric measured figure, its `Decision:` line is one of the three defined
  values and agrees with the presence or absence of `artifacts/csharp/coverage.xml` on disk, and
  `([xml](Get-Content artifacts/csharp/coverage.xml)).report.counter` returns a node when `Decision:`
  reads `WRITTEN`. This condition exists because
  `.claude/hooks/validate-feature-review-coverage.ps1:313` applies a hard-coded 85.0 floor whenever
  that file exists, while this repository's policy floor is 80 percent on the testable denominator;
  emitting the file while the measured figure sits between those two values would force a false
  failure downstream.

### Phase 8 — Acceptance Criteria and Spec Updates

Acceptance criteria are checked off in
`docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/spec.md`
per `.claude/skills/acceptance-criteria-tracking/SKILL.md`. Work Mode is `full-bug`, so `spec.md` is
the sole AC source. Change only `- [ ]` to `- [x]`; never edit criterion text; never add a criterion.
Leave any criterion whose evidence is absent unchecked.

- [x] [P8-T1] Confirm the spec carries no non-canonical evidence path and no absolute host path. Run
  `Select-String -Path 'docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/spec.md' -SimpleMatch 'evidence/coverage/'`
  and
  `Select-String -Path 'docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/spec.md' -SimpleMatch 'C:\Users'`.
  Acceptance: both commands return zero matches, and the spec's Correction Log contains the dated entry
  naming `.claude/skills/evidence-and-timestamp-conventions/SKILL.md` and the dated entry recording the
  removal of the absolute host path from the Context section. This task edits no criterion text; both
  substitutions were applied by the planner during preflight.
- [x] [P8-T2] Write the follow-up-issue dossier for the three non-goals to
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/other/p8-t2-followup-issue-dossier.md`,
  with one section per non-goal — (a) `COMException` from the live COM calls in the `ArchiveRootPath`
  getter at `TaskMaster/AppGlobals/AppOlObjects.cs:260-261`; (b) the log-only `async void` boundary
  sinks in `QuickFiler/Controllers/EfcFormController.cs`; (c) the five archive-root reads inside
  `QuickFiler/Controllers/EfcFormController.cs` — each carrying a title, a one-paragraph body, its
  verified citations, and a `ProposedLabels:` line. Acceptance: the file exists and contains exactly
  three `## ` sections whose titles name non-goals (a), (b) and (c).
- [x] [P8-T3] Check off AC1 in `spec.md` once
  `MoveToFolderAsync_WhenArchiveRootIsUnresolvable_ReturnsFalseInsteadOfThrowing` is recorded as
  passing in [P5-T1]. Acceptance: the AC1 line begins `- [x] AC1` and its text is unchanged.
- [x] [P8-T4] Check off AC2 once `OpenOlFolderAsync_WhenArchiveRootIsUnresolvable_ReportsAndReturns` is
  recorded as passing in [P5-T1]. Acceptance: the AC2 line begins `- [x] AC2` and its text is unchanged.
- [x] [P8-T5] Check off AC3 once `OpenFsFolderAsync_WhenArchiveRootIsUnresolvable_ReportsAndReturns` is
  recorded as passing in [P5-T1]. Acceptance: the AC3 line begins `- [x] AC3` and its text is unchanged.
- [x] [P8-T6] Check off AC4 once
  `ArchiveRootFailureDiagnostic_DoesNotContainTheArchivePathOrMailboxAddress` is recorded as passing in
  [P5-T1]. Acceptance: the AC4 line begins `- [x] AC4` and its text is unchanged.
- [x] [P8-T7] Check off AC5 once both
  `MoveToFolderAsync_WhenOneDriveIsMissing_ReturnsFalseWithoutReadingArchiveRoot` (in [P5-T1]) and
  `OpenFolderMethods_DelegateToDataModelWithoutExternalServices` (in [P5-T2]) are recorded as passing
  and [P5-T3] shows `QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs` unmodified.
  Acceptance: the AC5 line begins `- [x] AC5` and its text is unchanged.
- [x] [P8-T8] Check off AC6 once
  `MoveToFolderAsync_WhenMailInfoIsNull_ReturnsFalseWithoutReadingArchiveRoot` is recorded as passing in
  [P5-T1]. Acceptance: the AC6 line begins `- [x] AC6` and its text is unchanged.
- [x] [P8-T9] Check off AC7 once `MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce` is
  recorded as passing in [P5-T1]. Acceptance: the AC7 line begins `- [x] AC7` and its text is unchanged.
- [x] [P8-T10] Check off AC8 once `MoveToFolderAsync_WhenArchiveRootThrowsComException_StillPropagates`
  is recorded as passing in [P5-T1]. Acceptance: the AC8 line begins `- [x] AC8` and its text is
  unchanged.
- [x] [P8-T11] Check off AC9 once both
  `MoveToFolderAsync_WhenArchiveRootIsUnresolvable_ReturnsFalseInsteadOfThrowing` and
  `MoveToFolderAsync_WhenArchiveRootIsCrossStoreUnresolvable_ReturnsFalseInsteadOfThrowing` are recorded
  as passing in [P5-T1]. Acceptance: the AC9 line begins `- [x] AC9` and its text is unchanged.
- [x] [P8-T12] Check off AC10 once [P6-T5] records `Failed: 0` for the `QuickFiler.` and `TaskMaster.`
  test namespaces — the TRX-derived per-namespace figures, not the console all-assembly `Failed:`
  total — and [P5-T3] shows all six named existing test files unmodified. Acceptance: the AC10
  line begins `- [x] AC10` and its text is unchanged.
- [x] [P8-T13] Check off AC11 once [P3-T2] recorded the `<Compile Include=... />` entry and [P6-T5]'s
  TRX under `TestResults\p6-t5` lists at least one test whose fully qualified name contains
  `EfcDataModelArchiveRootTests`. Acceptance: the AC11 line begins `- [x] AC11` and its text is
  unchanged.
- [x] [P8-T14] Check off AC12 once both
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/regression-testing/p3-t15-regression-fail-before.md`
  and
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/regression-testing/p5-t1-regression-pass-after.md`
  exist and carry all four schema fields. Acceptance: the AC12 line begins `- [x] AC12` and its text is
  unchanged.
- [x] [P8-T15] Check off AC13 only when [P6-T2] recorded `EXIT_CODE: 0`. When [P6-T2] was accepted
  under the subset branch because [P0-T9] recorded a non-zero `BASELINE_UNFORMATTED_COUNT:`, leave AC13
  unchecked and append
  `REMEDIATION-REQUIRED: AC13 unmet — pre-existing format drift outside this change footprint, baseline count <N>, files <list>`
  to
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p6-t2-csharpier-check.md`,
  substituting the recorded count and file list. Acceptance: exactly one of the two branches is
  realized, and the realized branch is verifiable — either the AC13 line begins `- [x] AC13` with its
  text unchanged and [P6-T2] recorded `EXIT_CODE: 0`, or the AC13 line begins `- [ ] AC13` and the
  [P6-T2] artifact contains the `REMEDIATION-REQUIRED:` line.
- [x] [P8-T16] Check off AC14 once [P6-T3] records `0 Error(s)` and zero occurrences of
  `Skipping target "CoreCompile"`. Acceptance: the AC14 line begins `- [x] AC14` and its text is
  unchanged.
- [x] [P8-T17] Check off AC15 once [P6-T4] records `0 Error(s)`, zero occurrences of
  `Skipping target "CoreCompile"`, and a `Command:` free of `/p:Nullable=enable` and `/t:Build`.
  Acceptance: the AC15 line begins `- [x] AC15` and its text is unchanged.
- [x] [P8-T18] Check off AC16 only when [P6-T5] recorded `Failed: 0` with an empty baseline-exception
  list and no new test carries `[TestCategory("LiveOutlook")]`. When [P6-T5] was accepted with a
  non-empty exception list, leave AC16 unchecked and append
  `REMEDIATION-REQUIRED: AC16 unmet — pre-existing test failures <list> present in the [P0-T12] direct-harness BASELINE_FAILURE_SET`
  to
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p6-t5-vstest-coverage.md`,
  substituting the recorded names. AC16 as written at `spec.md:733-736` demands zero failed tests
  across `QuickFiler.Test` and `TaskMaster.Test` with no baseline carve-out, so the exception branch
  [P6-T5] permits does not satisfy it. Acceptance: exactly one of the two branches is realized, and
  the realized branch is verifiable — either the AC16 line begins `- [x] AC16` with its text unchanged
  and [P6-T5] recorded `Failed: 0` with an empty exception list, or the AC16 line begins `- [ ] AC16`
  and the [P6-T5] artifact contains the `REMEDIATION-REQUIRED:` line.
- [ ] [P8-T19] Check off AC17 once [P7-T2] records `CHANGED_LINE_COVERAGE_PERCENT:` at or above 90.0
  and [P7-T3] records all four numeric fields with a delta at or above the stated tolerance and with
  `BASELINE_COVERAGE_XML_MODE:` equal to `POSTCHANGE_COVERAGE_XML_MODE:`.
  Acceptance: the AC17 line begins `- [x] AC17` and its text is unchanged.
- [x] [P8-T20] Check off AC18 after verifying the change footprint against the merge base from the
  working tree, before any commit exists. Run
  `git diff --name-only ecdb1c84ba8541ab67042985919cfed4df768c01 -- QuickFiler QuickFiler.Test TaskMaster TaskMaster.Test UtilitiesCS ToDoModel .github docs`
  and, as its companion span,
  `git status --porcelain -uall -- QuickFiler QuickFiler.Test docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638`;
  the anchored diff enumerates tracked modifications and the porcelain span enumerates the files this
  plan created, so neither alone is sufficient. The `-uall` flag is required: without it
  `git status --porcelain` collapses a wholly untracked directory to a single directory entry, so the
  feature folder's untracked evidence tree would be reported as one path and the per-file clause below
  would be evaluated against a directory rather than against files. Acceptance: the union of the two
  outputs names
  `QuickFiler/Controllers/EfcDataModel.cs`,
  `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` and
  `QuickFiler.Test/QuickFiler.Test.csproj`; names no path under `QuickFiler/` other than
  `QuickFiler/Controllers/EfcDataModel.cs`; names no path under `TaskMaster/`, `UtilitiesCS/`,
  `ToDoModel/` or `.github/`; every remaining named path lies under
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/`; and the
  AC18 line then begins `- [x] AC18` with its text unchanged. [P9-T2] repeats this check against the
  commit and is the artifact-bearing record of it.
- [x] [P8-T21] Check off AC19 once [P4-T7] records both `POSTFIX_EFCDATAMODEL_LINE_COUNT:` and
  `POSTFIX_ARCHIVEROOTTESTS_LINE_COUNT:` at or below 500 and both counts are re-confirmed after the
  [P6-T1] formatting pass by re-running
  `(Get-Content -LiteralPath 'QuickFiler/Controllers/EfcDataModel.cs').Count` and
  `(Get-Content -LiteralPath 'QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs').Count`.
  Acceptance: both re-run counts are at most 500, the AC19
  line begins `- [x] AC19` and its text is unchanged.
- [x] [P8-T22] Resolve AC20. If three follow-up issue numbers for non-goals (a), (b) and (c) are
  already recorded in the spec's Rollout & Follow-up section, check off AC20. Otherwise leave AC20
  unchecked and append to
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/other/p8-t2-followup-issue-dossier.md`
  a line reading `REMEDIATION-REQUIRED: AC20 unmet — three follow-up issues not yet filed`, naming the
  dossier as the ready-to-file input and the promotion lifecycle as the filing route. Issue filing is
  an orchestrator responsibility and is not performed by this plan. Acceptance: exactly one of the two
  branches is realized, and the realized branch is verifiable — either the AC20 line begins `- [x] AC20`
  and the spec's Rollout & Follow-up section names three issue numbers, or the AC20 line begins
  `- [ ] AC20` and the dossier contains the `REMEDIATION-REQUIRED:` line.
- [x] [P8-T23] Update the `spec.md` header fields `- **Status:**` to `Implemented` and
  `- **Last Updated:**` to the current ISO-8601 `yyyy-MM-ddTHH-mm` value, and update this plan file's
  `- **Status:**` to `Executed`. Acceptance:
  `Select-String -Path 'docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/spec.md' -SimpleMatch 'Status:** Ready for Planning'`
  returns zero matches.

### Phase 9 — Commit, Footprint Verification, and Clean Tree

- [ ] [P9-T1] Stage and commit all work with pathspecs scoped to this change:
  `git add -A -- QuickFiler QuickFiler.Test docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638`
  followed by a commit whose subject references issue 638. The pathspecs are mandatory: `git add -A`
  without them would sweep unrelated tracked paths, including `.claude/agent-memory`, onto this branch.
  Acceptance: `git log -1 --name-only` lists
  `QuickFiler/Controllers/EfcDataModel.cs`,
  `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` and
  `QuickFiler.Test/QuickFiler.Test.csproj`, and lists no path outside the three pathspecs above; and
  `git log -1 --format=%s` contains the literal `638`.
- [ ] [P9-T2] Verify the change footprint against the merge base. Run
  `git diff --name-only ecdb1c84ba8541ab67042985919cfed4df768c01..HEAD` and, as its companion span,
  `git status --porcelain -uall -- QuickFiler QuickFiler.Test docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638`,
  and write
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p9-t2-change-footprint.md`
  with the four schema fields and both outputs verbatim. Acceptance: the diff output names
  `QuickFiler/Controllers/EfcDataModel.cs`,
  `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` and
  `QuickFiler.Test/QuickFiler.Test.csproj`; it names no path under `QuickFiler/` other than
  `QuickFiler/Controllers/EfcDataModel.cs`; it names no path under `TaskMaster/`, `UtilitiesCS/`,
  `ToDoModel/` or `.github/`; and every remaining named path lies under
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/`. In
  particular it must not name `QuickFiler/Controllers/EfcFormController.cs`,
  `TaskMaster/AppGlobals/AppOlObjects.cs`, `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs` or
  `UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs`.
- [ ] [P9-T3] Write
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p9-t3-clean-tree.md`
  with the four schema fields, summarizing the outcome of every phase and listing every evidence
  artifact this plan produced with its `EXIT_CODE:`, then stage and commit both it and the [P9-T2]
  artifact using the same three pathspecs as [P9-T1]. Acceptance: the file exists, lists at least 25
  artifact paths, and `git log -1 --name-only` names both
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p9-t2-change-footprint.md`
  and
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/evidence/qa-gates/p9-t3-clean-tree.md`.
- [ ] [P9-T4] Close the plan and confirm the working tree is clean within this change's scope. First
  mark every remaining unchecked box in
  `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/plan.2026-08-29T07-41.md`,
  including [P9-T3] and this task. Then run
  `git add -- docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638`
  followed by `git commit -m "docs(638): close plan checklist"`. Then run
  `git status --porcelain -uall -- QuickFiler QuickFiler.Test docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638`.
  Acceptance: the command prints nothing. The check-off precedes the commit so the tree is clean when
  the status runs. The pathspecs are required because
  `.claude/agent-memory` is tracked in this repository and an unscoped status would report unrelated
  agent-memory edits as uncommitted work for this change.

## Notes for the executor

- Every acceptance condition above is written to be falsifiable at the moment its task runs. If any
  condition cannot be satisfied because the tree differs from the facts recorded in the "Verified
  facts" section, stop and report the divergence rather than weakening the condition.
- No task in this plan requires a live Outlook profile, a live COM object, network access, the
  filesystem for test fixtures, or a temporary file.
- The four toolchain commands in Phase 6 are quoted from `CLAUDE.md`. Do not add `/p:Nullable=enable`
  and do not substitute `/t:Build` for `/t:Rebuild`.
