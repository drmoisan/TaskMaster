---
name: teardown-guard-enumeration-821
description: "Issue #821 enumeration: ProgressViewer is the 4th Cancel() SITE not the 4th sharer (9 holders); SetCancellationTokenSource is the unconditional enabler, not the CancelSource setter; only 1 extra unguarded ?.Invoke() site (EfcHomeController.cs:349)"
metadata:
  type: project
---

Research for issue #821 (teardown-guard enumeration gaps from #810), 2026-09-08. Three findings
that contradicted the issue text and would mislead a plan that trusted it.

**The "fourth sharer" label is wrong; the underlying number is right.** `ProgressViewer` is the
**fourth site that calls `Cancel()`** on the QFC session token source, not the fourth *holder*.
The derived holder count is **9** and ProgressViewer is the **third** to receive the reference.
The figure came from #810's research table headed "Every `Cancel()` on the shared instance", which
had exactly four rows; the label drifted to "sharer" when it was carried into `issue.md`.

**`ProgressViewer`'s enabling hole is in the opposite member from the one the issue names.** The
issue says the `CancelSource` setter (`ButtonCancel.Enabled = value != null;`) is "a second path
to enabling the button". That setter is the *correct* path. `SetCancellationTokenSource` is the
one that sets `Enabled = true` unconditionally with no null check. Fix that member, not the setter.

**Only ONE additional unguarded cleanup-callback site exists** beyond the named one, and it is
`EfcHomeController.Cleanup()` — which invokes `_parentCleanup.Invoke()` with no null-conditional
at all. `QuickFiler/Legacy/**` has two more but carries **no `<Compile Include>` entry**, so it is
dead source and must not be counted.

**Why:** #821 was filed specifically because #810 fixed one site and missed its siblings, so a plan
that re-inherits an unverified count or an inverted cause reproduces the exact failure the issue
exists to close. All three claims in the issue were flagged by the reporter as child-reported and
not re-derived.

**How to apply:** when a Site-B-style "N-th sharer" claim appears, ask which *family* was counted —
holders, `Cancel()` callers, and storing fields are three different numbers over the same graph.
When enumerating unguarded delegate invocations in QuickFiler, run BOTH an invocation-side
`\.Invoke\(\)` regex and a declaration-side delegate-field regex: the `readonly` Action fields in
`QuickFiler/Viewers/` are invoked as bare `X()` and the invocation regex alone misses them.
Always check `QuickFiler.csproj` for a `Legacy` `<Compile Include>` before counting a Legacy hit.

Related: [[qfc810-teardown-dropdown-residuals]], [[qfc-lifecycle-disposal-731]]
