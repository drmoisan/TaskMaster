---
name: plan-token-count-gate-broken-by-own-mandated-identifiers
description: A plan task that asserts an exact match count for a token is unsatisfiable when the identifiers the same plan mandates contain that token; check the arithmetic against the names the plan itself introduces
metadata:
  type: project
---

When a plan task asserts "a `Grep` for `<token>` over this file returns exactly N matches", check whether the identifiers that the *same plan* instructs the executor to create contain that token. If they do, N is arithmetically unreachable and the gate can never pass.

**Why:** observed on F824 (2026-09-09). Plan task `[P1-T4]` demanded `IsInitOnly` return exactly 2 matches, meaning the two assertion sites. But the same task mandated the test names `SingleByteOpCodes_FieldIsInitOnly` and `MultiByteOpCodes_FieldIsInitOnly`, each of which contains `IsInitOnly`, so the true count is 4 and always will be. Both escape routes are worse than the defect: renaming the tests to dodge the count breaks the acceptance criterion that names them, and swapping the assertion to `Attributes.HasFlag(FieldAttributes.InitOnly)` degrades a direct property check into an encoding check. The executor correctly asserted the discriminating pattern `\.IsInitOnly` = 2 instead and recorded the deviation.

This is a distinct failure class from [[preflight-catches-vacuous-gates]]. A vacuous gate passes whatever the executor does; this one *fails* whatever the executor does. Preflight looks for the first and can miss the second, because reading the task in isolation the count looks right — the collision only appears once you hold the task's own mandated names alongside its own count.

**How to apply:**

- At preflight, for every exact-count assertion, list the new identifiers the plan introduces (an atomic plan usually enumerates them; F824's plan had a `D20` decision listing every new literal verbatim) and check none of them contains the asserted token as a substring.
- Prefer a discriminating pattern over a bare token when the token is also a name fragment: `\.IsInitOnly` rather than `IsInitOnly`. The leading punctuation pins it to a use site rather than a declaration.
- The same trap already has a cousin recorded for the check-off phase: `**AC1` also prefixes `**AC10`, `**AC11` and `**AC12`, which is why the F824 plan required the em-dash form `**AC1 —` for every acceptance check-off grep. Treat substring-prefix collisions as a standing review item for any counting gate.
