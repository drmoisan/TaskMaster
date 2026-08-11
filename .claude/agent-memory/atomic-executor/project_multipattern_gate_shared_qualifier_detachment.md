---
name: multipattern-gate-shared-qualifier-detachment
description: Rewriting one clause of a multi-pattern grep gate can silently detach a shared allowlist qualifier from the other patterns; re-read the whole acceptance sentence after every edit
metadata:
  type: project
---

When a plan's acceptance criterion enumerates several grep patterns governed by one trailing
qualifier ("... and ... **outside the SD1 allowlist**"), any restructuring of that sentence risks
re-scoping the qualifier to only the last pattern, making the earlier patterns unconditional and the
gate unsatisfiable. This happened on #512 round 3: a rewrite of pattern (c) left patterns (a) and (b)
demanding zero hits repo-wide while 10 and 9 hits legitimately live in the protected SD1 mirrors.

**Why:** multi-pattern gates accumulate clauses across preflight rounds. Each round edits one clause
in isolation, so the shared qualifier's scope is never re-read as a whole. The failure mode is
invisible to a validator (the plan is well-formed) and only surfaces as an impossible gate at run
time.

**How to apply:** for any acceptance criterion naming two or more patterns, (1) confirm the
allowlist/exclusion qualifier is restated per pattern rather than shared, (2) enumerate the actual
world state with `git grep` per pattern and per exclusion set before declaring satisfiability, and
(3) check the inverse defect too — that the carve-out does not excuse an in-scope site. Pair the
carve-out with a per-pattern count-identity requirement against the Phase 0 before-state so the
allowlist cannot absorb a newly introduced or silently corrected site. Related:
[[verify-line-citations-with-numbered-output]], [[preflight-selfderived-gate-thresholds-are-blind]].
