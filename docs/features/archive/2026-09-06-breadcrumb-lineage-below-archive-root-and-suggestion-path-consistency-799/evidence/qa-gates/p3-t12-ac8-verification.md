# [P3-T12] AC8 verification — the renderer does not alter a leading underscore

Timestamp: 2026-09-07T08-20

Command: git grep -n -E 'Replace\(["'']_|letter-spacing|text-transform|first-letter|word-break|word-spacing' -- "UtilitiesCS/*.cs" "UtilitiesCS/*.html" "UtilitiesCS/*.css" "QuickFiler/*.cs" "QuickFiler/*.html" "QuickFiler/*.css" "ToDoModel/*.cs" "TaskMaster/*.cs" "Tags/*.cs" "TaskVisualization/*.cs"

EXIT_CODE: 1

ExpectedExitCode: 1

`git grep` exits 1 when it matches nothing, and zero matches is this task's SUCCESS outcome. The expectation is
declared explicitly so a passing gate is not normalised to `fail` by an evidence collector that defaults the
expectation to 0.

## Search result

OUTPUT-LINES: 0

The command produced no output lines at all. The regex, as passed to `git grep -n -E`, was:

```
Replace\(["']_|letter-spacing|text-transform|first-letter|word-break|word-spacing
```

The pattern was constructed from character codes inside the PowerShell block rather than typed as a literal,
because a bash-hosted single-quoted `pwsh -Command` payload cannot carry an embedded single quote. The pattern
actually passed to git is printed above verbatim from the run, so what was searched is recorded rather than
assumed.

## The five traced transformations, each re-verified against the current tree

Each claim below was checked directly in this pass; none is transcribed from the specification without
confirmation.

### 1. The verbatim splitter inserts nothing and trims nothing

UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs, `SplitVerbatim` at lines 242-246, with the
`Split` call itself at line 244:

```
private static string[] SplitVerbatim(string verbatimText)
{
    var parts = verbatimText.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);
    return parts.Length == 0 ? new[] { verbatimText } : parts;
}
```

It splits on the path separators and removes empty entries. `String.Split` copies the characters between
separators unchanged; `RemoveEmptyEntries` only discards zero-length parts. A leading underscore is not a
separator and is not zero-length, so it survives verbatim, and no whitespace is introduced anywhere. The
zero-parts fallback returns the original string by reference, so it cannot transform either.

### 2. The JSON serializer escapes only the double quote, the backslash and control characters

UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs, `Formatting = Formatting.None` at line 41 and
`JsonConvert.SerializeObject(message, OutboundSettings)` at line 58. `Formatting.None` emits no indentation and,
more to the point, adds no whitespace INSIDE a string value under any formatting setting. Json.NET's string writer
escapes the double quote, the backslash and the C0 control characters; the underscore is none of those and is
written through unchanged.

### 3. The QuickFiler page assigns segment text through `textContent`

QuickFiler/Resources/FolderBreadcrumb.html, seven assignments, all through the DOM `textContent` property:

```
L253: element.textContent = cell.text;
L262: element.textContent = "";
L266: element.textContent = cell.kind === "plus" ? "+" : "-";
L299: selectedPath.textContent = state.selectedFolder;
L309: pct.textContent = row.percentText;
L327: name.textContent = subfolder.displayName;
L357: list.textContent = "";
```

Line 253 is the segment-text assignment. `textContent` sets the node's character data literally: it performs no
HTML parsing, no entity decoding and no escaping, so it cannot introduce a space after an underscore. The page
contains no `innerHTML` assignment for segment text.

### 4. The Efc page encodes ampersand, less-than, greater-than and quote characters only

UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs routes every user-visible string through
`WebUtility.HtmlEncode` — at lines 99, 124, 134, 186, 188, 209, 224 and 226. The segment display name is line 188
and the full path line 186. `WebUtility.HtmlEncode` replaces the markup-significant characters and characters
above the ASCII range with numeric or named references; the underscore (U+005F) and the space (U+0020) are neither
markup-significant nor above the ASCII range, so both pass through byte-for-byte. No non-breaking space is emitted:
a case-insensitive search of the file for `nbsp` returned nothing.

### 5. Neither stylesheet contains a spacing or casing transform

There are exactly two breadcrumb stylesheets, both embedded rather than standalone `.css` files: the `<style>`
block at UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs line 39 (the Efc assets) and the `<style>`
block at QuickFiler/Resources/FolderBreadcrumb.html line 10 (the QuickFiler page). A search of each for
`letter-spacing`, `word-spacing`, `text-transform`, `first-letter` and `word-break` returned 0 matches in each.

A `git ls-files` scan for the substrings `.html` and `.css` returned four entries in the whole repository:

```
QuickFiler/Resources/EmailHeader.html
QuickFiler/Resources/FolderBreadcrumb.html
UtilitiesCS.Test/Resources/EmailHtmlBodyWithDownloadableLinks.html
UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Html.cs
```

Three of those are `.html` documents and the fourth is a `.cs` source file matched only because its name contains
the segment `.Html.`. No `.css` file is tracked anywhere in the repository. Of the three `.html` documents, one is
the QuickFiler breadcrumb page examined above, one is an unrelated email-header template and one is a test
fixture, so there is no third breadcrumb stylesheet the search could have missed.

## The WinForms path is also excluded

The WinForms mnemonic prefix character is the ampersand, not the underscore: a label or combo-box item containing
`&X` renders `X` with an underlined accelerator, and an underscore is an ordinary literal character to that
renderer. No combo-box, owner-draw or label path in the QuickFiler item controller can therefore be responsible
for inserting a space after a leading underscore.

## Conclusion, stated as a definite finding

The reported space after the leading underscore was a transcription artifact. Both render paths were traced end to
end and no code on either path alters a leading underscore. The renderer is correct, and a renderer change would
be a defect rather than a fix.

FILES-CHANGED-FOR-AC8: 0

No file was edited by this task. [P3-T20] records the separately anchored confirmation that the QuickFiler
resources directory carries no change either.

Output Summary: The repository-wide negative search across the six product projects returned zero matches and
exited 1, which is this task's success outcome and is declared as `ExpectedExitCode: 1`. All five traced
transformations were re-verified against the current tree with file and line citations: the verbatim splitter
inserts and trims nothing, the JSON serializer escapes only quote, backslash and control characters, the
QuickFiler page assigns through `textContent`, the Efc page encodes only markup-significant characters through
`WebUtility.HtmlEncode` and emits no non-breaking space, and neither of the two embedded stylesheets contains any
of the five spacing or casing properties. The WinForms mnemonic prefix is the ampersand, so no WinForms path can
be responsible. AC8 is satisfied by this verified finding and no code change was made.

## Path hygiene (R3)

No absolute host path, host account name, or machine name appears in this artifact.
