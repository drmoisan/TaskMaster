# P1-T2 — `.csharpierignore` gains the manifest and app.config patterns

Timestamp: 2026-09-19T12-08

Command: `git diff --numstat 734112ed25bba293cb074e71fee2286bc3b72fae -- .csharpierignore`; `git diff 734112ed25bba293cb074e71fee2286bc3b72fae -- .csharpierignore`; `git status --porcelain --untracked-files=all -- .csharpierignore`

EXIT_CODE: 0

## Edit

Two patterns appended with the `Edit` tool, each preceded by its own one-line comment giving the
reason recorded by Scope Decision 2.

## Merge-base diff

`.csharpierignore` exists at `<MERGE_BASE>` `734112ed25bba293cb074e71fee2286bc3b72fae`, so the
merge-base anchor is correct for this file.

```
git diff --numstat 734112ed25bba293cb074e71fee2286bc3b72fae -- .csharpierignore
4       0       .csharpierignore
```

```
@@ -12,3 +12,7 @@
 *.csproj
 *.props
 *.targets
+# The repository adopts the inline form the NuGet CLI writes these manifests in, so the formatter no longer owns them.
+**/packages.config
+# The repository adopts the inline form the NuGet CLI writes these binding-redirect files in, so the formatter no longer owns them.
+**/app.config
```

**4 added, 0 deleted — additions only.** The pre-existing 14 lines appear in no `-` position, so
they are unchanged.

Porcelain companion, per gate rule 8:

```
 M .csharpierignore
```

## Acceptance evaluation

| Clause | Measured | Verdict |
|---|---|---|
| A line whose text is exactly `**/packages.config` | present, line 16 | PASS |
| A line whose text is exactly `**/app.config` | present, line 18 | PASS |
| The other 14 lines unchanged, verified by a merge-base diff showing only additions | numstat `4  0`; no deletion hunk line | PASS |
| Each new pattern preceded by a one-line comment giving the reason | lines 15 and 17 | PASS |

The file is now 18 lines: the 14 pre-existing plus 2 comments and 2 patterns.

Output Summary: `.csharpierignore` gains `**/packages.config` and `**/app.config`, each preceded by
its own reason comment. The merge-base diff is 4 added and 0 deleted lines, additions only, so the
14 pre-existing lines are unchanged. Porcelain reports the file modified. This edit precedes the
P1-T7 normalisation, as Scope Decision 2 requires, so the normalisation is not reverted by the next
format step.
