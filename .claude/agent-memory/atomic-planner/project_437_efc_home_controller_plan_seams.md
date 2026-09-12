---
name: project-437-efc-home-controller-plan-seams
description: F8 (#437) EfcHomeController coverage — existing EFC test files sit at 459-476 lines, so new tests need new files; MoveFailureMessageAction defaults to MessageBox.Show; ClassLevel parallelism makes Production* statics a live hazard
metadata:
  type: project
---

Epic #136 child F8 (`quickfiler-efc-home-controller-coverage`, issue #437) planning facts that are
expensive to rediscover.

**Why:** the six F8 production files were already above the 80% line floor at baseline, so the plan
is gap closure plus invariant pinning. The binding constraints turned out to be test-side, not
production-side.

**How to apply:** when planning or executing any further work in the `EfcHomeController*` family:

- Three existing test classes have almost no 500-line headroom and must NOT absorb new tests:
  `QuickFiler.Test/Controllers/EfcHomeControllerDependenciesTests.cs` (476),
  `EfcHomeControllerDependenciesProductionFactoryTests.cs` (473),
  `EfcHomeControllerLifecycleTests.cs` (459). Route new cases to new files.
  Files with real headroom: `EfcHomeControllerTests.cs` (219), `EfcHomeControllerMetricsTests.cs`
  (244), `EfcHomeControllerSeamTests.cs` (291), `EfcHomeControllerExecuteMovesTests.cs` (340).
- `EfcHomeController.ExecuteMoves.cs` L22-23 declares
  `MoveFailureMessageAction { get; set; } = text => MessageBox.Show(text)`. Any test reaching
  `result == false` without overriding that seam raises a modal popup and hangs CI. Put the override
  requirement in the individual task text, not just a plan preamble.
- `scripts/vscode/TaskMaster.cli.runsettings` runs test **classes** in parallel
  (`<Scope>ClassLevel</Scope>`, `<Workers>0</Workers>`). There are **two** independent unsynchronized
  process-global mutation surfaces, each with an existing unmarked mutator class:
  (a) the 16 `Production*` statics in `EfcHomeControllerDependencyFactories.cs`, mutated by
  `EfcHomeControllerDependenciesTestsProductionFactory`
  (`EfcHomeControllerDependenciesProductionFactoryTests.cs` class decl L17); and
  (b) `EfcHomeController._defaultDependenciesFactory`, mutated by `EfcHomeControllerLifecycleTests`
  (class decl L20) via `SetDefaultDependenciesFactory` at L48 and L82.
  Both already have a restoring `[TestCleanup]` but neither carries `[DoNotParallelize]`.
  **Rule:** marking only the *new* mutating class does not isolate anything — an unmarked mutator
  still runs in the parallel bucket alongside a `[DoNotParallelize]` class. Every class touching the
  static must be marked, so a plan that adds a mutator must also add a retrofit task for each
  pre-existing mutator of the same static.
- Three of the 16 `Production*` defaults are **lambdas, not named methods**:
  `ProductionFormControllerWithDataInitializer` (L80), `ProductionFormControllerWithoutDataInitializer`
  (L92), `ProductionDataFieldsInitializer` (L105), re-assigned as lambdas at L120 and L124-128. Their
  `Method.Name` is a compiler-generated `<...>b__N_M` that is unstable across compiler versions. See
  [[never-assert-method-name-on-lambda-valued-delegate]].
- `EfcHomeController.cs` installed two distinct default-dependency lambdas with identical bodies
  (field initializer L24-25 and a separate lambda in `ResetDefaultDependenciesFactory` L37), which
  made that file's per-file coverage order-dependent. Consolidating to one `static readonly` default
  is AC8 and is acceptance-relevant because the coverage number is the acceptance evidence.
- `EfcHomeController.Timing.cs` reads no clock at all — it is four diagnostic-logging helpers. Its
  only failing floor is branch coverage (66.67% vs the 75% floor). An injected clock is inapplicable.

See also [[project_csharp_phase0_toolchain_bootstrap]],
[[project_legacy_csproj_explicit_compile_include]],
[[reference_invoke_mstest_with_coverage_script]].
