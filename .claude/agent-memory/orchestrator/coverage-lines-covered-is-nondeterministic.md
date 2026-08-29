---
name: coverage-lines-covered-is-nondeterministic
description: lines-valid is deterministic but lines-covered drifts - measured up to 18 lines at ROOT (0.03 pct points), so a repo-wide >= coverage comparison at two-decimal resolution is undecidable
metadata:
  type: project
---

**ROOT-LEVEL DRIFT IS ~0.03 PERCENTAGE POINTS, not one line (measured 2026-08-29, issue #644).**
An earlier revision of this note recorded root drift of a single line (53972 vs 53973) and that
figure understated it by more than an order of magnitude. Two full-suite runs of a **byte-identical
tree**, with no source change of any kind between them, produced:

| Run | lines-covered | lines-valid | Percent |
|---|---|---|---|
| E | 54793 | 64221 | 85.3194% |
| F | 54811 | 64221 | 85.3475% |

An 18-line spread at root. Critically, the two runs **straddled the 85.3303% baseline** — one below,
one above. A `>=` gate would have returned opposite verdicts on the same tree depending only on which
run it happened to observe.

**Consequence for plan authoring: never gate a repo-wide `post >= baseline` coverage comparison at
two-decimal resolution.** The clause is not decidable at the resolution it demands; it reports
regression on correct work at roughly a 0.03-point granularity. Either state a tolerance band wider
than the noise floor, or gate on `lines-valid` (deterministic) plus a changed-line coverage figure.

Diagnosing one of these: check whether the changed production file carries
`[ExcludeFromCodeCoverage]` and enumerate the `<class>` entries to confirm it is absent from the
document. If it is absent it is in neither numerator nor denominator and cannot move the figure
mechanically, which converts the question from "did I regress" into "is this noise".

**Measuring the noise floor is legitimate; shopping for a passing run is not.** Re-run once on an
unchanged tree with the declared purpose of estimating noise, and record BOTH runs. Selecting the
favorable run out of a set makes the acceptance unfalsifiable, which is exactly the defect the
plan-acceptance-gate rules exist to prevent.

Original per-file measurement, still valid, from parsing two filtered Cobertura files in `coverage/`:

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
