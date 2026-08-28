---
name: msbuild-msb3021-only-means-test-host-lock
description: An analyzer/nullable gate failing with only MSB3021/MSB3027 and ZERO compiler diagnostics is a live testhost holding bin\Debug — diagnose the contention, do not chase the code
metadata:
  type: feedback
---

`msbuild TaskMaster.sln /t:Rebuild ...` exiting 1 with a large error count is not automatically a code failure. Classify the errors before reacting.

On epic child 442 (2026-08-27) the analyzer gate returned `28 Error(s)` and every one was `MSB3021` or `MSB3027`:

```
error MSB3027: Could not copy "obj\Debug\X.Test.dll" to "bin\Debug\X.Test.dll".
  Exceeded retry count of 10. Failed. The file is locked by: "testhost (84376)"
```

**Zero `CS`/`CA`/`IDE` diagnostics were emitted.** The cause was a coverage-enabled `vstest` run live in the same worktree holding every test assembly's `bin\Debug` output. `/t:Rebuild` deletes and re-creates those files, so the two are mutually exclusive by construction.

**Why it matters:** the gate reads as a hard failure and invites a hunt through the diff. It is purely environmental, and the correct response is to free the worktree and restart the toolchain from step 1 — which the Phase 6 restart rule already requires.

**How to apply:**
- Triage by grepping the log for `: error CS` / `: error CA` / `: error IDE`. If that count is zero and the total is all `MSB302x`, it is contention.
- Find the contender by command line, not by name: `Get-CimInstance Win32_Process -Filter "Name='dotnet-coverage.exe' OR Name='vstest.console.exe'"` and match `CommandLine` against your worktree path. `testhost.exe`'s own command line does **not** name the worktree — resolve it through its `ParentProcessId` (the `vstest.console` that spawned it).
- Before terminating anything, prove the run is dead rather than slow: compare accumulated CPU against wall time (28.7 s of CPU across 30 min of wall), and check that no new `.trx` appeared under `TestResults/` and that `coverage/coverage.cobertura.xml` still carries an old mtime. A `/t:Rebuild` that already ran has invalidated its inputs anyway, so its result is worthless even if it completes.
- Record the termination and its justification in the toolchain-loop artifact. Killing a process another session started is a reasoned action, not routine cleanup, and an auditor should see why.
- Serialize instead of racing: gate the toolchain behind a wait loop that polls for zero `dotnet-coverage`/`vstest.console` processes naming your worktree before step 1.

Related: [[vstest-aggregate-crash-isolate-per-assembly]] for the genuinely environmental crash case, and [[parent-session-can-commit-into-child-worktree]] for who tends to be holding the lock.
