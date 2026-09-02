# Preflight Round 1 — Delta (issue 662)

- Timestamp: 2026-08-31T21-40
- Directive: `DIRECTIVE: PREFLIGHT VALIDATION ONLY`
- Reviewer: atomic-executor (validation-only pass; nothing was executed)
- Plan under review: `docs/features/active/efcselectionguard-banner-prefix-arity-and-stale-comment-662/plan.2026-08-31T20-11.md`
- Tree reviewed: HEAD `e592f0c7`, base `2b85134b42872e405602e6064e02dc9cda6c319b`
- Signal: `PREFLIGHT: REVISIONS REQUIRED`
- Convergence: `CONVERGENCE: NO FURTHER ROUNDS EXPECTED`

This artifact records the round-1 review verbatim so the plan revision is auditable and so a
later reader can see which defects were found before execution rather than during it.

---

## Verified correct in round 1 (no change required)

Re-derived against the tree:

- **Every count assertion.** P0-T14 (1/1), P0-T15 (2/2), P0-T16 (3 primary, 9-member superset
  cross-check), P0-T17 (no output, exit 1), P1-T2, P1-T3, P1-T5, P1-T6, P1-T7, P1-T8, P1-T9,
  P2-T11, P2-T12, P2-T13, P2-T14, P2-T15. Pre-change measurements: `= "===";` gives 1
  (`EfcSelectionGuard.cs:15`); `= "====";` gives 2 (`BreadcrumbRowBuilder.cs:19`,
  `FolderSuggestionTree.cs:16`); the declaration regex gives 3; `("===")` and `("====")` in
  `EfcSelectionGuardTests.cs` give 0 each; `three-character rejection` gives 0 repo-wide in
  `*.cs`; `must not be widened` gives 0 repo-wide in `*.cs`;
  `IsValidSelection keeps its "====" rejection` gives 1 (`EfcFormController.cs:319`).
- **The substring trap is controlled.** Confirmed empirically that `git grep -nE -- '"={3}";'`
  does not cross-match the four-character literal, and that `("===")` does not match `("====")`.
  All 16 count assertions carry either `-- '*.cs'` or a single-file pathspec; none is unscoped.
- **The `git grep -c` line-versus-occurrence trap does not bite.** P1-T5, P1-T8, P1-T9 and
  P2-T14 each assert a token that the mandated code shape places on its own statement, so lines
  and occurrences coincide.
- **Line citations.** All verified correct, including the finding that
  `LiveOutlookHookupIntegrationTests.cs:72` carries the repository's only
  `[TestCategory("LiveOutlook")]` attribute; the other two hits are doc comments.
- **The Directional Constraint mechanism.** Traced through the source. With the guard widened,
  `:462` still passes and `:463` fails. No task in the plan produces the prohibited edit, and
  P1-T2's second command is a genuine value-preservation control.
- **D1 and D7.** An aliasing declaration would match the AC2 regex, so deletion is required.
  D7's 92-character figure for both renamed call sites is correct (83 today, plus 9 for the
  longer identifier), and no `.csharpierrc` exists so print width is 100.
- **D3 versus AC5.** After deletion, `BannerPrefix` appears on one line of
  `FolderSuggestionTree.cs`. The rewritten reader measures 132 characters and CSharpier will
  wrap it, but the identifier stays on one line, so AC5's assertion survives the format pass.
- **AC5b and AC7 anchoring.** Both `git diff` gates carry an explicit ref operand, and both
  files are currently at their base state.
- **Evidence paths.** Every path resolves under the canonical feature `evidence/<kind>/` tree.
  No `artifacts/`-rooted evidence path appears anywhere.
- **Environment claims.** `.dotnet-sdk`, `packages/` and a nested `.claude/worktrees/` are all
  absent; `global.json` pins 8.0.205; `dotnet-tools.json` pins csharpier 1.2.6; `coverage.config`
  carries exactly the seven module exclusions named; Meziantou.Analyzer is pinned at 3.0.194;
  `spec.md` and `user-story.md` do not exist.

---

## Blocking defects

