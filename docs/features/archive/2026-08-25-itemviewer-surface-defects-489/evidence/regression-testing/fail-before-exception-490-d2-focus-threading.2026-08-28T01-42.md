# Fail-before exception dossier — issue #490 D2, `FocusSearch` threading

Timestamp: 2026-08-28T01-42
Command: (no runnable gate produces the fail-before signal — see below; the alternative proof is the P9-T1 / P9-T8 count pair)
EXIT_CODE: 0
Task: [P9-T6]

## WhyFailingRunImpossible:

"`FocusSearch()` does not marshal" cannot be observed by any test this repository can run.

**The defect lives inside the concrete viewer, not on the interface.** Before P9-T2,
`QuickFiler/Viewers/ItemViewer.FolderSearch.cs:79` read
`public void FocusSearch() => TxtboxSearch.Invoke(new Action(() => TxtboxSearch.Focus()));` — a
`Control.Invoke` with no `InvokeRequired` check and no handle guard, so it threw
`InvalidOperationException` whenever the window handle did not yet exist. Every controller-side test
of this path drives a `Mock<IItemViewer>`, whose `FocusSearch()` is a generated no-op. A mock records
that the member was called; it has no body, so the marshalling decision inside the concrete
implementation is invisible to it. No `Mock<IItemViewer>`-based test can distinguish the pre-change
body from the post-change body.

**The concrete type is unconstructible in the test host.** `ItemViewer` is a `UserControl` partial
carrying `[ExcludeFromCodeCoverage]` at `ItemViewer.cs:20`, and constructing it would create live
WinForms window handles inside the MSTest host — which the repository structural guard
`ExecutingAssembly_ContainsNoFormDerivedType` and the general unit-test policy both forbid.
Instantiating it to observe the `Invoke` call is therefore not available.

**The remaining option is an IL-shape assertion, and it is inadmissible.** One could reflect over the
`ItemViewer.FocusSearch` method body and assert that no `callvirt` to `Control.Invoke` appears. That
test asserts a compilation artefact rather than a behaviour: it is brittle against any compiler or
CSharpier change, it would break on an equivalent-but-differently-emitted body, and it would pass for
a body that marshals through a different API. It buys no confidence that the contract holds and it
would fail for reasons unrelated to the defect.

No failing run is therefore possible for this item, and this dossier stands in its place as the
evidence conventions permit.

## The adopted contract

**The viewer forwards; the controller marshals.**

Every other `ItemViewer` intent member is a thin forward to its underlying control, and no other one
calls `Control.Invoke` on its own behalf. `FocusSearch` was the single exception. P9-T2 brings it
into line, rewriting `ItemViewer.FolderSearch.cs:79` to
`public void FocusSearch() => TxtboxSearch.Focus();`. Marshalling is the responsibility of the
caller, and P9-T3 records that contract as XML documentation on `IItemViewer.FocusSearch` and
`IItemViewer.FocusSubject` so it is discoverable at the interface rather than inferable only from the
implementation.

## Alternative proof

### 1. The test that must stay green

| Test | Declared at | Assertion | Baseline outcome (P0-T13) |
|---|---|---|---|
| `JumpToSearchTextbox_TogglesKeyboardAndFocusesSearch` | `QuickFiler.Test/Controllers/QfcItemController.NavigationTests.cs:185` (`[TestMethod]` at `:184`) | `viewer.Verify(v => v.FocusSearch(), Times.Once())` at `:199` | passed |

The plan prints `:184` for the declaration and `:198` for the assertion. The observed values are
`:185` and `:199`; `:184` is the `[TestMethod]` attribute line. No acceptance condition asserts
either number, and the test is located by fully-qualified name, so the one-line difference is
recorded for the audit trail only.

This test proves the caller still reaches `FocusSearch()` exactly once after the viewer body changed.
It is recorded `passed` in the `BaselineNamedPins:` block of
`FEATURE/evidence/baseline/phase0-vstest-quickfiler.2026-08-28T00-14.md`, and P9-T9 re-runs it.

