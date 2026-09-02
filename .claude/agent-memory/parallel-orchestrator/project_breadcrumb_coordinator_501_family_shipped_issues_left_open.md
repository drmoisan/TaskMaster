---
name: breadcrumb-coordinator-501-family-shipped-issues-left-open
description: Issues 462, 500, 502 are delivered on main under the #501 breadcrumb-coordinator feature but still OPEN — fourth confirmed family; #462 verified to guard-site depth with all 32 spec ACs checked
metadata:
  type: project
---

Four defect issues — #501, #462, #500, #502 — were fixed and merged under the SINGLE feature
`breadcrumb-coordinator-hub-defects-501` (PR #659, merge `4cb709db`, single fix commit
`2434f07f`). Only **#501 is CLOSED**; #462, #500 and #502 remain OPEN as bookkeeping debt.

The commit subject is `fix(breadcrumb): enforce close, lifetime, broadcast and lease invariants` —
it names no issue at all, not even a feature slug. **Only the merge-commit subject line and the
fix-commit body carry the numbers**, so `git log origin/main --grep="462"` returns the merge and
the fix commit, and `--grep="fix(462)"` returns nothing. The fix commit body enumerates one bullet
per issue, which is the fastest read of who got what.

**#462 is verified to guard-site depth** (2026-08-31, against `origin/main`, on `/parallel-add
462`). The issue asked for `_closePending` to stop latching `true` after a successful close and
silently dropping a legitimate `RequestOpen`. On `main`:

- `_closePending` has **0 occurrences** in `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs`.
  It was split into `_closeInFlight` (`:36`) and `_closeCompleted` (`:46`), each with an XML doc
  comment giving the separation reason.
- `RequestOpen` (`:104`) guards on `if (_closeInFlight && _host.IsOpen)` (`:112`) — the in-flight
  flag only — then clears `_closeCompleted = false` (`:114`) before starting the open. That is
  exactly the remedy the issue's Expected Behavior section demanded.
- `CloseCore` preserves the repeated-close suppression the two must-pass tests encode:
  `_closeInFlight` early-return (`:314`), `_closeCompleted` early-return (`:316`), latch (`:318`),
  `_closeInFlight = false` in a `finally` (`:328`), `_closeCompleted = true` on the success path
  (`:335`). `Invalidate` clears `_closeCompleted` (`:352`).

Residual scope is closed: **all 32 acceptance criteria in
`docs/features/active/breadcrumb-coordinator-hub-defects-501/spec.md` are `[x]` and the file has
zero `- [ ]` lines of any kind**. AC-01/AC-02/AC-03 (invariants I-462.1 through I-462.5) and AC-16
(the regression test in `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs`)
are the #462 rows. The whole pre-check ran in six tool calls with no preparation delegated.

**#656 is NOT #462 residual scope.** The fix commit records two follow-ups as real issues rather
than prose — #655 (non-re-entrant upgrade-lifetime guard) and #656 (`_closeCompleted` residual
outside `RequestOpen`/`Invalidate`, owned by feature 488). Both are still OPEN. Because they were
promoted to their own issues, they do not leave #462 partially delivered; treat #656 as a separate
admissible candidate on its own merits, not as a reason to admit #462.

**#500 and #502 are NOT verified to that depth** — only their fix-commit bullets were read. Do the
guard-site read before rejecting either.

**Why:** Fourth confirmed family, after [[qfc-collection-468-family-shipped-issues-left-open]],
[[efc-464-family-shipped-issues-left-open]] and
[[webview2-host-476-family-shipped-issues-left-open]]. This one adds a new variant: the constituent
commit subject names **neither the issue nor a feature slug**, so even the slug heuristic would
have missed it and only the bare-number grep found it.

**How to apply:** Treat #462, #500, #502 as presumptively delivered and run the delivery pre-check
in [[verify-delivery-before-preparing-an-admission]] before preparing any of them. Re-verify before
relying on this; the memory goes stale the moment someone closes them.
