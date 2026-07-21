# Remediation Plan — Issue #398 (breadcrumb-suggestions-upgrade-race)

- Plan timestamp: 2026-07-20T22-30
- Work Mode: minor-audit (issue.md `## Acceptance Criteria` is the sole AC source; AC-5 coverage sub-clause is the PARTIAL item under remediation)
- Feature folder: docs/features/active/2026-07-20-breadcrumb-suggestions-upgrade-race-398
- Branch: bug/breadcrumb-suggestions-upgrade-race-398 (HEAD 1cb031f6)
- Requirements source: docs/features/active/2026-07-20-breadcrumb-suggestions-upgrade-race-398/issue.md
- Findings source: docs/features/active/2026-07-20-breadcrumb-suggestions-upgrade-race-398/remediation-inputs.2026-07-20T22-30.md
- Evidence root: docs/features/active/2026-07-20-breadcrumb-suggestions-upgrade-race-398/evidence/

## Scope Lock

Exactly two findings; no production-code changes.

- R1 (Major, policy FAIL): two test files exceed the 500-line limit.
  - UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs (536 lines)
  - UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs (545 lines)
  - Remedy: split each into cohesive, scenario-grouped files each < 500 lines; wire new files into
    UtilitiesCS.Test.csproj with explicit `<Compile Include>` items (legacy packages.config project,
    no glob). Full MSTest suite must remain 5061/5061.
- R2 (procedural FAIL; AC-5 coverage sub-clause): the canonical HEAD C# coverage artifact at
  artifacts/csharp/coverage.xml is absent.
  - Remedy: regenerate a HEAD-reflecting, first-party-scoped coverage artifact, converted from
    Cobertura to JaCoCo (so `//counter[@type="LINE"]` / `//counter[@type="BRANCH"]` parse), confirm
    repo-wide first-party line >= 85% and branch >= 75%, then confirm the AC-5 coverage sub-clause.

Out of scope: any change to production `*.cs`, `*.csproj` outside the two `<Compile Include>` additions,
or the breadcrumb fix logic. The non-blocking memory-model observation is not remediated.

## Split Design (deterministic, R1)

Both over-limit classes are split by converting each into a `sealed partial class` across two files so
the shared private helper methods stay in one place (simplicity-first; no helper duplication; every test
method exists in exactly one file). Class names, namespaces, and `[TestClass]` semantics are unchanged.

- BreadcrumbStateModelTests (536 -> two files, `public sealed partial class BreadcrumbStateModelTests`):
  - Kept file: UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs — usings, namespace,
    shared helpers (`Key`, `Segment`, `ThreeSegmentChain`, `ModelWithSuggestion`), and the Positive /
    Negative / Edge-case test groups.
  - New file: UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs — the
    State-transition-sequence group and the #398 atomic-replace (`ReplaceRows`) group with its
    `PlainRows` helper.
- FolderBreadcrumbBridgeRouterTests (545 -> two files,
  `public sealed partial class FolderBreadcrumbBridgeRouterTests`):
  - Kept file: UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs — usings,
    namespace, shared helpers (`LeafPath`, key fields, `Key`, `Segment`, `LeafChain`, `ProviderMock`,
    `PopulatedRouterAsync`), and the Positive-routing / Negative-routing / Edge-fall-through groups.
  - New file: UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterInFlightTests.cs — the
    multi-message State-transition-sequence group and the misc constructor/null/plain-row tests, plus the
    #398 in-flight rebuild invariant group with its `SecondPath`/`SecondKey`/`GatedTwoRowProvider`/
    `TwoScoredRows` helpers.

## Coverage Artifact Notes (R2)

- artifacts/csharp/coverage.xml is the coverage-gate tooling-input path read by
  `.claude/hooks/validate-feature-review-coverage.ps1` (`Get-JacocoRepoCoverage` sums
  `//counter[@type="LINE"]`; `Get-JacocoBranchCoverage` sums `//counter[@type="BRANCH"]`; floors line
  >= 85% / branch >= 75%). It is NOT an evidence output path and is explicitly permitted by
  `enforce-evidence-locations.ps1`; all evidence records about its generation are written under
  `<FEATURE>/evidence/<kind>/`.
- The denominator is the first-party production packages subject to the coverage floor
  (UtilitiesCS + QuickFiler as instrumented), excluding vendored third-party assemblies (via
  coverage.config) and the ratified COM/VSTO-exempt assemblies. This is the first-party denominator, not
  a cherry-picked subset. Prior verified post-change scope figures (production-identical to this
  test-only remediation): line 86.54%, branch 80.26%.
- Conversion is a Cobertura -> JaCoCo transform, not a copy. The transform may use an installed
  Cobertura->JaCoCo converter or a throwaway conversion script created and deleted within the executor
  session (throwaway session scripts are exempt from the 500-line and durable-script rules); it must not
  add a committed reusable script or a new package dependency.

## vstest Discovery Constraint

