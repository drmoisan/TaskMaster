# P3-T2: Skip Re-Validation — MetricChartViewer.cs

## File
`UtilitiesCS\EmailIntelligence\Bayesian\Performance\MetricChartViewer.cs`

## Current Coverage
`line-rate="0"` (0%) — no test file exists.

## Source Analysis
The file is a WinForms `partial class` inheriting `Form`:
```csharp
public partial class MetricChartViewer : Form
{
    public MetricChartViewer()
    {
        InitializeComponent();
    }
}
```
- Sole executable line: `InitializeComponent()` (auto-generated designer call).
- No business logic or public API surface beyond default Form construction.
- Companion `MetricChartViewer.Designer.cs` is fully auto-generated.

## Skip Rationale
Identical to `ConfusionViewer.cs`: the only coverable line is the auto-generated
`InitializeComponent()` call. No domain behaviour exists to verify.

## Decision: Skip Confirmed
