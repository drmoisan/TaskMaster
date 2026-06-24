# Cold-Start Capture Instructions — SpamBayes-init Sub-Step Attribution (issue #211)

Timestamp: 2026-06-24T15-10

Audience: maintainer (runtime, non-CI). This capture is NOT CI-automatable; it requires a live
Outlook process and a reproduction of the slow cold start.

## Purpose

The Phase 3.5 instrumentation emits one structured `[spam-init]` line per sub-step of
`SpamBayes.CreateAsync` and one per COM folder access in `SpamBayes.ValidatePathsSet()`, all via
the existing `log4net` Debug logger with `Stopwatch` F1 milliseconds. This capture names the exact
blocking sub-step and folder relative to the proven ~113 s STA freeze during Spam engine init.

## Six expected `[spam-init]` line tags

`SpamBayes.CreateAsync` (three sub-steps, in order):
1. `[spam-init] step=ValidatePathsSet ms=<F1>`
2. `[spam-init] step=ValidateSpamClassifier ms=<F1>`
3. `[spam-init] step=InitAsync(modelLoad) ms=<F1>`

`SpamBayes.ValidatePathsSet()` (three per-folder COM reads, in order, nested within the
`ValidatePathsSet` sub-step above):
4. `[spam-init] step=ValidatePathsSet.JunkCertain ms=<F1>`
5. `[spam-init] step=ValidatePathsSet.JunkPotential ms=<F1>`
6. `[spam-init] step=ValidatePathsSet.Inbox ms=<F1>`

Note: the three per-folder lines (4-6) are emitted BEFORE the aggregate `ValidatePathsSet` line (1),
because they run inside `ValidatePathsSet()` while line 1 is emitted after `ValidatePathsSet()`
returns. If a folder read throws `ArgumentNullException`, the per-folder line for that and any
subsequent folder is not emitted (the validation returns `false`); this preserves the
pre-instrumentation behavior and itself indicates which folder access failed.

## Steps for the maintainer

1. Confirm the build under test includes this branch (`bug/outlook-startup-latency-211`) with the
   Phase 3.5 instrumentation (`SpamInitTimingProbe` present; `SpamBayes.CreateAsync` and
   `ValidatePathsSet` emit `[spam-init]` lines).
2. Ensure `log4net` is configured to capture `Debug`-level output for the `SpamBayes` logger. The
   probe sink is `s => logger.Debug(s)`, where `logger` is the `SpamBayes` type logger
   (`UtilitiesCS.EmailIntelligence.SpamBayes`). Verify the active appender (file or console) shows
   `DEBUG` messages; raise the level / threshold if necessary.
3. Perform a NON-DEBUGGER cold start: close Outlook fully, then launch Outlook normally (not under
   the Visual Studio debugger) so the timing reflects production STA behavior. Reproduce the slow
   startup that exhibits the ~113 s freeze during Spam engine init.
4. Locate the six `[spam-init]` lines in the log4net Debug output (log file or console). They appear
   during add-in startup, around the Engines phase, alongside the existing `[engine-init]` and
   `[Startup timing]` lines (those existing lines are unchanged by this work).
5. Record, for the slow cold start:
   - the per-sub-step ms: `ValidatePathsSet`, `ValidateSpamClassifier`, `InitAsync(modelLoad)`;
   - the three per-folder ms: `ValidatePathsSet.JunkCertain`, `ValidatePathsSet.JunkPotential`,
     `ValidatePathsSet.Inbox`.
6. Paste the six raw `[spam-init]` lines into the placeholder artifact
   `runtime-capture-spam-init-PLACEHOLDER.md` (same `evidence/other/` folder) and note which
   sub-step / folder accounts for the ~113 s freeze.

## Interpretation

- If `InitAsync(modelLoad)` dominates, the freeze is in the model load/deserialize (`Task.Run`
  offload of `sb.InitAsync`), not in the COM folder reads.
- If `ValidatePathsSet` dominates and one of `ValidatePathsSet.JunkCertain` /
  `ValidatePathsSet.JunkPotential` / `ValidatePathsSet.Inbox` dominates within it, that exact COM
  folder access is the blocking call.
- If `ValidateSpamClassifier` dominates, the cost is in the classifier-group validation path.

The named blocking sub-step and folder are the targets for a later (out-of-scope) fix. This work is
diagnosis-only and behavior-preserving; it applies no fix.
