# P1-T3 — Compile entry for BreadcrumbDropDownHost.Diagnostics.cs

Timestamp: 2026-09-07T14-17
Task: [P1-T3]
Issue: #796
Channel used: A

Command:

```
pwsh -NoProfile -Command 'Select-String -Path QuickFiler\QuickFiler.csproj -SimpleMatch "BreadcrumbDropDownHost.Diagnostics.cs"'
```

EXIT_CODE: 0

Recorded output, verbatim:

```
QuickFiler\QuickFiler.csproj:417:    <Compile Include="Viewers\BreadcrumbDropDownHost.Diagnostics.cs" />
```

MATCHING-LINE-COUNT: 1

Exactly one matching line, at line 417, placed immediately after the existing entry
for `Viewers\BreadcrumbDropDownHost.Open.cs` at line 416. The two anchor entries
this plan cites, `Viewers\BreadcrumbDropDownHost.cs` at line 415 and
`Viewers\BreadcrumbDropDownHost.Open.cs` at line 416, were confirmed at those lines
before the edit.

QuickFiler/QuickFiler.csproj is non-SDK-style, so without this entry the new partial
part would be silently not compiled and every downstream assertion about it would be
vacuous.

The edit was applied to QuickFiler/QuickFiler.csproj. QuickFiler/QuickFiler.csproj.bak
is a tracked file that is not in the write set and was not read or edited.

Output Summary: One `<Compile Include>` entry added for the new diagnostics part;
the search reports exactly one matching line.
