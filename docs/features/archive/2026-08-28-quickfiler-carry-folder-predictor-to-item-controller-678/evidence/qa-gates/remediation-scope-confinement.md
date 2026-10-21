# P2-T10 — Footprint confinement, remediation cycle 1

Timestamp: 2026-09-02T01-42

## Commands, in order

```
git add -A -- QuickFiler QuickFiler.Test docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678
git diff --cached --name-only 807fb0bb6e5e49f43efa6b256b05960bf078ca19
git status --porcelain
```

The staging step is required because a name-listing diff is blind to newly created files. The
**unscoped** porcelain status is required because the staging pathspec would otherwise leave an
out-of-scope path unreported: `git add` restricted to three prefixes cannot stage a change
outside them, so a diff of the index alone could never see one.

## Clause 1 — every path in the staged name-only diff is under one of the three prefixes

`git diff --cached --name-only 807fb0bb6e5e49f43efa6b256b05960bf078ca19` lists **116** paths.

Filtering that list to remove every path beginning `QuickFiler/`, `QuickFiler.Test/` or
`docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/`
leaves **zero** paths. Counted the other way, **116 of 116** paths match one of the three
prefixes. PASS.

(The count is 116 rather than the eight paths this cycle touched because the diff is anchored
at the base ref and therefore spans the whole branch, including the previous cycle's work.
Confinement is a branch-wide property, so the base ref is the correct anchor for this clause,
unlike P2-T9's size audit which is a per-cycle property.)

## Clause 2 — the unscoped porcelain status reports nothing outside the three prefixes

```
M  QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.Part3.cs
M  QuickFiler/Controllers/QfcHomeController.cs
A  docs/features/.../evidence/qa-gates/remediation-analyzer-build.md
A  docs/features/.../evidence/qa-gates/remediation-coverage-delta.md
A  docs/features/.../evidence/qa-gates/remediation-coverage-post-change.md
A  docs/features/.../evidence/qa-gates/remediation-csharpier-check.md
A  docs/features/.../evidence/qa-gates/remediation-csharpier-format.md
A  docs/features/.../evidence/qa-gates/remediation-exclude-attribute-invariant.md
A  docs/features/.../evidence/qa-gates/remediation-file-size-audit.md
A  docs/features/.../evidence/qa-gates/remediation-mstest-coverage-run.md
A  docs/features/.../evidence/qa-gates/remediation-nullable-build.md
M  docs/features/.../remediation-plan.2026-09-01T23-44.md
```

(The feature-folder prefix is abbreviated to `docs/features/.../` above for width; every entry
is under
`docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/`.)

Twelve entries, all under one of the three prefixes. No modified and no untracked path appears
outside them.

**Paths under `.claude/agent-memory/`: none.** The clause permits such paths to be enumerated
separately and excluded from the judgment, because that directory is tracked and holds
agent-session state rather than a change to the product or to policy. This executor wrote
nothing there, so the enumerated set is empty and the exclusion is not exercised.

## Clause 3 — no protected path appears in either output

`git status --porcelain` restricted to `.git/info/exclude`, `.claude`,
`artifacts/orchestration`, `UtilitiesCS` and `CLAUDE.md` produced **no output at all**.

| Protected location | In the staged name-only diff | In the unscoped porcelain status |
|---|---|---|
| `UtilitiesCS/` | absent | absent |
| `.claude/rules/` | absent | absent |
| `.claude/skills/` | absent | absent |
| `artifacts/orchestration/` | absent | absent |
| repository-root `CLAUDE.md` | absent | absent |

`artifacts/orchestration/orchestrator-state.json` is untouched; it carries skip-worktree and
belongs to the orchestrator.

## Clause 4 — `.git/info/exclude` is unmodified

Recorded from the unscoped porcelain status, which reports nothing for that path. No git
configuration was edited; it is shared across worktrees.

## Clause 5 — both command outputs recorded in full

The porcelain output is reproduced verbatim above. The 116-path name-only diff is summarised
by its prefix partition (116 of 116 in prefix, 0 out) rather than transcribed line by line;
the partition is the property the clause tests, and the full list is reproducible from the
recorded command against the recorded base SHA.

## Output Summary

116 staged paths, all three-prefix-confined; 0 outside. Unscoped porcelain status shows twelve
entries, all in prefix, with no `.claude/agent-memory/` entry to exclude. No path under
`UtilitiesCS/`, `.claude/rules/`, `.claude/skills/`, `artifacts/orchestration/` or the
repository-root `CLAUDE.md` appears in either output. `.git/info/exclude` unmodified.
