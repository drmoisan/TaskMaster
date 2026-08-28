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

## Follow-up potential entry — CREATED

`docs/features/potential/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved.md`

It is filed against `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`, names the mechanism (the
returned `Task` is discarded at three of four call sites), names the trigger (a fault inside
`InitializeWebViewAsync`, which #488's D5 fix newly makes reachable via `ObjectDisposedException`), and
carries the standard potential-entry front matter and section headings the promotion tooling maps into
the GitHub bug issue template.

## Promotion to a GitHub issue — BLOCKED

**No GitHub issue was opened, and the criterion `[P5-T11]` addresses is therefore left unchecked.**

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

### Consequence for the acceptance criterion

The criterion `[P5-T11]` flips reads, in part: "If it is not observed, a new issue is opened against
`QfcItemController.ViewerSetup.cs` (484-owned) and referenced here". The task **is** not observed, so
that clause is live, and it names an **issue** specifically — unlike the three-follow-up criterion
`[P7-T14]` flips, which accepts "a potential entry or GitHub issue". A potential entry alone does not
satisfy it.

Half the criterion is delivered: the open item is discharged with recorded evidence, and the guard is
not weakened. The issue half is not. The criterion is left `- [ ]` rather than checked, and the gap is
carried into `[P9-T15]`'s reconciliation as remediation-required.

Output Summary: A faulted `QfcItemController.InitializeWebViewAsync` task is **NOT observed** — three
of its four production call sites discard it (`Initialization.cs:192`, `:288`, `:324`), only `:256`
awaits it, and the two `EfcItemController` sites discard theirs as well. The D5 guard is **not**
weakened in response. The follow-up potential entry
`docs/features/potential/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved.md` was created, but
**promotion to a GitHub issue is blocked**: the repository's `enforce-promotion-mcp-only.ps1` hook
forbids `gh issue create` and requires MCP promotion tools that are not in this executor's tool set. No
issue number or URL can be recorded, so `[P5-T11]`'s criterion is left unchecked.
