# Feature Audit — itemviewer-breadcrumb-lifecycle-defects (Issue #488; also closes #475)

- Timestamp: 2026-08-28T06-44 (UTC)
- Branch: `bug/itemviewer-breadcrumb-lifecycle-defects-488` at `d9ed9eb2`; base `epic/quickfiler-bug-family-integration` (merge base `12465043`, independently re-verified)
- Work mode: `full-bug` (from `issue.md`) — the acceptance-criteria source is `spec.md` only. `user-story.md` does not exist (verified on disk; its presence would be an integrity failure, and its absence is itself a checked criterion).
- Criterion count at HEAD: **54** `- [x]`, **0** `- [ ]` — matching the spec preamble's stated distribution (Process 2, D1 6, D2 5, D3 6, D4 6, D5 4, #475 7, scope 6, file-size/toolchain/coverage/integrity 12).

## Verification method

Every criterion was evaluated against primary evidence: direct diff and source reads at HEAD, this reviewer's own re-execution of the cheap checks (byte-identity diffs, line counts, identifier searches, Cobertura re-aggregation, TRX counter re-parsing), and the committed evidence artifacts for the expensive gates (coverage runs, full rebuilds, full-suite vstest). Line citations in `spec.md` are anchored to the pre-489 tree `988e819b`; per the spec's own instruction, every citation was resolved by member name, and ordinary line drift was not treated as a defect. No criterion was checked off or altered by this reviewer.

## Evaluation

### Process (2/2 PASS)

| Criterion | Verdict | Evidence |
|---|---|---|
| Phase 0 baseline before any production edit | PASS | Commit `b61faddc` (baseline evidence only) precedes every production commit; `evidence/baseline/` records 1192/1192 tests and the baseline Cobertura whose root element this reviewer re-read (`line-rate="0.852607"`). |
| Fail-before evidence for all six units | PASS | Seven committed fail TRX re-parsed: D1 0/1, D2 0/1, D3 1/2 (negative red, positive green — the correct pre-fix shape), D4 0/2, D5 0/1, #475 lazy 0/1, #475 ctor 0/1; failing names are the delivered test names; failure messages match the pre-fix mechanisms. |

### D1 (6/6 PASS)

Dispose lands between the early return and the replacement construction with a fresh `outgoing` pattern variable type-testing the concrete host (source read). The regression test uses two `FormatterServices` environments, a drainable non-inline context, and observes `DropDown.IsDisposed` plus `Close(...) == false` as the criterion specifies (with an additional discriminating `SetTheme`-throws assertion, candidly documented — the fail-before TRX shows the test red on it). `ItemViewerDisposal_OwnsHostAndDetachesBothSurfaces` and `..._RepeatedSameEnvironmentReusesPopupHost` pass unmodified (committed constraining-tests TRX 9/9; `BreadcrumbDropDownIntegrationTests.cs` byte-identical at exactly 500 lines — both re-verified by this reviewer). The 3-arg overload has no equivalent disposal and `BreadcrumbDropDownOpenCoordinator.cs` is byte-identical (empty diff).

### D2 (5/5 PASS)

`_retainedTheme` field assigned in `SetTheme` before both forwards; replay in the newly-adopted branch only, null/whitespace-guarded; `UpdateRequestProviders` branch performs no theme call (diff read). The coordinator-level test queues, sets, drains, asserts exact sequence `"dark"`, with no second thread or timing construct. The high-risk pooled-theme test and both lazy-theme tests pass unmodified — `488-d2-interaction.trx` 3/3 with the survival reasoning recorded in `488-d2-interaction.md`.

### D3 (6/6 PASS)

Reference-equality fail-fast with silent same-reference return (source read); private field assigned on success and nulled in `DisposeBreadcrumbResources`; both regression tests present (negative excludes `ObjectDisposedException`); no re-initialization branch; `BreadcrumbBridgeCoordinator`/`SetBridgeCoordinator`/`UnsubscribeBridge` all byte-identical. The no-production-behavior-change statement and the fail-fast/`SetBridgeCoordinator` coupling both appear in the spec and in `evidence/other/change-description.md` and `evidence/qa-gates/d3-coupling-recorded.md` (direct read), and the coupling contingency is carried into the promoted potential entry.

### D4 (6/6 PASS)

