# Policy Audit — issue #633, unsynchronized undo handoff after batch move

- Timestamp: 2026-09-01T11-32
- Feature folder: `docs/features/active/2026-08-26-qfc-unsynchronized-undo-handoff-after-batch-move-633/`
- Branch: `bug/qfc-unsynchronized-undo-handoff-after-batch-move-633`
- Head: `efd939cf408cf6edd7b025da721a37ca47953368`
- Base: `origin/main` = `06b1e02e5d545b4dfae398cdbf9ae10a3f98ac72`
- Merge base: `git merge-base origin/main HEAD` = `06b1e02e5d545b4dfae398cdbf9ae10a3f98ac72` (identical to
  `origin/main`, so the two-dot and three-dot diffs coincide; the three-dot form was used throughout)
- Work mode: `full-bug` (marker in `issue.md:15`). `spec.md` is the sole acceptance-criteria source.
  `user-story.md` does not exist and its absence is correct for this work mode; it is not reported as a gap.
- Working tree at review time: clean (`git status --porcelain` empty)

## Overall verdict

**PASS.** Zero blocking findings. Seven non-blocking findings and one dispositioned coverage row are
recorded in the Findings section and in `code-review.2026-09-01T11-32.md`.

## Audit scope

The audit scope is the full branch diff against the resolved base branch: 74 changed files
(6 code/project files, 68 under `docs/`), +95,852 / −37 lines. Two production files, three test files,
one project file, and the feature folder including its evidence tree.

### Scope narrowing assessment

None detected. The delegating prompt supplied five constraints. Each was assessed against the scope
invariant and none narrows the audit:

| Caller constraint | Assessment |
|---|---|
| "Do not edit `.git/info/exclude`." | Write restriction on a shared file. No audit scope effect. Honoured. |
| "Do not write helper scripts anywhere under `evidence/`." | Write-location restriction. The one helper written for this review lives in the system scratchpad outside the repository. No audit scope effect. |
| "Do not generate or commit `artifacts/csharp/coverage.xml` or any other file under `artifacts/`." | Write restriction, not a verification waiver. Coverage was still fully verified, from the primary Cobertura XML on disk, parsed independently by this reviewer. See the Coverage Verification section. |
| "The branch carries a strict footprint acceptance criterion (AC16)... Do not create a file anywhere else." | Write-location restriction. The three review artifacts are written inside the feature folder, which is under `docs/` and therefore inside the AC16 footprint. |
| "Known non-blocking item, already analyzed — confirm or refute, do not re-litigate." | Assessed independently rather than accepted. The finding was reproduced from primary sources; see NB-4. |

No caller instruction attempted to limit the review to a plan, task, or phase; to a subset of changed
files; or to mark any language "out of plan scope", "informational only", or "not applicable".

## Evidence Location Compliance

`validate_evidence_locations.py` does not exist in this repository, so the scan was performed directly
against the branch diff.

| Check | Result |
|---|---|
| Files in the diff under `artifacts/baselines/` | 0 |
| Files in the diff under `artifacts/qa/` | 0 |
| Files in the diff under `artifacts/coverage/` | 0 |
| Files in the diff under `artifacts/evidence/` | 0 |
| Files in the diff under `artifacts/` at all | 0 |
| Evidence written to the canonical `<FEATURE>/evidence/<kind>/` layout | Yes — `baseline/`, `qa-gates/`, `regression-testing/`, `issue-updates/`, `other/` |

