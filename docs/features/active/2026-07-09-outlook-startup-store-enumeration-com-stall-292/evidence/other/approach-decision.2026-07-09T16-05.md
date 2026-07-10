# Approach Decision (issue #292, remediation cycle 1)

- Timestamp: 2026-07-09T16-05
- Task: [P1-T4]

## Options evaluated

- **(A) Move every scope-opening `UtilitiesCS.Test` class into the serialized non-parallel bucket** by adding `[DoNotParallelize]` to each writer class. MSTest runs all `[DoNotParallelize]` classes sequentially and never concurrently with each other. The null-baseline readers (`CurrentStoreContextTests`, `ThreadMonitorTests`) are already in that bucket. Once every writer joins it, no writer remains in the parallel bucket that could overlap a reader. This is a structural mutual-exclusion guarantee, not a probability reduction, and it is not a timing hack.
- **(B) Remove/disable `[assembly: Parallelize]` for `UtilitiesCS.Test`.** Provably correct and single-file, but serializes the entire (largest) test assembly — a broad wall-clock regression across thousands of unrelated tests.
- **(C) Shared explicit lock acquired by every reader and writer test body.** Provably correct but requires editing every writer test body (larger surface than A) and adds a hand-maintained synchronization primitive.

## Selected: (A)

(A) provably removes the reader/writer overlap while remaining surgical: only the small set of `CurrentStoreContext`-touching classes is serialized, preserving parallel throughput for the majority of the assembly. Its only correctness dependency is completeness of the writer-class enumeration, which is closed by the Phase 1 census (`scope-open-census.2026-07-09T16-05.md`) and the Phase 2 completeness-verification gate (P2-T12) that fails if any scope-opening class remains unmarked.

Constraint compliance: production `CurrentStoreContext` stays a process-global static (unchanged), reader assertions are untouched, the enumeration-phase attribution scope is untouched, and no sleeps/retries/timing hacks are introduced.

## Residual durability risk (follow-up, out of scope)

A future `UtilitiesCS.Test` class that opens a `CurrentStoreContext` scope without `[DoNotParallelize]` would reintroduce the overlap. This is recorded as follow-up (e.g., an assembly-fixture assertion or a shared serial `[TestCategory]`/collection convention). It is out of scope for this remediation, which is a minimal, provably-correct test-isolation fix.
