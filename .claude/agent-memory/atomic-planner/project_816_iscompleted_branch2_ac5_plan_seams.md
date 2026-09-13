---
name: project-816-iscompleted-branch2-ac5-plan-seams
description: Issue #816 (UiThread IsCompleted _uiSyncContext exit hardening + issue 809 AC5 apartment measurement) R1 and R2 revision seams - recovered-plan re-derivation, coverage-dir ordering, UiThreadStateScope setter count, retry-class third test, porcelain-vs-hash format observation, HangDumpType=None, AC9 nondeterministic test vs EXIT_CODE 0
metadata:
  type: project
---

R1 revision of a plan recovered byte-exact from a dead predecessor worktree. Every citation had to be re-derived against the assigned worktree; the caller pre-verified most and listed them as "do not re-derive". R2 (preflight round 1, 2026-09-12) applied B1-B16 + R1/R2 corrections; task count stayed 74.

**Seams found in R1 (2026-09-12):**
- `UtilitiesCS/Threading/UiThread.cs`: `NonStaInitMessagePrefix` const is 230-231 (232 is blank); helper `NonStaInitMessage` 233-234. The plan had "230-232" once (P2-T6).
- `UtilitiesCS.Test/TestHelpers/UiThreadStateScope.cs` is in namespace `UtilitiesCS.Test` (NOT `.Threading`) and exposes exactly THREE setters: `SetUiSyncContext` 148, `SetUiThreadId` 155, `SetDispatcher(Dispatcher? value)` 161-162 (bare `FieldInfo.SetValue`, accepts null, `#nullable enable annotations` at 27). No `SetAutoScaleFactor`; only the `AutoScaleFactorField` getter 121-122. `Enter()` resets all statics via `UiThread.ResetForTesting()` (89), so `_dispatcher` is already null on entry.
- `UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs`: `SharedStaDispatcherHost` is `internal sealed` at 135; `ApartmentThreadRunner` 79-104 with `thread.Join();` at 102; `UiThreadInitRetryContract_Tests` (306) declares THREE `[TestMethod]`s (309, 340, 363), so "both retry-contract tests" is ambiguous - name the two FQNs AC5 lists.
- `UtilitiesCS.Test/Threading/UiThread_Tests.cs` declares TWO `private sealed class StaDispatcherHost` (319 inside `SynchronizationContextAwaiter_Tests` 12-370; 429 inside `UiThread_Dispatcher_Tests` 373). Cite the class, not just the name.
- Caller's delta text located a phrase in "P2-T6" that actually lived in P5-T6; grep the phrase rather than trusting the task id.

**Seams found in R2 (preflight round 1):**
- `SyncContextForm` is in namespace `QuickFiler.Viewers` (namespace line 14, class line 16 of `UtilitiesCS/Threading/SyncContextForm.cs`), compiled only into UtilitiesCS (`UtilitiesCS.csproj:1102`); a test in `UtilitiesCS.Test.Threading` needs an explicit `using QuickFiler.Viewers;`.
- `.gitignore:144` is `coverage/*` and `:145` is `!coverage/.gitkeep` (tracked), so `coverage\` always exists after checkout; only `coverage\logs` is untracked. Do not say the coverage directory "may be absent".
- A porcelain before/after capture around `csharpier format` is vacuous when the files are already modified vs the anchor; use `Get-FileHash -Algorithm SHA256 -LiteralPath a, b, c, d | Select-Object -ExpandProperty Hash` (never the Path property - absolute host path) and gate on four lines per capture.
- Every test command carries `Blame:...HangDumpType=None`, so "no dump file" cannot fail; a stall produces `Sequence_*.xml` in the results directory - gate on its absence.
- When AC9 names a nondeterministic test and permits an attributed failure, `EXIT_CODE: 0` on the full-suite tasks contradicts it; use `EXIT_CODE:` plus a one-time same-command re-run with hash proof of no intervening file change, `ExpectedExitCode: 1` on a second failure, and let the AC14 projection name the first zero-exit repetition as the fourth toolchain command.
- Fallback `msbuild /t:Restore` restores nothing for packages.config projects without `/p:RestorePackagesConfig=true`; gate on `(Get-ChildItem packages -Directory).Count` not directory existence.
- Under `/t:Rebuild` the `Skipping target "CoreCompile"` count is an invariant restatement; only the `Task "Csc"` count is the non-vacuity observation - say so.
- `dotnet tool run csharpier --version` has never been observed on this manifest; observe `dotnet tool list --local` row + `csharpier check --help` exit 0 instead.
- Reviewer REJECTED `Set-Location (git rev-parse --show-toplevel)` as a working-directory fix: from the session root it resolves to the session root. Require the delegation prompt to supply the absolute worktree root and record an identity check (solution + owned file exist, leaf dir name matches the feature folder's root) in P0-T7.
- MSTest does not discover non-public test classes; state `public` explicitly and gate on it, because a compile gate cannot catch it.
- A fail-before dossier filename must carry the artifact's write timestamp (matching its `Timestamp:` field), not the plan's authoring timestamp; express the variable portion in prose to avoid the placeholder guard.

**Seams found in R3 (preflight round 2, 2026-09-12; three deltas, converged):**
- When a gate asserts "N, one more than the single occurrence P0-Tx recorded", the baseline task must actually record that count. Every clause of a delta gate needs its own positive-control count in the baseline artifact; fix the baseline task, never the gate.
- On an all-green run vstest.console prints only `Test Run Successful.`, `Total tests:`, `Passed:`, `Total time:` - no `Failed:` and no `Skipped:` line, with or without a settings file and under `dotnet-coverage collect`. Any task demanding "Total, Passed, Failed and Skipped" must say Failed/Skipped are derived when absent (Failed=0; Skipped = Total - Passed - Failed) and the artifact states read-vs-derived per value. TRX `Counters/@notExecuted` is hard-coded 0 by the TRX logger and is never the skipped count. Fix once in the environment notes, not per task.
- An opening-brace line (`{`) carries no Cobertura line element; never demand "the hits value" for it - demand hits-or-non-executable-statement, and put the positive-control expectation on the `return` line only.
- Passed-by-absence from the console logger (not in Failed list and not in Skipped list) cannot distinguish Passed from never-discovered; discovery proof must rest on a TRX executed list.

**Why:** the coverage directory contents are gitignored (`coverage/*`, line 144) but `.gitkeep` is re-included, so only subdirectories can be absent; a redirect into `coverage\logs\` before any `New-Item` fails. The first writer (P0-T12) must create `logs`; vstest creates its own `/ResultsDirectory`; `dotnet-coverage --output coverage\x.xml` needs only the parent.

**How to apply:** for any recovered or carried-forward plan, grep the plan for every line-range figure and re-derive each against the assigned worktree; check the first task that writes under a gitignored directory creates it; when a delta names a task id, grep the phrase to find where it really lives; when a test class gains a third method, replace "both" with the two FQNs; when a write-mode observation compares state of already-modified files, use content hashes; when a blame collector has HangDumpType=None, gate on the Sequence document. Related: [[project-826-factory-outside-try-reachability-seams]], [[verify-citations-in-the-assigned-worktree]], [[csharpier-format-not-pipe-files-gate]].
