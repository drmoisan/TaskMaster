# [P0-T12] Coverage measurability of the seven Write Set production files

Timestamp: 2026-09-06T14-30

Command: a separator-anchored trailing-name query over every `class` element in
`coverage\791-baseline.cobertura.xml`:

```powershell
$doc = New-Object System.Xml.XmlDocument
$doc.Load((Resolve-Path -LiteralPath 'coverage\791-baseline.cobertura.xml').Path)
$names = @('QfcStreamingDequeueConfidenceGate.cs','IQfcDatamodel.cs','QfcDatamodel.QueueProcessing.cs','QfcDatamodel.cs','QfcFormController.EventHandlers.cs','QfcFormController.Deactivate.cs','QfcHomeController.cs')
foreach ($n in $names) {
    $hit = 0
    foreach ($c in $doc.SelectNodes('//class')) {
        $f = $c.GetAttribute('filename')
        if ($f.EndsWith('\' + $n) -or $f.EndsWith('/' + $n)) { $hit++ }
    }
    "$n classElements=$hit"
}
```

EXIT_CODE: 0

The match is separator-anchored on purpose: an unanchored `QfcDatamodel.cs` suffix would also select
`IQfcDatamodel.cs`, which would report the excluded partial as measurable through its interface
file's class element.

## Class-element counts the determination was made from

TOTAL-CLASS-ELEMENTS-IN-DOCUMENT: 3282

| Write Set production path | class elements |
|---|---|
| `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` | 3 |
| `QuickFiler/Interfaces/IQfcDatamodel.cs` | 1 |
| `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | 0 |
| `QuickFiler/Controllers/QfcDatamodel.cs` | 0 |
| `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` | 14 |
| `QuickFiler/Controllers/QfcFormController.Deactivate.cs` | 1 |
| `QuickFiler/Controllers/QfcHomeController.cs` | 8 |

## Determination

MEASURABLE: QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs
MEASURABLE: QuickFiler/Interfaces/IQfcDatamodel.cs
UNMEASURABLE: QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs
UNMEASURABLE: QuickFiler/Controllers/QfcDatamodel.cs
MEASURABLE: QuickFiler/Controllers/QfcFormController.EventHandlers.cs
MEASURABLE: QuickFiler/Controllers/QfcFormController.Deactivate.cs
MEASURABLE: QuickFiler/Controllers/QfcHomeController.cs

MEASURABLE-COUNT: 5
UNMEASURABLE-COUNT: 2
TOTAL: 7

## Reading

The determination matches D1 exactly. The two zero-count files are the two `QfcDatamodel` partials.
`QuickFiler/Controllers/QfcDatamodel.cs` line 25 carries `[ExcludeFromCodeCoverage]` on the partial
class declaration; the attribute applies to the whole type, so members declared in
`QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`, which declares
`public partial class QfcDatamodel` at line 12, are excluded too. Changed-line coverage for those
two files is structurally unmeasurable rather than merely low, and [P3-T7] records
`CHANGED-LINE-COVERAGE: NOT MEASURABLE` for both with named-test evidence as the substitute.

The five measurable paths are the set [P3-T7] compares. The command block [P3-T7] carries enumerates
exactly these five paths, so this determination and that block agree and no divergence has to be
recorded.

`QuickFiler/Interfaces/IQfcDatamodel.cs` is reported measurable at the file level because the
`QfcDequeueBatch` struct emits IL. That is a file-level fact and does not imply any *changed* line
in it is executable: [P1-T1] adds only an enum member, an interface method declaration and XML docs,
none of which emits IL. [P3-T7] resolves that one level lower, per changed line, with the
`hits=non-executable` marker.
