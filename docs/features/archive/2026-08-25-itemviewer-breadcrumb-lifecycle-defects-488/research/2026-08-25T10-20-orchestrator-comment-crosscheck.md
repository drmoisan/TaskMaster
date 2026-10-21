# Orchestrator cross-check of the #488 correction comment

Timestamp: 2026-08-25T10-20
Author: orchestrator (epic child 488, wave 3)
Purpose: close open item 1 of `2026-08-25T10-00-itemviewer-breadcrumb-lifecycle-defects-research.md`
§0.1, which recorded that the research session could not retrieve the GitHub comment on issue #488
and therefore treated its three claims as unverified hypotheses.

The orchestrator retrieved the comment verbatim via `gh issue view 488 --json comments` before
delegating research. This artifact diffs that comment against research §2 and §3 and verifies every
claim against source at HEAD. **No research conclusion is overturned.** Two claims the research left
unresolved are resolved here.

## Claim 1 — "Defect 1 is partly inaccurate; the previous host *is* disposed"

**Comment's position:** `coordinator.Release()` does dispose the host, quoting
`BreadcrumbDropDownOpenCoordinator.cs:150-159`. Defect 1's "the first host is never disposed" is
wrong; the residual is that disposal is asynchronous and fire-and-forget.

**Verified at HEAD:** CONFIRMED in substance, stale in citation. `Release()` occupies `:183-192`, not
`:150-159`, and calls `_host.Dispose()` at `:190` inside a `_operations.PostAsync(...)` whose task is
discarded. The comment was written against an earlier revision of the file.

**Agreement with research:** research §2/D1 reaches the same conclusion from an independent reading
and does not restate "never disposed". No change required.

**Residual framing:** the research narrows the comment further and is preferred. The comment lists
three residual risks; the `Invalidate(release: true)` returning `false` branch is reachable only on a
*second* `Release()`, after a dispose was already enqueued, so it is not itself a leak. The load-bearing
residual is ordering: the replacement host is constructed synchronously at
`ItemViewer.Breadcrumb.cs:159`, before any release is scheduled. Carry the research's framing into the
spec, not the comment's three-item list.

## Claim 2 — `SetBridgeCoordinator` replaces without disposing, while `Dispose()` disposes

**Comment's position:** `BreadcrumbItemViewerLifecycleCoordinator.cs:64-77` calls `UnsubscribeBridge()`
then overwrites `_bridgeCoordinator` without disposing the outgoing instance, whereas `Dispose()`
(`:216`) does dispose it. Unreachable today only because of the reference-equality guard at `:66-69`.
If Defect 3's fix allows a genuinely different bridge coordinator to be installed, this path becomes
live and leaks the outgoing coordinator's `BreadcrumbMessengerHub` and four event subscriptions.
"The two should be fixed together."

**Verified at HEAD:** CONFIRMED, and the comment's line citations are accurate.
- `SetBridgeCoordinator` spans `:62-77`; the reference guard is at `:66-69`.
- On replacement it calls `UnsubscribeBridge()` at `:71` then assigns `_bridgeCoordinator` at `:72`.
- `UnsubscribeBridge()` (`:306-317`) only detaches the four event handlers at `:313-316`. It disposes
  nothing.
- `Dispose()` calls `_bridgeCoordinator?.Dispose()` at `:216`, then nulls the field at `:217`.

The type therefore owns the bridge coordinator at teardown but not at replacement. The inconsistency
is real.

**Research gap:** research §3.3 cites `SetBridgeCoordinator` only as the reference-comparison precedent
D3 should mirror (§5.1 likewise lists its test as a precedent). It does not evaluate this as a defect
in its own right, which delegation item 3 requested.

**Scope decision — OUT of scope, and the comment's "fix together" condition is not triggered.**
The comment's coupling is conditional: it applies "if Defect 3's fix allows a genuinely different
bridge coordinator to be installed". Research §3.3 recommends D3 fail **fast** — throw on a different
provider — rather than re-initialize. Under fail-fast, `InitializeBreadcrumbPipeline` never constructs
a second `BreadcrumbBridgeCoordinator`, so nothing new ever reaches `SetBridgeCoordinator`'s
replacement branch and the path stays dormant exactly as it is today. Adopting the research's D3 design
discharges the comment's concern rather than deferring it.

This decision is **contingent on the spec keeping D3 as fail-fast.** If the spec instead adopts explicit
re-initialization, the replacement branch becomes live and this defect MUST be pulled into scope. The
spec must state that dependency explicitly.

Recommend promoting this to a follow-up issue so the inconsistency is tracked independently of D3's
design choice.

## Claim 3 — `Reset()` detaches two surfaces with different synchrony

**Comment's position:** `BreadcrumbItemViewerLifecycleCoordinator.cs:197` — `Reset()` detaches the
collapsed surface synchronously but the popup surface only via a posted lambda. Same class as Defect 2.
"Worth folding into Defect 2's fix so the ordering rule is applied once rather than per call site."

**Verified at HEAD:** CONFIRMED. `Reset()` spans `:191-199` (the comment's `:197` falls inside it) and
calls `DetachCollapsedMessenger()` synchronously at `:197`. It never calls `DetachPopupMessenger()`
directly; the popup detach reaches `_detachPopupMessenger()` at
`BreadcrumbDropDownOpenCoordinator.cs:178`, inside the `_operations.PostAsync(...)` opened at `:171`
by that type's `Reset()` (`:167-181`). The two surfaces are detached with different synchrony.

**Research gap:** research mentions `Reset()` only in passing (§2/D1 footnote and §3.6's D1c trigger
list). It does not evaluate the synchrony mismatch as a defect, which delegation item 3 requested.

**Scope decision — OUT of scope, on ownership grounds.**
The comment proposes folding this into Defect 2's fix. That is not available to this feature: the
asynchronous half lives in `BreadcrumbDropDownOpenCoordinator.cs`, which is owned by sibling feature
`breadcrumb-coordinator-hub-defects-501` for issue #462. That feature's
`docs/features/active/breadcrumb-coordinator-hub-defects-501/spec.md:155-164` explicitly cedes this
feature's four files to 488 and correspondingly retains the open-coordinator file. Applying "the
ordering rule once" would require editing a file this feature must not write.

The collapsed-side half alone is in an owned file, but changing only one side of a synchrony mismatch
does not fix the mismatch and would be an opportunistic refactor of a path neither #488 nor #475 filed.

Recommend promoting to a follow-up issue naming both files, so whoever owns it can change both halves
together.

## Net effect on the research

| Research open item | Disposition |
| --- | --- |
| 1. Read the #488 comment verbatim and diff against §2/§3 | CLOSED by this artifact. No conclusion overturned; two gaps filled (Claims 2 and 3). |
| 5. Promote D1c to a new issue | Unchanged, and now joined by two more follow-up candidates (Claims 2 and 3). |

Three follow-up issue candidates now exist, all in `BreadcrumbItemViewerLifecycleCoordinator.cs` or its
collaborators, none in scope for this feature:

1. D1c — `ConfigureHost`'s generation guard drops the incoming host without disposing it (research §3.6).
2. `SetBridgeCoordinator` replaces without disposing while `Dispose()` disposes (Claim 2).
3. `Reset()` detaches the collapsed and popup surfaces with different synchrony (Claim 3).

Promotion of these is out of scope for this preparation run and is recorded for the epic owner.
