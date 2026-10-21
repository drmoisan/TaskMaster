# CSharpier Format — Final QC (P2-T1)

Timestamp: 2026-09-01T15-59

Command: `dotnet tool run csharpier format .`

EXIT_CODE: 0

Output Summary:

Final summary line, transcribed verbatim from the final (second) loop pass:

```
Formatted 1566 files in 2071ms.
```

## Tree observation distinguishing a rewriting run from a non-rewriting one

The exit code alone cannot make this distinction, because this subcommand
rewrites files in place and still exits 0 after rewriting. The observation below
is `git status --porcelain -- QuickFiler UtilitiesCS QuickFiler.Test` captured
immediately before the command and again immediately after.

**Final pass (pass 2) — the pass AC9 is judged against.**

Before:

```
 M UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs
```

After:

```
 M UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs
```

**Set difference of the two listings: EMPTY.** No path appears in the after
listing that is absent from the before listing, and none is removed. This run
rewrote nothing, which is the non-rewriting outcome the loop requires before it
can proceed to a clean pass.

The single `M` entry present in both listings is the Phase 1 source change
already made by P1-T6 and then wrapped by pass 1 of this task; it is a
pre-existing uncommitted modification, not a rewrite performed by this run.

## Loop pass history

**Pass 1.** Before: empty listing. After:
`M UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs`. Set difference:
exactly one path, `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs`.
Summary line: `Formatted 1566 files in 5147ms.`

That path is one of the four in-scope files named in the Scope Boundary, so the
"stop and report" branch of this task's acceptance did not arise. The rewrite
was the CSharpier wrap of the `IsBanner` reader that P1-T6 explicitly predicted:
the rewritten reader measured 132 characters on one line and CSharpier wrapped
it into the same multi-line `StartsWith` shape used at
`EfcFormController.cs:1143-1148`. P1-T6 records that this rewrite is expected and
triggers the ordinary Phase 2 loop restart rather than being a failure.

Post-wrap shape at `FolderSuggestionTree.cs:194-201`:

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

The wrap keeps `BannerPrefix` on a single line (`:198`), so AC5's single-line
assertion holds after the format pass, as P1-T6 predicted. The wrap-sensitive
assertions were re-checked after the rewrite and all still hold:

- `git grep -n 'BannerPrefix' -- UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs` — 1 line (`:198`)
- `git grep -n -F -- 'StartsWith(BannerRejectionPrefix' -- QuickFiler/Controllers/EfcSelectionGuard.cs` — 2 lines (`:72`, `:98`)
- `git grep -c -F -- '("===")' -- QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs` — 2
- `git grep -c -F -- '("====")' -- QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs` — 2
- `git grep -c -F -- 'must not be widened' -- QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs` — 1

Neither `EfcSelectionGuard.cs`, `EfcFormController.cs` nor
`EfcSelectionGuardTests.cs` was rewritten by either pass. Neither protected file
— `BreadcrumbRowBuilder.cs` and `EfcFormControllerTests.cs` — appears in either
listing, so the repository-wide pass did not touch them and the AC5b and AC7
zero-diff gates remain reachable, as the P0-T7 baseline foresaw.

The loop is bounded at three passes; this run used two, and the second was
clean.