**B1 — `$msbuild` and `$vstest` are unbound in every task that uses them.** The plan resolves
them in a prose block and then uses them inside per-task command spans in P0-T8, P0-T9, P0-T11,
P0-T12, P2-T3, P2-T4, P2-T5, P2-T6, P2-T7, P2-T8. An executor runs each task as its own shell
invocation and shell state does not persist between invocations, so all ten commands would run
with a null command name and fail. The plan also gives no invocation wrapper for running
PowerShell syntax from the executor's shell.

**B2 — P1-T1 mandates a delegation the executor cannot perform.** "Hand the constrained
implementation to the small-path C# implementation engineer" requires a delegation tool. The
atomic-executor has Read, Grep, Glob, Edit, Write, Bash and the PoshQC MCP functions, and no
agent-invocation tool. As written the task is unexecutable with no alternative completion path.

**B3 — `/EnableCodeCoverage` is paired with a runsettings that carries no coverage exclusions.**
P0-T11, P0-T12, P2-T7 and P2-T8 pass `/EnableCodeCoverage` together with
`/Settings:scripts\vscode\TaskMaster.cli.runsettings`. That file contains MSTest parallelization
only; its own documentation in `Invoke-MSTestWithCoverage.ps1:20-26` states it deliberately
carries no coverage data collector. The repo-root `TaskMaster.runsettings` is the file carrying
the Code Coverage collector with the module exclusions. Both `QuickFiler.Test/packages.config`
and `UtilitiesCS.Test/packages.config` reference `Deedle 3.0.0` and `FSharp.Core 11.0.100`, so
the four runs would instrument exactly the modules the repository maintains two exclusion lists
to keep out. AC8 requires `failed="0"` from those runs.

**B4 — P0-T6 and P0-T7 detect a fatal precondition but carry no stop branch.** P2-T1 formats the
whole repository. If either protected file already carries CSharpier drift, P2-T1 rewrites it and
AC5b and AC7 become unsatisfiable; AC9's repo-wide clean-format requirement and those two
zero-diff gates are then mutually unsatisfiable with no in-plan resolution. P0-T7 is diagnostic
only. The same gap applies to P0-T6 for drift anywhere else in the tree, because P2-T1's own
acceptance halts the run if the format pass touches any file outside the four in-scope files.

**B5 — The coverage baseline and post-change captures can record two different metrics.**
`Invoke-MSTestWithCoverage.ps1` writes the post-processed document at line 343, after
`Assert-CoberturaLineCoverageThreshold` at line 341 and after the test-failure throw at line 236.
Whenever the script throws, the file left at `coverage\coverage.cobertura.xml` is
dotnet-coverage's raw output. The four root attributes are readable either way, but they are not
the same measurement: `ConvertTo-KoverageCoberturaXml` removes non-allowlisted packages, rewrites
`<class filename>` to repository-relative paths, merges duplicate class nodes by filename, and
recomputes `line-rate`, `lines-covered` and `lines-valid` (Helpers lines 411-445). If baseline and
post-change land in different states, P2-T10 compares an all-modules figure against a first-party
figure. Further: no task records which state the copied file is in; "the `<class>` node whose
filename ends with EfcSelectionGuard.cs" is singular but the raw pre-merge document can carry more
than one; and P0-T13's `NOT APPLICABLE` fallback attributes a missing class node to
`coverage.config` module exclusions when the actual filter is `Get-KoverageProjectAllowlist`,
which contains both owned projects.

**B6 — P2-T10 records no numeric changed-code coverage.** The plan states the changed-code figure
as prose. The coverage evidence contract requires baseline, post-change, and new/changed-code
coverage as recorded values.

**B7 — P2-T20 checks off AC9 against an incomplete artifact set.** AC9 names four toolchain steps
ending in test and requires an evidence artifact for each. P2-T20 requires only the four
format/analyze/type-check artifacts. The Phase 2 loop-restart rule has the same boundary: it
covers P2-T1 through P2-T4 only, so a test-step failure has no restart instruction.

**B8 — No gate constrains the change set to the four in-scope files.** CSharpier 1.2.6 also
processes `*.xml` and `packages.config`, so P2-T1 can rewrite files outside `QuickFiler`,
`UtilitiesCS` and `QuickFiler.Test`. P2-T1's tree observation is scoped to those three
directories and would not see such a rewrite; the `git add` pathspecs in P1-T11 and P2-T23 would
not stage it; and P2-T23's scoped status would still report clean. The run can end with
uncommitted out-of-scope formatter changes that no acceptance condition observes.

