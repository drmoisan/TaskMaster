# D5 — Research §3.5 Open Item: Is a Faulted `InitializeWebViewAsync` Task Observed? ([P5-T6])

Timestamp: 2026-08-28T05-55

Command: `git grep -n 'InitializeWebViewAsync' -- '*.cs'` and
`git grep -n 'EnsureBreadcrumbPipeline' -- 'QuickFiler/*.cs'`, followed by source reads of each call
site. `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` was **read only and not edited**; it is
a forbidden file under constraint C1, owned by sibling feature `qfc-item-controller-defects-484`.
EXIT_CODE: 0

## The throw path being evaluated

`EnsureBreadcrumbPipeline()` is called from `QfcItemController.ViewerSetup.cs:112`, which sits inside
`internal async Task InitializeWebViewAsync()` declared at `ViewerSetup.cs:48`. D5's
`ObjectDisposedException` therefore propagates out of `EnsureBreadcrumbResourceOwnership` →
`EnsureBreadcrumbLifecycle` → `InitializeBreadcrumbPipeline` → `EnsureBreadcrumbPipeline` →
`InitializeWebViewAsync`, faulting that method's returned `Task`.

The question research §3.5 left open is whether that faulted task is observed by its caller.

## Every in-repo caller, with its observation status

### `QfcItemController.InitializeWebViewAsync` — four call sites, all in `QfcItemController.Initialization.cs`

| Call site | Enclosing member | Form | Observed? |
| --- | --- | --- | --- |
| `Initialization.cs:192` | the fire-and-forget initialization tail, commented "Fire and forget WebView initialization" at `:191` | `_ = _itemViewer.UiDispatcher.InvokeAsync(InitializeWebViewAsync);` | **NO** — the `DispatcherOperation` is discarded by `_ =`, and the inner task is wrapped inside it |
| `Initialization.cs:256` | the sequential async initialization path | `await InitializeWebViewAsync();` | **yes** — awaited into the enclosing async method's own task |
| `Initialization.cs:288` | an async initialization overload | `_ = InitializeWebViewAsync();` | **NO** — discarded |
| `Initialization.cs:324` | a second async initialization overload | `_ = InitializeWebViewAsync();` | **NO** — discarded |

One further reference at `Initialization.cs:345` is commented out and is not a call site.

### `EfcItemController.InitializeWebViewAsync` — a distinct same-named method

| Call site | Form | Observed? |
| --- | --- | --- |
| `EfcItemController.cs:97` | `Task.Run(() => InitializeWebViewAsync());` | **NO** — the `Task` returned by `Task.Run` is not assigned or awaited |
| `EfcItemController.cs:153` | `Task.Run(() => InitializeWebViewAsync());` | **NO** — same |

These target `EfcItemController.InitializeWebViewAsync` declared at `EfcItemController.cs:174`, not the
`QfcItemController` method D5's throw travels through. They are recorded for completeness because they
exhibit the same discard pattern.

## OVERALL CONCLUSION — the task is NOT observed

**Three of the four `QfcItemController.InitializeWebViewAsync` call sites discard the returned task.**
Only `Initialization.cs:256` awaits it. A fault raised on any of the three discarding paths therefore
becomes an unobserved task exception. On .NET Framework 4.5 and later an unobserved task exception no
longer terminates the process by default, so it is finalized away with no log entry, no diagnostic, and
no observable effect.

The condition research §3.5 set — "confirm that fault is observed by the caller and does not become an
unobserved `TaskException`" — is **not** satisfied.

## The D5 guard is NOT weakened in response

Research §3.5 is explicit that if the task proves unobserved, "the correct response is a new issue
against `ViewerSetup.cs` (484-owned), **not** a weakening of this guard." The delivered D5 guard is
unchanged and unweakened: `EnsureBreadcrumbResourceOwnership` throws `ObjectDisposedException` when the
viewer reports `IsDisposed` or `Disposing`, before any container is created and before any
`BreadcrumbResourceOwner` is added. No silent early return was substituted, no severity was reduced,
and `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` was not edited.

## Follow-up potential entry — CREATED, THEN PROMOTED

Created by the executor at
`docs/features/potential/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved.md`, and moved by the
promotion tooling to its promoted location
`docs/features/potential/promoted/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved.md`.

