# [P0-T4] — Containment and acceptance-criteria invariance baseline

- Timestamp: 2026-08-30T02-08
- Task: `[P0-T4]`
- Issue: #644
- Branch: `bug/qfc-unregister-navigation-count-mismatch-orphan-644`
- Head at cycle entry: `85a1939f92f64ebada4e71d19cc095dc2e8e8a26`
- Shell: git and PowerShell 7.6.5, run from the repository root of the branch worktree.

All seven commands are scoped to the two paths this cycle's commit will stage, plus
the single production-file pair. None is repository-wide: the repository carries
pre-existing, unrelated drift under `.claude/agent-memory` and elsewhere that is out
of scope for this cycle and is not part of either comparison.

## Command 1 — anchored name-status diff, two commit pathspecs

- Command: `git diff --name-status 85a1939f92f64ebada4e71d19cc095dc2e8e8a26 -- QuickFiler.Test docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644`
- EXIT_CODE: 0
- Expected: empty
- Measured: **empty** (zero output lines)

No tracked file under either pathspec carries an uncommitted change at cycle entry.
The branch head is `85a1939f92f64ebada4e71d19cc095dc2e8e8a26` itself.

## Command 2 — porcelain status companion, two commit pathspecs

- Command: `git status --porcelain -- QuickFiler.Test docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644`
- EXIT_CODE: 0
- Measured listing:

```
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/code-review.2026-08-30T01-46.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/remediation-baseline/p0-t1-instructions-read.2026-08-30T02-08.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/remediation-baseline/p0-t2-target-lines-baseline.2026-08-30T02-08.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/remediation-baseline/p0-t3-class-sweep-baseline.2026-08-30T02-08.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/feature-audit.2026-08-30T01-46.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/policy-audit.2026-08-30T01-46.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/remediation-inputs.2026-08-30T02-08.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/remediation-plan.2026-08-30T02-08.md
```

Eight `??` entries, no `M`, `A` or `D` entry. Five are the pre-existing untracked
cycle artifacts the plan names (this plan file, `remediation-inputs.2026-08-30T02-08.md`,
and the three `2026-08-30T01-46` reaudit artifacts). The remaining three are the
evidence artifacts written by `[P0-T1]`, `[P0-T2]` and `[P0-T3]`, which run before this
task in the plan's own task order and therefore already exist when this command runs.
No `??` entry appears under `QuickFiler.Test`.

## Command 3 — spec.md checked-checkbox count

- Command: `@(Select-String -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644\spec.md -Pattern '^- \[x\]').Count`
- EXIT_CODE: 0
- Expected: `21`
- Measured: **`21`**

## Command 4 — spec.md unchecked-checkbox count

- Command: `@(Select-String -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644\spec.md -Pattern '^- \[ \]').Count`
- EXIT_CODE: 0
- Expected: `5`
- Measured: **`5`**

## Command 5 — production-file anchored name-only diff

- Command: `git diff --name-only 85a1939f92f64ebada4e71d19cc095dc2e8e8a26 -- QuickFiler/Controllers/QfcCollectionController.cs`
- EXIT_CODE: 0
- Expected: empty
- Measured: **empty** (zero output lines)

## Command 6 — production-file porcelain companion

- Command: `git status --porcelain -- QuickFiler/Controllers/QfcCollectionController.cs`
- EXIT_CODE: 0
- Expected: empty
- Measured: **empty** (zero output lines)

Neither `QuickFiler/Controllers/` nor any other production directory is covered by the
two pathspecs the commands above scope to, so this pair is the only check in this plan
that directly observes the production file. It is repeated in `[P3-T1]` clause 4.

## Command 7 — SHA-256 of the four untracked prior artifacts

- Command: `Get-FileHash -Algorithm SHA256 -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644\code-review.2026-08-30T01-46.md, docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644\feature-audit.2026-08-30T01-46.md, docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644\policy-audit.2026-08-30T01-46.md, docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644\remediation-inputs.2026-08-30T02-08.md | Select-Object Path, Hash`
- EXIT_CODE: 0
- Measured hashes (paths shown as feature-folder-relative file names; all four live
  directly under
  `docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/`):

| File | SHA-256 |
|---|---|
| `code-review.2026-08-30T01-46.md` | `9A998689B9C5D3B4D47F4C930986A2550BBE748DCEF004AEAB4E422F9B324FE7` |
| `feature-audit.2026-08-30T01-46.md` | `05D60726574C12C4C0A1F34646388C1229B0B71D977B3B62B2AB0C88CF906019` |
| `policy-audit.2026-08-30T01-46.md` | `6C9F0C07C297904BC6ECDB5C2AFEB64A70F6D2120E7747F9498F440479D430A1` |
| `remediation-inputs.2026-08-30T02-08.md` | `9E5F33FE7D767F2F84C8CF2FC3BAADECD59B2ECCADDE877FFA6218B46F18460D` |

These four artifacts are untracked at cycle entry, so no anchored git diff can observe
a modification to them. This hash set is the only mechanism that can, and `[P3-T1]`
clause 8 re-runs the identical command and compares against these four values.

## Output Summary

All seven commands returned `EXIT_CODE: 0`. Both anchored diffs are empty and the
production-file porcelain companion is empty, confirming no tracked file under either
commit pathspec and no change to `QuickFiler/Controllers/QfcCollectionController.cs`
at cycle entry. The two-pathspec porcelain companion lists eight `??` entries and no
`M`, `A` or `D` entry. `spec.md` reads `21` checked and `5` unchecked. Four SHA-256
hashes recorded for the untracked prior artifacts.
