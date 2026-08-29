# 2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed (Plan)

- **Issue:** #637
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-29T12-20
- **Status:** Draft
- **Version:** 0.2
- **Work Mode:** full-bug (from `issue.md`); `spec.md` is the sole acceptance-criteria source (AC1-AC30).

## Conventions (read before executing any task)

**FEATURE_DIR** — the feature folder for this issue is
`docs/features/active/2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637`.
Every evidence path in this plan is written relative to FEATURE_DIR
(for example `evidence/baseline/p0-t12-csharpier-check.md` means
`FEATURE_DIR/evidence/baseline/p0-t12-csharpier-check.md`). Commands that require a literal pathspec
or a literal search operand spell the folder path in full rather than using the name FEATURE_DIR,
because a command carrying a placeholder cannot be executed verbatim. The sites that spell it in full
are the git pathspecs described under "Git pathspec scoping" and the token scan in P7-T10.

**Working directory** — every command below runs with the current directory set to the worktree root
`C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a68051a23e4479267`. All repository-relative
paths in commands resolve against that root.

**Base commit** — the diff anchor for this plan is the literal commit
`ecdb1c84ba8541ab67042985919cfed4df768c01`. Every `git diff` in this plan supplies it explicitly. No
task pins a HEAD SHA.

**Git pathspec scoping** — `.claude/` is a tracked directory in this repository and carries unrelated
in-flight modifications, and `docs/features/parallel/` and `artifacts/` are owned by other processes.
Every `git status --porcelain` and `git diff` gate in this plan is therefore scoped with an explicit
pathspec naming only first-party source, test and feature-document trees. The feature-document
component of every such pathspec is this feature's own folder and never the parent directory
`docs/features/active`. That narrowing is load-bearing rather than cosmetic, and its justification is
forward-looking rather than a claim about the tree as it stands today. At the time this plan was
authored no sibling folder under `docs/features/active` is untracked in this worktree: a
`git status --porcelain` span over that parent directory lists only this feature's own folder, and the
sibling folders are committed — including
`docs/features/active/2026-08-07-breadcrumb-left-right-arrow-parent-child-navigation-440`, whose paths
were confirmed present in this worktree's git index. The narrowing is required regardless, because the
executor runs later than this planning pass: this repository carries several concurrent worktrees and
in-flight feature folders, and a concurrent run in this checkout can leave an untracked or modified
sibling folder under `docs/features/active` at any point between planning and execution. A `git add`
over the parent directory would then stage and commit another feature's folder onto this branch, and a
`git status --porcelain` over the parent directory would report that folder and make every emptiness
gate that consumes it unsatisfiable. This plan does not assume that the tree it observed at planning
time is the tree the executor will meet, so every gate is scoped to paths this plan owns. The default
pathspec is therefore
`-- QuickFiler QuickFiler.Test docs/features/active/2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637`;
P6-T5 narrows it to `-- QuickFiler QuickFiler.Test` for its post-commit cleanliness check, and P8-T30
widens it to the nine production and test trees that task audits. No pathspec in this plan names
`.claude/`, `docs/features/parallel/`, `artifacts/`, or the bare `docs/features/active`.
An unscoped gate is unsatisfiable here and must not be substituted.

**Evidence artifact schema** — every command-step artifact records, as separate lines:
`Timestamp:` (ISO-8601 `yyyy-MM-ddTHH-mm`), `Command:` (the exact command), `EXIT_CODE:`, and
`Output Summary:`. A task whose command is expected to exit non-zero additionally records
`ExpectedExitCode:` with that integer. Baseline and final-QC test artifacts additionally record the
numeric coverage headline values named in their task text. Canonical evidence kinds used by this plan
are `evidence/baseline/`, `evidence/regression-testing/`, `evidence/qa-gates/`, and `evidence/other/`.
`evidence/coverage/` is not a canonical kind and is not used. Nothing under `artifacts/` is used for
evidence.

**Nullable opt-in token discipline.** No evidence artifact under `evidence/` may spell the literal
token that P7-T10 scans for. Where a task requires recording that the solution-wide nullable opt-in
property is absent from a `Command:` line, the artifact records that fact on a line beginning with
the key `NULLABLE_OPT_IN_PROPERTY:` — the short form the tasks below name is
`NULLABLE_OPT_IN_PROPERTY: absent`, and the long form is
`NULLABLE_OPT_IN_PROPERTY: absent from the recorded Command line`; either satisfies the rule — and
quotes the `Command:` line verbatim, which carries the proof without reproducing the token. The task
text below is the binding form where the two differ. Exactly one artifact cannot satisfy
this rule: `evidence/qa-gates/p7-t10-toolchain-audit.md` must record its own scan command, and that
command's pattern is the token itself. P7-T10's scan is restricted to this feature's own folder, and
within that folder it excludes that single file by an explicit `--glob` exclusion stated in P7-T10;
no other evidence artifact of this feature is excluded from it. Evidence artifacts belonging to other
feature folders lie outside the scan's directory operand entirely, and this plan neither reads nor
changes them.

**PowerShell invocation form** — every MSBuild and vstest command is issued through
`pwsh -NoProfile -Command '...'` with outer single quotes and inner double quotes. A bare `/m` passed
to a POSIX shell layer is rewritten to a path and MSBuild fails with MSB1008. Every acceptance
condition expressed as a PowerShell expression — including every `(Get-Content -LiteralPath ...).Count`
check in P2-T2, P2-T4, P4-T1, P6-T1 and P6-T3, and every `Test-Path` check in P0-T9 and P0-T11 — is
likewise issued through `pwsh -NoProfile -Command '...'` with outer single quotes and inner double
quotes. Only `git` and `rg` invocations are issued directly.

**Search invocation form** — every `rg` invocation in this plan is issued with its pattern in single
quotes. `-F` is used only where the pattern is a fixed string whose regex metacharacters — a literal
backslash, or a parenthesis — must match those same characters in the target text, and in that case
a backslash is written once. This plan has exactly two such sites, P2-T3 and P5-T1, and both spell
`-F` in their own task text. Every other `rg` pattern in this plan is a regular expression whose
backslashes are regex escape sequences; those patterns are issued in single quotes without `-F`,
because `-F` would match the escape sequences as literal text and return zero matches for text that
is present. A
pattern that a task below renders in double quotes is issued with that same pattern text enclosed in
single quotes instead; single quotes preserve every backslash exactly, so no pattern below changes
meaning under this rule. The rule is load-bearing because a POSIX shell collapses a doubled backslash
inside double quotes, which turns a written `\\B` into the regex assertion `\B` and returns zero
matches for a literal that is present in the file.

**Toolchain order** — format, then analyzers, then nullable, then tests. Restart from the format step
whenever a step fails or changes a file. `/t:Rebuild` is mandatory: a warm `/t:Build` exits 0 with
`CoreCompile` skipped on every project, so the analyzer and nullable gates become vacuous.
`/p:Nullable=enable` must never be added: no project carries a `<Nullable>` element and there is no
`Directory.Build.props`, so the property conscripts every unannotated file.
`QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` carries `#nullable enable` at line 1;
`QuickFiler/Controllers/EfcDataModel.cs` does not.

**Test invocation, verified against `.github/workflows/_mstest-coverage.yml`.** That workflow's test
step (lines 70-86) discovers assemblies under `$env:GITHUB_WORKSPACE` filtered by `\bin\Debug\` and
not `\obj\` and not `\ref\`, and invokes vstest with `/EnableCodeCoverage /InIsolation /Logger:trx`
and `/TestCaseFilter:"TestCategory!=LiveOutlook"`. I verified all three required properties against
that file:

1. `TestCategory!=LiveOutlook` — present at `_mstest-coverage.yml:83`. Exactly one test method in the
   repository carries that category: `TaskMaster.Test/AppGlobals/LiveOutlookHookupIntegrationTests.cs:72`.
2. `/InIsolation` — present at `_mstest-coverage.yml:83`.
3. Workspace-root scoping of assembly discovery — present at `_mstest-coverage.yml:70`.

Full-suite runs in this plan use the repository wrapper
`scripts/vscode/Invoke-MSTestWithCoverage.ps1`. I read that script and verified its parameters and
its behavior rather than assuming them:

- Parameters are `-SearchRoot`, `-Configuration`, `-CoverageOutput`, `-NoExecute`
  (`Invoke-MSTestWithCoverage.ps1:1-13`, `:248-259`).
- It passes `/InIsolation` and `/TestCaseFilter:TestCategory!=LiveOutlook` on the inner vstest call
  (`:76`).
- It roots assembly discovery at `$repoRoot`, computed as the script directory plus `..\..` (`:271`),
  which for this worktree is the worktree root. That is the local analogue of CI's
  `$env:GITHUB_WORKSPACE` scoping and is why a `\.claude\` exclusion filter must **not** be added
  here: the worktree root path itself contains `\.claude\`, so such a filter would exclude every
  assembly and the run would discover nothing.
- `-SearchRoot .` is passed explicitly on every invocation in this plan.

Scoped verification runs in this plan call `vstest.console.exe` directly on **one explicitly named
assembly path**, `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`, resolved against the worktree root.
That is not a full-assembly discovery search, so no discovery filter can apply to it; naming the single
path is strictly stronger than any filter because it cannot resolve into another worktree. Every such
run still carries `/InIsolation` and conjoins `TestCategory!=LiveOutlook` into its
`/TestCaseFilter`, so its population is comparable to the baseline population restricted to that
assembly.

**Coverage observables — observed, not inferred.** I read
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` before writing any assertion over its output.
Findings that this plan depends on:

- The wrapper prints **no** coverage percentage on a successful run. Its success-case stdout literals
  are `Post-processing coverage XML for Koverage compatibility...` and `Done. Coverage artifact: `
  (`Invoke-MSTestWithCoverage.ps1:338`, `:344`). Numeric coverage is therefore read from the Cobertura
  document, not from stdout.
- `ConvertTo-KoverageCoberturaXml` sets `line-rate`, `branch-rate`, `lines-covered`, `lines-valid`,
  `branches-covered` and `branches-valid` on the `/coverage` element (`Helpers.ps1:442-447`; line 441
  is the `Get-CoberturaCoverageSummary` call that produces the values). Those six
  attributes are the numeric headline this plan asserts over.
- `Assert-CoberturaLineCoverageThreshold` (`Helpers.ps1:459-491`) throws when the repository line rate
  is below 80 percent, and it is called at `Invoke-MSTestWithCoverage.ps1:341`, **before** the
  post-processed document is written back at `:343`. A sub-threshold run therefore leaves the on-disk
  XML as the raw, unfiltered dotnet-coverage output. The headline reader task below re-applies
  `ConvertTo-KoverageCoberturaXml` in memory, which is idempotent on an already-processed document
  (path rewriting no longer matches a prefix, package filtering and class merging are no-ops, and the
  `sources` node already exists), so it yields the same six numbers in both cases.
- The enforced repository floor in the runner is **80 percent line coverage** (`Helpers.ps1:487-489`:
  the percentage is assigned at `:486`, the enforcing comparison `if ($percentage -lt 80)` is at
  `:487`, and the `80%` message literal is at `:489`),
  which matches CLAUDE.md. `.claude/rules/general-unit-test.md` states 85 percent line and 75 percent
  branch. This plan reports the repository-wide figure and treats the runner's own 80 percent gate as
  blocking; the change-scoped gates (no changed line loses coverage, new helper fully covered) are
  blocking regardless of which repository-wide figure is quoted. The conflict is recorded, not
  resolved, by this plan.

**Formatting observables.** `dotnet tool run csharpier format .` rewrites files and still exits 0, so
its exit code alone proves nothing. The discriminating observation used by this plan is therefore a
before-and-after `git status --porcelain` comparison over the scoped pathspec, taken in the same task
as the write-mode run. I did **not** observe CSharpier 1.2.6's success-case summary wording in this
session, so no acceptance condition in this plan asserts over that wording; every csharpier task
records its stdout verbatim into its artifact for audit and gates only on the exit code and on the
tree observation. `.csharpierignore` excludes `**/evidence/**`, `*.cobertura.xml`, `*.trx`,
`*.csproj`, `*.props` and `*.targets`, so evidence artifacts, coverage documents and the test project
file are outside the formatter's scope.

**Anchored-diff form.** Before P6-T5 commits, nothing this plan changes is in `HEAD`, so a two-dot
`BASE..HEAD` diff reports nothing for it. Every pre-commit diff gate in this plan therefore uses the
index form `git diff ecdb1c84ba8541ab67042985919cfed4df768c01 --cached -- <paths>` and is preceded in
the same task by a `git add` over the same paths. Every post-commit diff gate uses
`git diff ecdb1c84ba8541ab67042985919cfed4df768c01..HEAD -- <paths>`. Both forms are anchored to an
explicit ref; the bare unanchored `git diff` is never used.

**Name-listing diffs carry a companion.** A `git diff --name-only` or `--name-status` enumerates
tracked changes only and never reports an untracked path, so on its own it cannot fail on a file this
plan creates and leaves uncommitted. Every name-listing diff in this plan therefore carries a
`git add` span or a `git status --porcelain` span in the same task, and the task text states what the
executor must observe in that companion output. The two mechanisms are complementary and each alone
is wrong in one state: the anchored diff is blind to untracked files, and porcelain status goes empty
once the change is committed. This plan contains exactly two name-listing diff sites, and both carry a
companion. P6-T5 runs
`git add QuickFiler QuickFiler.Test docs/features/active/2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637`
before its diff and asserts that `git status --porcelain -- QuickFiler QuickFiler.Test` produces no
output after the commit. P8-T30 runs a porcelain span over the nine trees it audits, ahead of its two diffs, and
asserts that span is empty; the reconciliation of the two spans at that point in the ordering is
stated in P8-T30 itself.

