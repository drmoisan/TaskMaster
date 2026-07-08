# Policy Compliance Audit — Store Disable Service (F1, Issue #261)

- Timestamp: 2026-07-07T23-46
- Reviewer: feature-reviewer
- Feature branch: `feature/store-disable-service-261` @ HEAD `88366ad4`
- Base (merge-base): `8bd91d1d` on `origin/epic/store-lockup-resilience-integration`
- Diff scope: `git diff 8bd91d1d..HEAD` (full branch-vs-base diff)
- Work mode: `full-feature` (AC sources: `spec.md` §9 + `user-story.md`)

## Executive Summary

The branch delivers the F1 store-disable foundation entirely in C# (production + tests) plus
docs/evidence. Toolchain gates (csharpier, analyzers, nullable/TreatWarningsAsErrors, MSTest with
coverage) are green per the evidence tree. Repository line coverage is 81.08% (independently
confirmed from `coverage/postchange.cobertura.xml` root `line-rate="0.810827"`) and new-code
coverage is >= 90%, satisfying the authoritative CLAUDE.md floor.

One Blocking policy finding: `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs` is 688
lines, exceeding the unconditional 500-line file-size limit; this diff enlarged the file from 563
(baseline) to 688. All other reviewed policy areas are PASS or acceptable-with-documentation.

Overall verdict: **PARTIAL** (one Blocking file-size finding; remediation inputs produced).

## Authority-Order Note (coverage threshold precedence)

`.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` state line >= 85% / branch
>= 75%. `CLAUDE.md` (policy-compliance-order authority position 1) states repo-wide line coverage
>= 80% on the testable denominator and new code >= 90%, with the COM/VSTO/WinForms exemption. Per the
mandatory reading order, CLAUDE.md is authoritative where it conflicts with the `.claude/rules`
summaries. This audit applies the CLAUDE.md 80%/90% line-coverage gate. Under the 80% gate the
feature PASSES; branch coverage is not a CLAUDE.md gate and is not treated as a blocking metric here.

## Rejected Scope Narrowing

None. The caller instruction directed review of the full branch diff against the resolved base and
did not attempt to narrow scope to a plan/task/phase or a file subset. The caller's "decided points"
(coverage floor per CLAUDE.md; net48 `readonly struct` realization) are policy-authority and
platform-constraint clarifications, not scope narrowing. The full feature-vs-base diff was audited.

## Evidence Location Compliance

`validate_evidence_locations.py` was not required: a manual scan of the branch diff shows no files
written under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or
`artifacts/coverage/`. All feature evidence is under the canonical
`docs/features/active/2026-07-07-store-disable-service-261/evidence/<kind>/` tree
(baseline, qa-gates, issue-updates, other). Coverage Cobertura files are written to the repo-standard
`coverage/` directory produced by `scripts/vscode/Invoke-MSTestWithCoverage.ps1`; this is the
repository's canonical coverage output path, not a prohibited `artifacts/coverage/` path. PASS.

## 1. Coverage Verification (mandatory per changed language)

Only C# has changed code files in the branch diff. TypeScript, Python, and PowerShell have zero
changed files (verified: no `.ts/.tsx/.py/.ps1` in the diff), so no coverage verdict is required for
them.

### 1.2.1 C# coverage (changed language — verdict required)

- Coverage source: `coverage/postchange.cobertura.xml` (dotnet-coverage Cobertura merge over all 7
  `*.Test.dll` as CI does), reproduced as `coverage/verify.cobertura.xml`. Clean re-measured baseline:
  `coverage/cleanbaseline.cobertura.xml`. Evidence docs: `evidence/qa-gates/qa-04-test-coverage.md`,
  `evidence/qa-gates/qa-05-coverage-delta.md`.
- Repo-wide line coverage:
  - Baseline: 81.02% (79,345 / 97,933) [clean re-measure]
  - Post-change: 81.08% (79,667 / 98,254); reproduced 81.07%. Independently confirmed: Cobertura root
    `line-rate="0.810827"`.
  - Change: +0.06pp
  - Disposition: PASS against the CLAUDE.md >= 80% testable-denominator floor.
  - Evidence: `coverage/postchange.cobertura.xml`, `evidence/qa-gates/qa-04-test-coverage.md`.
- New/changed-code coverage: StoreIdentity.cs 100.00% (50/50); StoreDisableService.cs 97.92%
  (188/192); DisabledStoreEntry (IStoreDisableService.cs) 100.00% (8/8); StoreFilterAttribution.cs
  (touched) 100.00% (96/96); StoresWrapper.cs (touched) 98.60% (424/430). All >= 90%. PASS.
