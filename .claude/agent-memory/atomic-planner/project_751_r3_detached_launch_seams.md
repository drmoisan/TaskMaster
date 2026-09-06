---
name: project-751-r3-detached-launch-seams
description: Issue #751 round-3 preflight — a detached Start-Process vstest convention needs an exit-code reset, sentinel codes, a start-time-qualified pid poll, a TRX completion witness, and an attempt cap; plus a net-line band cannot exclude an added using directive
metadata:
  type: project
---

Round-3 preflight on the #751 plan concentrated every blocking defect in the plan's
"Long-running commands" (detached-launch) convention. The pattern generalises to any plan
that launches a long test run out-of-band and reads its exit code back off disk.

**Why:** each defect made a *failed or never-started* run indistinguishable from a green one,
so the gate could not fail. That is the same class the atomic-plan-contract's "observe a
command's success-case output" rule exists to prevent, but none of it is validator-decidable.

**How to apply:** when authoring or reviewing a detached-launch convention, and when authoring
a net-line-count acceptance band on a file.

1. **Reset `$LASTEXITCODE` immediately before the run.** The script's mandatory opening
   `Set-Location (git rev-parse --show-toplevel)` sets `$LASTEXITCODE` to 0. Without a reset,
   an unresolved or unlaunchable `$vstest` leaves that stale 0 in place and the artifact
   records a successful run that never happened.
2. **Sentinel codes, not silence.** `9001` = the command never set an exit code; `9002` = a
   terminating error was caught. Write the exit file from a `finally` block so it exists even
   when the run throws, and state explicitly that the sentinels are never a task's `EXIT_CODE:`.
   Redirect the caught error to a companion `<task-id>.fault.txt`.
3. **`-WorkingDirectory` on `Start-Process` is mandatory.** The child's cwd otherwise depends on
   the launching shell's process-level current directory, which `Set-Location` in an agent shell
   does not reliably set for a child. (Independently re-verified by the reviewer.)
4. **A pid poll must be qualified by start time.** `Get-Process -Id <pid>` alone treats a
   recycled pid as the original process. Print and record `$proc.StartTime.ToString('o')` as
   `LaunchedStart:` and compare it on every poll. Same rule for `Stop-Process`: verify start time
   (or `CreationDate` for a WMI-captured child) before killing, or an unrelated process dies.
5. **Capture descendants via `Get-CimInstance Win32_Process -Filter 'ParentProcessId=<pid>'`,
   recursively**, so `testhost` grandchildren of `vstest.console.exe` are terminated child-first.
   Record as `ChildPids:`; allow `none captured` when the run exited before the first poll.
6. **An exit code needs a completion witness.** Pair it with the TRX the task's
   `/Logger:trx;LogFileName=` names: absent or unparseable TRX means `RUN_INCOMPLETE`, whatever
   the exit file says. Cap polling (40 polls → `RUN_HUNG`) and cap relaunches (3 attempts total
   → `RUN_UNRECOVERABLE`, no numeric `EXIT_CODE:`), or the "relaunch from step 1" instruction is
   an unbounded loop.
7. **A net-line band cannot exclude an added `using` directive.** A band of 1-2 net lines that
   exists to tolerate formatter reflow of one inserted statement equally admits one added
   `using` or one added one-line field. Add a companion scan
   `git diff <BASE>..HEAD -- "<glob>" | Select-String -Pattern '^\+\s*(using\s|private\s|internal\s|protected\s|public\s|static\s)'`
   and require an empty match set. Then cite *that* scan, not the numstat band, as the proof of
   a "no new using directive" acceptance clause. See [[absolute-counts-in-shared-files-go-stale]].
8. **Independent outcome branches must not share an antecedent.** A plan that says "outcomes B
   and C are not mutually exclusive" while C's antecedent begins "P4-T2 recorded rung 1, but ..."
   (and B requires rung 2) has made the stated combination unreachable. Drop the borrowed clause.
9. **"The same three-rung ladder" must mean the same rung.** Two coverage figures produced by
   different rungs are computed by different methods (Cobertura root attributes vs summed
   per-module counters) and are not comparable; route a rung mismatch to the informational
   denominator-shift outcome, never to the numeric outcome.
   See [[conditional-ladder-and-unowned-class-gates]].
10. **A fixed body-join window is verified, not derived.** `$l[($s-1)..($s+24)]` is safe only
    while the method's post-edit extent fits; record the declaration line, the closing-brace
    line, and the remaining margin so a later editor knows to widen it.
