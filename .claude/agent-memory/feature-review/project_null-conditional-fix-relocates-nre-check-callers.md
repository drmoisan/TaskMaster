---
name: null-conditional-fix-relocates-nre-check-callers
description: when a bugfix changes a throwing property/method to return null via ?. (matching a sibling precedent), always enumerate every real caller before crediting the fix with resolving the reachable crash — an unguarded caller just gets the same NRE one frame later
metadata:
  type: project
---

#507 (`RibbonController.Engines`: `Globals.Engines` -> `Globals?.Engines`) looked like a clean,
minimal, sibling-precedent-matching fix (the `SB` property in the same file already used the same
pattern) and was fully evidenced (baseline/final toolchain + coverage, expect-fail/post-fix
regression tests). But grepping every call site of the changed member
(`rg '\bEngines\b' TaskMaster`) showed all 11 real production callers, in a sibling file
(`RibbonViewer.cs`) the plan explicitly forbade touching, dereference the result with zero null
guard. Before the fix: NRE thrown inside the property getter. After: the same click still throws an
NRE, just one frame later at the call site — the crash is relocated, not eliminated. This is not
"silent" (still an unhandled exception) and not a regression (nothing relies on the throw for
control flow — no try/catch, no `!= null` check anywhere), but it does mean the fix's real-world
impact is limited to property-boundary contract conformance, not resolution of the issue's own
described reachable-crash symptom.

**Why:** the issue's own risk section can pre-disclose this tradeoff ("shifts the failure mode ...
widening caller guards is out of scope") and still be worth flagging plainly as a Blocking finding,
because the AC text ("Engines returns null instead of throwing") is literally true and verified, yet
a reader could easily believe the underlying user-facing bug is now closed when it is not, for any
of the enumerated reachable callbacks in the issue's own "Reachable callbacks" list.

**How to apply:** whenever a diff changes a member from throwing to null-returning (or otherwise
weakens a fail-fast contract) to match a sibling precedent, grep every call site of that member
across the whole repo (not just the changed file), and for each one ask: does this site null-check
before use? If none do, state plainly that the crash relocates rather than resolves, cite the
specific call sites, and rate it Blocking — do not let a strong evidence trail (passing toolchain,
targeted regression tests, explicit issue-level scope disclosure) substitute for this specific
end-to-end check. Also check whether the *same* sibling precedent property has the identical
unguarded-caller pattern (it did here, for `SB`) — that tells you this is a pre-existing codebase
convention, not a brand-new defect class, which is relevant context for severity/disposition
language even when the finding stays Blocking.

Related: [[feedback_test-file-500-line-limit]] (the accompanying test-file-size Blocking finding in
the same review — a two-test addition to an already-452-line file crossed 500 lines again).
