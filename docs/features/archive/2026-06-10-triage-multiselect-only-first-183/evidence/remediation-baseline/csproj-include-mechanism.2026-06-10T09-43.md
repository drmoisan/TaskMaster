# Baseline — UtilitiesCS.Test.csproj Include Mechanism (Remediation Cycle 1, Issue #183 R1)

Timestamp: 2026-06-10T09-43

## Output Summary

- Mechanism: explicit `<Compile Include="..." />` items. No wildcard/glob (`**`, `*.cs`) is used for compile items in this non-SDK project.
- Anchor line (line 129): `<Compile Include="EmailIntelligence\ClassifierGroups\Triage\Triage_OlLogicTests.cs" />`
- Conclusion: the new sibling partial file `Triage_OlLogicTests.TrainSelection.cs` MUST be added with an explicit `<Compile Include=...>` entry; it will not be picked up automatically. The new entry will be inserted immediately after line 129.
