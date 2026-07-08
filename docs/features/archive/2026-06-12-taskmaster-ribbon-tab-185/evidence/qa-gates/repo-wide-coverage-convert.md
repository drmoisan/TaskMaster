# Phase 1 — Cobertura Conversion of Repo-Wide Coverage (Issue #185)

Timestamp: 2026-06-12T11-21

Command:
```
dotnet-coverage merge -f cobertura -o artifacts/csharp/coverage.xml \
  coverage-out/b14cd307-66bd-448e-9977-df5cf2dc5ca6/DanMoisan_MEGALODON4_2026-06-12.11_20_53.coverage
```
(dotnet-coverage v18.5.2.0)

EXIT_CODE: 0

Output Summary: Cobertura conversion succeeded. Canonical tool output `artifacts/csharp/coverage.xml` produced (~31 MB).
- Root element confirmed Cobertura: `<coverage line-rate="0.5893769565947007" branch-rate="1" complexity="9044" version="1.9" timestamp="1781277685" lines-covered="101852" lines-valid="172813">`.
- Per-line entries confirmed present, e.g. `<line number="43" hits="1" branch="False" />`.
- Resolved root `line-rate` = 0.5894 (58.94%); lines-covered 101852 / lines-valid 172813.
- Cobertura validity (root `<coverage line-rate=...>` + per-line `<line number= hits=>`) satisfied.
