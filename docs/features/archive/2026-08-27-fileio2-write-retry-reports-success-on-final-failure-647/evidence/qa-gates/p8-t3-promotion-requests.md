# P8-T3 — Promotion Requests for the Three Deferred Items

Timestamp: 2026-08-31T21-10
EXIT_CODE: 0

**This artifact is a request record, not a performed promotion.** This executor has no promotion MCP tool and no `gh`, so it does not itself run the feature-promotion lifecycle. Its deliverable is this record. **The orchestrator performs the MCP promotion from this record.**

The three items and their values are taken verbatim from the "Out of scope / non-goals (follow-up candidates, to be promoted separately)" list under Scope & Non-Goals in `spec.md`, and from the matching bullet under Rollout & Follow-up. They were not chosen by the executor.

## Entry 1

- **Short name:** `narrow-fileio2-retryable-exception-set`
- **Promotion type:** `bug`
- **Work mode:** `full-bug`
- **Rationale:** `DirectoryNotFoundException` derives from `IOException`, so an absent folder consumes the full 100-attempt window even though it can never succeed. Excluding it would remove that stall, but it is a behavior change beyond the issue's stated Expected Behavior, and the QuickFiler call site already guards on `Globals.FS.SpecialFolders.TryGetValue("MyDocuments", ...)` before writing, so the case is not reachable there.

## Entry 2

- **Short name:** `supported-async-text-writer-for-to-depricate-migration`
- **Promotion type:** `feature`
- **Work mode:** `full-feature`
- **Rationale:** No supported async text writer exists in the repository today, so deleting `FileIO2.WriteTextFileAsync` and migrating its callers is a new capability rather than a bug fix. This is the issue's own closing suggestion and the correct long-term disposition of the `To Depricate` folder, but it would expand #647 well past its stated scope.

## Entry 3

- **Short name:** `remove-unnecessary-interlocked-increment-in-fileio2`
- **Promotion type:** `feature`
- **Work mode:** `minor-audit`
- **Rationale:** The counter is a method-local captured by the async state machine and is never touched concurrently, so the interlocked call is unnecessary but harmless and the change is cosmetic. `Interlocked.Increment(ref attempts)` was deliberately retained by this change; the spec lists replacing it as out of scope and it must not be folded in.

## Summary

Three entries. Each carries a short name, a promotion type drawn from the two values `bug` and `feature`, a work mode drawn from the three values `minor-audit`, `full-feature` and `full-bug`, and a rationale sentence.

| Short name | Type | Work mode |
|---|---|---|
| `narrow-fileio2-retryable-exception-set` | `bug` | `full-bug` |
| `supported-async-text-writer-for-to-depricate-migration` | `feature` | `full-feature` |
| `remove-unnecessary-interlocked-increment-in-fileio2` | `feature` | `minor-audit` |

BLOCKER: no promotion MCP tool and no `gh` CLI are available to this executor, so no promotion was attempted. Per the plan's no-mid-plan-halt rule this blocker is recorded here and execution continued. The orchestrator performs the MCP promotions from this record after the executor returns.
