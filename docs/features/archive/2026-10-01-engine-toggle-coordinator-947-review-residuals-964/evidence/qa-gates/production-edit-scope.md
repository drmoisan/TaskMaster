# Production Edit Scope (P1-T21, P1-T22)

## FORMAT: (P1-T21)

Timestamp: 2026-10-03T07-51
Task: P1-T21
Command: dotnet tool run csharpier format TaskMaster/Ribbon/EngineToggleStateCoordinator.cs TaskMaster/Ribbon/EngineToggleStateCoordinator.Prime.cs TaskMaster/Ribbon/EngineToggleStateCoordinator.Messages.cs; dotnet tool run csharpier check TaskMaster/Ribbon/EngineToggleStateCoordinator.cs TaskMaster/Ribbon/EngineToggleStateCoordinator.Prime.cs TaskMaster/Ribbon/EngineToggleStateCoordinator.Messages.cs; Grep tool count over TaskMaster/Ribbon/EngineToggleStateCoordinator.Messages.cs for `private static string BuildNotifyFailedMessage\(string engineName\)`
EXIT_CODE: 0

Output Summary:
- F8 inserted directly after the closing brace of `BuildToggleFailedMessage`.
- Grep count `private static string BuildNotifyFailedMessage\(string engineName\)` in the Messages file: 1.
- Format: `Formatted 3 files in 2766ms.` exit 0 (processed count).
- Check: `Checked 3 files in 1021ms.` exit 0, no path listed.
- Verdict: PASS.

## P1-T22 — HALTED PENDING MAINTAINER APPROVAL (SUPERSEDED by the "P1-T22 — RESULTS" section below)

Timestamp: 2026-10-03T07-51
Task: P1-T22
Status: HALTED PENDING APPROVAL. No P1-T22 command (CMD-STRIPPED-COUNT, CMD-PHRASE-COUNT, CMD-PROTECTED-SPANS) has been run. Per the coordinator's binding instruction, execution stopped before P1-T22 so that a separate maintainer approval can be obtained (the CMD-PHRASE-COUNT payload with PHRASES-DOC was blocked at P0-T4 by `.claude/hooks/enforce-promotion-mcp-only.ps1`, and the 2026-10-03 one-time bypass covered P0-T4 only). The task remains unchecked in the plan; its census results will be appended to this file when it is executed.

## P1-T22 — RESULTS

Timestamp: 2026-10-03T08-05
Task: P1-T22
Command: CMD-STRIPPED-COUNT (TOKENS-STRUCT); CMD-PHRASE-COUNT (PHRASES-DOC); CMD-PROTECTED-SPANS (SIGNATURES-PROTECTED) over TaskMaster/Ribbon/EngineToggleStateCoordinator.cs, TaskMaster/Ribbon/EngineToggleStateCoordinator.Prime.cs and TaskMaster/Ribbon/EngineToggleStateCoordinator.Messages.cs
EXIT_CODE: 0

Output Summary:
- STRIPPED: 16 of 16 values equal their TOKENS-STRUCT required final values.
- PHRASE: 24 of 24 values equal their PHRASES-DOC required final values (coordinator-run rows, see below).
- SPAN-HASH: 14 of 14 lines read `equal=True` (twelve signatures and two field declarations).
- CATCH-SITES: two `catch` clauses, one in `HandleToggleClickAsync` and one in `TryInvokeSink`.
- Verdict: PASS (no EDIT SCOPE MISMATCH).

### Precondition for the coordinator-run phrase census

- `git rev-parse HEAD` printed `ca215e0683cbd3f10995272e1188e590a4405201`.
- `git diff --exit-code ca215e0683cbd3f10995272e1188e590a4405201 -- TaskMaster` printed nothing and exited 0.

### STRIPPED (CMD-STRIPPED-COUNT, executor-run; observed value, required final value)

