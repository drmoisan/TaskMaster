# Phase 3 Final QC — Step 4 Tests + Coverage (#177 Cycle 1)

- Timestamp: 2026-06-12T17-16 (UTC)
- Task: [P3-T1] step 4 of 4
- Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation`
- EXIT_CODE: 0
- Output Summary:
  - Full unfiltered assembly final run: Test Run Successful, Total tests 3904, Passed 3904, Failed 0
    (the pre-existing `IdleAsyncQueue` flake did not reproduce on this run, confirming its
    non-determinism). A deterministic full run excluding only that flaky test was also green (3903/3903).
  - Post-change strict per-type line coverage (authoritative full-suite capture, source byte-identical
    to Phase 2): `FolderHierarchyTree` 100.00% strict, `LcppnFolderPredictor` 97.71% strict — both
    exceed the >= 90% gate. Production assembly `UtilitiesCS.dll` strict 85.45% (no regression vs the
    85.31%/85.40% baseline; above the 80% floor).
  - Coverage XML: `evidence/qa-gates/2026-06-12T15-54/p2-coverage.xml`, mirrored to the canonical
    `artifacts/csharp/coverage.xml`.

Note on coverage attachment: under VS18 `/InIsolation`, the dynamic code-coverage attachment is
intermittently not flushed into the named `/ResultsDirectory` GUID subfolder. The authoritative
post-change coverage XML is the most recent full-suite capture (cov-p2), valid for Phase 3 because no
source file changed between Phase 2 and the final QC re-run; the final QC re-run only re-executed the
toolchain and confirmed the test result (3904/3904).
