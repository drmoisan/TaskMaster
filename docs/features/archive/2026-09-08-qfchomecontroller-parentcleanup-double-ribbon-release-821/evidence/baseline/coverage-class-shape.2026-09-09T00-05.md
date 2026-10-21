# Phase 0 — Baseline Cobertura class shape for the four production files

Timestamp: 2026-09-09T12-46
Task: [P0-T12]

This task exists so that the Phase 6 change-scoped coverage gate asserts over output that has
actually been observed on a successful run, rather than over an assumed schema.

Command:

```text
pwsh -NoProfile -Command '[xml]$c = Get-Content -LiteralPath "coverage/coverage.cobertura.xml" -Raw; foreach ($n in @("QfcHomeController.cs","EfcHomeController.cs","ProgressViewer.cs","ProgressPane.cs")) { foreach ($cl in $c.SelectNodes("//class")) { if ($cl.GetAttribute("filename").EndsWith($n)) { $zero = @($cl.SelectNodes("lines/line") | Where-Object { $_.GetAttribute("hits") -eq "0" } | ForEach-Object { $_.GetAttribute("number") }); "{0} :: {1} :: line-rate={2} :: zero-hit-lines={3}" -f $n, $cl.GetAttribute("name"), $cl.GetAttribute("line-rate"), ($zero -join ",") } } }'
```

EXIT_CODE: 0

Complete verbatim output:

```text
QfcHomeController.cs :: QuickFiler.Controllers.QfcHomeController :: line-rate=0.777344 :: zero-hit-lines=27,40,44,45,46,47,50,51,54,55,56,59,60,61,62,63,64,65,66,67,69,70,71,72,73,74,75,76,77,78,80,81,83,84,170,179,217,218,219,220,221,222,223,224,225,226,241,346,350,352,354,355,356,382,383,384,385
EfcHomeController.cs :: QuickFiler.EfcHomeController :: line-rate=0.982456 :: zero-hit-lines=52,288,289,418
ProgressViewer.cs :: UtilitiesCS.ProgressViewer :: line-rate=1 :: zero-hit-lines=
ProgressPane.cs :: UtilitiesCS.EmailIntelligence.TaskPane.ProgressPane :: line-rate=0.8947368421052632 :: zero-hit-lines=28,29
```

## Observed document shape

The query emitted **four rows, one per production file name**, each carrying a `line-rate=` value
and a `zero-hit-lines=` list. The Phase 6 change-scoped coverage gate is therefore satisfiable and no
plan revision or gate weakening is required.

The shape observed is **per-class**, not per-method: each `class` element carries a `line-rate`
attribute and a flat `lines/line` child set. No `<method>` element carrying its own `line-rate` was
relied on. This is the shape `[P6-T10]` names when it directs that condition (b) be discharged by
enumerating `zero-hit-lines` entries inside each member range rather than by reading a per-method
`line-rate`.

The `EndsWith` test is deliberate and was verified to behave as intended: `QfcHomeController.cs`
matched exactly one row and did not pull in the partial-class sibling `QfcHomeController.Metrics.cs`,
and `ProgressViewer.cs` matched exactly one row and did not pull in `ProgressViewer.Designer.cs`,
because neither sibling file name ends with the queried name.

## Baseline per-file figures, for the Phase 6 comparison

| File | Class | Baseline line-rate | Baseline zero-hit line count |
|---|---|---|---|
| `QfcHomeController.cs` | `QuickFiler.Controllers.QfcHomeController` | 0.777344 | 56 |
| `EfcHomeController.cs` | `QuickFiler.EfcHomeController` | 0.982456 | 4 |
| `ProgressViewer.cs` | `UtilitiesCS.ProgressViewer` | 1 | 0 |
| `ProgressPane.cs` | `UtilitiesCS.EmailIntelligence.TaskPane.ProgressPane` | 0.8947368421052632 | 2 |

## Two observations that bear directly on the Phase 6 gate

1. **The pre-existing zero-hit region inside `QfcHomeController.Cleanup` is confirmed.** Lines
   **382, 383, 384, 385** report zero hits at baseline. Those are the first `catch (System.Exception
   e)` block inside `Cleanup`, whose `logger.Error(...)` at line 384 is reached only if
   `_formViewer?.Worker` throws, which no test drives. This is the specific pre-existing zero-hit set
   that makes a whole-member no-zero-hit formulation unsatisfiable, and it is why `[P6-T10]`
   condition (b) is discharged by showing each in-range zero-hit line was already zero-hit here.

2. **Every line this fix will change is covered at baseline.** `QfcHomeController.cs` line 405 is
   absent from its zero-hit list; `EfcHomeController.cs` line 349 is absent from its zero-hit list;
   `ProgressViewer.cs` has an empty zero-hit list, so lines 64-68 and 70-77 are all covered; and
   `ProgressPane.cs`'s only zero-hit lines are 28 and 29, so lines 46-50 and 52-59 are all covered.
   The change-scoped condition (a) in `[P6-T10]` — every line changed by this fix reports non-zero
   hits — therefore starts from a fully covered baseline at all four sites, and any zero-hit changed
   line after the change would be a genuine regression rather than an inherited gap.

Output Summary: four rows emitted, one per production file, each with a `line-rate=` value and a
`zero-hit-lines=` list. The Cobertura shape is per-class with a flat line list and no per-method
`line-rate`. All four edit regions are covered at baseline; the only zero-hit lines inside any
member this plan touches are `QfcHomeController.cs` 382-385, which lie in a pre-existing catch block
this fix does not modify.
