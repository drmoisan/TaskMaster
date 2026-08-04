# P5-T208 — Dead-code removal scope and anti-masking ledger

Timestamp: 2026-07-22T19-31Z

Command: `git diff --stat -- QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs; git diff -- QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs; for f in coverage.config scripts/vscode/TaskMaster.cli.runsettings; do sha256sum "$f"; done; grep -n "BreadcrumbDropDownOpenLifetime\|BreadcrumbPopupBoundaryCoverageTests" QuickFiler/QuickFiler.csproj QuickFiler.Test/QuickFiler.Test.csproj`

EXIT_CODE: 0

## Baseline reference

P5-T202 pre-edit baselines (`p5-deadcode-removal-authorization.2026-07-22T19-14.md`):
- `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs` SHA-256 `e53de9be…c53f1`, 437 lines.
- `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs` SHA-256 `594d96f2…63db`, 480 lines.
- `coverage.config` `b9cd8035…0943`; `TaskMaster.cli.runsettings` `98ef03a8…ef57`.

## Production scope — exactly one production C# file changed

- **`QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs`** changed. Post-correction SHA-256
  `b8f4d6e27dc0fee69f2a659c7830f212bb0410f3283c1db8800bd005a9756cb0`, **425 lines** (at most 500 —
  satisfied). No other production C# file changed; every other production SHA-256 captured by P5-T202 is
  unchanged.
- **Behavioral change (sole):** the inner `try`/`catch (Exception recoveryFailure)` block at former
  lines 149-156 of `CompleteOpenAsync` was removed, so the outer `catch (Exception exception)` body is
  now exactly `await HandleOpenFailureAsync(exception, lease).ConfigureAwait(false);` with no nested
  `try`/`catch`. This is the diff hunk at lines 144-148.
- **Formatting-only normalizations (no behavioral effect):** the authorized P5-T204 `csharpier format`
  (mutating) gate produced a csharpier-clean file. Against HEAD the working-tree diff additionally shows
  two whitespace-only canonicalizations: the `Schedule` ternary
  `IsLifecycleCurrent(lease, allowDisposed: false) ? operation() : Task.CompletedTask` collapsed to one
  line (hunk at line 99), and the `IsLifecycleCurrent(BreadcrumbDropDownOpenLease lease, bool
  allowDisposed)` signature collapsed to one line (hunk at line 349). Neither changes any member's
  behavior, signature semantics, or control flow; both are pure csharpier layout. No member was added,
  removed, renamed, or had its behavior altered.
- No `[ExcludeFromCodeCoverage]`, no coverage exclusion, no rethrow added to `Report` or
  `HandleOpenFailureAsync`; the resolution is removal of the dead code only.

## Test scope — exactly one test file changed, comment-only

- **`QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs`** changed. Post-correction
  SHA-256 `88b42147682e935a01cf24bd7cb842410295234b26b3c0c6d0d09e247cdb0c23`, **480 lines** (at most 480 —
  satisfied). The full `git diff` shows the change is confined to the two-line `<summary>` doc comment on
  `OpenAsync_RecoveryDispatchFails_ReportsOnceAndClearsStoredOpenTask`, restating that the recovery
  dispatch failure is reported by `HandleOpenFailureAsync`'s internal `catch`. The four assertions
  (`RanToCompletion`, `Result` false, `Errors` equal to `[kickoffFailure, recoveryFailure]`,
  `StoredOpenTask` null) are outside the diff and therefore byte-identical. No assertion was added,
  removed, weakened, relaxed, reordered, or made conditional.
- Case count unchanged: the 17-class composition remains 170 (P5-T207 confirmed
  `BreadcrumbPopupBoundaryCoverageTests` at exactly 23 discovered and passed; the production-only removal
  adds and removes no case).

## Protected invariants (hash-identical before and after)

| Artifact | Baseline | Current | Identical |
|---|---|---|---|
| `coverage.config` | `b9cd8035…0943` | `b9cd80356c6bdbe03807a0b8cb106ae03d24efbdbb2515097fbf003099050943` | yes |
| `scripts/vscode/TaskMaster.cli.runsettings` | `98ef03a8…ef57` | `98ef03a8d3b0ebb2ed7a765e3b5e1b58e774d20202df2f294c03a7260b9cef57` | yes |
| `QuickFiler.csproj` OpenLifetime include | line 393 | line 393 `<Compile Include="Viewers\BreadcrumbDropDownOpenLifetime.cs" />` | yes |
| `QuickFiler.Test.csproj` PopupBoundary includes | lines 81-82 | lines 81-82 unchanged | yes |
| 17-class filter string | P5-T202 recorded | not referenced/edited by this batch | yes |

No package, threshold, or coverage/test exclusion was added, widened, or moved.

## Behavior preservation on the reachable path

- The outer `catch (Exception exception)` still swallows the original open/initialization failure (it
  awaits `HandleOpenFailureAsync` and does not rethrow).
- The `finally` still settles `completion` with `result` and clears `_openTask` (unchanged).
- `LastInitializationException` handling and rollback semantics are unchanged (they live in
  `HandleOpenFailureAsync`, untouched).
- The recovery-dispatch-failure report is still emitted exactly once by `HandleOpenFailureAsync`'s
  internal `catch` at its `Report` call, not by the removed block — proven green by P5-T207
  (`OpenAsync_RecoveryDispatchFails_ReportsOnceAndClearsStoredOpenTask` passed with `Errors ==
  [kickoffFailure, recoveryFailure]` intact).

## Anti-masking justification

This removal is dead-code removal per the P5-T202 unreachability proof and is expressly NOT a masking
action: no assertion was weakened, deleted, relaxed, or made conditional; no `Thread.Sleep`,
`Task.Delay`, wall-clock wait, retry loop, or timing threshold was added; no `[DoNotParallelize]`,
`[Ignore]`, or category-based skip was added; no test filter was narrowed; and no coverage or test
exclusion, threshold, or `coverage.config` value was added or changed. The test that exercises the
recovery path stayed green with all four assertions intact, which is the affirmative proof that the
removed lines were unreachable rather than load-bearing.

## Output Summary

Exactly one production C# file (`BreadcrumbDropDownOpenLifetime.cs`, 425 lines) changed; its sole
behavioral change is removal of the unreachable inner `try`/`catch` so `HandleOpenFailureAsync` is
awaited directly in the outer `catch`, plus two behavior-neutral csharpier formatting normalizations
from the authorized P5-T204 gate. Exactly one test file changed, comment-only, with all four assertions
byte-identical and the case count still 170. `coverage.config`, runsettings, csproj includes, filter,
packages, thresholds, and exclusions are hash-identical/unchanged. Behavior is preserved on the
reachable path and the change is not a masking action. No contradiction; P5-T209 is authorized.