Guard compares by reference, names the operation, escapes on null `UiSyncContext`, and is the first statement of all four entry points (source read). Both proxy tests (ambient-null via `InvokeAmbientNull`; different non-null context) assert the diagnostic and its message. The proxy limitation — proves the guard fires, does not prove the race is absent — is stated in the spec, the change description, and both tests' own remarks (all read); no criterion claims race elimination. Zero sync primitives in the diff (scanned). The three unconfirmed harnesses are confirmed install-before-construct in `evidence/qa-gates/d4-harness-order.md`, which additionally re-derived the whole site set, disclosed the 19-vs-13 discrepancy per C6's escalation rule, and corrected C6's stale reachability premise — adjudicated in the policy audit (ruling 2) as correct conduct.

### D5 (4/4 PASS)

`ObjectDisposedException` on `IsDisposed || Disposing` before any container creation, with the D-15 ordering stated in-source; regression test asserts the throw and a null coordinator; `ItemViewer.Designer.cs` byte-identical (empty diff). The research §3.5 open item is discharged with evidence (`d5-faulted-task-observation.md`): the faulted task is not observed at three of four QFC call sites, the guard was not weakened, and issue **#670** was opened against the 484-owned file — verified OPEN on GitHub with the recorded title, referenced from the spec's dated amendment.

### #475 (7/7 PASS)

`CaptureCurrentOrTests` gone — zero `.cs` hits repository-wide (re-searched); both `CreateForCurrentThreadTests` declarations survive unmodified; both host constructor chains swap to `CaptureCurrent()` with argument order untouched and the null-factory test green; `EnsureBreadcrumbLifecycle` takes a `Func<>` evaluated only after the early return, all three call sites updated; the boundary test replaced in place retaining the controlled-context half; the seam-preservation test recorded red before laziness (`475-lazy-fail.trx`) and green after (`475-pass.trx`); the seam audit enumerates all seventeen injecting files with only the two owned ones modified.

### Scope, ownership, 489 dependency (6/6 PASS)

Changed-source set is exactly the permitted subset (re-derived); every forbidden file byte-identical; no public member changes and both interfaces unmodified; the `.csproj` diff is exactly one line adjacent to the `ItemViewerBreadcrumbDropDownContractTests.cs` entry with the base's eleven sibling entries intact and unreordered (raw hunk read); the three out-of-scope candidates exist as potential entries in the diff with mechanisms and triggers carried forward, and no fix for any appears in the code diff; D489-1 through D489-6 are each recorded CONFIRMED in `dependencies-489-recheck.md` with D489-4 checked explicitly by fixed-string search (and D489-2 corroborated independently by this reviewer: `ItemViewer.Breadcrumb.cs` matches zero `class` elements in both Cobertura documents).

### File size, toolchain, coverage, document integrity (12/12 PASS)

All seven touched files at or under 500 lines (re-measured: 425/497/489/463 production, 419/483/480 test); `BreadcrumbDropDownIntegrationTests.cs` untouched at exactly 500; new tests use MSTest+Moq+FluentAssertions with none of the four banned constructs (the retained `Task.Run` is outside the banned set — policy audit ruling 3); csharpier/analyzer/nullable/vstest gates all EXIT 0 with non-vacuity counters recorded and `/p:Nullable=enable` absent; single consecutive pass with zero restarts and SHA-stable owned files; changed-line coverage not reduced and all three measured files at or above 90 percent (independently re-aggregated: 0.909091 / 0.991342 / 0.992883); repo-wide 85.28 raw and 85.08 testable, both figures recorded with baselines and both risen; the coverage-exemption record for the `ItemViewer.Breadcrumb.cs` fixes exists with zero `[ExcludeFromCodeCoverage]` additions or removals in the diff; `user-story.md` does not exist.

## Check-off integrity

The spec diff against base consists of exactly the 54 checkbox flips plus one nine-line dated prose amendment (the D5 discharge recording #670); no criterion text was added, removed, reworded, or reordered (diff filtered by this reviewer). The executor's own reconciliation honestly recorded an interim 53/54 remediation-required state before the orchestrator's MCP promotion closed the final criterion — supersession recorded in place, not overwritten.

## Acceptance Criteria Status

- Source: `docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/spec.md` (work mode `full-bug`)
- Total AC items: 54
- Checked off (delivered): 54
- Remaining (unchecked): 0
- Items remaining: none

## Overall

**GO.** All 54 acceptance criteria verify against primary evidence at HEAD with zero Blocking findings. The six Non-blocking residuals (TRX host-token sanitization, the committed raw Cobertura pair, the C6 stale-enumeration follow-up, the retained `Task.Run` note, the coordinator's 3-line headroom, and the repository threshold-wording conflict) are recorded in the policy audit and code review and transfer to fan-in/post-merge obligations; none requires a remediation cycle on this branch.