**B9 — The three clean-tree gates are self-falsifying.** P0-T18, P1-T11 and P2-T23 each require
an empty `git status --porcelain` under a pathspec containing the tracked plan file. The executor
must flip that task's own checkbox after the commit, which re-dirties the tree immediately.
P2-T23 compounds this by stating "write no further artifact after it", which forbids the commit
that would carry its own check-off.

---

## Non-blocking defects

**N1 — P0-T3's `dotnet --list-sdks` condition is unlikely to be satisfiable.** `--list-sdks`
enumerates SDKs under the host root and does not consult the `global.json` `paths` array. The
repo-local SDK is extracted to `.dotnet-sdk\sdk\8.0.205`, which is directly checkable on disk.
Separately, `rollForward: latestFeature` permits any `8.0.2xx` patch, so pinning
`dotnet --version` to exactly `8.0.205` is tighter than the configuration guarantees.

**N2 — P2-T12's causal sentence is incomplete.** It attributes the declaration count falling from
three to one solely to the rename. The rename accounts for one decrement; the P1-T6 deletion
accounts for the other.

**N3 — P1-T4 does not bound how many times the test identifier may appear in the doc,** while
P2-T13 asserts exactly one matching line.

**N4 — P1-T5 does not bound the replacement comment's line count.** `EfcFormController.cs` is
1189 lines and already exceeds the 500-line limit in `.claude/rules/general-code-change.md`.

**N5 — P2-T10's 0.50 percentage-point allowance is self-derived** with no recorded measurement
behind the figure.

**N6 — P0-T3 and P0-T13 both require network access.** Neither states a failure branch.

---

## Plan delta (apply all; no task is added or removed, and every task ID is unchanged)

### Toolchain prose section

Replace the paragraph beginning "Neither `msbuild` nor `vstest.console.exe` nor `vswhere` is on
`PATH`" with:

> Neither `msbuild` nor `vstest.console.exe` nor `vswhere` is on `PATH`. Shell state does not
> persist between tasks: each task runs in its own shell invocation, so a variable assigned in one
> task is unbound in the next. Every task whose command uses `$msbuild` or `$vstest` therefore
> repeats the resolution inline, in the same command span, immediately before the command it
> feeds. The mandatory prelude is the following lines, and every affected command span below
> begins with them:
>
> ```powershell
> $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
> $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
> $vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
> ```
>
> Tasks using only `$msbuild` may omit the `$vstest` line and vice versa. Each affected task runs
> its whole span as a single PowerShell invocation; when the executor's shell is not PowerShell,
> the span is passed to `pwsh -NoProfile -Command` as one argument. The `Command:` field of each
> artifact records the resolved absolute path actually used, not the variable name.

### P0-T3 — replace the acceptance sentence

> Acceptance: `Test-Path .dotnet-sdk\sdk\8.0.205` run from the worktree root returns `True`, and
> `dotnet --version` run from the worktree root prints a version in the `8.0.2` feature band; both
> outputs are transcribed into `Output Summary:`. Do not assert against `dotnet --list-sdks`: that
> command enumerates SDKs under the host root and does not consult the `global.json` `paths`
> array, so it is not expected to name the repo-local install. This step downloads the SDK zip
> from `builds.dotnet.microsoft.com`; if the download fails, stop and report BLOCKED naming the
> network failure, because every later `dotnet`, `msbuild` and coverage task depends on it.

### P0-T6 — append to the acceptance

> If the exit code is non-zero, list every file the check names. If that list contains any file
> outside the four in-scope files named in the Scope Boundary, stop and report BLOCKED before
> Phase 1 begins: the repository-wide format pass in P2-T1 would rewrite those files, which
> P2-T1's own acceptance halts on, and the plan carries no path that both leaves them unformatted
> and satisfies AC9.

### P0-T7 — replace the trailing rationale sentence with a branch

