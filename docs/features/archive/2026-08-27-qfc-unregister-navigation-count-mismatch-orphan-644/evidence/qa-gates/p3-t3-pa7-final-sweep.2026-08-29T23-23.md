# [P3-T3] — Final PA-7 Containment Sweep (three patterns, two runs)

Timestamp: 2026-08-29T23-23
Run performed: 2026-08-30T01-17
Task: [P3-T3]
Working directory: `<repo-root>` (the repository root of this worktree)
EXIT_CODE: 0 (all six command executions across both runs)

Scope: the whole feature folder
`docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644`, with
**no path exclusion of any kind**. This plan file and every evidence artifact this cycle has
written are inside that folder and are included.

Redaction note: this artifact reproduces no raw absolute-path text, no account token, and no
machine name.

## Why this task exists

`[P1-T5]` did not run at the end of Phase 1 — `[P1-T6]` follows it — and `[P1-T6]`, Phase 2, and
Phase 3 together write eight further artifacts into the very folder `[P1-T5]` swept
(`[P1-T6]`, `[P2-T1]`, `[P2-T2]`, `[P2-T3]`, `[P2-T4]`, `[P2-T5]`, `[P3-T1]`, and `[P3-T2]`), so
`[P1-T5]` cannot fail for any leak those artifacts introduce.

`[P1-T6]`'s own artifact carries no host path or account token — it records a plain test-string
edit — so it does not retroactively invalidate `[P1-T5]`'s zero counts. The remaining seven are
the highest-risk artifacts in the plan: the worktree root is itself an absolute path under the
user profile, msbuild echoes full project paths in its per-project output, and the default
`vstest.console.exe` TRX filename embeds both the account and the machine name. Each of those
seven artifacts was written with a generic repository-root placeholder, a generic account
placeholder, and a generic host placeholder, and the TRX filename is cited only in redacted
form.

## Pattern-delivery verification

The shape pattern delivered to the search engine was compared for case-sensitive equality
against a pattern constructed inside PowerShell from `[char]92`, which no shell escaping layer
can alter. The comparison returned `True` with `LEN=24`, confirming the executed regular
expression is character-for-character the pattern the plan mandates and not a de-doubled
variant whose class would match the forward slash only.

## Commands

1. `@(Get-ChildItem -Recurse -File -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644 | Select-String -Pattern '[A-Za-z]:[\\/]Users[\\/]').Count`
2. `$t=[regex]::Escape((Split-Path -Leaf $env:USERPROFILE)); @(Get-ChildItem -Recurse -File -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644 | Select-String -Pattern "(?i)$t").Count`
3. `$m=[regex]::Escape($env:COMPUTERNAME); @(Get-ChildItem -Recurse -File -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644 | Select-String -Pattern "(?i)$m").Count`

## Run 1 — before this artifact existed on disk

EXIT_CODE: 0 (all three commands)

| Pattern | Baseline in `[P0-T3]` | Run 1 | Required |
|---|---|---|---|
| Shape | `2` | `0` | `0` |
| Account-token | `3` | `0` | `0` |
| Machine-name | `0` | `0` | `0` |

Files swept in run 1: `66`.

The machine-name pattern's measured baseline at cycle entry was `0`, recorded in `[P0-T3]`,
because every file in the feature folder that references a host or TRX identifier already writes
the `<HOST>` placeholder. This run confirms the seven artifacts written by `[P1-T6]` through
`[P3-T2]` did not introduce a machine name. `[P1-T5]` deliberately omits this pattern, because a
pattern whose baseline is already `0` cannot demonstrate a fix; this task is a final
invariant-preservation sweep rather than a before-and-after remediation check, and a steady `0`
here is exactly what it is designed to confirm.

## Run 2 — with this artifact present on disk

All three patterns were re-run after this artifact was written to disk. Its presence was
confirmed at the start of the second run with `Test-Path`, which returned `True`.

EXIT_CODE: 0 (all three commands)

| Pattern | Baseline in `[P0-T3]` | Run 1 | Run 2 | Required |
|---|---|---|---|---|
| Shape | `2` | `0` | `0` | `0` |
| Account-token | `3` | `0` | `0` | `0` |
| Machine-name | `0` | `0` | `0` | `0` |

Files swept in run 2: `67`, one more than run 1's `66`. The difference is this artifact itself,
which is the object the second run exists to observe.

The shape pattern was verified canonical again before run 2, returning `True` with `LEN=24`.

## Acceptance

The **second** run of all three patterns must return `0`. The second run is required because
this task's own artifact lands inside the folder it sweeps, so a single run cannot observe it.

Measured on the second run: `0`, `0`, `0`. Required: `0`, `0`, `0`. Result: **PASS**

No pattern returned nonzero on either run, so the repeat-and-redact branch this task authorizes
is not triggered and `[P3-T4]` proceeds.

## Confirmatory third run — after run 2's results were recorded

The plan requires the second triple of counts to be recorded in this artifact, which is itself a
write into the swept folder occurring after run 2. To avoid asserting the freeze rather than
observing it, all three patterns were run a third time after that text was written.

Measured on the third run: `0`, `0`, `0`, over the same `67` files. The shape pattern was again
verified canonical. The recorded run-2 text therefore introduced none of the three patterns, and
the folder is confirmed clean in the exact state that carries this artifact's full contents.

## State frozen at this point

Nothing that can carry a host path, an account name, or a machine name lands after this second
sweep run. Exactly two single-character checkbox flips happen between it and the commit — this
task's own checkbox, flipped from `- [ ]` to `- [x]` on completion, and `[P3-T4]`'s own
checkbox, flipped the same way when it runs — and neither can introduce any of the three
patterns. `[P3-T4]` writes no evidence artifact of its own, so the state observed by run 2 is,
apart from those two flips, exactly the state the commit stages.

## Output Summary

Final containment sweep passed. All three patterns — shape, account-token, and machine-name —
returned `0` over the whole feature folder with no path exclusion, on both runs. The shape and
account-token patterns are down from the `2` and `3` recorded at the `[P0-T3]` baseline; the
machine-name pattern held at its baseline `0`, confirming the seven high-risk artifacts written
by `[P1-T6]` through `[P3-T2]` introduced no machine name despite msbuild echoing full project
paths and the default TRX filename embedding both the account and the host. Run 1 swept 66
files, run 2 swept 67 including this artifact, and both returned the same all-zero triple.

