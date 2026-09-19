# P0-T4 — PowerShell Batch-Budget State

Timestamp: 2026-09-19T12-22

Command:
```
ls -la <execution worktree>/.claude/state/
ls -la <session worktree>/.claude/state/
git -C <execution worktree> ls-files .claude/state/
git -C <execution worktree> check-ignore -v .claude/state/powershell-batch-budget.default.json
cat <session worktree>/.claude/state/current-session-id
echo $CLAUDE_SESSION_ID ; echo $CLAUDE_POWERSHELL_BUDGET_PROD ; echo $CLAUDE_POWERSHELL_BUDGET_TEST
```
plus a read of `.claude/hooks/enforce-powershell-batch-budget.ps1` lines 55-93, 111-174, 255-307
and 309-388.

EXIT_CODE: 0

## The exact state-file path the hook will use

```
.claude/state/powershell-batch-budget.4b68295b-3901-4320-add3-cde634c30dae.json
```

Derivation, from `Get-PowerShellBatchBudgetSessionId` (hook lines 111-174) and
`Invoke-PowerShellBatchBudgetHook` (lines 352-366):

1. The session id is the first non-empty of the explicit `-SessionId` argument, `$env:CLAUDE_SESSION_ID`,
   the contents of `<Root>/.claude/state/current-session-id`, and a worktree-derived
   `worktree-<leaf>-<sha8>` fallback.
2. `$env:CLAUDE_SESSION_ID` is set to `4b68295b-3901-4320-add3-cde634c30dae` and wins at step 2, so
   neither the session-id file nor the worktree fallback is consulted. The session-id file in the
   session worktree holds the identical value `4b68295b-3901-4320-add3-cde634c30dae`, so the two
   agree and the file name is the same under either resolution.
3. `ConvertTo-PowerShellBatchBudgetSafeSegment` replaces every character outside `[A-Za-z0-9._-]`.
   The id is hexadecimal digits and hyphens only, so it passes through unchanged.
4. The file name is `powershell-batch-budget.$resolvedSessionId.json` inside
   `Join-Path $Root '.claude/state'`.

**Directory.** `$Root` defaults to `Split-Path (Split-Path $PSScriptRoot -Parent) -Parent`, the
worktree containing the hook script that executes. `.claude/settings.json:144` registers the hook as
`pwsh -NoProfile -File .claude/hooks/enforce-powershell-batch-budget.ps1` — a **relative** path,
resolved against the hook process's working directory, which is the Claude Code project directory
`C:\Users\DanMoisan\repos\TaskMaster-wt\2026-09-12T10-15` (the session worktree), not the execution
worktree. The resolved absolute state-file path is therefore

```
C:\Users\DanMoisan\repos\TaskMaster-wt\2026-09-12T10-15\.claude\state\powershell-batch-budget.4b68295b-3901-4320-add3-cde634c30dae.json
```

The file name is certain. The directory is inferred from the relative hook registration rather than
observed, because no PowerShell file has been written in this session and the hook has therefore not
yet run. The two candidate directories are the two worktrees' `.claude/state/`; **neither contains a
file of that name**, so the starting counts below are the same under either resolution.

## Starting slot counts

| Slot kind | Starting count | Cap |
|---|---|---|
| Production (`prodFiles`) | **0** | 3 |
| Test (`testFiles`) | **0** | 3 |

Both counts are 0 because no `powershell-batch-budget.4b68295b-3901-4320-add3-cde634c30dae.json`
exists in either candidate directory. `Invoke-PowerShellBatchBudgetHook` line 367 initialises the
state to empty arrays and only rehydrates from disk when the per-session file exists.

Caps are 3 and 3, the `Invoke-PowerShellBatchBudgetHook` parameter defaults (lines 316-317).
`CLAUDE_POWERSHELL_BUDGET_PROD` and `CLAUDE_POWERSHELL_BUDGET_TEST` are both unset, and raising
either is not authorised by this plan.

## The stale `default` state file is not this session's file

