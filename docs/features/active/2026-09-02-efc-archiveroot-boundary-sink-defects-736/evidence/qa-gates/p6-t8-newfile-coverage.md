# P6-T8 — New-file coverage for `TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs`

Timestamp: 2026-09-04T02-03

Command: XML query over the P6-T6 Cobertura document, grouping every `//class/lines/line` element on
the pair (filename, number) and taking the maximum `hits`, plus a source read of
`TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs` to derive the wrapper's source span.

EXIT_CODE: 0

**The document read here is the one produced by the P6-T6 execution that followed P6-T13**, not the
one that preceded it. P6-T13 added three tests to
`QuickFiler.Test/Controllers/EfcFormControllerTests.Part2.cs` and therefore executed before this task
despite its higher number; the toolchain restart it triggered re-ran P6-T1 through P6-T7 and
overwrote the Cobertura document. Every count below is derived from that refreshed document, whose
SHA-256 the P6-T6 artifact records as
`A462D34E34BCA57A8AFC77A861562C1CBD5674B27EAC062BFE3DBC729044A777`.

## Class key matched

The key matched on, spelled with the Windows separator, is exactly:

```
TaskMaster\AppGlobals\AppOlObjects.ArchiveRoot.cs
```

`ConvertTo-KoverageCoberturaXml` passes `-PathSeparator`, defaulting to
`[System.IO.Path]::DirectorySeparatorChar`, into `ConvertTo-KoverageRelativePath`, whose `\` branch
returns `$relativePath.Replace('/', '\')` at Invoke-MSTestWithCoverage.Helpers.ps1:95, so every
`<class @filename>` in this document is repository-relative with backslashes. A match against the
forward-slash spelling this plan's prose uses returns zero rows, which would make the floor
unevaluable rather than failed. One `<class>` node carries this key.

## Clause 1 — strict figures

| Figure | Value |
|---|---|
| Strict coverable | **21** |
| Strict covered | **18** |
| Strict quotient | **85.71%** |

The class key matched 21 rows, so the floor is evaluable. The 21 line numbers are 45, 50, 51, 52,
53, 54, 55, 60, 61, 62, 63, 64, 67, 68, 69, 70, 71, 72, 89, 90, 91.

## Clause 2 — did the exclusion attribute fail wholesale?

**No.** The `[ExcludeFromCodeCoverage]` COM-touching wrapper
`internal string ResolveValidatedArchiveRootPath()` is declared at line 86 and its closing brace is
at line 93, so its source span is lines 86 through 93. The nearest preceding non-blank line is line
85, which is `[ExcludeFromCodeCoverage]`, as clause 3(a) requires. The only lines of that span
present anywhere in the document are 89, 90 and 91, which are exactly the lambda-argument lines
clause 3 enumerates. **No body line of the wrapper other than those appears**, so the attribute did
not fail wholesale and there is nothing to report as a drift condition.

## Clause 3 — the lifted-lambda set `L`

Derived mechanically. Under the `<class>` element carrying the backslash-spelled key above, the
`//class/methods/method` elements whose `name` attribute, after XML entity decoding, begins with the
fixed string `<ResolveValidatedArchiveRootPath>b__` are, with their decoded names recorded verbatim:

| Decoded `name` | Line | Hits |
|---|---|---|
| `<ResolveValidatedArchiveRootPath>b__74_0` | 89 | 0 |
| `<ResolveValidatedArchiveRootPath>b__74_1` | 90 | 0 |
| `<ResolveValidatedArchiveRootPath>b__74_2` | 91 | 0 |

`L` = {89, 90, 91}. **Size of `L`: 3.** Every member lies inside the span 86 through 93 derived in
clause 3(a), so no line is outside the span and no drift condition arises.

The angle brackets are literal characters of the compiler-generated member name. In the XML they are
entity-encoded, so a raw text search must use the form `&lt;ResolveValidatedArchiveRootPath&gt;b__`;
the decoded spelling returns zero matches against the raw file and would silently empty `L`. The
query above reads the attribute through the XML DOM, which decodes the entities, so it matches on
the decoded prefix correctly. The trailing `74_0` ordinal is compiler-assigned from the enclosing
member's position in the class and shifts when unrelated members are added, so it is not asserted
verbatim; the anchor is the prefix.

The three lines are the wrapper's argument list — the delegate literals
`() => Path.Combine(Root.FolderPath, "Archive")`, `() => ArchiveRoot?.FolderPath`, and
`message => logger.Error(message)`: two live Outlook COM crossings and the logger sink. Each captures
`this`, so the compiler lifts it into a separate instance member of `AppOlObjects` rather than into a
compiler-generated display type, and the attribute on the declaring member does not reach a member
emitted beside it. That is why the attribute removed the wrapper's own body lines while leaving
these three.

## Clause 4 — adjusted figures, and the `>= 90.00` floor

| Figure | Value |
|---|---|
| Strict coverable | 21 |
| Strict covered | 18 |
| Strict quotient | 85.71% |
| Size of `L` | **3** |
| Members of `L` with maximum `hits` > 0 | **0** |
| Adjusted coverable (21 − 3) | **18** |
| Adjusted covered (18 − 0) | **18** |
| **Adjusted quotient** | **100.00%** |

**The `>= 90.00%` floor for this file is met: 100.00% against a floor of 90.00%.** Both figures and
the size of the adjustment are recorded side by side above so the size of the adjustment is visible
rather than absorbed into a single number.

After clause 3 the denominator is the delegate-driven static core
`ResolveValidatedArchiveRootPath(Func<string>, Func<string>, Action<string>)` and nothing else, whose
eighteen lines are all covered. This gate would have failed had two of those eighteen gone uncovered.

## Clause 5 — uncovered lines remaining after `L` is removed

`none`

Every uncovered line the document carries for this file — 89, 90 and 91 — is a member of `L`.

## Relationship to D2 and to P6-T9

Clause 4's adjustment is not the exclusion-shopping D2 prohibits. The removed set is fixed by a name
prefix this plan quotes and by a source span anchored on a literal this plan also quotes, both
properties of the compiler's lambda lowering rather than judgments about which lines feel untestable,
and the derivation is reproducible by a third party from the document and the source file alone. The
claim is confined to those three lines; no line of the decision logic is removed by it.

These same three lines remain counted in P6-T9's strict denominator `N`, which is the figure that
task's floor is evaluated against, and they are also members of P6-T9 clause 1's unreachable set `U`
as group (c). `U` and `N` are different quantities: `U` scales only the `10U` escape condition, and
every line of `L` is still counted in P6-T9's strict denominator and strict quotient.

Output Summary: the class key `TaskMaster\AppGlobals\AppOlObjects.ArchiveRoot.cs` matched one class
node carrying 21 coverable lines, 18 of them covered, a strict quotient of 85.71%. No wrapper body
line other than the three lambda-argument lines appears in the document, so the exclusion attribute
did not fail wholesale. The lifted-lambda set `L` is {89, 90, 91}, size 3, all with `hits="0"` and
all inside the wrapper's source span of lines 86 to 93. The adjusted figures are 18 covered over 18
coverable, **100.00%**, which meets the `>= 90.00%` new-file floor. No uncovered line remains after
`L` is removed.
