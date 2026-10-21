# Phase 4 — manual review of the `QfcItemController.ViewerSetup.cs` replacement bytes

Timestamp: 2026-08-28T00-29
Task: [P4-T6]
Command: `sed -n '61p' QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` and a per-character byte dump of that line, before and after the edit; `git diff` over the file
EXIT_CODE: 0

## Why review is the instrument here

The changed line sits inside `QfcItemController.InitializeWebViewAsync`, which carries its own
`[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` attribute (at `ViewerSetup.cs:47`, directly
above the member's declaration at `:48`) and requires the real WebView2 runtime to execute. Under the
unit-test policy — no live host, no external process, no runtime dependency — that member cannot be run,
so no executable assertion can observe the value it passes.

Unlike the `EfcItemController` site, a hoisted constant is **not** available here: this feature's diff
over `QfcItemController.ViewerSetup.cs` is constrained to exactly one line, and introducing a member
would exceed that. The instrument is therefore review of the delivered bytes, plus the one-line-diff
assertion recorded in `evidence/qa-gates/463-viewersetup-one-line-diff.md`. That limitation is stated
here rather than concealed.

## Delivered text of the changed line

Line **61** of `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`:

```csharp
            CoreWebView2EnvironmentOptions options = new("--incognito ");
```

## Code point of each of the first two argument characters

The string argument begins immediately after the opening quotation mark (`0x22`). Its first two
characters, read from the delivered file's bytes:

| Position in the argument | Byte | Code point | Name |
|---|---|---|---|
| 0 | `2D` | **U+002D** | HYPHEN-MINUS |
| 1 | `2D` | **U+002D** | HYPHEN-MINUS |

Both are U+002D, which is what Chromium requires to introduce a command-line switch.

Before the edit, position 0 was the three-byte sequence `E2 80 93`, the UTF-8 encoding of U+2013 EN DASH,
and there was no character at position 1 other than the `i` of `incognito`.

## Confirmation that no other character on the line changed

The two byte dumps of line 61, taken before and after the edit, are recorded in full in
`evidence/qa-gates/463-viewersetup-one-line-diff.md`. They are identical except for the single
substitution of `E2 80 93` by `2D 2D`. Specifically unchanged:

- the twelve leading space characters of the indentation;
- the whole of `CoreWebView2EnvironmentOptions options = new(`;
- the token `incognito`;
- the **trailing space** inside the string, byte `20`, which separates this switch from any further one;
- the closing `");` and the CRLF terminator `0D 0A`.

## Reviewer confirmation statement

I have compared the delivered line 61 against its pre-change bytes character by character. The only
difference is that the single U+2013 EN DASH that opened the `CoreWebView2EnvironmentOptions` argument
has been replaced by two U+002D HYPHEN-MINUS characters. The argument now reads exactly `--incognito `
with its trailing space preserved. No other character on the line, and no other line in the file, was
modified. The change is correct and complete for the QuickFiler path of #463.

## Residual limitation, stated

This confirms the value present in the source. It does not confirm the runtime effect: no WebView2
process was launched and no browsing-data behaviour was observed, because the unit-test policy forbids
it. The assumption that Chromium silently ignores an unrecognised `AdditionalBrowserArguments` token
remains marked UNVERIFIED in `spec.md` and is not discharged by this artifact.

Output Summary: The delivered `QfcItemController.ViewerSetup.cs:61` reads
`CoreWebView2EnvironmentOptions options = new("--incognito ");`. The first two argument characters are
U+002D and U+002D. Byte comparison confirms no other character on the line changed, including the
trailing space. Review is the stated instrument because the enclosing member is coverage-exempt and needs
the real WebView2 runtime.
