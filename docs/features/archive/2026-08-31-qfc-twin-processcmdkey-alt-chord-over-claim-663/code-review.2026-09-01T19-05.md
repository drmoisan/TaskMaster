# Code Review — qfc-twin-processcmdkey-alt-chord-over-claim-663

- **Issue:** #663
- **Branch:** `bug/qfc-twin-processcmdkey-alt-chord-over-claim-663` @ `20f1b201`
- **Base:** `origin/main` @ `9ca9e99a` (also the merge base)
- **Review timestamp:** 2026-09-01T19-05
- **C# files reviewed:** 3 (the complete `.cs` change set)

The worktree root is rendered as `<repo-root>` throughout.

## Summary

The change is small, correct, well-tested, and consistent with the delivered precedent for the twin surface. The claim decision now depends on the key-code half of the key value rather than on the Alt modifier bit alone, which is the right invariant: `ProcessCmdKey` dispatches the parameterless `ToggleKeyboardDialogAsync()`, an overload that accepts no key data and therefore can only encode a bare Alt press.

**Blocking findings: 0.** Five non-blocking observations follow, each with an explicit reachability statement.

## The Change

`QuickFiler/Controllers/QfcFormKeyHandler.cs` gains one member and one `using` directive:

```csharp
internal static bool ClaimsAltChord(IQfcKeyboardHandler handler, Keys keyData)
{
    if (handler is null || !keyData.HasFlag(Keys.Alt))
    {
        return false;
    }

    Keys keyCode = keyData & Keys.KeyCode;
    return keyCode == Keys.Menu || keyCode == Keys.None;
}
```

`QuickFiler/Viewers/QfcFormViewer.cs` replaces a four-line guard with a single call:

```csharp
if (Controllers.QfcFormKeyHandler.ClaimsAltChord(_keyboardHandler, keyData))
```

`IsAltKeyCommand` is untouched, as `git diff -U0` confirms: zero removed lines match `IsAltKeyCommand`.

## Correctness

**Behavior preservation on the retained path.** The old guard was `_keyboardHandler is not null && IsAltKeyCommand(keyData)`. The new predicate performs the same null test and the same `HasFlag(Keys.Alt)` test in the same order, then adds the key-code mask. Every input the old guard rejected is still rejected; the new predicate rejects a strict superset. The null-handler semantics are unchanged, and moving the null test inside the predicate is safe because the call site passes `_keyboardHandler` directly and the body still dereferences it only after the predicate returns `true`.

**Mask arithmetic.** `Keys.KeyCode` is `0x0000FFFF` and `Keys.Alt` is `0x00040000`, so `keyData & Keys.KeyCode` strips all three modifier bits and isolates the virtual-key code. `Keys.Menu` is 18, documented as "The ALT key". The arithmetic in the behavior table of `spec.md` was checked row by row against the delivered code and is correct in every row.

**Short-circuit ordering is load-bearing and correct.** Testing `HasFlag(Keys.Alt)` before the mask is necessary, not stylistic. `Keys.Control` masks to `Keys.None` in its key-code half, so a predicate that inspected only the mask would claim a bare Ctrl press. `ClaimsAltChord_WithoutAltFlag_ReturnsFalse` pins exactly this, and its second because-string states the reason explicitly: "Keys.Control carries no Alt flag even though its key-code half is Keys.None". That is a well-chosen test with a genuinely load-bearing assertion, not a filler case.

**Precedent parity, verified rather than asserted.** The delivered #467 predicate at `QuickFiler/Viewers/EfcViewer.cs:96-104` was read directly. Its body is character-equivalent to the new one — same guard, same mask, same acceptance pair. The parity claim in `spec.md` is accurate. The only deliberate deviation is placement, on the type rather than on the viewer, which `spec.md` justifies on two grounds that both check out: `QfcFormViewer` carries `[ExcludeFromCodeCoverage]` at line 17 while `QfcFormKeyHandler` carries none, so only this placement is measurable; and the class's own XML summary already describes it as holding pure routing predicates extracted from the `ProcessCmdKey` overrides for unit testing. The placement choice is better than the precedent it deviates from.

## Design and Structure

| Aspect | Assessment |
|---|---|
| Simplicity | One pure function, one bitwise mask, two comparisons, early return on the negative guard. No indirection introduced. |
| Separation of concerns | Pure claim logic sits in a controller-side helper; WinForms message plumbing stays in the viewer. Consistent with the class's stated purpose. |
| Cohesion | The new member belongs on `QfcFormKeyHandler` alongside `IsAltKeyCommand`; both are pure key-routing predicates. |
| Public surface | Type and member are both `internal`. The test assembly reaches the member through the pre-existing `[assembly: InternalsVisibleTo("QuickFiler.Test")]`, so no attribute or project change was needed. |
| Documentation | Full XML doc comment: `<summary>`, both `<param>` elements, `<returns>`. Explains what the predicate decides, not how the bit test works. |
| Naming | `ClaimsAltChord` matches the delivered precedent, so the two surfaces read the same. Local `keyCode` is descriptive. |
| Error handling | The predicate is total over its input domain: no throw, no log, no allocation, no side effect. Correct for a pure predicate. |
| File size | 39, 293, and 211 lines — all far below the 500-line limit. |

