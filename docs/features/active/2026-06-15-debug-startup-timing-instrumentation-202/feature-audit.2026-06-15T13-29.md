# Feature Audit: debug-startup-timing-instrumentation (Issue #202)

**Audit Date:** 2026-06-15
**Feature Folder:** `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202`
**Base Branch:** `main`
**Head Branch:** `feature/debug-startup-timing-instrumentation-202`
**Work Mode:** `full-feature`
**Audit Type:** Initial acceptance review

---

## Scope and Baseline

- **Base branch:** `main` (commit `a21d09e18dfebb9e3450c1b3322e7715c09d91e6`)
- **Head branch/commit:** `feature/debug-startup-timing-instrumentation-202` (commit `1d193d90dba55eec0a739ff13f5ecb5e3d218b99`)
- **Merge base:** `a21d09e18dfebb9e3450c1b3322e7715c09d91e6`
- **Evidence sources:**
  - Primary: `artifacts/pr_context.summary.txt`
  - Secondary baseline diff: `artifacts/pr_context.appendix.txt`
  - Feature evidence: `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202/evidence/**`
  - Additional evidence: `git diff a21d09e1..1d193d90` (source/test diff) and `TestResults/final-full.cobertura.xml`
- **Feature folder used:** `docs/features/active/2026-06-15-debug-startup-timing-instrumentation-202`
- **Requirements source:** `spec.md` and `user-story.md` (full-feature work mode)
- **Work mode resolution note:** `issue.md` contains `- Work Mode: full-feature`, so the authoritative AC sources are `spec.md` and `user-story.md`. The two files carry identical AC text.
- **Scope note:** Audit scope is the full branch diff vs the merge-base. The PR-context summary reports "Core logic changes: 0 files," which is a misclassification of the C# source changes; scope was therefore taken from the actual git diff (6 changed `.cs` files plus build/settings/docs). No caller scope-narrowing was applied.

---

## Acceptance Criteria Inventory

**Authoritative AC source files for this run:**
- `spec.md` — primary source (full-feature)
- `user-story.md` — primary source (full-feature)

The AC text is identical in both files.

### Acceptance criteria

1. A flag exists that enables or disables startup timing instrumentation; when disabled there is no behavioral or output change to startup.
2. When enabled, each startup sub-component's elapsed wall-clock time is captured during startup.
3. When enabled, a formatted plain-text table of sub-component names and elapsed times (plus a total row) is written to the output screen after startup completes.
4. The timing recorder/formatter is a testable unit (no Outlook/COM dependency) with MSTest coverage meeting the repository floor for new code.
5. Instrumentation uses existing logging/output infrastructure and existing approved dependencies; it does not change functional startup behavior.