It is filed against `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`, names the mechanism (the
returned `Task` is discarded at three of four call sites), names the trigger (a fault inside
`InitializeWebViewAsync`, which #488's D5 fix newly makes reachable via `ObjectDisposedException`), and
carries the standard potential-entry front matter and section headings the promotion tooling maps into
the GitHub bug issue template.

## Promotion to a GitHub issue — BLOCKED IN THE EXECUTOR, THEN COMPLETED BY THE ORCHESTRATOR

**Outcome: GitHub issue [#670](https://github.com/drmoisan/TaskMaster/issues/670) is OPEN.** The record
of the executor's blocker is retained below rather than deleted, because it documents a real tool-set
boundary that a future executor on this repository will hit again.

The repository enforces an MCP-only promotion path. `.claude/hooks/enforce-promotion-mcp-only.ps1` is a
`PreToolUse` hook that blocks, by its own documentation:

```
    Forbidden command tokens (legacy promotion-script bypass):
      - new-potential-entry.ps1
      - new_potential_bug_entry
      - potential_to_issue
      - new_active_feature_folder

    Forbidden gh-CLI patterns (raw GitHub issue creation bypass):
      - gh issue create (with any flag suffix)
      - gh issue new
      - gh api against repos/<owner>/<repo>/issues with explicit POST method
        (-X POST or --method POST)
```

with the reason string:

```
PROMOTION_MCP_ONLY_BLOCKED: Direct GitHub issue creation via `gh` bypasses the approved drm-copilot MCP
promotion path (`mcp__drm-copilot__new_potential_entry` -> `mcp__drm-copilot__potential_to_issue` ->
`mcp__drm-copilot__new_active_feature_folder`). Use those MCP tools instead.
```

**The three named MCP promotion tools are not available in this executor's tool set.** The only
`drm-copilot` MCP tools exposed to this session are `run_poshqc_format`, `run_poshqc_analyze`,
`run_poshqc_test`, and `run_poshqc_analyze_autofix`. `gh` is installed (version 2.87.3) and
authenticated as `drmoisan` with `repo` scope, so the blocker is the approved-path policy, not
credentials.

The approved path was therefore not available and the forbidden path was **not** used. No attempt was
made to reword a `gh` invocation to evade the hook. This is reported to the caller, who holds the MCP
promotion tools.

### Resolution by the orchestrator

Timestamp: 2026-08-28T06-40

The orchestrator holds the promotion tool set and completed the approved path. It did not use `gh issue
create` and did not reword anything to evade the hook.

Command: `mcp__drm-copilot__potential_to_issue` with
`potential_path=<worktree>/docs/features/potential/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved.md`,
`promotion_type=bug`, `work_mode=full-bug`

EXIT_CODE: 0

Receipt:

| Field | Value |
| --- | --- |
| Issue | **#670** |
| URL | https://github.com/drmoisan/TaskMaster/issues/670 |
| Title | Bug: qfc-initializewebviewasync-fault-is-unobserved |
| State | OPEN (verified with `gh issue view 670 --json number,title,state,url`) |
| Target repository | `drmoisan/TaskMaster` |
| Promoted record | `docs/features/potential/promoted/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved.md` (verified present on disk) |

Two details worth recording for a future run. First, `potential_to_issue` rejected the
workspace-relative `potential_path` and required an absolute path. Second, it **moved** the source out
of `docs/features/potential/` rather than copying it, which is the documented behaviour for a source
resolved directly from `docs/features/potential/`; the promoted record was confirmed present
afterwards.

No active feature folder was created for #670. The acceptance criterion requires an issue to be opened
and referenced, not a delivery workflow, and #670 is filed against a `qfc-item-controller-defects-484`
owned file that this feature must not edit. Creating an active folder would have left a dangling
delivery scaffold for work nobody is executing.

### Consequence for the acceptance criterion

The criterion `[P5-T11]` flips reads, in part: "If it is not observed, a new issue is opened against
`QfcItemController.ViewerSetup.cs` (484-owned) and referenced here". The task **is** not observed, so
that clause is live, and it names an **issue** specifically — unlike the three-follow-up criterion
`[P7-T14]` flips, which accepts "a potential entry or GitHub issue". A potential entry alone does not
satisfy it.

All three clauses are now delivered:

1. **The open item is discharged with recorded evidence.** The task is not observed; the per-call-site
   table above is the evidence.
2. **A new issue is opened against `QfcItemController.ViewerSetup.cs` and referenced here.** Issue
   **#670**, OPEN, referenced by number and URL in this artifact and in the D5 section of `spec.md`.
3. **The guard is not weakened in response.** `EnsureBreadcrumbResourceOwnership` throws
   `ObjectDisposedException` as its first action, unchanged from the delivered D5 design.

The criterion is therefore checked `- [x]`. `[P9-T15]`'s reconciliation moves from
remediation-required to pass, with 54 of 54 criteria delivered.

Output Summary: A faulted `QfcItemController.InitializeWebViewAsync` task is **NOT observed** — three
of its four production call sites discard it (`Initialization.cs:192`, `:288`, `:324`), only `:256`
awaits it, and the two `EfcItemController` sites discard theirs as well. The D5 guard is **not**
weakened in response. The follow-up potential entry was created by the executor and promoted through the
approved MCP path by the orchestrator to GitHub issue **#670**
(https://github.com/drmoisan/TaskMaster/issues/670, OPEN), whose promoted record now lives at
`docs/features/potential/promoted/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved.md`.
Promotion was initially blocked inside the executor because `enforce-promotion-mcp-only.ps1` forbids
`gh issue create` and the three required MCP promotion tools are absent from the executor's tool set;
the forbidden path was not used and nothing was reworded to evade the hook. All three clauses of
`[P5-T11]`'s criterion are delivered and the criterion is checked.