**Checkbox-counting declaration (operator constraint 3).** This plan ships **no** tool, script, or
reusable helper that counts checkboxes, criteria, or list items in a generated document. The only
counting of checkboxes it performs is three inline, section-scoped counts, all of them confined to
the `## Acceptance Criteria` section of `spec.md`: P0-T6's baseline count of the acceptance criteria
in that section, and the two independently constructed verifications in P8-T31. Section scoping is
mandatory in all three because `spec.md` genuinely contains five checkboxes outside that section — the Impact/Severity
block at `spec.md:54-57` (four) and the Logs/Screenshots line at `spec.md:86` (one) — so a whole-file
count over-reports by exactly five. Because no reusable tool is introduced, the fixture-test
obligation attached to such a tool does not arise; if a future revision introduces one, that
obligation attaches and must be satisfied before the tool is used.

## Scope

In scope, exactly four changes:

- **A.** `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` — bind the discarded `out _` of
  `ArchiveStemContract.TryMakeArchiveRelative` at line 99, commit the stem when non-empty, and treat an
  empty stem as a deterministic non-selection with a value-free diagnostic. The change stays nested
  inside the existing `ArchiveStemContract.IsFullOutlookPath(selection)` arm.
- **B.** `QuickFiler/Controllers/EfcDataModel.cs` — one new pure `internal static` helper called from
  the `DestinationOlStem` assignment at line 287 in the `string` overload of `MoveToFolderAsync`.
- **C.** `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` — one assertion, one test
  method name, and one two-line arrange comment, recorded as a deliberate spec correction.
- **D.** Three stale "deferred to issue #637" records.

Out of scope and owned by issue #695: the `Globals.Ol.ArchiveRootPath` benign degrade, the unhandled
keyboard entry points to `ActionOkAsync`, the half-completed button-path teardown, and the verbatim
`DestinationOlStem` assignments in `EfcDataModel.OpenOlFolderAsync` and `OpenFsFolderAsync`. No task in
this plan touches any of those.

## Fixed identifiers (the executor does not choose these)

- New helper: `EfcDataModel.ToFilingStemOrVerbatim(string candidatePath, string archiveAncestor)`,
  `internal static string`.
- New test file: `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue637Tests.cs`, class
  `BreadcrumbBridgeRouterIssue637Tests`, with exactly these ten test methods:
  `RowSelected_ArchiveRootExactFilingTarget_IsNotSelected`,
  `RowSelected_ArchiveRootExactFilingTarget_PreservesAPriorValidSelection`,
  `RowSelected_RootedTargetUnderArchiveRoot_CommitsTheArchiveRelativeStem`,
  `RowSelected_RootedTargetUnderArchiveRoot_CaseInsensitiveAndTrailingSeparatorRoot_CommitsTheStem`,
  `RowSelected_RelativeFilingTarget_CommitsTheValueVerbatim`,
  `RowSelected_TrashPseudoRow_CommitsTheSentinelVerbatim`,
  `RowSelected_OutOfRootRootedTarget_IsStillRejected`,
  `RowSelected_SeparatorBoundaryNearMissTarget_IsStillRejected`,
  `RowSelected_RootedTargetWithNoBoundArchiveRoot_PassesThroughVerbatim`,
  `SelectFirstRow_RootedTargetUnderArchiveRoot_CommitsTheArchiveRelativeStem`.
- New sibling test class `EfcDataModelIssue637Tests`, whose declaration line is written verbatim as
  `    public class EfcDataModelIssue637Tests`, matching the form of the existing declaration at
  `QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs:16`. It is added to that same existing
  file, with exactly these eight test methods:
  `ToFilingStemOrVerbatim_RootedUnderAncestor_ReturnsTheStem`,
  `ToFilingStemOrVerbatim_RootedUnderCaseDifferingAncestor_ReturnsTheStem`,
  `ToFilingStemOrVerbatim_RelativeStem_ReturnsTheInputVerbatim`,
  `ToFilingStemOrVerbatim_TrashSentinel_ReturnsTheInputVerbatim`,
  `ToFilingStemOrVerbatim_ArchiveRootExact_ReturnsTheInputVerbatimAndDoesNotThrow`,
  `ToFilingStemOrVerbatim_OutOfRootRootedInput_ReturnsTheInputVerbatimAndDoesNotThrow`,
  `ToFilingStemOrVerbatim_NullEmptyWhitespaceOrSeparatorOnlyAncestor_ReturnsTheInputVerbatim`,
  `ToFilingStemOrVerbatim_NullOrEmptyCandidate_ReturnsTheInputVerbatim`.
- Renamed test method (change C):
  `Issue439AlreadyRootedTargetRemainsUnchangedWithCaseInsensitiveArchiveMatch` becomes
  `Issue439RootedTargetUsesOriginalPathForProviderLookupCaseInsensitively`.
- New archive-root-exact diagnostic string (change A), value-free and containing no `@`:
  `Breadcrumb row rejected: target is the archive root itself.`
- Preserved out-of-root diagnostic string, unchanged:
  `Breadcrumb row rejected: target is outside the archive root.`
- Change-D replacement texts, fixed here:
  1. `QuickFiler/Controllers/EfcSelectionGuard.cs:30` becomes
     `        /// normalization in BreadcrumbBridgeRouter.SelectRow is implemented by issue #637.`
  2. `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs:146` becomes
     `            // RC-1 inversion: rooted values are never filing stems here; the producer normalizes.`
  3. `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs:152` becomes
     `                    "a rooted value is never a filing stem at this surface and the producer now normalizes before this predicate is reached"`

## Tree observations recorded while authoring this plan

These were re-derived against the working tree and one disagrees with `spec.md`. They are recorded so
no downstream artifact inherits a wrong figure.

1. `QuickFiler/Controllers/EfcDataModel.cs` is **423** lines, not the 424 stated in the spec's
   implementation table and in AC25's parenthetical. Headroom to the 500-line limit is 77, not 76.
   AC25's binding clause ("at or under 500 lines") is unaffected.
2. The composition test `Issue614_GuardAcceptedSelection_DoesNotThrowAtFilingBoundary` spans
   `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs:167-214`; AC23 and the spec cite `:167-213`.
   The closing brace is at 214. No behavioral consequence.
3. The `#499` clear-on-rebind block spans `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:143-147`;
   the spec cites `:143-146`. The write at `:145` and the read at `:143` that AC24 names are exact.
4. Research section 11's claim that no `EfcDataModelTests.cs` exists is wrong; the file exists at 409
   lines. `spec.md` already records this correction and the spec wins.
5. Research section 6's "16 matching lines across 6 files" for the `MoveToFolder` family is 16 lines
   across **5** files on the tree. `spec.md` already records this correction and the spec wins.
6. This worktree has no `.dotnet-sdk` directory and no `packages` directory, so the repo-local SDK and
   the NuGet package restore must both be bootstrapped before any toolchain command runs.

### Phase 0 — Context, policy reads, and baseline capture

- [ ] [P0-T1] Read `CLAUDE.md` in full at the worktree root. Acceptance: the file is read in this
      session before any other task in this phase, and its four-step C# toolchain command list is
      quoted verbatim into the artifact written by P0-T5.
- [ ] [P0-T2] Read `.claude/rules/general-code-change.md` in full. Acceptance: the file is read, and
      its 500-line file-size limit clause is quoted verbatim into the artifact written by P0-T5.
- [ ] [P0-T3] Read `.claude/rules/general-unit-test.md` in full. Acceptance: the file is read, and its
      line-coverage and branch-coverage threshold sentence is quoted verbatim into the artifact written
      by P0-T5.
- [ ] [P0-T4] Read `.claude/rules/csharp.md` in full. Acceptance: the file is read, and its statement
      about the required test framework, mocking library and assertion library is quoted verbatim into
      the artifact written by P0-T5.
- [ ] [P0-T5] Write `evidence/baseline/phase0-instructions-read.md` containing `Timestamp:`,
      `Policy Order:` naming the four files in the order P0-T1 through P0-T4 read them, an explicit
      bulleted list of those four file paths, and the four verbatim quotations required above.
      Acceptance: the file exists and contains all of `Timestamp:`, `Policy Order:`, `CLAUDE.md`,
      `general-code-change.md`, `general-unit-test.md`, `csharp.md`.
- [ ] [P0-T6] Read `spec.md` in full and write `evidence/baseline/p0-t6-spec-read.md` recording the
      count of acceptance criteria found inside the `## Acceptance Criteria` section only. Acceptance:
      the recorded count is exactly 30, and the artifact also records that the five checkboxes at
      `spec.md:54`, `:55`, `:56`, `:57` and `:86` lie outside that section and are excluded.
- [ ] [P0-T7] Read `research/research.2026-08-29T12-30.md` in full and write
      `evidence/baseline/p0-t7-research-read.md` listing the two numbered corrections `spec.md`
      records under "Corrections to the research file", the second of which bundles two distinct
      file-count facts, and stating that `spec.md` governs where they conflict.
      Acceptance: the artifact names the `EfcDataModelTests.cs` existence correction, the
      `MoveToFolder` five-file correction, and the `SelectedFolderPath` three-production-file
      correction.
- [ ] [P0-T8] Record the branch and base commit. Run
      `git rev-parse --abbrev-ref HEAD`, `git rev-parse HEAD`, and
      `git merge-base --is-ancestor ecdb1c84ba8541ab67042985919cfed4df768c01 HEAD`, and write
      `evidence/baseline/p0-t8-git-base.md`. Acceptance: the `merge-base --is-ancestor` invocation
      exits 0, and the recorded branch name is
      `bug/breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637`. If the branch name
      differs, record `BRANCH MISMATCH` in the artifact, stop, and report to the orchestrator; do not
      proceed to P0-T9.
- [ ] [P0-T9] Bootstrap the repo-local .NET SDK with
      `pwsh -NoProfile -File scripts/vscode/Install-RepoDotNetSdk.ps1` and write
      `evidence/baseline/p0-t9-sdk-bootstrap.md`. Acceptance: `EXIT_CODE: 0`, and after the run the
      path `.dotnet-sdk/dotnet.exe` exists (record the result of `Test-Path .dotnet-sdk/dotnet.exe` as
      `True` in `Output Summary:`). `global.json` pins SDK `8.0.205` with `paths` `[".dotnet-sdk", "$host$"]`,
      so this step is a prerequisite of every `dotnet` invocation below. If the exit code is non-zero
      or the path does not exist, record the captured output under a section headed
      `BOOTSTRAP_FAILED:`, stop, and report to the orchestrator; do not proceed to the next task and
      do not attempt a repair, because no toolchain command in this plan can run without the
      repo-local SDK.
- [ ] [P0-T10] Restore the pinned CSharpier tool with
      `pwsh -NoProfile -Command 'dotnet tool restore; "EXIT_CODE=$LASTEXITCODE"'` and write
      `evidence/baseline/p0-t10-dotnet-tool-restore.md`. The manifest is `dotnet-tools.json` at the
      worktree root and pins `csharpier` `1.2.6`. Acceptance: `EXIT_CODE: 0`, and the captured stdout
      is recorded verbatim in the artifact. No assertion is placed on a version banner, because
      CSharpier 1.2.6 requires a subcommand and the bare-option form is not a form I have observed
      running here; the operative proof that the restore succeeded is that P0-T12's
      `dotnet tool run csharpier check .` produces a CSharpier result rather than a tool-resolution
      error, which P0-T12 records. If the exit code is non-zero, record the captured output under a
      section headed `BOOTSTRAP_FAILED:`, stop, and report to the orchestrator; do not proceed to the
      next task and do not attempt a repair.
- [ ] [P0-T11] Restore NuGet packages with
      `pwsh -NoProfile -File scripts/vscode/Invoke-Restore.ps1` and write
      `evidence/baseline/p0-t11-nuget-restore.md`. This script resolves MSBuild through vswhere and
      runs `/t:Restore /p:RestorePackagesConfig=true`; it does not rewrite any `.csproj` HintPath.
      Acceptance: `EXIT_CODE: 0`, and after the run the directory `packages` exists (record
      `Test-Path packages` as `True`). If the exit code is non-zero or the directory does not exist,
      record the captured output under a section headed `BOOTSTRAP_FAILED:`, stop, and report to the
      orchestrator; do not proceed to the next task and do not attempt a repair, because an
      unrestored package graph produces CS0006 reference errors that are indistinguishable from real
      analyzer findings.
- [ ] [P0-T12] Capture the baseline format state **read-only** with
      `pwsh -NoProfile -Command 'dotnet tool run csharpier check .; "EXIT_CODE=$LASTEXITCODE"'` and
      write `evidence/baseline/p0-t12-csharpier-check.md`. The write-mode `format` command must not be
      run in Phase 0: repairing pre-existing drift before the baseline would either waive it silently
      or make a later zero-diff gate unsatisfiable. Acceptance: the artifact records `EXIT_CODE:`, the
      captured stdout verbatim, and a section headed `BASELINE_FORMAT_DRIFT:` listing every file path
      the captured output names as needing formatting (the list is empty when `EXIT_CODE: 0`, and a
      non-zero exit code is the signal that the list is non-empty). Later zero-diff formatting gates
      exclude exactly the paths in that list and nothing else. The artifact also records whether the
      invocation produced a CSharpier result at all, which is the proof that P0-T10's tool restore
      succeeded.
