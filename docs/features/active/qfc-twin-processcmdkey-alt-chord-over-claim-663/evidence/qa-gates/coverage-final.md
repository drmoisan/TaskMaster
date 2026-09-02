# Phase 4 — Coverage post-processing and figures ([P4-T7])

Timestamp: 2026-09-01T23-21

Command:

```
pwsh -NoProfile -Command '. ./scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1; $f = "docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/coverage.cobertura.xml"; $raw = Get-Content -LiteralPath $f -Raw -Encoding UTF8; $p = ConvertTo-KoverageCoberturaXml -XmlContent $raw -RepoRoot (Get-Location).Path; Set-Content -LiteralPath $f -Value $p -Encoding UTF8 -NoNewline'
```

EXIT_CODE: 0

`[P0-T13]` recorded BASELINE_CLASS_LINE_RATE from the **post-processed** document, so every class-scoped
and method-scoped reading below is taken from the post-processed document as well, and the two figures are
commensurable. The `[P4-T7]` raw-document branch does not apply.

The document byte size is 10,792,221 both before and after the out-of-band transform, which corroborates
its idempotence: the wrapper had already post-processed in place, and re-applying the transform is safe
because the `<sources>` injection is guarded.

## AC-11 evidence of record:

`coverage.cobertura.xml` was transcribed into this file and then deleted. The two verbatim XML fragments
recorded below — the `<method>` element whose `name` is `ClaimsAltChord` under the `<class>` element whose
`filename` ends with `QfcFormKeyHandler.cs`, and that `<class>` element itself — are the evidence AC-11's
verification names. Raw Cobertura is machine-generated measurement data of order ten megabytes and is not
committed in this repository; this mirrors the disposition feature #464 recorded for the same class of
artifact.

The class qualifier is part of the identification rather than decoration: `ClaimsAltChord` is not a unique
method name in the instrumented tree, because QuickFiler/Viewers/EfcViewer.cs declares a member of the
same name.

## Acceptance reading 1 — the `ClaimsAltChord` method element

The post-processed document contains a `method` element whose `name` attribute is `ClaimsAltChord`, under
the class whose `filename` attribute ends with `QfcFormKeyHandler.cs`. Its `line-rate` attribute is **1**,
which parses to a value of at least 0.90 as required. Its `branch-rate` is 1.

Verbatim XML fragment:

```xml
<method line-rate="1" branch-rate="1" complexity="6" name="ClaimsAltChord" signature="(QuickFiler.Interfaces.IQfcKeyboardHandler, System.Windows.Forms.Keys)"><lines><line number="29" hits="1" branch="False" /><line number="30" hits="1" branch="True" condition-coverage="100% (4/4)"><conditions><condition number="0" type="jump" coverage="100%" /><condition number="1" type="jump" coverage="100%" /></conditions></line><line number="31" hits="1" branch="False" /><line number="32" hits="1" branch="False" /><line number="35" hits="1" branch="False" /><line number="36" hits="1" branch="True" condition-coverage="100% (2/2)"><conditions><condition number="0" type="jump" coverage="100%" /></conditions></line><line number="37" hits="1" branch="False" /></lines></method>
```

Every one of the seven lines of the new method carries `hits="1"`, and both branch lines report
`condition-coverage="100%"` — 4 of 4 on the compound null-or-no-Alt guard on line 30, and 2 of 2 on the
`Keys.Menu` or `Keys.None` acceptance on line 36. The new method is at 100% line and branch coverage,
against the unit-test policy's `>= 90%` new-method floor.

## Acceptance reading 2 — the class line-rate against BASELINE_CLASS_LINE_RATE

Class `line-rate`: **1**.
BASELINE_CLASS_LINE_RATE from `[P0-T13]`: **1**.

1 is not lower than 1, so the clause holds. Both figures were read from the post-processed document.

Verbatim XML fragment of the `<class>` element:

