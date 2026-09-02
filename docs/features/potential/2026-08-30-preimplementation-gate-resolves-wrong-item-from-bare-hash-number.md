# Preimplementation gate resolves the wrong parallel item from a bare hash-number in prose

- Type: bug
- Surface: `.claude/hooks/enforce-orchestration-preimplementation-gate-modes.ps1` (push-down-owned; fix upstream in drm-copilot)
- Observed: 2026-08-30, parallel run `bugs-638-644-647`, relaunching item 644

## Symptom

An `Agent(orchestrator)` delegation for item 644 was denied with:

```
PREIMPLEMENTATION_GATE_BLOCKED: this parallel-mode delegation was evaluated against
artifacts/orchestration/parallel-orchestrator-state.json, and the failed readiness
predicate is 'merge_status'.
```

The checkpoint was correct: item 644 carried `merge_status: not_started`, which is not terminal. The
deny message points the operator at the checkpoint, where nothing is wrong.

## Root cause

Two mechanisms compose.

1. `Find-OrchestrationDelegationIssueNumber` matches the keyed form with the regex
   `issue[_-]?num(?:ber)?\s*[:=]\s*#?(\d+)`. The separator class is `[_-]` only, so the natural
   English spelling `Issue number: 644` — with a space — does **not** match. The function then falls
   back to a bare `#(\d+)` scan over the entire prompt and returns the first hash-number found. On a
   parallel run that is typically a sibling item named in a justification sentence.

2. `Find-OrchestrationModeRecord` iterates `items[]` in array order and tests `TargetFolder` then
   `IssueNumber` **per record**:

   ```powershell
   foreach ($record in @($Records)) {
       if ($TargetFolder) { ...; if ($basename -eq $TargetFolder) { return $record } }
       if ($IssueNumber)  { ...; if ($issue -eq $IssueNumber)     { return $record } }
   }
   ```

   A wrong issue number matching an **earlier** record therefore wins over a correct
   `feature_folder` match on a **later** record. The function's own docstring states it matches
   "the normalized feature_folder basename first and issue_num second", which holds only within a
   single record, not across the collection.

In the observed case the prompt carried `Issue number: 644.` and the phrase `#638`. The extractor
returned `638`; item 638 sits first in `items[]` and had the terminal `merge_status:
worktree_removed`; the gate denied.

## Why it matters

The failure is silent and misdirecting. The named predicate (`merge_status`) belongs to a record the
operator never intended to reference, so the natural response is to edit a correct checkpoint. The
deny is also order-dependent: the same prompt against the same run passes once the already-merged
item is no longer first in `items[]`.

This is the same defect family as the already-promoted
`docs/features/potential/promoted/2026-08-29-parallel-run-merge-gate-misparses-pr-number.md`, in
which `enforce-epic-merge-gate.ps1` scans the whole command string for a digit run and binds it to
the wrong record.

## Suggested fix

1. Widen the keyed separator to accept whitespace: `issue[_\s-]?num(?:ber)?`.
2. Resolve `TargetFolder` across **all** records before falling back to `IssueNumber` across all
   records, so the documented precedence holds at collection level. Correct the docstring either way.
3. Consider making the deny message name the record it actually resolved (issue number and
   `merge_status`), so a misresolution is visible without dot-sourcing the hook.

## Workaround

Write the item key as `issue_num: <N>.` in the delegation prompt, and never write a bare `#<N>`
referring to another item of the run.
