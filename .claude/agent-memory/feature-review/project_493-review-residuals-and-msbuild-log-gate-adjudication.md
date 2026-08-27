---
name: 493-review-residuals-and-msbuild-log-gate-adjudication
description: '#493 review PASS/0 blocking; how the P4-T2 msbuild-log byte-equality gate failure was adjudicated (AC-6 still PASS); residuals CR-1 restore try/finally, CR-2 R2/R3 Ensure-race flake window, #648 open; UTC timestamps'
metadata:
  type: project
---

#493 (quickfiler-test-uithread-dispatcher, epic child of quickfiler-bug-family) reviewed 2026-08-27T15-07: PASS, 0 Blocking, all 10 spec ACs PASS.

**Why:** the one genuine discrepancy was plan task P4-T2 — byte-exact set equality of msbuild-log lines containing `QfcItemController.FocusAndThemeTests.cs` failed because at default verbosity every matching line is a csc.exe invocation enumerating the project's whole source set, so the gate is structurally unsatisfiable whenever the plan itself adds compile items. Adjudication technique that settled it: delete exactly the added compile-input tokens from the final extract and `cmp` against the baseline extract (strip `\r` — a CRLF artifact broke the first compare); both analyzer and nullable extracts became byte-identical. AC judged on the spec criterion's own clauses (byte-identity via `git hash-object` vs base blob, zero diagnostic-bearing log lines, named tests passing), NOT on the plan's proxy; the checked-off-despite-literal-failure task recorded as a Non-blocking finding instead of downgrading the AC.

**How to apply:**
- Future plans must not gate on raw compiler-invocation text; if a reviewer sees such a gate fail, reproduce the token-removal normalization before accepting or rejecting the executor's story.
- Residuals to re-check at epic fan-in / later QuickFiler reviews: CR-1 — `PumpHarness.Restore` (Part2.cs) and `UiThreadDispatcherTransaction.Dispose` lack try/finally around restore-before-release (theoretical gate leak); CR-2 — R2/R3 assert absolute field values and can theoretically flake against a concurrent unowned `EnsureDispatcher` caller (design keeps Ensure off TransactionGate deliberately) — diagnose a rare R2/R3 flake as this, not a fixture defect; issue #648 (WpfUiDispatcherTests ungated swap, restores in finally) OPEN, out of scope by design.
- TaskMaster executor evidence timestamps are UTC while the local clock is -04:00; use `date -u` for review artifact timestamps so they sort after the evidence they cite.
- Coverage handling that passed the hook: no `artifacts/csharp/coverage.xml` emitted (deliberate, per delegation); C# coverage row written as FAIL (artifact-absence rule) dispositioned Non-blocking with the byte-identical Cobertura triple (0.19049434489769984 / 78690) as zero-delta proof — hook accepts because absent XML yields $null repoPct.
