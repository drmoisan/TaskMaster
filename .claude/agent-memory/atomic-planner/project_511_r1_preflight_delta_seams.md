---
name: project-511-r1-preflight-delta-seams
description: "#511 R1 revision lessons: mid-cycle raw-evidence deletion breaks resolves-to-existing gates; git-log scan legs must run post-commit; absolute MSBuild path under pwsh -NoProfile; detached Start-Process mechanic for ~20-min runs; per-class dotnet-coverage noise tolerance -0.50pp"
metadata:
  type: project
---

Eight blocking + seven warning preflight deltas on the #511 remediation plan (2026-08-23), most of
them generalizable planner traps:

1. **Raw evidence deleted between plan-writing and execution invalidates every
   "cited path resolves to an existing file" gate.** When a maintainer deletes raw TRX/.coverage
   mid-cycle, sweep the whole plan for citations of the deleted directories (`p4-t2/` appeared in a
   check-off task, a rationale task, AND prose); re-point gates at the distilled Markdown records
   plus the disposition record, and mark prose mentions "named in prose only, must not be asserted
   to exist". Reword prospective deletion clauses into the past tense.
2. **A `git log --format=%B $MergeBase..HEAD` closing-keyword scan placed before the plan's commit
   tasks can never fail** (0 messages from this plan are visible yet). Put the git-log leg in the
   commit task itself (post-commit) and repeat it after any later commit; keep only file legs in the
   pre-commit handoff index. Cousin of [[diff-gates-need-a-commit-task]].
3. **Bare `msbuild` does not resolve under `pwsh -NoProfile`** (no VS developer environment;
   `command -v msbuild` empty). Use the verified absolute path
   `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe` via `&`,
   and require each artifact to record the resolved path. Extends
   [[project-csharp-phase0-toolchain-bootstrap]].
4. **Plans must mandate a detached-launch mechanic for long commands**: nine-assembly
   `/InIsolation` vstest and dotnet-coverage runs are ~20 min over 6,437 tests vs a 600,000 ms tool
   ceiling. Phase preamble must require `Start-Process -PassThru` + `-RedirectStandardOutput/-Error`
   to a log, PID recorded, log polled with short waits, `EXIT_CODE:` from the process object's
   `ExitCode`, and kill-the-whole-tree (pwsh runner + testhost + vstest.console + dotnet-coverage)
   before any retry.
5. **Per-class `dotnet-coverage` line-rates are not bit-stable.** On a comments-only diff, keep the
   package-level delta gate strict `>= 0` but allow per-class deltas `>= -0.50` percentage points,
   recorded as measurement noise with both raw rates cited. A no-tolerance per-class `>= 0` gate
   fails for measurement reasons.
6. **A coverage-run escape hatch must be bounded and signature-scoped**: the baseline records
   Invoke-MSTestWithCoverage failing once under load with 60,000 ms `PumpTimeoutMs` expiries (#592,
   out of scope) and throwing before post-processing. Authorize re-running that one task up to twice
   on that signature only, logging each attempt + machine load; any other failure restarts the loop.
7. **Verify-don't-recreate an existing `.gitignore`**: the dictated create-with-exactly-N-lines task
   would have deleted `Deploy_*/` — the vstest deployment scratch dir whose default name embeds
   account+host (a host-identifier leak, see [[_shared_no_absolute_host_paths]]). Convert to
   verify-lines + append-only.
8. **G6 line-wrap fix by shortening the literal to the head fragment**: the spec sentence wrapped at
   `...record the` / `number here.`, so assert `File this as its own issue and record the` (1 line
   today, 0 after the edit) instead of the full sentence (0 both ways).

**How to apply:** treat 1-4 as a standing sweep whenever raw artifacts were disposed mid-cycle or a
plan carries msbuild/vstest commands; 5-6 whenever a coverage-delta gate rides a comments-only diff.
