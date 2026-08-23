# SpamBayes branch-scope formatter ledger

Timestamp: 2026-07-27T03-25
Command: `git diff --name-only origin/main...HEAD -- '*.cs'` combined with `git ls-files --others --exclude-standard -- 'QuickFiler/**/*.cs' 'QuickFiler.Test/**/*.cs' 'UtilitiesCS/**/*.cs' 'UtilitiesCS.Test/**/*.cs'`, filtered to the four authorized roots and ordered with `StringComparer.OrdinalIgnoreCase`.
EXIT_CODE: 0
Output Summary: Derived 64 authorized C# paths. The LF-joined SHA-256 is `260AD1BC2E644FBDA9CA8CCE204A221AFC4E1E6680AAB46CA1C706FD25EEA088`. The set includes `UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.Actions.cs` and `UtilitiesCS.Test/EmailIntelligence/SpamBayesActionsRegressionTests.cs`; neither is classified as unrelated. Protected hashes match P8-T20: `coverage.config` is `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943` and `.csharpierignore` is `362211A82C6C6887E023B1B1936408715D6C35CC42E0D91B46E8E76A29318C25`.

## Authorized paths

The deterministic path set is represented by the count and LF-joined SHA-256 above. Its SpamBayes members are:

- `UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.Actions.cs`
- `UtilitiesCS.Test/EmailIntelligence/SpamBayesActionsRegressionTests.cs`
