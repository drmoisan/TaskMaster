# P4-T11 — No catch clause for the base cancellation type in the production Write Set

Timestamp: 2026-09-13T03-19

Command: the plan's fixed search-gate form applied to the literal `catch (OperationCanceledException` in each of the two production Write Set files, together with the literal `catch (TaskCanceledException` in the first file as the in-task positive control for the search mechanism.

EXIT_CODE: 0

```
OCE_CATCH_TA=0
OCE_CATCH_FAILURES=0
CTRL_CATCH_TCE=3
```

Output Summary: all three acceptance clauses hold. Neither production file carries a catch clause for the base cancellation type, and the control count of 3 shows the search mechanism can find a catch clause in the very file the two zero counts are asserted against, so neither zero is the result of a search that cannot match. The control value of 3 is the same value P0-T21 recorded before the change, confirming the change added and removed no `TaskCanceledException` catch clause.

Both searches are scoped to explicit file paths. That scoping is mandatory rather than tidy: the literal also occurs in `spec.md` inside acceptance criterion 6's own text and in the plan file, so an unscoped repository-wide search could never reach zero and the gate would be unsatisfiable however correct the source.

The design reason the count must be zero is that cancellation is propagated rather than caught. The cancellation branch of the task-cancelled catch rethrows with a bare `throw;`, and the final guard raises cancellation through `token.ThrowIfCancellationRequested()` before it reports a timeout. Adding a catch for the base type would have been the alternative design and would have risked converting a caller's cancellation into a locally-decided outcome; the delivered shape does not do that. Together with P4-T18 this decides acceptance criterion 6.
