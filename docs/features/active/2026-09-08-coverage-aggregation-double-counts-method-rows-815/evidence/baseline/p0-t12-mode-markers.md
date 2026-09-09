# P0-T12 — Full-Bug Mode Markers

Timestamp: 2026-09-09T10-51
Task: [P0-T12]
EXIT_CODE: 0

## Condition 1 — `issue.md` carries the work-mode marker

Command: `git grep -c -F -e '- Work Mode: full-bug' -- docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/issue.md`
EXIT_CODE: 0

```
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/issue.md:1
```

The marker is present exactly once, at `issue.md` line 12.

## Condition 2 — `spec.md` carries an `## Acceptance Criteria` heading

Command: `git grep -c -F -e '## Acceptance Criteria' -- docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/spec.md`
EXIT_CODE: 0

```
docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/spec.md:1
```

The heading is present exactly once, at `spec.md` line 458, and the section under it carries AC1
through AC14 as markdown checkbox items.

## Condition 3 — no `user-story.md` exists in the feature folder

Command: `pwsh -NoProfile -Command 'Test-Path -LiteralPath "docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/user-story.md"'`
EXIT_CODE: 0

```
False
```

Output Summary: All three conditions hold. The persisted work-mode marker resolves to `full-bug`,
which per `.claude/skills/acceptance-criteria-tracking/SKILL.md` makes `spec.md` the sole
authoritative acceptance-criteria source for this feature. `spec.md` carries the required
`## Acceptance Criteria` heading with fourteen checkbox criteria. `user-story.md` does not exist,
and **its absence is correct by design for full-bug mode**: the mode table maps `full-bug` to
`spec.md` only, so a `user-story.md` is neither required nor expected, and its absence is not a gap.
Phase 6 checks off AC1 through AC14 in `spec.md` and in no other file.
