# P0-T3 — Defect measurement and document baselines (cycle 1)

Timestamp: 2026-08-28T03-41
Task: [P0-T3]
Command: git grep -F -n "_itemViewer.PicturesChanged -= this.CbxPictures_CheckedChanged;" -- QuickFiler/
EXIT_CODE: 1
ExpectedExitCode: 1

`git grep` exits `1` when it finds no match. A zero-match result is exactly what this task asserts, so
the non-zero exit is the expected outcome and is declared as such.

## The imbalance, measured

Member bounds re-derived from the file at REM_BASE, not carried forward from any earlier artifact:

- `internal void WireIntentEvents()` is declared at `QuickFiler/Controllers/QfcItemController.EventWiring.cs:66`; its body ends at `:95`.
- `internal void UnwireIntentEvents()` is declared at `:446`; its body ends at `:481`.

| Measurement | Range | Value |
|---|---|---:|
| Raw `+=` line count over `WireIntentEvents()` | 66-95 | **18** |
| Live `+=` subscription statements (comment lines excluded) | 66-95 | **17** |
| Raw `-=` line count over `UnwireIntentEvents()` | 446-481 | **16** |
| Live `-=` detachment statements (comment lines excluded) | 446-481 | **16** |

Both raw and live figures are recorded for the wire side because they differ. The single line that
inflates the raw count is a commented-out subscription and is not a subscription:

```
//_itemViewer.TxtboxSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxSearch_KeyDown);
```

On the unwire side raw and live agree at 16 — that member contains no commented-out `-=` line.

**17 subscriptions against 16 detachments.** That one-line gap is RC-1.

## The missing detachment is absent from the whole production tree

```
git grep -F -n "_itemViewer.PicturesChanged -= this.CbxPictures_CheckedChanged;" -- QuickFiler/
```

Match count: **0**. The command printed no output line.

**Zero-match residual, stated explicitly.** A `git grep` that finds nothing exits `1`, and piping such
a command into a counting stage does not clear the shell's last-exit-code variable — the native `1`
survives the pipe. The verdict here is therefore taken from the **reported match count of 0**, and the
PowerShell measurement block that produced the line and checkbox counts below reported `$?` = `True`
and `$Error.Count` = `0` under `$ErrorActionPreference = 'Stop'`. No bare zero exit code is claimed
for the grep: its exit code is `1`, it is expected to be `1`, and `ExpectedExitCode: 1` is declared
above.

## File line counts at REM_BASE

Measured with `(Get-Content -LiteralPath <path>).Count`:

| File | Lines | Ceiling headroom |
|---|---:|---:|
| `QuickFiler/Controllers/QfcItemController.EventWiring.cs` | **483** | 17 |
| `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs` | **81** | 419 |

Both match the planning-time figures exactly. The production file grows by one line in P2-T1 (to 484,
16 spare) and the test file by the one appended test method in P1-T1, both well inside the 500-line
ceiling.

## CheckboxBaselineCount:

CheckboxBaselineCount: 62

Measured over `docs/features/active/itemviewer-surface-defects-489/spec.md` as the count of lines
matching the regular expression `^- \[[ x]\] `. This measured value — not the expected value —
governs the P3-T2 equality gate: after the spec amendment, the same measurement must return the same
number.

## Acceptance

| P0-T3 condition | Result |
|---|---|
| Subscription count 17 recorded | **Yes** — live 17, raw 18, with the commented-out line identified |
| Detachment count 16 recorded | **Yes** — live 16, raw 16 |
| Zero matches for the detachment literal | **Yes** — match count 0, `EXIT_CODE: 1`, `ExpectedExitCode: 1` declared |
| Both file line counts recorded | **Yes** — 483 and 81 |
| `CheckboxBaselineCount:` recorded | **Yes** — 62 |

Output Summary: `WireIntentEvents()` (`:66-95`) performs **17** live subscriptions — 18 raw `+=`
lines, one of which is a commented-out `TxtboxSearch.KeyDown` line — while `UnwireIntentEvents()`
(`:446-481`) performs **16** live detachments, raw and live agreeing. The detachment literal
`_itemViewer.PicturesChanged -= this.CbxPictures_CheckedChanged;` returns **0** matches anywhere under
`QuickFiler/`; the grep exits `1` on that zero-match result, which is declared as
`ExpectedExitCode: 1` and judged from the match count rather than the exit code, with the PowerShell
measurement block reporting `$?` = `True` and `$Error.Count` = `0`. Line counts at REM_BASE:
`QfcItemController.EventWiring.cs` **483**, `QfcItemController.EventWiringTests.Part2.cs` **81**.
`CheckboxBaselineCount: 62`.
