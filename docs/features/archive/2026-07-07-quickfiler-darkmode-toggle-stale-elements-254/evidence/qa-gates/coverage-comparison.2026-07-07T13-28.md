# Coverage Comparison — Baseline vs Post-Change (Issue #254)

Timestamp: 2026-07-07T13-28

Sources:
- Baseline: `evidence/baseline/baseline-tests-coverage.2026-07-07T13-10.md`
- Post-change: `evidence/qa-gates/qc-tests-coverage.2026-07-07T13-28.md`
- Both measured via `dotnet-coverage collect` -> Cobertura over the same two test assemblies (`UtilitiesCS.Test`, `QuickFiler.Test`).

## Delta Table

| Scope | Baseline | Post-change | Delta |
|---|---|---|---|
| Overall line-rate | 64.28% (110106/171299) | 64.28% (110182/171399) | +0.00 pp (no regression) |
| UtilitiesCS module | 87.89% (70284/79967) | 87.93% (70324/79981) | +0.04 pp |
| Theme (`Theme.cs`) | 62.71% (296/472) | 66.10% (312/472) | +3.39 pp |
| Theme (`Theme.Rendering.cs`, changed) | 44.78% (60/134) | 54.05% (80/148) | +9.27 pp |

## New / Changed-Code Coverage

Changed region: `Theme.Rendering.cs` lines 42-59 — the new `bool isRead; try { isRead = MailRead(); } catch (COMException) { isRead = false; }` block plus the following `if (!isRead) SetMailUnread(); else SetMailRead();`.

Per-line hits from the post-change Cobertura (executable lines 44-59): every line has hits >= 1.

- Changed executable lines covered: 14 / 14 = **100%**.
- Branches covered: try-success->read, try-success->unread, and catch->default (unread) — all three exercised by the three new tests.

## Threshold Verdict

- New/changed-code coverage: **100% >= 90%** floor — PASS.
- No regression on changed lines: the changed lines went from uncovered (the private renderer was not driven end-to-end in baseline) to fully covered; no previously-covered line lost coverage — PASS.
- Overall and UtilitiesCS coverage did not regress (both flat-to-up) — PASS.

Outcome: PASS.