- STRIPPED [catch(] = 2 (required 2) MATCH
- STRIPPED [catch(Exceptionex)] = 2 (required 2) MATCH
- STRIPPED [catch(Exception)] = 0 (required 0) MATCH
- STRIPPED [TryInvokeSink(] = 5 (required 5) MATCH
- STRIPPED [_logError(] = 3 (required 3) MATCH
- STRIPPED [_notifyUnavailable(] = 1 (required 1) MATCH
- STRIPPED [TryInvokeSink(()=>_notifyUnavailable(BuildUnavailableMessage(engineName)),outvarnotifyFailure)] = 1 (required 1) MATCH
- STRIPPED [TryInvokeSink(()=>_logError(BuildNotifyFailedMessage(engineName),notifyFailure),out_)] = 1 (required 1) MATCH
- STRIPPED [TryInvokeSink(()=>_logError(BuildToggleFailedMessage(engineName),ex),out_)] = 1 (required 1) MATCH
- STRIPPED [failure),out_)){_reportedPrimeFaults[reportKey]=0;}] = 1 (required 1) MATCH
- STRIPPED [_reportedPrimeFaults[reportKey]=0;] = 1 (required 1) MATCH
- STRIPPED [privatestaticboolTryInvokeSink(ActionsinkCall,outExceptionsinkFailure)] = 1 (required 1) MATCH
- STRIPPED [privatestaticstringBuildNotifyFailedMessage(stringengineName)] = 1 (required 1) MATCH
- STRIPPED [internalsealedclassEngineToggleStateCoordinator] = 0 (required 0) MATCH
- STRIPPED [internalsealedpartialclassEngineToggleStateCoordinator] = 3 (required 3) MATCH
- STRIPPED [_primeTasks.TryRemove(engineName,out_);] = 1 (required 1) MATCH

### PHRASE (CMD-PHRASE-COUNT)

Provenance: coordinator-run under the maintainer's second one-time bypass of enforce-promotion-mcp-only.ps1 (2026-10-03), worktree agent-a3fb26aa2afc7c52c, HEAD ca215e068, PowerShell tool. The executor did not run this command; the rows below are recorded verbatim as supplied, followed by the executor's comparison against the PHRASES-DOC required final value.

- PHRASE [The in-flight] = 0 (required 0) MATCH
- PHRASE [most recently completed] = 0 (required 0) MATCH
- PHRASE [The prime task, or] = 0 (required 0) MATCH
- PHRASE [The registration marker for an engine key] = 1 (required 1) MATCH
- PHRASE [The marker is not the prime task itself] = 1 (required 1) MATCH
- PHRASE [The registered marker, or] = 1 (required 1) MATCH
- PHRASE [The other two are sink guards] = 0 (required 0) MATCH
- PHRASE [which holds the only other <c>catch</c> clause] = 1 (required 1) MATCH
- PHRASE [the only <c>catch</c> clause in this type that observes an engine fault] = 1 (required 1) MATCH
- PHRASE [The three] = 0 (required 0) MATCH
- PHRASE [all sit in] = 0 (required 0) MATCH
- PHRASE [The two <c>catch</c> clauses in this type are the click boundary] = 1 (required 1) MATCH
- PHRASE [also contains a failure of the sink] = 0 (required 0) MATCH
- PHRASE [routes its sink call through] = 1 (required 1) MATCH
- PHRASE [a sink failure is contained here] = 0 (required 0) MATCH
- PHRASE [a sink failure is contained by] = 1 (required 1) MATCH
- PHRASE [by the statement directly after the sink call] = 0 (required 0) MATCH
- PHRASE [by the only statement of the branch taken when] = 1 (required 1) MATCH
- PHRASE [never throws, even when the sink throws] = 0 (required 0) MATCH
- PHRASE [never throws on either path, even when both sinks throw] = 1 (required 1) MATCH
- PHRASE [honours its non-throwing precondition] = 2 (required 2) MATCH
- PHRASE [and must not throw] = 1 (required 1) MATCH
- PHRASE [The call is guarded (issue #964)] = 1 (required 1) MATCH
- PHRASE [Receives an observed prime fault, toggle fault or notification failure] = 1 (required 1) MATCH

### SPAN-HASH (CMD-PROTECTED-SPANS, executor-run)

- SPAN-HASH [internal EngineToggleStateCoordinator(] equal=True
- SPAN-HASH [internal bool GetPressed(string engineName)] equal=True
- SPAN-HASH [internal async Task ExecuteToggleAsync(string engineName)] equal=True
- SPAN-HASH [internal Task GetPrimeTask(string engineName)] equal=True
- SPAN-HASH [private void StartPrimeIfNeeded(string engineName, string controlId)] equal=True
- SPAN-HASH [private void StartObservedPrime(] equal=True
- SPAN-HASH [private async Task ApplyPrimeAsync(] equal=True
- SPAN-HASH [private static string RenderEngineName(string engineName)] equal=True
- SPAN-HASH [private static string BuildUnavailableMessage(string engineName)] equal=True
- SPAN-HASH [private static string BuildToggleFailedMessage(string engineName)] equal=True
- SPAN-HASH [private static string BuildPrimeFailedMessage(string engineName)] equal=True
- SPAN-HASH [private static string BuildUnmappedKeyMessage(string engineName)] equal=True
- SPAN-HASH [_primeTasks = new ConcurrentDictionary<] equal=True
- SPAN-HASH [(string EngineName, Type FaultType),] equal=True

### CATCH-SITES (Read tool over the formatted TaskMaster/Ribbon/EngineToggleStateCoordinator.cs)

CATCH-SITES:
- `HandleToggleClickAsync`: `try` at line 204, `catch (Exception ex)` clause lines 208 to 211 (body routes `_logError(BuildToggleFailedMessage(engineName), ex)` through `TryInvokeSink`).
- `TryInvokeSink`: `try` at line 289, `catch (Exception ex)` clause lines 295 to 299 (assigns `sinkFailure` and returns `false`).
- No `catch` clause in TaskMaster/Ribbon/EngineToggleStateCoordinator.Prime.cs or TaskMaster/Ribbon/EngineToggleStateCoordinator.Messages.cs (only documentation mentions of `<c>catch</c>`).
