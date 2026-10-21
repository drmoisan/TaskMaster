# Follow-up findings to promote — issue #798

Timestamp: 2026-09-07T05-58
Task: [P9-T16]
Issue: #798

These are the three follow-up findings enumerated under **Rollout & Follow-up** in spec.md. All three
are explicit non-goals of this change: each was observed while delivering the AC1 through AC5 fix and
none is in this item's sixteen-path write set.

## Promotion route — not exercised in this session

**The potential-to-issue promotion lifecycle was not run on this branch, and no potential-entry file
was created.** This is recorded explicitly rather than omitted, which is what this task's acceptance
requires when the promotion route is not exercised.

The reason is a hard constraint from this item's own acceptance criteria. AC13 fixes the change to
exactly sixteen paths, verified by the anchored write-set diff in
`evidence/qa-gates/p7-ac13-write-set-diff.md`, which observes a count of 16 both at P7-T2 and again at
P8-T9. Creating a promotion file on this branch would add a seventeenth path and would falsify the
write-set gate that P9-T13 checks off. Running promotion tooling here would therefore break a
criterion in order to record a non-goal.

**Each of the three findings below is to be promoted through the potential-to-issue lifecycle after
this branch merges**, so that none is lost when this feature folder is archived. The findings are
recorded here in full, with locations, so the promotion can be performed from this artifact alone
without re-deriving anything.

---

## Finding 1 — unreachable `catch (TimeoutException)` in the two `repeatAttempts` timeout overloads

**Location:** `UtilitiesCS/Threading/TimeOutTask.cs`

- Generic overload `public static Task<TResult> TimeoutAfter<TResult>(...)` taking `int repeatAttempts`, declared at line 824. Its `catch (TimeoutException)` is at line 836, and the documented retry is at lines 839 to 845.
- Non-generic overload `public static Task TimeoutAfter(this Task task, int millisecondsTimeout, int repeatAttempts)`, declared at line 924. Its `catch (TimeoutException)` is at line 932, and the documented retry is at lines 934 to 936.

**The defect.** Both overloads wrap the inner call in a `try` and catch `TimeoutException` in order to
retry up to `repeatAttempts` times. The retry never executes. The wrapped call returns a task proxy
rather than throwing synchronously, so the `TimeoutException` surfaces when that proxy is awaited by
the caller, which is outside the `try` block. The catch clause is therefore unreachable and both
overloads silently provide no retry at all, contrary to what their parameter name and their warning
messages state. A caller passing `repeatAttempts: 3` receives one attempt.

**Why it was not fixed here.** `UtilitiesCS/Threading/TimeOutTask.cs` is 1011 lines and already over
the repository's 500-line cap, so editing it would force an unrelated cap remediation inside a
targeted bugfix, which the bugfix workflow in CLAUDE.md prohibits as an opportunistic refactor. The
file is an explicit non-goal of this item and AC8 pins it as untouched: the anchored diff and the
porcelain status over that directory both produce zero output lines, recorded in
`evidence/qa-gates/p7-ac8-timeoutafter-unchanged.md`. This change reuses the unrelated three-argument
`TimeoutAfter(Task, int, TimeProvider?)` overload, which does not carry the defect.

**Severity note for the promoted issue.** No caller in this change relies on the retry, so this is a
latent correctness defect rather than an active one. It is misleading rather than currently harmful,
which is why it is a follow-up and not a blocker.

---

## Finding 2 — unguarded shared-static message-box seam mutation under class-level parallelization

**Location:** `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs`

Four save-mutate-restore sites on the process-wide static `DfDeedle.MessageBoxInvoker`:

| Save | Mutate | Restore |
|---|---|---|
| line 197 | line 198 | line 210 |
| line 222 | line 223 | line 235 |
| line 261 | lines 264 onward | line 280 |
| line 317 | line 318 | line 333 |

**The defect.** Each site correctly saves the previous delegate and restores it in a `finally`, so the
pattern is safe against sequential execution. It is not safe against concurrent execution. The
UtilitiesCS test assembly parallelizes at class level, and `DfDeedle.MessageBoxInvoker` is a single
static shared by the whole process. A different test class running concurrently and reaching
`EnsureTriageColumnExists` observes whichever delegate the COM test class installed most recently,
and can observe the restore landing mid-assertion. This is a latent flake: it does not fail
deterministically, which is why it has not been caught by a run.

**Why it was not fixed here.** The file is in this item's write set only because P1-T11 repaired two
reflection invocations that the widened `AddQfcColumnsAsync` signature broke. Adding
`[DoNotParallelize]` to the class, or introducing a seam lock, is a behavioural change to test
scheduling that is outside AC1 through AC5 and would alter the runtime characteristics of a class
this item did not otherwise touch. spec.md names it a non-goal in Risks item 4.

**What this change did do.** The new timeout test class
`UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs` carries `[DoNotParallelize]`, because
it drives process-wide log4net state. The new validator test class is pure and needs no attribute.
The pre-existing class is untouched in this respect.

---

## Finding 3 — pre-existing 500-line-cap violation in the same COM test class

**Location:** `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs`

| Measurement | Lines |
|---|---|
| At base commit c431dc32 | 882 |
| **Observed post-change** | **869** |
| Repository cap | 500 |

**Observed post-change line count: 869.**

**The finding.** The file was already 369 lines over the repository's 500-line cap before this change
touched it. This change reduces it by 13 lines, delivered by the P1-T11 reflection-test repair that
removed the `GetAddQfcColumnsAsyncMethod` helper and its two local bindings and replaced two
reflective invocations with direct calls. It remains 369 lines over the cap.

**Why it was not brought under the cap here.** Doing so would require splitting the file into at least
one additional file, which would be a seventeenth path outside the sixteen-path write set that
spec.md fixes and AC13 pins, and would break the write-set gate at P7-T2 and P8-T9. The bugfix
workflow prohibits that kind of refactor inside a targeted defect fix.

**Relationship to AC13.** AC13 holds this one pre-existing over-cap file to a strictly decreasing line
count relative to its base value of 882, rather than to the absolute cap; 869 < 882 satisfies it.
That reading was established at baseline in `evidence/baseline/line-cap-preexisting.md` and applied
in `evidence/qa-gates/p7-ac13-line-cap.md`. AC13's final clause requires this pre-existing violation
to be promoted as a follow-up, and this entry is that record — it is the entry P9-T13's acceptance
requires to exist before AC13 may be checked off.

---

Output Summary: Three follow-up findings recorded with locations. (1) Unreachable
`catch (TimeoutException)` in the two `repeatAttempts` `TimeoutAfter` overloads at
`UtilitiesCS/Threading/TimeOutTask.cs` lines 836 and 932, whose documented retry never executes
because the wrapped call returns a proxy that faults after the `try` block. (2) Unguarded
shared-static `DfDeedle.MessageBoxInvoker` mutation at four sites in
`UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` under class-level parallelization. (3) The
pre-existing 500-line-cap violation in that same file, observed post-change at 869 lines against a
base of 882 and a cap of 500. The potential-to-issue promotion route was **not** exercised in this
session, because creating a promotion file on this branch would add a seventeenth path and falsify
the AC13 write-set gate; each finding is to be promoted after this branch merges.
