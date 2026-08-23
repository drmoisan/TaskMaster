# P8-T45 SpamBayes branch-scope remediation

Timestamp: 2026-07-27T03-01Z

Command: `git merge-base --is-ancestor 1cd21eb4 origin/main; git merge-base --is-ancestor 1cd21eb4 HEAD; git log --oneline origin/main..HEAD -- UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.Actions.cs`

EXIT_CODE: 0

Output Summary: Commit `1cd21eb4` is not an ancestor of `origin/main` (exit 1) and is an ancestor of `HEAD` (exit 0). The active branch log contains `1cd21eb4 (fix): SpamBayes.Actions.cs when Junk Folder is null`; therefore `SpamBayes.Actions.cs` is active branch scope for P8-T46 onward.

## Branch checks

| Check | Exit code | Required result |
| --- | ---: | --- |
| `git merge-base --is-ancestor 1cd21eb4 origin/main` | 1 | nonzero |
| `git merge-base --is-ancestor 1cd21eb4 HEAD` | 0 | zero |

`git log --oneline origin/main..HEAD -- UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.Actions.cs`:

`1cd21eb4 (fix): SpamBayes.Actions.cs when Junk Folder is null`

## Scope and preservation

- The historical P8-T20 through P8-T26 62-path ledger and protected-hash evidence remain historical formatter evidence.
- The historical exclusion of `UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.Actions.cs` is superseded for P8-T46 onward because that file is active `origin/main..HEAD` scope.
- Canonical P8-T44 TRX SHA-256: `C073B9E35134FECFB64EB015D11F475B7FEB70FD3823F684058B611AC358E235`
- Canonical P8-T44 diagnostic markdown SHA-256: `7642D99EAB29A8C14B72030D70635A4A4D523C0C64B3762B653933E999725236`
- Preserved `coverage.config` SHA-256: `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`
- Preserved `.csharpierignore` SHA-256: `362211A82C6C6887E023B1B1936408715D6C35CC42E0D91B46E8E76A29318C25`

## Reconfirmed P8-T44 failures

- `UtilitiesCS.Test.EmailIntelligence.ClassifierGroups.SpamBayes_Additional_Tests.GetDestinationFolder_WhenSpamTrue_ReturnsJunkCertain`
- `UtilitiesCS.Test.EmailIntelligence.ClassifierGroups.SpamBayes_Additional_Tests.MoveSpamOrHam_WithMailItemAndDestination_MovesMail`
- `UtilitiesCS.Test.EmailIntelligence.ClassifierGroups.SpamBayes_Additional_Tests.TestAsync_Selection_WhenInputContainsMailItem_ProcessesMessage`
- `UtilitiesCS.Test.EmailIntelligence.ClassifierGroups.SpamBayes_Additional_Tests.TestAsync_Object_WhenInputIsMailItem_ProcessesMessage`
