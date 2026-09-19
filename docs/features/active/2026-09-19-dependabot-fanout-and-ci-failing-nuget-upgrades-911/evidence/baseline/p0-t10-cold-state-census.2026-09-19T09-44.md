# P0-T10 — Cold-Cache Precondition Census

Timestamp: 2026-09-19T12-36

Command:
```
pwsh -NoProfile -Command 'Set-Location "C:\Users\DanMoisan\repos\TaskMaster-wt\dependabot-911";
  Test-Path -LiteralPath ".\packages\Meziantou.Analyzer.3.0.235" -PathType Container;
  Test-Path -LiteralPath ".\packages\Meziantou.Analyzer.3.0.203" -PathType Container;
  Get-ChildItem -Path ".\packages" -Directory -Filter "Meziantou.Analyzer.*" |
      Select-Object -ExpandProperty Name | Sort-Object'
```

EXIT_CODE: 0

## Recorded booleans

| Directory | Exists |
|---|---|
| `packages/Meziantou.Analyzer.3.0.235` | **True** |
| `packages/Meziantou.Analyzer.3.0.203` | **False** |

## Full sorted match list for `Meziantou.Analyzer.*` under `packages/`

```
Meziantou.Analyzer.3.0.235
```

Member count: **1**.

## Acceptance evaluation

- The `3.0.235` directory exists — `True`. PASS.
- The `3.0.203` directory does not exist — `False`. PASS.
- The recorded match list has exactly one member — count 1, the single name
  `Meziantou.Analyzer.3.0.235`. PASS.

**Why the positive member count is the non-vacuity guard.** A census that enumerated nothing — a
wrong root, a mistyped filter, an absent `packages/` tree — would also report the `3.0.203`
directory absent, and the absence assertion alone would pass for a reason unrelated to the property
it asserts. The match list is therefore asserted positively at exactly one member, naming that
member, so an empty enumeration fails.

## Relationship to the AC6 failing state

This is the cold-cache precondition the AC6 failing direction depends on. The restored package tree
ships `Meziantou.Analyzer.3.0.235` and does **not** ship `3.0.203`, while 15 `*.csproj` files still
carry an `<Analyzer Include>` naming `Meziantou.Analyzer.3.0.203` — the #898 defect. The failure is
caused by the absent `3.0.203` directory rather than by an absent `packages/` tree, which is why the
AC6 failing state is still the current state even though `packages/` holds 172 restored package
directories (P0-T7).

Output Summary: `packages/Meziantou.Analyzer.3.0.235` exists, `packages/Meziantou.Analyzer.3.0.203`
does not, and the sorted `Meziantou.Analyzer.*` directory list has exactly one member,
`Meziantou.Analyzer.3.0.235`. All three acceptance clauses hold, and the positive member count
rules out a vacuous enumeration.
