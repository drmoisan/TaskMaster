# Phase 0 — Baseline consumer-side catch blocks (AC15 precondition)

Timestamp: 2026-09-09T12-47
Task: [P0-T14]

Command:

```text
pwsh -NoProfile -Command 'Select-String -SimpleMatch -Path "QuickFiler/Controllers/QfcHomeController.cs" -Pattern "catch (System.Exception e)"'
```

EXIT_CODE: 0

Verbatim output:

```text
count=2
382: catch (System.Exception e)
399: catch (System.Exception e)
```

## Baseline matches

| # | Line | Text |
|---|---|---|
| 1 | **382** | `catch (System.Exception e)` |
| 2 | **399** | `catch (System.Exception e)` |

Exactly two matches, at lines 382 and 399 — the values AC15 pins. `-SimpleMatch` is required because
the pattern contains regular-expression metacharacters `(`, `)` and `.`; in regex mode it would not
mean what it reads as.

Structural context, read from the file: `Cleanup` spans lines 371-408. The first `catch` at 382-385
closes the detach-worker-completed stage; the second at 399-402 closes the datamodel-and-fields
stage; the `finally` opens at 403 and closes at 407. Neither `catch` encloses the `finally`, and the
edit region this plan touches begins at line 405, below both.

Output Summary: exactly two matches recorded, at lines 382 and 399. `[P2-T5]` re-runs this command
after the Site A edit and requires the same two line numbers unchanged, which holds if and only if
the edit stayed at or below line 405 and did not restructure the try/catch/finally. Widening either
catch to enclose the `finally` would break the live test
`Cleanup_DatamodelCleanupThrows_StillInvokesParentCleanup`, which pins the opposite behaviour.
