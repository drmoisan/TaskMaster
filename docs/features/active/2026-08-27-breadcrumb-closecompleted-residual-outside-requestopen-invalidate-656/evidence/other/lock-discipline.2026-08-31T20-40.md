# SR-4 Lock-Discipline Invariant After the Production Edit (Issue #656)

Timestamp: 2026-09-01T14-45
Task: [P2-T5]

Command:
```
Select-String -Path QuickFiler\Viewers\BreadcrumbDropDownOpenCoordinator.cs -Pattern '^\s*[^/\s].*_host\.'
Select-String -Path QuickFiler\Viewers\BreadcrumbDropDownOpenCoordinator.cs -SimpleMatch 'lock (_sync)'
```

EXIT_CODE: 0

## Counts against the P0-T12 baseline

| Measurement | Baseline (P0-T12) | Post-change | Expected | Match |
|---|---|---|---|---|
| Non-comment `_host.` lines | 5 | 6 | 6 (baseline 5 plus the one hoisted read) | yes |
| `lock (_sync)` occurrences | 12 | 12 | 12 (unchanged) | yes |

## Enumerated non-comment `_host.` lines (6)

- L119: `if (_closeInFlight && _host.IsOpen)`
- L200: `(!_host.IsOpen || !_host.Close(BreadcrumbDropDownCloseReason.Uncommitted))`
- L265: `? _host.OpenAsync(anchor, workingArea(), size)`
- L266: `: _host.OpenAsync(anchor, workingArea(), size, takeFocus: false);`
- L326: `bool hostOpen = _host.IsOpen;`   <- the line added by this change
- L340: `closed = _host.Close(reason);`

## Enumerated `lock (_sync)` lines (12)

L91, L103, L113, L141, L154, L244, L327, L344, L349, L363, L377, L385.

## Lock-body membership of each non-comment `_host.` line

Determined by reading each enclosing region directly rather than by a brace-counting heuristic,
because a heuristic misclassified three of these lines on a first attempt.

| Line | Enclosing member | Inside a `lock (_sync)` body? | Basis |
|---|---|---|---|
| L119 | `RequestOpen` | **yes** | The lock opens at L113 and closes at L124; L119 lies between them. |
| L200 | `Reset` | no | Inside the `_operations.PostAsync` lambda opened at L197. No lock is held there. |
| L265 | `OpenCoreAsync` | no | The lock opened at L244 closes at L254; L265 follows it. |
| L266 | `OpenCoreAsync` | no | Same block as L265, after the lock closed at L254. |
| L326 | `CloseCore` | no | It is the first statement of the method body (opening brace L325) and precedes the `lock (_sync)` at L327. |
| L340 | `CloseCore` | no | The lock opened at L327 closes at L336; L340 sits in the `try` block that follows. |

**Exactly one** non-comment `_host.` line sits inside a `lock (_sync)` body: the pre-existing
`if (_closeInFlight && _host.IsOpen)` at L119 in `RequestOpen`. Every other such line, including the
line this change added, is outside every lock body.

## Why this is the SR-4 invariant

SR-4 of #501 declined the refinement written as an `_host.IsOpen` read taken *inside* `_sync`, on
the ground that it enlarges the set of foreign calls made while the coordinator's lock is held.
`IsOpen` is an interface member and the coordinator holds an `IBreadcrumbDropDownHost` rather than
the concrete class, so a substituted implementation could take its own lock or re-enter the
coordinator from inside `_sync`.

This change places the read at L326, before the lock is acquired at L327, and only a `bool` local
crosses into the critical section. The count of foreign calls made while `_sync` is held is
therefore unchanged at one, and it is the same pre-existing call it was before the change. SR-4 is
neither overridden nor contradicted: its stated objection does not apply to the hoisted form, which
was not among the shapes SR-4 evaluated.

## Note on the search pattern's coverage

The pattern `^\s*[^/\s].*_host\.` requires a character before the `_host.` token on the same line,
so a line whose first non-whitespace token *is* `_host.` does not match it. One such line exists in
the file, `_host.Reset();` in the `Reset` continuation, and it is absent from both the baseline
count of 5 and the post-change count of 6. This does not affect the invariant: that line is inside
the same `_operations.PostAsync` lambda as L200 and is likewise outside every lock body, so the
"exactly one inside a lock" conclusion holds whether or not it is counted. The same pattern was used
for the baseline and for this measurement, so the delta of exactly one is a like-for-like
comparison.

Output Summary: The lock-discipline invariant holds. Non-comment `_host.` lines went from 5 to 6,
exactly the one hoisted read; `lock (_sync)` count is unchanged at 12; and exactly one non-comment
`_host.` line sits inside a lock body, the pre-existing one at L119 in `RequestOpen`. No new foreign
call under `_sync` was introduced.


## Declared Member Lines:

Task: [P2-T6]
Timestamp: 2026-09-01T14-46

Command:
```
Select-String -Path QuickFiler\Viewers\BreadcrumbDropDownOpenCoordinator.cs -Pattern '^\s+(internal|public)\s'
```

EXIT_CODE: 0

Count: **12**, unchanged from the P0-T12 baseline of 12.

Enumerated lines:

- L12: `internal sealed class BreadcrumbDropDownOpenCoordinator`
- L58: `internal BreadcrumbDropDownOpenCoordinator(`
- L85: `internal IBreadcrumbDropDownHost Host => _host;`
- L87: `internal Task<bool> CurrentOpenTask`
- L96: `internal void UpdateRequestProviders(`
- L111: `internal Task<bool> RequestOpen()`
- L139: `internal void LatchNextOpenTakesNoFocus()`
- L150: `internal bool NextOpenTakesNoFocus`
- L159: `internal void SetDroppedDown(bool droppedDown)`
- L178: `internal void HandleSelectorOpenStateChanged()`
- L193: `internal void Reset()`
- L209: `internal void Release()`

The member set is identical to the baseline set; only the line numbers shifted, by the number of
documentation lines this change inserted above each declaration. No `internal` or `public` member
was added, so no new production seam was introduced. `CloseCore` remains `private`, and the change
adds only a method-local `bool`.

The pattern excludes XML documentation lines because a `///` line's first non-whitespace character
is a forward slash, which `^\s+(internal|public)\s` cannot match. The `remarks` blocks added by
P2-T3 and P2-T4 therefore cannot inflate this count.

Output Summary: Declared member count is 12, unchanged from baseline. No new production seam.
