# .editorconfig Severity Map for New Analyzer Rules (Issue #181)

Timestamp: 2026-06-08T12-27

## Rule ID prefixes added, all targeting severity = suggestion
| Package | ID prefix | Enumerated count | Default-severity finding |
|---|---|---|---|
| Meziantou.Analyzer 3.0.101 | MA#### | 200 IDs (MA0001..MA0202) | 111 default to Warning/Error (incl. MA0037/MA0039/MA0049 = Error) |
| SonarAnalyzer.CSharp 10.27.0.140913 | S### / S#### | ~450 (NOT fully statically enumerable in this host) | many default to Warning |
| Roslynator.Analyzers 4.15.0 | RCS#### (+ ROS0003) | 243 IDs | 16 default to Warning |
| AsyncFixer 2.1.0 | AsyncFixer01..AsyncFixer06 | 6 IDs | default Warning |
| SecurityCodeScan.VS2019 5.6.7 | SCS#### | SCS0000..SCS0034 (35) | default Warning |
| Microsoft.CodeAnalysis.BannedApiAnalyzers 3.3.4 | RS0030, RS0031, RS0035 | 3 IDs | RS0030/RS0031 Warning, RS0035 Error |

All target severity: suggestion (RS0030 specifically held at suggestion per P1-T8 banned-symbol volume of 143 existing usages).

## Enumeration method and the implementation decision (IN-SCOPE per directive)
- Default severities were obtained by loading each analyzer DLL under the build's Roslyn 5.6 host (pwsh) and reading DiagnosticAnalyzer.SupportedDiagnostics[].DefaultSeverity.
- Meziantou, Roslynator, and BannedApi fully enumerated. AsyncFixer (AsyncFixer01-06) and SecurityCodeScan (SCS0000-SCS0034) IDs are well-defined fixed ranges.
- SonarAnalyzer.CSharp 10.x ships ~450 rules and would NOT enumerate via static reflection in this host (it references a conflicting Microsoft.CodeAnalysis version), so its complete warning-default ID set cannot be reliably enumerated statically.
- BECAUSE a single missed warning-default ID would be promoted to an error under the nullable /p:TreatWarningsAsErrors=true gate, the robust guarantee chosen is a GLOBAL analyzer-diagnostic default at suggestion:
    `dotnet_analyzer_diagnostic.severity = suggestion`
  This sets the default severity for ALL analyzer diagnostics that lack a more-specific entry, which guarantees that every new package rule (including the un-enumerable Sonar set) is at suggestion and therefore cannot break the nullable gate.
- To avoid lowering the ONE existing meaningful analyzer warning observed in the baseline (MSTEST0032, 1 occurrence in QuickFiler.Test), it is explicitly re-asserted at warning AFTER the global default:
    `dotnet_diagnostic.MSTEST0032.severity = warning`
  No other analyzer rule emitted at warning in the baseline analyzer build (all other baseline warnings are compiler CS-prefixed diagnostics, which are unaffected by dotnet_analyzer_diagnostic.severity).
- In ADDITION to the global default, explicit per-ID `dotnet_diagnostic.<id>.severity = suggestion` entries are written for every fully-enumerated new-package ID (Meziantou 200, Roslynator 243 + ROS0003, AsyncFixer 6, SecurityCodeScan 35, BannedApi RS0030/RS0031/RS0035) for auditability and to make the intent explicit and reviewable.
- This is the directive-blessed in-scope mechanism ("the fix is almost always setting the offending analyzer rule severity to suggestion in .editorconfig"), applied so that NO new analyzer rule can be at warning/error under the nullable build.

## Critical invariant satisfied
- RS0030, RS0031, RS0035 are all set to suggestion (never warning/error), with an in-file comment that RS0030 promotion to warning is post-cleanup follow-up.
- No new analyzer ID is set to warning or error anywhere in .editorconfig.
- Existing `csharp_style_namespace_declarations = block_scoped:silent` is left unchanged.