Verdict: **PASS**. No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` event occurred during this review; this
reviewer wrote no evidence artifact of its own.

## Policy compliance table

Policies applied in the mandated order: `CLAUDE.md`, `.claude/rules/general-code-change.md`,
`.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md`, `.claude/rules/tonality.md`,
and the C# sections of `CLAUDE.md` (C#1–C#7, CUT1–CUT3).

| # | Policy requirement | Verdict | Evidence |
|---|---|---|---|
| P1 | CSharpier formatting clean (`dotnet tool run csharpier check .`) | **PASS** | Re-run by this reviewer in-session: exit 0, `Checked 1566 files in 4637ms.`, 0 unformatted. Corroborates `evidence/qa-gates/p7-pass2-gates.2026-09-01T11-10.md`. |
| P2 | Analyzer gate: `msbuild /t:Rebuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` exits 0 | **PASS** | `evidence/qa-gates/p7-t4-analyze.msbuild.txt`: `5 Warning(s)`, `0 Error(s)`. |
| P3 | Analyzer gate is not vacuous (`/t:Rebuild`, zero `Skipping target "CoreCompile"`) | **PASS** | Reviewer counted `Skipping target "CoreCompile"` in the analyze log: **0**. `/t:Rebuild` present in the recorded command. |
| P4 | Nullable / type-check gate: `msbuild /t:Rebuild ... /p:TreatWarningsAsErrors=true` exits 0 | **PASS** | `evidence/qa-gates/p7-t5-nullable.msbuild.txt`: `5 Warning(s)`, `0 Error(s)`. |
| P5 | Nullable gate is not vacuous | **PASS** | Reviewer counted `Skipping target "CoreCompile"` in the nullable log: **0**. |
| P6 | Prohibited MSBuild variants not used (`/p:Nullable=enable` absent, `/t:Build` not substituted) | **PASS** | Both recorded commands use `/t:Rebuild` and omit `/p:Nullable=enable`, matching `CLAUDE.md` §C#1.2/§C#1.3 and `.github/workflows/ci.yml`. |
| P7 | Warning count did not regress | **PASS** | 5 warnings at baseline (`evidence/baseline/p0-t8-analyze.2026-09-01T10-31.md`) and 5 after. All five are the pre-existing System.Reactive `packages.config` warnings. |
| P8 | Full test suite green | **PASS** | `evidence/qa-gates/p7-t6-test-coverage.2026-09-01T11-10.md`: `Test Run Successful. Total tests: 6924, Passed: 6924`, exit 0. Baseline was 6912/6912; delta +12 matches the 12 added tests. |
| P9 | Toolchain ran in the mandated order in a single uninterrupted pass | **PASS** | `evidence/qa-gates/p7-pass2-gates.2026-09-01T11-10.md` plus the 11-10 test artifact. Format → analyze → type-check → test, no source file edited between stages. See NB-3 for an evidence-granularity observation on this pass. |
| P10 | Bugfix Workflow: failing regression test written first | **PASS** | `evidence/regression-testing/fail-before-run.2026-09-01T10-46.md` with `p2-t5.trx`: reviewer counted 3 `outcome="Failed"` (2 `UnitTestResult` + 1 `ResultSummary`), 0 passed. `p4-t6.trx` after the fix: 2 passed, `ResultSummary outcome="Completed"`. Genuine RED→GREEN on the same two tests. |
| P11 | Bugfix Workflow: documented exception where a failing run is structurally impossible | **PASS** | `evidence/regression-testing/fail-before-exception.2026-09-01T10-48.md`. The seven queue-level tests name `WhenDrainedAsync()`, which does not exist pre-fix, so the pre-fix state is compile-red rather than test-red. Reviewer confirmed the API is absent from `origin/main`'s `FilerQueue.cs`. Compile-red is an accepted RED-first equivalent in this repository. |
| P12 | Minimal targeted fix; no opportunistic refactor | **PASS** | Diff confined to the queue's internals plus 12 changed lines in `QfcFormController.EventHandlers.cs`. The handshake repair is argued as a barrier precondition in `spec.md:154-170` and the reviewer independently agrees: a drain over the pre-existing one-shot guard could report drained while an item is stranded. |
| P13 | File size limit — no production or test file over 500 lines | **PASS** | `FilerQueue.cs` 197; `QfcFormController.EventHandlers.cs` 408; `FilerQueueTests.cs` 358; `QfcFormControllerUndoHandoffTests.cs` 428; `QfcItemController.SeamFactoryTests.cs` 470. Counted with `awk 'END{print NR}'`. |
| P14 | Test framework: MSTest (CUT1) | **PASS** | `[TestClass]`/`[TestMethod]`/`[TestInitialize]`/`[TestCleanup]` from `Microsoft.VisualStudio.TestTools.UnitTesting` throughout. No xUnit or NUnit introduced. |
| P15 | Mocking: Moq; assertions: FluentAssertions (CUT2) | **PASS** | `QfcFormControllerUndoHandoffTests.cs` uses `Mock<T>` for eight collaborators and `Should()` assertions exclusively. `FilerQueueTests.cs` uses FluentAssertions throughout. |
| P16 | Determinism: no banned wait API in test code | **PASS** | Reviewer grep for `Thread\.Sleep\|Task\.Delay\|\.Wait(\|\.Result\b\|DateTime\.(Now\|UtcNow)` across the three touched test files returned zero matches (exit 1). All concurrency is driven by `TaskCompletionSource` gates through the `ItemProcessor` seam. |
| P17 | No temporary files in tests | **PASS** | No `Path.GetTempPath`, `Path.GetTempFileName`, `File.Create`, or `Directory.Create` in the three touched test files. The new tests are entirely in-memory. |
| P18 | Test file location mirrors production structure | **PASS** | `QuickFiler.Test/Controllers/*` mirrors `QuickFiler/Controllers/*`. No test file was placed in the production tree. |
| P19 | Coverage exclusion policy — no production path excluded from measurement | **PASS** | No `[ExcludeFromCodeCoverage]` attribute was added; no `coverage.config` assembly exclude was added. Both changed production files appear in the post-change Cobertura file with non-zero line counts. |
| P20 | net481 language constraints (no `init`, `record`, `record struct`) | **PASS** | Reviewer grep for `\binit\s*[;{]\|\brecord\b` over both changed production files returned zero matches (exit 1). Both builds compile with `CS0518` count 0. |
| P21 | Public API compatibility — change is additive | **PASS** | `Consumer` retains its type, accessibility and `Task.CompletedTask` default (`FilerQueue.cs:76`), pinned by `FilerQueue_NewInstance_HasCompletedConsumerByDefault`, whose body is unmodified in the diff. Both `Enqueue` overloads retain their signatures. `WhenDrainedAsync()` is added; `ItemProcessor` is `internal`. |
| P22 | Error handling — worker failure is contained and logged | **PARTIAL (non-blocking)** | The per-item `catch` and its `logger.Error` diagnostic are preserved verbatim and the decrement is in a `finally`, so the documented failure mode is handled. However, an exception raised *by the catch handler itself* still escapes the worker loop and leaves the running flag set. See NB-1. |
| P23 | Tonality — professional, non-hyperbolic, evidence-matched | **PASS** | Spec, plan, and evidence artifacts are factual and measured. One overstated claim was found in an evidence artifact, recorded as NB-2; it is an accuracy defect, not a tone defect. |
| P24 | Policy documents unmodified | **PASS** | Diff contains no path under `.claude/rules/`, `.claude/skills/`, `.github/instructions/`, or `CLAUDE.md`. |
| P25 | Evidence artifact hygiene — no absolute host path or account/machine token in a committed artifact | **PARTIAL (non-blocking)** | 16 committed evidence files and `plan.2026-08-31T19-35.md` still contain the developer account token, and the TRX files contain `<account>@<HOST>`. See EQ-1. |

## Coverage Verification

### Languages with changed files in the branch diff

Derived from `git diff --name-status origin/main...HEAD`, not from a caller-supplied list.

| Language | Changed files on the branch | Coverage obligation |
|---|---|---|
| C# (`.cs`) | 5 (`FilerQueue.cs`, `QfcFormController.EventHandlers.cs`, `FilerQueueTests.cs`, `QfcFormControllerUndoHandoffTests.cs`, `QfcItemController.SeamFactoryTests.cs`) | Mandatory |
| PowerShell (`.ps1`/`.psm1`) | 0 | None |
| Python (`.py`) | 0 | None |
| TypeScript (`.ts`/`.tsx`) | 0 | None |

Only one language has changed files on this branch, so only one language carries a coverage obligation.

### PR context artifacts

`artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` are absent from this worktree.
They were not regenerated: the delegating constraint prohibits creating any file under `artifacts/`, and
regenerating them would additionally add a path the AC16 footprint criterion forbids. Scope was therefore
derived from the authoritative alternative source named in the scope invariant — the resolved base branch
from `pr-base-branch-merge-base`, recomputed in-session as `06b1e02e` — and enumerated with
`git diff --name-status origin/main...HEAD`. This substitution changes nothing about the audit scope: the
git diff is the same population the summary artifact would have described, and it is not subject to the
recurring `.cs`-misclassification defect that the generated summary exhibits.

### C# coverage artifact located

The canonical path `artifacts/csharp/coverage.xml` does not exist. The primary C# coverage evidence for
this branch is the pair of Cobertura files produced by the repository's own wrapper
`scripts/vscode/Invoke-MSTestWithCoverage.ps1`:

- `coverage/baseline.cobertura.xml` (10,781,801 bytes, written 2026-09-01T10:34)
- `coverage/post-change.cobertura.xml` (10,789,123 bytes, written 2026-09-01T11:10)

Both were parsed directly by this reviewer with an independent XML reader rather than read off the
executor's markdown. Both classify as FILTERED (first-party) denominators: each contains exactly the same
nine packages — QuickFiler, SVGControl, Tags, TaskMaster, TaskTree, TaskVisualization, ToDoModel,
UtilitiesCS, VBFunctions — with no vendored third-party assembly present. The comparison is therefore
between two figures over the same denominator, and the raw-unfiltered-versus-filtered confound described
in `spec.md` and in the delegating prompt does not apply.

Note the artifacts live under `coverage/`, which is gitignored, so they are not committed. See EQ-2.

### Repository-wide C# coverage

| Measure | Baseline | Post-change | Delta | Floor | Row verdict |
|---|---|---|---|---|---|
| C# repo-wide line coverage (Cobertura `line-rate`) | 85.3172 % | **85.3910 %** | +0.0738 pt | >= 85 % | **PASS** |
| C# repo-wide branch coverage (Cobertura `branch-rate`) | 79.3172 % | **79.4014 %** | +0.0842 pt | >= 75 % | **PASS** |
| C# lines covered / valid | 54882 / 64327 | 54973 / 64378 | +91 / +51 | — | PASS |
| C# branches covered / valid | 13081 / 16492 | 13106 / 16506 | +25 / +14 | — | PASS |

C# repo-wide line coverage of 85.39 % clears the 85 % uniform floor in `.claude/rules/quality-tiers.md`
and the 80 % floor in `CLAUDE.md` UT2. C# repo-wide branch coverage of 79.40 % clears the 75 % branch
floor. No regression: both rates rose.

### Per-file C# coverage for the changed production files

Computed by the reviewer as distinct covered line numbers over distinct instrumented line numbers,
unioned across every `class` element whose `filename` names the file. Class-level `line-rate` attributes
were not averaged.

| File | Tier | Baseline | Post-change | Uncovered lines after | Row verdict |
|---|---|---|---|---|---|
| `QuickFiler/Controllers/FilerQueue.cs` | modified | 18/49 = 36.73 % | **96/96 = 100.00 %** | 0 | C# per-file line coverage **PASS** (>= 85 % modified-file floor, and >= 90 % for the members AC20 names) |
| `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` | modified | 113/249 = 45.38 % | **125/253 = 49.41 %** | 128 | C# per-file line coverage **FAIL** against the 85 % modified-file floor — dispositioned non-blocking, see CV-1 |

No file on this branch is new production code, so the new-code tier does not apply to any production
file. The only added file is a test file, which is excluded from the coverage denominator by policy.

### Changed-line C# coverage

| File | Changed lines | Instrumented | Uncovered |
|---|---|---|---|
| `QuickFiler/Controllers/FilerQueue.cs` | 126 | 58 | **0** |
| `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` | 12 | 7 | **0** |
| **Total** | **138** | 65 | **0** |

Zero uncovered changed lines. The changed-line gate is not vacuous (138 > 0). This reviewer corroborated
the result from the opposite direction: the post-change uncovered-line set for `FilerQueue.cs` is empty,
so no changed line in that file can be uncovered; and the uncovered-line set for
`QfcFormController.EventHandlers.cs` is a strict subset of its baseline uncovered set (128 lines versus
136, with the first 25 identical), so no previously covered line became uncovered.

### C# coverage summary row

C# coverage verification is complete and the C# coverage verdict is **PASS**: repo-wide line coverage
85.39 % over the filtered first-party denominator, repo-wide branch coverage 79.40 %, zero uncovered
changed lines, and no regression on any measure.

The one C# coverage row recorded as FAIL is the per-file line coverage of
`QuickFiler/Controllers/QfcFormController.EventHandlers.cs` at 49.41 %, which is dispositioned
non-blocking in CV-1 for reasons stated there.

### Other languages

Each row below is stated explicitly rather than omitted, so that the verdict is legible even though these
languages have zero changed files on this branch.

| Language | Changed files on branch | Coverage verdict |
|---|---|---|
| PowerShell | 0 | PowerShell coverage: **PASS** — zero changed PowerShell files, so no line-coverage obligation is triggered. Pester measures no branch coverage, so no branch figure is evaluated for it in any case. |
| Python | 0 | Python coverage: **PASS** — zero changed Python files, so no coverage obligation is triggered. |
| TypeScript | 0 | TypeScript coverage: **PASS** — zero changed TypeScript files, so no coverage obligation is triggered. |

This enumeration was derived from `git diff --name-status origin/main...HEAD` by file extension, not from
any caller-supplied language list.

## Findings

Seven non-blocking findings and one dispositioned coverage row. Zero blocking findings. Details, code
locations, and recommended remediations are in `code-review.2026-09-01T11-32.md`, where NB-1 through NB-4
carry the same identifiers and EQ-1 through EQ-3 appear under "Evidence-quality observations" as items 1,
2 and 3.

| ID | Severity | Summary | Blocking |
|---|---|---|---|
| NB-1 | Major | The worker loop clears `_consumerRunning` only on the normal `TryTake`-fails path. An exception raised by the `catch` handler itself escapes the loop and wedges the queue permanently. | No |
| NB-2 | Minor | `ConsumeAsync()` is public and can now corrupt the `_consumerRunning` invariant it does not own, reopening the window this change closed. | No |
| NB-3 | Minor | The drain barrier is queue-wide rather than per-batch as the specification prose describes, and it accepts no `CancellationToken` and has no upper bound. | No |
| NB-4 | Minor | `DASLFilterParserTests.PrintTree_WritesIndentedTreeToConsole` lacks `[DoNotParallelize]` and carries an unmitigated `Console.SetOut` parallelism flake. Confirmed, not refuted. | No |
| EQ-1 | Minor | `evidence/qa-gates/p8-t1-sanitisation.2026-09-01T11-15.md` claims the committed evidence tree "carries no absolute host path in any file's content". 16 committed evidence files plus `plan.2026-08-31T19-35.md` still contain the developer account token, and the TRX files contain `<account>@<HOST>`. | No |
| EQ-2 | Minor | The primary Cobertura XMLs are under gitignored `coverage/`, not committed under `<FEATURE>/evidence/`, so the figures cannot be re-derived from the repository alone once the working tree is cleaned. | No |
| EQ-3 | Minor | The Phase 7 pass-2 CSharpier check result exists only as a quoted summary line inside `p7-pass2-gates.2026-09-01T11-10.md`. Closed by this reviewer's own in-session re-run. | No |
| CV-1 | Coverage row | `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` per-file line coverage 49.41 %, below the 85 % modified-file floor. | No |

### CV-1 disposition rationale

The row is recorded as FAIL rather than PASS because 49.41 % is genuinely below the floor and this audit
does not record a false PASS. It is dispositioned non-blocking, and no remediation-inputs artifact is
produced, for five reasons taken together:

1. It is a pre-existing shortfall, not a regression. The file stood at 45.38 % on `origin/main`.
2. This change improved it by +4.03 points and reduced its uncovered line count from 136 to 128.
3. Every line this change touched in that file is covered; the uncovered remainder is untouched
   pre-existing code.
4. The uncovered remainder is Outlook-interop and WinForms-bound event-handler code of the class that
   `CLAUDE.md` UT2's COM/VSTO exemption describes, in an assembly whose testable seams this change did
   not touch.
5. Raising it to 85 % would require new tests across `QfcFormController`'s unmodified event handlers,
   which is a substantially larger change than issue #633 authorises and is constrained by the AC16
   footprint criterion the branch is delivering against.

The correct disposal is a separate coverage-uplift issue for `QfcFormController`, filed from its own
branch.

## Remediation

No remediation-inputs artifact is produced. There are zero blocking findings, and every non-blocking
finding either concerns a file outside this branch's authorised footprint (NB-4), concerns pre-existing
code this change did not touch (CV-1), is a hardening item best filed as its own issue (NB-1 through
NB-3), or is an evidence-hygiene item (EQ-1 through EQ-3). Per the delegating instruction, none of these
is promoted from this branch, because promotion would add paths the AC16 footprint criterion forbids.
They are stated in `code-review.2026-09-01T11-32.md` with enough file, location and rule detail to be
filed from a separate branch.
