# AC16 Check-off Summary (issue #211, Phase 3.4)

Timestamp: 2026-06-24T14-30

## AC16 — Per-store filter attribution probe (diagnosis-only)

Status: PASS (automated portion). Runtime portion (maintainer cold-start capture) is maintainer-gated.

### Mapping to satisfying tasks

| AC16 sub-requirement | Satisfying task(s) | Evidence |
| --- | --- | --- |
| Matched-rule enum + pure include/exclude decision (mirrors ShouldIncludeStore short-circuit order) | P1-T1, P1-T2 | StoreFilterAttribution.cs (StoreFilterRule enum, Decide); StoreFilterAttributionTests.cs Decide_* tests |
| Pure single-line [store-filter] formatter (5 fields, F1 InvariantCulture, <null> guard) | P1-T3 | StoreFilterAttribution.FormatLine; FormatLine_* tests |
| Stopwatch-wrapped per-store COM reads (ExchangeStoreType, FilePath) + one [store-filter] line per store | P2-T1, P2-T2 | StoresWrapper.ShouldIncludeStoreInstrumented; GetFilteredStores routes through it |
| Synchronous-Init GetFilteredStores summary line ([store-filter] GetFilteredStores completed) | P2-T3 | StoresWrapper.Init() materializes + emits summary; async RewireOlObjectsAsync left unchanged |
| Existing instrumentation + PreserveReferencesHandling.All untouched | P2-T4 | diff review; [Startup timing]/[ui-heartbeat]/[gc-delta]/[continuation-resume]/[engine-init]/[startup-lifetime-heartbeat] unchanged; StoreWrapper.cs not touched |
| Virtual-seam override handling | P2-T5 | P0-T8 finding: no new virtual seam (private non-virtual helper); no StoresWrapperTests override change needed |
| Deterministic MSTest (MSTest + FluentAssertions; no live COM/timer/fs/network; no temp files) | P3-T2, P3-T3, P3-T4 | 13 tests pass (P3-T7); final-qc-tests-coverage |
| csproj Compile-Include wiring (both projects, legacy packages.config, explicit includes) | P1-T1, P3-T1 | UtilitiesCS.csproj + UtilitiesCS.Test.csproj Compile Include items |
| Coverage (new code >= 90%; no repo regression), full toolchain in order, files <= 500 lines | P5-T1..P5-T6 | final-qc-csharpier / -analyzers / -nullable / -tests-coverage / -coverage-delta / -filesize |
| Behavior-preserving (identical filter result, included set, order, predicate short-circuit semantics) | P1-T2 (Decide mirrors predicate), P2-T1/T2/T3 | Decide reproduces ShouldIncludeStore branch order exactly; included set/order unchanged; existing StoresWrapper Init/RewireOlObjectsAsync inclusion tests still pass |
| Maintainer cold-start capture of [store-filter] lines during a slow startup | P4-T1, P4-T2 | coldstart-store-filter-capture-instructions + runtime-capture-store-filter-PLACEHOLDER (maintainer-gated, runtime) |

### Toolchain result (single clean pass)

- CSharpier: EXIT 0 (1095 files, formatter-clean).
- Analyzer build: EXIT 0 (no errors; no new warnings).
- Nullable/TWAE build (incremental, canonical gate): EXIT 0 (no first-party errors).
- MSTest + coverage: EXIT 0 (3929/3929 passed).

### Coverage

- New code StoreFilterAttribution = 100% (>= 90% floor).
- StoresWrapper module = 98.71%; the 4 uncovered lines are the two empty `catch { }` blocks guarding live COM reads (COM/VSTO exemption per CLAUDE.md).
- Repo-wide raw overall 59.35% vs baseline 59.28%: no regression.

### Runtime portion (maintainer-gated)

P4-T1/P4-T2 deliver the capture instructions and a PENDING placeholder. The actual non-debugger cold-start
DebugView capture is a maintainer runtime task; it is NOT executed by the automated toolchain and remains pending.
AC16's runtime clause is therefore noted as maintainer-gated, consistent with AC5/AC9 handling for this issue.
