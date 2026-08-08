---
name: project-430-quickfiler-keyboard-plan-seams
description: "#430 (epic #136 child F3) planning seams — K1 mandatory via InternalsVisibleTo gap, R2 Option-A rationale, amended AC9 two-csproj allowance, ItemViewer sync-context construction trap"
metadata:
  type: project
---

Planning facts for `quickfiler-keyboard-actions-coverage` (#430, child F3 of epic #136), captured 2026-08-07 while writing `docs/features/active/2026-08-07-quickfiler-keyboard-actions-coverage-430/plan.2026-08-07T20-41.md` (13 phases, 234 tasks).

**Why:** Epic #136 mandates one production file per plan phase and one atomic task per test case, so an F3 revision pass must re-derive the same seam decisions rather than re-reading eleven research artifacts.

**How to apply:**

- **K1 (`IQfcDialogPrompt` + `MyBoxDialogPrompt`) is mandatory, not a style choice.** `UtilitiesCS/Properties/AssemblyInfo.cs:18-20` grants `InternalsVisibleTo` to `DynamicProxyGenAssembly2`, `UtilitiesCS.Test`, and `ToDoModel.Test` only — **not** `QuickFiler.Test`. The existing `MyBox.DialogInvoker` seam is therefore unreachable from QuickFiler tests, and any test touching `KeyboardHandler.cs:304` or `:350` would show a human-interactive modal. Adding `InternalsVisibleTo("QuickFiler.Test")` is prohibited (shared, non-F3 file). Check the `InternalsVisibleTo` list before ever declaring an existing seam "already available".
- **R2 disposition decided as Option A (separate `MyBoxDialogPrompt.cs`), on a rationale that does not depend on F1.** Option B (in-file `private static readonly Func<...>` default) would put the uncoverable forwarding statement *inside* the 456-line `KeyboardHandler.cs` that AC1/AC2 gate at `>= 80%`. Isolating it in a one-statement adapter keeps the gated file's denominator clean. The `>= 90%` new-code floor on the adapter is handled by a ledger-ratification **request** to the epic orchestrator, never self-granted.
- **`QuickFiler/QuickFiler.csproj` must be edited even though AC9 lists only `QuickFiler.Test.csproj` as permitted.** The legacy non-SDK project has no globbing, so the two new F3-authored production files need explicit `<Compile Include>` entries adjacent to the `Interfaces\` block at lines 358-368. Record this as an explicit Decisions-Record reconciliation or a reviewer will read it as an AC9 violation.
- **Four zero-case interface files still need a phase each** (`IKbdAction.cs`, `IQfcKeyboardHandler.cs`, `IMailItemActions.cs`, `IItemControler.cs`): zero-executable-IL verification, F1-ledger classification citation, byte-identical verification, and an `N/A` per-file evidence entry. F16's "all 121 files accounted for" check depends on each being explicitly dispositioned; `N/A` never `0%`.
- **AC11 is a negative gate.** `KaStringAsync` contains no async/await/timer/clock at all — the `Async` suffix names only the stored delegate type. Do not plan a timer seam; plan a verification task instead. See [[research-claims-as-acceptance-clauses]].
- **`QuickFiler.ItemViewer` cannot be constructed with a null ambient `SynchronizationContext`.** Its constructor calls `TaskScheduler.FromCurrentSynchronizationContext()`. Any plan task saying "headless `new ItemViewer()`" must also name the enclosing inline-sync-context scope, or the executor writes a test that throws at Arrange. Precedent: `QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs:353-364` (`ViewerScope` installs an `InlineSynchronizationContext` before constructing, restores on `Dispose`). This bit #430 in three tasks (P1-T79, P1-T89, P1-T92).
- **AC9 was amended mid-cycle to permit exactly two `<Compile Include>`-only `.csproj` edits** (`QuickFiler.Test.csproj` for test files, `QuickFiler.csproj` for the two F3 production files). A Decisions Record that says a csproj is "edited exactly N times" collides with any contingency-split task that adds another hunk — bound the decision by *file count*, not hunk count.

Related: [[plan-validator-phase-heading-constraint]], [[plan-validator-task-id-sequential-constraint]], [[project_legacy_csproj_explicit_compile_include]], [[reference-vstest-scoped-run-command]].
