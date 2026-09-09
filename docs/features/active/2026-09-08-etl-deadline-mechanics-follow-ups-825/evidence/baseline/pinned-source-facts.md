# Baseline — Pinned Pre-Change Source Facts

Timestamp: 2026-09-09T16-40

Every later acceptance condition in this plan is expressed as a transition from one of the values
below. Each was measured directly against this worktree at the D3 anchor commit
96fd3dd86cff542226d192158f1c2f63d5eee926, and each matched its stated expectation.

TimeOutTaskLines: 1011
OlTableExtensionsTestsLines: 1846
EtlCsLines: 476
TableAccessCsLines: 432
DfDeedleCsLines: 319
DfDeedleQfcColumnsCsLines: 297
EtlCsDataBangCount: 2
EtlCsBudgetExpressionCount: 2
TimeOutTaskCatchTimeoutCount: 3
TableAccessLiteral2000ArgumentCount: 1
OlTableExtensionsTestsFakeTimeProviderCount: 1

## What each value counts

TimeOutTaskLines is the line count of UtilitiesCS/Threading/TimeOutTask.cs.
OlTableExtensionsTestsLines is the line count of
UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs.
EtlCsLines is the line count of UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs.
TableAccessCsLines is the line count of
UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs.
DfDeedleCsLines is the line count of UtilitiesCS/Extensions/DfDeedle.cs.
DfDeedleQfcColumnsCsLines is the line count of UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs.
EtlCsDataBangCount is the number of occurrences of the token `data!` in
OlTableExtensions.Etl.cs.
EtlCsBudgetExpressionCount is the number of occurrences of the token `250 * rowCount` in
OlTableExtensions.Etl.cs.
TimeOutTaskCatchTimeoutCount is the number of occurrences of the token `catch (TimeoutException)`
in TimeOutTask.cs.
TableAccessLiteral2000ArgumentCount is the number of lines in OlTableExtensions.TableAccess.cs
matching the regular expression `^\s+2000,$`.
OlTableExtensionsTestsFakeTimeProviderCount is the number of occurrences of the token
`new FakeTimeProvider()` in OlTableExtensions_Tests.cs. Its single pre-change occurrence is at line
974, inside EtlAsync_WithBinaryAndObjectFieldsAndProgress_ReturnsTransformedData, which is unrelated
to GetTableInViewAsync and must not be mistaken for the subject of AC22.

## Supplementary measurement

TimeOutTask_Tests.cs carries 217 lines before this feature. That figure is not one of the eleven
pinned values; it is recorded here because P4-T4 asserts a strict reduction from it.
