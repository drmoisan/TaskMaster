# Production Anchor (P0-T3, P0-T4)

## P0-T3 — base anchor of the branch

Timestamp: 2026-10-02T22-39
Task: P0-T3
Command: git rev-parse HEAD; git rev-parse origin/main; git merge-base 94287369908cc920b21b0e3256314f988ad7d2f5 HEAD; git diff --exit-code --stat 94287369908cc920b21b0e3256314f988ad7d2f5 -- TaskMaster TaskMaster.Test
EXIT_CODE: 0

Output Summary:
- MERGE-BASE: 94287369908cc920b21b0e3256314f988ad7d2f5 (equals BASE-SHA; BASE is an ancestor of HEAD).
- ANCHOR-CODE-DIFF-EXIT=0 (the diff printed nothing: TaskMaster/ and TaskMaster.Test/ equal BASE-SHA).
- HEAD: 98934d356e6ab39167e3b56edc9a2aa78e60795b
- ORIGIN-MAIN: 993fdd01566dee82e5f37acb761a600feaaa1454
- ORIGIN-MAIN MOVED (origin/main differs from BASE-SHA; recorded without stopping, because every anchor in the plan is the BASE-SHA literal except the MERGE-SHA anchor of P2-T8).
- Verdict: PASS (no BASE NOT ANCESTOR, no CODE DIFFERS FROM BASE).

## P0-T4 — first attempt (INCOMPLETE: blocked by a hook; superseded by the completed run below)

Timestamp: 2026-10-02T22-41
Task: P0-T4
Command: Read tool over TaskMaster/Ribbon/EngineToggleStateCoordinator.cs lines 1-10, 43-55, 253-272, 403-407, 433-442, 492-496; CMD-STRIPPED-COUNT with TOKENS-STRUCT; CMD-PHRASE-COUNT with PHRASES-DOC (blocked); Grep count of `^`
EXIT_CODE: BLOCKED

Output Summary:
- Grep line count: 496 (matches).
- Split anchor lines: all match fact 2 (no SPLIT ANCHOR MOVED).
- CMD-STRIPPED-COUNT: all 16 STRIPPED values equal their TOKENS-STRUCT base values.
- CMD-PHRASE-COUNT: NOT RUN. The PreToolUse Bash hook denied the payload before execution (verbatim message under Details). Per the binding instructions the command was not rephrased or restructured.
- Task status: unchecked; plan execution stopped at P0-T4.

Details:

Anchor lines observed (Read tool):
- 3 `using System.Globalization;`
- 4 `using System.Threading;`
- 45 `    internal sealed class EngineToggleStateCoordinator`
- 46 `    {`
- 47 `        /// <summary>`
- 51 `        private const string NullEngineNameToken = "(null)";`
- 52 (blank)
- 53 `        private readonly Func<IAppItemEngines> _enginesAccessor;`
- 255 `        }`
- 256 (blank)
- 257 `        /// <summary>`
- 270 `        internal Task GetPrimeTask(string engineName)`
- 405 `        private void CompletePrime(Task completed, string engineName)`
- 435 `        }`
- 436 (blank)
- 437 `        /// <summary>`
- 440 `        private static string RenderEngineName(string engineName)`
- 494 `        }`
- 495 `    }`
- 496 `}`

