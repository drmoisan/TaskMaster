---
name: enumeration-variable-must-match-consumer
description: A plan task that enumerates test assemblies must assign the exact variable name later vstest tasks splat, and enumeration plus invocation must sit in one pwsh -Command payload — otherwise vstest runs on zero assemblies and still reports zero failures
metadata:
  type: feedback
---

When one task computes a list and a later task splats it (`& $vstest @assemblies ...`), the producing command must assign that exact name, and the producer's body must be re-executed inside the SAME `pwsh -NoProfile -Command` payload that invokes the consumer.

**Why:** #491 preflight revision 3. P0-T17 assigned `$all`, `$claude`, and `$kept`; P0-T18 and P3-T6 both splatted `@assemblies`. An undefined `@assemblies` expands to nothing, so vstest would have run against ZERO assemblies and still reported a zero failure count — a silently green full-suite gate. The plan's own conventions paragraph already stated the no-shell-state-persists rule and still shipped the mismatch, because the rule was written about session lifetime and nobody re-read the producing command for the name it actually emits. P0-T17 is itself a `pwsh -Command` child process, so no variable it sets can survive into a later tool invocation regardless.

**How to apply:** Two checks before a plan leaves preflight.

1. **Name check.** Grep the plan for every `@<name>` splat and every `$<name>` consumption, then confirm the producing command literally assigns that identifier. Renaming the producer's variable is the cheaper fix than renaming every consumer; a display label such as `KEPT={2}` in a format string may stay as-is and should be called out as deliberate so a reviewer does not flag the mismatch.
2. **Payload check.** Every consuming task's text must say explicitly that it re-executes the enumeration body verbatim in the same payload, and that a bare `@<name>` in a fresh session is forbidden. Add an acceptance clause pinning the count actually passed on the command line to the count recorded in the enumerating task's artifact, with a `>= 1` floor — otherwise the empty-splat failure mode has no gate that can catch it.

Related: [[wiring-gates-must-be-wiring-sensitive]] — a count floor is the observable that makes an empty run fail; [[powershell-gate-observables]] for the quoting rules of the enclosing payload.
