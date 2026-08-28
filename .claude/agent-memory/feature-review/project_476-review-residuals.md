---
name: 476-review-residuals
description: "#476/#458/#477 WebView2 host review (2026-08-27): PASS/0 blocking; PA-1 90%-newly-measured-member floor dispositioned non-blocking (exemption-narrowing entrants are not 'added' code); residuals CR-1 predecessor Disposed-subscription retention (promotion candidate), CR-2/CR-3 test gaps, caller-brief misdescribed the #477 fix shape"
metadata:
  type: project
---

Epic child `bug/webview2-host-initializer-defects-476-exec` (HEAD d1dcabd6) into
`epic/quickfiler-bug-family-integration`: 37/37 spec ACs substantiated, 0 blocking. Artifacts at
`docs/features/active/webview2-host-initializer-defects-476/{policy-audit,code-review,feature-audit}.2026-08-27T23-46.md`
(caller worktree `agent-addb1ac138c7882d0`).

Key dispositions and open residuals to carry into later quickfiler/epic reviews:

- **PA-1 precedent:** the plan's own 90% line floor on members "newly entering measurement" (after
  narrowing a class-level `[ExcludeFromCodeCoverage]` to member level) failed for 4 members
  (86.87% aggregate). Dispositioned non-blocking: CLAUDE.md UT2's 90% floor binds *added* members,
  and pre-existing members that enter measurement through exemption-narrowing are not added code —
  penalizing the narrowing deters measurement honesty. Verify structurally before granting this:
  each uncovered line must be SDK-reaching or its brace. Nuance found: `WebView2.NavigateToString`
  on an uninitialized control throws `InvalidOperationException` IN-PROCESS (no Evergreen runtime),
  so "requires the runtime" claims for such lines are overstated — but a throw-asserting test pins
  third-party wrapper behavior and could not reach the floor anyway (the post-call line stays dead).
- **CR-1 (promotion candidate at epic close):** `WebView2BreadcrumbHost.DetachCore` does not remove
  the predecessor's `_control.Disposed += OnControlDisposed` subscription, so a superseded host
  stays reachable from the control until disposal; spec residual-risk item 3's "the control->stale
  host edge is removed" is overstated by exactly this edge. Fix is one line + one assertion.
- **CR-2/CR-3 cheap test gaps:** `InitializeAsync` null-guard (lines 242-243) untested;
  `LogDispatchFailure` 0% but reachable in-process via a throwing dispatched forward.
- **Caller-brief hazard:** the orchestrator's brief said the #477 fix "restores the SDK's
  browserExecutableFolder parameter to the contract"; the spec mandates the opposite (unchanged
  signatures + documented Evergreen-only narrowing) and the code follows the spec. Check the spec,
  not the brief, before flagging contract shape.
- Hook note (confirms [[review-worktree-differs-from-session-cwd-mirror-artifacts]]): simulation
  passed exit 0 from the caller worktree (hand-authored pr_context.summary.txt there, C# rows
  hook-safe) and failed from the session cwd where the artifacts do not exist; caller had forbidden
  mirroring into the shared session cwd, so termination relied on the hook running in the worktree.
