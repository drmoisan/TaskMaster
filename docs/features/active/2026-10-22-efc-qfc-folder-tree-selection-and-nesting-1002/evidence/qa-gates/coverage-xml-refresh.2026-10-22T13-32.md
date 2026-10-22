# Canonical coverage artifact refresh (P4-T6, #1002 cycle 1)

Timestamp: 2026-10-22T13-32
Command: copy of coverage/coverage.cobertura.xml to artifacts/csharp/coverage.xml (same derivation as the earlier executor final artifact: byte-identical copy of the post-processed Cobertura); cmp reports identical
EXIT_CODE: 0
Output Summary:
- Source: coverage/coverage.cobertura.xml (post-processed Cobertura of the P4-T5 run). Destination: artifacts/csharp/coverage.xml (gitignored, not staged).
- Destination modification time is later than the P4-T5 trx and Cobertura write times.
- First-party totals: lines 57541/66677 (86.30%), branches 14036/17419 (80.58%).
