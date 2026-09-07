# Out-of-Scope Follow-Up Promotions ([P7-T5])

Timestamp: 2026-08-28T06-15

Command: created three potential entries under `docs/features/potential/`, then
`git diff --name-only 12465043e052fce66a1861bf1ddd037a1aa81afc -- QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs`
and a filtered diff of `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` for the members
the three candidates name.
EXIT_CODE: 0

## The three entries created by this task

Each names its mechanism, its trigger, and its owning file or files, and carries the standard
potential-entry front matter and section headings the promotion tooling maps into the GitHub bug issue
template.

### 1. D1c — the generation guard drops the incoming host without disposing it

`docs/features/potential/2026-08-28-configurehost-generation-guard-drops-incoming-host.md`

- **Mechanism.** `BreadcrumbItemViewerLifecycleCoordinator.ConfigureHost`'s posted lambda opens with a
  generation guard. When `_generation` advanced between the schedule and the run, the lambda returns
  early and the **incoming** host — already constructed by the caller on the assumption the coordinator
  would take ownership — is dropped undisposed, leaking a `ToolStripDropDown` and potentially a
  WebView2-backed surface.
- **Triggers.** Two paths advance the generation: `Reset()`, reached from `QfcItemController.Cleanup()`
  through the viewer-setup teardown path, and `Dispose()`.
- **Owning file.** `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs`.
- **Why out of scope for #488.** It leaks the **incoming** host, a different defect from the outgoing-host
  disposal ordering that defect D1 filed; and adding a `Dispose()` to a branch no current test exercises
  would be an unpinned behaviour change inside a bugfix change-set.

### 2. `SetBridgeCoordinator` replaces without disposing while `Dispose()` disposes

`docs/features/potential/2026-08-28-setbridgecoordinator-replaces-without-disposing.md`

- **Mechanism.** On replacement, `SetBridgeCoordinator` calls `UnsubscribeBridge()` — which detaches
  four event handlers and disposes nothing — then overwrites the field. `Dispose()` by contrast calls
  `_bridgeCoordinator?.Dispose()`. The type owns the bridge coordinator at teardown but not at
  replacement, so a replacement leaks the outgoing instance's `BreadcrumbMessengerHub` and its four
  subscriptions.
- **Trigger.** Any call installing a genuinely different bridge coordinator, which the method's
  reference-equality guard currently prevents.
- **Owning file.** `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs`.
- **Why out of scope for #488, and the contingency carried forward.** The path is dormant **because**
  D3 was implemented as fail-fast: `InitializeBreadcrumbPipeline` never constructs a second
  `BreadcrumbBridgeCoordinator`, so nothing new reaches the replacement branch. The entry records
  explicitly that if D3 were ever amended to adopt explicit re-initialization, this defect becomes live
  and must be pulled into scope in the same change-set.

### 3. `Reset()` detaches the two surfaces with different synchrony

`docs/features/potential/2026-08-28-reset-detaches-collapsed-and-popup-surfaces-with-different-synchrony.md`

- **Mechanism.** `Reset()` detaches the collapsed surface synchronously via `DetachCollapsedMessenger()`,
  but the popup surface only through the posted lambda opened by
  `BreadcrumbDropDownOpenCoordinator.Reset()`. On any non-inline context the two detaches land at
  different times, and a caller treating `Reset()` as complete on return is right about one surface and
  wrong about the other.
- **Trigger.** Any `Reset()` issued from off the UI boundary, where posts do not run inline.
- **Owning files — the entry names BOTH**, as this task requires:
  `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` (the synchronous half) and
  `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` (the asynchronous half).
- **Why out of scope for #488.** The asynchronous half lives in a file owned by sibling feature
  `breadcrumb-coordinator-hub-defects-501` for issue #462. Changing only the collapsed half does not fix
  a synchrony mismatch.

## No fix for any of the three appears in this feature's diff

| Check | Result |
| --- | --- |
| `git diff --name-only <BASE_SHA> -- QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | **no output lines** — both untouched |
| Added or removed lines in `BreadcrumbItemViewerLifecycleCoordinator.cs` matching `SetBridgeCoordinator`, `UnsubscribeBridge`, `Reset()`, `IsCurrent`, or `_generation` | **0** |

The only change this feature makes to `BreadcrumbItemViewerLifecycleCoordinator.cs` is D2's
retained-theme replay, whose sixteen added lines `[P3-T5]` quotes in full. None of them touches the
generation guard's early-return branch, `SetBridgeCoordinator`, `UnsubscribeBridge`, or `Reset()`.

## The fourth candidate — carried forward from [P5-T6], NOT duplicated

`[P5-T6]` concluded that a faulted `QfcItemController.InitializeWebViewAsync` task **is not observed**:
three of its four production call sites discard it. That triggered the fourth follow-up, which
`[P5-T6]` created rather than this task:

- **Path:** `docs/features/potential/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved.md`
- **GitHub issue number:** **none — not created**
- **GitHub issue URL:** **none — not created**

**No duplicate entry and no second issue were created by this task.** The path above is carried forward
verbatim from `[P5-T6]`'s artifact, as instructed.

The issue number and URL cannot be recorded because promotion is blocked. `[P5-T6]`'s artifact records
the blocker in full: `.claude/hooks/enforce-promotion-mcp-only.ps1` forbids `gh issue create`, `gh issue
new`, and a POST to the issues API, and requires the `mcp__drm-copilot__new_potential_entry` →
`potential_to_issue` → `new_active_feature_folder` MCP path, none of which is available in this
executor's tool set. The forbidden path was not used and no wording was altered to evade the hook.

**This does not block the criterion `[P7-T14]` flips.** That criterion accepts "a potential entry **or**
GitHub issue" for the three candidates this task creates, and all three exist as potential entries. It
does block the criterion `[P5-T11]` flips, which names an issue specifically for the fourth; that
criterion is left unchecked and reported.

Output Summary: The **three** out-of-scope follow-up candidates — D1c, `SetBridgeCoordinator`
replace-without-dispose, and the `Reset()` synchrony mismatch — are each recorded as a potential entry
under `docs/features/potential/`, each naming its mechanism, trigger, and owning file, with the third
naming both of its files. **No fix for any of the three appears in this feature's diff**: the two
501-owned files are untouched, and the coordinator diff contains zero lines matching the members the
candidates name. The fourth entry, created by `[P5-T6]`, is carried forward by path; its issue number
and URL are **not** recorded because MCP-only promotion is unavailable, and no duplicate entry or second
issue was created.