### 2. The `TxtboxSearch.Invoke` count pair

The falsifiable proof that the viewer no longer marshals is the literal count of
`TxtboxSearch.Invoke` across `QuickFiler/Viewers/`, taken before and after the change:

| Gate | Task | Count | Artifact |
|---|---|---|---|
| Before | P9-T1 | **1**, at `QuickFiler/Viewers/ItemViewer.FolderSearch.cs:79` | `FEATURE/evidence/regression-testing/p9-t1-txtboxsearch-invoke-before.2026-08-28T01-37.md` |
| After | P9-T8 | **0** | `FEATURE/evidence/regression-testing/p9-t8-txtboxsearch-invoke-after.<timestamp>.md` |

This pair is falsifiable in the way an IL assertion is not: had P9-T2 not run, P9-T8 would record `1`
and fail. It is the fail-before / pass-after evidence for the AC31 zero-match assertion.

### 3. Residual — promoted as reframed finding O3

Making `FocusSearch()` a bare forward converts a throw into a **silent no-op** when the member is
called off the UI thread. The sole production caller is

```
QuickFiler/Controllers/QfcItemController.Navigation.cs:54:            _itemViewer.FocusSearch();
```

inside `JumpToSearchTextbox()` (declared at `:51`), and that file is **444-owned**: it carries no
controller-side marshal and no `InvokeRequired` guard. After this feature, an off-UI-thread
`JumpToSearchTextbox` therefore silently does nothing instead of throwing
`InvalidOperationException`.

That residual is **out of scope for #489** — `QfcItemController.Navigation.cs` is read-only for this
feature and P10-T5 asserts it is absent from the diff — and is promoted as the **reframed** finding
O3 recorded at `FEATURE/spec.md:731`. The original O3, the viewer-side unguarded `Control.Invoke`, is
resolved in scope by P9-T2 and must not be promoted as written; what is promoted is the caller-side
guard on `Navigation.cs:54`, owned by 444.

## Required-element checklist (P9-T6 acceptance)

| Required element | Present |
|---|---|
| "Does not marshal" is unobservable through `Mock<IItemViewer>` because the defect lives inside the concrete, unconstructible viewer | Yes — WhyFailingRunImpossible: paragraphs 1 and 2 |
| An IL-shape assertion would be brittle | Yes — WhyFailingRunImpossible: paragraph 3 |
| The adopted contract, the viewer forwards and the controller marshals, is stated | Yes — § The adopted contract |
| `JumpToSearchTextbox_TogglesKeyboardAndFocusesSearch` named as the test that must stay green, with its declaration and its `FocusSearch` assertion | Yes — alternative proof section 1 |
| The P9-T1 and P9-T8 `TxtboxSearch.Invoke` counts cited as the alternative proof | Yes — alternative proof section 2 |
| The `QfcItemController.Navigation.cs:54` residual recorded and promoted as reframed finding O3 | Yes — alternative proof section 3 |

Output Summary: Fail-before exception dossier for #490 D2. A failing run is impossible because the
defect is confined to the concrete `ItemViewer`, which is unconstructible in the MSTest host, and is
invisible through the `Mock<IItemViewer>` every controller test uses; the only remaining form, an
IL-shape assertion, is brittle and asserts a compilation artefact rather than a behaviour. The
adopted contract is that the viewer forwards and the controller marshals. The alternative proof is
the already-green `JumpToSearchTextbox_TogglesKeyboardAndFocusesSearch`
(`QfcItemController.NavigationTests.cs:185`, assertion at `:199`, `passed` at P0-T13 baseline) plus
the falsifiable P9-T1 = 1 / P9-T8 = 0 `TxtboxSearch.Invoke` count pair. The recorded residual is the
444-owned `QfcItemController.Navigation.cs:54` caller with no guard, which now silently no-ops
off-thread; it is promoted as reframed finding O3 at `FEATURE/spec.md:731`. All five elements P9-T6
requires are present.