- No regression on previously-covered lines: PASS (+322 covered lines; all touched files 98.6%-100%;
  pre-existing StoresWrapper/StoreFilterAttribution tests still pass; 4995 -> 5032 tests, 0 failures,
  0 removals).

**C# coverage verdict: PASS.**

Note on baseline anomaly: the Phase-0 raw baseline (`coverage/baseline.cobertura.xml`, 47.16%,
denominator 180,246) was a dotnet-coverage double-count anomaly under Workers=0 parallelism. The
executor re-measured a clean apples-to-apples baseline by git-stashing F1 and re-running; the clean
baseline (81.02%) is the authoritative comparison point. This methodology is documented in
`qa-05-coverage-delta.md` and is coherent with the independently-confirmed post-change root figure.

## 2. General Code Change Policy (`CLAUDE.md`, `.claude/rules/general-code-change.md`)

| Area | Verdict | Evidence |
|---|---|---|
| Simplicity / separation of concerns | PASS | `StoreIdentity` is a pure value type (no COM in the pure overload); `StoreDisableService` is a thin orchestration layer over `StoresWrapper` (single source of truth); filter logic isolated in `StoreFilterAttribution.Decide` (pure). |
| Classes vs functions | PASS | Domain concepts modeled as types (`StoreIdentity`, `DisabledStoreEntry`, `StoreDisableService`); pure static resolver factory; enum for scope. |
| Error handling (fail-fast) | PASS | Writes validate identity and throw `ArgumentException`; null-model writes throw `InvalidOperationException`; reads are safe-empty. Narrow COM try/catch only around the guarded FilePath read (mirrors existing filter guard); no broad swallowing in service logic. |
| Naming / docs | PASS | Descriptive names; XML docs on all public members with contracts and "why" comments (issue #261 references). |
| Module cohesion | PASS | New types placed in cohesive `OutlookObjects/Store` and `Interfaces/IGlobals` locations. |
| File-size limit (500 lines) | **FAIL** | `StoresWrapperTests.cs` = 688 lines (see §5). |
| Dependencies | PASS | No new external dependencies; reuses `SmartSerializable`, `StoresWrapper`, existing timer seam. |
| I/O boundaries | PASS | Pure resolver performs no COM/I-O; COM overload confined to filter call sites; persistence via existing debounced `Model.Serialize()`. |
| Public API compatibility | PASS (with note) | Two public signatures changed: `StoreFilterAttribution.Decide` (+`isDisabled`) and static `StoresWrapper.StoreIsIncluded` (+`isDisabled`). The change is additive-trailing and was called out in spec §6. Verified no non-test caller of `StoreIsIncluded` exists (grep: only definition + test caller); `Decide`'s single production caller is updated in-repo. Acceptable per §7 (breaking change called out, all in-repo callers updated). Non-blocking. |

## 3. General Unit Test Policy (`.claude/rules/general-unit-test.md`, CLAUDE.md UT/CUT)

| Area | Verdict | Evidence |
|---|---|---|
| Independence / isolation | PASS | Each test constructs its own model + service via `CreateModel`/`CreateService`; no shared mutable state. |
| Determinism | PASS | No `Thread.Sleep`/`Task.Delay`/real timers/`Date.now`. Serialization observed via the `ManualFireTimerWrapper` never-fired timer seam (`StartCount`), not wall-clock waits. |
| No temp files | PASS | Round-trip uses `SerializeToString()`/`DeserializeObject(json, settings)`; no filesystem writes. Config.Disk.FilePath is a string only; the manual timer is never fired so no write reaches disk. |
| No external deps / live Outlook | PASS | `Mock<IOlObjects>`, `Mock<IApplicationGlobals>`, `Mock<Outlook.Store>`, `Mock<IStoreRehookService>`; no live COM. |
| AAA + clear assertions | PASS | Arrange/Act/Assert structure; FluentAssertions with reason strings. |
| Scenario completeness | PASS | Positive/negative/idempotency/edge (case-insensitive, both-scope dedup, sentinel/default identity, null model) covered across the three surfaces + serialization. |
| Test file location | PASS | Tests live in `UtilitiesCS.Test/OutlookObjects/Store/` mirroring production; no colocation. |
| Coverage exclusions | PASS | No production `src` path excluded; `StoreFilterAttribution` intentionally coverage-tracked (not `[ExcludeFromCodeCoverage]`). |
| Effective assertion of async throw | PARTIAL (Non-blocking) | `ReenableAsync` exception cases use `.Should().ThrowAsync<...>()` without `await` (StoreDisableServiceTests.cs lines 226-229, 261-263), so those specific async assertions do not execute. See code-review. Behavior is still correct (shared `ValidateIdentity` runs first) and is exercised by the two synchronous write methods. |

## 4. C# Code Change / Unit Test Policy (CLAUDE.md C#*, CUT*)

| Area | Verdict | Evidence |
|---|---|---|
| net48 `readonly struct` (no `record struct`/`init`) | PASS | `StoreIdentity` and `DisabledStoreEntry` are plain `public readonly struct` with ordinary ctor + get-only props, matching the documented CS0518/`IsExternalInit` constraint and the `ResourceTimingRow` precedent. Required realization, not a defect. |
| Formatting (csharpier) | PASS (Advisory on command form) | `qa-01-format.md`: `csharpier check .` reports 1283 files checked, 0 needing formatting, idempotent, EXIT 0. The v1 `format`/`check` subcommands were used instead of the CLAUDE.md-listed bare `csharpier .` (v0) form; the pinned tool is 1.2.6 (v1) where the subcommands are the correct equivalent. Advisory only — CLAUDE.md command text predates the tool version; result is clean. |
| Analyzers | PASS | `qa-02-analyzers.md`: build succeeded, 0 errors, 70 warnings (down from 72 baseline); all 70 pre-existing test-project warnings (CS8632/CS0067). No new diagnostic from any scope-lock file. |
| Nullable / TreatWarningsAsErrors | PASS | `qa-03-nullable.md`: 0 warnings, 0 errors. |
| Framework/libraries | PASS | MSTest `[TestClass]`/`[TestMethod]`/`[DataRow]`, Moq, FluentAssertions throughout. |

## 5. File-Size Limit Finding (Blocking)

- Rule: CLAUDE.md §4.1 "Do not exceed 500 lines for any one file"; `.claude/rules/general-code-change.md`
  "No production code, test code, or reusable script file may exceed 500 lines" (exceptions: throwaway
  scripts, raw text fixtures, Markdown — none apply to a `.cs` test file).
- File: `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs` — 688 lines (independently
  confirmed via `wc -l`).
- Baseline: 563 lines at `8bd91d1d` (already over-limit pre-existing). This diff added ~125 lines
  (P7-T4 disabled-store filter/serialization tests + one call-site argument update), enlarging an
  already-non-compliant file.
- Severity: **Blocking**. The limit is a hard, unconditional rule and this diff demonstrably worsens
  compliance. This drives feature-audit AC15 to PARTIAL (its "all touched files remain under 500
  lines" clause is not met).
- Mitigating context (documented for an informed exception decision): the file was already over-limit
  at baseline independent of this feature; `evidence/other/file-size-confirmation.md` reports the limit
  is not enforced by any CI/hook gate and dozens of sibling test files range 600-1824 lines; the plan
  (P7-T4) explicitly directed extending this file. All NEW files added by the feature comply
  (max 405 lines).
- In-scope remediation (narrow): extract the ~125 newly-added disabled-store tests into a new file
  (e.g., `StoresWrapperDisableTests.cs` or a `partial` companion), bringing the feature's added lines
  out of the over-limit file. Remediating the pre-existing 563-line baseline is repo-wide debt not
  attributable to F1 and is out of scope for this feature.

## 6. Documented Deviations — Policy Dispositions

1. **Interface member forces 7 test-double implementers** — Acceptable / PASS. Adding `StoreDisable`
   to `IApplicationGlobals` is mandated by spec §4.4. A hand-written implementer of a C# interface must
   implement every member or the solution fails to compile (every QA gate requires a green build).
   The 7 changes are minimal one-liners (`=> null;` / `=> throw new NotSupportedException();`) matching
   each file's existing member style; none exercise `StoreDisable`. Mechanically necessary consequence,
   not scope creep. Documented in `evidence/other/scope-budget-confirmation.md`.
2. **StoresWrapperTests.cs 688 lines** — Blocking (see §5).
3. **`Resolve(displayName, filePath)` instead of `Resolve(store)`** — Acceptable / PASS. The third
   filter surface uses the pure overload with already-read `store.DisplayName` + the FilePath already
   read earlier in the method, rather than the COM overload which would re-read FilePath. This avoids a
   second blocking COM read (the exact call the epic prohibits) and is functionally equivalent. Sound
   deviation aligned with the epic's no-blocking-COM constraint.
4. **CSharpier v1 `check`/`format` subcommands** — Advisory (see §4). Correct equivalent for the pinned
   1.2.6 tool; formatting verified clean and idempotent.

## Verdict Summary

- Blocking findings in this artifact: 1 (StoresWrapperTests.cs > 500 lines).
- Non-blocking: unawaited async throw assertions (test-quality); public-signature changes (contained).
- Advisory: CSharpier v1 command form.
- Overall policy verdict: PARTIAL. Remediation inputs produced.
