# Subfolder Commit Failure-Before Gate

Timestamp: 2026-07-21T23-14Z

Command: `$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'; $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1; if (-not $vstest -or -not (Test-Path -LiteralPath $vstest)) { throw 'vstest.console.exe not found via vswhere.' }; & $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbSubfolderSelectorSessionTests|FullyQualifiedName~BreadcrumbSubfolderActivationTests|FullyQualifiedName~BreadcrumbSelectorMessagesTests" /Logger:"console;Verbosity=normal"`

EXIT_CODE: 1

Output Summary: Expected-failure gate accepted. VSTest resolved through `vswhere`, discovered all 30 filtered cases across both assemblies, and completed in 3.6896 seconds. Eighteen existing/invalid-input controls passed and 12 newly named regressions failed through intended assertions. No build, discovery, tool-resolution, display, infrastructure, environmental, or unrelated failure occurred.

- Resolved VSTest: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`.
- Total: 30; passed controls: 18; intended failures: 12.

## Durable open-session commit failures

- `OpenSelector_SubfolderActivationThenEnter_PreservesCommittedFullPath` failed because the selector remained open after subfolder activation.
- `OpenSelector_SubfolderActivationThenEscape_PreservesCommittedFullPath` failed because the selector remained open after subfolder activation.
- `OpenSelector_SubfolderActivationThenAutomaticClose_PreservesCommittedFullPath` failed because the selector remained open after subfolder activation.

Each test captured the correct subfolder full-path readback at activation and then exercised its independent follow-up before asserting; the missing explicit commit allowed the follow-up to end or roll back the still-open session.

## Composed event, close, and focus failures

- `OpenSelector_SubfolderActivationThenEnter_PublishesAndClosesExactlyOnce` failed because activation left the selector open instead of ending it through explicit commit.
- `OpenSelector_SubfolderActivationThenEscape_PublishesAndClosesExactlyOnce` failed for the same missing durable close boundary.
- `OpenSelector_SubfolderActivationThenNativeClose_PublishesAndClosesExactlyOnce` failed for the same missing durable close boundary.

These tests also require one full-path `SelectionChanged`, one explicit-commit host close, one focus return, no rollback, and later Enter/Escape/native close no-ops after remediation.

## Explicit message-contract failures

- `SubfolderActivationMessage_RoundTripsUniqueRowIdentityAndSubfolderIndex` failed because `BreadcrumbSelectorSubfolderActivationMessage` does not exist.
- `SubfolderActivationConstructor_RejectsBlankIdentityAndNegativeIndex` failed at the same absent message type.
- Four `Parse_InvalidSubfolderActivationPayload_RejectsExplicitly` cases failed because `selectorSubfolderActivate` is still unknown rather than validating `rowIdentity` and `subfolderIndex`.

## Passing boundary controls

- Existing selector view/toggle/key/row-activation serialization and parser tests passed.
- `OpenSelector_InvalidSubfolderIndexes_LeaveSessionAndParentSelectionUnchanged` passed.
- `OpenSelector_InvalidIdentityAndIndexes_DoNotPublishCloseFocusOrMutate` passed.
- `OpenSelector_SubfolderActivationForPlainRow_IsDeterministicNoOp` passed.

The result isolates the missing explicit subfolder commit/message behavior and preserves the existing invalid-input no-op/error contracts.
