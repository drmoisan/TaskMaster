# Final QC — JaCoCo Coverage Conversion (P7-T5)

Timestamp: 2026-07-18T10-55

Command: python conversion script (inline): parse `final-coverage.cobertura.xml` (produced by `dotnet-coverage merge -f cobertura` from the P7-T4 vstest `.coverage` attachment), dedup lines per (sourcefile, line-number) within class and package, scope to first-party assemblies only {QuickFiler, UtilitiesCS, TaskMaster, TaskVisualization, ToDoModel, Tags, SVGControl} (test assemblies and third-party/instrumented libraries excluded), emit JaCoCo-format `counter` elements at class/package/report level to `artifacts/csharp/coverage.xml`.
EXIT_CODE: 0
Output Summary:
- `artifacts/csharp/coverage.xml` written in JaCoCo format (report/package/class `counter` elements), regenerated from the definitive final-pass `.coverage` attachment (`TestResults\a3ec0285-...\...09_56_29.coverage`).
- Report-level LINE counter totals: covered=42,221 missed=14,416 (total 56,637) => 74.55% first-party line coverage under the two-suite (QuickFiler.Test + UtilitiesCS.Test) measurement scope.
- Report-level BRANCH counter totals: covered=0 missed=0 — the dotnet-coverage Cobertura export carries no branch/condition-coverage attributes for these assemblies, so BRANCH counters are emitted as 0/0 (no branch data available from this collector), consistent with prior feature conversions.
- Scope note: the 74.51% figure under-represents the repository floor because first-party assemblies not exercised by these two suites (TaskMaster, TaskVisualization, ToDoModel, Tags — covered by their own suites in PR CI) report near-0% here; the per-assembly figures for the directly exercised assemblies are QuickFiler 72.67% and UtilitiesCS 88.69% (see `final-qc-test-coverage.2026-07-18T10-50.md`). Repo-wide aggregation is deferred to PR CI per established practice; no assemblies were cherry-picked out of the first-party scope.
