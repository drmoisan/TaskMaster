# Issue #262 AC Reconciliation Mirror (P5-T4)

Timestamp: 2026-07-08T00-10
PostedAs: unknown (local mirror only; no GitHub post performed by this execution — spec.md is the
authoritative full-bug AC source and was updated locally per P5-T4)

## spec.md `## Acceptance Criteria` section (checked, with evidence annotations)

- [x] AC1: When the persisted `StoresWrapper` config is missing, `LoadStoresAsync` builds a fresh
      model from the live Outlook stores (via `BuildFreshStoresWrapper()` -> `new StoresWrapper(_globals).Init()`)
      instead of leaving `StoresWrapper` null. (Evidence: P3-T1 fix; P2-T1 regression; P2-T4 fail-before; P3-T3 pass-after.)
- [x] AC2: When the persisted config deserializes to null, the same fresh-build fallback applies
      rather than being silently tolerated; `AwaitStoreRewireAsync` not invoked on the fresh-build path.
      (Evidence: P3-T1 fix; P2-T2 regression; P2-T4 fail-before; P3-T3 pass-after.)
- [x] AC3: A genuine, unrecoverable load failure is surfaced — logged at `Error` with the exception
      attached and `StoresWrapper`-specific context; no retry, no new dialog. (Evidence: P3-T1 try/catch;
      P2-T3 regression; P2-T4 fail-before; P3-T3 pass-after.)
- [x] AC4: `StoreWrapperController.Launch()` opens with a populated model on recoverable paths;
      controller unmodified. (Evidence: P5-T3 ac4-controller-unchanged.md; P3-T3.)
- [x] AC5: Deterministic MSTest suite (fail-before/pass-after), inverted mis-specified test, Moq, no
      live Outlook/temp files. (Evidence: P2-T4 fail-before-262.md; P3-T3 pass-after-262.md.)
- [x] AC6: `AppOlObjects.cs` <= 500 via new partial `AppOlObjects.StoreLoading.cs`; both files <= 500
      (495 and 75). (Evidence: P1-T3 file-size-after-extraction.md; P5-T1 file-size-final.md.)
- [x] AC7: Full C# toolchain in order; new-code >= 90% (100%); no regression; net48. (Evidence:
      P4-T1..P4-T5 qa-gates/qa-01..05.)

## issue.md `## Acceptance Criteria` reconciliation (issue AC1-AC6 numbering)
- issue AC1-AC5 -> spec AC1-AC5 (checked).
- issue AC6 (toolchain) -> spec AC7 (checked).
- spec AC6 (file-size <= 500) is spec-only; no issue.md counterpart; tracked in spec.md.
All issue.md AC1-AC6 checked in the local issue.md.
