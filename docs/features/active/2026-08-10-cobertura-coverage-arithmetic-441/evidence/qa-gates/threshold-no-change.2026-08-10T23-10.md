# No-Threshold-Change Gate (P4-T9)

Timestamp: 2026-08-10T23-10

Enforces `spec.md` AC-17 and the plan's § Non-Goals item 1: **no coverage threshold may be re-tuned
by this feature.** Threshold reconciliation is owned by child feature #494 (wave 2).

Command:

```powershell
$root = (git rev-parse --show-toplevel) -replace '/', '\'
Set-Location $root
git diff --name-only edf3d34c -- CLAUDE.md .claude/rules coverage.config
git diff HEAD -- scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 |
    Select-String '^\+' | Select-String -Pattern '\b(85|90|75)\b'
```

EXIT_CODE: 0

Output Summary:

```
=== git diff --name-only edf3d34c -- CLAUDE.md .claude/rules coverage.config ===
=== end (empty = clean) ===

=== added lines matching \b(85|90|75)\b ===
+        # case. The merged rate must be 3/5 = 0.6, not the blended 6/8 = 0.75.
```

## Part 1 — threshold-bearing files

`git diff --name-only edf3d34c -- CLAUDE.md .claude/rules coverage.config` returned **empty output**.

- `CLAUDE.md` — unchanged.
- Every file under `.claude/rules/` — unchanged.
- `coverage.config` — unchanged.

**PASS.**

## Part 2 — enumerated added lines containing `85`, `90` or `75`

The judgment is bounded to the enumerated matches, so the check is mechanically reproducible.
Exactly **one** added line across both changed source files matches.

| # | Added line | File | Classification | Justification |
| --- | --- | --- | --- | --- |
| 1 | `        # case. The merged rate must be 3/5 = 0.6, not the blended 6/8 = 0.75.` | `tests/.../Invoke-MSTestWithCoverage.Helpers.Tests.ps1` (fixture F3 comment) | **fixture rate value** | `75` matched inside the literal `0.75`, which is the *defective* per-file `line-rate` that F3 exists to detect — the miniature of the confirmed `QfcHomeController.Iteration.cs` case (`0.8625` -> `0.803571` at repository scale becomes `0.75` -> `0.6` at fixture scale). It is a Cobertura attribute value asserted by a regression test, **not** a coverage threshold. It configures nothing and gates nothing. |

**No added line expresses a coverage threshold. PASS.**

## Recorded handoff, acted upon nowhere

The corrected repository-wide line rate for the #424 committed sample is **85.0317%** against the
uniform **85%** line floor in `.claude/rules/general-unit-test.md` — a margin of **0.03 percentage
points**. That observation is recorded as fact in
`<FEATURE>/evidence/other/threshold-handoff-494.<TS>.md` and is acted upon nowhere in this change.
This feature proposes no threshold change and makes no threshold edit.
