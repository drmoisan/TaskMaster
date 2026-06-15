# Production-Only Coverage Baseline (post-#197)

Timestamp: 2026-06-14T08-22

Command: read/derive per-package line-rate values from artifacts/csharp/coverage-firstparty.cobertura.xml (authoritative roadmap baseline input, read-only)

EXIT_CODE: 0

Source artifact: artifacts/csharp/coverage-firstparty.cobertura.xml
(top-level coverage line-rate=0.768112, lines-covered=167013, lines-valid=217433; this includes
vendored and test packages and is NOT the production-only figure)

## Output Summary

Production-only baseline: 71.65% (authority-scoped exception 197-COV-001, post-#197 testable
denominator). This is the authoritative comparison baseline for net coverage increase in this
feature.

Per-assembly production-only line-rate (target assemblies, from the source cobertura packages):
- ToDoModel:  line-rate 0.108244 (10.82%)
- QuickFiler: line-rate 0.251987 (25.20%)
- TaskMaster: line-rate 0.257757 (25.78%)

These three production assemblies hold the seams targeted by Increments 1-3. The covered-line
counts on the named seams in each assembly are expected to increase after the test additions; the
net production-only rate is re-measured in Phase 4 (P4-T4/P4-T5) and compared to 71.65%.

Note: The production-only denominator method (per the #197 roadmap and recorded analysis) is the
per-`<line>` count across all deduped Cobertura production packages; the 71.65% figure is the
authoritative recorded value and is used as-is as the baseline. Raw cobertura XML is NOT copied
into this evidence folder per evidence-hygiene rules; only numeric headlines are recorded here.
