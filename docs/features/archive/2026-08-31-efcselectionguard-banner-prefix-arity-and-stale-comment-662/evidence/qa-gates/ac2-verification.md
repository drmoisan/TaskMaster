# AC2 Verification (P2-T12)

Timestamp: 2026-09-01T16-49

Command: `git grep -nE -- 'const +string +[A-Za-z_]*BannerPrefix' -- '*.cs'`

EXIT_CODE: 0

Output Summary — exactly one line, located in
`UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs`:

```
UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs:19:        public const string BannerPrefix = "====";
```

Command: `git grep -n -F -- 'StartsWith(BannerRejectionPrefix' -- QuickFiler/Controllers/EfcSelectionGuard.cs`

EXIT_CODE: 0

Output Summary — exactly two lines:

```
QuickFiler/Controllers/EfcSelectionGuard.cs:72:            return !value.StartsWith(BannerRejectionPrefix, System.StringComparison.Ordinal)
QuickFiler/Controllers/EfcSelectionGuard.cs:98:                && !value.StartsWith(BannerRejectionPrefix, System.StringComparison.Ordinal)
```

Both figures hold: **1** declaration line and **2** call-site lines.

## The declaration count fell from three to one by two independent decrements

The P0-T16 baseline recorded three matching declarations:
`QuickFiler/Controllers/EfcSelectionGuard.cs:15`,
`UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs:19`, and
`UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs:16`.

1. The guard's constant was renamed to `BannerRejectionPrefix`, which does not
   contain the substring `BannerPrefix` the regex requires, so
   `EfcSelectionGuard.cs` no longer matches.
2. The `FolderSuggestionTree.cs` declaration was deleted outright by P1-T6,
   rather than re-aliased. Deletion is required by Decisions Record D1: an
   aliasing declaration would still match this regex and would make the count
   two instead of one.

The one surviving match is the producers' shared public constant in
`BreadcrumbRowBuilder.cs`, which is the intended post-change state.

## Call-site count

The two call sites are the ones formerly at `:49` in `IsValidFilingSelection`
and `:75` in `IsValidCreationSelection`; they now sit at `:72` and `:98`
respectively, because P1-T4's multi-line XML doc shifted the file. Both read the
renamed constant.

This second command counts call sites only and is deliberately insensitive to
how many times the new name appears in doc-comment prose, so AC3's wording
cannot perturb this count. A count of one would have proved one call site was
missed and a count of zero that neither was updated, so the single figure
discriminates every partial outcome.

**AC2 checked off in `issue.md`.**
