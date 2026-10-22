# P0-T2 — Feature Documents Read

Timestamp: 2026-09-13T04-52
Task: [P0-T2]

Command: pwsh -NoProfile -Command '... (Get-Content -LiteralPath $f).Count ...' over the three documents

## Documents Read End To End, With Line Counts As Read

| Document (repository-relative) | Line count |
|---|---|
| docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/spec.md | 308 |
| docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/issue.md | 78 |
| docs/features/active/2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling-873/research/2026-09-12T11-00-test-evidence-projection-research.md | 1070 |

Three integers, bare: 308, 78, 1070.

## Output Summary

All three documents were read end to end in this worktree. Observations material to Phase 0:

- `spec.md` records Work Mode full-bug and declares itself the single acceptance-criteria source.
  It carries 23 acceptance criteria, AC1 through AC23, at lines 272 through 294, all unchecked.
- `spec.md`'s `## Write Set` section spans lines 191 through 212 and holds 22 backticked entries,
  with no prose beneath the list. This is the pre-state P0-T16 acts on.
- AC15 (line 286) and AC22 (line 293) each contain the phrase `no worse than the Phase 0 baseline`.
  This is the pre-state P0-T17 verifies.
- `user-story.md` also exists in this folder. Under full-bug mode it is narrative only and carries
  no acceptance criteria; spec.md line 9 states this. Its presence is not a fail-closed condition
  for full-bug mode.
- The research artifact records R1 through R12 plus a Numeric Derivation Evidence section deriving
  the two-member argument-builder family (`Get-VsTestArgumentList`, `Get-DotnetCoverageArgumentList`).
- Research R5 records the helpers file at 470 content lines, that is 29 lines of headroom against
  the 500-line ceiling. P0-T10 and P0-T13 re-measure this in this worktree rather than adopting the
  research figure.

EXIT_CODE: 0
