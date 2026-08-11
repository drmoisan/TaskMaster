# 2026-08-10-excludefromcodecoverage-nested-lambdas-457 (Plan)

- **Issue:** #457
- **Parent:** epic `build-ci-coverage-gate-fidelity` (wave 1)
- **Owner:** drmoisan
- **Last Updated:** 2026-08-10T16-05
- **Status:** Revised against the fourth preflight delta (three corrections applied in place: `Pester Coverage Artifact:` field added to the four calling tasks' acceptance bullets, P0-T6 hunk-level attribution branches made exhaustive, Conventions `<kind>` assignment sentence de-slashed); ready for preflight re-validation
- **Version:** 1.4
- **Work Mode:** `full-bug` — `spec.md` is the sole acceptance-criteria source. `user-story.md` carries no acceptance criteria and must not be treated as an AC source.
- **Depends On:** issue #441 (epic wave 0). This plan is written against the post-#441 contract and halts at `[P0-T1]` if #441 is not present.

## Conventions

- `<FEATURE>` = `docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457`.
- All evidence artifacts resolve under `<FEATURE>/evidence/<kind>/` where `<kind>` is one of `baseline/`, `regression-testing/`, `qa-gates/`, `issue-updates/`, `other/`. Any `artifacts/`-rooted evidence path is invalid and must be rejected.
- `<timestamp>` = ISO-8601 `yyyy-MM-ddTHH-mm` captured at artifact-write time.
- Every command-step artifact carries `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
- All code locators in this plan are function/symbol anchors. Absolute line numbers are prohibited because #441 lands first and shifts every line in `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`. Where a line number appears in this plan it is a preflight observation about an existing file, recorded as descriptive context, never as a locator to edit against.
- PowerShell test evidence: `mcp__drm-copilot__run_poshqc_test` returns only `{ok, tool, workspace_root, summary}`. It carries no exit code, no passed/failed/skipped counts, no per-test names and no coverage figure. Every task requiring numeric or per-test PowerShell test evidence MUST run the MCP tool for the policy record AND pair it with a direct run that supplies the numbers:
  `pwsh -NoProfile -Command 'Import-Module Pester -MinimumVersion 5.0; $c = New-PesterConfiguration; $c.Run.Path = <test paths>; $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = <production paths>; $c.CodeCoverage.OutputPath = "<FEATURE>/evidence/<kind>/pester-coverage.<timestamp>.xml"; $r = Invoke-Pester -Configuration $c; "Passed=$($r.PassedCount) Failed=$($r.FailedCount) Skipped=$($r.SkippedCount) Coverage=$($r.CodeCoverage.CoveragePercent)"; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'`
  Record `MCP Result:` (`ok:true`/`ok:false` plus its summary) from the MCP run and `EXIT_CODE:` from the paired direct run. Pester 5.6.1 reports a command/line coverage percent only and emits no branch-coverage metric; where branch coverage is requested, record `branch coverage: not emitted by Pester 5` — that is a measured fact, not a placeholder. `Run.Exit` defaults to `$false` in Pester 5 and `Invoke-Pester` does not set a process exit code; do not set `Run.Exit = $true` (it calls `exit` before the count-emitting statements run). The explicit trailing `exit` above is what makes every `EXIT_CODE:` recorded from a direct Pester run load-bearing. Without it, `EXIT_CODE:` is 0 regardless of failures and every gate that reads it is vacuous.
  Quoting (measured, not stylistic): the outer wrapper uses single quotes and the inner script uses only double quotes. Do not use `\"`, and do not wrap the inner script in double quotes: both git-bash and a PowerShell host expand `$` inside a double-quoted outer argument, which empties every count and forces a spurious non-zero exit. The single-quoted form above was verified end-to-end in both shells, emitting populated counts with exit `0` on an all-passing run and exit `1` on a seeded failure; the double-quoted form emitted `Passed= Failed=` with exit `1` on an all-passing run, inverting every gate that reads `EXIT_CODE:`.
  Coverage output path (mandatory): `CodeCoverage.OutputPath` defaults to `coverage.xml` relative to the process working directory and Pester always writes it when coverage is enabled. Every direct Pester run MUST redirect it under `<FEATURE>/evidence/<kind>/` so it is a deliberate committed artifact; a repo-root `coverage.xml` is not gitignored (`.gitignore` covers `*.coverage`, `*.coveragexml` and `coverage/*`, none of which match it) and would fail the P2-T11 changed-file audit and violate the temporary-file prohibition below. Choose `<kind>` by calling task: `<kind>` is `baseline` for P0-T8, `regression-testing` for P1-T11, and `qa-gates` for P3-T1 and P3-T4. Each calling task records the resulting artifact path.

## Fail-Closed Evidence Rule

If any required baseline artifact, regression artifact, QA-gate artifact, or coverage-comparison artifact is missing or has an incomplete field set, the outcome is BLOCKED or INCOMPLETE, never PASS. A plan checkbox must remain unchecked when its artifact is absent or incomplete.

## Scope Prohibitions (binding on every task in this plan)

- Do NOT re-tune, lower, raise, or otherwise adjust any coverage threshold. Threshold reconciliation is owned by issue #494 (epic wave 2). A corrected figure that would fail an existing threshold is recorded in evidence and handed to #494.
- Do NOT modify `CLAUDE.md` or anything under `.claude/rules/`. Those edits are owned by sibling features #512 and #494.
- Do NOT modify any C# source file. The fix is PowerShell only.
- Do NOT modify `coverage.config`, `TaskMaster.runsettings`, or `scripts/vscode/TaskMaster.cli.runsettings`.
- Do NOT use `/p:Nullable=enable` as a gate. That documented command is a known defect (issue #522) producing roughly 200-414 spurious errors against a clean `main`. Where a C# build is needed to produce test assemblies, use a plain Debug build without `/p:Nullable=enable`.
- Do NOT create temporary files anywhere, in production code, in tests, or in evidence capture.
- Do NOT commit raw full-repository Cobertura dumps of tens of megabytes. Copy only what is needed; where the artifact exceeds 5 MB, record the numeric headline values plus the per-file extracts in a Markdown artifact instead.

### Phase 0 — Context, policy reads, and baseline capture

- [ ] [P0-T1] Verify that issue #441's corrections are present in `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` before any other Phase 0 work; write `<FEATURE>/evidence/baseline/dependency-441-verification.<timestamp>.md`.
  - This task runs first in the plan, ahead of the policy reads, precisely so that the only stop condition in the plan sits at the very first task.
  - Static check: a search for the descendant-axis literal `.//lines/line` across the module returns zero matches, and `Get-CoberturaCoverageSummary` selects over the child axis `./lines/line`.
  - Functional check: dot-source the module in a `pwsh -NoProfile` session and call `Get-CoberturaCoverageSummary` against an inline here-string fixture whose single `<class>` carries the same two line numbers both under `<methods>/<method>/<lines>` and under the class-level `<lines>`. Expected `LinesValid` is `2`; `4` proves the pre-#441 double count.
  - Blended-denominator check (functional, not a code reading): build an inline here-string fixture with two `<class>` elements sharing one `filename` — a primary class `Ns.T` carrying line 10 both under `<methods>/<method>/<lines>` and under its class-level `<lines>`, and a sibling `Ns.T.&lt;&gt;c` whose class-level `<lines>` carries line 11. Give the primary's line 10 `hits="1"` and the closure's line 11 `hits="0"`. Run the fixture through `Merge-CoberturaClassesByFilename`, then import the surviving merged `<class>` into a scratch `<coverage><packages><package><classes /></package></packages></coverage>` document and call `Get-CoberturaCoverageSummary` on it. Assert `LinesValid` is exactly `2` and `LinesCovered` is exactly `1`. `LinesValid` of `3` (with `LinesCovered` of `2`) proves the pre-#441 blend and is a BLOCKED outcome. Do not use the merged class's `line-rate` attribute as the denominator proof: a rate does not expose its denominator, and any fixture whose lines all share one `hits` value yields an identical rate under both arithmetics. This check is load-bearing for regression case 6: if the merge kept only the primary's method lines, the closure lines would already be gone after the merge and case 6 would pass regardless of the filter's call position, making the ordering gate vacuous.
  - HALT semantics: if any of the three checks fails, write `BLOCKED: dependency #441 corrections not present on this branch` into the artifact together with the failing check's observed value, stop plan execution, and report the blocked state to the epic-orchestrator. Do not proceed to P0-T2 or to any baseline capture. A baseline captured against the pre-#441 double count is worthless. This stop is a plan-directed stop reported to `epic-orchestrator`, not an agent-initiated block; the executor is following the plan text, not exercising a blocking discretion it does not have after preflight.
  - Acceptance: the artifact exists and records either all three checks passing or the explicit BLOCKED state with observed values.

- [ ] [P0-T2] Read the repository policy documents in the order defined by `policy-compliance-order` and write `<FEATURE>/evidence/baseline/phase0-instructions-read.<timestamp>.md`.
  - Read order: `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/powershell.md`, `.claude/rules/quality-tiers.md`, `.claude/rules/tonality.md`.
  - Acceptance: the artifact exists and contains `Timestamp:`, `Policy Order:` (the ordered list above), and an explicit `Files Read:` list naming each file with its repo-relative path.

- [ ] [P0-T3] Read the feature requirement documents and write `<FEATURE>/evidence/baseline/phase0-feature-documents-read.<timestamp>.md`.
  - Documents: `<FEATURE>/issue.md`, `<FEATURE>/spec.md`, `<FEATURE>/research/2026-08-10T14-10-excludefromcodecoverage-nested-lambdas-fix-surface.md`, `docs/features/epics/build-ci-coverage-gate-fidelity/epic.md`, `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`, `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`, `coverage.config`.
  - Acceptance: the artifact exists, contains `Timestamp:` and the explicit file list, and records `Work Mode: full-bug` and `AC Source: <FEATURE>/spec.md` (sole source).

- [ ] [P0-T4] Record the branch and commit baseline in `<FEATURE>/evidence/baseline/branch-commit-baseline.<timestamp>.md`.
  - Commands: `git rev-parse --abbrev-ref HEAD`, `git rev-parse HEAD`, `git status --porcelain`.
  - Acceptance: the artifact records the branch name, the full HEAD SHA, and the verbatim porcelain output. The recorded SHA is a record of state, never an expectation any later task asserts against.

- [ ] [P0-T5] Record the post-#441 size of `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` in `<FEATURE>/evidence/baseline/helpers-module-size.<timestamp>.md`.
  - Acceptance: the artifact records the current line count and states the remaining headroom against the 500-line ceiling in `.claude/rules/general-code-change.md` and `.claude/rules/powershell.md`. This is the measurement that research §8.7 requires before the new-file decision is acted on.

- [ ] [P0-T6] Capture the PoshQC format baseline over the PowerShell scan set and write `<FEATURE>/evidence/baseline/poshqc-format.<timestamp>.md`.
  - Command: `mcp__drm-copilot__run_poshqc_format` over the scan set `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`, `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`.
  - `scripts/vscode/Invoke-MSTestWithCoverage.ps1` is deliberately excluded from the baseline scan set. It is not modified by this feature, `run_poshqc_format` rewrites in place, and formatting churn in that file would be indistinguishable from a feature edit in the P2-T11 changed-file audit, whose acceptance permits no third production file. Its analyze baseline is not consumed by P3-T3 either, so nothing downstream depends on scanning it here.
  - Acceptance: the artifact carries `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; the summary records whether any file was rewritten.
  - Acceptance (attribution): `run_poshqc_format` rewrites files in place, so the artifact MUST list explicitly, by repo-relative path, every file this baseline run rewrote — and state `no file rewritten` when none was. Baseline formatting churn in `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` must be attributable to this baseline run rather than being mistaken for a third edit to that file in the P2-T11 changed-file audit.
  - Acceptance (hunk-level attribution): a file list cannot identify hunks, and the AC 13 measurement in P2-T11 and P3-T10 excludes hunks, not files. Therefore this artifact MUST carry a `baseline format diff:` field for `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` in every case. If that file was rewritten, record the verbatim `git diff -- scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` taken immediately after the format run (the tree is clean at this point, so the diff is exactly the baseline hunks). If that file was not rewritten — whether or not `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` was rewritten — record `baseline format diff: empty`. These two branches are exhaustive over this file, so P2-T11 and P3-T10 always have a recorded referent. Preflight measured `Invoke-Formatter` as a byte-for-byte no-op on both `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` and `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` under default settings, so the empty branch is the expected one; the recorded diff makes the AC 13 measurement mechanically exact in either branch.

- [ ] [P0-T7] Capture the PoshQC analyze baseline over the same scan set and write `<FEATURE>/evidence/baseline/poshqc-analyze.<timestamp>.md`.
  - Command: `mcp__drm-copilot__run_poshqc_analyze` over the P0-T6 scan set, which is exactly `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` and `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`. `scripts/vscode/Invoke-MSTestWithCoverage.ps1` is excluded here for the reason recorded in P0-T6.
  - Acceptance: the artifact carries the four required fields; the summary records the diagnostic count by severity, and records the full diagnostic list verbatim (rule name, severity, file, line) obtained by pairing the MCP run with a direct `pwsh -NoProfile -Command "Invoke-ScriptAnalyzer -Path <file>"` for each file in the scan set, because the MCP payload reports only a count. This verbatim list is the baseline set that P3-T3 compares against.

- [ ] [P0-T8] Capture the PoshQC Pester test baseline and write `<FEATURE>/evidence/baseline/poshqc-test.<timestamp>.md`.
  - Command: `mcp__drm-copilot__run_poshqc_test` with `scan_folders` supplied explicitly as `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` (the tool accepts file paths), paired with the direct Pester run defined in Conventions (bundled PoshQC settings; the tool exposes no settings parameter and `scripts/powershell/PoshQC/settings/pester.runsettings.psd1` does not exist in this repository — the path in `.claude/rules/powershell.md` names a bundled extension resource — and `config/poshqc-scan.json` also does not exist in this repository, which is why `scan_folders` is supplied explicitly rather than defaulted).
  - Acceptance: the artifact carries the four required fields; `Output Summary:` records numeric passed/failed/skipped counts and the numeric PowerShell line/command-coverage percent from the paired direct run, and `branch coverage: not emitted by Pester 5` (no placeholder values). Record `MCP Result:` verbatim. Record `Pester Coverage Artifact:` as the repo-relative path of the `pester-coverage.<timestamp>.xml` written by the paired direct run, which is under `<FEATURE>/evidence/baseline/` for this task.

- [ ] [P0-T9] Build the repository sufficiently to produce test assemblies and write `<FEATURE>/evidence/baseline/csharp-build.<timestamp>.md`.
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"`.
  - `/p:Nullable=enable` is deliberately absent per issue #522 and the scope prohibitions above. This step exists only to produce `*.Test.dll` assemblies for coverage collection; it is not a type-check gate.
  - Acceptance: the artifact carries the four required fields; `EXIT_CODE: 0`; `Output Summary:` records the warning and error counts and confirms `*.Test.dll` outputs exist under `bin\Debug\`.

- [ ] [P0-T10] Collect the repository coverage baseline through the canonical runner and write `<FEATURE>/evidence/baseline/coverage-collection.<timestamp>.md`.
  - Command: `pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\coverage.cobertura.xml`.
  - Record in `Output Summary:` the repository headline values read from the post-processed document element: `lines-covered`, `lines-valid`, `line-rate`, `branches-covered`, `branches-valid`, `branch-rate`.
  - Record in `Output Summary:` the per-file figures for `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` and `TaskVisualization/FlagTasks.cs`: the class `line-rate`, `branch-rate`, the count of `<line>` elements, and the count of `<line>` elements with `hits` greater than zero. If a file is absent from the report, record `absent` and the reason.
  - Record in `Output Summary:` the observed wall-clock duration of the post-processing step, per the performance note in `spec.md`.
  - Record in `Output Summary:` the number of discovered `*.Test.dll` assemblies and the executing repository root resolved at run time (`git rev-parse --show-toplevel`, equivalently `(Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path` from `scripts/vscode/`). Do not hard-code an absolute path: this plan is executed on the epic integration branch, in a different worktree from the one it was authored in. Confirm every discovered path begins with that resolved root; then strip the resolved-root prefix from each path and confirm the remainder contains no `.claude\worktrees\` segment. The second assertion is the one that actually excludes a nested agent worktree's stale assemblies; the first is tautological given the discovery root and is recorded as state, not as a gate. A `\.claude\` substring test over the full path is unsatisfiable when the executing worktree is itself under `.claude\worktrees\`.
  - Artifact copy rule: copy the post-processed `coverage\coverage.cobertura.xml` to `<FEATURE>/evidence/baseline/coverage-baseline.cobertura.xml` only when it is 5 MB or smaller. When it exceeds 5 MB, do not copy it; instead write `<FEATURE>/evidence/baseline/coverage-baseline-extract.<timestamp>.md` containing the document-element attributes verbatim and the two named per-file `<class>` extracts, and state in the command artifact that the full dump was omitted for size.
  - Acceptance: the command artifact exists with the four required fields and every numeric value above populated, and either the copied artifact or the extract artifact exists.

- [ ] [P0-T11] Resolve research §6.2's open question and write `<FEATURE>/evidence/baseline/async-d-state-machine-probe.<timestamp>.md`.
  - Question: does `dotnet-coverage` emit a `Type.<Member>d__<N>` state-machine class for a member that carries `[ExcludeFromCodeCoverage]` and is `async`?
  - Step 1: enumerate first-party C# members that carry `[ExcludeFromCodeCoverage]` and are declared `async`, recording each as a `(namespace-qualified declaring type, member name)` pair. Read-only; no C# file is modified.
  - Step 2: select a raw Cobertura corpus. A raw corpus has absolute `filename` attributes and retains closure classes as sibling `<class>` elements. The post-processed Phase 0 artifact is not a raw corpus, because `Merge-CoberturaClassesByFilename` collapses `d__` classes into the declaring type's class. Use `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/baseline/coverage-baseline.cobertura.xml`, which research §1 identifies as raw, and verify that identification before use.
  - Step 3: for at least one pair from step 1 whose declaring type appears in the corpus, search for `name="<DeclaringType>.&lt;<Member>&gt;d__"`. Record the exact search pattern and every match.
  - Step 4 (soundness guard on the negative branch): before recording `NO`, establish from `git log -1 --format=%cI -- <file>` for the selected member's file, and from the corpus's own recorded capture date, that the `[ExcludeFromCodeCoverage]` attribute on that member predates the corpus. If that cannot be established, the answer is `NOT-DETERMINABLE-FROM-CORPUS`, not `NO`.
  - Acceptance: the artifact records `Probe Answer:` as exactly one of `YES` (a `d__` class is emitted for an attributed async member), `NO` (no such class is emitted), or `NOT-DETERMINABLE-FROM-CORPUS`, together with `Corpus:`, `SearchPatterns:`, `SearchResult:`, and the `(declaring type, member)` pairs examined. This task must not be skipped; it determines whether the residual text in `spec.md` is accurate.

### Phase 1 — Regression tests, expected to fail

Every task in this phase is tagged `[expect-fail]`. Test files dot-source `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` in `BeforeAll`, mirroring the existing helper test, so the intended pre-implementation failure is `CommandNotFoundException` on the not-yet-existing filter functions. Fixtures are inline here-string Cobertura XML only: no temporary files, no on-disk fixtures, no committed `.cs` sources. Class names use the escaped forms (`&lt;&gt;c__DisplayClass41_0`) and method names the escaped `&lt;Member&gt;b__0`, exactly as emitted by the collector.

Every fixture uses fully-qualified Roslyn class names: a closure class is named `Ns.T.&lt;&gt;c__DisplayClass<N>_<M>` (or `Ns.T.&lt;&gt;c`) and its declaring class is named `Ns.T`. A bare `&lt;&gt;c__DisplayClass41_0` carries no `.<>c` marker and would be classified as a non-closure class, failing cases 1, 4, 5 and 8 against a correct implementation. Every `<class>` carries a class-level `<lines>` element in addition to its `<methods>/<method>/<lines>`, because post-#441 `Get-CoberturaCoverageSummary` counts the child axis `./lines/line` at class level only. Every call to `ConvertTo-KoverageCoberturaXml` passes `-PathSeparator '\'` explicitly, matching the existing tests in that file, so no fixture depends on the host `DirectorySeparatorChar`. This matches the established precedent in `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` (`UtilitiesCS.MeetingItemHelper.&lt;&gt;c`).

- [ ] [P1-T1] [expect-fail] Create `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1` with `Set-StrictMode -Version Latest`, a `BeforeAll` that dot-sources `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` resolved from `$PSScriptRoot`, and regression case 1.
  - Case 1 (exclude, required direction 1): a declaring class carrying `<method name="Visible">` plus a sibling `<>c__DisplayClass41_0` carrying `<method name="&lt;Exempt&gt;b__0">` on two lines.
  - Assertion: after `Remove-CoberturaExemptClosureCoverage`, neither closure line survives, and the recomputed `lines-valid` counts only `Visible`'s lines.
  - Evidence: `<FEATURE>/evidence/regression-testing/case-01-exclude.<timestamp>.md`. Path mirrors the production path `scripts/vscode/`; `tests/scripts/powershell/` is not used.

- [ ] [P1-T2] [expect-fail] Add regression case 2 (keep, required direction 2) to `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1`.
  - Fixture: the case-1 shape, but the closure method is `<method name="&lt;Visible&gt;b__0">`.
  - Assertion: the closure lines survive and remain in `lines-valid`.
  - Evidence: `<FEATURE>/evidence/regression-testing/case-02-keep.<timestamp>.md`.

- [ ] [P1-T3] [expect-fail] Add regression case 3 (keep, async guard) to `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1`.
  - Fixture: closure `<>c__DisplayClass33_1` carrying `&lt;Async&gt;b__0`, no plain `<method name="Async">` anywhere, plus a sibling class `Ns.T.&lt;Async&gt;d__33` whose only method is `MoveNext`.
  - Assertion: the closure lines survive. This is the load-bearing second direction: the declaring member is present only as a `Type.<Member>d__<N>` state-machine class. It fails if the presence set omits `d__` classes.
  - Evidence: `<FEATURE>/evidence/regression-testing/case-03-async-guard.<timestamp>.md`.

- [ ] [P1-T4] [expect-fail] Add regression case 4 (mixed closure) to `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1`.
  - Fixture: one `<>c` class carrying both `&lt;Exempt&gt;b__0_0` and `&lt;Visible&gt;b__1_0`, with `<method name="Visible">` present on the declaring class.
  - Assertion: only the exempt method's lines are dropped; the class survives; its `<lines>` equals the de-duplicated union of the retained methods' lines; `line-rate` is recomputed.
  - Evidence: `<FEATURE>/evidence/regression-testing/case-04-mixed-closure.<timestamp>.md`.

- [ ] [P1-T5] [expect-fail] Add regression case 5 (whole-class removal) to `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1`.
  - Fixture: a closure class carrying at least one `<method>` whose every method resolves to an absent member, with no declaring-type class for that filename.
  - Assertion: the `<class>` element is removed from its `<classes>` parent entirely and the filename no longer appears in the document.
  - Evidence: `<FEATURE>/evidence/regression-testing/case-05-whole-class-removal.<timestamp>.md`.

- [ ] [P1-T6] [expect-fail] Add regression case 7 (state machine untouched) to `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1`.
  - Fixture: class `Ns.T.&lt;Foo&gt;d__1` with `MoveNext` and no plain `<method name="Foo">`.
  - Assertion: the class is retained unchanged, including its `<lines>` set and its rate attributes. This pins the documented async residual so it cannot regress silently in either direction.
  - Evidence: `<FEATURE>/evidence/regression-testing/case-07-state-machine-untouched.<timestamp>.md`.

- [ ] [P1-T7] [expect-fail] Add regression case 8 (covered closure lines) to `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1`.
  - Fixture: an exempt-member closure whose lines carry `hits="1"` — the `<>c__DisplayClass42_0` / `DisposeProductionSurface` shape.
  - Assertion: the lines leave **both** `lines-covered` and `lines-valid`, and the recomputed document rate is consistent with the reduced numerator and denominator.
  - Evidence: `<FEATURE>/evidence/regression-testing/case-08-covered-closure-lines.<timestamp>.md`.

- [ ] [P1-T8] [expect-fail] Add regression case 9 (unit purity of name derivation) to `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1`.
  - Direct calls to `Get-CoberturaClosureDeclaringMemberName` with each of: `<M>b__0`, `<M>b__1_2`, `<M>g__L|3_0`, `Ns.T.<M>d__4`, `Ns.T.<>c__DisplayClass5_0.<<M>b__0>d`, `MoveNext`, `.ctor`.
  - Assertion: each input returns the expected declaring-member token or `$null`; `MoveNext` and `.ctor` return `$null` (fail-safe retention path). Also assert that, for each input, the function emits exactly one object on the success output stream (its return value, which may be `$null`) and zero records on the error, warning, verbose and information streams, and does not throw on an unrecognized shape.
  - Evidence: `<FEATURE>/evidence/regression-testing/case-09-unit-purity.<timestamp>.md`.

- [ ] [P1-T9] [expect-fail] Add regression case 10 (idempotence) to `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1`.
  - Fixture: any fixture the filter modifies (reuse the case-4 shape).
  - Assertion: running `Remove-CoberturaExemptClosureCoverage` twice over the same document produces `OuterXml` identical to the single-pass result, and, in the same test, assert that the two `Remove-CoberturaExemptClosureCoverage` invocations together emit zero objects on the success output stream and zero records on the error, warning, verbose and information streams (capture with `-ErrorVariable`/`-WarningVariable`/`3>&1 4>&1 6>&1` and assert the captured collections are empty).
  - Evidence: `<FEATURE>/evidence/regression-testing/case-10-idempotence.<timestamp>.md`.

- [ ] [P1-T10] [expect-fail] Extend `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` with regression case 6, the end-to-end pre-merge-ordering assertion.
  - Fixture: the case-1 shape, driven through `ConvertTo-KoverageCoberturaXml` with `-RepoRoot`, an explicit `-ProjectNames` list so the assertion does not depend on the production allowlist, and an explicit `-PathSeparator '\'`.
  - Assertion: the single merged `<class>` for that filename contains none of the exempt closure lines, and the document `lines-valid` counts only the declaring member's lines. This test is the ordering constraint's regression guard: it fails if the filter is placed after `Merge-CoberturaClassesByFilename`.
  - Evidence: `<FEATURE>/evidence/regression-testing/case-06-pre-merge-ordering.<timestamp>.md`.

- [ ] [P1-T11] [expect-fail] Run the two test files and record that all ten cases fail for the correct reason.
  - Command: `mcp__drm-copilot__run_poshqc_test` with `scan_folders` supplied explicitly as `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1` and `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` (the tool accepts file paths), paired with the direct Pester run defined in Conventions (bundled PoshQC settings; the tool exposes no settings parameter and `scripts/powershell/PoshQC/settings/pester.runsettings.psd1` does not exist in this repository — the path in `.claude/rules/powershell.md` names a bundled extension resource — and `config/poshqc-scan.json` also does not exist in this repository, which is why `scan_folders` is supplied explicitly rather than defaulted).
  - Acceptance: `<FEATURE>/evidence/regression-testing/expect-fail-run.<timestamp>.md` exists with `Timestamp:`, `Command:`, `MCP Result:`, `EXIT_CODE:` (non-zero, from the paired direct Pester run) and an `Output Summary:` naming each of the ten failing tests with its individual test name and observed failure reason. Expected reasons: cases 1, 2, 3, 4, 5, 7, 8, 9 and 10 fail with `CommandNotFoundException` on `Remove-CoberturaExemptClosureCoverage` or `Get-CoberturaClosureDeclaringMemberName`; case 6 fails with an assertion failure showing the exempt closure lines still present in the merged class, because `ConvertTo-KoverageCoberturaXml` already exists and merely does not yet call the filter. A Pester discovery error, a here-string syntax error, or a malformed-XML harness error does not satisfy this task. Record `Pester Coverage Artifact:` as the repo-relative path of the `pester-coverage.<timestamp>.xml` written by the paired direct run, which is under `<FEATURE>/evidence/regression-testing/` for this task.

- [ ] [P1-T12] Audit the two test files for fixture purity and size; write `<FEATURE>/evidence/regression-testing/fixture-purity-audit.<timestamp>.md`.
  - Acceptance: a search across both files returns zero matches for `New-TemporaryFile`, `[System.IO.Path]::GetTempPath`, `$env:TEMP`, `$env:TMP`, `TestDrive`, `Out-File`, `Set-Content`, and `Add-Content`; every fixture is an inline here-string; no `.cs` file is added under `tests/`; and both files are under 500 lines.

### Phase 2 — Implementation

Implements the Candidate 1c design from research §5.2 and `spec.md` § Proposed Fix. Production surface is exactly two PowerShell files, within the 2-production-file direct-mode budget in `.claude/rules/powershell.md`. All functions are advanced functions with approved verbs. Every function except `Remove-CoberturaExemptClosureCoverage` uses `[CmdletBinding()]`; `Remove-CoberturaExemptClosureCoverage` uses `[CmdletBinding(SupportsShouldProcess = $true)]` per `.claude/rules/powershell.md`.

- [ ] [P2-T1] Create `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` with `Set-StrictMode -Version Latest` and the function `Test-CoberturaClosureClassName`.
  - Contract: pure; returns `$true` when the class name contains the `.<>c` marker, covering `<>c`, `<>c__DisplayClass<N>_<M>`, generic suffixes, and nested `<>c….<<Member>b__K>d`; returns `$false` for `Type.<Member>d__<N>` state machines.
  - Acceptance: the file exists, the function is defined with `[CmdletBinding()]` and `[OutputType([bool])]`, and regression case 9's class-name inputs classify as specified.

- [ ] [P2-T2] Add `Get-CoberturaClosureDeclaringMemberName` to `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1`.
  - Contract: pure; given a synthesized class name or method name, returns the declaring member token or `$null`. Recognizes `^<(?<m>[^<>]+)>b__`, `^<(?<m>[^<>]+)>g__`, the last `<(?<m>[^<>]+)>d__\d+` segment of a class name, and the inner token of `<<(?<m>[^<>]+)>b__\d+>d`.
  - Acceptance: an unrecognized shape returns `$null` and does not throw; the function writes nothing to the output or error streams beyond its return value.

- [ ] [P2-T3] Add `Get-CoberturaDeclaringTypeName` to `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1`.
  - Contract: pure; returns the class name truncated at the first `.<`.
  - Acceptance: a class name with no `.<` returns the name unchanged.

- [ ] [P2-T4] Add `Get-CoberturaInstrumentedMemberName` to `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1`.
  - Contract: builds the presence set for one `<package>`, keyed by `"$declaringType|$filename"`.
  - The presence set admits members from exactly two sources: (1) a plain `<method name="X">` on a class whose name contains no `.<`, where `X` does not begin with `<`; (2) the `<Member>` token of a class named `Type.<Member>d__<N>`.
  - Acceptance: `<Member>g__Local|N_M` local-function methods are deliberately NOT admitted, so they cannot mask an otherwise-absent declaring member. Source (2) is mandatory, not optional: without it, regression case 3 fails and lambdas inside non-exempt async members are wrongly deleted.

- [ ] [P2-T5] Add the orchestrating function `Remove-CoberturaExemptClosureCoverage -XmlDocument [xml]` to `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1`, covering traversal and the drop decision.
  - Contract: `[CmdletBinding(SupportsShouldProcess = $true)]`, mandatory `[xml]$XmlDocument`, mutated in place, returns nothing — matching the existing mutation convention of `Merge-CoberturaClassesByFilename`. The mutation is guarded by a single `$PSCmdlet.ShouldProcess('Cobertura document', 'Remove exempt closure coverage')` call. `SupportsShouldProcess` is required, not stylistic: preflight confirmed that a `Remove-` verb declared with a bare `[CmdletBinding()]` raises `PSUseShouldProcessForStateChangingFunctions` (Warning), which fails the P3-T3 gate, and that the guarded form yields zero diagnostics.
  - For each `<package>`: build the presence set, then for each closure class derive each `<method>`'s declaring member, falling back to a class-name-derived token when the method name yields none (for example `MoveNext` on a nested async-lambda state machine). Drop methods whose declaring member is absent from the presence set for that `(declaringType, filename)` key.
  - Acceptance (fail-safe direction, non-negotiable): a method whose declaring member could not be derived is RETAINED. No code path may remove coverage for a member the filter failed to resolve. Over-exclusion is not an acceptable failure mode. A `<class>` whose name contains no `.<>c` marker is not mutated.

- [ ] [P2-T6] Add the retained-line rebuild and rate recomputation to `Remove-CoberturaExemptClosureCoverage`.
  - When no method was dropped, leave the class untouched. Otherwise rebuild `./lines` as the de-duplicated union of the retained methods' `./lines/line` (maximum `hits`, richest `condition-coverage`) and recompute `line-rate` and `branch-rate` by reusing `Get-CoberturaLineConditionCoverageParts` and `Get-CoberturaCoverageSummary` against a scratch document, exactly as `Merge-CoberturaClassesByFilename` already does.
  - Acceptance: regression case 4 passes and regression case 8 shows the dropped covered lines leaving both `lines-covered` and `lines-valid`.

- [ ] [P2-T7] Add whole-class removal to `Remove-CoberturaExemptClosureCoverage`.
  - Remove the `<class>` element from its `<classes>` parent only when at least one method was dropped AND zero methods are retained.
  - A closure class with no `<methods>` element, or with an empty `<methods>` element, is left untouched. "Zero methods present" is not "zero methods retained"; treating it as such deletes coverage the filter never resolved and violates the fail-safe invariant in P2-T5.
  - Acceptance: regression case 5 passes; a file whose every class is removed disappears from the report, which is the correct semantic for a wholly exempt file such as `TaskVisualization/FlagTasks.cs`; and the two pre-existing tests in `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` that carry a `<>c` closure class with an empty `<methods />` element ('merges duplicate class entries that point to the same source file' and 'normalizes stale TaskMaster roots before merging duplicate production class entries') still pass unchanged.

- [ ] [P2-T8] Add the dot-source line `. (Join-Path $PSScriptRoot 'Invoke-MSTestWithCoverage.ClosureFilter.ps1')` near the top of `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`.
  - Acceptance: this is edit 1 of exactly 2 permitted edits to that file. `$PSScriptRoot` resolves to the containing script's own directory even when the file is itself dot-sourced, which is how `Invoke-MSTestWithCoverageMain` loads it.

- [ ] [P2-T9] Add a single `Remove-CoberturaExemptClosureCoverage -XmlDocument $xml` call inside `ConvertTo-KoverageCoberturaXml` in `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`.
  - **Hard ordering acceptance criterion:** the call is placed AFTER the `//class[@filename]` path-normalization loop and BEFORE the `Merge-CoberturaClassesByFilename -XmlDocument $xml` call. The resulting order is: remove non-allowlisted `<package>` → normalize `//class[@filename]` → `Remove-CoberturaExemptClosureCoverage` → `Merge-CoberturaClassesByFilename` → inject `<sources>` → `Get-CoberturaCoverageSummary` and write the document-level rate attributes.
  - Rationale that makes this a constraint rather than a preference: a closure type always shares its declaring type's `filename`, so the merge always collapses it and the surviving node keeps only the primary's `<methods>`. Running the filter after the merge does not merely produce a worse result — it produces no result at all: the merged node is named for the declaring type, carries no `.<>c` marker, and no longer contains the `<Member>b__…` methods the filter resolves against, so every exempt closure line survives in the merged class-level `<lines>` and `lines-valid` is unchanged. Measured against the current helpers module using the case-1 shape: filter before the merge, the surviving class carries class-level lines `{10,11}` and `lines-valid = 2`; filter after the merge, `Merge-CoberturaClassesByFilename` produces a single class named `Ns.T` whose class-level `<lines>` is `{10,11,20,21}` and `lines-valid` stays at `4` — the filter is a no-op.
  - Acceptance: this is edit 2 of exactly 2 permitted edits to that file; the call site is verified by regression case 6 driving a fixture end-to-end through `ConvertTo-KoverageCoberturaXml`; no other call site of `Get-CoberturaCoverageSummary` is changed.

- [ ] [P2-T10] Verify both production PowerShell files are under 500 lines; write `<FEATURE>/evidence/other/production-file-size.<timestamp>.md`.
  - Files: `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` and `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`.
  - Acceptance: the artifact records both line counts and both are strictly below 500. This check runs again after the final formatting pass in Phase 3, as task P3-T10, because formatting can change line counts.

- [ ] [P2-T11] Audit the production surface; write `<FEATURE>/evidence/other/production-surface-audit.<timestamp>.md`.
  - Acceptance: the changed-file set for this feature contains exactly `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` (new) and `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` (two edits) on the production side, plus the two test files and this feature folder's documents and evidence. The artifact records that no `.cs` file, no `coverage.config`, no `*.runsettings`, no `CLAUDE.md`, and nothing under `.claude/rules/` is modified. Scope the audit's file listing to exclude `.claude/agent-memory/`, which is tracked and may be written independently of this feature.
  - The changed-file set is computed as `git status --porcelain -uall` from the repository root, with `.claude/agent-memory/` filtered out. A `<MERGE_BASE>..HEAD` diff must not be used: this plan contains no commit task, so that diff is empty and the check would be vacuous.
  - Record additionally the verbatim output of the restricted listing `git status --porcelain -uall -- scripts/vscode tests/scripts/vscode`. That restricted listing, not the whole-repository set, is the value P3-T10 re-measures and compares byte-for-byte; the whole-repository set necessarily grows between P2-T11 and P3-T10 because Phase 3 mandates new evidence artifacts under `<FEATURE>/evidence/`.
  - Any rewrite of `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` recorded by the P0-T6 baseline format run is attributed to that baseline, not counted as a third edit.
  - The `pester-coverage.<timestamp>.xml` files written under `<FEATURE>/evidence/baseline/` and `<FEATURE>/evidence/regression-testing/` by the direct Pester runs in P0-T8 and P1-T11 are expected feature-folder evidence and are accounted for as such, not as unaccounted paths. A `coverage.xml` at the repository root is NOT expected and fails this task: it means a direct Pester run omitted the mandatory `CodeCoverage.OutputPath` redirection defined in Conventions.
  - Run `git diff -- scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` and record the verbatim diff in the artifact together with the numeric added-line and removed-line counts. Acceptance: excluding any hunk present verbatim in the baseline format diff that P0-T6 recorded for this file (a file list cannot identify hunks; the recorded diff can, and it is `empty` in the expected branch), the diff consists of exactly two added lines — the `. (Join-Path $PSScriptRoot 'Invoke-MSTestWithCoverage.ClosureFilter.ps1')` dot-source and the `Remove-CoberturaExemptClosureCoverage -XmlDocument $xml` call — and zero removed lines. A third added line, or any removed line, fails this task. This is the only measurement in the plan that establishes spec AC 13's "exactly two edits"; `git status --porcelain -uall` reports only that the file is modified.
  - Also write `<FEATURE>/evidence/other/filter-purity-audit.<timestamp>.md`. Acceptance: a search of `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` returns zero matches for filesystem access (`Get-Content`, `Set-Content`, `Add-Content`, `Out-File`, `Import-Csv`, `Export-`, `Get-ChildItem`, `Test-Path`, `Resolve-Path`, `[System.IO.File]`, `[System.IO.Directory]`), process invocation (`Start-Process`, `Invoke-Expression`, `Invoke-Command`), clock or entropy reads (`Get-Date`, `[datetime]::Now`, `[datetime]::UtcNow`, `Get-Random`, `[guid]::NewGuid`), and network calls (`Invoke-WebRequest`, `Invoke-RestMethod`, `System.Net`). Record each pattern searched and its match count.

### Phase 3 — Verification and final QC loop

- [ ] [P3-T1] Re-run the regression suite and record that all ten cases now pass.
  - Command: `mcp__drm-copilot__run_poshqc_test` with `scan_folders` supplied explicitly as `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1` and `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` (the tool accepts file paths), paired with the direct Pester run defined in Conventions (bundled PoshQC settings; the tool exposes no settings parameter and `scripts/powershell/PoshQC/settings/pester.runsettings.psd1` does not exist in this repository — the path in `.claude/rules/powershell.md` names a bundled extension resource — and `config/poshqc-scan.json` also does not exist in this repository, which is why `scan_folders` is supplied explicitly rather than defaulted).
  - Acceptance: `<FEATURE>/evidence/regression-testing/pass-after-run.<timestamp>.md` exists with the four required fields, `MCP Result:`, `EXIT_CODE: 0`, and an `Output Summary:` naming each of the ten cases (1 through 10) with its individual test name and a `Passed` result. `EXIT_CODE: 0` from the paired direct run is the substantive gate here; no `ok:true` gate is imposed at this task, because AC 7's "completed `run_poshqc_test` step" is discharged by P3-T4's MCP-completion clause. Record `Pester Coverage Artifact:` as the repo-relative path of the `pester-coverage.<timestamp>.xml` written by the paired direct run, which is under `<FEATURE>/evidence/qa-gates/` for this task.
  - The run must also record that every pre-existing test in `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` still passes. Baseline for that file, measured at preflight: Passed=8, Failed=0, Skipped=0. Post-change the file must report Passed=9, Failed=0, Skipped=0 (the eight pre-existing tests plus case 6).

- [ ] [P3-T2] Run the final PoshQC format step and write `<FEATURE>/evidence/qa-gates/poshqc-format.<timestamp>.md`.
  - Command: `mcp__drm-copilot__run_poshqc_format` over `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1`, `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`, `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1`, `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`.
  - Acceptance: the artifact carries the four required fields; `Output Summary:` records whether any file was rewritten, listing each rewritten file by repo-relative path. If any file was rewritten, the loop restarts at this step after the rewrite is accepted.

- [ ] [P3-T3] Run the final PoshQC analyze step and write `<FEATURE>/evidence/qa-gates/poshqc-analyze.<timestamp>.md`.
  - Command: `mcp__drm-copilot__run_poshqc_analyze` over the P3-T2 file set.
  - Acceptance: the artifact records the full diagnostic list (rule name, severity, file, line), obtained by pairing the MCP run with a direct `pwsh -NoProfile -Command "Invoke-ScriptAnalyzer -Path <file>"` for each file in the set, because the MCP payload reports only a count. The gate passes when the diagnostic set contains no diagnostic on `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` and none on either test file, and no diagnostic on `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` other than those recorded verbatim in the P0-T7 baseline artifact. `EXIT_CODE: 1` is acceptable if and only if the diagnostic set is identical to the P0-T7 baseline set; any new diagnostic fails the gate and restarts the loop from P3-T2.
  - Preflight-recorded fact: `Invoke-MSTestWithCoverage.Helpers.ps1` carries a pre-existing `PSUseSingularNouns` Warning on `Get-CoberturaLineConditionCoverageParts`, and `run_poshqc_analyze` exits 1 on a Warning. Renaming that function is out of scope: it would exceed the two permitted edits fixed by P2-T8, P2-T9 and AC 13.

- [ ] [P3-T4] Run the final PoshQC Pester step in coverage mode and write `<FEATURE>/evidence/qa-gates/poshqc-test.<timestamp>.md`.
  - Command: `mcp__drm-copilot__run_poshqc_test` with `scan_folders` supplied explicitly as `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1` and `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` (the tool accepts file paths), paired with the direct Pester run defined in Conventions (bundled PoshQC settings; the tool exposes no settings parameter and `scripts/powershell/PoshQC/settings/pester.runsettings.psd1` does not exist in this repository — the path in `.claude/rules/powershell.md` names a bundled extension resource — and `config/poshqc-scan.json` also does not exist in this repository, which is why `scan_folders` is supplied explicitly rather than defaulted).
  - Acceptance: the artifact carries the four required fields; `EXIT_CODE: 0`; `Output Summary:` records numeric passed/failed/skipped counts, the numeric PowerShell line/command-coverage percent from the paired direct run, and `branch coverage: not emitted by Pester 5`, and the coverage figure for the new module `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1`. Type checking is not applicable to PowerShell and is skipped by policy, not by omission. Any failure or file change restarts the loop from P3-T2. Record `Pester Coverage Artifact:` as the repo-relative path of the `pester-coverage.<timestamp>.xml` written by the paired direct run, which is under `<FEATURE>/evidence/qa-gates/` for this task.
  - Acceptance (MCP completion): `MCP Result:` must be `ok:true`; an `ok:false` result is a gate failure that restarts the loop from P3-T2, because AC 7 requires a completed `run_poshqc_test` step, not merely a recorded one.
  - The artifact additionally records a pass/fail verdict for `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` against the line-coverage floor of 85% in `.claude/rules/powershell.md` and `.claude/rules/quality-tiers.md`. Recording the figure without a verdict does not satisfy this task. A figure below 85% is a blocking finding remedied by adding tests, never by adjusting a threshold.

- [ ] [P3-T5] Record the clean-pass discipline in `<FEATURE>/evidence/qa-gates/toolchain-loop.<timestamp>.md`.
  - Acceptance: the artifact records the number of loop iterations executed, and for the final iteration records that format → analyze → test completed in order with no file changed and no gate failure as defined by P3-T2, P3-T3, and P3-T4. It names the three per-step artifacts produced by P3-T2, P3-T3, and P3-T4 for that final iteration.

- [ ] [P3-T6] Rebuild the repository for post-change coverage collection and write `<FEATURE>/evidence/qa-gates/csharp-build.<timestamp>.md`.
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"`.
  - `/p:Nullable=enable` is deliberately absent per issue #522. No C# source is changed by this feature; this step exists solely to guarantee current `*.Test.dll` assemblies for the re-capture.
  - Acceptance: the artifact carries the four required fields with `EXIT_CODE: 0`.

- [ ] [P3-T7] Re-capture repository coverage post-change and write `<FEATURE>/evidence/qa-gates/coverage-collection.<timestamp>.md`.
  - Command: identical to P0-T10 — `pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\coverage.cobertura.xml`.
  - Record the same numeric set as P0-T10: repository `lines-covered`, `lines-valid`, `line-rate`, `branches-covered`, `branches-valid`, `branch-rate`; the per-file figures for `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` and `TaskVisualization/FlagTasks.cs` (class `line-rate`, `branch-rate`, `<line>` count, covered `<line>` count); and the observed post-processing wall-clock duration.
  - Apply the P0-T10 assembly-discovery rule by reference: record the number of discovered `*.Test.dll` assemblies and the executing repository root resolved at run time, do not hard-code an absolute path, confirm every discovered path begins with the resolved root (state, not gate), and confirm that each path with the resolved-root prefix stripped contains no `.claude\worktrees\` segment (the actual gate).
  - `TaskVisualization/FlagTasks.cs` is expected to disappear from the report entirely, because every member of the type is attributed. `absent (all classes removed)` is a valid measured outcome and must be recorded as such, not as a missing measurement.
  - Artifact copy rule: copy the post-processed `coverage\coverage.cobertura.xml` to `<FEATURE>/evidence/qa-gates/coverage-final.cobertura.xml` only when it is 5 MB or smaller; otherwise write `<FEATURE>/evidence/qa-gates/coverage-final-extract.<timestamp>.md` with the document-element attributes and the named per-file extracts, and state in the command artifact that the full dump was omitted for size.
  - Acceptance: the command artifact exists with the four required fields and every numeric value populated, and either the copied artifact or the extract artifact exists.

- [ ] [P3-T8] Produce the comparison artifact `<FEATURE>/evidence/qa-gates/coverage-delta.<timestamp>.md`.
  - Report, for each of `lines-covered`, `lines-valid`, `line-rate`, `branches-covered`, `branches-valid`, `branch-rate`: the P0-T10 baseline figure, the P3-T7 post-change figure, and the delta.
  - Report the same three columns for `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` and for `TaskVisualization/FlagTasks.cs`.
  - The artifact must state explicitly that every corrected per-file rate is **measured, not derived**: research confirmed that `<>c__DisplayClass42_0` contributes two **covered** lines from the exempt member `DisposeProductionSurface`, which a correct fix removes from both the numerator and the denominator, so the corrected rate is NOT `covered / (valid - 22)`. Any figure presented as arithmetic derived from the pre-fix figure is non-compliant with `spec.md`.
  - Acceptance: the artifact exists, cites the two source artifacts by path, and contains no derived percentage.

- [ ] [P3-T9] Check the measured figures against the existing repository coverage thresholds and write `<FEATURE>/evidence/qa-gates/threshold-assessment.<timestamp>.md`.
  - Compare the P3-T7 figures against the thresholds currently documented in `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` (line coverage >= 85%, branch coverage >= 75%) and in `CLAUDE.md` § UT2 (line coverage >= 80%). Record the conflict between the two documented figures as an observation; do not resolve it.
  - **This plan MUST NOT re-tune, lower, raise, or otherwise adjust any threshold.** Threshold reconciliation is owned by issue #494 and runs after this feature. Adjusting a threshold here is a scope violation regardless of whether a corrected figure fails one.
  - If any corrected figure would fail an existing threshold, record it in this artifact and write an explicit handoff note naming issue #494 and the exact figure, threshold, and source document.
  - Acceptance: the artifact exists, records each comparison with a pass/fail verdict, contains the `#494` handoff note when any comparison fails, and `git status --porcelain -uall -- CLAUDE.md .claude/rules` returns no output (recorded verbatim, including the empty result).

- [ ] [P3-T10] After the final clean toolchain iteration recorded by P3-T5, re-verify the production surface and file sizes; write `<FEATURE>/evidence/qa-gates/production-surface-final.<timestamp>.md`.
  - Re-run the P2-T10 line-count measurement for `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` and `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`, and the P2-T11 changed-file audit, against the post-format state.
  - Re-run the P2-T11 `git diff -- scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` measurement against the post-format state and record the verbatim diff with its added-line and removed-line counts. Acceptance: excluding any hunk present verbatim in the baseline format diff that P0-T6 recorded for this file, the diff is still exactly two added lines (the dot-source line and the `Remove-CoberturaExemptClosureCoverage -XmlDocument $xml` call) and zero removed lines. This is the post-format re-measurement of spec AC 13's "exactly two edits".
  - Acceptance: both files are strictly below 500 lines after formatting, and the changed-file set restricted to the production and test surface — `git status --porcelain -uall -- scripts/vscode tests/scripts/vscode` — is byte-identical to the same restricted listing recorded by P2-T11. Paths added under `<FEATURE>/evidence/` since P2-T11 are expected and are recorded but not compared. Additionally re-run the P3-T9 check `git status --porcelain -uall -- CLAUDE.md .claude/rules` and record its verbatim (empty) result.

### Phase 4 — Documentation, residuals, and acceptance criteria

Acceptance criteria are checked off in `<FEATURE>/spec.md` only. `spec.md` is the sole AC source under work mode `full-bug`; `user-story.md` carries no acceptance criteria. Each check-off task covers exactly one criterion, changes only `- [ ]` to `- [x]`, preserves the criterion text verbatim, and is performed only after the cited evidence exists.

- [ ] [P4-T1] Record the three documented residuals in `<FEATURE>/evidence/other/documented-residuals.<timestamp>.md`.
  - Residual (a): lambda bodies inside `[ExcludeFromCodeCoverage]` **async** members remain counted, because a `d__` state-machine class is admitted into the presence set and admitting it is mandatory for required direction 2.
  - Residual (b): local functions (`<Member>g__Local|N_M`) inside attributed members remain counted, because they are emitted inside the declaring type's own `<class>` and the filter scopes to closure classes only.
  - Residual (c): overload-name collisions cause under-exclusion, never over-exclusion, because the presence set is keyed by member name rather than signature.
  - Acceptance: the artifact names each residual, states why it is a deliberate scope choice, and states that none of them is to be absorbed into #457 or used to widen its scope.

- [ ] [P4-T2] Open a follow-up potential entry for residual (a) and record its path in `<FEATURE>/evidence/other/documented-residuals.<timestamp>.md`.
  - Use `mcp__drm-copilot__new_potential_bug_entry`. Title the entry for the exempt-async-member lambda residual and reference #457 and this feature folder.
  - Acceptance: the returned entry file exists under `docs/features/potential/` and its path is recorded in the residuals artifact together with the intended promotion path (`potential_to_issue`, to be run by the epic-orchestrator at epic close).
  - If `mcp__drm-copilot__new_potential_bug_entry` is unavailable to the executing agent, do not block: author the entry directly at `docs/features/potential/<yyyy-MM-dd>-<slug>.md`, following the shape of existing entries in that folder, and record in the residuals artifact that the entry was authored directly, together with the reason the MCP tool was unavailable.

- [ ] [P4-T3] Open a follow-up potential entry for residual (b) and record its path in `<FEATURE>/evidence/other/documented-residuals.<timestamp>.md`.
  - Acceptance: the returned entry file exists under `docs/features/potential/`, describes the local-function (`g__`) residual and the symmetric-extension option evaluated and deferred, and its path is recorded in the residuals artifact.
  - If `mcp__drm-copilot__new_potential_bug_entry` is unavailable to the executing agent, do not block: author the entry directly at `docs/features/potential/<yyyy-MM-dd>-<slug>.md`, following the shape of existing entries in that folder, and record in the residuals artifact that the entry was authored directly, together with the reason the MCP tool was unavailable.

- [ ] [P4-T4] Open a follow-up potential entry for residual (c) and record its path in `<FEATURE>/evidence/other/documented-residuals.<timestamp>.md`.
  - Acceptance: the returned entry file exists under `docs/features/potential/`, describes the overload-name-collision under-exclusion and states that signature-based keying is deliberately not attempted in #457, and its path is recorded in the residuals artifact.
  - If `mcp__drm-copilot__new_potential_bug_entry` is unavailable to the executing agent, do not block: author the entry directly at `docs/features/potential/<yyyy-MM-dd>-<slug>.md`, following the shape of existing entries in that folder, and record in the residuals artifact that the entry was authored directly, together with the reason the MCP tool was unavailable.

- [ ] [P4-T5] Reconcile the P0-T11 probe result against the residual text in `<FEATURE>/spec.md` § Risks & Mitigations, residual 1.
  - If `Probe Answer: YES`, the residual text stands as written; record the confirmation in `<FEATURE>/evidence/other/probe-reconciliation.<timestamp>.md`.
  - If `Probe Answer: NO`, correct the residual text in `spec.md` to state that the collector emits no `d__` class for an attributed async member and that those lambdas are therefore already excluded, narrowing the residual; record the before and after text in the reconciliation artifact. This branch is available only when P0-T11 step 4's soundness guard was satisfied.
  - If `Probe Answer: NOT-DETERMINABLE-FROM-CORPUS`, leave the residual text unchanged, mark it `unverified` in the reconciliation artifact, and record the full search scope, patterns, and result.
  - Acceptance: the reconciliation artifact exists and states which of the three branches was taken and what changed in `spec.md`, if anything.

- [ ] [P4-T6] Check off spec AC 1 (a lambda inside a member carrying `[ExcludeFromCodeCoverage]` does not appear in the coverage denominator) in `<FEATURE>/spec.md`, citing the case-1 and case-6 evidence artifacts and the P3-T7 measured figures.

- [ ] [P4-T7] Check off spec AC 2 (a lambda inside a member that does not carry `[ExcludeFromCodeCoverage]` still appears in the denominator) in `<FEATURE>/spec.md`, citing the case-2 and case-3 evidence artifacts.

- [ ] [P4-T8] Check off spec AC 3 (the selected fix surface is recorded in `spec.md` with an explicit justification against every candidate alternative) in `<FEATURE>/spec.md`, citing the § Proposed Fix candidate table.

- [ ] [P4-T9] Check off spec AC 4 (deterministic Pester regression tests with no temporary files, no on-disk fixtures, and no committed `.cs` sources) in `<FEATURE>/spec.md`, citing the P1-T12 fixture-purity audit artifact.

- [ ] [P4-T10] Check off spec AC 5 (a repository coverage baseline re-captured against the post-#441 arithmetic and recorded numerically under `evidence/baseline/` and `evidence/qa-gates/`) in `<FEATURE>/spec.md`, citing the P0-T1 dependency verification, the P0-T10 baseline artifact, and the P3-T7 re-capture artifact.

- [ ] [P4-T11] Check off spec AC 6 (no coverage threshold is changed; any corrected figure that would fail an existing threshold is recorded and handed to issue #494) in `<FEATURE>/spec.md`, citing the P3-T9 threshold-assessment artifact.

- [ ] [P4-T12] Check off spec AC 7 (full PowerShell toolchain pass in order with recorded exit codes in `evidence/qa-gates/`) in `<FEATURE>/spec.md`, citing the P3-T2, P3-T3, P3-T4, and P3-T5 artifacts.

- [ ] [P4-T13] Check off spec AC 8 (the filter is invoked after path normalization and before `Merge-CoberturaClassesByFilename`, proven end-to-end by regression case 6) in `<FEATURE>/spec.md`, citing the P2-T9 call site and the case-6 evidence artifact.

- [ ] [P4-T14] Check off spec AC 9 (the presence set admits `Type.<Member>d__<N>` names, and a covered lambda inside a non-exempt async member is retained) in `<FEATURE>/spec.md`, citing the P2-T4 contract and the case-3 evidence artifact.

- [ ] [P4-T15] Check off spec AC 10 (all ten regression cases implemented as individually named passing Pester tests across the two test files) in `<FEATURE>/spec.md`, citing the P3-T1 pass-after-run artifact.

- [ ] [P4-T16] Check off spec AC 11 (the filter is a pure XML-to-XML transform that reads no file, invokes no process, reads no clock, makes no network call, and is idempotent) in `<FEATURE>/spec.md`, citing the case-9 and case-10 evidence artifacts and the `[P2-T11]` filter-purity audit artifact.

- [ ] [P4-T17] Check off spec AC 12 (an unrecognized compiler-generated name shape causes retention, never removal) in `<FEATURE>/spec.md`, citing the P2-T5 fail-safe acceptance criterion and the case-9 evidence artifact.

- [ ] [P4-T18] Check off spec AC 13 (production changes limited to the new file and exactly two edits in the helpers module; both files under 500 lines; no C#, config, runsettings, `CLAUDE.md`, or `.claude/rules/` change) in `<FEATURE>/spec.md`, citing the P2-T10, P2-T11 and P3-T10 artifacts. The "exactly two edits" clause is checked off against the `git diff -- scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` added-line/removed-line measurement recorded by P2-T11 and re-measured by P3-T10, not against the `git status --porcelain` changed-file listing, which cannot count edits.

- [ ] [P4-T19] Check off spec AC 14 (the corrected per-file figure for `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` is measured from an actual post-fix, post-#441 run, with the `<>c__DisplayClass42_0` numerator note recorded) in `<FEATURE>/spec.md`, citing the P3-T8 coverage-delta artifact.

- [ ] [P4-T20] Check off spec AC 15 (the three documented residuals are recorded and handed off as follow-up references rather than absorbed or widened) in `<FEATURE>/spec.md`, citing the P4-T1 residuals artifact and the three potential-entry paths from P4-T2, P4-T3, and P4-T4.

- [ ] [P4-T21] Check off spec AC 16 (the async-`d__` probe is executed, its observed result recorded under `evidence/baseline/`, and the residual description corrected if the probe contradicts it) in `<FEATURE>/spec.md`, citing the P0-T11 probe artifact and the P4-T5 reconciliation artifact.

- [ ] [P4-T22] Emit the acceptance-criteria status summary required by `acceptance-criteria-tracking` into `<FEATURE>/evidence/other/ac-status-summary.<timestamp>.md` and into the executor's final completion report.
  - Required block shape: `### Acceptance Criteria Status` with `- Source:`, `- Total AC items:`, `- Checked off (delivered):`, `- Remaining (unchecked):`, `- Items remaining:`.
  - Acceptance: `Source:` names `<FEATURE>/spec.md` only; `Total AC items:` equals the count of checkbox items under `## Acceptance Criteria` in `spec.md`; any unchecked item is listed verbatim with the reason it could not be verified. If any item remains unchecked, the plan outcome is INCOMPLETE, not PASS.
