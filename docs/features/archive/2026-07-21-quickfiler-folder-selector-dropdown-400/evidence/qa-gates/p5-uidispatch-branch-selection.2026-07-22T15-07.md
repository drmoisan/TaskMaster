# P5-T175 — UI-dispatch correction branch selection

Timestamp: 2026-07-22T15-07Z

Command: `cd "C:/Users/DanMoisan/repos/TaskMaster-wt/2026-07-21T10-25" && grep -n "DETERMINATION: B" docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/p5-uidispatch-rootcause-diagnosis.2026-07-22T15-07.md`

EXIT_CODE: 0

## BRANCH: B

Selected solely from the P5-T172 determination
(`evidence/qa-gates/p5-uidispatch-rootcause-diagnosis.2026-07-22T15-07.md`), which recorded
`DETERMINATION: B` — at least one production path completes the returned task without crossing the captured UI
dispatcher.

Branch B therefore executes P5-T177 (production correction), scoped to the single production file
`QuickFiler/Viewers/BreadcrumbUiDispatcher.cs` named by the P5-T172 deciding lines.

## BRANCH: A — NOT APPLICABLE

Branch A ("no production path can complete the returned task without posting, so the failure is a harness determinism
defect at the line 52 observation gate") is not applicable. Reason, quoted from the P5-T172 cited lines:

- `BreadcrumbUiDispatcher.cs` line 78: `            if (IsCurrentBoundary())`
- `BreadcrumbUiDispatcher.cs` line 84: `                    action();`
- `BreadcrumbUiDispatcher.cs` line 94: `                return Task.CompletedTask;`
- `BreadcrumbUiDispatcher.cs` lines 259-262:
  ```
  259                || (
  260                    _ownerThreadId.HasValue
  261                    && Environment.CurrentManagedThreadId == _ownerThreadId.Value
  262                );
  ```

A production path therefore does exist that runs the action inline and returns a completed task without ever reaching
`_context.Post` (line 122): when the router continuation resumed after `ConfigureAwait(false)` lands on a recycled
thread-pool thread whose managed thread ID equals the captured owner thread ID. Because a production defect explains the
failure, correcting only the test would be a Blocking masking finding under the plan's anti-masking rule.

P5-T176 is consequently satisfied without any file change by this `NOT APPLICABLE` record.

## Binding anti-masking constraints governing P5-T176 and P5-T177

1. No assertion may be weakened, relaxed, renamed away, or deleted.
2. No `Thread.Sleep`, `Task.Delay`, wall-clock wait, retry loop, or timing threshold may be added.
3. `[DoNotParallelize]`, `[Ignore]`, and category-based skips may not be used as the fix.
4. No test filter may be narrowed and no coverage or test exclusion may be added.
5. The corrected behavior must be deterministic under **both** instrumented (`dotnet-coverage`) and uninstrumented
   execution.

Authorities cited:

- `.claude/rules/csharp.md`, "Prohibited Behaviors", which lists "Weakening assertions or relaxing test expectations to
  make tests pass" and "Adding sleeps, retries, or timing hacks to mask flaky behavior".
- `.claude/rules/general-unit-test.md`, "Determinism Infrastructure", which requires deterministic test code and lists
  "`setTimeout`, `Thread.Sleep`, `Task.Delay`, real wall-clock waits" among banned APIs in test code.

Output Summary: `BRANCH: B` selected, derived only from the P5-T172 `DETERMINATION: B`. `BRANCH: A` recorded
`NOT APPLICABLE` with the deciding lines quoted (`BreadcrumbUiDispatcher.cs` 78, 84, 94, 259-262), because a production
path does complete the returned task without posting to the captured context. Exactly one branch is applicable. P5-T176
is satisfied with no file change by this record. The five binding anti-masking constraints and their two rule-file
authorities are restated above and govern the P5-T177 correction batch. EXIT_CODE: 0.
