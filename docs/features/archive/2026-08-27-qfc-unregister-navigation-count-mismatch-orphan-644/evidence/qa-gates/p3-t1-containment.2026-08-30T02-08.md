# [P3-T1] — Containment and invariance re-verification

- Timestamp: 2026-08-30T02-08
- Task: `[P3-T1]`
- Issue: #644
- Branch: `bug/qfc-unregister-navigation-count-mismatch-orphan-644`
- Anchor ref: `85a1939f92f64ebada4e71d19cc095dc2e8e8a26`
- Working directory: repository root of the branch worktree. No absolute host path,
  account name, or machine name is written to this artifact.
- EXIT_CODE: 0 (every command below)

## Clause 1 — anchored name-status diff, two commit pathspecs

- Command: `git diff --name-status 85a1939f92f64ebada4e71d19cc095dc2e8e8a26 -- QuickFiler.Test docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644`
- EXIT_CODE: 0
- Measured:

```
M	QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs
```

Required: exactly one line, `M	QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs`.
Measured: **exactly that one line**. PASS.

Every tracked file the plan's hard scope limits protect — `spec.md`,
`plan.2026-08-29T07-42.md`, `remediation-plan.2026-08-29T23-23.md`,
`remediation-inputs.2026-08-29T23-23.md`, `issue.md`, the
`code-review/feature-audit/policy-audit` artifacts at `2026-08-29T23-06`, and every
pre-existing file under `evidence/` — lives under one of these two pathspecs, so an
edit to any of them would appear here. None does.

## Clause 2 — porcelain companion shows no other tracked change

- Command: `git status --porcelain -- QuickFiler.Test docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644`
- EXIT_CODE: 0
- Measured: 18 entries — one ` M` and seventeen `??`:

```
 M QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/code-review.2026-08-30T01-46.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/other/p1-t1-cr6-edit.2026-08-30T02-08.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/other/p1-t2-cr2-edit.2026-08-30T02-08.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/other/p1-t3-class-sweep-final.2026-08-30T02-08.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/qa-gates/p2-t1-csharpier-format.2026-08-30T02-08.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/qa-gates/p2-t2-csharpier-check.2026-08-30T02-08.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/qa-gates/p2-t3-analyzer-build.2026-08-30T02-08.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/qa-gates/p2-t4-nullable-build.2026-08-30T02-08.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/qa-gates/p2-t5-vstest-final.2026-08-30T02-08.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/remediation-baseline/p0-t1-instructions-read.2026-08-30T02-08.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/remediation-baseline/p0-t2-target-lines-baseline.2026-08-30T02-08.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/remediation-baseline/p0-t3-class-sweep-baseline.2026-08-30T02-08.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/remediation-baseline/p0-t4-invariance-baseline.2026-08-30T02-08.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/feature-audit.2026-08-30T01-46.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/policy-audit.2026-08-30T01-46.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/remediation-inputs.2026-08-30T02-08.md
?? docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/remediation-plan.2026-08-30T02-08.md
```

Required: no `M`, `A` or `D` entry for any path other than the digits test file, and
every `??` entry under
`docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/`,
with no `??` entry under `QuickFiler.Test`.

Measured: the single ` M` entry is the digits test file; there is no `A` and no `D`
entry; all seventeen `??` entries are under the feature folder; no `??` entry appears
under `QuickFiler.Test`. PASS.

The anchored name-status diff enumerates tracked changes only and is structurally blind
to the untracked cycle artifacts; this porcelain companion is what observes them.

## Clause 3 — new evidence artifacts are visible as untracked

Required: at least one `??` entry covering this cycle's new evidence artifacts under
`docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/`.

Measured: **twelve** such entries — four under `evidence/remediation-baseline/`
(`p0-t1` through `p0-t4`), three under `evidence/other/` (`p1-t1` through `p1-t3`), and
five under `evidence/qa-gates/` (`p2-t1` through `p2-t5`). This artifact itself is
written after the command ran, so it is not among the twelve; `[P3-T3]`'s pre-staging
porcelain observes it. PASS.

