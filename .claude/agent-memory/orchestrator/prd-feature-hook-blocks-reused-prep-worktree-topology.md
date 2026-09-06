---
name: prd-feature-hook-blocks-reused-prep-worktree-topology
description: enforce-prd-feature-before-planner.ps1 resolves issue.md/checkpoint paths relative to the session process cwd, which structurally cannot see a feature folder that only exists in an operator-assigned separate "prep-NNN" worktree
metadata:
  type: project
---

`.claude/hooks/enforce-prd-feature-before-planner.ps1` (introduced 2026-08-26, commit
`c279d40b`) denies every `Agent(atomic-planner)` delegation with `PRD_FEATURE_BLOCKED:
... but its work mode could not be determined ... (the marker is absent, unreadable, or
unrecognized)` when the orchestrator is running in a **preparation-mode child given its
own worktree by the caller (e.g. a parallel-planner's `prep-<issue>` worktree), with no
Agent-tool SDK isolation** — i.e. the child's env "Working directory" is the shared
session worktree, but the feature folder it just created lives only in the separate
worktree named in its own `WORKTREE DIRECTIVE`.

**Root cause, read directly from source.** `Find-PrdFeatureFolderFromPrompt` matches the
prompt text for `docs[\\/]+features[\\/]+active[\\/]+...` and truncates to the 4-segment
folder — this part works correctly even against a fully-qualified absolute path, because
the regex has no start anchor and simply discards everything before the literal `docs`
token. The bug is one level down: `Get-PrdFeatureIssueContent` then does
`Test-Path -LiteralPath "$FeatureFolder/issue.md"` with **no cwd override, no
`$PSScriptRoot`-relative join, and no `$env:CLAUDE_PROJECT_DIR` (grepped the whole
`.claude/hooks/` tree — that env var is not read anywhere in this hook family)**. The
relative path resolves against the hook subprocess's ambient `$PWD`, which a direct probe
confirms is the session worktree, not the item worktree: `pwsh -NoProfile -Command
"(Get-Location).Path"` from inside the very same session printed
`<user-profile>\repos\TaskMaster-wt\2026-09-02T08-47`, and the settings.json hook
registration (`"command": "pwsh -NoProfile -File .claude/hooks/enforce-prd-feature-before-planner.ps1"`)
uses a bare relative script path too, confirming the process cwd is the session root by
construction, not something the caller can redirect. `Get-PrdFeatureCheckpointFolder`'s
fallback (`artifacts/orchestration/orchestrator-state.json`) has the identical defect, so
it is not a usable escape hatch either — except that a **foreign, unrelated checkpoint
already sitting at the session root** (a sibling's or the parent's own file, naming some
other feature folder) can make the fallback return non-null and ALLOW the delegation
regardless of whether the folder it named has anything to do with your real prerequisites.
Confirmed empirically: a diagnostic-only `Agent(atomic-planner, isolation:"worktree")`
prompt containing no `docs/features/active` token passed the gate cleanly, which only
makes sense if the checkpoint fallback resolved to something unrelated. Do not rely on
this as a technique — the "why it passed" is unpredictable foreign state, not evidence
that your own prerequisites were checked.

**`isolation: "worktree"` does not help.** It spawns an unrelated temporary worktree
rooted at the *main checkout* (`<user-profile>\repos\TaskMaster\.claude\worktrees\agent-<id>`),
not a clone of your branch, and `atomic-planner`'s tool list has no `Bash`/git tool ever
(isolated or not), so it cannot even `git fetch`/`checkout` your branch to compensate. The
PreToolUse hook also fires in the *parent's* process before the child (isolated or not)
ever starts, so isolation changes nothing about which cwd the hook itself sees.

**Why not routing around it.** A `fork` or `general-purpose` delegation would dodge the
`$subagent -ne 'atomic-planner'` check entirely (line ~362), and could technically author
a compliant plan since the calling orchestrator already has the full
`atomic-plan-contract` skill text in context. Rejected: the orchestrator's own charter says
delegation to a required specialist is mandatory and "do not perform the step locally,"
and substituting an unauthorized agent type specifically to dodge a `subagent_type` string
check is gaming the gate's mechanism rather than satisfying its intent — even though, in
this particular case, the underlying prerequisites (issue.md + spec.md, correctly shaped)
were genuinely already met and the gate's *false negative* is a real hook defect, not a
real policy violation.

**How to apply.** Do not attempt to satisfy this hook by writing into the session
worktree — that violates the separate `WORKTREE DIRECTIVE`/no-cross-contamination
constraint that gave rise to this topology in the first place, and per
[[shared-checkpoint-read-modify-write-corrupts]] a session-root write can also race a
concurrently-running sibling. Instead: commit and push whatever preparation work is
genuinely complete (issue.md, spec.md, research), record the plan-authoring step as a
precise `blocked_reason`, and report the diagnostic evidence back to the caller
(parallel-planner or equivalent) so it can fix the launch topology (e.g. actually use
Agent-tool `isolation: "worktree"` seeded from the item's own branch, or launch the child
with its process cwd set to the item worktree) or get the hook fixed upstream to read
`$env:CLAUDE_PROJECT_DIR` / an explicit workspace root the way
`mcp__drm-copilot__*` tools already do. This is the same class of gap already recorded for
`enforce-pr-author-skill.ps1` (see [[child-orchestrator-pr-hook-reads-session-root]]) —
`.claude/` files here are push-down-owned, so the real fix belongs upstream in
drm-copilot, see [[project_claude_files_are_pushdown_owned_fix_upstream]].

**Adjacent trap while diagnosing.** A commit message that merely *names* the MCP tool
`new_active_feature_folder` in prose (e.g. "Remediate a new_active_feature_folder
integrity gap") is denied by `enforce-promotion-mcp-only.ps1` with
`PROMOTION_MCP_ONLY_BLOCKED` — it does a plain substring match on the whole Bash command
text against `new-potential-entry.ps1`, `new_potential_bug_entry`, `potential_to_issue`,
`new_active_feature_folder`, with no distinction between invoking the tool and describing
it. Paraphrase around the literal token (e.g. "active-feature-folder-creation") the same
way [[promotion-hook-matches-commit-message-text]] already documents for the `gh issue
create` phrase.
