# Code Review — quickfiler-session-metrics-twelve-hour-time-format-645

- Timestamp: 2026-09-03T12-00
- Scope: full branch diff, `origin/main` merge-base `5ebaaf105d8241f309f704d1ff90af2e32e5a6c1` to
  `b3bb8fef` (HEAD).

## Production Change Review

### `QuickFiler/Controllers/QfcHomeController.Metrics.cs`

- Line 48: `dataLineBeg = $"{now:MM/dd/yyyy},{now:HH:mm},";` (was `{now:hh:mm}`).
- Line 127: `curTimeText = now.ToString("HH:mm");` (was `now.ToString("hh:mm")`).
- Both changes are single-token format-specifier substitutions inside pre-existing, otherwise
  unmodified statements. No new branches, no new allocations, no signature or contract changes.
  `now` is sourced from `TimeProvider.GetLocalNow().LocalDateTime` in both methods (an existing
  injectable seam, untouched by this change), so the fix is deterministic and testable exactly as
  before.
- A commented-out dead-code line at line 46
  (`//var curTimeText = DateTime.Now.ToString("hh:mm");`) still shows the old 12-hour literal. This
  is intentionally excluded from scope per spec.md ("commented-out dead code, not a live site"),
  and correctly left untouched. Note for future cleanup: this comment is now inconsistent with the
  live code beneath it and could mislead a future reader skimming the file; not a blocking issue
  for this bug fix, since spec.md explicitly excludes it and removing dead comments is unrelated
  scope creep.

### `QuickFiler/Controllers/EfcHomeController.Metrics.cs`

- Line 96: `var curTimeText = currentDateTime.ToString("HH:mm");` (was `"hh:mm"`). Single-token
  change, no other logic touched. `currentDateTime` is a pre-existing parameter of
  `BuildQuickFileMetricLines`, unaffected by this change.

### Consistency with adjacent code

- Both files' adjacent `SentDate`/`curDateText` fields already use `"HH:mm:ss"`/`"MM/dd/yyyy"`
  respectively; the fix makes the time-of-day field internally consistent with the row's existing
  24-hour convention, exactly as spec.md's rationale states. No `CultureInfo.InvariantCulture` was
  added to any of the three touched calls, matching the explicit scope exclusion (tracked
  separately as issue #742); the numeric fields' pre-existing `CultureInfo.InvariantCulture` calls
  in the same methods were left untouched.

## Test Change Review

### `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`

- `expectedDataLineBeg` in both `WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps` and
  `QuickFileMetrics_WRITE_UsesInjectedClock_ForDataLine` now builds its expected time literal via
  `expectedLocal.ToString("HH:mm")`, where `expectedLocal` is derived from the same
  `FakeTimeProvider` instance the controller consumes (`fake.GetLocalNow().LocalDateTime`). This is
  the correct pattern: the expectation is computed the same way the production code computes it,
  so the test continues to validate the seam (clock injection) rather than pinning an
  independently-typed literal. `FixedClock()` fixes the instant to 14:30:45, which is squarely in
  the 12-hour/24-hour ambiguous zone (`hh:mm` would render `02:30`); the updated test now
  meaningfully discriminates the two formats, whereas an equivalent literal in the AM would not
  have caught this class of regression.
- The two touched XML doc comments (lines 227/265, in the delegation prompt's line numbering) were
  updated from `"hh:mm"` to `"HH:mm"` to describe the corrected format. Comment-only change, no
  functional impact; improves comment accuracy per General Code Change Policy §5 ("comment why,
  not what," and keeping comments synchronized with behavior).

### `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs`

- The fixed asserted literal at the single assertion in
  `BuildQuickFileMetricLines_WithMovedMailItems_FormatsMetricLine` changed from `01:05` to `13:05`.
  Independently verified against the fixture: `MetricsNow = new DateTime(2026, 7, 4, 13, 5, 0)`
  (line 25), which under `"HH:mm"` renders `13:05`. The test continues to pass the fixture's fixed
  `DateTime` directly into the production method under test (`BuildQuickFileMetricLines`), so no
  clock-injection mechanism was altered — only the asserted string literal.

## Design and Style Observations

- Both production sites use the format specifier directly in a string-interpolation expression
  (`$"{now:HH:mm}"`) or via `.ToString("HH:mm")`. This is consistent with each site's pre-existing
  style; the fix did not introduce a new style pattern.
- No new public API surface, no new class, no new dependency. The change is the minimal edit that
  satisfies the defect, consistent with the Bugfix Workflow in CLAUDE.md ("change only what is
  needed to make the failing test pass").
- Diff size: 4 files changed, 8 lines changed total in production+test code (2 production lines +
  1 line each ×2 doc comments + 2 expectedDataLineBeg lines + 1 fixed literal = matches spec.md's
  enumerated scope exactly; independently confirmed via `git diff` line counts above).

## Toolchain Evidence Quality

- CSharpier, analyzer-rebuild, and nullable-rebuild evidence artifacts each record
  `Timestamp`/`Command`/`EXIT_CODE`/`Output Summary`, plus corroborating signals beyond the bare
  exit code (SHA-256 before/after hashes for the format pass; `CoreCompile:` entry counts and
  assembly `LastWriteTimeUtc` deltas for both rebuilds), which is stronger evidence than a bare
  exit-code claim and directly rules out an incremental-build false pass (a known trap for `/t:Build`
  documented in CLAUDE.md C#1 — this branch correctly used `/t:Rebuild` throughout).
- The coverage-threshold script's non-zero exit code (`EXIT_CODE: 1`) is recorded honestly rather
  than omitted or misreported, with the underlying vstest pass/fail counts quoted separately from
  the threshold-assertion failure. This is good evidence discipline: a less careful executor could
  have reported only the vstest summary and hidden the non-zero process exit code.

## Findings Requiring Attention

1. **Blocking (evidence hygiene, not production code):** both committed Cobertura evidence files
   embed the operator's account name and absolute worktree path 2,007 times each (4,014 total).
   See `policy-audit.2026-09-03T12-00.md` § "Evidence Hygiene — Host Path Leak" and
   `remediation-inputs.2026-09-03T12-00.md` for detail and remediation guidance. This does not
   affect the production fix's correctness.
2. **Non-blocking, informational:** the dead-code comment at
   `QfcHomeController.Metrics.cs:46` retains the pre-fix `"hh:mm"` literal in a commented-out line
   and is now inconsistent with the live code below it. Correctly out of scope per spec.md; no
   action required in this PR.

## Verdict

Production and test code changes are correct, minimal, well-tested, and policy-compliant. The
sole blocking finding is a content-hygiene defect in the branch's own committed coverage evidence
artifacts (host path leak), not in the production or test code.
