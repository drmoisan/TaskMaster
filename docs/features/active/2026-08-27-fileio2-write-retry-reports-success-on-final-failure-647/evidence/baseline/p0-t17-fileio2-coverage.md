# P0-T17 — Baseline Per-File and Per-Method Coverage for the File Under Change

Timestamp: 2026-08-31T19-09
Command: read `coverage\coverage.cobertura.xml` under the governing derivation and aggregate over `UtilitiesCS/To Depricate/FileIO2.cs`
EXIT_CODE: 0

## Recorded Figures

BASELINE_FILEIO2_LINES_COVERED: 106
BASELINE_FILEIO2_LINES_VALID: 126
BASELINE_WRITETEXTFILEASYNC_LINES_COVERED: 23
BASELINE_WRITETEXTFILEASYNC_LINES_VALID: 29

Derived line rate for the method at baseline: 23 / 29 = 0.793103.

## Per-file aggregation

FILEIO2_CLASS_ELEMENT_COUNT: 1. Exactly one `class` element in the document has a `filename` attribute ending with `FileIO2.cs`: `name=UtilitiesCS.FileIO2`, `filename=UtilitiesCS\To Depricate\FileIO2.cs`. The plan's execution rules anticipate that an async method's state machine may be emitted as a separate `class` element and require aggregating every matching one; the aggregation is written to do that and in this document finds one.

The aggregation reads the class-level `lines/line` entries, not a descendant-or-self `.//line` search. This distinction is load-bearing and is recorded so the post-change figure is produced by the identical rule: the `<method>`-level `<line>` entries are a subset of the class-level list, so a descendant-or-self search counts every method line twice. The uncorrected descendant-or-self reading returns 189 / 223 for this same file and is not the figure recorded here.

## Per-method aggregation and a measured departure from the plan's stated mechanism

METHOD_ELEMENT_UNION_COUNT: 0.

The plan's P0-T17 defines the per-method aggregation as the union of `method` elements whose `name` attribute is `WriteTextFileAsync` together with every `method` element in a matching class whose `name` contains that text. **That union is empty in this coverage document.** The `UtilitiesCS.FileIO2` class element carries 9 `method` elements — `DELETE_TextFile`, `WriteTextFile`, `WriteUTF8`, `CSV_ReadTxtF`, `CsvRead`, `SplitArrayTo2D`, `CsvReadTo2D`, `CsvReadToJagged` and `.cctor` — and none of them is or contains `WriteTextFileAsync`. A repository-wide search of the document for any `class` element whose `name` contains `WriteTextFileAsync`, and for any `method` element anywhere whose `name` contains it, also returned zero matches.

The measured cause: dotnet-coverage attributes the async method's compiler-generated state machine lines to the parent class's class-level `<lines>` list without emitting a corresponding named `<method>` entry, and without emitting a separate state-machine `<class>` element. The plan's execution rules anticipated the separate-class shape; the observed shape is the merged-into-parent one. Reporting the stated union verbatim would record 0 covered of 0 valid, which is numeric but vacuous and would leave the AC20 changed-method threshold unevaluable.

**Substitute derivation, fixed here and used identically at post-change.** The per-method figure is the subset of the class-level line list whose `number` falls inside the source-line span of a `WriteTextFileAsync` declaration in `UtilitiesCS/To Depricate/FileIO2.cs`. Spans are located mechanically: scan the source for a line whose trimmed text matches the declaration form `^(public|internal)\s+static\s+(async\s+)?Task(<bool>)?\s+WriteTextFileAsync\(`, then brace-match forward from that line to the closing brace of a block body or to the terminating semicolon of an expression body. At baseline this locates exactly one span, lines 50 through 89, matching the declaration `public static async Task WriteTextFileAsync(` at line 50. The identical scan run against post-change source locates both overloads, so the same rule produces the post-change figure with no re-interpretation.

## Zero-hit lines inside the method span at baseline

Six of the 29 lines in the span carry `hits="0"`:

```
line 69 | {
line 70 | success = true;
line 71 | foreach (var output in strOutput)
line 72 | await sw.WriteLineAsync(output);
line 73 | }
line 74 | }
```

These are the entire body of the writer's `using` block. Line 68, the `StreamWriter` constructor, carries `hits="1"`: the existing locked-fixture test reaches the constructor, which throws on every attempt, so no test in the suite has ever executed a single line of the write body. That is the direct measurement behind the research file's finding that the mid-write failure branch is unexercised, and it is why the mid-write defect could survive undetected.