- [ ] [P0-T13] Capture the baseline analyzer build. Run
      `pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $msbuildExe = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $msbuildExe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; "EXIT_CODE=$LASTEXITCODE"'`
      and write `evidence/baseline/p0-t13-msbuild-analyzers.md`. Acceptance: `EXIT_CODE: 0`; the
      artifact records the MSBuild final status line and the `Warning(s)` and `Error(s)` counts as
      printed; the captured output contains the literal `(Rebuild target(s))` at least once, which is
      the per-project completion line MSBuild emits for the Rebuild target and is therefore the
      discriminator against a skipped incremental Build; and the recorded `Command:` line does not
      contain the solution-wide nullable opt-in property — record this as
      `NULLABLE_OPT_IN_PROPERTY: absent`; do not spell the token in the artifact. If the exit code is
      non-zero, record the full diagnostic list under a section headed `BASELINE_BUILD_RED:`, stop,
      and report to the orchestrator; do not proceed to the next task and do not attempt a repair,
      because a pre-existing red baseline is outside this plan's scope.
- [ ] [P0-T14] Capture the baseline nullable build. Run
      `pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $msbuildExe = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $msbuildExe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true; "EXIT_CODE=$LASTEXITCODE"'`
      and write `evidence/baseline/p0-t14-msbuild-nullable.md`. Acceptance: `EXIT_CODE: 0`; the
      captured output contains `(Rebuild target(s))`; and the recorded `Command:` line contains
      `/t:Rebuild` and does not contain the solution-wide nullable opt-in property — record this as
      `NULLABLE_OPT_IN_PROPERTY: absent`; do not spell the token in the artifact. If the exit code is
      non-zero, record the full diagnostic list under a section headed `BASELINE_BUILD_RED:`, stop,
      and report to the orchestrator; do not proceed to the next task and do not attempt a repair,
      because a pre-existing red baseline is outside this plan's scope.
- [ ] [P0-T15] Capture the baseline full test run with coverage. Run
      `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\p0-t15-baseline.cobertura.xml`
      and write `evidence/baseline/p0-t15-mstest-coverage.md`. `coverage/*` is gitignored
      (`.gitignore:144`), so the Cobertura document does not dirty the tree. Acceptance: the artifact
      records `EXIT_CODE:`, the number of discovered test assemblies printed by the wrapper, the total
      and passed and failed test counts, and a section headed `BASELINE_FAILURE_SET:` naming every
      failing test's fully qualified name (empty when the run passes). The file
      `coverage/p0-t15-baseline.cobertura.xml` exists after the run. `Output Summary:` additionally
      carries the six numeric `/coverage` attribute values and the derived line and branch
      percentages that P0-T16 reads, copied in once P0-T16 has produced them; this task is not
      complete until that copy-back has been made, because the plan contract requires the baseline
      test-step artifact itself to carry the numeric coverage headline.
- [ ] [P0-T16] Read the baseline numeric coverage headline. Run
      `pwsh -NoProfile -Command '. ".\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1"; $raw = Get-Content -LiteralPath ".\coverage\p0-t15-baseline.cobertura.xml" -Raw -Encoding UTF8; [xml]$d = ConvertTo-KoverageCoberturaXml -XmlContent $raw -RepoRoot (Get-Location).Path; $c = $d.SelectSingleNode("/coverage"); foreach ($a in @("line-rate","branch-rate","lines-covered","lines-valid","branches-covered","branches-valid")) { $a + "=" + $c.GetAttribute($a) }'`
      and write `evidence/baseline/p0-t16-coverage-headline.md`. Acceptance: `EXIT_CODE: 0`, and
      `Output Summary:` records all six numeric values, plus the derived baseline line-coverage
      percentage computed as `line-rate` multiplied by 100 and the derived branch percentage computed
      as `branch-rate` multiplied by 100. These are the baseline figures the Phase 7 delta task
      compares against.
- [ ] [P0-T17] Record the baseline uncovered-line sets for the two production files this plan changes.
      Run
      `pwsh -NoProfile -Command '. ".\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1"; $raw = Get-Content -LiteralPath ".\coverage\p0-t15-baseline.cobertura.xml" -Raw -Encoding UTF8; [xml]$d = ConvertTo-KoverageCoberturaXml -XmlContent $raw -RepoRoot (Get-Location).Path; foreach ($f in @("QuickFiler\Controllers\BreadcrumbBridgeRouter.Selection.cs","QuickFiler\Controllers\EfcDataModel.cs")) { $u = @(); foreach ($c in $d.SelectNodes("//class")) { if ($c.GetAttribute("filename") -eq $f) { foreach ($l in $c.SelectNodes("./lines/line")) { if ([int]$l.GetAttribute("hits") -eq 0) { $u += [int]$l.GetAttribute("number") } } } }; $f + " uncovered=" + (($u | Sort-Object -Unique) -join ",") } '`
      and write `evidence/baseline/p0-t17-baseline-uncovered-lines.md`. Acceptance: `EXIT_CODE: 0`, and
      the artifact records one `uncovered=` line for each of the two file paths, even when the set is
      empty.

### Phase 1 — Pre-change census re-derivation, two independent searches per number

Every number in this phase is a number that `spec.md` already carries inside an approved acceptance
criterion. Each task verifies it by a search over the full symbol family and cross-checks it with a
second, independently constructed search. No number in this phase is verified by a single-pass grep.

- [ ] [P1-T1] Re-derive the selection family census (AC9: 2 declarations, 7 call sites).
      Search 1, path-anchored: `rg -n "Select(Row|HierarchyPath)\s*\(" --glob "*.cs" QuickFiler/Controllers/BreadcrumbBridgeRouter.cs QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs`.
      Search 2, syntax-anchored and independently constructed on unqualified-invocation form rather
      than on file paths: `rg -n "^\s+(private void )?Select(Row|HierarchyPath)\(" --glob "*.cs" .`
      run over the whole repository. Search 2 excludes the unrelated Family-B `SelectRow(int index)`
      surface on `BreadcrumbStateModel`, `BreadcrumbSelectionSession`, `FolderBreadcrumbBridgeRouter`
      and `BreadcrumbBridgeCoordinator`, because every Family-B site carries a receiver or a
      non-`private void` modifier. A naive `rg "SelectRow"` returns roughly ten times too many lines
      and must not be used. Write `evidence/baseline/p1-t1-selection-family.md`. Acceptance: both
      searches return exactly 9 lines; the two line sets are identical; the artifact lists them and
      classifies each as declaration or call; declarations total 2
      (`BreadcrumbBridgeRouter.Selection.cs:83` and `:109`) and call sites total 7
      (`BreadcrumbBridgeRouter.cs:201`, `:286`, `BreadcrumbBridgeRouter.Arrows.cs:138`, `:153`, `:161`,
      `BreadcrumbBridgeRouter.Selection.cs:33`, `:47`).
- [ ] [P1-T2] Re-derive the `MoveToFolder` family census (AC16: 3 declarations, 6 call sites).
      Search 1, family-stem: `rg -n "MoveToFolder" --glob "*.cs" .` — the bare stem catches any
      non-`Async` sibling or partially renamed overload that an `Async`-suffixed pattern would miss.
      Search 2, independently constructed on invocation and declaration syntax:
      `rg -n "MoveToFolderAsync\s*\(" --glob "*.cs" .` — this excludes the `MoveToFolderAsyncAction`
      delegate property, its null test and its invocation, which are textual references rather than
      family members. Write `evidence/baseline/p1-t2-movetofolder-family.md`. Acceptance: Search 1
      returns 16 lines across 5 files; Search 2 returns 9 lines across 4 files; the artifact classifies
      Search 2's 9 lines as exactly 3 declarations (`EfcDataModel.cs:259`, `EfcDataModel.cs:336`,
      `EfcHomeController.ExecuteMoves.cs:89`) and 6 call sites
      (`EfcHomeController.ExecuteMoves.cs:78`, `:98`, `EfcDataModel.cs:346`, `EfcFormController.cs:537`,
      `:844`, `EfcHomeControllerExecuteMovesTests.cs:87`); and the artifact records that Search 1 minus
      Search 2 leaves exactly 7 non-member textual references, closing the 16-line accounting. The
      artifact also records that the file count is 5, not the 6 stated in research section 6.
- [ ] [P1-T3] Re-derive the `SelectedFolderPath` surface (AC24: 9 lines across 3 production files, 2
      writes, 3 reads). Search 1: `rg -c "SelectedFolderPath" --glob "*.cs" .`, recording the per-file
      counts. Search 2, independently constructed by scoping to the production project directories up
      front rather than by subtracting the test projects from Search 1's table:
      `rg -n "SelectedFolderPath" --glob "*.cs" QuickFiler/ UtilitiesCS/ TaskMaster/ ToDoModel/ Tags/ TaskVisualization/`.
      Write
      `evidence/baseline/p1-t3-selectedfolderpath-surface.md`. Acceptance: Search 1 returns 74 lines
      across 9 files; Search 2 returns exactly 9 lines across exactly 3 files
      (`BreadcrumbBridgeRouter.cs`, `BreadcrumbBridgeRouter.Selection.cs`, `EfcFormController.cs`); the
      artifact classifies those 9 lines as 1 declaration (`BreadcrumbBridgeRouter.cs:59`), 1 doc
      reference (`:61`), 2 writes (`:145` and `BreadcrumbBridgeRouter.Selection.cs:134`), 3 reads
      (`:143`, `BreadcrumbBridgeRouter.Selection.cs:138`, `EfcFormController.cs:321`) and 2 event-only
      lines (`:62`, `:146`); and it records that the production split is 3 production files and 6 test
      files, not the 2-and-7 stated in research section 7.
- [ ] [P1-T4] Re-derive the stale deferral record census (AC22: 3 records).
      Search 1, on the deferral phrase: `rg -n "deferred to issue #637" --glob "*.cs" .`.
      Search 2, independently constructed on the issue reference alone so it cannot miss a differently
      worded deferral: `rg -n "#637" --glob "*.cs" .`. Write
      `evidence/baseline/p1-t4-deferral-records.md`. Acceptance: Search 1 returns exactly 3 lines
      (`QuickFiler/Controllers/EfcSelectionGuard.cs:30`,
      `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs:146`, and `:152`); Search 2 returns a
      superset whose every additional line is enumerated in the artifact and individually classified as
      not a deferral claim; and the artifact quotes the current text of all three Search 1 lines
      verbatim.
- [ ] [P1-T5] Re-derive the existing `ToArchiveRelativeStem` test count (AC15: 8 tests).
      Search 1, on the method-name convention:
      `rg -n "public void ToArchiveRelativeStem_" QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs`.
      Search 2, independently constructed on the call to the member under test rather than on test
      naming: `rg -n "EfcDataModel\.ToArchiveRelativeStem\(" QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs`.
      Write `evidence/baseline/p1-t5-toarchiverelativestem-tests.md`. Acceptance: both searches return
      exactly 8 lines; the artifact records the declaration line numbers 21, 34, 48, 62, 72, 87, 100,
      111 and records that `ToArchiveRelativeStem_ArchiveRootItself_Throws` is the method at line 62.
- [ ] [P1-T6] Re-derive the no-bound-root pass-through test pair (AC4: 2 tests).
      Search 1, by name:
      `rg -n "Issue439SlashOnlyArchiveRootPreservesFullHierarchySelection|SegmentActivate_WithNoBoundArchiveRoot_PreservesThePassThroughMode" --glob "*.cs" .`.
      Search 2, independently constructed on the binding mechanism that produces an empty bound root —
      a separator-only fourth argument or the three-argument public overload:
      `rg -n "BindRowsAsync\(" --glob "*.cs" QuickFiler.Test/Controllers/` with each hit classified by
      whether it supplies an archive root. Write `evidence/baseline/p1-t6-passthrough-tests.md`.
      Acceptance: Search 1 returns exactly 2 declaration lines
      (`BreadcrumbBridgeRouterIssue439Tests.cs:619` and `BreadcrumbBridgeRouterIssue614Tests.cs:188`);
      the artifact records that the first binds `@"\"` at `:645` and asserts `Be(@"\Archive")` at
      `:665`, and the second uses the three-argument overload at `:213` and asserts `Be(@"\Archive")` at
      `:221`; and Search 2's classification identifies the same two tests as the only pass-through
      cases and no others.
- [ ] [P1-T7] Re-derive the file line counts AC25 depends on. Construction 1:
      `pwsh -NoProfile -Command 'foreach ($p in @("QuickFiler\Controllers\EfcDataModel.cs","QuickFiler\Controllers\BreadcrumbBridgeRouter.Selection.cs","QuickFiler\Controllers\EfcSelectionGuard.cs","QuickFiler.Test\Controllers\BreadcrumbBridgeRouterIssue439Tests.cs","QuickFiler.Test\Controllers\EfcDataModelIssue614Tests.cs","QuickFiler.Test\Controllers\EfcSelectionGuardTests.cs")) { $p + "=" + (Get-Content -LiteralPath $p).Count }'`.
      Construction 2, independently constructed with a line-oriented search rather than a file read:
      `rg -c "^" --glob "*.cs" QuickFiler/Controllers/ QuickFiler.Test/Controllers/` filtered to the
      same six paths. `Measure-Object -Line` must not be substituted for `(Get-Content).Count`; it
      reports a different figure for a file without a trailing newline. Write
      `evidence/baseline/p1-t7-file-line-counts.md`. Acceptance: both constructions agree on all six
      paths; `EfcDataModel.cs` is 423; `BreadcrumbBridgeRouter.Selection.cs` is 209;
      `EfcSelectionGuard.cs` is 79; `BreadcrumbBridgeRouterIssue439Tests.cs` is 694;
      `EfcDataModelIssue614Tests.cs` is 123; `EfcSelectionGuardTests.cs` is 296; and the artifact
      records that `spec.md` states 424 for `EfcDataModel.cs` and that the tree value 423 governs.