All vstest/coverage runs target the two first-party test assemblies by explicit path
(UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll and QuickFiler.Test\bin\Debug\QuickFiler.Test.dll). Any
recursive `*.Test.dll` discovery MUST exclude every path containing `\.claude\` so stale agent-worktree
builds are never loaded.

---

### Phase 0 — Baseline Capture

Read policies in required order, capture the C# toolchain baseline, and record the starting FAIL state
for R1 and R2. Baseline evidence is written under
docs/features/active/2026-07-20-breadcrumb-suggestions-upgrade-race-398/evidence/remediation-baseline/.
Each command-step artifact records `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.

- [x] [P0-T1] Read the policy files in order (CLAUDE.md; .claude/rules/general-code-change.md;
  .claude/rules/general-unit-test.md; .claude/rules/csharp.md) and write
  evidence/remediation-baseline/phase0-instructions-read.md containing `Timestamp:`, `Policy Order:`,
  and the explicit list of files read. Acceptance: the artifact exists with all three fields populated.
- [x] [P0-T2] Run `csharpier .` in check mode and write
  evidence/remediation-baseline/csharpier.2026-07-20T22-30.md. Acceptance: artifact records the exact
  command, `EXIT_CODE:`, and an `Output Summary:` stating whether the tree is already formatted.
- [x] [P0-T3] Run the analyzer build
  (`msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`)
  and write evidence/remediation-baseline/analyzer-build.2026-07-20T22-30.md. Acceptance: artifact
  records command, `EXIT_CODE:`, and pass/fail summary.
- [x] [P0-T4] Run the nullable build
  (`msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`)
  and write evidence/remediation-baseline/nullable-build.2026-07-20T22-30.md. Acceptance: artifact
  records command, `EXIT_CODE:`, and pass/fail summary.