`.claude/state/powershell-batch-budget.default.json` exists in **both** worktrees, is **tracked in
git** (`git ls-files` lists it; `git check-ignore` exits 1), and carries `prodFiles` already at 3 of
3. It is not the file this session uses: the session-id segment is the GUID above, not `default`.

It would also be harmless if it were. Its three entries are
`C:/Users/DANMOI~1/AppData/Local/Temp/claude/…/scratchpad/run-vstest.ps1`,
`…/postrebase_verify.ps1` and `…/run-toolchain-442.ps1`, all under the system temp tree.
`ConvertTo-PowerShellBatchBudgetState` (lines 218-225) drops every persisted entry failing
`Test-PowerShellBatchBudgetPathInRoot` against the current root, and all three fail it, so they
would rehydrate to an empty array rather than to a full batch.

## Path-storage form (required record)

The hook stores the **absolute `file_path` the `Write` or `Edit` tool supplied, with backslashes
normalised to forward slashes**, and nothing else:

- hook line 347, `$normalized = $filePath -replace '\\', '/'`;
- hook lines 300-304, `$State.prodFiles = @($State.prodFiles) + @($normalized)` and the test-list
  equivalent.

No repo-relative reduction is performed. Every later boundary assertion in this plan
(P2-T9, P4-T8, P6-T7) must therefore compare path **suffixes**, never repo-relative equality.

The production-versus-test split is by path shape, hook line 284: a candidate matching
`(^|/)tests/.*\.ps1$` or `\.Tests\.ps1$` is a test file, everything else is production.

## Forward risk recorded at the point it was measured (not a Phase 0 failure)

`Invoke-PowerShellBatchBudgetDecision` lines 277-282 discard an out-of-root candidate rather than
denying it: the decision is `allow`, **no slot is consumed and no state is written**. The
containment test at lines 82-92 admits a relative path unconditionally but requires an absolute path
to be equal to, or prefixed by, the root.

Every PowerShell file this plan creates lives under the **execution** worktree
`C:\Users\DanMoisan\repos\TaskMaster-wt\dependabot-911`, and the `Write` tool supplies an absolute
path. If `$Root` is the session worktree as derived above, every one of those writes is out-of-root,
is discarded, consumes no slot, and causes no state file to be created at all.

The consequence is for Phases 2, 4 and 6, not for Phase 0. Scope Decision 4 states that a file
written by a heredoc "never appears in `prodFiles` or `testFiles`, and the boundary assertions at
P2-T9, P4-T8 and P6-T7 — which read those arrays — become unsatisfiable", and prescribes the `Write`
and `Edit` tools as the remedy. The measurement above indicates the remedy is insufficient on its
own: with the hook rooted at the session worktree, a `Write`-tool write into the execution worktree
produces the same empty arrays as a heredoc would, so those three boundary assertions would read an
absent or empty state file and could not fail. That is the absence-shaped shape gate rule 2
prohibits.

This is recorded, not acted on. The executor is not authorised to amend the plan, and no Phase 0
task depends on the outcome. It is reported to the coordinator for resolution before Phase 2 runs.
The directory inference should be confirmed empirically at the first `Write` of a PowerShell file
(P1-T4): if no `powershell-batch-budget.4b68295b-…json` appears in either `.claude/state/`
directory after that write, the out-of-root discard is confirmed.

Output Summary: State file the hook will use is
`.claude/state/powershell-batch-budget.4b68295b-3901-4320-add3-cde634c30dae.json`, resolved from
`$env:CLAUDE_SESSION_ID`, most probably under the session worktree
`C:\Users\DanMoisan\repos\TaskMaster-wt\2026-09-12T10-15`. It does not exist in either candidate
directory, so starting counts are production **0** of 3 and test **0** of 3. The hook records the
absolute supplied `file_path` with backslashes normalised to forward slashes, so later boundary
assertions must compare suffixes. The tracked `powershell-batch-budget.default.json` is a different
session's file and its three temp-path entries would be dropped by the containment filter in any
case. Forward risk recorded: out-of-root candidates are discarded without consuming a slot or
writing state, which would leave the P2-T9, P4-T8 and P6-T7 boundary assertions unable to fail.
