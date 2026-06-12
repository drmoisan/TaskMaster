# NO-/collect Verification (P2-T2, AC5/AC3) — FINDING: ACCEPTANCE NOT MET

Timestamp: 2026-06-12T19-45

Command:
```
"...\vstest.console.exe" "...\UtilitiesCS.Test.dll" /Tests:Deedle \
  /Settings:"...\TaskMaster.runsettings" /InIsolation
```
(Run against the EDITED `TaskMaster.runsettings`, WITHOUT `/collect`.)

EXIT_CODE: 0

Output Summary:
- `Test Run Successful. Total tests: 42, Passed: 42`.
- ACCEPTANCE FAILURE: a code-coverage attachment WAS produced on this normal (no-`/collect`) run:
  `...\TestResults\bdc4e269-...\DanMoisan_MEGALODON4_2026-06-12.19_31_33.coverage`.
- AC5 requires "produces no code-coverage attachment". This was NOT satisfied by the current edit. AC3 (opt-in, no coverage forced on a normal run) is therefore also NOT satisfied.

## Diagnosis (controlled comparison)

1. NO-`/collect` against the BASELINE (no `<DataCollectionRunSettings>`) runsettings: NO attachment produced (confirmed via an out-of-repo temp copy of the pre-edit file). This isolates the added block as the cause.
2. NO-`/collect` against the EDITED runsettings (no `enabled` attribute on `<DataCollector>`): attachment IS produced. => A declared `<DataCollector friendlyName="Code Coverage">` activates by default in this VSTest 18.7.0 environment even without `/collect`. Absence of `enabled` defaults to enabled, contradicting the plan's assumption that omitting `enabled="true"` keeps it opt-in.
3. Adding `enabled="false"` to the `<DataCollector>`:
   - NO-`/collect` run: NO attachment (AC3/AC5 would pass), BUT
   - WITH-`/collect:"Code Coverage"` run: the collector throws during `DynamicCoverageDataCollector.Initialize` / `OnFirstCollectorToInitialize` and the run exits 1. The `/collect`-supplied collector conflicts with the `enabled="false"` declared collector. This breaks AC4.

## Conclusion

The single mechanical edit prescribed by the plan (add the collector block, omit `enabled="true"`) cannot simultaneously satisfy AC4 (WITH-`/collect` passes) and AC3/AC5 (no coverage on a normal run) in this CLI environment:
- no `enabled` attribute  -> AC4 OK, AC3/AC5 FAIL (forces coverage on normal run)
- `enabled="false"`       -> AC3/AC5 OK, AC4 FAIL (Initialize exception under `/collect`)

This is a scope-change finding. Execution of Phase 2 implementation is halted pending plan revision per the executor directive ("the collector forces coverage on a normal run" -> STOP and report).