```xml
<class line-rate="1" branch-rate="1" complexity="6" name="QuickFiler.Controllers.QfcFormKeyHandler" filename="QuickFiler\Controllers\QfcFormKeyHandler.cs"><methods><method line-rate="1" branch-rate="1" complexity="1" name="IsAltKeyCommand" signature="(System.Windows.Forms.Keys)"><lines><line number="19" hits="1" branch="False" /></lines></method><method line-rate="1" branch-rate="1" complexity="6" name="ClaimsAltChord" signature="(QuickFiler.Interfaces.IQfcKeyboardHandler, System.Windows.Forms.Keys)"><lines><line number="29" hits="1" branch="False" /><line number="30" hits="1" branch="True" condition-coverage="100% (4/4)"><conditions><condition number="0" type="jump" coverage="100%" /><condition number="1" type="jump" coverage="100%" /></conditions></line><line number="31" hits="1" branch="False" /><line number="32" hits="1" branch="False" /><line number="35" hits="1" branch="False" /><line number="36" hits="1" branch="True" condition-coverage="100% (2/2)"><conditions><condition number="0" type="jump" coverage="100%" /></conditions></line><line number="37" hits="1" branch="False" /></lines></method></methods><lines><line number="19" hits="1" branch="False" /><line number="29" hits="1" branch="False" /><line number="30" hits="1" branch="True" condition-coverage="100% (4/4)"><conditions><condition number="0" type="jump" coverage="100%" /><condition number="1" type="jump" coverage="100%" /></conditions></line><line number="31" hits="1" branch="False" /><line number="32" hits="1" branch="False" /><line number="35" hits="1" branch="False" /><line number="36" hits="1" branch="True" condition-coverage="100% (2/2)"><conditions><condition number="0" type="jump" coverage="100%" /></conditions></line><line number="37" hits="1" branch="False" /></lines></class>
```

The class carries exactly two method elements, `IsAltKeyCommand` and `ClaimsAltChord`, both at line-rate 1.
The `[P0-T13]` baseline recorded the same class with one method element, `IsAltKeyCommand`, and no
`ClaimsAltChord`; the new element is the change this task measures.

## Acceptance reading 3 — root figures against the `[P0-T13]` baseline

| Attribute | `[P0-T13]` baseline | Post-change | Difference |
|---|---|---|---|
| `line-rate` | 0.853866 | **0.853726** | −0.000140 |
| `lines-covered` | 54977 | **54974** | −3 |
| `lines-valid` | 64386 | **64393** | +7 |

Also recorded, for completeness, though not named by the acceptance: `branch-rate` moved from 0.794064 to
0.794078 (+0.000014), `branches-covered` from 13110 to 13115 (+5), and `branches-valid` from 16510 to
16516 (+6).

The plan states no threshold on the root figures; the acceptance is that they are recorded alongside the
baseline with the difference stated, which the table does.

Two observations on the root movement, stated at the strength of the evidence:

1. `lines-valid` grew by 7, which is the size of the new method's instrumented line set — lines 29, 30,
   31, 32, 35, 36 and 37, all listed in the fragment above.
2. `lines-covered` fell by 3 while every changed line is covered. AC-11's no-regression clause is about
   the changed lines, and those are at `hits="1"` throughout, so the changed-line requirement is met by
   direct measurement. The three-line movement lies outside the changed set. This repository's
   instrumented denominator is not fully deterministic between runs of the concurrent instrumented suite,
   and the two runs being compared are separate instrumented executions, so a small movement in the
   whole-repository totals is not attributable to this change on the evidence available. No stronger claim
   is made here.

The post-change root `line-rate` of 0.853726 remains above the repository-wide 0.85 line-coverage
threshold.

## Artifact disposition

`docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/qa-gates/coverage.cobertura.xml`
byte size before deletion: **10,792,221 bytes**. The file was deleted after the figures and the two XML
fragments above were transcribed.

Every absolute worktree path is rendered as `<repo-root>`; none remains in this artifact.

Output Summary: The coverage document was post-processed in place and read. The `<method>` element named
`ClaimsAltChord` exists under the `<class>` element whose `filename` ends with `QfcFormKeyHandler.cs`,
with `line-rate` 1 and `branch-rate` 1, meeting the `>= 0.90` new-method floor at 100%. That class's
`line-rate` is 1, not lower than the BASELINE_CLASS_LINE_RATE of 1 read from the same document kind. Root
`line-rate` moved from 0.853866 to 0.853726, `lines-covered` from 54977 to 54974 and `lines-valid` from
64386 to 64393. Both XML fragments are transcribed verbatim above and the raw 10,792,221-byte document was
then deleted.