- [ ] [P1-T8] Re-derive the single pinning assertion (AC20: exactly 1 existing assertion changes).
      Construction 1, on the assertion form:
      `rg -n "SelectedFolderPath\.Should\(\)\.Be\(" --glob "*.cs" QuickFiler.Test/` with every hit
      classified by whether the selected row's filing target is a full Outlook path at or under a
      non-empty bound root. Construction 2, independently constructed on the trigger side rather than
      the assertion side: `rg -n "rowSelected|SelectFirstRow" --glob "*.cs" QuickFiler.Test/` with
      every hit classified by its bound root and its presented filing target. Write
      `evidence/baseline/p1-t8-pinning-assertion.md`. Acceptance: both constructions identify exactly
      one assertion that must change — `BreadcrumbBridgeRouterIssue439Tests.cs:165`,
      `router.SelectedFolderPath.Should().Be(fullTarget);` inside the method declared at `:119` — and
      the artifact records that no test anywhere binds a presented row whose filing target equals the
      bound archive root, so zero tests depend on the archive-root-exact case being a selection.

### Phase 2 — Regression tests that fail before the fix

The change-B tests name a member that does not exist yet, and a test file referencing a missing member
makes the whole `QuickFiler.Test` assembly fail to compile, which would prevent every other test in
this phase from running at all. P2-T1 therefore lands a behavior-preserving seam first: the helper is
declared and called, but returns its input verbatim, which is byte-for-byte the behavior of the current
assignment at `EfcDataModel.cs:287`. The red in this phase is a genuine runtime red, not a compile
failure.

- [ ] [P2-T1] Add the behavior-preserving seam. In `QuickFiler/Controllers/EfcDataModel.cs`, declare
      `internal static string ToFilingStemOrVerbatim(string candidatePath, string archiveAncestor)`
      whose body is exactly
      `_ = ArchiveStemContract.TryMakeArchiveRelative(candidatePath, archiveAncestor, out _);` followed
      by `return candidatePath;`, with an XML documentation comment stating that this is the #637 seam
      and that the normalization lands in P4-T1. Change the assignment at `EfcDataModel.cs:287` from
      `DestinationOlStem = folderpath,` to
      `DestinationOlStem = ToFilingStemOrVerbatim(folderpath, Globals.Ol.ArchiveRootPath),`.
      The explicit discard on the `TryMakeArchiveRelative` call is required so that both parameters are
      used and no unused-parameter diagnostic can be promoted to an error by
      `/p:TreatWarningsAsErrors=true`. Declare the helper immediately after the closing brace of the
      `string` overload of `MoveToFolderAsync` at original line 297 and before
      `internal async Task OpenOlFolderAsync` at original line 299, so the insertion hunk falls
      outside both ranges P4-T6 excludes. Do not place it adjacent to `ToArchiveRelativeStem`. The
      seam's XML documentation must not contain the token `MoveToFolder`; refer to its caller as
      "the `string` filing overload" instead, because P8-T16 asserts the family stem search still
      returns exactly 16 lines. For the same reason the seam's XML documentation must not contain
      either of the two literals this task asserts an exact count of 1 for —
      `internal static string ToFilingStemOrVerbatim` and `DestinationOlStem = ToFilingStemOrVerbatim`
      — so it must not reproduce the declaration signature or the assignment statement; naming the
      method by its bare identifier is permitted and is classified rather than counted by P4-T2.
      Acceptance: `rg -n "internal static string ToFilingStemOrVerbatim" QuickFiler/Controllers/EfcDataModel.cs`
      returns exactly 1 line, and
      `rg -n "DestinationOlStem = ToFilingStemOrVerbatim" QuickFiler/Controllers/EfcDataModel.cs`
      returns exactly 1 line.
- [ ] [P2-T2] Create `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue637Tests.cs` containing
      the class `BreadcrumbBridgeRouterIssue637Tests` with exactly the ten test methods named in the
      "Fixed identifiers" section. Use fixture Shape 2 and do not invent a new fixture shape: a
      `[TestInitialize]` `Setup` and `[TestCleanup]` `Cleanup` modelled on
      `BreadcrumbBridgeRouterIssue614Tests.cs:38-58`, the log4net `MemoryAppender` attach and detach
      helpers modelled on `:338-356`, the `Key` and `Segment` helpers modelled on `:328-336`, the
      `Inbound` and `RowSelected` JSON helpers modelled on `:264-267` and `:288-291`, the
      `RenderedMessages` and `AssertRejectionDiagnosticWithoutIdentifiers` helpers modelled on
      `:304-326`, and a `BindRows` helper modelled on `BindChain` at `:236-262` that accepts an archive
      root plus one or more presented row texts and sets up `ResolveLeafKeyAsync` and
      `GetAncestorChainAsync` for every presented row that `BreadcrumbRowBuilder.Classify` treats as a
      suggestion. To produce an empty bound root, pass the separator-only value `@"\"`, which
      `BindRowsAsync` trims to empty at `BreadcrumbBridgeRouter.cs:107-109`. Framework is MSTest with
      Moq and FluentAssertions; no temporary file, no wall-clock wait, no Outlook process. No text in
      this file may contain the token `MoveToFolder`, because P8-T16 asserts that a repository-wide
      `*.cs` search for that stem still returns exactly 16 lines. Acceptance:
      the file exists; `rg -c "\[TestMethod\]" QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue637Tests.cs`
      returns 10; each of the ten fixed method names is found exactly once by
      `rg -n "public void " QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue637Tests.cs`; and
      `(Get-Content -LiteralPath "QuickFiler.Test\Controllers\BreadcrumbBridgeRouterIssue637Tests.cs").Count`
      is at most 500.
- [ ] [P2-T3] Register the new test file in the non-SDK project. Insert
      `    <Compile Include="Controllers\BreadcrumbBridgeRouterIssue637Tests.cs" />` into
      `QuickFiler.Test/QuickFiler.Test.csproj` immediately after the existing line 64,
      `    <Compile Include="Controllers\BreadcrumbBridgeRouterIssue439Tests.cs" />`. A file absent
      from this project compiles into nothing and its tests silently never run. The literal this task
      creates is `Controllers\BreadcrumbBridgeRouterIssue637Tests.cs`, quoted here verbatim because it
      is absent from the tracked tree until this task inserts it. The acceptance search below is the
      fixed-string, single-quoted form required by the "Search invocation form" convention: the
      backslash is written once and `-F` disables regex interpretation, so no shell layer and no
      regex engine can consume it. Acceptance:
      `rg -F -n 'Controllers\BreadcrumbBridgeRouterIssue637Tests.cs' QuickFiler.Test/QuickFiler.Test.csproj`
      returns exactly 1 line, and that line is inside the same `ItemGroup` that begins at line 57.
- [ ] [P2-T4] Add the change-B helper tests. In the existing file
      `QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs`, add a new sibling `[TestClass]`
      `EfcDataModelIssue637Tests` containing exactly the eight test methods named in the "Fixed
      identifiers" section, reaching `EfcDataModel.ToFilingStemOrVerbatim` through the existing
      `InternalsVisibleTo("QuickFiler.Test")` at `QuickFiler/Properties/AssemblyInfo.cs:5`. The eight
      existing `ToArchiveRelativeStem` tests in the file are not modified. The file is already
      registered at `QuickFiler.Test/QuickFiler.Test.csproj:114`, so no new `Compile Include` is
      required. The declaration line this task creates is
      `    public class EfcDataModelIssue637Tests`, matching the form of the existing declaration at
      `QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs:16`. The acceptance search below
      therefore asserts over the literal `class EfcDataModelIssue637Tests`, which is quoted here
      verbatim because it is absent from the tracked tree until this task creates it. No text this
      task adds may contain the token `MoveToFolder`, because P8-T16 asserts that a repository-wide
      `*.cs` search for that stem still returns exactly 16 lines. Acceptance:
      `rg -n "class EfcDataModelIssue637Tests" QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs`
      returns exactly 1 line; each of the eight fixed method names is found exactly once in that file;
      `git add QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs` followed in the same task by
      `git diff ecdb1c84ba8541ab67042985919cfed4df768c01 --cached -- QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs`
      shows zero removed content lines, meaning zero lines beginning with a single `-`; and
      `(Get-Content -LiteralPath "QuickFiler.Test\Controllers\EfcDataModelIssue614Tests.cs").Count` is
      at most 500.
- [ ] [P2-T5] Run the analyzer build and write `evidence/regression-testing/p2-t5-msbuild-analyzers.md`
      using the P0-T13 command verbatim. Acceptance: `EXIT_CODE: 0`; the output contains
      `(Rebuild target(s))`; and the `Error(s)` count is 0. A non-zero exit here means the seam or the
      new test files do not compile and must be repaired before P2-T7 runs.
- [ ] [P2-T6] Run the nullable build and write `evidence/regression-testing/p2-t6-msbuild-nullable.md`
      using the P0-T14 command verbatim. Acceptance: `EXIT_CODE: 0`; the output contains
      `(Rebuild target(s))`; the recorded `Command:` line does not contain the solution-wide nullable
      opt-in property — record this as `NULLABLE_OPT_IN_PROPERTY: absent`; do not spell the token in
      the artifact. This gate is where an unused-parameter or nullable diagnostic introduced by the
      seam would surface as an error.
- [ ] [P2-T7] [expect-fail] Run the new router regression tests before the fix. Run
      `pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; $asm = Join-Path (Get-Location).Path "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll"; & $vstest $asm /InIsolation "/TestCaseFilter:FullyQualifiedName~BreadcrumbBridgeRouterIssue637Tests&TestCategory!=LiveOutlook" /Logger:trx "/ResultsDirectory:coverage\testresults\p2-t7"; "EXIT_CODE=$LASTEXITCODE"'`
      and write `evidence/regression-testing/p2-t7-router-tests-red.md` with `ExpectedExitCode: 1`.
      Acceptance: the output does not contain `No test matches the given testcase filter`; the run
      reports 10 tests total; exactly these 5 fail, named individually in the artifact:
      `RowSelected_ArchiveRootExactFilingTarget_IsNotSelected`,
      `RowSelected_ArchiveRootExactFilingTarget_PreservesAPriorValidSelection`,
      `RowSelected_RootedTargetUnderArchiveRoot_CommitsTheArchiveRelativeStem`,
      `RowSelected_RootedTargetUnderArchiveRoot_CaseInsensitiveAndTrailingSeparatorRoot_CommitsTheStem`,
      `SelectFirstRow_RootedTargetUnderArchiveRoot_CommitsTheArchiveRelativeStem`; and exactly these 5
      pass: `RowSelected_RelativeFilingTarget_CommitsTheValueVerbatim`,
      `RowSelected_TrashPseudoRow_CommitsTheSentinelVerbatim`,
      `RowSelected_OutOfRootRootedTarget_IsStillRejected`,
      `RowSelected_SeparatorBoundaryNearMissTarget_IsStillRejected`,
      `RowSelected_RootedTargetWithNoBoundArchiveRoot_PassesThroughVerbatim`. A different partition is a
      defect in the tests, not evidence of the bug, and must be repaired before Phase 3.
- [ ] [P2-T8] [expect-fail] Run the new helper tests before the fix. Run the P2-T7 command with the
      filter substring changed to `FullyQualifiedName~EfcDataModelIssue637Tests` and the results
      directory changed to `coverage\testresults\p2-t8`, and write
      `evidence/regression-testing/p2-t8-helper-tests-red.md` with `ExpectedExitCode: 1`. Acceptance:
      the output does not contain `No test matches the given testcase filter`; the run reports 8 tests
      total; exactly these 2 fail: `ToFilingStemOrVerbatim_RootedUnderAncestor_ReturnsTheStem` and
      `ToFilingStemOrVerbatim_RootedUnderCaseDifferingAncestor_ReturnsTheStem`; and the other 6 pass,
      because the seam already returns the input verbatim for every non-normalizable case.
- [ ] [P2-T9] Prove the new test file actually executes rather than silently compiling into nothing.
      From the TRX produced by P2-T7 at `coverage\testresults\p2-t7`, extract every `UnitTestResult`
      whose `testName` begins with one of the ten fixed method names, and write
      `evidence/regression-testing/p2-t9-compile-include-observed.md`. Acceptance: the artifact records
      exactly 10 such results; it quotes the `Compile Include` line added by P2-T3 verbatim; and it
      records that removing that line would make this count 0, which is the observable AC26 requires.

### Phase 3 — Change A, producer normalization in `SelectRow`

- [ ] [P3-T1] Apply change A in `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs`. Replace
      the guard currently at lines 94-106 so that the `_boundRoot.Length != 0` and
      `ArchiveStemContract.IsFullOutlookPath(selection)` conjunction opens a block; inside that block,
      a failed `ArchiveStemContract.TryMakeArchiveRelative(selection, _boundRoot, out string stem)`
      logs the unchanged message `Breadcrumb row rejected: target is outside the archive root.` and
      returns; a succeeded call with `stem.Length == 0` logs the new message
      `Breadcrumb row rejected: target is the archive root itself.` and returns; otherwise `selection`
      is reassigned to `stem`. Control then falls through to the unchanged
      `CommitSelection(row, selection);`. `selection` is a non-nullable `string` and `stem` is
      definitely assigned by `ArchiveStemContract.cs:112` on every exit path, so no nullable temporary
      is introduced and `CommitSelection`'s non-nullable `string` parameter is satisfied. Replace the
      stale comment at lines 94-95, which asserts the superseded pass-verbatim behavior. The
      replacement comment must not contain the token `IsFullOutlookPath`; refer to the arm as the
      full-path gate instead, because this task's acceptance asserts an exact count of 1 for that
      token in this file. For the same reason the replacement comment must not contain the token
      `out string stem`, whose exact count of 2 this task also asserts, nor the token `out _`, whose
      count this task asserts is 0, nor either of the two diagnostic message literals whose exact
      count of 1 apiece P3-T2 asserts over this same file: describe the two rejection outcomes in
      prose without quoting their message text. Acceptance:
      `rg -n "out string stem" QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` returns 2
      lines (the new one in `SelectRow` and the existing one at `:120` in `SelectHierarchyPath`);
      `rg -n "out _" QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` returns 0 lines; and
      `rg -c "IsFullOutlookPath" QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` returns 1.
