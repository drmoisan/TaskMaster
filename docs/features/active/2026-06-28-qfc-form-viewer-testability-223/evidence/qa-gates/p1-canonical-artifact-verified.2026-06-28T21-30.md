# P1-T6 — Canonical Coverage Artifact Verified (Remediation Cycle 1, Issue #223)

Timestamp: 2026-06-28T21-50
Command: ls -la artifacts/csharp/coverage.xml; [xml]parse + read coverage root and package names
EXIT_CODE: 0

Output Summary (Finding 1 artifact-existence sub-claim RESOLVED):
- `artifacts/csharp/coverage.xml` EXISTS (size ~8.97 MB).
- Parses as well-formed Cobertura. Repo-wide root attributes: `line-rate=0.741108`, `lines-covered=71654`, `lines-valid=96685`.
- Contains nine first-party packages with third-party stripped: `QuickFiler`, `UtilitiesCS`, `TaskMaster`, `Swordfish.NET.General`, `SVGControl`, `Tags`, `ToDoModel`, `TaskVisualization`, `VBFunctions`. The two vendored packages (`Swordfish.NET.General`, `SVGControl`) are retained per the #197 first-party-denominator convention. `.Test` packages were stripped by the Koverage pipeline (Issue #193).
- The canonical artifact existence half of Finding 1 (FAIL: artifact absent) is now resolved. The repo-wide measurement and floor decision are completed in Phase 2.
