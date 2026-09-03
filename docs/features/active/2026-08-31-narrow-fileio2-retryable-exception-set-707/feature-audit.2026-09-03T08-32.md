# Feature Audit — narrow-fileio2-retryable-exception-set (Issue #707)

- Reviewed: 2026-09-03T08-32
- Work mode: `full-bug` -> AC source: `spec.md` `## Acceptance Criteria` only (9 items, AC1-AC9)
- Diff scope: `67c2e3b0eca90a52e9aee82ccd100acce4722169..HEAD -- ":(exclude).claude"`

## Acceptance Criteria Evaluation

| # | Criterion (summarized) | Verdict | Evidence |
|---|---|---|---|
| AC1 | `WriteTextFileAsync` catches `DirectoryNotFoundException` ahead of `catch (IOException ex)`; returns `false` after exactly 1 factory invocation, 0 delay invocations (was up to 100/99) | **PASS** | Direct read of compiled method: `catch (DirectoryNotFoundException ex)` line 126 precedes `catch (IOException ex)` line 134. `evidence/regression-testing/p2-t3-missingdirectory-fail-before.md` (pre-fix: 100 factory calls) + `p4-t2-fileio2-tests-postfix.md` (post-fix: test passes, asserts factory=1/delay=0) independently corroborate. |
| AC2 | New catch block logs via `logger.Error` before returning `false`, without incrementing `attempts` or calling `delayAsync` | **PASS** | Direct diff read: block body is exactly `logger.Error(...)` then `return false;`; no `Interlocked.Increment`/`delayAsync` reference in the 8-line hunk. `evidence/regression-testing/p3-t1-minimal-fix.md` corroborates whole-file token counts unchanged (1 each for `Interlocked.Increment`/`delayAsync`). |
| AC3 | New regression test asserts `false`/`1`/`0` and fails against pre-fix source | **PASS** | Direct read of `FileIO2_Tests.cs`: three FluentAssertions calls match. RED-first proven: `p2-t3-missingdirectory-fail-before.md` (fails pre-fix, exit 1, first assertion violated at 100 vs expected 1); GREEN post-fix in `p4-t2-fileio2-tests-postfix.md`. |
| AC4 | All pre-existing `FileIO2_Tests.cs` tests still pass unmodified | **PASS** | `p4-t2-fileio2-tests-postfix.md`: 12/12 passed (11 pre-existing + 1 new), 0 failed. `git diff` for the test file shows only an addition (38 new lines), no modification to any existing test body. |
| AC5 | `UnauthorizedAccessException` behavior unchanged, no new handling, no test regression | **PASS** | `p6-t5-ac5.md`: 0 occurrences of `UnauthorizedAccessException` in either changed file (confirmed: it derives from `SystemException`, not `IOException`, so it was never in the retry set and this fix does not touch it). |
| AC6 | General `catch (IOException ex)` retry-exhaustion path (100 attempts, 100ms delay) unchanged for non-`DirectoryNotFoundException` cases | **PASS** | Direct diff read: the existing `catch (IOException ex)` block body is untouched (diff hunk only inserts a new preceding block). `p6-t6-ac6.md` + `p4-t2-fileio2-tests-postfix.md`: `WriteTextFileAsync_WhenEveryOpenAttemptFails_ShouldReturnFalseAfterBudget` and `WriteTextFileAsync_WhenTransientOpenFailureThenSucceeds_ShouldReturnTrueAndWriteAllLines` both pass unmodified. |
| AC7 | `PathTooLongException` not handled by this fix; no test asserts on it | **PASS** | `p6-t7-pathtoolongexception-absence.md`: 0 occurrences in both changed files. Confirmed by direct grep against the diff. |
| AC8 | Neither production caller (`AppOlObjects.cs`, `QfcHomeController.Metrics.cs`) requires a code change | **PASS** | `p6-t8-ac8-caller-scope.md`'s 360-path diff (computed against a stale base) is a confirmed superset of the correct 50-path scope (`git merge-base --is-ancestor 687f15fb 67c2e3b0` = 0), so its negative-match result is valid; independently re-confirmed by this review's own scoped 50-file diff, which contains neither caller path. |
| AC9 | Full C# toolchain passes clean in a single pass, including `vstest.console.exe` against `UtilitiesCS.Test` "with all tests green" | **PASS with disclosed deviation** | Format/analyzer/nullable all clean (0/0). The literal AC9 text ("all tests green") is not met by a strict reading: the full-suite run has 17 pre-existing failures in both the baseline (`p0-t20-baseline-failure-set.md`) and post-change (`p5-t5-utilitiescs-coverage.md`) runs. This review independently cross-checked the two 17-name failure lists and confirms they are **identical sets**, all `Deedle`/F#-reflection `VerificationException` failures unrelated to `FileIO2.cs`/`FileIO2_Tests.cs`, and pre-existing on `main` before this branch (visible already in the P0 baseline run, captured before any change was made). The executor's own `p6-t9-ac9.md` transparently discloses this literal-text gap rather than silently checking the box, and grounds the check-off in the plan's narrower, still-genuinely-satisfied task-level acceptance text (full `FileIO2_Tests` suite green, 12/12). This reviewer accepts the same disposition: a known, disclosed, identical-before-and-after, out-of-footprint test-infrastructure defect is not attributable to this change, consistent with this repository's established precedent for treating pre-existing unrelated failures as non-blocking when the failure set is proven identical across baseline and post-change runs. |