> If either command exits non-zero, stop and report BLOCKED before Phase 1 begins. Do not proceed
> and do not attempt a workaround. The reason is that the two requirements are then mutually
> unsatisfiable: AC9 requires a repository-wide CSharpier-clean tree, and AC5b and AC7 require
> these two files to show an empty diff against the base commit. Formatting the file satisfies AC9
> and fails AC5b or AC7; leaving it unformatted does the reverse. Resolving that conflict requires
> a plan revision, not an executor decision.

### P0-T11 and P0-T12

In both command spans, replace `/Settings:scripts\vscode\TaskMaster.cli.runsettings` with
`/Settings:TaskMaster.runsettings`, and prepend the `$vstest` prelude. Append to both task texts:

> The runsettings is the repository-root `TaskMaster.runsettings`, not the `scripts\vscode` CLI
> variant. `/EnableCodeCoverage` activates the Code Coverage collector, and only the
> repository-root file supplies that collector's module exclusions. The CLI variant carries MSTest
> parallelization only and no collector configuration, so pairing it with `/EnableCodeCoverage`
> instruments `Deedle` and `FSharp.Core`, which both `QuickFiler.Test/packages.config` and
> `UtilitiesCS.Test/packages.config` reference and which the repository excludes in both
> `coverage.config` and `TaskMaster.runsettings` for that reason.

### P0-T13 — replace from "Acceptance:" to the end of the task

> Acceptance: the artifact records the observed `EXIT_CODE:` and, when that value is non-zero,
> `ExpectedExitCode:` set to the same value together with the script line that produced it.
> `Output Summary:` carries, in this order: (1) a `PostProcessed:` field whose value is `yes` when
> the `<class filename>` attributes in the copied Cobertura are repository-relative and `no` when
> they are absolute host paths — this is the discriminator, because `ConvertTo-KoverageCoberturaXml`
> always rewrites those paths and the script writes the post-processed document only at
> `Invoke-MSTestWithCoverage.ps1:343`, after both the test-failure throw at `:236` and the
> threshold assertion at `:341`; (2) the root `line-rate`, `lines-covered` and `lines-valid`
> attributes; (3) the line percentage derived as `lines-covered / lines-valid * 100` to two decimal
> places; (4) for each of `EfcSelectionGuard.cs` and `FolderSuggestionTree.cs`, the count of
> `<class>` nodes whose `filename` ends with that name and, for each such node, its `line-rate` —
> when `PostProcessed:` is `no` there may be more than one node per filename, because
> `Merge-CoberturaClassesByFilename` has not run; (5) the `hits` attribute of the `<line>` element
> for `EfcSelectionGuard.cs` lines 49 and 75 and for `FolderSuggestionTree.cs` line 197, which are
> the three executable statements this change touches. The two named files are first-party
> non-`.Test` projects and are therefore in the `Get-KoverageProjectAllowlist` set, so their nodes
> are expected in both the raw and the post-processed document; if a node is genuinely absent,
> record `NOT APPLICABLE` and state which of the two filters removed it — the `coverage.config`
> module exclusions, or the Koverage project allowlist. `dotnet tool install --global
> dotnet-coverage` requires network access; if it fails, stop and report BLOCKED. The `coverage/`
> directory is git-ignored by `.gitignore:144`; the copy under `evidence/baseline/` is the durable
> artifact.

### P0-T18 — replace the first sentence

