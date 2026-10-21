# Secondary Sanitisation of the Enumerated Residual Set — Remediation R-1, Issue #752

- Timestamp: 2026-09-03T23-44
- Task: `[P1-T2]`

Command:

1. `pwsh -NoProfile -File <repo-root>/coverage/r1-host-path-sweep.ps1 -Mode File -Path docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/issue.md` (before and after the edit)
2. `pwsh -NoProfile -File <repo-root>/coverage/r1-host-path-sweep.ps1 -Mode File -Path docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/spec.md` (before and after the edit)
3. `pwsh -NoProfile -File <repo-root>/coverage/r1-host-path-sweep.ps1 -Mode File -Path docs/features/potential/promoted/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root.md` (before and after the edit)
4. `pwsh -NoProfile -Command` performing the line-scoped substitution described below, one file at a time, preserving each file's existing line-ending form and total line count.

EXIT_CODE:

1. `0` (both invocations)
2. `0` (both invocations)
3. `0` (both invocations)
4. `0`

Output Summary:

## Selection

`SELECTED: 3`. The `[P0-T4]` enumeration listed four `MATCHFILE:` entries. One of them,
`research/research-findings.2026-09-03T00-00.md`, was already handled in `[P1-T1]` and is excluded
here. The remaining three are all `.md` files under `docs/features/` and are the selection for this
task. The selection rule is the whole `docs/features/` tree rather than the feature folder alone,
which is why the pre-promotion copy under `docs/features/potential/promoted/` is included.

The selection agrees with the plan's expected-members list, both in file set and in line numbers. No
divergence is recorded.

## Per-file record

| File (repo-relative) | Lines changed | Token class substituted | Placeholder selected | Substitution branch |
|---|---|---|---|---|
| `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/issue.md` | 22, 42 | Windows user-profile path prefix, together with the account segment immediately following it | `<user-profile>` | (a) token is part of a path |
| `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/spec.md` | 19 | Windows user-profile path prefix, together with the account segment immediately following it | `<user-profile>` | (a) token is part of a path |
| `docs/features/potential/promoted/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root.md` | 20, 40 | Windows user-profile path prefix, together with the account segment immediately following it | `<user-profile>` | (a) token is part of a path |

## Placeholder selection rationale

Every one of the five substituted values names the agent-worktree root of a **different** checkout,
belonging to item #735, rather than this repository's own root. Under hygiene rule 5 of the plan and
the required-placeholder table at `.claude/agent-memory/_shared_no_absolute_host_paths.md` lines
17-26, the correct placeholder for a path rooted at an operator user-profile directory that is not
this repository is `<user-profile>`. The repository-root placeholder was used only on the single line
handled in `[P1-T1]`, where the value genuinely was this checkout's own root. The placeholder was
therefore selected per case, not applied uniformly.

The trailing portion of each path — everything from the segment following the account segment onward
— is byte-identical to what was there before, matching the worked shape the plan prescribes. Each
edit replaced exactly one occurrence on its line; no other character on any of the five lines was
altered.

## Constraints observed

- No line beginning with `- [x] ` or `- [ ] ` was modified. Each of the five target lines was tested
  for those two prefixes before the edit and none carried either.
- Line counts are unchanged: `issue.md` 71, `spec.md` 123, the promoted copy 66. Each file was
  LF-terminated with zero carriage returns before and after the edit.
- No removed value is quoted anywhere in this artifact. Every substituted token is named by class.

## Audit-artifact branch, recorded per file

The `[P0-T4]` enumeration selected **none** of this loop's four audit artifacts, because a Diff-mode
enumeration observes only committed content and all four are untracked in this worktree. The
"enumeration does not select them" branch therefore applied to each:

- `policy-audit.2026-09-03T12-23.md` — not selected; untracked; no change made here.
- `code-review.2026-09-03T12-23.md` — not selected; untracked; no change made here.
- `feature-audit.2026-09-03T12-23.md` — not selected; untracked; no change made here.
- `remediation-inputs.2026-09-03T12-23.md` — not selected; untracked; no change made here.

`[P2-T5]` step 4 discloses all four in File mode instead.

## Post-edit verification

A File-mode run after the edit printed, for each of the three selected files, exactly one
`FILECOUNT:` line and no `FILEMATCH:` line:

```
FILECOUNT: docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/issue.md | COUNT: 0
FILECOUNT: docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/spec.md | COUNT: 0
FILECOUNT: docs/features/potential/promoted/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root.md | COUNT: 0
```