STRIPPED rows (observed; base value per TOKENS-STRUCT in parentheses):
- STRIPPED [catch(] = 3 (3)
- STRIPPED [catch(Exceptionex)] = 1 (1)
- STRIPPED [catch(Exception)] = 2 (2)
- STRIPPED [TryInvokeSink(] = 0 (0)
- STRIPPED [_logError(] = 2 (2)
- STRIPPED [_notifyUnavailable(] = 1 (1)
- STRIPPED [TryInvokeSink(()=>_notifyUnavailable(BuildUnavailableMessage(engineName)),outvarnotifyFailure)] = 0 (0)
- STRIPPED [TryInvokeSink(()=>_logError(BuildNotifyFailedMessage(engineName),notifyFailure),out_)] = 0 (0)
- STRIPPED [TryInvokeSink(()=>_logError(BuildToggleFailedMessage(engineName),ex),out_)] = 0 (0)
- STRIPPED [failure),out_)){_reportedPrimeFaults[reportKey]=0;}] = 0 (0)
- STRIPPED [_reportedPrimeFaults[reportKey]=0;] = 1 (1)
- STRIPPED [privatestaticboolTryInvokeSink(ActionsinkCall,outExceptionsinkFailure)] = 0 (0)
- STRIPPED [privatestaticstringBuildNotifyFailedMessage(stringengineName)] = 0 (0)
- STRIPPED [internalsealedclassEngineToggleStateCoordinator] = 1 (1)
- STRIPPED [internalsealedpartialclassEngineToggleStateCoordinator] = 0 (0)
- STRIPPED [_primeTasks.TryRemove(engineName,out_);] = 1 (1)

HOOK BLOCK (verbatim, CMD-PHRASE-COUNT payload with PHRASES-DOC substituted):
```
PreToolUse:Bash hook error: PROMOTION_MCP_ONLY_BLOCKED: Direct GitHub issue creation via `gh` bypasses the approved drm-copilot MCP promotion path (`mcp__drm-copilot__new_potential_entry` -> `mcp__drm-copilot__potential_to_issue` -> `mcp__drm-copilot__new_active_feature_folder`). Use those MCP tools instead.
```
The message is the gh-issue-creation reason of `.claude/hooks/enforce-promotion-mcp-only.ps1` (emitted for `gh issue create`, `gh issue new`, or `gh api` against an issues endpoint with an explicit POST). The payload contains none of those command forms and invokes no `gh`; it does contain the word `issue` (phrase `The call is guarded (issue #964)`), the word `New-Object`, and the letter sequence `gh` inside `through`. The CMD-STRIPPED-COUNT payload, which carries no `issue` text, was allowed. Which scanner path matched is not determined; the block appears to be a false positive of the hook's command scanner on this payload text.

## P0-T4 — split anchors and false-before values (completed run)

Timestamp: 2026-10-03T07-34
Task: P0-T4
Command: Read tool over TaskMaster/Ribbon/EngineToggleStateCoordinator.cs lines 1-10, 43-55, 253-272, 403-407, 433-442, 492-496; CMD-STRIPPED-COUNT with TOKENS-STRUCT; CMD-PHRASE-COUNT with PHRASES-DOC (coordinator-run, see below); Grep tool count of pattern `^` over the file
EXIT_CODE: 0

Output Summary:
- Pre-record checks: `git rev-parse HEAD` = 0561003e95a67b954fea2106abf7432cd2e61a13; `git diff --exit-code --stat 0561003e95a67b954fea2106abf7432cd2e61a13 -- TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` exited 0 and printed nothing (file unchanged against that commit).
- Grep line count: 496 (matches).
- Split anchor lines: all 20 cited lines match fact 2 (no SPLIT ANCHOR MOVED).
- CMD-STRIPPED-COUNT (executor-run): all 16 STRIPPED values equal their TOKENS-STRUCT base values.
- CMD-PHRASE-COUNT: all 24 PHRASE values equal their PHRASES-DOC base values (coordinator-run under the maintainer one-time bypass; rows below).
- Verdict: PASS (no SPLIT ANCHOR MOVED, no BASELINE TOKEN MISMATCH). These values are the false-before half of the P1-T22 gate.

Details:

Anchor lines observed (Read tool, this run):
- 3 `using System.Globalization;`
- 4 `using System.Threading;`
- 45 `    internal sealed class EngineToggleStateCoordinator`
- 46 `    {`
- 47 `        /// <summary>`
- 51 `        private const string NullEngineNameToken = "(null)";`
- 52 (blank)
- 53 `        private readonly Func<IAppItemEngines> _enginesAccessor;`
- 255 `        }`
- 256 (blank)
- 257 `        /// <summary>`
- 270 `        internal Task GetPrimeTask(string engineName)`
- 405 `        private void CompletePrime(Task completed, string engineName)`
- 435 `        }`
- 436 (blank)
- 437 `        /// <summary>`
- 440 `        private static string RenderEngineName(string engineName)`
- 494 `        }`
- 495 `    }`
- 496 `}`

STRIPPED rows (executor-run this pass; base value per TOKENS-STRUCT in parentheses):
- STRIPPED [catch(] = 3 (3)
- STRIPPED [catch(Exceptionex)] = 1 (1)
- STRIPPED [catch(Exception)] = 2 (2)
- STRIPPED [TryInvokeSink(] = 0 (0)
- STRIPPED [_logError(] = 2 (2)
- STRIPPED [_notifyUnavailable(] = 1 (1)
- STRIPPED [TryInvokeSink(()=>_notifyUnavailable(BuildUnavailableMessage(engineName)),outvarnotifyFailure)] = 0 (0)
- STRIPPED [TryInvokeSink(()=>_logError(BuildNotifyFailedMessage(engineName),notifyFailure),out_)] = 0 (0)
- STRIPPED [TryInvokeSink(()=>_logError(BuildToggleFailedMessage(engineName),ex),out_)] = 0 (0)
- STRIPPED [failure),out_)){_reportedPrimeFaults[reportKey]=0;}] = 0 (0)
- STRIPPED [_reportedPrimeFaults[reportKey]=0;] = 1 (1)
- STRIPPED [privatestaticboolTryInvokeSink(ActionsinkCall,outExceptionsinkFailure)] = 0 (0)
- STRIPPED [privatestaticstringBuildNotifyFailedMessage(stringengineName)] = 0 (0)
- STRIPPED [internalsealedclassEngineToggleStateCoordinator] = 1 (1)
- STRIPPED [internalsealedpartialclassEngineToggleStateCoordinator] = 0 (0)
- STRIPPED [_primeTasks.TryRemove(engineName,out_);] = 1 (1)

PHRASE rows: coordinator-run under the maintainer one-time bypass of enforce-promotion-mcp-only.ps1 (2026-10-03), worktree agent-a3fb26aa2afc7c52c, HEAD 0561003e9, PowerShell tool (PRELUDE plus CMD-PHRASE-COUNT with PHRASES-DOC; base value per PHRASES-DOC in parentheses):
- PHRASE [The in-flight] = 1 (1)
- PHRASE [most recently completed] = 1 (1)
- PHRASE [The prime task, or] = 1 (1)
- PHRASE [The registration marker for an engine key] = 0 (0)
- PHRASE [The marker is not the prime task itself] = 0 (0)
- PHRASE [The registered marker, or] = 0 (0)
- PHRASE [The other two are sink guards] = 1 (1)
- PHRASE [which holds the only other <c>catch</c> clause] = 0 (0)
- PHRASE [the only <c>catch</c> clause in this type that observes an engine fault] = 1 (1)
- PHRASE [The three] = 1 (1)
- PHRASE [all sit in] = 1 (1)
- PHRASE [The two <c>catch</c> clauses in this type are the click boundary] = 0 (0)
- PHRASE [also contains a failure of the sink] = 1 (1)
- PHRASE [routes its sink call through] = 0 (0)
- PHRASE [a sink failure is contained here] = 1 (1)
- PHRASE [a sink failure is contained by] = 0 (0)
- PHRASE [by the statement directly after the sink call] = 1 (1)
- PHRASE [by the only statement of the branch taken when] = 0 (0)
- PHRASE [never throws, even when the sink throws] = 1 (1)
- PHRASE [never throws on either path, even when both sinks throw] = 0 (0)
- PHRASE [honours its non-throwing precondition] = 0 (0)
- PHRASE [and must not throw] = 0 (0)
- PHRASE [The call is guarded (issue #964)] = 0 (0)
- PHRASE [Receives an observed prime fault, toggle fault or notification failure] = 0 (0)
