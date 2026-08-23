# SpamBayes branch-scope CSharpier gate

Timestamp: 2026-07-27T03-25
Command: `csharpier format UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.Actions.cs UtilitiesCS.Test/EmailIntelligence/SpamBayesActionsRegressionTests.cs` followed by `csharpier check UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.Actions.cs UtilitiesCS.Test/EmailIntelligence/SpamBayesActionsRegressionTests.cs`.
EXIT_CODE: 0
Output Summary: Format and check both exited 0. A repeated stable format pass preserved `SpamBayes.Actions.cs` SHA-256 `A33BA83881F1030293BDB725D05AFC355A0ACA019F390BED2FFD3819CE1FD0A3` and `SpamBayesActionsRegressionTests.cs` SHA-256 `B0E7C5229BD48933B912993E55871F87866B880E3AE6CD1D92D8D41164537E66`, proving no post-format delta. Physical lines are 115 and 51 respectively, both below 500. No delta occurred outside the two scoped C# sources and the authorized adjacent `UtilitiesCS.Test.csproj` include. Protected hashes remain unchanged: `coverage.config` `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`; `.csharpierignore` `362211A82C6C6887E023B1B1936408715D6C35CC42E0D91B46E8E76A29318C25`.
