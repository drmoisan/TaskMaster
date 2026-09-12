---
name: efc-home-controller-deps-437
description: "Issue #437 (epic #136 F8): EfcHomeControllerDependency* files are already ~86-93% covered; F9 consumes nothing from them; Production* statics vs ClassLevel parallelism hazard"
metadata:
  type: project
---

Findings from the 2026-08-07 per-file coverage research on QuickFiler's `EfcHomeControllerDependencies.cs`
(428 lines) and `EfcHomeControllerDependencyFactories.cs` (268 lines), epic #136 child F8 / issue #437.

1. **The "no dedicated test file" premise in spec.md was wrong in effect.** The class
   `EfcHomeControllerDependenciesTestsProductionFactory` lives in the file
   `EfcHomeControllerDependenciesProductionFactoryTests.cs` — the class name does not match the file
   name, so name-based searches miss the association. All four of its tests target the
   `...DependencyFactories.cs` partial. 26 of 28 members are already exercised. Estimated coverage
   ~86-91% (Factories) and ~90-93% (Dependencies) — both likely already over the 80% floor.

2. **The cross-child dependency direction is the reverse of what the spec implies.**
   `EfcFormController.cs` and `EfcItemController.cs` (sibling child F9) reference
   `EfcHomeControllerDependencies` **zero** times. F8's files consume F9's surface (two
   `EfcFormController` ctors plus `Initialize()` / `InitializeWithoutData()` / `InitializeDataFields()`),
   not the other way round. Consequence: all gap-closure is test-only and trivially additive.

3. **Irreducible residual (CCN-1):** the five `controller => controller.Initialize()`-style closure
   bodies in the Factories file cannot be executed without a live EfcFormController
   (`LoadUserSettings()` reads disk-backed `Settings.Default` then dereferences designer controls).
   Recommended disposition: leave uncovered, record in spec.md, do not edit F9.

**Why:** issue #136 mandates per-file research and forbids duplicating existing tests; sizing this
child as "rescue work" would have produced redundant tests.

**How to apply:** when planning or reviewing any F8/F9 work, verify the actual test class names
before assuming a production file is untested, and treat any proposed edit to
`EfcFormController.cs` / `EfcItemController.cs` from F8 as out of scope. See also
[[qfc-item-controller-227-r2-denial]] and
[[feedback-exemption-audit-check-proven-techniques]].

**Latent hazard worth fixing:** `scripts\vscode\TaskMaster.cli.runsettings` sets MSTest
`<Scope>ClassLevel</Scope>` with `<Workers>0</Workers>`, so test classes run in parallel. The 16
`EfcHomeControllerDependencies.Production*` statics are unsynchronized process-global state and
`EfcHomeControllerDependenciesTestsProductionFactory` is not `[DoNotParallelize]`. It is safe today
only because it is the sole mutator; adding a second mutating class makes the flakiness live.
Precedent for the fix: `QuickFiler.Test\Helper Classes\ViewerQueueStaticWrapperTests.cs`.
