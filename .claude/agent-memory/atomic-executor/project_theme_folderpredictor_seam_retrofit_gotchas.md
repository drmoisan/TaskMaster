---
name: project-theme-folderpredictor-seam-retrofit-gotchas
description: TaskMaster #227 cycle-3 — gotchas retrofitting IUiDispatcher into Theme and a factory-delegate seam onto FolderPredictor, plus a coverage-XML aggregation snippet
metadata:
  type: project
---

From issue #227 cycle-3 (QfcItemController residual-exemption reduction, 41→24), retrofitting new
seams onto pre-existing handle-less test-double helpers surfaced two non-obvious traps.

**Why:** both traps look like they should be safe (the code compiles, the specific new test passes)
but silently break *other, unrelated, pre-existing* passing tests — only caught by running the full
suite, not just the new tests.

**How to apply:**
- **Shared test-double builder + new required field = regression risk.** When a plan adds a new
  instance field to a class (e.g. `Theme._uiDispatcher`) that is read unconditionally by an existing
  method (`SetQfcTheme(bool async)`'s `async:true` branch), and a *shared* test helper elsewhere builds
  that class via a constructor that doesn't populate the new field (e.g. `Theme()` parameterless ctor
  used by `QfcItemControllerTestSupport.BuildColorTheme`), every *other* test that transitively uses
  that shared helper and exercises the now-dependent code path breaks with NRE — even tests unrelated
  to the current plan task. Fix: inject a **non-executing** double (`Mock<IUiDispatcher>` whose
  `InvokeAsync`/`BeginInvoke` returns `Task.CompletedTask`/`Mock.Of<IAsyncResult>()` without a
  `.Callback` that runs the delegate) into the shared builder itself, preserving the pre-retrofit
  "posted but never actually executed" observable behavior the old tests asserted against — do NOT use
  a synchronously-executing dispatcher (`BuildSyncDispatcher()`) here, since that WOULD run the
  delegate body against still-handle-less controls and NRE differently.
- **`FolderPredictor.InitAsync(_, FolderPredictor.InitOptions.FromField)` is COM-bound; `FromArrayOrString` is not.** `FromField` → `InitializeFromEmail` → `FromFolderKey(MailItemHelper)` → `FolderScorer.LoadFromField` → `AddConversationBasedSuggestions(mailInfo.Item, ...)`/`AddOlFolderKeys` — touches `Suggestions` (null unless built via the 2-arg/3-arg ctor, not the 1-arg `FolderPredictor(Outlook.Application)` ctor used by the existing `BuildFolderHandlerWithArray` test helper) and a real `MailItem.UserProperties`/`.Recipients` COM chain. `FromArrayOrString` → `FromArrayOrString(obj)` only sets `_folderList` in-memory — zero COM/Suggestions access. When a test needs to call `LoadFolderHandlerAsync`/`PopulateFolderComboBoxAsync` end-to-end against a factory-returned real `FolderPredictor` double (not a full mock), pass a non-null `varList` (routes `FromArrayOrString`) rather than relying on the default `FromField` path, or the double's `InitAsync` call throws NRE.
- **Coverage denominator per class via `Microsoft.CodeCoverage.Console` XML:** `awk` filtering `<module name="X.dll">...</module>` block boundaries plus `type_name="ClassName` substring match on `<function ...>` lines, summing `lines_covered`/`lines_partially_covered`/`lines_not_covered` attributes, gives an affected-denominator percentage matching (within rounding) the plan's coverage targets — reuse the exact awk one-liner in [[project_build_test_env]] rather than re-deriving it each cycle. Per-method breakdowns (e.g. confirming one specific changed line inside a multi-branch method is covered) require grepping the individual `<function name="MethodName(...)" ...>` line rather than the class aggregate, since branch-level (not just class-level) evidence is often required by the plan's `Output Summary:` acceptance criteria.