- [ ] [P3-T2] Verify the nesting and the preserved diagnostics required by AC3 and AC6. Acceptance:
      `rg -n "Breadcrumb row rejected: target is outside the archive root." QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs`
      returns exactly 1 line;
      `rg -n "Breadcrumb row rejected: target is the archive root itself." QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs`
      returns exactly 1 line; neither message contains the character `@`; and the artifact
      `evidence/regression-testing/p3-t2-nesting.md` quotes the whole edited `SelectRow` body and
      records that `_boundRoot.Length != 0` is still the first conjunct, so the no-bound-root
      pass-through mode is untouched.
- [ ] [P3-T3] Verify AC8: `SelectHierarchyPath` and `CommitSelection` are unmodified. Run
      `git add QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` then
      `git diff ecdb1c84ba8541ab67042985919cfed4df768c01 --cached -U0 -- QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs`
      and write `evidence/regression-testing/p3-t3-selectionfile-diff.md`. Acceptance: every hunk
      header in the diff addresses a line range that lies entirely within the original lines 83 to 107;
      no hunk touches the original line range 109 to 139; and the artifact lists the hunk headers
      verbatim.
- [ ] [P3-T4] Run the analyzer build and the nullable build using the P0-T13 and P0-T14 commands
      verbatim, and write `evidence/regression-testing/p3-t4-builds.md` recording both. Acceptance:
      both record `EXIT_CODE: 0`; both outputs contain `(Rebuild target(s))`; and neither recorded
      `Command:` line contains the solution-wide nullable opt-in property — record this as
      `NULLABLE_OPT_IN_PROPERTY: absent`; do not spell the token in the artifact.
- [ ] [P3-T5] Run the router regression suite green. Use the P2-T7 command with the results directory
      changed to `coverage\testresults\p3-t5`, and write
      `evidence/regression-testing/p3-t5-router-tests-green.md`. Acceptance: `EXIT_CODE: 0`; 10 tests
      total; 10 passed; 0 failed; 0 skipped; and the five tests that failed in P2-T7 are named
      individually in the artifact as now passing.
- [ ] [P3-T6] Run the unmodified router test classes to prove no collateral regression. Use the P2-T7
      command with the filter
      `"/TestCaseFilter:(FullyQualifiedName~BreadcrumbBridgeRouterIssue614Tests|FullyQualifiedName~BreadcrumbBridgeRouterTests|FullyQualifiedName~BreadcrumbBridgeRouterQueueTests)&TestCategory!=LiveOutlook"`
      and the results directory `coverage\testresults\p3-t6`, and write
      `evidence/regression-testing/p3-t6-router-siblings.md`. Acceptance: `EXIT_CODE: 0`; 0 failed; and
      the artifact records that `RowSelected_OutOfRootFilingTarget_DoesNotStoreAFullOutlookPath` and
      `SegmentActivate_WithNoBoundArchiveRoot_PreservesThePassThroughMode` both passed. This filter
      deliberately excludes `BreadcrumbBridgeRouterIssue439Tests`, whose pinning assertion is expected
      to be red between P3-T1 and P5-T1; that class is run green in P5-T6.

### Phase 4 — Change B, normalization in the `string` overload of `MoveToFolderAsync`

- [ ] [P4-T1] Replace the seam body in `QuickFiler/Controllers/EfcDataModel.cs` with the real
      normalization. `ToFilingStemOrVerbatim` returns `candidatePath` unchanged when
      `ArchiveStemContract.IsFullOutlookPath(candidatePath)` is false; otherwise it calls
      `ArchiveStemContract.TryMakeArchiveRelative(candidatePath, archiveAncestor, out string stem)` and
      returns `stem` when that call succeeds and `stem.Length != 0`, and returns `candidatePath`
      unchanged in every other case. The method is total, never throws, performs no I/O, writes no log,
      and touches no static mutable state. It deliberately does not adopt
      `ToArchiveRelativeStem`'s throw on the archive-root-exact input; the rationale is recorded in
      `spec.md` under "Error handling and logging updates". Update the XML documentation to state the
      final contract and remove the seam wording added by P2-T1. The helper's XML documentation must
      not contain the token `MoveToFolder`; refer to its caller as "the `string` filing overload"
      instead, because P8-T16 asserts the family stem search still returns exactly 16 lines. Three
      further tokens are barred from that documentation for the same reason — each is a token some
      acceptance condition asserts an exact count for over this same file, and the natural wording of
      the contract would otherwise add an occurrence. First, `IsFullOutlookPath`: AC12 phrases the
      contract as "The helper is gated on `ArchiveStemContract.IsFullOutlookPath`", but this task
      asserts an exact count of 1 for that token in this file, so the documentation states the gate as
      "returns its input unchanged unless the input is a full Outlook path" without naming the
      predicate. Second, the character sequence `throw` in any form, including `throws`: this task
      compares the matched line texts of `rg -n "throw"` taken before the edit against those taken
      after it and requires the two sets identical, so the totality claim is worded as
      "returns a value for every input and
      propagates no exception". Third, `Globals.Ol.ArchiveRootPath`: P4-T6 asserts an exact count of 4
      for that token in this file, so the documentation describes the second parameter as the archive
      ancestor supplied by the caller rather than naming the global. Record
      the run in
      `evidence/regression-testing/p4-t1-helper-implemented.md`, capturing the output of
      `rg -n "throw" QuickFiler/Controllers/EfcDataModel.cs` taken immediately before and immediately
      after the edit. Acceptance: the two `rg` outputs contain the identical set of matched line
      **texts**, compared without their line numbers because the helper body changes length and shifts
      every later line number, so the helper introduces no new throw site;
      `rg -n "IsFullOutlookPath" QuickFiler/Controllers/EfcDataModel.cs` returns exactly 1 line and it
      is inside the helper, where before this task it returned 0 lines; and
      `(Get-Content -LiteralPath "QuickFiler\Controllers\EfcDataModel.cs").Count` is at most 500.
- [ ] [P4-T2] Record the helper's line range and verify its purity, and write
      `evidence/regression-testing/p4-t2-helper-shape.md`. Acceptance: the artifact records the first
      and last line numbers of the `ToFilingStemOrVerbatim` declaration body; it records that the body
      contains no `await`, no `Globals`, no `logger`, and no `throw`; it records that the only call
      sites of the helper are the single assignment in the `string` overload and the eight tests in
      `EfcDataModelIssue637Tests`, verified by `rg -n "ToFilingStemOrVerbatim" --glob "*.cs" .`; and it
      enumerates every line `rg -n "ToFilingStemOrVerbatim" --glob "*.cs" QuickFiler/` returns and
      classifies each as the single declaration, the single call, or an XML-documentation reference,
      with exactly one declaration and exactly one call. A second call site anywhere in `QuickFiler/`
      fails this task.
- [ ] [P4-T3] Run the analyzer build and the nullable build using the P0-T13 and P0-T14 commands
      verbatim, and write `evidence/regression-testing/p4-t3-builds.md`. Acceptance: both record
      `EXIT_CODE: 0`; both outputs contain `(Rebuild target(s))`; and neither recorded `Command:` line
      contains the solution-wide nullable opt-in property — record this as
      `NULLABLE_OPT_IN_PROPERTY: absent`; do not spell the token in the artifact.
- [ ] [P4-T4] Run the helper test class green. Use the P2-T8 command with the results directory
      changed to `coverage\testresults\p4-t4`, and write
      `evidence/regression-testing/p4-t4-helper-tests-green.md`. Acceptance: `EXIT_CODE: 0`; 8 tests
      total; 8 passed; 0 failed; and the two tests that failed in P2-T8 are named individually as now
      passing.
- [ ] [P4-T5] Prove the eight existing `ToArchiveRelativeStem` tests are unchanged and still pass. Use
      the P2-T7 command with the filter
      `"/TestCaseFilter:FullyQualifiedName~EfcDataModelIssue614Tests&TestCategory!=LiveOutlook"` and the
      results directory `coverage\testresults\p4-t5`, and write
      `evidence/regression-testing/p4-t5-toarchiverelativestem-unchanged.md`. Acceptance:
      `EXIT_CODE: 0`; the run reports 8 tests for that class; 8 passed including
      `ToArchiveRelativeStem_ArchiveRootItself_Throws`; and, after
      `git add QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs`,
      `git diff ecdb1c84ba8541ab67042985919cfed4df768c01 --cached -- QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs`
      shows zero removed content lines.
- [ ] [P4-T6] Verify AC17: the non-goals are untouched. Run
      `git add QuickFiler/Controllers/EfcDataModel.cs` then
      `git diff ecdb1c84ba8541ab67042985919cfed4df768c01 --cached -U0 -- QuickFiler/Controllers/EfcDataModel.cs`
      and write `evidence/regression-testing/p4-t6-nongoals-untouched.md`. Acceptance: no hunk header
      addresses any line inside the original ranges 299 to 334 (`OpenOlFolderAsync` and
      `OpenFsFolderAsync`) or 336 to 386 (the `MAPIFolder` overload and `ToArchiveRelativeStem`); a
      pure-insertion hunk whose old-side range is `-297,0` or `-298,0` is the helper declaration
      required by P2-T1 and is expected; any other hunk outside the line-287 assignment fails this
      task; and `rg -n "Globals.Ol.ArchiveRootPath" QuickFiler/Controllers/EfcDataModel.cs` returns
      exactly 4 lines, quoted in the artifact and classified as the 3 pre-existing `OlAncestor`
      initializers (originally lines 289, 310 and 328, shifted by the length of the helper this plan
      adds) plus the single new argument on the `DestinationOlStem` assignment introduced by P2-T1;
      and none of the 4 is inside a `try` or `catch` block, verified by quoting the enclosing
      statement of each.

### Phase 5 — Change C, the recorded spec correction to the issue #439 assertion

- [ ] [P5-T1] In `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs`, replace line 165
      `            router.SelectedFolderPath.Should().Be(fullTarget);` with
      `            router.SelectedFolderPath.Should().Be(@"Clients\North");`. The expected value is
      derived from the fixture in the same method: `archiveRoot` is `@"\Archive"` at `:123` and
      `fullTarget` is `@"\aRcHiVe\Clients\North"` at `:124`, so `TryMakeArchiveRelative` matches through
      the `OrdinalIgnoreCase` `StartsWith` at `ArchiveStemContract.cs:131`, the boundary character at
      index 8 is a backslash per `:137-141`, and the stem is
      `fullTarget.Substring(8).TrimStart('\\','/')`, which is `Clients\North`. Acceptance: the
      fixed-string search
      `rg -F -n 'router.SelectedFolderPath.Should().Be(@"Clients\North");' QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs`
      returns exactly 1 line and it is line 165, and the fixed-string search
      `rg -F -n 'router.SelectedFolderPath.Should().Be(fullTarget);' QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs`
      returns 0 lines.
- [ ] [P5-T2] Rename the enclosing method at line 119 from
      `Issue439AlreadyRootedTargetRemainsUnchangedWithCaseInsensitiveArchiveMatch` to
      `Issue439RootedTargetUsesOriginalPathForProviderLookupCaseInsensitively`, on one line so the file
      line count is unchanged. Acceptance:
      `rg -n "Issue439AlreadyRootedTargetRemainsUnchangedWithCaseInsensitiveArchiveMatch" --glob "*.cs" .`
      returns 0 lines, and
      `rg -n "Issue439RootedTargetUsesOriginalPathForProviderLookupCaseInsensitively" --glob "*.cs" .`
      returns exactly 1 line.
- [ ] [P5-T3] Narrow the arrange comment at lines 121-122 to the provider claim it still supports,
      keeping it exactly two lines so the file line count is unchanged. The replacement text is:
      `            // Arrange: the presented target is rooted with casing different from the configured`
      and
      `            // root, so the provider must receive the original full path unchanged (#439).`
      Acceptance:
      `rg -n "so the provider must receive the original full path unchanged" QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs`
      returns exactly 1 line, and
      `rg -n "already rooted with casing different" QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs`
      returns 0 lines.
- [ ] [P5-T4] Verify AC19: the companion provider assertion and `ToHierarchyPath` are preserved. Write
      `evidence/regression-testing/p5-t4-provider-assertion-preserved.md`. Acceptance: lines 161 to 164
      of `BreadcrumbBridgeRouterIssue439Tests.cs` are byte-identical to their pre-change text, quoted
      in the artifact; and, after `git add QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`,
      `git diff ecdb1c84ba8541ab67042985919cfed4df768c01 --cached -U0 -- QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`
      produces no output at all, since this plan changes no line of that file.
- [ ] [P5-T5] Verify the file did not grow and that exactly one assertion changed. Run
      `pwsh -NoProfile -Command '(Get-Content -LiteralPath "QuickFiler.Test\Controllers\BreadcrumbBridgeRouterIssue439Tests.cs").Count'`,
      then `git add QuickFiler.Test` followed by
      `git diff ecdb1c84ba8541ab67042985919cfed4df768c01 --cached -- QuickFiler.Test`, and write
      `evidence/regression-testing/p5-t5-single-assertion-change.md`. Acceptance: the line count is
      exactly 694; among the diff's removed content lines, exactly one matches `.Should()`, and it is
      `            router.SelectedFolderPath.Should().Be(fullTarget);`; and the artifact records the
      change as a deliberate spec correction, stating that the issue #439 criterion that a rooted target
      survives selection is superseded by issue #614's archive-relative-stem invariant, which #614
      enforced on the `SelectHierarchyPath` half and at the filing boundary but not on the `SelectRow`
      half, and that this is explicitly not a weakened test; and the artifact additionally records the
      P5-T2 rename (both the removed and the added method name) and the two replacement comment lines
      from P5-T3, quoted verbatim, so that all three clauses of AC18 are evidenced in one artifact.
