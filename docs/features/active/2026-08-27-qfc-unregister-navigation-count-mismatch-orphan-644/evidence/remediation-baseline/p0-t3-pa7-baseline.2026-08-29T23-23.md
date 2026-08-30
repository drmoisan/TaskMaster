# [P0-T3] — PA-7 Pre-Edit Baseline (three sweep patterns, no path exclusion)

Timestamp: 2026-08-29T23-23
Run performed: 2026-08-30T01-17
Task: [P0-T3]
Working directory: repository root of the worktree
EXIT_CODE: 0 (all six commands; the PowerShell session returned exit 0)

Scope note: the sweep target is the whole feature folder
`docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644`
with **no path exclusion of any kind**. The remediation plan file itself is inside that
folder and is therefore included in the sweep; commands 4 through 6 confirm it contributes
zero self-matches under each of the three patterns.

Redaction note: this artifact reproduces no raw absolute-path text, no account token, no
machine name, and no bare account spelling. The three remaining leak locations are cited by
file and line number only, with generic content descriptions.

## Pattern-delivery verification (micro-action, recorded for auditability)

The shape pattern is a 24-character regular expression whose third character class is written
with a doubled backslash. Passing it through the bash-to-native-executable argument layer with
a single level of doubling silently de-doubles it, producing a degraded 22-character pattern
whose class matches the forward slash only and cannot match a backslash separator. That
degraded pattern would return the same count as the correct one on this tree, because the two
present leaks use forward slashes, but it would be blind to any backslash-separated path.

Before running, the delivered pattern was compared for case-sensitive equality against a
pattern constructed inside PowerShell from `[char]92`, which cannot be affected by shell
escaping. The comparison returned `True` with `LEN=24`, confirming the regular expression the
sweep actually executed is character-for-character the pattern the plan mandates. The same
verified pattern is used for commands 1 and 4 and will be used for `[P1-T5]` and `[P3-T3]`.

## Command 1 — shape pattern, whole feature folder

Command: `@(Get-ChildItem -Recurse -File -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644 | Select-String -Pattern '[A-Za-z]:[\\/]Users[\\/]').Count`
EXIT_CODE: 0
Measured: `2`   Expected: `2`   Match: yes

Matching locations (file and line only):

- `policy-audit.2026-08-29T23-06.md` line 482 — a Windows absolute path naming the account and an agent-worktree identifier, quoted inside the PA-7 finding's `Content:` citation.
- `research/research.2026-08-29T07-55.md` line 5 — a Windows absolute path naming the account and an agent-worktree identifier, on the `- Worktree:` bullet.

## Command 2 — account-token pattern, whole feature folder

Command: `$t=[regex]::Escape((Split-Path -Leaf $env:USERPROFILE)); @(Get-ChildItem -Recurse -File -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644 | Select-String -Pattern "(?i)$t").Count`
EXIT_CODE: 0
Measured: `3`   Expected: `3`   Match: yes

Matching locations (file and line only):

- `policy-audit.2026-08-29T23-06.md` line 482 — as described above.
- `policy-audit.2026-08-29T23-06.md` line 483 — the PA-7 finding's `Verification:` bullet: a quoted search pattern naming a doubled-backslash absolute path, a bare account name, and the account's mail local-part.
- `research/research.2026-08-29T07-55.md` line 5 — as described above.

The account-token count exceeds the shape count by one because line 483 writes its path with
doubled backslashes and additionally carries bare account tokens with no drive-letter prefix.
The shape pattern's `[\\/]` class matches exactly one separator character, so a doubled
backslash presents a second separator where the pattern expects the literal `Users` and the
match fails; and a bare token carries no drive letter at all. Line 483 has therefore never
been a shape-pattern match, which is why it requires its own remediation task, `[P1-T4]`.

## Command 3 — machine-name pattern, whole feature folder

Command: `$m=[regex]::Escape($env:COMPUTERNAME); @(Get-ChildItem -Recurse -File -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644 | Select-String -Pattern "(?i)$m").Count`
EXIT_CODE: 0
Measured: `0`   Expected: `0`   Match: yes

Every file in the feature folder that references a host or TRX identifier already writes the
`<HOST>` placeholder rather than a real machine-name string. No remediation is required for
this pattern in this cycle. `[P3-T3]` re-measures it as an invariant-preservation check over
the artifacts written after this baseline.

## Command 4 — shape pattern, plan file self-match

Command: `@(Select-String -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644\remediation-plan.2026-08-29T23-23.md -Pattern '[A-Za-z]:[\\/]Users[\\/]').Count`
EXIT_CODE: 0
Measured: `0`   Expected: `0`   Match: yes

The plan file quotes the shape pattern's own literal text in several places and still returns
zero self-matches: the character immediately before the `:` in the pattern's printed form is
the class-closing `]`, which is outside the `[A-Za-z]` class, so the pattern is self-exempt by
construction rather than by exclusion.

## Command 5 — account-token pattern, plan file self-match

Command: `$t=[regex]::Escape((Split-Path -Leaf $env:USERPROFILE)); @(Select-String -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644\remediation-plan.2026-08-29T23-23.md -Pattern "(?i)$t").Count`
EXIT_CODE: 0
Measured: `0`   Expected: `0`   Match: yes

The plan file never spells the account token; it derives it at run time.

## Command 6 — machine-name pattern, plan file self-match

Command: `$m=[regex]::Escape($env:COMPUTERNAME); @(Select-String -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644\remediation-plan.2026-08-29T23-23.md -Pattern "(?i)$m").Count`
EXIT_CODE: 0
Measured: `0`   Expected: `0`   Match: yes

The plan file never spells the machine name; it derives it at run time.

## REMEDIATION-REQUIRED status

None. All three whole-folder counts equal their expected values (`2`, `3`, `0`), so the
reporting branch this task authorizes is not triggered and Phase 1 proceeds.

## Output Summary

Six commands run, all EXIT_CODE 0. Measured counts `2`, `3`, `0`, `0`, `0`, `0`, matching the
six expected values exactly. Three PA-7 instances remain open at baseline, at
`research/research.2026-08-29T07-55.md` line 5, `policy-audit.2026-08-29T23-06.md` line 482,
and `policy-audit.2026-08-29T23-06.md` line 483; these are remediated by `[P1-T2]`, `[P1-T3]`,
and `[P1-T4]` respectively. The machine-name pattern is already at zero and needs no
remediation. The plan file contributes zero self-matches under all three patterns and needs no
exclusion from its own sweep. The shape pattern was verified character-for-character against a
shell-independent construction before use.
