---
name: ribbon-engine-toggle-defects-735
description: "#735 research (2026-09-02): RibbonExplorer.xml IS CSharpier-formatted; 84 distinct callback names / 5 dead; ManagerAsyncLazy is test-constructible; coordinator test file at 459 lines forces a partial split"
metadata:
  type: project
---

Research for issue #735 (three consolidated `TaskMaster/Ribbon/` defects) completed 2026-09-02.
Research file: `docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/research/2026-09-02T09-15-ribbon-engine-toggle-defects-research.md`

**Why:** several findings are non-obvious and cost real verification time; they generalize past #735.

**How to apply:** when touching the ribbon layer or writing XML/reflection tests in this repo.

- **`TaskMaster/Ribbon/RibbonExplorer.xml` is formatted by CSharpier.** `.csharpierignore` only
  excludes `**/evidence/**`, coverage/TRX artifacts and `*.csproj|*.props|*.targets`. That is why the
  XML is attribute-per-line wrapped. Any XML edit must be followed by `csharpier format .` and the
  reflow accepted. Do not hand-format it.
- **Callback-attribute rule for CustomUI:** an attribute is a callback iff its local name is
  `onAction`, `onChange`, `onLoad`, or begins with `get`. A generic `\s(get[A-Z]|on[A-Z])[A-Za-z]*=`
  scan of the document found exactly 7 families and 106 occurrences (98 live, 8 inside XML comments,
  84 distinct names). Enumerating `document.Descendants()` excludes commented-out controls for free
  because `XComment` nodes carry no attributes — no regex needed.
- **`ManagerAsyncLazy` is cheaply test-constructible**: `new ManagerAsyncLazy(new Mock<IApplicationGlobals>().Object)`
  then `mockAf.Setup(a => a.Manager).Returns(manager)`. Its ctor only assigns an `AsyncLazy` and
  never runs the factory. Proven at `UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/Triage_Tests.ManagerAndAdditional.cs:23-25`.
  So a guard seam over `Func<IAppAutoFileObjects>` is fully mockable; do NOT take a real
  `ApplicationGlobals` (every existing test reaches into it via `BindingFlags.NonPublic` reflection).
- **`AppAutoFileObjects.Manager` is a plain auto-property assigned only in `LoadParallelAsync`/
  `LoadSequentialAsync`** — genuinely null pre-load, so a `Manager` null check is load-bearing, not
  defensive noise.
- **`TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` is 459 lines** (500-line ceiling),
  so new tests must go in a second partial file. `RibbonControllerTests.cs` / `.Engines.cs` is the
  in-directory precedent for `public partial class` test splitting.
- **Both `TaskMaster.csproj` and `TaskMaster.Test.csproj` are legacy non-SDK** — every new `.cs`
  file needs an explicit `<Compile Include>`, which forces a csproj edit outside any
  `TaskMaster/Ribbon/`-only scope fence. Flag it as a scope note rather than omitting it.
- Issue #735 cites `ClearSpamManagerAsync` at "216-231"; the actual method is
  `RibbonController.Intelligence.cs:206-233`.
- `ClearSpam` is deliberately NOT in `EngineCommandCatalog` (no `getEnabled` in the XML) — its real
  dependency is `AF.Manager`, not `InboxEngines` readiness, so it cannot be routed through
  `EngineGatedCommandRunner`.

Related: [[ribbon-engine-readiness-503]], [[ribbon-toggle-state-guards-505]]
