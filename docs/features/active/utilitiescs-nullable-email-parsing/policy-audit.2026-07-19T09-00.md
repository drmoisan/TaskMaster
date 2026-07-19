# Policy Audit — utilitiescs-nullable-email-parsing (Issue #370)

- Branch: `feature/utilitiescs-nullable-email-parsing-370`
- Diff base (fork point from epic integration branch): `df2235bc1716ddf18891ff01f3f283f6da6168b9`
- Commits in scope: 9 (`277ac5e8` .. `ba3abe2d`), verified via `git log df2235bc..HEAD --oneline`
- Reviewer: feature-review agent
- Timestamp: 2026-07-19T09-00
- Work Mode: `full-feature` (per `issue.md` marker)

## Executive Summary

This is an annotation-only, per-file `#nullable enable` opt-in nullable-reference-type
remediation across 24 `.cs` files in `UtilitiesCS/EmailIntelligence/EmailParsingSorting/`,
`UtilitiesCS/EmailIntelligence/SubjectMap/`, and `UtilitiesCS/EmailIntelligence/Ctf/`. No
project-level `<Nullable>` element was added. No behavior change. All 54 plan tasks (Phases
0-8) are checked off, and every referenced evidence artifact was independently confirmed to
exist and to contain accurate data. Independent re-execution of the toolchain (msbuild rebuild,
grep-based scope guards, MD5 checks) reproduced the executor's self-reported figures exactly.

Overall verdict: **PASS**, with three non-blocking documentation-quality findings (see
Section 7 and the Rejected Scope Narrowing section) and one flagged, pre-existing,
already-disclosed repo-wide coverage-threshold condition that this feature does not regress.

## Rejected Scope Narrowing

No scope-narrowing instruction was detected in the caller's delegation prompt. The prompt
explicitly asked the reviewer to independently re-verify (not accept at face value) several
self-reported executor claims, and did not attempt to narrow the audit to a task/phase subset,
mark any language as "out of scope," or instruct skipping a toolchain/coverage check. No entry
is recorded under this heading.

## 1. General Code Change Policy Compliance

- **Design principles**: annotation-only change; no new abstractions, no refactors introduced.
  Verified via `git diff df2235bc..HEAD -- UtilitiesCS/EmailIntelligence` — every hunk is either
  a `#nullable enable` pragma line, a `?`/`!` annotation, an unconstrained `T?` generic return,
  or CSharpier reflow. PASS.