## Test Quality

All seven methods named in the `spec.md` Test Strategy table are present with exactly the specified names, and all seven follow explicit Arrange-Act-Assert sectioning with a because-string on every assertion.

Three aspects are worth calling out as above-baseline quality:

1. **Both key-data shapes of a bare Alt press are pinned.** `ClaimsAltChord_WithBareAltFlagAndHandler_ReturnsTrue` covers the synthetic `Keys.Alt` value, and `ClaimsAltChord_WithMenuKeyCodeAndAltFlag_ReturnsTrue` covers `Keys.Menu | Keys.Alt`, the shape a physical keyboard produces. `spec.md` identifies that the Email Filer suite pins only the synthetic shape, leaving the `Keys.Menu` arm of that predicate deletable without detection. This suite closes that gap on its own surface. Deleting either arm of `keyCode == Keys.Menu || keyCode == Keys.None` here now fails a named test.

2. **The because-strings carry surface-specific justification.** `ClaimsAltChord_WithAltM_ReturnsFalse` names Move Options rather than copying the Email Filer's Filters wording, which would have stated a justification that is false for this surface — `ButtonFilters.Text` on the QuickFiler form is the plain string `"Filters"` with no ampersand. The red-run transcript shows the payoff: the failure message read as a complete, surface-accurate explanation of what was wrong.

3. **The RED state is genuine and is evidenced by content, not by claim.** `evidence/regression-testing/red-run.md` reproduces three FluentAssertions failures reading `but found True` against an expected `False`, each attributed to declaring type `QuickFiler.Controllers.Tests.QfcFormKeyHandlerTests` from its stack trace. That attribution matters, because `QuickFiler.Test` separately declares methods named `ClaimsAltChord_WithAltM_ReturnsFalse` and `ClaimsAltChord_WithNullHandler_ReturnsFalse` on the Email Filer fixture, so a bare method name is not a unique identifier in this assembly. The executor recorded the declaring type for every failure rather than the bare name. That is the correct discipline and it was applied without prompting.

The test-count arithmetic is self-checking and consistent across four runs: baseline 6927; red 6934 with 3 failed; green and final 6934 with 0 failed. `6927 + 7 = 6934` confirms that exactly seven methods were added and that no existing test was removed or renamed — independent corroboration of AC-8 that does not rely on reading the diff.

## Non-Blocking Findings

### CR-1 — The `Keys.None` acceptance arm claims modifier-only Alt combinations

`ClaimsAltChord` returns `true` when the key-code half is `Keys.None`. Two input classes reach that arm besides the synthetic `Keys.Alt` value the unit tests supply:

- `Keys.Alt | Keys.Shift` masks to `Keys.None` (`0x00040000 | 0x00010000` has no bits inside `0x0000FFFF`), so an Alt+Shift value would be claimed.
- `Keys.Alt | Keys.Control` masks to `Keys.None` likewise.

Separately, an AltGr press on an international keyboard is delivered as `Keys.Menu | Keys.Control | Keys.Alt`, whose key-code half is `Keys.Menu`, so AltGr is claimed by the `Keys.Menu` arm.

**Assessment.** In practice a `WM_SYSKEYDOWN` message always carries a virtual-key code, so the modifier-only shapes are not expected to arrive from physical input; the `Keys.None` arm exists to accept the synthetic value used in tests and mirrors the delivered #467 precedent. No evidence in this feature establishes whether the host can deliver such a value.

**Reachability: latent, and not a regression.** Every input in this class was claimed by the previous guard too, since `HasFlag(Keys.Alt)` is true for all of them. The change strictly narrows the claim set, so no input becomes newly claimed. AltGr+letter chords such as AltGr+M are in fact newly released, which is an improvement. There is no live defect here and nothing to fix on this branch. Not merge-method-dependent.

**Recommendation.** Take no action. Removing the `Keys.None` arm would break the synthetic-shape test and diverge from the twin surface for no established behavioral gain.

### CR-2 — `IsAltKeyCommand` now has zero compiled production consumers

Verified by a repository-wide search. After the guard replacement, `IsAltKeyCommand` is referenced by:

- `QuickFiler/Viewers/QfcFormViewerDark.cs:43` and `QuickFiler/Viewers/QfcFormViewerExpanded.cs:43` — both **uncompiled**. Confirmed directly against `QuickFiler/QuickFiler.csproj`: neither path appears in the `<Compile Include>` item list, while `Viewers\QfcFormViewer.cs` does appear, at line 452.
- Four `[TestMethod]` consumers in `QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs`.

So the member is now dead code in the compiled product, retained deliberately. `spec.md` non-goal 1 and AC-8 both require its retention, and the rationale is sound: the uncompiled variants dispatch through a different contract, `KeyboardHandler_KeyDown(object, KeyEventArgs)`, which branches on `e.KeyCode`, and `Keys.Left` is a registered `KeyActions` entry there. Narrowing or deleting the shared predicate would silently change that contract.

