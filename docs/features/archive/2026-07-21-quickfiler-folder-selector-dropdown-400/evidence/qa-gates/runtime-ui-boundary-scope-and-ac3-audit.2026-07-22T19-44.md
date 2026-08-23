# P5-T212 — Runtime UI-boundary scope and AC-3 audit (post-dead-code-removal)

Timestamp: 2026-07-22T19-44Z

Command: `read-only reconciliation of passing P5-T102 through P5-T211 evidence under evidence/qa-gates and evidence/regression-testing; grep of AC-3/AC-18 checkbox state in spec.md; grep of P5 task checkbox state in the remediation plan`

EXIT_CODE: 0

## Scope

This audit uses only passing P5-T102 through P5-T211 evidence. It preserves all prior behavioral,
disposal, and thread-affinity proof and reconciles the P5 correction history into a single authoritative
closure statement. AC-3 and AC-18 are kept unchecked (their final verification is the P9 full-repository
obligation).

## Reconciled evidence chain (all passing)

- **ItemViewer extraction and J2 public exception-boundary / source-qualified-identity correction:** the
  changed host-neutral open/close/selector orchestration was extracted from `ItemViewer.Breadcrumb.cs`
  into `BreadcrumbDropDownOpenCoordinator`; `ItemViewer.Breadcrumb.cs` retains one-line delegation.
- **P5-T120 -> P5-T130 replacement and the 37/37 preservation pass** (`p5-open-coordinator-pass-after`,
  `p5-open-coordinator-preservation-reconciliation`): 37/37 with per-class counts 5+10+8+4+10, corrected
  constructor `ArgumentNullException.ParamName == "surfaceFactory"` and native-close observations
  preserved.
- **Line-limit split preservation:** the coordinator and popup-boundary test classes were split into
  partial-class pairs, each `.Part2.cs` kept at most 480 lines.
- **P5-T172 UI-dispatch root-cause determination and P5-T184 anti-masking closure:** the single
  instrumented failure in
  `BreadcrumbUiThreadDispatchTests.SetSuggestionsAsync_WorkerProviderCompletion_SchedulesPostOnOwningContext`
  was root-caused read-only with its selected branch, and the fix was proven not test-only-masking; the
  superseded 160/160 P5-T183 composition is retained as historical.
- **Zero-production-file ten-case branch-coverage correction (batches N1/N2):** proven by
  `p5-coordinator-branch-coverage-ledger` (P5-T193) and
  `p5-branch-coverage-scope-and-anti-masking-ledger` (P5-T200) — exactly two test files changed
  (`BreadcrumbDropDownOpenCoordinatorTests.Part2.cs`, `BreadcrumbPopupBoundaryCoverageTests.Part2.cs`),
  zero production files changed, ten cases added (five per batch) mapped one-per-unit, no assertion
  weakened, no masking primitive introduced.
- **Single-production-file unreachable-dead-code removal** at `BreadcrumbDropDownOpenLifetime.cs`
  former lines 153-156, proven behavior-preserving by the P5-T208 ledger
  (`p5-deadcode-removal-scope-and-anti-masking-ledger.2026-07-22T19-31.md`): the outer `catch` still
  swallows the original failure, the `finally` still settles completion and clears `_openTask`, and the
  recovery-dispatch failure is still reported once by `HandleOpenFailureAsync`'s internal `catch`; the
  affected test `OpenAsync_RecoveryDispatchFails_ReportsOnceAndClearsStoredOpenTask` stayed green with all
  four assertions intact (P5-T207).
- **Authoritative 170/170 P5-T209 numeric composition** (`coverage-p5-deadcode-removal.2026-07-22T19-32.cobertura.xml`,
  `p5-deadcode-removal-composition.2026-07-22T19-32.md`) supersedes the pre-dead-code-removal 170/170
  P5-T201 composition (which still recorded `<CompleteOpenAsync>d__16` at 24/28).
- **P5-T210 nine-unit closure** (`p5-branch-coverage-nine-unit-closure.2026-07-22T19-39.md`): all nine
  P5-T185 units at or above 90%, seven never-regress units unchanged.
- **P5-T211 authoritative numeric decision** (`p5-authoritative-focused-coverage-decision.2026-07-22T19-42.md`):
  PASS, with **ITEMVIEWER OMISSION: CLEARED** proven by the coordinator's per-member numeric coverage
  (all members at 100%; primary type 150/151 = 99.34%) and the extraction diff.
- **File/include/exclusion scope:** no `QuickFiler.csproj`/`QuickFiler.Test.csproj` include change, no
  package, no `coverage.config`/runsettings/threshold change, no coverage or test exclusion; the 17-class
  filter string is byte-identical across P5-T171/P5-T183/P5-T201/P5-T209.
- **P2 30/30** duplicate/probability run remains authoritative for its 30 passing tests.
- **Historical/superseded evidence** retained but not cited as passing: the 07-59/08-44 popup composition
  artifacts, the nonpassing historical P5-T100 numeric decision, the non-authoritative 159/160
  `2026-07-22T14-46`/`14-44` artifacts, and the below-threshold `2026-07-22T16-29`/`16-22` numeric
  artifacts.
- **P9 full-repository obligation:** final repository line coverage (>=80%), new/changed selector
  type/member coverage (>=90%), and changed-line no-regression remain mandatory after P6/P7 and are
  verified at P9-T4/P9-T6, not by this focused P5 gate.

## AC-3 and AC-18 status

- **AC-3** (`spec.md` line 241) is kept **unchecked**. The host-owned, non-topmost popup ownership
  behavior is exercised by the passing P5 tests, but AC-3's full verification is deferred to the final
  audit/P9 repository pass.
- **AC-18** (`spec.md` line 256) is kept **unchecked**. AC-18 requires one final uninterrupted
  full-repository toolchain pass with repository-wide >=80% line coverage and numeric
  baseline/post-change/delta evidence, which is the P9-T4/P9-T6 obligation; the focused P5 gate cannot
  satisfy it.

## Replacement mappings applied (plan checklist)

Applied only after every replacement passed (P5-T209 170/170, P5-T210 nine-unit closure, P5-T211 PASS):

- P5-T73 -> T168, T74 -> T169, T75 -> T170, T76 -> T171, T77 -> T201, T78 -> T211,
  T87 -> T201, T88 -> T211, T89 -> T212, T99 -> T201, T100 -> T211, T101 -> T212.
- Marked completed by replacement evidence: **P5-T73 through P5-T78, P5-T87 through P5-T89, and P5-T101.**
- Preserved historical checkmarks: **P5-T99 and P5-T100** (unchanged `[x]`).
- Checked from passing T209 through T212 evidence: **P5-T67 and P5-T68**.

## Output Summary

The full P5 correction history reconciles into a single passing closure: the ItemViewer extraction, the
P5-T120 -> P5-T130 replacement with 37/37 preservation, the line-limit splits, the P5-T172/P5-T184
UI-dispatch root-cause and anti-masking closure, the zero-production ten-case branch-coverage correction,
and the single-production-file unreachable-dead-code removal all hold; the authoritative 170/170 P5-T209
composition, the P5-T210 nine-unit closure, and the P5-T211 PASS decision with ITEMVIEWER OMISSION:
CLEARED are consistent and non-contradictory. AC-3 and AC-18 are kept unchecked pending the P9
full-repository obligation. The replacement mappings are applied: P5-T73 through P5-T78, P5-T87 through
P5-T89, and P5-T101 are marked completed by replacement evidence; P5-T99/P5-T100 historical checkmarks are
preserved; and P5-T67/P5-T68 are checked from passing T209 through T212 evidence. EXIT_CODE: 0.
