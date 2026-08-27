---
name: coverage-lines-covered-is-nondeterministic
description: In this repo lines-valid is deterministic across runs but lines-covered drifts by several lines per file, so any per-file covered-line gate fails on correct work
metadata:
  type: project
---

Measured directly by parsing two filtered Cobertura files in `coverage/`:

`coverage.cobertura.filtered.p9-t4.xml` and `coverage.cobertura.filtered.p0-t9r.xml` are measurements
of the **same tree** — all 550 files carry identical `lines-valid` and both roots read `63594`. Yet
per-file `lines-covered` differs between them:

| file | delta |
|---|---|
| `SubjectMapSco.Orchestration.cs` | **-4** |
| `PropertyStore.cs` | +4 |
| `EfcHomeController.cs` | +1 |

Root `lines-covered`: 53972 vs 53973. A second pair (`p0-t9r` to `p5-t4`, a change touching neither
file) shows `PropertyStore.cs` -4 and `EfcHomeController.cs` -1.

**Consequences for gate design:**

- `lines-valid` IS deterministic. A `lines-valid` mismatch is real signal — it means a file changed
  instrumented size — and can be gated hard.
- `lines-covered` is NOT deterministic at single-line or even 4-line granularity per file. A gate of
  the form "any retained file with a negative covered-line delta fails" **fails on correct work**,
  and it emits the identical signal for ambient drift and for a genuine one-line regression.
- The workable shape is: hard-fail on the deterministic quantity, and treat a negative covered-line
  delta as a CANDIDATE that must reproduce across two post-change measurements against the same
  baseline before it fails.

**Counting method also matters.** Cobertura repeats every line under both `./lines` and
`./methods/method/lines`. A plain descendant-axis count is roughly double: `EfcSelectionGuard.cs`
is 31/31 deduplicated but 62/62 naive; `EfcFormController.cs` 717/81 vs 1185/151. Deduplicate by
line number taking max `hits` — that reduction reproduces the root attributes exactly, and it is
what `Get-CoberturaClassLineSummary` in `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` does.

**How to apply:** Before writing any coverage no-regression gate, decide which quantity carries the
assertion. Anchor hard gates on `lines-valid`. Never gate a pass/fail decision on a single-file
`lines-covered` movement without a reproduction requirement. Verify the parse against the file's own
root attributes first — if your per-file sum does not equal the root, your reduction is wrong.
Related: [[csharp-coverage-denominator-two-figures]] on filtered vs unfiltered denominators.
