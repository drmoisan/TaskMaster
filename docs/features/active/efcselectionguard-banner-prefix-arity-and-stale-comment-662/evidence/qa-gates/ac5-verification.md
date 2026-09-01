# AC5 Verification (P2-T15)

Timestamp: 2026-09-01T16-52

Command: `git grep -n -F -- '= "====";' -- '*.cs'`

EXIT_CODE: 0

Output Summary — exactly one line, located in
`UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs`:

```
UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs:19:        public const string BannerPrefix = "====";
```

The P0-T15 baseline recorded two matching lines
(`BreadcrumbRowBuilder.cs:19` and `FolderSuggestionTree.cs:16`). The count fell
to one because P1-T6 deleted the `FolderSuggestionTree.cs` declaration outright.
The surviving line is the producers' shared public constant, unchanged.

Command: `git grep -n 'BannerPrefix' -- UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs`

EXIT_CODE: 0

Output Summary — exactly one line:

```
UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs:198:                    OutlookObjects.Folder.BreadcrumbRowBuilder.BannerPrefix,
```

Both figures hold.

## The single remaining line is the qualified reference inside `IsBanner`

`FolderSuggestionTree.cs:194-201`, after the P2-T1 CSharpier wrap:

```csharp
        private static bool IsBanner(string row)
        {
            return row != null
                && row.StartsWith(
                    OutlookObjects.Folder.BreadcrumbRowBuilder.BannerPrefix,
                    StringComparison.Ordinal
                );
        }
```

The matching line `:198` sits inside `IsBanner` and contains
`BreadcrumbRowBuilder.BannerPrefix`, as AC5 requires.

`UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs` therefore declares no
banner-prefix constant at all. The declaration formerly at `:16` was DELETED, not
re-aliased. Deletion rather than aliasing is required because an aliasing
declaration (`private const string BannerPrefix = BreadcrumbRowBuilder.BannerPrefix;`)
would still match AC2's declaration regex and would make AC2's count two instead
of one; AC2's verification artifact records that count as one.

Per Decisions Record D3, no comment in this file contains the token
`BannerPrefix`; the surviving doc comment at `:193` refers to the banner shape as
`<c>"===="</c>` without naming the constant, which is why the count is one rather
than two.

The check-off in `issue.md` was anchored on the identifier `AC5` followed by a
space and an em dash. The identifier `AC5` is a prefix of `AC5b`, so an
unanchored edit would have altered the wrong criterion.

**AC5 checked off in `issue.md`.**
