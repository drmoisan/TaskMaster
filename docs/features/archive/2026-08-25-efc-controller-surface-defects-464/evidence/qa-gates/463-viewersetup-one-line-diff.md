# Phase 4 — `QfcItemController.ViewerSetup.cs` one-line diff

Timestamp: 2026-08-28T00-28
Task: [P4-T5]
Command: `git diff --numstat 002335989830ba9f3ad802858ef0b794f6281750 -- QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`; `git diff 002335989830ba9f3ad802858ef0b794f6281750 -- QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`; `wc -l` on the file; a per-line byte dump before and after; `dotnet tool run csharpier check` on the file
EXIT_CODE: 0

## Diff size

```
1       1       QuickFiler/Controllers/QfcItemController.ViewerSetup.cs
```

**Exactly 1 added line and 1 deleted line.** The complete set of changed lines in the diff is:

```
-            CoreWebView2EnvironmentOptions options = new("–incognito ");
+            CoreWebView2EnvironmentOptions options = new("--incognito ");
```

No other line, no other character, and no other member of that file was touched. `csharpier check`
returns `EXIT_CODE: 0` on the delivered file, so the edit introduced no formatting drift.

## Byte-level proof that only the two argument characters changed

Before (line 61):

```
20 20 20 20 20 20 20 20 20 20 20 20 43 6F 72 65 57 65 62 56 69 65 77 32 45 6E 76 69 72 6F 6E 6D 65 6E
74 4F 70 74 69 6F 6E 73 20 6F 70 74 69 6F 6E 73 20 3D 20 6E 65 77 28 22 E2 80 93 69 6E 63 6F 67 6E 69
74 6F 20 22 29 3B 0D 0A
```

After (line 61):

```
20 20 20 20 20 20 20 20 20 20 20 20 43 6F 72 65 57 65 62 56 69 65 77 32 45 6E 76 69 72 6F 6E 6D 65 6E
74 4F 70 74 69 6F 6E 73 20 6F 70 74 69 6F 6E 73 20 3D 20 6E 65 77 28 22 2D 2D 69 6E 63 6F 67 6E 69 74
6F 20 22 29 3B 0D 0A
```

The single difference is `E2 80 93` (UTF-8 for U+2013 EN DASH) becoming `2D 2D` (two U+002D
HYPHEN-MINUS). Everything else, including the twelve leading spaces, the trailing space inside the
string, and the CRLF terminator, is byte-identical.

## Recorded deviations from the task's stated locators

`[P4-T5]` says to "change line 55 only" and asserts "the file's line count is still 430". Both figures are
stale, exactly as `plan-base-drift-addendum.2026-08-27T21-01.md` predicted and as `[P0-T15]` recorded:

| Item | Task's figure | Actual on this base |
|---|---|---|
| line of the incognito literal | 55 | **61** |
| file line count | 430 | **499**, unchanged by this edit |

The +69-line drift is merged feature #484, which the base carries. The site was resolved by content — the
sole occurrence of the token `incognito` in the file — not by line number, and the delivered line count
is 499 both before and after, which is the substantive requirement: the edit changes one line and adds
none.

## Textual-conflict risk, restated at its true level

The plan states this risk as materially raised because "the branch point does not carry #484", making the
two edits concurrent. **That premise is false on this base.** #484 merged as PR #619 and is present: this
edit lands on the post-#484 text of the file, which is why the literal is at `:61` rather than `:55`.
The two edits are therefore **sequential, not concurrent**, and the conflict risk is correspondingly
**lower** than the plan rates it, not higher.

Independently, `spec.md` §RC5 records that a search of the entire `qfc-item-controller-defects-484`
folder for `incognito`, `CoreWebView2EnvironmentOptions` and `ViewerSetup.cs:5x` returned zero matches,
so #484 neither touches nor depends on this literal.

Should a conflict nevertheless arise at fan-in, `spec.md` §RC5 is binding: **keep both edits, never drop
this one.** This feature is the only owner of this defect; if the edit is lost, the defect survives on the
QuickFiler path with no other owner. `[P9-T5]` and `[P10-T2]` re-verify the one-line shape after any such
resolution.

## Ownership

This edit adds no `<Compile Include>` entry and touches no project file, so it is not a breach of any
sibling-owned project-file region. It is carved out explicitly by `issue.md` and by `spec.md` §RC5 as the
one authorised exception to the "no other change to `QfcItemController.ViewerSetup.cs`" rule.

Output Summary: `git diff --numstat` reports exactly 1 added and 1 deleted line. The only byte-level
change is `E2 80 93` becoming `2D 2D` at the incognito argument; the file is still 499 lines and still
passes `csharpier check`. The literal is at line 61, not the plan's stale 55, and the file is 499 lines,
not the plan's stale 430 — both deviations predicted by the base-drift addendum.