**All 9 AC boxes in `spec.md` are already checked `[x]` and this review's independent evidence corroborates 8 as unconditional PASS and 1 (AC9) as PASS-with-disclosed-deviation. No AC is left unchecked or requires un-checking.**

### Acceptance Criteria Status
- Source: `docs/features/active/2026-08-31-narrow-fileio2-retryable-exception-set-707/spec.md`
- Total AC items: 9
- Checked off (delivered): 9
- Remaining (unchecked): 0
- Items remaining: none

## Baseline vs. Post-Change Failure-Set Cross-Check (independent verification)

Compared `evidence/baseline/p0-t20-baseline-failure-set.md` (17 names) against `evidence/qa-gates/p5-t5-utilitiescs-coverage.md`'s post-change failed-name list (17 names): both name-for-name identical (`DeedleDoodles`, `GetColumnEid_WithStringValues_ReturnsOrdinalSeries`, `GetEmailDataFromTable_OneRow_ReturnsFrameWithExpectedFields`, `FromArray2D_EmptyData_ReturnsFrameWithColumnsButNoRows`, `GetEmailDataInView_WithInjectedEtlResult_ReturnsPopulatedFrame`, `FromArray2D_EmailLikeArray_ReturnsExpectedRowCountAndColumnLayout`, `Email2dArrayToDf_ViaReflection_ValidData_ReturnsFrame`, `GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform`, `FromDefaultFolder_EmptyStores_ReturnsEmptyFrame`, `FromDefaultFolder_StoresWithOneStoreThatHasNoData_ReturnsEmptyFrame`, `PrintToLog_WithPopulatedFrame_LogsWithoutThrowing`, `DropFirstN_DropsFirstNRows`, `Exclude_EmptyOtherFrame_ReturnsSameRowCount`, `Exclude_NonEmptyOtherFrame_RemovesMatchingRows`, `GetDuplicateEntriesByColumn_ReturnsDuplicateValues`, `FromDefaultFolder_Store_WithInjectedEtlResult_ReturnsPopulatedFrame`, `FromDefaultFolder_Stores_FirstStoreHasData_ReturnsNonEmptyFrame`). Root cause shared across all 17: `System.Security.VerificationException: Operation could destabilize the runtime` from `Deedle.Reflection`'s F# type initializer under `dotnet-coverage` IL instrumentation — a documented dotnet-coverage/Deedle incompatibility, orthogonal to `FileIO2.cs`. No `FileIO2`-named test appears in either failure list. **Confirms the 17 failures are genuinely pre-existing and unrelated, per the delegation prompt's verification instruction #5.**

## Out-of-Scope File Check (independent verification)

`git diff --stat 67c2e3b0..HEAD -- ":(exclude).claude" -- "*.csproj" "*.editorconfig" "*AssemblyInfo.cs" "artifacts/*"` returned no output — zero matches. The full 50-file diff contains only: `FileIO2.cs`, `FileIO2_Tests.cs`, and 48 files under this feature folder (plan, spec, evidence). No `.csproj`, `.editorconfig`, `AssemblyInfo.cs`, or caller file (`AppOlObjects.cs`, `QfcHomeController.Metrics.cs`) was modified. **Confirms delegation prompt's verification instruction #4.**

## Toolchain Substitution Check (issue #752 workaround)

`evidence/baseline/p0-t17-utilitiescs-coverage.md` (P0) and `evidence/qa-gates/p5-t5-utilitiescs-coverage.md` (P5) both open with an identical `KNOWN_ENVIRONMENT_DEFECT: issue #752` disclosure, cite the same substituted command shape (`dotnet-coverage collect <vstest> <dll> /InIsolation ... --output-format cobertura`), and record the same acceptance conditions (total/passed/failed counts, failure-name-set comparison) the literal wrapper-script invocation would have produced. The substitution did not weaken the gate: it changed only how the Cobertura XML was produced, not what was measured or what threshold was applied. **Confirms delegation prompt's verification instruction re: P0-T17/P5-T5 substitution documentation.**

## Overall Verdict

**PASS. Ready to merge; 0 blocking feature-audit findings.** All 9 spec.md acceptance criteria are delivered and independently verified; AC9 carries a disclosed, non-blocking deviation (pre-existing unrelated test-infrastructure failures) that does not represent unmet scope of this fix.
