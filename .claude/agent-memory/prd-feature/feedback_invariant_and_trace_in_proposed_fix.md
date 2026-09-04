---
name: invariant-and-trace-in-proposed-fix
description: A bug spec's Proposed Fix must state the established invariant in one sentence and trace one accepted value from guard to throwing boundary to current absorption point to the new catch location
metadata:
  type: feedback
---

In `spec.md` for a defect, the Proposed Fix section must contain two things beyond the design:

1. **The invariant, as a single explicit sentence** naming the contract the fix establishes (exact
   return condition, exact exception type, message-redaction rule, and what can no longer escape).
2. **A numbered trace of one accepted value**, with real citations: accept point (the guard that lets
   the call proceed, and what it does *not* validate) -> throw point (the boundary that actually
   raises) -> current absorption point (where the exception is swallowed today, and why that location
   cannot report) -> where the fix moves the catch and why that boundary *can* report. Add a short
   "why neither half suffices alone" paragraph so the two halves are not read as redundant.

Pick the trace path that has **no guard anywhere** between accept and absorption; that is the path
that proves the fix is load-bearing.

**Why:** A prior remediation in this repo failed because the fix widened a strict guard so two guards
merely agreed with each other, preflight verified the widened guard in isolation, and nobody ever
followed an accepted value through to the boundary that throws — an `async void` context then turned
the unhandled result into a UI-thread crash. See the user-level memory
`state-the-invariant-not-the-symptom-in-remediation-inputs`.

**How to apply:** Write the invariant before the implementation steps, and make its restatement plus
the four trace steps a numbered acceptance criterion of its own ("the delivered implementation matches
that trace"), not just prose. Also pair it with the inverse constraint: name explicitly which
consumer-side catches must *not* be widened, and cite the live test that pins the opposite behavior,
so an implementer cannot "be safe" by broadening a catch. Related:
[[ac-gates-verify-satisfiability]], [[backticked-paths-are-the-change-footprint]].
