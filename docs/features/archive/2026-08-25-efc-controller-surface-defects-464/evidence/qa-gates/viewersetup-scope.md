# [P9-T5] `QfcItemController.ViewerSetup.cs` one-line constraint

Timestamp: 2026-08-28T01-50
Task: [P9-T5]
Command 1: `git diff --numstat <BASE> -- QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`
Command 2: `git diff --unified=0 <BASE> -- QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`
EXIT_CODE: 0

## numstat — identical under both bases

| Base | added | deleted |
|---|---|---|
| `38f097898639b054428188c9c5e266e54972c259` (evaluated) | **1** | **1** |
| `002335989830ba9f3ad802858ef0b794f6281750` (`BASELINE_SHA`, as written) | **1** | **1** |

This file is unaffected by the base-drift recorded in `changed-file-set.md`: neither merged sibling
touched it, so both bases give the same figures and the acceptance condition is satisfied as literally
written.

## Unified diff at `--unified=0`, verbatim

```
diff --git a/QuickFiler/Controllers/QfcItemController.ViewerSetup.cs b/QuickFiler/Controllers/QfcItemController.ViewerSetup.cs
index fb671d3e..8dd91b77 100644
--- a/QuickFiler/Controllers/QfcItemController.ViewerSetup.cs
+++ b/QuickFiler/Controllers/QfcItemController.ViewerSetup.cs
@@ -61 +61 @@ namespace QuickFiler.Controllers
-            CoreWebView2EnvironmentOptions options = new("–incognito ");
+            CoreWebView2EnvironmentOptions options = new("--incognito ");
```

There is exactly **one** removed line and exactly **one** added line, and both are the incognito
argument line. There is exactly one hunk, `@@ -61 +61 @@`, so nothing else in the file moved.

## Byte-level proof of the replacement

`od -c` over delivered line 61 shows the two characters preceding `incognito` as `-` `-`. Read as Unicode
code points they are `0x2d` and `0x2d` — two **U+002D HYPHEN-MINUS** characters. The removed line carried
a single **U+2013 EN DASH**, which is why the pre-change WebView2 environment rejected the switch.

This is the byte-level evidence that `[P11-T13]`'s second manual reviewer check calls for.

## File size

`wc -l QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` reports **499**, unchanged from the
`[P0-T15]` baseline of 499. The plan's figure of 430 is a stale pre-#484 measurement recorded against a
different commit, as `file-sizes-and-exemptions.md` and the base-drift addendum both state; the
substantive constraint is the one-added/one-deleted numstat, which holds.

## Ownership

The one-line edit adds no `<Compile Include>` entry, so it is not a project-file region breach against
live sibling #489. It is carved out for this feature by `issue.md:212-214` and `spec.md` §RC5. Merged
feature #484 contains zero occurrences of `incognito`, `AdditionalBrowserArguments` or
`CoreWebView2EnvironmentOptions` in its spec or plan, so this edit intersects no #484 edit.

Output Summary: PASS. The diff over `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` is exactly
1 added and 1 deleted line under both bases, in a single `@@ -61 +61 @@` hunk, and both lines are the
incognito argument line. The delivered replacement bytes are two U+002D characters, verified by `od -c`
and by code-point inspection. The file remains 499 lines.
