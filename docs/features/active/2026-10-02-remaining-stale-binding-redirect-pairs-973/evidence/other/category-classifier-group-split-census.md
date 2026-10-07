# P3-T24 split census of CategoryClassifierGroup (issue #973; AC23)

Timestamp: 2026-10-06T18-19
Command: CMD-VERBATIM-MOVE (pwsh -NoProfile -Command 'Set-Location -LiteralPath "<execution-worktree-root>"; ... git -C $repo show ("a6915d62fe9d85218e5453fc5ac5cd5674b04984:" + $rel) ... git -C $repo diff -U0 a6915d62fe9d85218e5453fc5ac5cd5674b04984 -- $rel ... $removed ... Compare-Object ... -CaseSensitive ...', issued exactly as section 7 defines it with PLAN-START-HEAD substituted; coordinator-run under the maintainer standing approval of 2026-10-04, see section (iii)); CMD-MEMBER-CENSUS over both files; Greps for the partial declaration, the base list and the namespace in both files; CMD-HUNKS for the original (git -C <execution-worktree-root> diff -U0 a6915d62fe9d85218e5453fc5ac5cd5674b04984 -- UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.cs); the csproj hunks and numstat from P3-T22 (no edit since)
EXIT_CODE: 0
Output Summary: PASS. CMD-VERBATIM-MOVE (coordinator-run at HEAD d506eb11f2f3d6e32f9c7fbf82650d206708bf71, the item HEAD) printed REGION-BEFORE-LINES 93, REGION-AFTER-LINES 93, REGION-DIFFERENCES 0, REMOVED-LINES 98, ADDED-LINES 1 (the partial declaration), REMOVED-USING-LINES 2, REMOVED-OTHER-LINES 95, BODY-LINES 93, REMOVED-OTHER-TRIMMED 93, BODY-TRIMMED 93, MOVE-DIFFERENCES 0, pwsh exit 0. Line counts 442/442 and 106/106 (re-taken 2026-10-06), both under 500; member census 24 + 12 = 36 matching the P3-T19 list (35 distinct); three hunks in the original; two hunks and 10/0 in the csproj. Every acceptance clause holds.

## Execution history

First attempt (2026-10-03T11-41, previous executor run): STOP: HOOK-BLOCKED; P3-T24 left unchecked. Second attempt (2026-10-06, this run, the command issued exactly once, unchanged): refused by a different PreToolUse Bash hook, verbatim:

HOOK-BLOCKED (2026-10-06): PreToolUse:Bash hook error: EPIC_WORKTREE_REMOVAL_BLOCKED: TARGET_WORKTREE_NOT_DERIVABLE: the command names no usable worktree_path, so the epic run it belongs to cannot be identified. EPIC_WORKTREE_REMOVAL_BLOCKED: git worktree remove for '' requires either an epic checkpoint features[] record with merge_status in {merged, worktree_removed}, or a parallel-orchestrator checkpoint with route_id == "parallel" whose matching items[] record (matched by worktree_path) has merge_status in {merged, worktree_removed}. No checkpoint authorized this removal.

Both refusals are approval-(a) false positives (the payload runs no git worktree removal). The payload was relayed unchanged as request.973-P3-T24 and run by the coordinator; the output is in section (iii).

HOOK-BLOCKED (2026-10-03): PreToolUse:Bash hook error: PARALLEL_WORKTREE_REMOVAL_BLOCKED: TARGET_WORKTREE_NOT_DERIVABLE: the command names no usable worktree_path, so the parallel run it belongs to cannot be identified. PARALLEL_WORKTREE_REMOVAL_BLOCKED: git worktree remove for '' requires a matching parallel checkpoint items[] record with merge_status in {merged, worktree_removed}. The checkpoint was unreadable, no matching record was found, or merge_status was not yet safe for removal.

Assessment (not a workaround): the payload contains no `git worktree remove` command. It contains the token `git` (in `git -C $repo show` and `git -C $repo diff`) together with the variable names `$removed`, `$removedUsing`, `$removedOther` and `$removedTrimmed` that CMD-VERBATIM-MOVE defines; the parallel-worktree-removal guard appears to match on `git` plus a `remove` substring anywhere in the command string. This is a false positive of that guard against a plan-mandated read-only payload. Resolution requires an orchestrator or planner ruling (for example a plan revision that renames the payload variables, or a guard fix upstream); the executor does not apply either.

## (i) Line counts

