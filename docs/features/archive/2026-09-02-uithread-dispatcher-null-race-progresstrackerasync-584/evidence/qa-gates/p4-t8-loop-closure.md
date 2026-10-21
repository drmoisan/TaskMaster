# P4-T8 — Phase 4 loop closure and post-format file-size re-audit

Timestamp: 2026-09-03T22-15

Command:
```text
env -C <worktree-root> wc -l UtilitiesCS/Threading/UiThread.cs UtilitiesCS.Test/Threading/UiThread_Tests.cs UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs "QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs"
see P4-T1..P4-T7
```

EXIT_CODE: 0

## Output Summary

### Phase 4 pass record — every pass, chronological order

Phase 4 ran twice. Both passes are recorded below, earliest first, per clause 2.

#### Pass 1 — 2026-09-03, first pass (FAILED at P4-T6)

| Step | Outcome | Artifact path |
|---|---|---|
| P4-T1 Format | green | `evidence/qa-gates/p4-t1-format.md` (subsequently overwritten by pass 2) |
| P4-T2 Format check | green | `evidence/qa-gates/p4-t2-format-check.md` (subsequently overwritten by pass 2) |
| P4-T3 Analyze | green | `evidence/qa-gates/p4-t3-analyzer-build.md` (subsequently overwritten by pass 2) |
| P4-T4 Type-check | green | `evidence/qa-gates/p4-t4-nullable-build.md` (subsequently overwritten by pass 2) |
| P4-T5 Test UtilitiesCS.Test | green | `evidence/qa-gates/p4-t5-utilitiescs-tests.md` (subsequently overwritten by pass 2) |
| P4-T6 Test QuickFiler.Test | **FAILED**, EXIT_CODE 1, 8 of 1312 failing | preserved at `evidence/regression-testing/p4-t6-first-pass-failure.md` |
| P4-T7 Coverage delta | not reached | — |

Pass 1 recorded `Total tests: 1312`, `Passed: 1304`, `Failed: 8` at P4-T6, with all eight failures in
`QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs`. That failure is the fail-before evidence
for P2-T4, and it is the reason this phase ran more than once. Its artifact was preserved under a
separate name by P4-T6's conditional preservation clause before pass 2 overwrote the qa-gates copy.

Between pass 1 and pass 2, P2-T4 rewrote the tracked source file
`QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs`, retargeting its reflective dispatcher
snapshot from the public `Dispatcher` property to the private `_dispatcher` backing field. Because
that rewrite landed after pass 1's four earlier steps had run, pass 1's artifacts described a tree
that no longer existed, and every step in this phase was re-run in order against the tree P2-T4 left.

#### Pass 2 — 2026-09-03, second pass (all steps green)

| Step | EXIT_CODE | Artifact path |
|---|---|---|
| P4-T1 Format | 0 | `evidence/qa-gates/p4-t1-format.md` |
| P4-T2 Format check | 0 | `evidence/qa-gates/p4-t2-format-check.md` |
| P4-T3 Analyze | 0 | `evidence/qa-gates/p4-t3-analyzer-build.md` |
| P4-T4 Type-check | 0 | `evidence/qa-gates/p4-t4-nullable-build.md` |
| P4-T5 Test UtilitiesCS.Test | 0 | `evidence/qa-gates/p4-t5-utilitiescs-tests.md` |
| P4-T6 Test QuickFiler.Test | 0 | `evidence/qa-gates/p4-t6-quickfiler-tests.md` |
| P4-T7 Coverage delta | 0 | `evidence/qa-gates/p4-t7-coverage-delta.md` |

Pass 2 is the pass whose artifacts this task lists, and it is the last entry.

### Did any step after P4-T1 rewrite a tracked file?

No. In pass 2:

- P4-T1's own before-and-after unscoped `git status --porcelain` outputs are byte-identical, and the
  formatter left all six owned files at the same line counts they had before it ran.
- P4-T2 is `csharpier check`, which is read-only; it reported no unformatted path.
- P4-T3 and P4-T4 are MSBuild `/t:Rebuild` invocations. They write build output under `bin/` and
  `obj/`, both of which are gitignored, and they modify no tracked source file.
- P4-T5 and P4-T6 write only under `coverage/` and `TestResults/`, both gitignored.
- P4-T7 reads the diff and the Cobertura document; it writes only its evidence artifact.

The unscoped `git status --porcelain` taken at this task reports exactly the same tracked-source
entries P4-T1 observed before the formatter ran: the six owned files and the two feature documents.
No tracked file outside that set changed state during the phase, so the loop did not need to restart
from P4-T1 after pass 2 and pass 2 is a single clean pass.

### Post-format per-file line counts

```text
  172 UtilitiesCS/Threading/UiThread.cs
  179 UtilitiesCS.Test/Threading/UiThread_Tests.cs
  348 UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs
  206 UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs
  514 UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs
  320 QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs
```

The trailing `total` row is ignored.

| File | Baseline | Post-format | Clause | Result |
|---|---|---|---|---|
| `UtilitiesCS/Threading/UiThread.cs` | 163 (P0-T13) | **172** | P2-T3 clause 1 (< 500) | PASS |
| `UtilitiesCS.Test/Threading/UiThread_Tests.cs` | 104 (P0-T13) | **179** | P2-T3 clause 1 (< 500) | PASS |
| `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` | 347 (P0-T13) | **348** | P2-T3 clause 1 (< 500) | PASS |
| `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` | 205 (P0-T13) | **206** | P2-T3 clause 1 (< 500) | PASS |
| `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` | 514 (P0-T13) | **514** | P2-T3 clause 2 (<= baseline + 1) | PASS |
| `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` | 320 (P2-T4 post-edit) | **320** | clause 4 (< 500 and <= 320 + 2) | PASS |

PRE-EXISTING FILE-SIZE OVERAGE: UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs

That overage exists at BASE `87cb4df338322844abfa580abea14df77e738e5c`, where the file is already 514
lines, and it is not introduced by this change. Its post-format count is unchanged at 514, so the
baseline-plus-one tolerance is not consumed.

## Acceptance

1. **Satisfied.** All seven steps are listed in order with their artifacts, and no step after P4-T1
   rewrote a tracked file, so the loop did not restart after pass 2.
2. **Satisfied.** Every Phase 4 pass is recorded explicitly, one entry per pass, in chronological
   order. The first entry is the 2026-09-03 pass with P4-T1 through P4-T5 green and P4-T6 failing 8
   of 1312. The last entry is the pass whose artifacts this task lists. Two entries are recorded
   because Phase 4 ran twice; the clause states no upper bound.
3. **Satisfied.** The first five post-format line counts satisfy both P2-T3 clauses evaluated against
   the P0-T13 baseline counts, as tabulated above.
4. **Satisfied.** The sixth post-format line count is 320, which is strictly less than 500 and is
   less than or equal to the post-edit count P2-T4 recorded (320) plus 2. `csharpier` did not re-wrap
   the retargeted `GetField(` call, so the plan's intent of a count unchanged from P2-T4's was met
   exactly and the plus-two tolerance was not consumed.
