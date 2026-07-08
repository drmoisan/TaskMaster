# Baseline — csproj Compile-Include Wiring (issue #211)

Timestamp: 2026-06-24T15-10

Command: `grep -c "Compile Include" <csproj>` and `grep -c 'Compile Include="\*\*' <csproj>` and `grep -n "SpamBayes.cs" UtilitiesCS/UtilitiesCS.csproj`

EXIT_CODE: 0

Output Summary:
- `UtilitiesCS/UtilitiesCS.csproj`: 399 explicit `<Compile Include>` items; 0 glob includes (`Compile Include="**`). Uses explicit-include wiring.
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj`: 349 explicit `<Compile Include>` items; 0 glob includes. Uses explicit-include wiring.
- Existing anchor: `SpamBayes.cs` is wired at line 630 of `UtilitiesCS/UtilitiesCS.csproj`:
  `<Compile Include="EmailIntelligence\ClassifierGroups\SpamBayes\SpamBayes.cs" />`.
- Planned new files NOT yet wired (confirmed absent from both csproj files): `SpamBayes.Conditions.cs`,
  `SpamBayes.Actions.cs`, `SpamBayes.Classify.cs`, `SpamInitTimingProbe.cs` (UtilitiesCS.csproj);
  `SpamInitTimingProbeTests.cs` (UtilitiesCS.Test.csproj).
- Conclusion: both projects require explicit `<Compile Include>` wiring for new `.cs` files; no glob auto-inclusion.
