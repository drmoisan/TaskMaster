# Coverage Artifact (Issue #1007, remediation cycle 1, P5-T6)

Timestamp: 2026-10-26T00-04
Command: Copy-Item coverage/coverage.cobertura.jacoco.xml artifacts/csharp/coverage.xml; Get-FileHash -Algorithm SHA256 on both; count `counter` elements by `type`; git check-ignore -v artifacts/csharp/coverage.xml; git ls-files -- artifacts/csharp/coverage.xml
EXIT_CODE: 0
Output Summary: The copy is byte-identical to the P5-T4 JaCoCo package projection (equal SHA-256). It carries 9 `LINE` and 9 `BRANCH` counters. It is ignored by `.gitignore` (`artifacts/`) and untracked (`git ls-files` returns nothing).

| Check | Result |
|---|---|
| Source SHA-256 (`coverage/coverage.cobertura.jacoco.xml`) | B1BD0F3247A5139C0C4B57919E5AFC5CD2F324073DD2DC52A2220853265178E4 |
| Copy SHA-256 (`artifacts/csharp/coverage.xml`) | B1BD0F3247A5139C0C4B57919E5AFC5CD2F324073DD2DC52A2220853265178E4 |
| Hashes equal | yes |
| `counter` elements, type `LINE` | 9 |
| `counter` elements, type `BRANCH` | 9 |
| `git check-ignore -v` | `.gitignore` rule `artifacts/` matches (exit 0) |
| `git ls-files` | empty (untracked) |
