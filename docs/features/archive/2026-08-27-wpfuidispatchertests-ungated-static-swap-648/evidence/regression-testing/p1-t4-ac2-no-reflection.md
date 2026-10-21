# P1-T4 — AC-2 Verified by Measurement (No Reflection in the Test File)

Timestamp: 2026-09-01T14-07

Command:
```
grep -c "GetField" QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs
grep -c "SetValue" QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs
grep -c "using System.Reflection;" QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs
```
plus an independent ripgrep-family count of the same three tokens against the same single file.

EXIT_CODE: 0

Output Summary:

All three tokens return zero matches under both methods. Six measurements, all zero:

| Token | Method one (`grep -c`) | Method two (ripgrep-family count) |
|---|---|---|
| `GetField` | 0 | 0 |
| `SetValue` | 0 | 0 |
| `using System.Reflection;` | 0 | 0 |

Method two reported `No matches found` and `Found 0 total occurrences across 0 files.` for each of the
three searches.

Baseline for comparison, from
`docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648/evidence/baseline/p0-t16-structural-counts.md`:
before the change the same file carried `using System.Reflection;` at `:1`, `GetField` at `:42`, and
`SetValue` at `:51` and `:83`. All four occurrences are gone.

AC-2 states that `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs` contains no occurrence of
`GetField`, no occurrence of `SetValue`, and no `using System.Reflection;` directive. That condition
holds.