- [ ] [P5-T6] Run the issue #439 test class green. First re-run the P0-T13 analyzer build command
      verbatim so that the Phase 5 test edits are compiled into
      `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`; record its `EXIT_CODE:` and its
      `(Rebuild target(s))` line in the same artifact. Without this rebuild the scoped run would
      execute the assembly P4-T3 produced, which still carries the old method name and the old
      assertion, and its acceptance would be unsatisfiable. Then use the P2-T7 command with the filter
      `"/TestCaseFilter:FullyQualifiedName~BreadcrumbBridgeRouterIssue439Tests&TestCategory!=LiveOutlook"`
      and the results directory `coverage\testresults\p5-t6`, and write
      `evidence/regression-testing/p5-t6-issue439-green.md`. Acceptance: `EXIT_CODE: 0`; 0 failed; and
      the artifact records that both `Issue439RootedTargetUsesOriginalPathForProviderLookupCaseInsensitively`
      and `Issue439SlashOnlyArchiveRootPreservesFullHierarchySelection` passed.

### Phase 6 — Change D, stale-comment cleanup

- [ ] [P6-T1] Replace `QuickFiler/Controllers/EfcSelectionGuard.cs:30` with the fixed replacement text
      given in "Fixed identifiers", item 1. The surrounding claim that the guard still rejects rooted
      values stays as written; only the deferral wording changes. Acceptance:
      `rg -n "is implemented by issue #637" QuickFiler/Controllers/EfcSelectionGuard.cs` returns
      exactly 1 line, and `(Get-Content -LiteralPath "QuickFiler\Controllers\EfcSelectionGuard.cs").Count`
      is exactly 79.
- [ ] [P6-T2] Replace `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs:146` with the fixed
      replacement text given in "Fixed identifiers", item 2. Acceptance:
      `rg -n "the producer normalizes" QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs` returns
      exactly 1 line, and it is line 146.
- [ ] [P6-T3] Replace the `because` string at `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs:152`
      with the fixed replacement text given in "Fixed identifiers", item 3, on one line so the file
      line count is unchanged. Acceptance:
      `rg -n "the producer now normalizes before this predicate is reached" QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs`
      returns exactly 1 line, and
      `(Get-Content -LiteralPath "QuickFiler.Test\Controllers\EfcSelectionGuardTests.cs").Count` is
      exactly 296.
- [ ] [P6-T4] Verify the deferral is gone and the guard's behavior is unchanged. First re-run the
      P0-T13 analyzer build command verbatim so that the P6-T2 and P6-T3 test edits are compiled into
      `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`; record its `EXIT_CODE:` and its
      `(Rebuild target(s))` line in the same artifact. Without this rebuild the scoped run would
      execute the assembly P4-T3 produced, which predates those edits, so the run would not be
      evidence about the edited file that AC23 requires. Then run
      `rg -c "deferred to issue #637" --glob "*.cs" .`, then run the P2-T7 command with the filter
      `"/TestCaseFilter:FullyQualifiedName~EfcSelectionGuardTests&TestCategory!=LiveOutlook"` and the
      results directory `coverage\testresults\p6-t4`, and write
      `evidence/regression-testing/p6-t4-deferral-cleared.md`. The `*.cs` glob is load-bearing: the
      phrase remains present in `spec.md`, in the research file, and in this plan, all of which are
      Markdown and are correctly excluded. Acceptance: the `rg` invocation reports 0 matches and exits
      non-zero, recorded with `ExpectedExitCode: 1` for that step; the scoped test run records
      `EXIT_CODE: 0` with 0 failed; and the artifact records that
      `Issue614_GuardAcceptedSelection_DoesNotThrowAtFilingBoundary` passed and that neither
      `IsValidFilingSelection` nor `IsValidCreationSelection` had any executable line changed, verified
      by a `git diff ecdb1c84ba8541ab67042985919cfed4df768c01 --cached -- QuickFiler/Controllers/EfcSelectionGuard.cs`
      run in the same task after `git add QuickFiler/Controllers/EfcSelectionGuard.cs`, whose only
      changed line is line 30.
- [ ] [P6-T5] Commit changes A through D. Run
      `git add QuickFiler QuickFiler.Test docs/features/active/2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637`
      then
      `git commit -m "fix(637): normalize the breadcrumb producer and the string filing overload"` and
      write `evidence/other/p6-t5-commit.md`. A commit is required here because every Phase 7 and
      Phase 8 gate is anchored to `ecdb1c84ba8541ab67042985919cfed4df768c01..HEAD`, and an anchored diff
      reports nothing for changes that are not yet committed. Acceptance: `EXIT_CODE: 0`;
      `git status --porcelain -- QuickFiler QuickFiler.Test` produces no output; and
      `git diff --name-only ecdb1c84ba8541ab67042985919cfed4df768c01..HEAD -- QuickFiler QuickFiler.Test`
      lists exactly these eight paths and no others:
      `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs`,
      `QuickFiler/Controllers/EfcDataModel.cs`, `QuickFiler/Controllers/EfcSelectionGuard.cs`,
      `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs`,
      `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue637Tests.cs`,
      `QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs`,
      `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs`,
      `QuickFiler.Test/QuickFiler.Test.csproj`.

### Phase 7 — Final QC toolchain loop and coverage delta

Run the four steps in order. If any step fails or changes a file, return to P7-T1 and run the phase
again from the start.

- [ ] [P7-T1] Format. Record
      `git status --porcelain -- QuickFiler QuickFiler.Test docs/features/active/2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637`
      before the run, then run
      `pwsh -NoProfile -Command 'dotnet tool run csharpier format .; "EXIT_CODE=$LASTEXITCODE"'`, then
      record the same `git status --porcelain` invocation after the run, and write
      `evidence/qa-gates/p7-t1-csharpier-format.md`. The exit code alone proves nothing here because
      CSharpier exits 0 whether or not it rewrote a file, and its summary line was not observed in
      this planning session, so no acceptance condition here reads it. Acceptance: `EXIT_CODE: 0`; both porcelain
      outputs are recorded verbatim; and any path that differs between them is either a path this plan
      changed or a path listed in the `BASELINE_FORMAT_DRIFT` section of
      `evidence/baseline/p0-t12-csharpier-check.md`. A path that is in neither set means the repo-wide
      format pass touched unrelated source and must be reported to the orchestrator before proceeding.
- [ ] [P7-T2] Verify the format. Run
      `pwsh -NoProfile -Command 'dotnet tool run csharpier check .; "EXIT_CODE=$LASTEXITCODE"'` and
      write `evidence/qa-gates/p7-t2-csharpier-check.md`. Acceptance: `EXIT_CODE: 0`, and the captured
      stdout is recorded verbatim. The exit code is the gate here rather than any summary wording,
      because `check` is read-only and returns non-zero exactly when some file would be reformatted;
      the write-mode discrimination that a read-only command cannot supply is provided by P7-T1's
      before-and-after porcelain pair. Then, in this same task, run `git add QuickFiler QuickFiler.Test`
      and `git commit -m "style(637): apply csharpier formatting before the coverage gates"`, so that
      every subsequent `ecdb1c84ba8541ab67042985919cfed4df768c01..HEAD` diff describes the same file
      contents the P7-T5 build measured. If nothing changed, record that the commit was a no-op and
      that the tree already matched `HEAD`. Record the commit result in
      `evidence/qa-gates/p7-t2-csharpier-check.md`.
- [ ] [P7-T3] Analyzers. Run the P0-T13 command verbatim and write
      `evidence/qa-gates/p7-t3-msbuild-analyzers.md`. Acceptance: `EXIT_CODE: 0`; the output contains
      `(Rebuild target(s))`; the `Error(s)` count is 0; and the recorded `Command:` line contains
      `/t:Rebuild` and `EnableNETAnalyzers=true` and `EnforceCodeStyleInBuild=true` and contains
      neither `/t:Build` nor the solution-wide nullable opt-in property — record this as
      `NULLABLE_OPT_IN_PROPERTY: absent`; do not spell the token in the artifact.
- [ ] [P7-T4] Nullable. Run the P0-T14 command verbatim and write
      `evidence/qa-gates/p7-t4-msbuild-nullable.md`. Acceptance: `EXIT_CODE: 0`; the output contains
      `(Rebuild target(s))`; and the recorded `Command:` line contains `/t:Rebuild` and
      `TreatWarningsAsErrors=true` and contains neither `/t:Build` nor the solution-wide nullable
      opt-in property — record this as `NULLABLE_OPT_IN_PROPERTY: absent`; do not spell the token in
      the artifact.
- [ ] [P7-T5] Full test run with coverage. Run
      `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\p7-t5-postchange.cobertura.xml`
      and write `evidence/qa-gates/p7-t5-mstest-coverage.md`. The exit-code condition is stated
      against the recorded baseline rather than as an unconditional zero, because the wrapper throws
      whenever the inner vstest run reports any failure and a repository-wide zero-failure demand would
      be unsatisfiable if the baseline itself carried failures. Acceptance: the number of discovered
      test assemblies matches the number recorded in `evidence/baseline/p0-t15-mstest-coverage.md`; the
      post-change failing set is a subset of the `BASELINE_FAILURE_SET` recorded there; no test that
      passed in the baseline is failing now; the artifact names every baseline failure that is still
      failing; and when `BASELINE_FAILURE_SET` is empty, `EXIT_CODE: 0` and 0 failed are required.
      `Output Summary:` additionally carries the six numeric `/coverage` attribute values and the
      derived line and branch percentages that P7-T6 reads, copied in once P7-T6 has produced them;
      this task is not complete until that copy-back has been made, because the plan contract
      requires the final-QC test-step artifact itself to carry the numeric coverage headline.
- [ ] [P7-T6] Read the post-change numeric coverage headline. Run the P0-T16 command with the input
      path changed to `.\coverage\p7-t5-postchange.cobertura.xml` and write
      `evidence/qa-gates/p7-t6-coverage-headline.md`. Acceptance: `EXIT_CODE: 0`, and `Output Summary:`
      records all six numeric attribute values plus the derived line-coverage percentage and branch
      percentage.
- [ ] [P7-T7] Verify changed-line coverage. Run the P0-T17 command with the input path changed to
      `.\coverage\p7-t5-postchange.cobertura.xml`, and in the same task run
      `git diff -U0 ecdb1c84ba8541ab67042985919cfed4df768c01..HEAD -- QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs QuickFiler/Controllers/EfcDataModel.cs`
      to enumerate the added line numbers from the hunk headers. Re-derive the
      `ToFilingStemOrVerbatim` line range against the post-format working tree in this same task,
      recording the declaration line and the closing-brace line, and record both that range and the
      range `evidence/regression-testing/p4-t2-helper-shape.md` recorded, stating whether they differ.
      Every coverage assertion in this task is evaluated against the re-derived range; the P4-T2 range
      is recorded for audit only. This re-derivation is required because P4-T2 measured the range in
      Phase 4, P7-T1 then ran the write-mode formatter over `EfcDataModel.cs` — the first format pass
      over the hand-written helper body, since Phases 2 through 6 contain no format step — and P7-T5
      measured the tree after it, so a formatter change to the helper's extent would make the P4-T2
      range identify uncovered lines in `OpenOlFolderAsync` or unrelated covered lines instead of the
      helper. Write
      `evidence/qa-gates/p7-t7-changed-line-coverage.md`. Acceptance: the artifact lists, per file, the
      set of added line numbers and the set of line numbers with zero hits; the intersection of those
      two sets is empty for both files; and, for `QuickFiler/Controllers/EfcDataModel.cs`, every line
      number inside the re-derived `ToFilingStemOrVerbatim` range has non-zero hits, which is the
      new-code coverage requirement for the new helper stated in AC29; and, for the re-derived
      `ToFilingStemOrVerbatim` range, the artifact records the line nodes carrying `branch="True"`
      together with their `condition-coverage` values, and states that the `IsFullOutlookPath`
      conditional shows both branches taken — that is, a `condition-coverage` value of the form
      `100% (2/2)` on that line — which is AC29's "both sides of its gate exercised" clause. The
      capitalized `True` and the `condition-coverage` spelling are the forms the runner actually
      emits, observed in
      `docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/evidence/qa-gates/coverage-final.cobertura.xml`,
      where `branch="True"` occurs 11674 times and `branch="true"` occurs zero times; a lowercase
      match must not be substituted. If no line node inside the re-derived range carries
      `branch="True"` at all — the case where the compiler emits no separate branch point for the
      guard — the artifact records that observation verbatim and satisfies AC29's clause instead by
      naming the two fixed witness tests recorded green in
      `evidence/regression-testing/p4-t4-helper-tests-green.md`:
      `ToFilingStemOrVerbatim_RootedUnderAncestor_ReturnsTheStem` for the true side and
      `ToFilingStemOrVerbatim_RelativeStem_ReturnsTheInputVerbatim` for the false side. The executor
      does not choose between these two forms of proof: the branch-node form applies whenever a
      `branch="True"` node exists in the re-derived range, and the witness-test form applies only when
      none does. The `HEAD` this diff is anchored against already carries the formatting result,
      because P7-T2 committed it before P7-T5 measured the tree.