## Clause 4 — the production file carries no change

- Command: `git diff --name-only 85a1939f92f64ebada4e71d19cc095dc2e8e8a26 -- QuickFiler/Controllers/QfcCollectionController.cs`
- EXIT_CODE: 0
- Measured: **empty** (zero output lines)

- Command: `git status --porcelain -- QuickFiler/Controllers/QfcCollectionController.cs`
- EXIT_CODE: 0
- Measured: **empty** (zero output lines)

Both are still empty, identical to `[P0-T4]`. `QuickFiler/Controllers/QfcCollectionController.cs`
carries no change. PASS.

## Clause 5 — spec.md checkbox counts unchanged, and the read-only AC Status Summary

- Command: `@(Select-String -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644\spec.md -Pattern '^- \[x\]').Count`
- EXIT_CODE: 0 — `[P0-T4]`: `21`. Measured now: **`21`**.
- Command: `@(Select-String -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644\spec.md -Pattern '^- \[ \]').Count`
- EXIT_CODE: 0 — `[P0-T4]`: `5`. Measured now: **`5`**.

Both counts are identical to the cycle-entry baseline. Nothing in `spec.md` was
changed by this cycle. PASS.

### Acceptance Criteria Status (read-only)

- Source: `docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/spec.md`
- Work mode: `full-bug` (`spec.md` is the sole AC source)
- Total AC items (`- [ ] **AC-` / `- [x] **AC-` form): 18
- Checked off (delivered): 17
- Remaining (unchecked): 1
- Items remaining: **AC-16 (no coverage regression on changed lines)** — spec.md line
  707.

AC-16 stands as PARTIAL, unchecked, referred and reported. Its disposition is final and
unchanged by this cycle. No coverage comparison was run, no coverage figure was read
from `[P2-T5]`'s `.coverage` attachments, and no acceptance criterion changed state.

For completeness, the plan's invariant counts of `21` and `5` are over every top-level
checkbox in `spec.md`, which is a superset of the AC items: the remaining four
unchecked non-AC checkboxes are the severity selectors at lines 42, 43 and 45 and the
report-template item at line 78. All four are unchanged. No checkbox anywhere in
`spec.md` was modified.

## Clause 6 — the seven sibling-region tokens are unchanged

Each command has the shape
`@(Select-String -Path QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs -SimpleMatch -Pattern '<token>').Count`.
All returned `EXIT_CODE: 0`.

| Token | `[P0-T2]` | Required | Measured | Result |
|---|---|---|---|---|
| `modelling the unbracketed` | 1 | 1 | **1** | PASS |
| `The single residual` | 1 | 1 | **1** | PASS |
| `the tenth key was never visited whatever the digit width` | 1 | 1 | **1** | PASS |
| `StartsWith("0", StringComparison.Ordinal)` | 1 | 1 | **1** | PASS |
| `issue #644 replaced the count-bounded removal loop with a ledger that replays` | 1 | 1 | **1** | PASS |
| `the added tenth group is irrelevant to` | 1 | 1 | **1** | PASS |
| `regardless of group count` | 1 | 1 | **1** | PASS |

## Clause 7 — file shape unchanged

| Command | Required | Measured | EXIT_CODE | Result |
|---|---|---|---|---|
| `(Get-Content QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs).Count` | 226 | **226** | 0 | PASS |
| `@(Select-String -Path QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs -SimpleMatch -Pattern '[TestMethod]').Count` | 3 | **3** | 0 | PASS |

## Clause 8 — the four untracked prior artifacts are unmodified

- Command: `Get-FileHash -Algorithm SHA256 -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644\code-review.2026-08-30T01-46.md, docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644\feature-audit.2026-08-30T01-46.md, docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644\policy-audit.2026-08-30T01-46.md, docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644\remediation-inputs.2026-08-30T02-08.md | Select-Object Path, Hash`
- EXIT_CODE: 0