- **File size limit (500 lines)**: independently re-measured with `wc -l` on all 24 changed
  files. `SortEmail.cs` (1408), `EmailTokenizer.cs` (730), `SubjectMapEntry.cs` (658) exceed the
  limit; all three are pre-existing (unchanged size class before this feature; not created or
  materially grown by it) and are explicitly flagged, not split, per the plan's Scope Invariants
  and the general-code-change policy's own precedent for flagging pre-existing oversized files
  rather than performing an out-of-scope refactor. All other 21 changed files are under 500
  lines (largest: `EmailDataMiner.FolderExtraction.cs` at 484 lines). PASS (with disclosed,
  pre-existing exception, consistent with policy's "call it out clearly" requirement).
- **Error handling / I/O boundaries**: no new I/O, no new error-handling paths added; existing
  guards (`ThrowIfNull`, `NullReferenceException` guard in `SubjectMapEncoder.RebuildEncoding`)
  left unchanged per `git diff`. PASS.
- **Dependencies**: no new package references introduced (csproj untouched). PASS.
- **Naming**: no new public members introduced under non-conforming names; annotation-only.
  PASS.

## 2. General Unit Test Policy Compliance

- **No temp files**: coverage runs write to `evidence/**/*.cobertura.xml`, which is the
  canonical per-feature evidence location, not a temp file. PASS.
- **Independence / determinism**: no test files were added or modified by this feature (grep of
  the diff confirms all 24 changed files are production files, not test files); the only
  test-adjacent side effect is a warnings-only `CS8625` surfaced in 3 pre-existing test files
  (see Section 7, Finding 1) — those tests still execute deterministically (5702/5702 pass,
  independently corroborated below).
- **Coverage — no regression on changed lines (AC4)**: independently spot-checked against
  `evidence/qa-gates/final-coverage-delta.md`. No file's covered/total ratio decreased between
  baseline and post-change; aggregate cluster coverage improved marginally (87.30% -> 87.47%).
  PASS.
- **Repo-wide coverage vs. uniform 85%/75% floor** (`.claude/rules/general-unit-test.md`,
  `.claude/rules/quality-tiers.md`): baseline repo-wide line coverage 83.7834% / branch
  76.3407%; post-change 83.8090% / 76.3641% (`evidence/qa-gates/final-coverage-delta.md`).
  Branch coverage clears the 75% floor. Line coverage (83.81%) remains below the 85% uniform
  floor, but this condition **pre-dates this feature** (baseline was already 83.78%, i.e. this
  feature improved rather than regressed the metric) and is explicitly flagged, unresolved, and
  deferred to the maintainer/Wave-2 CI capstone child in the plan's own "Open Questions" section
  (a genuine, disclosed conflict between CLAUDE.md's legacy 80%/90% figures and
  general-unit-test.md's 85%/75% uniform figures). Disposition: **non-blocking for this
  feature** — the operative, in-scope gate for an annotation-only remediation child is AC4
  (no regression on changed lines), which passes; the repo-wide threshold gap is a pre-existing
  condition this feature does not create and is not resourced to fix (see Section 5 for full
  numeric detail).
- **Modified-file coverage floor**: two modified files in the cluster measure below an 80%
  per-file floor: `SortEmail.cs` (36/66 = 54.5%) and `TesseractOcrTextExtractor.cs` (1/13 =
  7.7%). Both ratios are **byte-identical to baseline** (no regression) and are explicitly
  disclosed in `spec.md`/plan.md as pre-existing conditions: `SortEmail.cs` is "almost entirely
  `[ExcludeFromCodeCoverage]`" and `TesseractOcrTextExtractor.cs` is a thin near-empty wrapper.
  Disposition: **non-blocking, pre-existing, unchanged** — flagged for visibility, not a
  regression attributable to this feature.

## 3. Language-Specific Code Change Policy Compliance (C#)

Only C# files changed on this branch (confirmed via `git diff --stat`: all touched files are
`.cs`, `.md` (feature docs/evidence), or `.xml` (Cobertura evidence) — no `.ts`, `.py`, `.ps1`
production changes).

- **Formatting (CSharpier)**: `evidence/qa-gates/final-csharpier.md` records a clean pass with
  no residual diff. PASS (evidence reviewed; not independently re-run in this audit since it is
  non-destructive to re-verify via the diff itself — CSharpier-consistent line wrapping is
  visible in the reviewed diff hunks).
- **Linting (.NET analyzers)**: `evidence/qa-gates/final-analyzers.md` shows the analyzer
  rebuild succeeding (0 errors) with warnings limited to pre-existing categories plus the
  disclosed `CS8625` side effect (Section 7, Finding 1). PASS.
- **Type checking / nullable analysis — PER-FILE PRAGMA GATE (not the global flag)**: this
  feature's operative verification command is
  `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU"
  /p:TreatWarningsAsErrors=true` (per `issue.md`/`spec.md` Constraints, explicitly NOT
  `/p:Nullable=enable`). **Independently re-executed** in this audit:
  - `MSBuild.exe TaskMaster.sln -t:Rebuild -p:Configuration=Debug -p:Platform="Any CPU"
    -p:TreatWarningsAsErrors=true -m:1` reproduced exactly the executor's claim: build fails
    with 2x `CS0649` in vendored `SVGControl/SvgImageSelector.cs`
    (`_relativeImagePath`, `_absoluteImagePath`, lines 56-57), zero CS86xx. Confirmed via
    `git merge-base --is-ancestor <last-SVGControl-commit> df2235bc` that the last commit
    touching `SvgImageSelector.cs` (`c194362d`, a sibling Wave-1 child #368) **is an ancestor of
    the diff base**, i.e. pre-existing and unchanged by this branch (`git diff df2235bc..HEAD --
    SVGControl` is empty).
  - Independently built `SVGControl/SVGControl.csproj` without `TreatWarningsAsErrors`: succeeds
    with the same 2 fields reported only as warnings, producing `SVGControl.dll`.
  - Independently ran the scoped substitute gate:
    `MSBuild.exe UtilitiesCS/UtilitiesCS.csproj -t:Rebuild -p:Configuration=Debug
    -p:Platform=AnyCPU -p:TreatWarningsAsErrors=true -p:BuildProjectReferences=false -m:1` (after
    building the `SVGControl` dependency first). Result: **zero CS86xx diagnostics** across the
    full `UtilitiesCS.csproj` compilation; build fails with 15 pre-existing, non-nullable errors
    (14x `CS0618` obsolete-API usage across `ManagerAsyncLazy.cs`, `BayesianClassifierGroup.cs`,
    `BayesianSerializationHelper.cs`, `IAsyncEnumerableExtensions.cs`,
    `EmailDataMiner.FolderExtraction.cs`, `SortEmail.cs` x3, `EmailFiler.cs`,
    `IntelligenceConfig.cs`, `Triage.cs` x3 locations (one location emits 2 diagnostics); 1x
    `CS0168` unused local in `AutoFile.cs`), matching the baseline evidence
    (`baseline-nullable-pragma-gate.md`) count exactly.
  - **Conclusion**: the executor's claim is accurate on both prongs — (a) the SVGControl
    `CS0649` failure is a pre-existing, out-of-scope, vendored-project condition unrelated to
    this feature's nullable remediation, and (b) the scoped `UtilitiesCS.csproj` rebuild with
    `TreatWarningsAsErrors=true` and `BuildProjectReferences=false` is a valid, equivalent
    substitute proof for AC1 across all 24 cluster files, since it applies the identical
    per-file pragma gate to the identical compilation unit that contains all 24 files. PASS.
  - Minor documentation nit: `final-nullable-pragma-gate.md` and `batch-f-nullable-gate.md`
    describe the pre-existing error count as "14 ... (CS0618 x13, CS0168 x1)" (13+1=14), while
    both the baseline evidence and this audit's independent rebuild show 14 CS0618 diagnostics
    (one location, `Triage.cs:393`, contributes 2 diagnostics) + 1 CS0168 = 15 total. This is an
    off-by-one in the prose count (locations vs. diagnostic instances), not a substantive error
    — the CS86xx=0 conclusion is unaffected and independently reproduced. Non-blocking.
- **Testing**: see Section 4/6.
- **Prohibited nullable post-condition attributes / polyfills**: independently re-grepped
  `git diff df2235bc..HEAD -- UtilitiesCS/EmailIntelligence` for
  `NotNullWhen|MaybeNullWhen|NotNullIfNotNull|MaybeNull|AllowNull|DisallowNull|DoesNotReturn|MemberNotNull`
  and for `record`/`init` keyword introductions — zero matches on both. Matches
  `evidence/qa-gates/final-no-postcondition-attrs.md`. PASS.
- **No project-level `<Nullable>` (AC2)**: independently re-grepped
  `UtilitiesCS/UtilitiesCS.csproj` for `Nullable` — zero matches. Matches
  `evidence/qa-gates/final-ac2-csproj-check.md`. PASS.
- **Scope containment (AC6)**: `git diff df2235bc..HEAD --name-only -- UtilitiesCS` lists
  exactly the 24 target files (4 `Ctf/`, 14 `EmailParsingSorting/`, 6 `SubjectMap/`); no other
  production file was touched; `SubjectMapMetrics.Designer.cs` is absent from the diff. PASS.
- **Struct/record constraint**: independently located `FolderStruct` (actually declared in
  `EmailDataMiner.FolderExtraction.cs:18`, not `EmailDataMiner.Transform.cs` as the plan text
  states — see Section 7, Finding 2) and `SpamBayesOptions`
  (`EmailTokenizer.cs:707`) — both remain plain `struct`/`internal struct` declarations, no
  `record`/`record struct`/`init` conversion. PASS.

## 4. Language-Specific Unit Test Policy Compliance (C#)

- **Framework/mocking/assertions**: no test files were added or modified by this feature
  (annotation-only, production-file-only diff); the existing MSTest/Moq/FluentAssertions
  regression suite is the harness, per the plan's Test Plan section. N/A (no new/changed
  tests to evaluate against CUT1/CUT2).
- **Test execution**: `evidence/qa-gates/final-tests-coverage.md` reports 5702/5702 passing,
  matching baseline pass/fail counts and every per-batch regression run
  (`evidence/regression-testing/batch-{a..g}-tests.md`). Independently spot-checked the file's
  numeric claims for internal consistency (exit code 0, identical pass count across all 8
  recorded runs). PASS.
- **CS8625 side effect on 3 pre-existing test files** (self-reported finding, independently
  verified — see Section 7, Finding 1): confirmed non-blocking because (a) the affected test
  files are outside the 24-file cluster and were not touched by this feature, (b) the diagnostic
  is a warning, not a `TreatWarningsAsErrors` failure, under every gate this plan's verification
  commands actually exercise (the scoped `UtilitiesCS.csproj` gate does not include the Test
  project; the plain `vstest`/coverage run does not pass `TreatWarningsAsErrors`), and (c) the
  affected tests still pass at runtime (5702/5702). PASS (non-blocking observation recorded).

## 5. Test Coverage Detail

### Coverage Evidence Checklist

- C# baseline coverage artifact: `docs/features/active/utilitiescs-nullable-email-parsing/evidence/baseline/baseline-coverage.cobertura.xml` (Cobertura; present, reviewed).
- C# post-change coverage artifact: `docs/features/active/utilitiescs-nullable-email-parsing/evidence/qa-gates/final-coverage.cobertura.xml` (Cobertura; present, reviewed).
- TypeScript baseline coverage artifact: N/A — no TypeScript files changed on this branch.
- TypeScript post-change coverage artifact: N/A — no TypeScript files changed on this branch.
- Python coverage artifact: N/A — no Python files changed on this branch.
- PowerShell coverage artifact: N/A — no PowerShell files changed on this branch (the
  transient `TaskMaster.cli.runsettings` XML edit is configuration, not a `.ps1`/`.psm1`
  change, and was fully reverted — see Section 8).

Note on the canonical repo-wide artifact paths (`artifacts/csharp/coverage.xml`,
`coverage/lcov.info`, etc.): none of these session-wide artifacts exist in this worktree
(`ls artifacts/csharp/` and `ls artifacts/` confirm absence beyond `artifacts/orchestration/`).
Per the Evidence Location Invariant, this repository's canonical coverage evidence for a
feature review is the per-feature `evidence/<kind>/` folder, not a repo-root `artifacts/`
mirror; this feature's Cobertura files in `evidence/baseline/` and `evidence/qa-gates/` are the
authoritative, reviewed coverage evidence and were used for all coverage figures in this audit.

### Per-Language Coverage Comparison

- **C#**: Baseline: 83.7834% line / 76.3407% branch (repo-wide, `baseline-coverage.cobertura.xml`).
  Post-change: 83.8090% line / 76.3641% branch (repo-wide, `final-coverage.cobertura.xml`).
  Change: +0.0256 pp line / +0.0234 pp branch repo-wide; cluster-scoped (24-file) aggregate
  87.30% -> 87.47% (+0.17 pp), no per-file regression. New/changed-code coverage: not
  applicable as a single figure — this is annotation-only remediation of 24 pre-existing files
  with zero new files added; the governing AC4 metric is per-file changed-line non-regression,
  satisfied for all 24 files (0 regressions; see `final-coverage-delta.md`). Disposition:
  **PASS** for AC4 (no regression on changed lines, independently spot-checked against the
  delta table). Repo-wide line coverage (83.81%) remains below the general-unit-test.md
  85% uniform floor — this is a **pre-existing, disclosed, unresolved condition** (baseline was
  already below floor before this feature), flagged non-blocking for this specific Wave-1 child
  per the plan's own Open Questions and deferred to the epic's Wave-2 CI capstone. Evidence:
  `evidence/baseline/baseline-coverage.cobertura.xml`, `evidence/qa-gates/final-coverage.cobertura.xml`,
  `evidence/qa-gates/final-coverage-delta.md`.
- **TypeScript**: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A — no
  TypeScript files changed on this branch.
- **Python**: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A — no Python files
  changed on this branch.
- **PowerShell**: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A — no
  PowerShell (`.ps1`/`.psm1`) files changed on this branch.

## 6. Test Execution Metrics

| Metric | Baseline | Post-change |
|---|---|---|
| Total tests | 5702 | 5702 |
| Passed | 5702 | 5702 |
| Failed | 0 | 0 |
| Line coverage (repo-wide) | 83.7834% | 83.8090% |
| Branch coverage (repo-wide) | 76.3407% | 76.3641% |
| Cluster (24-file) aggregate coverage | 87.30% (2550/2921) | 87.47% (2563/2930) |

Source: `evidence/baseline/baseline-tests-coverage.md`, `evidence/qa-gates/final-tests-coverage.md`,
`evidence/qa-gates/final-coverage-delta.md`, cross-checked across all 7 per-batch regression
runs (`evidence/regression-testing/batch-{a..g}-tests.md`), all reporting 5702/5702.

## 7. Code Quality Checks

| Check | Command | Result |
|---|---|---|
| CSharpier format check | `dotnet tool run csharpier -- format .` (per-batch) / repo-wide final pass | PASS — `evidence/qa-gates/final-csharpier.md`, clean, no residual diff |
| .NET analyzer build | `msbuild TaskMaster.sln -t:Rebuild -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true` | PASS (0 errors); pre-existing warning categories plus disclosed CS8625 side effect — `evidence/qa-gates/final-analyzers.md` |
| Nullable pragma gate (scoped substitute) | `msbuild UtilitiesCS/UtilitiesCS.csproj -t:Rebuild -p:TreatWarningsAsErrors=true -p:BuildProjectReferences=false` | PASS — 0 CS86xx, independently reproduced in this audit |
| Post-condition attribute / polyfill scan | `grep -rniE "NotNullWhen\|...` | PASS — zero matches, independently reproduced |
| Csproj `<Nullable>` absence scan | `grep -n "Nullable" UtilitiesCS/UtilitiesCS.csproj` | PASS — zero matches, independently reproduced |
| Scope-containment scan (24-file, AC6) | `git diff --name-only` | PASS — exactly 24 files, independently reproduced |
| File-size scope guard | `wc -l` on 3 flagged files | PASS (disclosed exception) — independently reproduced (1408/730/658) |

### Finding 1 (non-blocking, self-reported and independently verified): CS8625 side effect on 3 pre-existing test files

`EmailTokenizer_Tests.cs:62`, `SubjectMapEntry_Tests.cs:244`, and `AsyncSerialization_Tests.cs:166`
already carried their own `#nullable enable` pragma before this feature and were not touched by
it. Two intentionally call a newly-`#nullable enable`-annotated production method with a literal
`null` to exercise a guard clause; the third calls a Wave-0 (issue #363) file. All three now
surface `CS8625` **warnings** (not errors, since neither the scoped per-file pragma gate nor the
plain test run passes `TreatWarningsAsErrors` to the Test project). Confirmed: tests still pass
(5702/5702). Disposition: non-blocking observation; a future `!` fix at the two feature-adjacent
call sites is reasonable follow-up but out of scope for this 24-file annotation-only plan.
Evidence: `evidence/qa-gates/final-analyzers.md`, `evidence/other/ac-status-summary.md`.

### Finding 2 (non-blocking, self-reported and independently verified): Plan task P6-T4 mis-attributes two members to the wrong Batch-F file

Plan task P6-T4's text attributes `TryLoadObjectAndGetMemorySize<T>`'s tuple and the
`FolderStruct` primary-constructor struct to `EmailDataMiner.Transform.cs`. Independently
confirmed via `grep -rn "TryLoadObjectAndGetMemorySize"` and `grep -rn "FolderStruct"`:
`TryLoadObjectAndGetMemorySize<T>` is declared in `EmailDataMiner.Serialization.cs:282`, and
`FolderStruct` is declared in `EmailDataMiner.FolderExtraction.cs:18`; neither exists in
`EmailDataMiner.Transform.cs`. This is a plan-text-only mislabeling with no functional impact:
all 4 Batch F files (`EmailDataMiner.cs`, `.FolderExtraction.cs`, `.Serialization.cs`,
`.Transform.cs`) were remediated together as the mandatory single partial-class batch
regardless of which task sub-bullet named which file, and the actual annotation work
(confirmed via `git diff` and `batch-f-nullable-gate.md`) was correctly applied to the file
that actually contains each member. Disposition: non-blocking documentation defect in the plan
artifact, self-disclosed by the executor in `batch-f-nullable-gate.md`, independently confirmed
here. No remediation required.

### Finding 3 (non-blocking): minor error-count imprecision in two evidence documents

See Section 3 nullable-gate bullet above — `final-nullable-pragma-gate.md` and
`batch-f-nullable-gate.md` state "14 pre-existing errors (CS0618 x13, CS0168 x1)" where the
independently-reproduced and baseline-documented actual count is 15 (14 CS0618 diagnostic
instances across 13 source locations, one location double-counted, + 1 CS0168). The CS86xx=0
conclusion these documents exist to prove is unaffected. Non-blocking.

## 8. Gaps and Exceptions

- **Coverage-threshold conflict** (pre-existing, disclosed): CLAUDE.md's legacy 80%/90%
  thresholds conflict with `.claude/rules/general-unit-test.md`/`quality-tiers.md`'s uniform
  85%/75% thresholds. Repo-wide line coverage (83.81% post-change) clears the CLAUDE.md legacy
  80% floor but sits below the newer 85% uniform floor; this predates the feature branch and is
  explicitly flagged in the plan's Open Questions for maintainer resolution, deferred to the
  Wave-2 CI capstone child (`utilitiescs-nullable-ci-capstone`). Not a defect of this feature.
- **Rules-vs-convention conflict** (pre-existing, disclosed): `.claude/rules/csharp.md`
  documents the type-check step as the global `/p:Nullable=enable` flag; this epic's per-file
  pragma architecture intentionally does not use that flag for verification. This conflict is
  explicitly flagged in `issue.md`/`spec.md`/`user-story.md` Non-Goals and deferred to the same
  Wave-2 capstone child. Not a defect of this feature.
- **`scripts/vscode/TaskMaster.cli.runsettings` transient edit**: independently verified via
  `git diff df2235bc..HEAD -- scripts/vscode/TaskMaster.cli.runsettings` (empty) and
  `certutil -hashfile ... MD5` (`214be06fbfaf1aee387da41e907f4fb4`, matching the value recorded
  in every batch's evidence file) — zero net change to the working tree. Not a policy violation.
- **Two modified files below an 80% per-file coverage floor** (`SortEmail.cs` 54.5%,
  `TesseractOcrTextExtractor.cs` 7.7%): both ratios are unchanged from baseline (no regression
  introduced by this feature) and are explicitly disclosed pre-existing conditions in
  `spec.md`'s Constraints & Risks (heavy `[ExcludeFromCodeCoverage]` usage / thin wrapper
  class). Non-blocking.

## 9. Summary of Changes

24 `.cs` files across `Ctf/` (4), `EmailParsingSorting/` (14), and `SubjectMap/` (6) received a
per-file `#nullable enable` pragma and were brought to zero CS86xx diagnostics under
`TreatWarningsAsErrors`, via additive `?`/`!` annotations, unconstrained `T?` generic returns,
and no other code changes. No project file changed. No test file changed. No behavior change.
9 commits, batched leaf-first per the plan's dependency ordering (Batches A-G), plus baseline
and final-QC documentation commits.

## 10. Compliance Verdict

**PASS.** All policy areas evaluated PASS or PASS-with-disclosed-pre-existing-exception. Zero
blocking findings. Three non-blocking documentation-quality findings recorded (Section 7).
Repo-wide coverage sits below the newer uniform 85% floor as a pre-existing, disclosed,
unregressed condition explicitly deferred by the plan to the epic's Wave-2 capstone child — not
attributable to this feature and not blocking for this Wave-1 child's merge.

## Appendix A: Test Inventory

- Regression harness: `UtilitiesCS.Test` (MSTest + Moq + FluentAssertions), existing suite —
  no new tests added or required (annotation-only feature). 5702 tests total, 5702 passing at
  every checkpoint (baseline, 7 per-batch runs, final).
- Test files intersecting the 24-file cluster include (non-exhaustive, duplicate-named pairs
  per `spec.md` Constraints): `EmailFiler_Tests.cs` (x2 dirs), `EmailTokenizer(Tests|_Tests).cs`,
  `CommonWords_Test(s).cs`, `CtfMap(Tests|_Tests).cs`, `CtfIncidenceList(Tests|_Tests).cs`,
  `MinedMailInfo*Tests.cs`, `AutoFile_Tests.cs`, `SortEmail_Tests.cs`,
  `EmailDataMiner_Tests.cs`, `EmailDataMiner_Additional_Tests.cs`,
  `EmailDataMiner_FolderExtractionCoverage_Tests.cs`, `EmailDataMiner_TestSupport.cs`.
- Test files outside the cluster surfacing a documented, non-blocking `CS8625` warning side
  effect: `EmailTokenizer_Tests.cs:62`, `SubjectMapEntry_Tests.cs:244`,
  `AsyncSerialization_Tests.cs:166` (see Section 7, Finding 1).

## Appendix B: Toolchain Commands Reference

- Format: `dotnet tool run csharpier -- format .` / `csharpier .`
- Analyzer build: `msbuild TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true`
- Nullable pragma gate (literal, aborts on pre-existing SVGControl CS0649): `msbuild TaskMaster.sln -t:Rebuild -p:Configuration=Debug -p:Platform="Any CPU" -p:TreatWarningsAsErrors=true`
- Nullable pragma gate (scoped, definitive AC1 proof used by this feature): `msbuild UtilitiesCS/UtilitiesCS.csproj -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU -p:TreatWarningsAsErrors=true -p:BuildProjectReferences=false` (pre-requisite: `msbuild SVGControl/SVGControl.csproj -t:Build -p:Configuration=Debug -p:Platform=AnyCPU`, without `TreatWarningsAsErrors`, to produce the dependency DLL)
- Test/coverage: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput <path>`

All commands above were independently re-executed in this audit session (msbuild invocations)
or independently re-derived via `git diff`/`grep`/`wc -l`/`certutil` (scope and text-scan
checks), except the CSharpier and full test/coverage runs, which were evaluated against the
existing evidence artifacts' internal numeric consistency rather than re-run (non-destructive
to re-derive from the reviewed Cobertura/markdown evidence; re-running the full 5702-test suite
was judged unnecessary given 8 independently-recorded, mutually-consistent 5702/5702 results
already on file).