- [ ] [P7-T8] Report the coverage delta. Write `evidence/qa-gates/p7-t8-coverage-delta.md` containing
      three labelled numeric sections: baseline coverage, copied from
      `evidence/baseline/p0-t16-coverage-headline.md`; post-change coverage, copied from
      `evidence/qa-gates/p7-t6-coverage-headline.md`; and changed and new-code coverage, copied from
      `evidence/qa-gates/p7-t7-changed-line-coverage.md`. Acceptance: all three sections carry numeric
      values and none carries a placeholder; the post-change line-coverage percentage is at or above
      80, which is the floor the coverage runner itself enforces at
      `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:487-489` and which CLAUDE.md states, except
      that if the baseline figure recorded in `evidence/baseline/p0-t16-coverage-headline.md` is
      itself already below 80 the artifact records `BASELINE BELOW FLOOR`, reports that pre-existing
      condition to the orchestrator, and the binding requirement becomes that the post-change figure is
      at or above the recorded baseline figure; the
      artifact records the post-change figure against the 85 percent line and 75 percent branch figures
      in `.claude/rules/general-unit-test.md` and states explicitly which of the two repository-wide
      figures each threshold comes from, without resolving the conflict; and the changed-line section
      records an empty uncovered intersection.
- [ ] [P7-T9] File-size audit, run after the formatter rather than before it, because CSharpier can
      change a file's line count. Run
      `pwsh -NoProfile -Command 'foreach ($p in @("QuickFiler\Controllers\EfcDataModel.cs","QuickFiler\Controllers\BreadcrumbBridgeRouter.Selection.cs","QuickFiler\Controllers\EfcSelectionGuard.cs","QuickFiler.Test\Controllers\BreadcrumbBridgeRouterIssue439Tests.cs","QuickFiler.Test\Controllers\BreadcrumbBridgeRouterIssue637Tests.cs","QuickFiler.Test\Controllers\EfcDataModelIssue614Tests.cs","QuickFiler.Test\Controllers\EfcSelectionGuardTests.cs")) { $p + "=" + (Get-Content -LiteralPath $p).Count }'`
      and write `evidence/qa-gates/p7-t9-file-sizes.md`. Acceptance: `EfcDataModel.cs` is at most 500;
      `BreadcrumbBridgeRouterIssue637Tests.cs` is at most 500; `EfcDataModelIssue614Tests.cs` is at most
      500; `BreadcrumbBridgeRouterIssue439Tests.cs` is at most 694 and therefore has not grown, with
      the exact value recorded; `EfcSelectionGuard.cs` is at most 79, with the exact value recorded;
      `EfcSelectionGuardTests.cs` is at most 296, with the exact value recorded; and
      `BreadcrumbBridgeRouter.Selection.cs` is at most 500. The three upper bounds replace exact
      equalities because this task runs after a write-mode formatter that can reduce a line count,
      and AC25 requires only that these files not grow.
- [ ] [P7-T10] Toolchain non-vacuity audit. Write `evidence/qa-gates/p7-t10-toolchain-audit.md`
      enumerating the four final-QC command steps in order with their recorded `Command:` lines quoted
      verbatim. Acceptance: the format step is `dotnet tool run csharpier format .` invoked through
      `dotnet tool run` and not through a globally installed binary; both MSBuild `Command:` lines
      contain `/t:Rebuild` and neither contains `/t:Build`; no evidence artifact of this feature
      spells the solution-wide nullable opt-in token, verified by
      `rg -n 'Nullable=enable' docs/features/active/2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637 --glob '**/evidence/**/*.md' --glob '!**/p7-t10-toolchain-audit.md'`
      returning 0 matches and exiting non-zero, recorded with `ExpectedExitCode: 1` for that step;
      and both MSBuild transcripts contain `(Rebuild target(s))`. Three scope restrictions on that
      command are load-bearing. The directory operand must be this feature's folder rather than
      `docs/features/active`, because 121 evidence Markdown files under other feature folders in that
      tree contain the token and this plan cannot change them, so the parent-directory form can never
      return 0 and the gate could never pass. The restriction to the evidence subtree is required
      because `spec.md` and this plan both discuss the token in prose and both live at the root of
      this feature's folder, so a scan of the folder without the `**/evidence/**/*.md` glob would
      never return 0 either. The exclusion of `p7-t10-toolchain-audit.md` is required because this
      artifact must record its own scan under `Command:`, and that command's pattern is the token
      itself, so without the exclusion the gate would be defeated by the plan's own recording
      instruction. Every other evidence artifact of this feature is in scope, and the "Nullable opt-in
      token discipline" convention keeps each of them free of the token by recording
      `NULLABLE_OPT_IN_PROPERTY: absent` instead. This artifact
      additionally records the two deliberate substitutions against AC28's literal command list, each
      with its justification: `msbuild` is invoked through the vswhere-resolved absolute path rather
      than as a bare `msbuild` PATH entry, with the switch list character-for-character identical to
      AC28's, and the artifact records the output of
      `pwsh -NoProfile -Command 'if (Get-Command msbuild -ErrorAction SilentlyContinue) { "ON_PATH" } else { "NOT_ON_PATH" }'`
      stating which of the two outcomes was observed and what it means. When the probe
      records `NOT_ON_PATH`, the vswhere resolution is necessary and the artifact records it as such.
      When the probe records `ON_PATH`, the artifact additionally records the output of the command
      that prints the resolved source of the `msbuild` command —
      `pwsh -NoProfile -Command '(Get-Command msbuild).Source'` — and states that the vswhere
      resolution is used for determinism rather than necessity, because the PATH entry is not pinned
      by this repository and can resolve to a different MSBuild, and records whether the two resolved
      paths name the same binary; and the
      test step is `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, whose inner
      vstest call at `Invoke-MSTestWithCoverage.ps1:76` carries `/InIsolation` and
      `/TestCaseFilter:TestCategory!=LiveOutlook` and whose coverage is collected by
      `dotnet-coverage --output-format cobertura` rather than by `/EnableCodeCoverage`, which is the
      repository's standard runner and the local analogue of
      `.github/workflows/_mstest-coverage.yml:83`. The substitutions are recorded, not resolved.
- [ ] [P7-T11] Commit the QA evidence and any residual formatting result. Run
      `git add QuickFiler QuickFiler.Test docs/features/active/2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637`
      then
      `git commit -m "chore(637): final QC toolchain pass and coverage evidence"` and write
      `evidence/other/p7-t11-commit.md`. Acceptance: `EXIT_CODE: 0`, and
      `git status --porcelain -- QuickFiler QuickFiler.Test` produces no output; and
      `git status --porcelain -- docs/features/active/2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637`
      lists at most this task's own evidence
      artifact and this plan file, with every other feature-folder path already in `HEAD`. Record
      both outputs verbatim. This task's commit carries the Phase 7 evidence artifacts; the
      formatting result itself is normally already in `HEAD` because P7-T2 committed it, so a
      source-only no-op here is expected rather than exceptional. If the commit fails because nothing
      changed at all, record that outcome and both `git status` results.

### Phase 8 — Acceptance-criteria reconciliation

Each task below verifies one acceptance criterion against evidence already on disk and then changes
that criterion's `- [ ]` to `- [x]` in the `## Acceptance Criteria` section of `spec.md`. No criterion
is checked off before its cited evidence exists. Exactly one criterion is checked off per task.

- [ ] [P8-T1] AC1: cite `evidence/regression-testing/p3-t5-router-tests-green.md` showing
      `RowSelected_ArchiveRootExactFilingTarget_IsNotSelected` and
      `RowSelected_ArchiveRootExactFilingTarget_PreservesAPriorValidSelection` passing, and
      `evidence/regression-testing/p2-t7-router-tests-red.md` showing both failing before the fix.
      Acceptance: both artifacts exist and name both tests; AC1 is checked off.
- [ ] [P8-T2] AC2: cite `evidence/regression-testing/p3-t5-router-tests-green.md` showing
      `RowSelected_RootedTargetUnderArchiveRoot_CommitsTheArchiveRelativeStem` and
      `RowSelected_RootedTargetUnderArchiveRoot_CaseInsensitiveAndTrailingSeparatorRoot_CommitsTheStem`
      passing. Acceptance: the artifact names both tests as passing; AC2 is checked off.
- [ ] [P8-T3] AC3: cite `evidence/regression-testing/p3-t2-nesting.md` for the nesting inside the
      `IsFullOutlookPath` arm, and `evidence/regression-testing/p3-t5-router-tests-green.md` for
      `RowSelected_RelativeFilingTarget_CommitsTheValueVerbatim` passing. Acceptance: both artifacts
      exist and the nesting artifact quotes the edited body; AC3 is checked off.
- [ ] [P8-T4] AC4: cite `evidence/baseline/p1-t6-passthrough-tests.md` for the two existing tests,
      `evidence/regression-testing/p3-t6-router-siblings.md` and
      `evidence/regression-testing/p5-t6-issue439-green.md` for both passing unmodified, and
      `evidence/regression-testing/p3-t5-router-tests-green.md` for the new
      `RowSelected_RootedTargetWithNoBoundArchiveRoot_PassesThroughVerbatim`. Acceptance: all three
      artifacts exist and name the tests; AC4 is checked off.
- [ ] [P8-T5] AC5: cite `evidence/regression-testing/p3-t5-router-tests-green.md` for
      `RowSelected_TrashPseudoRow_CommitsTheSentinelVerbatim`, and
      `evidence/regression-testing/p4-t4-helper-tests-green.md` for
      `ToFilingStemOrVerbatim_TrashSentinel_ReturnsTheInputVerbatim`. Acceptance: both artifacts exist
      and name the tests; AC5 is checked off.
- [ ] [P8-T6] AC6: cite `evidence/regression-testing/p3-t2-nesting.md` for the preserved message
      literal, `evidence/regression-testing/p3-t5-router-tests-green.md` for
      `RowSelected_OutOfRootRootedTarget_IsStillRejected` and
      `RowSelected_SeparatorBoundaryNearMissTarget_IsStillRejected`, and
      `evidence/regression-testing/p3-t6-router-siblings.md` for
      `RowSelected_OutOfRootFilingTarget_DoesNotStoreAFullOutlookPath`. Acceptance: all three artifacts
      exist and name the tests; AC6 is checked off.
- [ ] [P8-T7] AC7: cite `evidence/regression-testing/p3-t2-nesting.md` recording that the new message
      contains no `@`, and `evidence/regression-testing/p3-t5-router-tests-green.md` for
      `RowSelected_ArchiveRootExactFilingTarget_IsNotSelected`, which asserts through
      `AssertRejectionDiagnosticWithoutIdentifiers`. Acceptance: both artifacts exist; AC7 is checked
      off.
- [ ] [P8-T8] AC8: cite `evidence/regression-testing/p3-t3-selectionfile-diff.md` showing no hunk in
      the original line range 109 to 139. Acceptance: the artifact exists and lists the hunk headers;
      AC8 is checked off.
- [ ] [P8-T9] AC9: re-run both P1-T1 searches against the post-change tree and write
      `evidence/qa-gates/p8-t9-selection-family-post.md`. Acceptance: both searches still return
      exactly 9 lines with the same classification of 2 declarations and 7 call sites, and no Family-B
      member appears in either result; AC9 is checked off.
- [ ] [P8-T10] AC10: cite `evidence/regression-testing/p3-t5-router-tests-green.md` for
      `SelectFirstRow_RootedTargetUnderArchiveRoot_CommitsTheArchiveRelativeStem`, which reaches
      `SelectRow` through the public `SelectFirstRow` at `BreadcrumbBridgeRouter.cs:196-203` rather
      than through the `rowSelected` inbound message. Acceptance: the artifact names that test as
      passing; AC10 is checked off.
- [ ] [P8-T11] AC11: cite `evidence/regression-testing/p4-t2-helper-shape.md` for the single
      `internal static` declaration, the single assignment call site, and the purity record, and
      `evidence/regression-testing/p4-t4-helper-tests-green.md` for the eight tests that invoke the
      helper directly without constructing an `EmailFiler`. Acceptance: both artifacts exist; AC11 is
      checked off.
- [ ] [P8-T12] AC12: cite `evidence/regression-testing/p4-t4-helper-tests-green.md` for
      `ToFilingStemOrVerbatim_RelativeStem_ReturnsTheInputVerbatim` and
      `ToFilingStemOrVerbatim_TrashSentinel_ReturnsTheInputVerbatim`. Acceptance: the artifact names
      both tests as passing; AC12 is checked off.
- [ ] [P8-T13] AC13: cite `evidence/regression-testing/p4-t4-helper-tests-green.md` for
      `ToFilingStemOrVerbatim_RootedUnderAncestor_ReturnsTheStem` and
      `ToFilingStemOrVerbatim_RootedUnderCaseDifferingAncestor_ReturnsTheStem`, together with
      `evidence/regression-testing/p2-t8-helper-tests-red.md` showing both failing before the fix.
      Acceptance: both artifacts exist and name both tests; AC13 is checked off.
- [ ] [P8-T14] AC14: cite `evidence/regression-testing/p4-t4-helper-tests-green.md` for
      `ToFilingStemOrVerbatim_ArchiveRootExact_ReturnsTheInputVerbatimAndDoesNotThrow`,
      `ToFilingStemOrVerbatim_OutOfRootRootedInput_ReturnsTheInputVerbatimAndDoesNotThrow`,
      `ToFilingStemOrVerbatim_NullEmptyWhitespaceOrSeparatorOnlyAncestor_ReturnsTheInputVerbatim` and
      `ToFilingStemOrVerbatim_NullOrEmptyCandidate_ReturnsTheInputVerbatim`, and
      `evidence/regression-testing/p4-t2-helper-shape.md` for the record that the body contains no
      `throw`. Acceptance: both artifacts exist and name all four tests; AC14 is checked off.
- [ ] [P8-T15] AC15: cite `evidence/baseline/p1-t5-toarchiverelativestem-tests.md` for the count of 8
      and the declaration line numbers, and
      `evidence/regression-testing/p4-t5-toarchiverelativestem-unchanged.md` for the zero-removed-line
      diff and the 8 passing results including `ToArchiveRelativeStem_ArchiveRootItself_Throws`, and
      `evidence/regression-testing/p4-t6-nongoals-untouched.md` for the unmodified `MAPIFolder`
      overload and its call at the original line 345. Acceptance: all three artifacts exist; AC15 is
      checked off.
