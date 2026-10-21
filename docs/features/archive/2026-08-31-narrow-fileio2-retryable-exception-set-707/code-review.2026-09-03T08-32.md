# Code Review — narrow-fileio2-retryable-exception-set (Issue #707)

- Reviewed: 2026-09-03T08-32
- Diff scope: `67c2e3b0eca90a52e9aee82ccd100acce4722169..HEAD -- ":(exclude).claude"` (50 files; footprint is 2 source files)

## Production Change: `UtilitiesCS/To Depricate/FileIO2.cs`

```csharp
catch (DirectoryNotFoundException ex)
{
    logger.Error(
        $"Failed to write to {filepath}: the target directory does not exist.",
        ex
    );
    return false;
}
catch (IOException ex)
{
    if (opened) { ... }
    Interlocked.Increment(ref attempts);
    if (attempts >= 100) { ... }
    await delayAsync(100, token);
}
```

- **Correctness**: `DirectoryNotFoundException` derives from `IOException`; C# requires the more-derived catch clause to appear first in the same `try`, which this diff does (verified by direct read of the compiled method — `catch (DirectoryNotFoundException ex)` at line 126, `catch (IOException ex)` at line 134). Reversing the order is CS0160, self-detecting at compile time; the analyzer and nullable rebuilds both succeeded at 0/0, corroborating the ordering is valid.
- **Behavior**: the new block does not call `Interlocked.Increment(ref attempts)` or `await delayAsync(...)` — confirmed both by direct grep of the diff hunk and by the regression test's assertion (`missingDirectoryDelayCalls.Should().Be(0)`, `missingDirectoryFactoryCalls.Should().Be(1)`). The general `catch (IOException ex)` block is untouched (single-hunk diff), so the retry-exhaustion path for other `IOException` subtypes (e.g. bare sharing-violation `IOException`) is unaffected.
- **Logging**: uses the same `logger.Error(string, Exception)` two-argument overload as the sibling `catch (IOException ex)` block, with a message distinguishing the missing-directory case from the generic retry-exhaustion message. Consistent with the repo's established logging pattern in this file; no ad-hoc console output introduced.
- **Design principles**: minimal, additive, single-responsibility catch block. No opportunistic refactor of the surrounding method. Matches the repo's Bugfix Workflow (minimal targeted fix, no broader restructuring). File remains 301 lines, well under the 500-line limit.
- **Naming**: no new identifiers introduced in production code beyond the caught exception's bound name `ex`, matching the sibling block's convention.
- **Nullability / error handling**: no new nullable surface; exception is caught, logged, and the method returns its existing `Task<bool>` contract unchanged. Fail-fast is preserved — the method still does not throw on a failed write, consistent with the documented `<returns>` contract above it (unmodified by this diff).

No defects identified in the production change.

## Test Change: `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs`

- New test `WriteTextFileAsync_WhenDirectoryDoesNotExist_ShouldReturnFalseWithoutRetrying` (38 lines) follows the exact structural pattern of its sibling `WriteTextFileAsync_WhenWriteFailsAfterOpen_ShouldReturnFalseWithoutRetrying` immediately above it: local call-counters, injected `writerFactory`/`delay` delegates, `CancellationTokenSource` disposed via `using`, three FluentAssertions calls (`factoryCalls.Should().Be(1)`, `delayCalls.Should().Be(0)`, `result.Should().BeFalse()`).
- Assertion order places the two count assertions before the boolean-result assertion; this matches the RED-first evidence (`p2-t3-missingdirectory-fail-before.md`) where the pre-fix failure surfaces on the first assertion (`missingDirectoryFactoryCalls.Should().Be(1)`, actual 100) — a clear, actionable failure message pinpointing the retry-count defect rather than only the boolean outcome.
- No temp files, no real filesystem access (`writerFactory` throws before any I/O), no real wall-clock delay (`delay` is a synchronous stub returning `Task.CompletedTask`, and is asserted never called). Fully compliant with UT4 (external dependencies / temp file prohibition) and the Determinism Infrastructure rules (no `Thread.Sleep`/`Task.Delay` in test code).
- XML-doc-style comment above the test explains both the scenario (`DirectoryNotFoundException` is structurally non-retryable) and the observable proof shape (factory-call and delay-call counts), satisfying the "document intent" requirement.
- Test file remains 373 lines, well under the 500-line limit.

No defects identified in the test change.

## Evidence-Trail Observations (non-blocking)

1. **AC9 wording gap, self-disclosed.** `spec.md`'s AC9 text says "with all tests green" for the full `vstest.console.exe` run against `UtilitiesCS.Test`, but the full-suite run has 17 pre-existing, unrelated Deedle/F# failures in both baseline and post-change runs (identical sets, confirmed by this review — see feature-audit). The executor's own `p6-t9-ac9.md` evidence explicitly flags this literal-text gap and reconciles it against the plan's narrower task-level acceptance text (full `FileIO2_Tests` suite green) rather than silently checking the box. This is good practice — the discrepancy is surfaced, not hidden — but it means AC9 as literally worded in `spec.md` is not fully satisfied by a strict reading. See feature-audit for the disposition.
2. **Stale-merge-base evidence, one file not caught up.** `evidence/qa-gates/p6-t8-ac8-caller-scope.md` (AC8 verification) computed its diff using the stale `BASE_SHA` (`687f15fb`) from `p0-t7-base-ref.md`, before the discrepancy was identified and disclosed in the later `p7-t2-commit-verification.md`. This reviewer confirmed `687f15fb` is an ancestor of the correct reconciliation-merge base `67c2e3b0` (`git merge-base --is-ancestor` exit 0), so the 360-path diff AC8 searched is a strict superset of the correct 50-path scope; the negative-match conclusion for the two excluded caller files is unaffected. No corrective action needed, but a future pass could backfill the discrepancy note into `p6-t8-ac8-caller-scope.md` for internal consistency with `p7-t2`.
3. **Cobertura delta line-count discrepancy is unexplained in evidence.** The raw source diff adds 8 lines to `FileIO2.cs`; the Cobertura-derived new-code delta (`p5-t8-coverage-delta.md`) reports 14 new "valid" and 14 new "covered" lines. The evidence attributes this to the async state-machine class-merge transform but does not show the underlying per-class breakdown that would make the 8-vs-14 gap independently reproducible. The acceptance conclusion (100% new-code coverage) is not in question — baseline and post-change both went through the identical merge transform — but a future evidence pass could attach the raw (pre-merge) per-class Cobertura fragment for full auditability.

None of these three observations rises to a blocking finding; all are documentation/traceability quality notes on an otherwise clean change.

## Summary

The production fix is a single, minimal, compiler-verified-correct catch-block insertion that satisfies the issue's stated Expected Behavior. The regression test is well-isolated, deterministic, and demonstrably RED-before/GREEN-after. No best-practice violations (naming, structure, error handling, file size, dependency isolation) were found in either changed file.

**No blocking code-review findings.**