| File | `[P0-T4]` hash | Hash now | Result |
|---|---|---|---|
| `code-review.2026-08-30T01-46.md` | `9A998689B9C5D3B4D47F4C930986A2550BBE748DCEF004AEAB4E422F9B324FE7` | `9A998689B9C5D3B4D47F4C930986A2550BBE748DCEF004AEAB4E422F9B324FE7` | IDENTICAL |
| `feature-audit.2026-08-30T01-46.md` | `05D60726574C12C4C0A1F34646388C1229B0B71D977B3B62B2AB0C88CF906019` | `05D60726574C12C4C0A1F34646388C1229B0B71D977B3B62B2AB0C88CF906019` | IDENTICAL |
| `policy-audit.2026-08-30T01-46.md` | `6C9F0C07C297904BC6ECDB5C2AFEB64A70F6D2120E7747F9498F440479D430A1` | `6C9F0C07C297904BC6ECDB5C2AFEB64A70F6D2120E7747F9498F440479D430A1` | IDENTICAL |
| `remediation-inputs.2026-08-30T02-08.md` | `9E5F33FE7D767F2F84C8CF2FC3BAADECD59B2ECCADDE877FFA6218B46F18460D` | `9E5F33FE7D767F2F84C8CF2FC3BAADECD59B2ECCADDE877FFA6218B46F18460D` | IDENTICAL |

All four hashes match. These artifacts are untracked, so no anchored git diff can
observe a modification to them; this hash set is the only mechanism that can, and it
confirms they are carried into the commit unmodified. PASS.

## Second pass — full re-run required by `[P3-T3]`'s redaction branch

`[P3-T3]`'s pre-staging host-identity sweep returned `1` on a synthetic placeholder path
quoted inside `[P3-T2]`'s own artifact, which did not exist when `[P3-T2]` ran. Under the
repeat branch `[P3-T3]` authorizes, that literal was redacted in place and this task was
re-run in full. All eight clauses were re-measured and all eight still hold:

| Clause | Required | Measured on the re-run | Result |
|---|---|---|---|
| 1 — anchored name-status diff | exactly one `M` line for the digits test file | **exactly that one line** | PASS |
| 2 — porcelain: no other `M`/`A`/`D`, every `??` under the feature folder, none under `QuickFiler.Test` | as stated | **1 `M`, 19 `??`, all under the feature folder, none under `QuickFiler.Test`** | PASS |
| 3 — at least one `??` under `evidence/` | >= 1 | **fourteen** (four `remediation-baseline`, three `other`, seven `qa-gates`) | PASS |
| 4 — production-file diff and porcelain both empty | empty | **both empty** | PASS |
| 5 — `spec.md` counts | 21 checked, 5 unchecked | **21 checked, 5 unchecked** | PASS |
| 6 — seven sibling tokens | each 1 | **each 1** | PASS |
| 7 — file shape | 226 lines, 3 `[TestMethod]` | **226 lines, 3 `[TestMethod]`** | PASS |
| 8 — four SHA-256 hashes | identical to `[P0-T4]` | **all four identical** | PASS |

The two additional `??` entries relative to the first pass are this artifact and
`[P3-T2]`'s artifact, both of which were written after the first pass ran. The redaction
changed only prose inside `[P3-T2]`'s artifact; it touched no tracked file, so clause 1
is unchanged.

## Output Summary

All eight acceptance clauses hold, on the first pass and again on the full re-run. The
anchored name-status diff contains exactly one
`M` line for
`QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs`; the
porcelain companion shows no other tracked change, no `??` under `QuickFiler.Test`, and
twelve `??` entries covering the new evidence artifacts; the production file
`QuickFiler/Controllers/QfcCollectionController.cs` carries no change on either the
anchored diff or the porcelain companion; `spec.md` still reads `21` checked and `5`
unchecked with AC-16 unchanged as the one unchecked acceptance criterion; all seven
sibling-region tokens still count `1`; the file is still `226` lines with `3`
`[TestMethod]` attributes; and all four SHA-256 hashes of the untracked prior artifacts
are identical to `[P0-T4]`.
