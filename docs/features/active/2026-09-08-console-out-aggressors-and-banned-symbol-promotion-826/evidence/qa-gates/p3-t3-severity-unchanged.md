# RS0030 severity untouched by the item-3 edit (issue #826, [P3-T3])

Timestamp: 2026-09-09T19-19

Command, run as one `pwsh -NoProfile -Command` block carrying the plan's C2 preamble branch guard:

```
git diff $Base -- .editorconfig
```

with the changed lines then partitioned in-process: lines beginning with a single `+` or `-`, excluding
the `+++` and `---` file headers, counted for the `-SimpleMatch` tokens `dotnet_diagnostic.` and
`.severity`.

EXIT_CODE: 0

## Gate figures

| Measure | Observed | Required |
|---|---|---|
| changed lines containing `dotnet_diagnostic.` | 0 | 0 |
| changed lines containing `.severity` | 0 | 0 |
| lines in `.editorconfig` exactly equal to `dotnet_diagnostic.RS0030.severity = suggestion` | 1 | 1 |

Total changed lines in the anchored diff: 11 (9 added, 2 removed). Every one of them is a comment line
inside the `BannedApiAnalyzers` block. None is a severity assignment.

## Full anchored diff of `.editorconfig`

Anchored on base `dea7b49dae31a9bda8d35ecb73b8c8d646b1a460`:

```
diff --git a/.editorconfig b/.editorconfig
index e77198d0..ac7ab9cf 100644
--- a/.editorconfig
+++ b/.editorconfig
@@ -543,8 +543,15 @@ dotnet_diagnostic.AsyncFixer05.severity = suggestion
 dotnet_diagnostic.AsyncFixer06.severity = suggestion
 
 # --- Microsoft.CodeAnalysis.BannedApiAnalyzers 3.3.4 (RS003x) ---
-# RS0030 held at suggestion for initial rollout. Promotion to warning is a
-# post-cleanup follow-up (143 existing banned-symbol usages, see issue #181 evidence).
+# RS0030 is held at suggestion. Promotion to warning is blocked by exactly one
+# precondition: the pre-existing banned-symbol call sites must be cleared first,
+# because toolchain step 3 (msbuild ... /p:TreatWarningsAsErrors=true, mirrored by
+# .github/workflows/_build-nullable.yml) promotes every warning to a build error,
+# and the analyzer step, which passes no TreatWarningsAsErrors, would not break.
+# Verified textual surface, 2026-09-08 (re-measured at implementation time):
+# DateTime.Now 53, DateTime.UtcNow 20, Random.Shared 5, Thread.Sleep 15,
+# Task.Delay 60 = 153 textual hits. Textual hits are not diagnostics; the earlier
+# recorded figure was a diagnostic count and is not comparable to this one.
 dotnet_diagnostic.RS0030.severity = suggestion
 dotnet_diagnostic.RS0031.severity = suggestion
 dotnet_diagnostic.RS0035.severity = suggestion
```

The unchanged context line `dotnet_diagnostic.RS0030.severity = suggestion` sits immediately below the
rewritten comment, which is direct visual confirmation that the value survived the comment amendment.

## Recorded mechanical note — BOM stripped and restored

An intermediate state of this task showed one additional changed-line pair at the top of the file:

```
-﻿[*.cs]
+[*.cs]
```

The cause was the write-back in [P3-T2]: PowerShell 7's `Set-Content -Encoding UTF8` writes UTF-8
**without** a byte-order mark, and `.editorconfig` carries one. The comment content was unaffected; only
the BOM was lost. The file was rewritten with `Set-Content -Encoding utf8BOM`, which restored the mark
and removed that hunk from the anchored diff. The diff above is the post-restore state and its first
hunk starts at line 543, not line 1.

The lone-carriage-return separators the file carries were preserved throughout, because the read-modify-
write cycle used `Get-Content -Raw` and `Set-Content -NoNewline` and therefore never re-split the
content into lines. Recorded because an encoding-only change to line 1 is invisible in a rendered diff
and would otherwise have travelled into the commit unnoticed.

Output Summary: the anchored diff of `.editorconfig` contains 11 changed lines, all of them comment
lines in the `BannedApiAnalyzers` block. Zero changed lines contain `dotnet_diagnostic.` and zero contain
`.severity`, and the file still contains exactly one line equal to
`dotnet_diagnostic.RS0030.severity = suggestion`. AC11's no-severity-change requirement holds against the
item-3 edit.