- [x] [P0-T5] Run the full first-party suite with coverage against the two explicit assemblies
  (`vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`,
  excluding any `\.claude\` path) and write
  evidence/remediation-baseline/tests-coverage.2026-07-20T22-30.md. Acceptance: artifact records the
  command, `EXIT_CODE:`, total/passed/failed counts (expected 5061/5061/0), and the numeric coverage
  headline (instrumented-scope line % and branch %) in `Output Summary:`.
- [x] [P0-T6] Measure and record the current line counts of the two over-limit files into
  evidence/remediation-baseline/file-line-counts.2026-07-20T22-30.md. Acceptance: artifact records
  BreadcrumbStateModelTests.cs = 536 and FolderBreadcrumbBridgeRouterTests.cs = 545 (or the measured
  values), documenting the R1 FAIL starting state.
- [x] [P0-T7] Confirm the R2 starting state by recording the absence (or staleness) of
  artifacts/csharp/coverage.xml into
  evidence/remediation-baseline/coverage-artifact-absence.2026-07-20T22-30.md. Acceptance: artifact
  records `SearchScope:`, `SearchPatterns:`, and `SearchResult:` for artifacts/csharp/coverage.xml,
  establishing that no valid HEAD JaCoCo artifact exists.

### Phase 1 — R1 Test-File Split

Split the two over-limit test files per the Split Design. No production `*.cs` is modified. Every test
method must exist in exactly one file after the split (no duplicates, no losses).

- [x] [P1-T1] Create UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs
  declaring `public sealed partial class BreadcrumbStateModelTests` in namespace
  `UtilitiesCS.Test.OutlookObjects.Folder`, containing the State-transition-sequence test group and the
  #398 `ReplaceRows` group plus its `PlainRows` helper (moved verbatim from BreadcrumbStateModelTests.cs),
  with the required usings. Acceptance: file exists, compiles as a partial of the same class, and is
  < 500 lines.
- [x] [P1-T2] Edit UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs to declare
  `public sealed partial class BreadcrumbStateModelTests` and retain only the shared helpers and the
  Positive / Negative / Edge-case groups (the sequence and `ReplaceRows` groups removed). Acceptance:
  file is < 500 lines, contains no method now living in BreadcrumbStateModelSequenceTests.cs, and the
  shared helpers remain present exactly once.
- [x] [P1-T3] Create UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterInFlightTests.cs
  declaring `public sealed partial class FolderBreadcrumbBridgeRouterTests` in namespace
  `UtilitiesCS.Test.OutlookObjects.Folder`, containing the multi-message State-transition-sequence group,
  the misc constructor/null/plain-row tests, and the #398 in-flight rebuild invariant group plus its
  `SecondPath`/`SecondKey`/`GatedTwoRowProvider`/`TwoScoredRows` helpers (moved verbatim from
  FolderBreadcrumbBridgeRouterTests.cs), with the required usings. Acceptance: file exists, compiles as a
  partial of the same class, and is < 500 lines.
- [x] [P1-T4] Edit UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs to declare
  `public sealed partial class FolderBreadcrumbBridgeRouterTests` and retain only the shared helpers and
  the Positive-routing / Negative-routing / Edge-fall-through groups. Acceptance: file is < 500 lines,
  contains no method now living in FolderBreadcrumbBridgeRouterInFlightTests.cs, and the shared helpers
  remain present exactly once.
- [x] [P1-T5] Add two explicit `<Compile Include>` items to UtilitiesCS.Test/UtilitiesCS.Test.csproj —
  `OutlookObjects\Folder\BreadcrumbStateModelSequenceTests.cs` and
  `OutlookObjects\Folder\FolderBreadcrumbBridgeRouterInFlightTests.cs` — adjacent to the existing
  BreadcrumbStateModelTests.cs / FolderBreadcrumbBridgeRouterTests.cs entries. Acceptance: both new
  entries are present and the existing two entries are unchanged.
- [x] [P1-T6] Measure the four resulting files and write
  evidence/qa-gates/file-line-counts-post-split.2026-07-20T22-30.md. Acceptance: artifact records each
  of the four files' line counts, all four < 500, with `Timestamp:` and `Output Summary:`.

### Phase 2 — Final QC Loop, Coverage Regeneration (R2), and AC-5 Confirmation

Run the full C# toolchain in order. If any step changes files or fails, fix and restart from P2-T1.
Every command task is unconditional and records its own qa-gate artifact under
docs/features/active/2026-07-20-breadcrumb-suggestions-upgrade-race-398/evidence/qa-gates/ with
`Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.

- [x] [P2-T1] Run `csharpier .` (global) and write evidence/qa-gates/csharpier.2026-07-20T22-30.md.
  Acceptance: `EXIT_CODE: 0` and `Output Summary:` confirms no files needed reformatting; if any file was
  reformatted, restart the loop from P2-T1.
- [x] [P2-T2] Run the analyzer build
  (`msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`)
  and write evidence/qa-gates/analyzer-build.2026-07-20T22-30.md. Acceptance: `EXIT_CODE: 0` with zero
  analyzer errors.
- [x] [P2-T3] Run the nullable build
  (`msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`)
  and write evidence/qa-gates/nullable-build.2026-07-20T22-30.md. Acceptance: `EXIT_CODE: 0` with zero
  nullable/warning-as-error failures.
- [x] [P2-T4] Run the full first-party suite with coverage against the two explicit assemblies
  (`vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`,
  excluding any `\.claude\` path) and write evidence/qa-gates/tests-coverage.2026-07-20T22-30.md.
  Acceptance: `EXIT_CODE: 0`, total/passed/failed = 5061/5061/0, and the numeric coverage headline
  (instrumented-scope line % and branch %) recorded in `Output Summary:`.
- [x] [P2-T5] Generate the HEAD first-party-scoped Cobertura coverage over the two explicit test
  assemblies (excluding any `\.claude\` path, with vendored/third-party excluded via coverage.config),
  convert it Cobertura -> JaCoCo, and write the JaCoCo result to artifacts/csharp/coverage.xml; record
  the generation in evidence/qa-gates/jacoco-coverage-artifact.2026-07-20T22-30.md. Acceptance:
  artifacts/csharp/coverage.xml exists and is valid JaCoCo XML containing `//counter[@type="LINE"]` and
  `//counter[@type="BRANCH"]` aggregated over the first-party production denominator; the evidence
  artifact records the command(s), `EXIT_CODE:`, and the conversion mechanism used.
- [x] [P2-T6] Verify the JaCoCo artifact parses to passing floors by dot-sourcing the gate functions
  `Get-JacocoRepoCoverage` and `Get-JacocoBranchCoverage` from
  .claude/hooks/validate-feature-review-coverage.ps1 against artifacts/csharp/coverage.xml, and write
  evidence/qa-gates/coverage-floor-verification.2026-07-20T22-30.md. Acceptance: recorded numeric
  first-party line coverage >= 85% and branch coverage >= 75% (both values, not placeholders).
- [x] [P2-T7] Record the no-regression determination into
  evidence/qa-gates/coverage-delta.2026-07-20T22-30.md. Acceptance: artifact states the Phase 0 baseline
  line/branch values (from P0-T5) and the post-change values (from P2-T6), confirms no regression, and
  notes that this remediation changes only test files so there is no new/changed production code
  (the >= 90% new-code target is not re-triggered; the prior fix's new-code coverage remains 100%).
- [x] [P2-T8] Confirm the AC-5 coverage sub-clause in issue.md and write the issue-update mirror at
  evidence/issue-updates/issue-398.2026-07-20T22-30.md. Acceptance: the mirror records the confirmed
  numeric coverage and the artifacts/csharp/coverage.xml path, issue.md AC-5 carries a confirmation
  annotation referencing the regenerated canonical artifact, and the mirror includes `Timestamp:` and
  `PostedAs:`.

## Acceptance Criteria Mapping

- R1 acceptance: P1-T1..P1-T6 (all four files < 500 lines; new files wired into csproj) and P2-T1..P2-T4
  (CSharpier/analyzer/nullable green; full suite 5061/5061).
- R2 acceptance / AC-5 coverage sub-clause: P2-T4..P2-T8 (HEAD JaCoCo artifact at
  artifacts/csharp/coverage.xml; first-party line >= 85% and branch >= 75%; no regression; AC-5 confirmed).
