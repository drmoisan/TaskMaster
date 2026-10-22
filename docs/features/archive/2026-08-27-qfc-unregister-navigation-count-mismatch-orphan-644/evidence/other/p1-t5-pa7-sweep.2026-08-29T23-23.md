# [P1-T5] — PA-7 Post-Edit Sweep (both patterns, no path exclusion)

Timestamp: 2026-08-29T23-23
Run performed: 2026-08-30T01-17
Task: [P1-T5]
Working directory: repository root of the worktree
EXIT_CODE: 0 (both commands)

Scope: the whole feature folder
`docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644`, with
**no path exclusion of any kind**. The remediation plan file and every evidence artifact this
cycle has written so far are inside that folder and are included in the sweep. 57 files were
swept.

Redaction note: this artifact reproduces no raw absolute-path text and no account token.

## Pattern-delivery verification

The shape pattern delivered to the search engine was compared for case-sensitive equality
against a pattern constructed inside PowerShell from `[char]92`, which no shell escaping layer
can alter. The comparison returned `True` with `LEN=24`, confirming the executed regular
expression is character-for-character the pattern the plan mandates and not a de-doubled
variant whose class would match the forward slash only.

## Sweep 1 — shape pattern

Command: `@(Get-ChildItem -Recurse -File -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644 | Select-String -Pattern '[A-Za-z]:[\\/]Users[\\/]').Count`
EXIT_CODE: 0
Baseline in `[P0-T3]`: `2`   Measured now: `0`   Required: `0`   Result: PASS

Residual match list: empty.

## Sweep 2 — account-token pattern

Command: `$t=[regex]::Escape((Split-Path -Leaf $env:USERPROFILE)); @(Get-ChildItem -Recurse -File -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644 | Select-String -Pattern "(?i)$t").Count`
EXIT_CODE: 0
Baseline in `[P0-T3]`: `3`   Measured now: `0`   Required: `0`   Result: PASS

Residual match list: empty.

## Why both patterns are required

The shape pattern alone is structurally incapable of seeing the third instance. Its `[\\/]`
class matches exactly one separator character, so it cannot match a path written with doubled
backslashes; and it carries a mandatory drive-letter prefix, so it cannot see a bare account
token at all. A sweep running only the shape pattern would have returned `0` while an account
token was still present on `policy-audit.2026-08-29T23-06.md` line 483. `[P0-T3]` measured that
asymmetry directly: shape `2` against account-token `3`, the extra match being line 483.

## Falsifiability across the correction

This gate can fail. The two shape-pattern instances remained open until `[P1-T2]` and `[P1-T3]`
ran, and the third account-token instance remained open until `[P1-T4]` ran. Immediately before
those tasks executed, the two patterns read `2` and `3` — both nonzero, both false against the
required `0`. `[P0-T3]` records those nonzero readings. Both now read `0`.

## Machine-name pattern deliberately omitted here

The third sweep pattern is not run in this task. Its baseline is already `0`, measured in
`[P0-T3]`, because every file in the feature folder that references a host or TRX identifier
already writes the `<HOST>` placeholder. A pattern whose baseline is already `0` cannot
demonstrate a fix: the clause would be true before the edit and still true after it, verifying
nothing. `[P3-T3]` is the correct place for it, because that task is a final
invariant-preservation sweep rather than a before-and-after remediation check, and a steady `0`
there is exactly what it is designed to confirm.

## Output Summary

Both sweep patterns return `0` over the whole feature folder with no path exclusion, down from
the `2` and `3` recorded at the `[P0-T3]` baseline. All three PA-7 instances are remediated.
Neither residual match list contains any entry. 57 files were swept, including this plan file
and every evidence artifact written so far in this cycle.
