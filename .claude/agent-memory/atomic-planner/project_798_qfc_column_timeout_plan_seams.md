---
name: project-798-qfc-column-timeout-plan-seams
description: Issue #798 planning seams — frozen write set vs a pre-existing 882-line file; relocation voids per-file coverage comparison; R1 adds the unwired-seam, CSharpier-wrap, generic-overload-anchor and erasing-fallback traps
metadata:
  type: project
---

Planning seams found while authoring the atomic plan for issue #798 (QuickFiler column-add timeout
swallowed, `KeyNotFoundException`).

**A "no file exceeds 500 lines" AC is unsatisfiable when the write set is frozen and one member is
already over the cap.** spec.md AC13 says "No file created or modified by this change exceeds 500
lines" and the write set is fixed at 16 paths. But `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs`
is 882 lines at base commit `c431dc32` and *must* be modified: widening `AddQfcColumnsAsync` from a
4-parameter `private static` to a 6-parameter `internal static` makes its two reflection `Invoke`
calls throw `TargetParameterCountException` (reflection does not apply C# defaults absent
`Type.Missing` plus `BindingFlags.OptionalParamBinding`). Getting to 500 needs 382+ lines moved into a
17th path.

**Why:** the cap clause and the frozen write set were authored independently and neither noticed the
pre-existing over-cap file.

**How to apply:** do not silently weaken the AC and do not silently add the 17th path. Author the gate
as: absolute 500 cap for every file *except* the named pre-existing offender, plus a **strictly
decreasing** line count for that one, and record the deviation in a dedicated Phase 0 artifact that
the AC check-off task cites. Report the deviation to the caller rather than burying it. See
[[absolute-counts-in-shared-files-go-stale]].

**A relocation change voids a per-file coverage comparison.** The plan moves four methods out of
`UtilitiesCS/Extensions/DfDeedle.cs` (410 lines) into a new partial. Comparing baseline vs final
`covered=` for the source file alone compares different denominators and can only be read as a
regression. Gate on the **sum** of `covered=` across the source file and the new partial instead. See
[[deletion-adjusted-coverage-no-regression-gate]].

**A cross-assembly log4net probe needs no new project entry.** The Phase 0 open question ("does a
`MemoryAppender` attached from `UtilitiesCS.Test` capture the `UtilitiesCS.DfDeedle` logger?") looks
like it needs a throwaway test file, which in a non-SDK project would be a 17th write path. It does
not: append the probe method to `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` (already in the
write set), run it filtered by `FullyQualifiedName~`, then remove it and gate on
`git diff --stat c431dc32 -- <that file>` being empty. `log4net` 3.4.0 is already referenced by that
project (`packages.config:11`, csproj `:576`), so no package change is needed either — unlike
`QuickFiler.Test`, which did need one.

**A "does not throw" test on a defect-preserving path is a guard test, not a fail-before test.** The
AC1 cancellation case (`AddQfcColumnsAsync` must stay silent when the token is cancelled) passes
against the *unfixed* code, because today the method returns normally in every exhaustion branch.
Tagging it `[expect-fail]` would be a false declaration. Tag only the tests whose assertion is
false before the fix.

**Two of the three deadline-positive cases are fail-before, one is not.** "Adder completes before the
first deadline" already passes today; the second and third fail, because today each retry starts a
*new* `Task.Run` so the adder is invoked more than once. Record the observed status per test rather
than declaring a uniform expectation for the group.

## Revision round 1 (2026-09-07, 20 defects)

**An injected seam that is declared but not WIRED makes every fail-before observation zero.** P1-T3
added `Action<object, object>? columnAdder = null` to `AddQfcColumnsAsync` but left the loop body as
`Task.Run(() => AddQfcColumns(table, folder), token)`, which never consults the parameter. The
Phase 2 tests then demanded "an observed adder invocation count of 3" from a seam invoked zero
times. The inverse of [[declaration-only-seam-task-for-fail-before]]: a seam task must name the
substitution at the *call site*, not only the signature, and must forward the new arguments through
any recursion or the second and third iterations run on the real clock.

**Forwarding two arguments to a recursion pushes the call past CSharpier's 100-column width, so a
whole-expression literal gate matches on no tree.** `await AddQfcColumnsAsync(table, folder, token,
counter + 1, columnAdder, timeProvider);` at 20 columns of indentation is 107 characters and wraps
one-argument-per-line. Assert a short token that survives the wrap — here `counter + 1`, exactly two
after the seam task and zero after the fix — not the whole call expression. See
[[literal-call-clauses-block-file-size-tightening]].

**A trailing `\(` anchor misses every generic overload.** `public static .*TimeoutAfter\(` returns 2
of the 4 declarations in `UtilitiesCS/Threading/TimeOutTask.cs`: the two at 824 and 862 are
`public static Task<TResult> TimeoutAfter<TResult>(` and the type-parameter list sits between the
name and the parenthesis. Drop the anchor when the count is the assertion.

**A fallback assertion strategy must not route through the seam that erases what it asserts.** The
AC2 fallback said: if cross-assembly log4net capture is unavailable, assert the timing
instrumentation "indirectly through the injected adder". But injecting an adder replaces the whole
`AddQfcColumns` body, so the instrumentation never executes; the adder's own entry-to-exit interval
reads identically before and after the fix, and a single delegate cannot observe six per-column
intervals. The sound fallback attaches the `MemoryAppender` to the repository reached through the
production assembly's own logger instance (reflect the private static `logger` field, then
`Logger.Repository`), which removes the default-repository assumption without removing the subject.

**Reaching a `private static` member needs a named access path.** `AddQfcColumns` and
`HasUserDefinedProperty` stay `private` (only `AddQfcColumnsAsync` is widened), and
`InternalsVisibleTo` does not reach `private`. The existing `DfDeedle_COM_Tests` already reaches all
three by `typeof(DfDeedle).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)` (lines 93,
158, 288, 495); relocating the methods into a partial of the same class keeps those sites working.

**A sentinel thrown upstream of the wrapping call arrives unwrapped.** Correction 2's
`AggregateException` wrapping happens at the dataframe-transform `TimeoutAfter` (DfDeedle.cs
186-190), which is *downstream* of table acquisition (line 151). A stack assertion for a sentinel
thrown at acquisition must be shape-agnostic — walk the exception and every inner exception — rather
than assuming either wrapping or unwrapping.

**AC13's wording moved; the plan's "documented deviation" framing became a self-reported failure.**
spec.md AC13 now itself requires only a strictly decreasing count for the pre-existing 882-line
file, and requires the violation to be promoted as the third follow-up. Three plan tasks still told
the executor to record a deviation from a criterion the change satisfies. When a spec is amended to
absorb a deviation, sweep every task that narrates it. See
[[feedback_spec_corrections_sweep_sibling_sections]].

**`git diff --name-only` cannot report a file created under the pathspec it guards.** Four
negative-space scope gates ran before the commit task, so a new file under the guarded directory
would be untracked and invisible. Each needs a `git status --porcelain --untracked-files=all`
companion with the same pathspec, in the same task.

Related: [[project_791_hc_deadline_cancel_teardown_plan_seams]] (same repo area, `TimeProvider`
availability), [[expect-fail-needs-a-synchronous-seam]],
[[declaration-only-seam-task-for-fail-before]],
[[csharpier-formatted-n-is-processed-count]], [[porcelain-collapses-untracked-directories]].