The risk `spec.md` anticipated — that removing the last compiled consumer might trip an unused-member analyzer — did not materialize. The analyzer gate produced zero console lines matching `: warning [A-Z]+[0-9]+:`, with 36 `Task "Csc"` occurrences proving compilation actually ran.

**Reachability: dead in the compiled product; latent maintenance cost only.** Not merge-method-dependent.

**Recommendation.** Take no action on this branch. If the uncompiled viewer variants are ever deleted or brought into the build, revisit `IsAltKeyCommand` at that time.

### CR-3 — The predicate takes an interface reference solely to null-test it

`ClaimsAltChord` accepts `IQfcKeyboardHandler handler` but never dereferences it; the parameter exists only for `handler is null`. A stricter reading of the pure-function guidance would pass a `bool hasHandler` and keep the predicate free of a domain type it does not use.

**Assessment.** The current signature is the better choice and should be kept. It is character-equivalent to the delivered #467 predicate, so the two twin surfaces read identically and a future reader comparing them sees no spurious difference. It also keeps the call site honest: `ClaimsAltChord(_keyboardHandler, keyData)` reads as a question about the actual handler rather than about a derived boolean the caller computed. Consistency with the delivered precedent outweighs the marginal purity gain.

**Reachability: not a defect; a style observation only.** Not merge-method-dependent.

**Recommendation.** Take no action.

### CR-4 — Retained unused locals in `ProcessCmdKey` (reported for completeness, not as a defect)

`QuickFiler/Viewers/QfcFormViewer.cs` still constructs `object sender = FromHandle(msg.HWnd)` and `var e = new KeyEventArgs(keyData)`, and still assigns `e.Handled = true`, while the dispatch on the following line calls the parameterless `_keyboardHandler.ToggleKeyboardDialogAsync()`. Neither local is read.

**This is not a finding against the change.** `spec.md` non-goal 4 requires their retention on the ground that the bugfix policy in `CLAUDE.md` mandates the minimal targeted fix and forbids opportunistic refactors, and AC-14 pins their survival via pattern VC-2, which was reproduced and returns exactly two matches. Their retention is correct and deliberate. It is recorded here only so the reader does not mistake the omission for an oversight, which the specification explicitly asks reviewers not to do.

`spec.md` already lists their removal as follow-up candidate 1. That is the right disposition and this review concurs.

**Reachability: latent, and inert.** `FromHandle` is a cheap lookup and `KeyEventArgs` is a small allocation, both on a per-keystroke path that now runs strictly less often than before. Not merge-method-dependent.

### CR-5 — Stale line-number citations in prose

`spec.md`, `plan.2026-08-31T20-16.md`, and the research artifact all cite `QuickFiler.Test/QuickFiler.Test.csproj:151` for the `QfcFormKeyHandlerTests.cs` compile entry. The entry is at line 152 at head. Independently, AC-14's criterion text cites the retained unused locals at `QuickFiler/Viewers/QfcFormViewer.cs:64-67`; post-change they sit at lines 61, 62, and 64, because the guard collapsed from four source lines to one.

Neither is load-bearing: no acceptance condition asserts a line number, and both criteria are verified by path-level diff listings and by content-matching patterns, all of which reproduce correctly.

**Reachability: latent, documentation only.** Not merge-method-dependent.

## Follow-Up Candidates

`spec.md` lists four follow-up candidates under "Rollout & Follow-up". This review has examined each and **concurs with all four as written**; none needs restating or refiling.

1. Removal of the unused locals at `QuickFiler/Viewers/QfcFormViewer.cs`. Concur — see CR-4.
2. The discarded `bool` return at `TaskVisualization/TaskViewer.cs:260` versus the consumed one at line 395. Concur; correctly scoped to the TaskVisualization project and correctly described as unresolved rather than as a known defect.
3. Adding the missing `Keys.Menu | Keys.Alt` positive case to the Email Filer suite. Concur, and this is the most valuable of the four: that arm of the delivered #467 predicate is currently deletable without failing a test, which this branch's own suite demonstrates is worth pinning.
4. Issue #713, already opened, for the single-assembly search root throwing under `Set-StrictMode`. Concur; correctly excluded from this fix and correctly promoted so the finding survives the merge.

One item this review would add to a consolidated follow-up, drawn from the policy audit rather than from the code: the PR context collection tooling classifies these three `.cs` files as documentation and reports zero core-logic changes, which causes the coverage-validation hook to compute an empty changed-language set and skip C# enforcement. That is a tooling defect outside this branch's footprint. Per the review instruction, it is reported here in prose rather than promoted.

## Verdict

**PASS. Blocking findings: 0.**

The change is minimal, correct, measured, and consistent with the delivered twin. The test suite pins both key-data shapes of the retained positive case and every negative case named in the specification, and the RED-first sequence is evidenced by verbatim failure output rather than by assertion.