LINES-BEFORE: 539 (P0-T20)
LINES-BEFORE-PART-H: 537 (P3-T19)
LINES-AFTER-ORIGINAL: 442 (CRCOUNT 442; P3-T21, no edit since; P3-T23 format rewrote nothing)
LINES-AFTER-NEW: 106 (CRCOUNT 106; P3-T20, no edit since)
Both under 500 with equal carriage-return counts.

## (ii) Member census

MEMBER-COUNT-BEFORE: 36 (P3-T19 list)
MEMBER-COUNT-AFTER: 36 (original 24 + new file 12, CMD-MEMBER-CENSUS over both files)
The original's 24 lines are the P3-T19 entries outside the region and the new file's 12 lines are the P3-T19 entries inside it (P3-T20 and P3-T21 artifacts list them), so the multisets are equal:
MEMBER-ONLY-BEFORE: none
MEMBER-ONLY-AFTER: none
MEMBER-DISTINCT-BEFORE: 35
MEMBER-DISTINCT-AFTER: 35
MEMBER-DUPLICATE: private async Task<BayesianClassifierGroup> LoadClassifierGroup( x2

## (iii) CMD-VERBATIM-MOVE

Coordinator-run under the maintainer standing approval of 2026-10-04. Source: relay response file response.973-P3-T24.md (coordinator relay directory for run bugs-2026-09-28), quoted verbatim below. The response HEAD equals the item HEAD at request time (d506eb11f2f3d6e32f9c7fbf82650d206708bf71). The response names enforce-parallel-worktree-removal-gate.ps1 (the 2026-10-03 refusal); this run's refusal came from the epic gate; both are approval class (a).

    # Coordinator response: 973-P3-T24 (CMD-VERBATIM-MOVE)

    - Run by: coordinator, main session TM-bugs2, under the maintainer standing approval of 2026-10-04 (false positive of enforce-parallel-worktree-removal-gate.ps1; the payload runs only git show, git diff and file reads, no git worktree remove and no issue creation).
    - Payload: request.973-P3-T24.payload.txt, run unchanged in one PowerShell process (pwsh -NoProfile -Command <payload text>).
    - HEAD: d506eb11f2f3d6e32f9c7fbf82650d206708bf71
    - Timestamp: 2026-10-06T18-18
    - EXIT_CODE: 0

    ## stdout and stderr (verbatim)

    REGION-BEFORE-LINES: 93
    REGION-AFTER-LINES: 93
    REGION-DIFFERENCES: 0
    REMOVED-LINES: 98
    ADDED-LINES: 1
    ADDED:     public partial class CategoryClassifierGroup : IConditionalEngine<MailItemHelper>
    REMOVED-USING-LINES: 2
    REMOVED-OTHER-LINES: 95
    BODY-LINES: 93
    REMOVED-OTHER-TRIMMED: 93
    BODY-TRIMMED: 93
    MOVE-DIFFERENCES: 0

Every value equals the section 7 expectation.

## (iv) Hunks of the original at PLAN-START-HEAD

HUNK @@ -11,2 +10,0 @@ (the two Graph using lines)
HUNK @@ -25 +23 @@ (`    public class CategoryClassifierGroup : IConditionalEngine<MailItemHelper>` to `    public partial class CategoryClassifierGroup : IConditionalEngine<MailItemHelper>`)
HUNK @@ -443,95 +440,0 @@ (two blank lines and the 93-line region removed)
Three hunks, as stated.

## (v) UtilitiesCS/UtilitiesCS.csproj at PLAN-START-HEAD (P3-T22, no edit since)

NUMSTAT: 10	0
HUNK @@ -400,0 +401,9 @@ (the nine Part C lines: the five-line comment, the Reference start tag with Version=10.0.0.12, the HintPath, the Aliases child and the closing tag)
HUNK @@ -610,0 +620 @@ (`    <Compile Include="EmailIntelligence\ClassifierGroups\Categories\CategoryClassifierGroup.ConditionalEngine.cs" />`)

## Greps

`^    public partial class CategoryClassifierGroup`: 1 in each file
`class CategoryClassifierGroup : IConditionalEngine<MailItemHelper>`: 1 in the original, 0 in the new file
`^#nullable enable\r?$` at line 1 of the new file (P3-T20)
`^using Microsoft\.Graph` in the new file: 0 (P3-T20)
`^namespace UtilitiesCS\.EmailIntelligence\.ClassifierGroups\.Categories\r?$`: 1 in each file

RESULT: PASS (every clause of P3-T24 holds; the 2026-10-03 STOP: HOOK-BLOCKED is superseded by the coordinator-run observation of 2026-10-06)
