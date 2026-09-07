# P1-T6 — Compile entry for BreadcrumbDropDownCloseOrderingTests.cs

Timestamp: 2026-09-07T14-21
Task: [P1-T6]
Issue: #796
Channel used: A

Command:

```
pwsh -NoProfile -Command 'Select-String -Path QuickFiler.Test\QuickFiler.Test.csproj -SimpleMatch "BreadcrumbDropDownCloseOrderingTests.cs"'
```

EXIT_CODE: 0

Recorded output, verbatim:

```
QuickFiler.Test\QuickFiler.Test.csproj:83:    <Compile Include="Viewers\BreadcrumbDropDownCloseOrderingTests.cs" />
```

MATCHING-LINE-COUNT: 1

Exactly one matching line. The entry was placed alongside the existing Viewers
entries, which occupied lines 83 through 89 before the edit
(`Viewers\BreadcrumbDropDownHostTests.cs` at 83 through
`Viewers\BreadcrumbPendingOpenCloseTests.cs` at 89); the new entry takes line 83 and
shifts those seven down by one.

QuickFiler.Test/QuickFiler.Test.csproj is non-SDK-style, so without this entry the
new test class would be silently not compiled and the P1-T11 assertion about it would
be vacuous.

Output Summary: One `<Compile Include>` entry added for the new test class; the
search reports exactly one matching line, at line 83.
