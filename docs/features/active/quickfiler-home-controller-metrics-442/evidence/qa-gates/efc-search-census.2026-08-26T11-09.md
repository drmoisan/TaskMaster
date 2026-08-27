# Phase 2 — EFC Post-Fix Search Census

Timestamp: 2026-08-26T11-09
Task: [P2-T10]
Command: five `git grep -n` invocations, each listed with its own result below
EXIT_CODE: 0

This artifact is the post-fix half of the grep-based acceptance criteria AC-10, AC-12, and AC-15,
and it supplies the `Stopwatch.StartNew` reading that AC-9 and [P7-T2] cite. Its pre-fix
counterpart is `evidence/baseline/defect-site-census.2026-08-26T10-42.md`.

## Output Summary

| # | Search | Scope | Pre-fix hits | Post-fix hits | Required |
| --- | --- | --- | --- | --- | --- |
| 1 | `Elapsed.Seconds` | `QuickFiler/Controllers/EfcHomeController.Metrics.cs` | 1 | **0** | zero |
| 2 | `int elapsedSeconds` | `QuickFiler/` | 2 | **0** | zero |
| 3 | `NotImplementedException` | `QuickFiler/Controllers/EfcHomeController.Metrics.cs` | 1 | **0** | zero |
| 4 | `RecipientSender` | `QuickFiler.Test/` | 1 | **0** | zero |
| 5 | `Stopwatch.StartNew` | `QuickFiler/Controllers/EfcHomeController.cs` | 1 | **3** | exactly 3 at lines 76, 176, 225 |

The first four searches each return zero hits and the fifth returns exactly three, so the task's
acceptance condition holds.

### 1. `git grep -n "Elapsed.Seconds" -- QuickFiler/Controllers/EfcHomeController.Metrics.cs`

No output; `git grep` exited 1 (no match). The 0-59 `Seconds` component read is gone, replaced by
`TotalSeconds` by [P2-T3].

### 2. `git grep -n "int elapsedSeconds" -- QuickFiler/`

No output; exit 1. Both parameters were widened to `double elapsedSeconds` by [P2-T4], at
`EfcHomeController.Metrics.cs:41` and `:63`. Both declaring members are `internal`, so no public
API changed.

### 3. `git grep -n "NotImplementedException" -- QuickFiler/Controllers/EfcHomeController.Metrics.cs`

No output; exit 1. The single-argument `QuickFileMetrics_WRITE(string filename)` is now guarded
delegation to the three-argument overload rather than a throw. The interface member at
`QuickFiler/Interfaces/IFilerHomeController.cs:41` is unchanged, and its signature is unchanged.

Note that `NotImplementedException` still occurs elsewhere in the partial class, at
`QuickFiler/Controllers/EfcHomeController.cs:391` for the `Loaded` property. That site is outside
this search's scope, outside this feature's scope, and is not affected by any change here.

### 4. `git grep -n "RecipientSender" -- QuickFiler.Test/`

No output; exit 1. The concatenated substring in the expected literal was replaced with the
separated substring `,Recipient,Sender,` by [P1-T1].

### 5. `git grep -n "Stopwatch.StartNew" -- QuickFiler/Controllers/EfcHomeController.cs`

```
QuickFiler/Controllers/EfcHomeController.cs:76:                _stopWatch = Stopwatch.StartNew();
QuickFiler/Controllers/EfcHomeController.cs:176:            var selectionStopwatch = Stopwatch.StartNew();
QuickFiler/Controllers/EfcHomeController.cs:225:            _stopWatch = Stopwatch.StartNew();
```

Exactly three hits, at lines 76, 176, and 225. Lines 76 and 225 are the two `_stopWatch`
construction sites that [P2-T2] converted from an allocated-but-never-started `new Stopwatch()`.
Line 176 is the pre-existing `selectionStopwatch` call in the selection-change path; it is
unrelated to `_stopWatch` and was not modified.
