# Phase 5 — AC16 wrong-label scan

Timestamp: 2026-09-09T13-24
Task: [P5-T3]

The phrase this scan searches for is the mislabelling the research record contradicted: it conflates
two distinct derived figures. The token source constructed at
`QuickFiler/Controllers/QfcHomeController.cs` line 54 has **9 holders**, of which `ProgressViewer` is
the **third** to receive the reference; the figure 4 is correct only for the narrower family "sites
that call `Cancel()` on that instance", where `ProgressViewer` is the fourth of exactly four. The
unqualified phrase must not appear in delivered code, comments or commit messages.

Command:

```text
pwsh -NoProfile -Command '$cs = @(Get-ChildItem -Path . -Recurse -File -Filter "*.cs"); "cs file count=" + $cs.Count; $m = @($cs | Select-String -SimpleMatch -Pattern "fourth sharer"); "fourth sharer matches=" + $m.Count'
```

EXIT_CODE: 0

Verbatim output:

```text
cs file count=1674
fourth sharer matches=0
```

## Result

**0 matches across all 1674 `*.cs` files in the repository**, comments included. `-SimpleMatch` was
used, so the search is a literal substring match rather than a regular expression.

## Why the search is scoped to `*.cs`

The scoping is deliberate and is not a narrowing that makes the condition easier to satisfy. The
phrase legitimately appears in this feature's own documents: `issue.md` carries it as the claim under
correction, and `spec.md` carries it inside correction row C3 and in the "stated so the label cannot
drift again" paragraph, both of which exist precisely to deny it. A repository-wide search would
return those denial passages, and the zero-hit condition could then never be met however clean the
code was. Restricting to `*.cs` targets exactly what AC16 governs for code: the delivered production
code, test code and code comments.

The commit-message half of AC16 is discharged separately by `[P7-T24]`, which searches the committed
message for the same literal.

Output Summary: **0 matches** for the literal across all 1674 `*.cs` files in the repository,
comments included. The wrong label was not carried into code.
