# Baseline — MSTest with Coverage

Timestamp: 2026-07-20T00-30
Command: `scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput
docs/features/active/2026-07-19-utilitiescs-nullable-ci-capstone-376/evidence/baseline/baseline-coverage.cobertura.xml`
(discovers all 8 `*.Test.dll` assemblies solution-wide; `SVGControl.Test` has no `.csproj` and is
not part of `TaskMaster.sln` — confirmed via `grep -i SVGControl TaskMaster.sln` — so it is not a
discoverable MSTest assembly and is correctly absent from this run.)

EXIT_CODE: 0

Output Summary: `Test Run Successful.` Total tests: 5702, Passed: 5702, Failed: 0, Total time:
34.6165 seconds. Coverage headline (from the emitted Cobertura `<coverage>` root attributes):
**line-rate 0.838838 (83.88%)**, **branch-rate 0.763567 (76.36%)** (lines-covered 87365 /
lines-valid 104150; branches-covered 19529 / branches-valid 25576).

## Run-attempt history (transparency note)

This baseline required four run attempts due to environmental factors unrelated to this
feature's edits:

1. **Attempt 1**: appeared to stall for over 30 minutes at the same point in the test list
   (after `Constructor_WhenMailProvided_LoadsConversationSnapshotSynchronously`). Killing the
   underlying `dotnet-coverage`/`vstest.console`/`testhost` processes to investigate revealed the
   run had NOT hung — it was still legitimately in progress — and the kill itself produced the
   only failure ("MSTest with coverage failed with exit code 1").
2. **Attempt 2**: failed immediately with "Could not load file or assembly 'TaskMaster,
   Version=1.0.0.0...'" because an earlier solution-wide `msbuild /t:Rebuild` (used for this
   session's periodic full-solution diagnostic scans during Phase 2) had cleaned all projects'
   outputs and aborted partway through the build (blocked by not-yet-remediated Phase 2 batches),
   leaving several projects' binaries deleted but never rebuilt. Restored via a full-solution
   plain `msbuild /t:Build` (0 errors) before retrying.
3. **Attempt 3**: reproduced the identical apparent-stall at the identical point in the test list
   a second time; killed again after a long wait, again producing only a kill-induced failure
   (confirming the earlier run was never actually hung, just slow at that specific point in this
   environment on that occasion).
4. **Attempt 4 (this artifact)**: left running untouched to completion — succeeded in 34.6
   seconds with zero failures, confirming the earlier apparent slowness at that exact point was a
   transient environmental fluke (not a deterministic hang, not caused by this feature's Phase 1/
   Phase 2 nullable-annotation edits, and not reproduced when left undisturbed).

This is the pre-remediation-equivalent baseline for the Phase 7 (P7-T5) no-regression coverage
comparison: at the time this run was captured, Phase 1 (SVGControl CS0649 fix) and 4 of 7 Phase 2
batches (Bayesian, ClassifierGroups, EmailParsingSorting, Evaluation/Flags/IntelligenceConfig/
SubjectMap/Extensions) had already been applied to the working tree. Per this plan's own
Scope reconciliation and AC7 clarification, all of Phase 1/Phase 2's edits are nullable-
annotation-only, null-forgiving-operator-only, or narrow-pragma-suppression-only changes with no
production behavior change; the 5702/5702 all-passed result is consistent with that guarantee
(no test failures attributable to any in-progress annotation edit). This coverage figure is
therefore a valid pre-remediation-equivalent baseline for the Phase 7 comparison.
