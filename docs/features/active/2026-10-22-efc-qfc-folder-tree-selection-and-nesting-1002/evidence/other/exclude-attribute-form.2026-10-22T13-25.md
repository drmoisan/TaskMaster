# ExcludeFromCodeCoverage attribute form for the net48 target (P1-T11, #1002 cycle 1)

Timestamp: 2026-10-22T13-25
Command: msbuild QuickFiler/QuickFiler.csproj /t:Build /p:Configuration=Debug /p:Platform=AnyCPU with a temporary [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = "probe")] on ActionOkAsync (probe edit replaced by P1-T12)
EXIT_CODE: 1
Output Summary:
- Compiler rejected the named property: error CS0246 "The type or namespace name 'Justification' could not be found" at EfcFormController.Actions.cs on the ActionOkAsync attribute.
- The .NET Framework 4.8.1 ExcludeFromCodeCoverageAttribute has no Justification property.
ATTRIBUTE-FORM: COMMENT