> Mark this task's own checkbox `[x]` in this plan file first, then run `git add
> docs/features/active/efcselectionguard-banner-prefix-arity-and-stale-comment-662` followed by a
> commit whose message names issue 662 and the phase, then run `git status --porcelain --
> docs/features/active/efcselectionguard-banner-prefix-arity-and-stale-comment-662`. The check-off
> precedes the `git add` because this plan file sits inside the pathspec: flipping the box after
> the commit would leave the tree dirty and falsify the status this task records.

### P1-T1 — replace the first sentence

> Write the constrained-implementation brief to
> `docs/features/active/efcselectionguard-banner-prefix-arity-and-stale-comment-662/evidence/other/small-path-handoff.md`,
> then perform the edits in P1-T2 through P1-T9 directly. This task is a brief, not a delegation:
> the executor has no agent-invocation tool, and P1-T2 through P1-T9 already specify every edit
> completely, so no delegation is required to complete them.

### P1-T4 — append to the acceptance

> The identifier must appear exactly once in the replacement doc text, because P2-T13 asserts a
> single matching line.

### P1-T5 — append

> The replacement occupies at most three comment lines. `EfcFormController.cs` is 1189 lines and
> already exceeds the 500-line limit in `.claude/rules/general-code-change.md`; this task must not
> widen that condition.

### P1-T6 — append

> The rewritten reader measures 132 characters on one line and CSharpier will wrap it in P2-T1
> into the same multi-line `StartsWith` shape used at `EfcFormController.cs:1143-1148`. That wrap
> keeps `BannerPrefix` on a single line, so this task's assertion and AC5's hold both before and
> after the format pass. A rewrite of this file by P2-T1 is expected and triggers the ordinary
> Phase 2 loop restart; it is not a failure.

### P1-T11 — replace the first sentence

> Mark this task's own checkbox `[x]` in this plan file first, then run `git add QuickFiler
> UtilitiesCS QuickFiler.Test
> docs/features/active/efcselectionguard-banner-prefix-arity-and-stale-comment-662` followed by a
> commit whose message names issue 662 and the phase, then run `git status --porcelain --
> QuickFiler UtilitiesCS QuickFiler.Test
> docs/features/active/efcselectionguard-banner-prefix-arity-and-stale-comment-662`.

### Phase 2 preamble — replace the first sentence

> Run steps P2-T1 through P2-T4 in the stated order, then P2-T5 through P2-T8. If any of those
> eight fails, or if the format step rewrites a file, fix the cause and restart the loop from
> P2-T1. The test steps are inside the loop, not after it: the repository toolchain order is
> format, lint, type-check, test, and a failure at the test stage restarts from the format stage.

### P2-T3, P2-T4

Prepend the `$msbuild` prelude to each command span.

### P2-T5, P2-T6

Prepend the `$vstest` prelude to each command span. Leave
`/Settings:scripts\vscode\TaskMaster.cli.runsettings` unchanged in these two: they pass no
`/EnableCodeCoverage`, so no collector is activated and no exclusion list is needed.

### P2-T7 and P2-T8

Prepend the `$vstest` prelude, replace `/Settings:scripts\vscode\TaskMaster.cli.runsettings` with
`/Settings:TaskMaster.runsettings`, and append the same runsettings rationale paragraph added to
P0-T11 and P0-T12.

### P2-T9 — replace from "Acceptance:" through "two decimal places"

> Acceptance: the artifact records the observed `EXIT_CODE:`, and `ExpectedExitCode:` when that
> value is non-zero. Its `Output Summary:` carries the same five field groups P0-T13 records, in
> the same order and read from the copied post-change Cobertura file: the `PostProcessed:`
> discriminator, the root `line-rate`, `lines-covered` and `lines-valid`, the derived line
> percentage to two decimal places, the per-filename `<class>` node count and `line-rate` for
> `EfcSelectionGuard.cs` and `FolderSuggestionTree.cs`, and the `<line>` `hits` values for the
> three changed executable statements.

### P2-T10 — replace the whole acceptance

> Acceptance: the artifact states, first, the `PostProcessed:` value recorded by P0-T13 and by
> P2-T9. If those two values differ, the gate cannot pass: record BLOCKED and stop, because the
> raw document's root attributes are computed over all instrumented modules while the
> post-processed document's are recomputed over the first-party package allowlist only, so the two
> figures measure different denominators and their difference is not a coverage delta. When the
> two values agree, the artifact states: the baseline line percentage from P0-T13, the post-change
> line percentage from P2-T9, and their signed difference in percentage points; the baseline and
> post-change `line-rate` for `EfcSelectionGuard.cs` and for `FolderSuggestionTree.cs`; and the
> changed-code coverage as a number — the three changed executable statements are
> `EfcSelectionGuard.cs:49`, `EfcSelectionGuard.cs:75` and `FolderSuggestionTree.cs:197`
> (post-format line numbers, resolved from the file as it stands after P2-T1), and the figure is
> the count of those three whose post-change `<line>` `hits` value is greater than zero, expressed
> as `covered/3` and as a percentage. The gate passes when the changed-code figure is `3/3`, and
> the post-change `line-rate` for each of the two named classes is not lower than its baseline
> value. The repository aggregate percentage and its signed difference are recorded but are not
> gated, because the aggregate is a full-suite figure whose denominator this plan has not measured
> across repeated runs and for which no allowance value has an evidential basis. If a class node is
> recorded as `NOT APPLICABLE` in the baseline, record it identically here and state that a 0/0
> denominator yields no comparable figure.

### P2-T12 — replace the sentence beginning "The declaration count falls from three to one"

> The declaration count falls from three to one by two independent decrements: the guard's
> constant is renamed to `BannerRejectionPrefix`, which no longer contains the substring the regex
> requires, and the `FolderSuggestionTree.cs` declaration is deleted by P1-T6.

### P2-T20 — replace the artifact list in the acceptance

> Acceptance: the six artifacts `csharpier-format.md`, `csharpier-check.md`,
> `msbuild-analyzers.md`, `msbuild-nullable.md`, `vstest-quickfiler-postchange.md` and
> `vstest-utilitiescs-postchange.md` under
> `docs/features/active/efcselectionguard-banner-prefix-arity-and-stale-comment-662/evidence/qa-gates/`
> all exist, all carry `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`, all record
> `EXIT_CODE: 0`, and all carry timestamps from the same final loop pass. The two test artifacts
> are required because AC9 names four toolchain steps ending in test and requires an evidence
> artifact for each. The format artifact's `Output Summary:` carries the transcribed CSharpier
> summary line and the before-and-after tree observation rather than the exit code alone. A summary
> of those six records is written to
> `docs/features/active/efcselectionguard-banner-prefix-arity-and-stale-comment-662/evidence/qa-gates/ac9-verification.md`,
> and the line `- [ ] AC9 —` in `issue.md` becomes `- [x] AC9 —` with the criterion text unchanged.

### P2-T23 — replace the whole task

> - [ ] [P2-T23] Confirm the change set matches the Scope Boundary, then commit every remaining
>   change. First run `git diff 2b85134b42872e405602e6064e02dc9cda6c319b --name-only -- '*.cs'
>   '*.csproj' '*.props' '*.targets' '*.xml' 'packages.config'` and `git status --porcelain --
>   '*.cs' '*.csproj' '*.props' '*.targets' '*.xml' 'packages.config'`, and confirm that the union
>   of the two listings is exactly the four in-scope files:
>   `QuickFiler/Controllers/EfcSelectionGuard.cs`, `QuickFiler/Controllers/EfcFormController.cs`,
>   `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs`,
>   `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs`. Any additional entry means the
>   repository-wide format pass in P2-T1 rewrote a file outside the three project directories that
>   task observes — CSharpier 1.2.6 also processes `*.xml` and `packages.config` — so stop and
>   report before committing. The diff is anchored to the base commit and the status span is its
>   companion, because the anchored diff cannot report an untracked path and the status goes empty
>   once the change is committed. Then mark this task's own checkbox `[x]` in this plan file, run
>   `git add QuickFiler UtilitiesCS QuickFiler.Test
>   docs/features/active/efcselectionguard-banner-prefix-arity-and-stale-comment-662` followed by a
>   commit whose message names issue 662, then run `git status --porcelain -- QuickFiler
>   UtilitiesCS QuickFiler.Test
>   docs/features/active/efcselectionguard-banner-prefix-arity-and-stale-comment-662`. Acceptance:
>   the scope comparison holds and the final status command returns no output. The check-off
>   precedes the `git add` because this plan file sits inside the pathspec. Files written under
>   `.claude/agent-memory/` are tracked but lie outside every pathspec in this plan and are
>   committed separately; record their presence in the artifact rather than staging them here.

---

## Residual condition named by the reviewer

Whether the working tree is currently CSharpier-clean determines whether AC5b and AC7 are
satisfiable at all, and the validation-only directive prohibited running `dotnet`. The B4 delta
converts that from an unmeasurable preflight question into an explicit BLOCKED stop at P0-T6 and
P0-T7, before any file is edited.
