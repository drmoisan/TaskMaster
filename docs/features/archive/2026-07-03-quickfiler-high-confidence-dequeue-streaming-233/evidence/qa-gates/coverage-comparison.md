Timestamp: 2026-07-03T17:50:04.4596297-04:00
Command: Compare `evidence/baseline/coverage-baseline.md` with `evidence/qa-gates/vstest-final.md` and final VSTest coverage output.
EXIT_CODE: 1
Output Summary:
- Baseline coverage: unavailable. `evidence/baseline/coverage-baseline.md` records that no VSTest result or coverage attachment was found because the Phase 0 `vstest.console.exe` command was not available on PATH.
- Final coverage: binary coverage attachment produced, but numeric coverage values unavailable.
- Final coverage attachment:
  - `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/vstest-results/d6d6f998-bf78-4a04-85d2-859e5219e314/DanMoisan_MEGALODON4_2026-07-03.17_49_06.coverage`
- Final VSTest result: 382 total tests, 382 passed.
- Changed/new-code coverage: unavailable because the coverage conversion attempt failed.
- Repository coverage floor status: unavailable because neither baseline nor final numeric coverage was produced.
- PASS criteria not met. The plan requires PASS only when changed/new non-COM-bound code is at or above 90% and repository coverage does not regress below the applicable baseline/floor. Those numeric checks could not be performed from the available artifacts.
- Result: FAIL / remediation required for numeric coverage conversion and comparison.