- [ ] [P8-T16] AC16: re-run both P1-T2 searches against the post-change tree and write
      `evidence/qa-gates/p8-t16-movetofolder-family-post.md`. Acceptance: the syntax-anchored search
      still returns exactly 9 lines classified as 3 declarations and 6 call sites, the stem search
      still returns 16 lines across 5 files, and no new overload and no signature change appears; AC16
      is checked off.
- [ ] [P8-T17] AC17: cite `evidence/regression-testing/p4-t6-nongoals-untouched.md`. Acceptance: the
      artifact shows no hunk in the original ranges 299 to 334 and 336 to 386 and records that no
      `Globals.Ol.ArchiveRootPath` read gained a `try` or `catch`; AC17 is checked off.
- [ ] [P8-T18] AC18: cite `evidence/regression-testing/p5-t5-single-assertion-change.md`, which
      records all three clauses. Acceptance: the artifact records the corrected assertion at
      line 165, the renamed method, and the narrowed two-line comment; AC18 is checked off.
- [ ] [P8-T19] AC19: cite `evidence/regression-testing/p5-t4-provider-assertion-preserved.md`.
      Acceptance: the artifact quotes lines 161 to 164 byte-identically and shows no diff hunk over
      `ToHierarchyPath` at the original `BreadcrumbBridgeRouter.cs:152-167`; AC19 is checked off.
- [ ] [P8-T20] AC20: cite `evidence/baseline/p1-t8-pinning-assertion.md` for the pre-change derivation
      by two independent constructions, and
      `evidence/regression-testing/p5-t5-single-assertion-change.md` for the post-change diff showing
      exactly one removed `.Should()` line across the whole `QuickFiler.Test` tree. Acceptance: both
      artifacts exist and agree that the count is 1; AC20 is checked off.
- [ ] [P8-T21] AC21: write `evidence/other/p8-t21-spec-correction-record.md` carrying the change
      description text for this correction, copied from the record P5-T5 wrote into
      `evidence/regression-testing/p5-t5-single-assertion-change.md`. This artifact is the designated
      source text for the pull-request change description, so the statement is owned by this plan
      rather than deferred to a step outside it. Acceptance: the artifact states that the issue #439
      criterion that a rooted target survives selection is superseded by issue #614's
      archive-relative-stem invariant, which #614 enforced on the `SelectHierarchyPath` half and at the
      filing boundary but not on the `SelectRow` half, and that the change is a deliberate spec
      correction and explicitly not a weakened test; AC21 is checked off.
- [ ] [P8-T22] AC22: cite `evidence/baseline/p1-t4-deferral-records.md` for the pre-change count of 3
      and `evidence/regression-testing/p6-t4-deferral-cleared.md` for the post-change count of 0 over
      `*.cs`. Acceptance: both artifacts exist and the post-change count is 0; AC22 is checked off.
- [ ] [P8-T23] AC23: cite `evidence/regression-testing/p6-t4-deferral-cleared.md` for the scoped
      `EfcSelectionGuardTests` run with 0 failures including
      `Issue614_GuardAcceptedSelection_DoesNotThrowAtFilingBoundary`, and for the single-line diff over
      `EfcSelectionGuard.cs`. Acceptance: the artifact exists and records both; AC23 is checked off.
- [ ] [P8-T24] AC24: re-run both P1-T3 searches against the post-change tree and write
      `evidence/qa-gates/p8-t24-selectedfolderpath-post.md`. Acceptance: the production surface is
      still 9 lines across 3 files with 2 writes and 3 reads, no new write site appears, no new public
      API member appears, and `rg -n "public string\? SelectedFolderPath \{ get; private set; \}" QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`
      returns exactly 1 line; AC24 is checked off.
- [ ] [P8-T25] AC25: cite `evidence/qa-gates/p7-t9-file-sizes.md`. Acceptance: the artifact shows every
      listed file at or under 500 lines, `BreadcrumbBridgeRouterIssue439Tests.cs` at or under 694 and
      therefore not grown, and it records that the spec's stated 424 for `EfcDataModel.cs` was 423 on
      the tree before the change; AC25 is checked off. The bound is stated as "at or under" rather
      than "exactly" for the same reason it is in P7-T9: the figure is read after a write-mode
      formatter that can reduce a line count, and AC25 requires only non-growth.
- [ ] [P8-T26] AC26: cite `evidence/regression-testing/p2-t9-compile-include-observed.md` for the
      `Compile Include` line and the 10 observed test results, and
      `evidence/regression-testing/p3-t5-router-tests-green.md` for the same 10 tests executing after
      the fix. Acceptance: both artifacts exist and both record 10 executed tests; AC26 is checked off.
- [ ] [P8-T27] AC27: cite `evidence/qa-gates/p7-t4-msbuild-nullable.md` for the clean nullable build,
      and verify in the same task that
      `rg -n "^#nullable enable" QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` returns
      line 1 and that `evidence/regression-testing/p3-t2-nesting.md` records that `stem` is a
      non-nullable `string` passed to `CommitSelection` without a nullable temporary. Acceptance: all
      three checks hold; AC27 is checked off.
- [ ] [P8-T28] AC28: cite `evidence/qa-gates/p7-t10-toolchain-audit.md`. Acceptance: the artifact
      quotes all four final-QC commands verbatim in order, shows `/t:Rebuild` on both MSBuild lines,
      records `NULLABLE_OPT_IN_PROPERTY: absent` for every one of the four quoted final-QC `Command:`
      lines — do not spell the token in this task's own record — and shows every step recording
      `EXIT_CODE: 0` in the final pass; and the artifact records the two deliberate substitutions
      against AC28's literal command list, each with its justification: `msbuild` is invoked through
      the vswhere-resolved absolute path rather than as a bare `msbuild` PATH entry, with the switch
      list character-for-character identical to AC28's and with the recorded `ON_PATH` or
      `NOT_ON_PATH` observation P7-T10 captured together with the statement of what that outcome
      means: on `NOT_ON_PATH` the artifact must record the vswhere resolution as necessary, and on
      `ON_PATH` it must record the resolved source of the `msbuild` command, state that the vswhere
      resolution is used for determinism rather than necessity because the PATH entry is not pinned
      by this repository, and record whether the two resolved paths name the same binary; and the
      test step is
      `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, whose inner vstest call at
      `Invoke-MSTestWithCoverage.ps1:76` carries `/InIsolation` and
      `/TestCaseFilter:TestCategory!=LiveOutlook` and whose coverage is collected by
      `dotnet-coverage --output-format cobertura` rather than by `/EnableCodeCoverage`, which is the
      repository's standard runner and the local analogue of
      `.github/workflows/_mstest-coverage.yml:83`. The substitutions are recorded, not resolved.
      AC28 is checked off.
- [ ] [P8-T29] AC29: cite `evidence/baseline/p0-t16-coverage-headline.md`,
      `evidence/qa-gates/p7-t6-coverage-headline.md`,
      `evidence/qa-gates/p7-t7-changed-line-coverage.md` and
      `evidence/qa-gates/p7-t8-coverage-delta.md`. Acceptance: the baseline capture is under
      `evidence/baseline/` and the post-change capture under `evidence/qa-gates/`, with no artifact
      written to `evidence/coverage/` or to any path under `artifacts/`; the changed-line uncovered
      intersection is empty; every line of the new helper has non-zero hits, judged against the
      `ToFilingStemOrVerbatim` range P7-T7 re-derived against the post-format working tree rather than
      against the pre-format range `evidence/regression-testing/p4-t2-helper-shape.md` recorded, with
      P7-T7's record of whether the two ranges differ cited here; the artifact
      `evidence/qa-gates/p7-t8-coverage-delta.md` records either a post-change line-coverage
      percentage at or above 80 or an explicit `BASELINE BELOW FLOOR` finding with the post-change
      figure at or above the recorded baseline; the `IsFullOutlookPath` conditional in the new helper
      shows both branches taken, per the `condition-coverage` values P7-T7 recorded, or — when P7-T7
      records that the helper's range carries no `branch="True"` node — per the two witness tests
      P7-T7 names; and AC29 is checked off.
- [ ] [P8-T30] AC30: verify no behavior outside changes A through D was altered. Run three commands in
      this task, in this order. First the porcelain companion,
      `git status --porcelain -- QuickFiler QuickFiler.Test UtilitiesCS UtilitiesCS.Test TaskMaster TaskMaster.Test ToDoModel Tags TaskVisualization`.
      Second
      `git diff --name-only ecdb1c84ba8541ab67042985919cfed4df768c01..HEAD -- QuickFiler QuickFiler.Test`.
      Third
      `git diff --name-only ecdb1c84ba8541ab67042985919cfed4df768c01..HEAD -- UtilitiesCS UtilitiesCS.Test TaskMaster TaskMaster.Test ToDoModel Tags TaskVisualization`.
      Write `evidence/qa-gates/p8-t30-scope-boundary.md` recording all three outputs verbatim. The
      porcelain companion is required because a name-listing diff enumerates tracked changes only and
      never reports an untracked path, so the two diffs alone cannot fail on a file this plan created
      and left uncommitted. At the point this task runs the division of labour between the two
      mechanisms is fixed and is stated here: P6-T5 committed changes A through D, P7-T2 committed the
      formatting result, and P7-T11 committed the Phase 7 evidence, so both anchored diffs carry the
      enumeration assertion, and the porcelain
      span is expected to be empty because every path it covers is already in `HEAD`. That emptiness
      is itself the assertion and not a null result — an untracked or unstaged file anywhere in those
      nine trees, whether a new test file, a stray source file, or an evidence artifact written
      outside the feature folder, appears in the porcelain output and in neither diff, so any line in
      that output fails this task and must be reported to the orchestrator. The porcelain pathspec
      omits `docs/features/active` deliberately: Phase 8 writes evidence artifacts and edits `spec.md`
      under that path and P8-T33 commits them afterwards, so a porcelain span covering it would be
      non-empty for reasons this gate is not measuring. Acceptance: the porcelain invocation produces
      no output; the second command lists the eight paths enumerated in P6-T5, plus — only when the
      `BASELINE_FORMAT_DRIFT` section of `evidence/baseline/p0-t12-csharpier-check.md` is non-empty —
      the paths in that section that lie under `QuickFiler` or `QuickFiler.Test`, each of which the
      artifact must show as a formatting-only change committed by P7-T2, and no others; when
      `BASELINE_FORMAT_DRIFT` is empty the list is exactly the eight paths; the third command
      produces no output, which is the evidence that `UtilitiesCS`, `TaskMaster`,
      `ToDoModel`, `Tags`, `TaskVisualization`, `UtilitiesCS.Test` and `TaskMaster.Test` contain no
      changed file; and AC30 is checked off.
- [ ] [P8-T31] Verify all thirty criteria are checked off, using two independently constructed
      section-scoped counts. Construction 1, range-scoped: extract the lines of `spec.md` between the
      line matching `^## Acceptance Criteria$` and the line matching `^## Risks & Mitigations$`, and
      count within that slice the lines matching `^- \[x\] AC` and the lines matching `^- \[ \] AC`.
      Construction 2, token-scoped and independent of the range extraction: count over the whole file
      the lines matching `^- \[x\] AC` and the lines matching `^- \[ \] AC`. Construction 2 is
      section-discriminating by token rather than by range, because the five checkboxes outside the
      section at `spec.md:54`, `:55`, `:56`, `:57` and `:86` are not followed by `AC`. Write
      `evidence/qa-gates/p8-t31-ac-reconciliation.md`. Acceptance: both constructions report 30 checked
      and 0 unchecked; both agree; and the artifact records that an unscoped count of every `- [x]` and
      `- [ ]` line in `spec.md` would over-report by exactly 5, naming those five line numbers.
- [ ] [P8-T32] Record the spec-versus-tree discrepancies found during this work in
      `evidence/other/p8-t32-spec-tree-discrepancies.md`. Acceptance: the artifact records the three
      items listed under "Tree observations recorded while authoring this plan" that concern `spec.md`
      citations — the 423-versus-424 line count for `EfcDataModel.cs`, the `:167-214`-versus-`:167-213`
      span of `Issue614_GuardAcceptedSelection_DoesNotThrowAtFilingBoundary`, and the
      `:143-147`-versus-`:143-146` span of the clear-on-rebind block — and states for each that no
      acceptance criterion's binding clause is affected. `spec.md` itself is not edited for these; only
      the acceptance-criteria checkboxes are edited by this phase.
- [ ] [P8-T33] Final commit and clean tree. Run
      `git add docs/features/active/2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637`
      then
      `git commit -m "docs(637): reconcile acceptance criteria and record final evidence"` and write
      `evidence/other/p8-t33-final-commit.md`. Acceptance: `EXIT_CODE: 0`, and
      `git status --porcelain -- QuickFiler QuickFiler.Test` produces no output; and
      `git status --porcelain -- docs/features/active/2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637`
      lists at most this task's own evidence
      artifact and this plan file, with every other feature-folder path already in `HEAD`. Record
      both outputs verbatim. The pathspec scoping is required because `.claude/` is tracked and
      carries unrelated in-flight modifications that this plan must not commit, and because sibling
      feature folders under `docs/features/active` are owned by other work. No sibling folder there is
      untracked at planning time, but this task runs long after planning, and a concurrent run in this
      checkout can leave an untracked or modified sibling folder under that parent directory before
      this task executes; a `git add` over the parent directory would then commit another feature's
      work onto this branch, and a `git status --porcelain` span over the parent directory would report
      that folder and make this gate unsatisfiable. Both spans are therefore scoped so that this gate
      cannot depend on state this plan does not own. The feature-folder
      span is stated separately because this task necessarily writes its own artifact after the
      commit and checks off its own box in this plan file, both of which live under this feature's
      folder.
