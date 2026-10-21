# QA Gate — The #462 Two-Flag Field Split Is Complete (P6-T6, AC-01)

Timestamp: 2026-08-27T20-53

## Search scope

Both commands are scoped to the single file `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs`.
The scoping is deliberate and load-bearing: the old identifier also appears in this feature's own
documents (`spec.md`, the research document, this plan, and several evidence artifacts) and in tracked
agent-memory files, so a repository-wide zero-hit assertion would be unsatisfiable by construction
however correct the production change was.

## Command 1 — the old field is gone

Command: `git grep -F -n '_closePending' -- QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs`

EXIT_CODE: 1 (`git grep` exits 1 on no match)

Output: **empty. Zero matched lines.**

Required: zero matched lines. **SATISFIED.**

Note recorded for the audit trail: an earlier draft of the replacement fields carried an XML doc comment
that referred to the old identifier by name inside a `<c>` tag, which produced one match and would have
failed this gate. The comment was reworded to say "the single close flag these two replace" instead. The
production behaviour was identical either way; only the comment text changed.

## Command 2 — the new completed-flag is present

Command: `git grep -F -n '_closeCompleted' -- QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs`

EXIT_CODE: 0

Output, verbatim — **6** matched lines:

```
QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs:46:        private bool _closeCompleted;
QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs:114:                _closeCompleted = false;
QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs:304:        /// distinct meanings (see <see cref="_closeInFlight"/> and <see cref="_closeCompleted"/>), and
QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs:316:                if (_closeCompleted)
QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs:335:                    _closeCompleted = true;
QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs:352:                _closeCompleted = false;
```

Required: at least three matched lines. Observed: 6. **SATISFIED.**

## What the six matches are

| Line | Role |
| ---: | --- |
| 46 | the field declaration, carrying its own XML doc comment stating its distinct meaning |
| 114 | cleared by `RequestOpen`, so a legitimate reopen is not suppressed |
| 304 | the `CloseCore` doc comment cross-referencing both flags |
| 316 | the repeated-close suppression guard (`if (_closeCompleted) return true;`), which preserves I-462.3 |
| 335 | set on the successful-close path, inside the same `lock` that increments `_generation` |
| 352 | cleared by `Invalidate`, covering `Reset()` and `Release()` |

The companion field `_closeInFlight` has 8 matches in the same file (verified at P1-T5, where the
acceptance required at least four). It is latched before `_host.Close(reason)` and cleared in a
`finally` around that call, so it reads `false` on the success, not-closed, throw and released exits
alike — which is the I-462.1 half of AC-01.

Together the two flags carry the two jobs the single replaced flag was doing at once: suppressing a
concurrent close of an in-flight operation, and suppressing a repeated close of an already-closed host.
Separating them is what lets a legitimate reopen through (I-462.2) while keeping idempotent close
(I-462.3), as the P1-T6 run demonstrates behaviourally.
