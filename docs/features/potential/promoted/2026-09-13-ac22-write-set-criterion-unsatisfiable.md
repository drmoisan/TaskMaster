# ac22-write-set-criterion-unsatisfiable (Issue #885)

- Date captured: 2026-09-13
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/ac22-write-set-criterion-unsatisfiable/ (Issue #885)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #885
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/885
- Last Updated: 2026-09-13
## Summary

The "AC22 Write Set" acceptance-criterion pattern requires an agent-executed change's footprint to match a spec's declared Write Set exactly. Every agent-executed delivery in this repository also writes tracked `.claude/agent-memory/` files that no spec's Write Set enumerates in advance, so a criterion phrased this way fails on every delivery regardless of whether the substantive requirement (no untouched production or test file modified) was met. This is a defect in the criterion template wording, not in the work it judges.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: not applicable; this concerns the atomic-plan/spec acceptance-criterion template used for C# and cross-language feature work
- Command/flags used: n/a (documentation/process defect); observed via `git -C <item-871-worktree> diff --name-only <merge-base>..HEAD` type footprint review during feature-audit
- Data source or fixture: `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/spec.md` (AC22), pull request #883

## Steps to Reproduce

1. Open item 871's spec.md AC22 and its pull request #883 (`fix(#871): add injectable seams to the QfcQueue enqueue path`).
2. Read PR #883's "Follow-ups" section, which states verbatim: "Acceptance criterion AC22 is the one criterion not met as literally worded, and is left unchecked. Its substantive requirement holds: no untouched production or test file was modified. It fails only because the branch carries three tracked agent-memory files that the declared Write Set does not enumerate."
3. Confirm `.claude/agent-memory/` is a tracked directory in this repository (verified: dozens of files under it are tracked and modified across active sessions, including four `.md` files touched in this same session).
4. Observe that every agent that executes a plan writes to its configured agent-memory root during the run, so this same AC22-style criterion will recur on the next delivery, and the one after that, independent of which agent or which feature is involved.

## Expected Behavior

An AC22-style "Write Set" criterion should be satisfiable by a correctly-scoped, agent-executed change. Either the criterion should carry an explicit repository-wide carve-out for `.claude/agent-memory/**` writes (since these are infrastructure bookkeeping, not production or test changes), or the Write Set declaration mechanism should be extended to auto-include the agent-memory paths that any executing agent is expected to touch.

## Actual Behavior

The criterion as currently worded is unsatisfiable in principle by any agent-executed change, because:
- `.claude/agent-memory/` is tracked in git.
- Every agent (orchestrator, atomic-executor, feature-review, etc.) writes lesson/memory files to it during normal operation.
- No spec's Write Set, authored before execution, can enumerate agent-memory files that do not yet exist at authoring time.
- The failure therefore recurs on every item, not just item 871, and cannot be fixed by tightening any individual plan's scope.

This shares a root cause with a related, separately-observed problem: some agents' configured agent-memory root resolves into a worktree their own directives forbid them to write to, which produced four disclosed directive breaches in one run (reported separately; not itself the subject of this issue, but the underlying agent-memory-path/worktree-resolution mechanism is the same one implicated here).

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: PR #883 body, "Follow-ups" section, final bullet: "Acceptance criterion AC22 is the one criterion not met as literally worded, and is left unchecked. Its substantive requirement holds: no untouched production or test file was modified. It fails only because the branch carries three tracked agent-memory files that the declared Write Set does not enumerate. The remedy is a one-line documentation amendment, not a code change." (verified by reading `gh pr view 883` on 2026-09-13)

## Impact / Severity

- [ ] Blocker
- [x] Medium
- [ ] Low

Medium: the underlying implementation work was verified sound (no untouched production/test file modified), so no delivered change is actually defective. The cost is a recurring, unfixable-per-item AC failure that will misreport delivered work as incomplete on every future item until the template wording changes.

## Suspected Cause / Notes

The Write Set criterion template was authored without accounting for `.claude/agent-memory/` being both (a) tracked in git and (b) written by every executing agent as a side effect of normal operation, independent of the feature's actual scope. A plan-level clause cannot fix this because a plan clause cannot amend a spec-level acceptance criterion; the fix has to live in the template that generates AC22-style criteria.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: not applicable (template/documentation change, not source code)
- [x] Integration scenario to retest: apply the corrected template to the next feature that declares an AC22-style Write Set criterion and confirm the criterion can pass without requiring the agent to omit or falsify its agent-memory writes
- [x] Manual verification notes: add an explicit carve-out clause to the Write Set criterion template — for example, "the Write Set is understood to exclude `.claude/agent-memory/**`, which every executing agent may write to as bookkeeping regardless of feature scope" — at the criterion-template level (not per-plan), since a plan clause cannot amend a spec criterion.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch

Cross-reference: pull request #883 (item 871) discloses this exact unchecked AC22 and points at this issue.
