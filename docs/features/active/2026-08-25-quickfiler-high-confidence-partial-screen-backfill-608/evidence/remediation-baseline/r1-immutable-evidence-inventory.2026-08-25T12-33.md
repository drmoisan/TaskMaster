# Issue #608 immutable-evidence inventory

Timestamp: 2026-08-25T12-50
Command: Get-FileHash -Algorithm SHA256 and Get-Content -Raw for each inventoried path.
EXIT_CODE: 0
Output Summary: All three inventoried records exist and were read without modification. The original plan remains immutable; the global-nullable receipt remains failed and is not a passing QA record.

| Immutable record | SHA-256 | Inventory result |
| --- | --- | --- |
| `plan.2026-08-25T11-53.md` | `6C0BBCE13D903E25E9A329459A0DEF4A3777994EB38EB5A16E1C4B5AC0EFF2FD` | Original Issue #608 plan; no write performed. |
| `evidence/other/execution-sequence-deviation.2026-08-25T12-31.md` | `1C34634AC75B50971BC36E143EE718CFA2483BA9BACE08450E502A90D87FF40A` | Chronology-deviation record; no write performed. |
| `evidence/qa-gates/csharp-nullable.2026-08-25T12-33.md` | `EBB9893A2DF462E9583BC6B0B0E6689131A6E08AA840B824796062B0DB95D0A1` | Failed global `/p:Nullable=enable` receipt with 195 legacy diagnostics; no write performed. |

Write Verification: `git diff --name-only` identifies only the established Issue #608 production and regression-test files before this remediation evidence; no inventoried path is modified.
