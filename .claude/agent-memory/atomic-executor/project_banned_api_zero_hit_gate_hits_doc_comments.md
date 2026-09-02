---
name: banned-api-zero-hit-gate-hits-doc-comments
description: A "zero hits for Thread.Sleep / Task.Delay / UiThread.Init / ShowDialog" gate over test files is unsatisfiable when the same plan mandates XML doc comments saying those APIs are not used; measure comment-stripped and report both counts
metadata:
  type: project
---

A test-policy gate phrased as "all four searches return zero hits across those files" cannot pass
when the plan elsewhere mandates in-file comments explaining that the banned API is deliberately
avoided. Always run the search twice — raw, and with comment lines stripped — and report both.

**Why:** #468's P14-T12 required zero hits for `Thread.Sleep`, `Task.Delay`, `UiThread.Init`, and
`ShowDialog` across seven test files. The raw every-line search returned **1, 1, 1, 1**; the
comment-stripped search returned **0, 0, 0, 0**. All four hits were `///` XML doc lines asserting the
API is *not* used, and two of them were required by other decisions in the same plan — D9 mandates
the STA class carry a comment stating it never calls `Show()` or `ShowDialog()`. Deleting the four
lines would have turned the search green by destroying documentation the plan itself demanded.

The strip filter that worked: `grep -v -E '^\s*(///|//|\*|/\*)'` piped into `grep -F -c "$lit"`.

**PowerShell variant — `Select-String` is case-insensitive unless `-CaseSensitive` is passed.** A
zero-hit gate spelled `Select-String -SimpleMatch 'throw'` also matches `ThrowIfCancellationRequested`,
`NotThrowAsync`, `InvokesWithoutThrowing`, and `ThrowsAsync`. Issue #670's P1-T4 required zero `throw`
hits in a new fault-boundary file whose body — dictated by the same task — carried the comment
`// fault: InitializeWebViewAsync opens with Token.ThrowIfCancellationRequested().`, so the gate was
unsatisfiable as written. The fix is `-CaseSensitive`, not deleting the comment: a genuine rethrow is
always lower-case `throw;` or `throw ex;`, so the case-sensitive search still detects the thing the
gate exists to detect. Sweep every `Select-String` zero-match and exact-count condition in a plan for
this: the same trap fires on `'Task.Delay'` vs `task.delay`, and on any identifier whose PascalCase
form contains the banned lower-case token.

**How to apply:** at preflight, rewrite such a clause to name the executable-code measurement
explicitly. During execution, if the clause survives as written, record both counts side by side,
quote each raw hit with its file:line and the full comment text, and state plainly which reading is
met and which is not. Do not delete the comments. This is the same distinction the plan's own
`### Literals asserted by acceptance conditions` section already draws for prose in `docs/**` — a
mention is not a use — and it applies equally inside an XML doc comment. See
[[bugfix-phase-grows-the-file-despite-dead-code-removal]] for the sibling case of a clause whose
presumed fact turns out false.