---

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|-----------|--------|----------|--------------------------|-------|
| 1 | Flag enables/disables; disabled = no behavioral/output change | PASS | New `StartupTimingEnabled` user setting (default `False`) in `Settings.settings`/`Settings.Designer.cs`. `LoadAsync` selects `NullStartupTimingRecorder` when off (records/emits nothing). Tests `LoadAsync_WhenTimingDisabled_RecordsNothingAndEmitsNoTable` and `LoadAsync_PreservesPhaseOrderingAndYieldCount_WhenTimingOnVersusOff` (identical visited-stage order and yield count = 5 in both modes). | `git diff a21d09e1..1d193d90 -- TaskMaster/AppGlobals/ApplicationGlobals.cs TaskMaster/Properties/Settings.settings` | Default-off and no-output-when-off both verified by test assertions. |
| 2 | Each sub-component elapsed wall-clock time captured when enabled | PASS | `LoadSequentialAsync` wraps each phase await with a shared `Stopwatch`/`StopAndRestart`; LoadBasic measured in `LoadBasicMethod`. Test `LoadAsync_WhenTimingEnabled_RecordsAllPhasesInStartupOrderWithLoadBasicFirst` asserts the seven phases recorded in order: LoadBasic, IntelConfig, OlObjects, ToDo, AutoFile, Engines, Events. | `vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll ...` | Seven established phase seams captured per spec. |
| 3 | Formatted plain-text table (sub-components + total row) written after startup | PASS | `StartupTimingRecorder.FormatTable` renders `Duration`/`Action` columns via `PrettyPrinters.ToFormattedText` plus a summed TOTAL row; `EmitTable` logs once with `[Startup timing]` prefix at end of `LoadAsync`. Test `LoadAsync_WhenTimingEnabled_EmitsExactlyOneTableWithPhaseNamesAndTotal` asserts exactly one emission containing all phase names and TOTAL; recorder test asserts TOTAL equals the sum of injected spans. | `vstest.console.exe ... /EnableCodeCoverage` | "Output screen" is realized as the existing log4net channel (spec/user-story document this as the single channel used by #139/#141). |
| 4 | Recorder/formatter is a testable unit (no Outlook/COM) with MSTest coverage meeting new-code floor | PASS | `IStartupTimingRecorder`/`StartupTimingRecorder`/`NullStartupTimingRecorder` have no Outlook/COM/IO dependency. 7 recorder unit tests inject deterministic spans. New-code line coverage 100% (>= 90% floor): `TaskMaster.StartupTimingRecorder` and `TaskMaster.NullStartupTimingRecorder` both `line-rate="1"`. | `grep -oE '<class line-rate="[0-9.]*"[^>]* name="TaskMaster\.(StartupTimingRecorder\|NullStartupTimingRecorder)"' TestResults/final-full.cobertura.xml` | Meets the >= 90% new-code floor with margin. |
| 5 | Uses existing logging/output + approved deps; does not change functional startup behavior | PASS | Uses existing `ApplicationGlobals` log4net logger (`logger.Info`), `Stopwatch`, and `UtilitiesCS.PrettyPrinters` — all already present/approved; no new dependency. Ordering/yield-count parity test confirms unchanged functional startup behavior. No COM-thread or async-restructuring change. | `git diff a21d09e1..1d193d90 -- TaskMaster/AppGlobals/ApplicationGlobals.cs` | Banned-API rule respected (Stopwatch instead of DateTime.Now/UtcNow). |

---

## Summary

**Overall Feature Readiness:** NEEDS REVISION

All five acceptance criteria are functionally satisfied and verified by tests and coverage evidence. Feature readiness is held at NEEDS REVISION (rather than PASS) because of a policy violation outside the acceptance criteria themselves: the modified test file `TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs` is 687 lines, exceeding the repository 500-line file-size limit (documented in `policy-audit.2026-06-15T13-29.md` and `code-review.2026-06-15T13-29.md`). The acceptance criteria are all PASS; the blocking item is a separable, mechanical refactor (split the test file).

**Criteria summary:**
- **PASS:** 5 criteria
- **PARTIAL:** 0 criteria
- **UNVERIFIED:** 0 criteria
- **FAIL:** 0 criteria

**Top gaps preventing PASS:**

1. Test-file size violation: `ApplicationGlobalsTests.cs` (687 lines) exceeds the 500-line limit. This is a policy gap, not an acceptance-criteria gap.
2. Canonical C# coverage artifact `artifacts/csharp/coverage.xml` is absent (Minor/process; coverage verified from `TestResults/final-full.cobertura.xml`).

**Recommended follow-up verification steps:**

1. Split the startup-timing wiring tests and helpers into a new test file so each file is under 500 lines, then re-run the C# toolchain (format -> analyze -> nullable -> test+coverage).
2. Emit/copy the merged Cobertura output to `artifacts/csharp/coverage.xml` to satisfy the workflow artifact contract.

---

## Acceptance Criteria Check-off

Per the acceptance-criteria tracking rules, all five criteria evaluate PASS and are represented as markdown checkboxes in `spec.md` and `user-story.md`. They were already checked off `[x]` by the implementing agent during execution; this audit confirms each check-off is supported by evidence. No further checkbox change was required (all items already `[x]` and verified PASS).

### AC Status Summary

- Source: `spec.md`, `user-story.md`
- Total AC items: 5 (per file)
- Checked off (delivered): 5 (per file)
- Remaining (unchecked): 0
- Items remaining: None.

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|-------------|----------|----------------|-----------|-------|
| `spec.md` | 5 | 5 | 0 | Checkbox-backed; all already `[x]` and confirmed PASS |
| `user-story.md` | 5 | 5 | 0 | Checkbox-backed; all already `[x]` and confirmed PASS |

No source-file checkbox change was made because all AC items were already checked off and each is supported by inspected evidence.
