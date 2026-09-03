---
name: project-735-evidence-content-sanitization-seams
description: "#735 ribbon-engine-toggle-defects revision round 1 — name-only evidence sanitization gates cannot fail; TRX/Cobertura carry account+machine tokens in content; csproj alphabetical placement clause was self-contradictory"
metadata:
  type: project
---

Revision-round seams found while applying the preflight delta to
`docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/plan.2026-09-02T12-04.md`.

**A sanitization gate that checks only file and directory NAMES cannot fail.**
A committed `.trx` carries the local account token in the single `runUser=` attribute of its
`TestRun` element and the machine-name token in the `computerName=` attribute of EVERY
`UnitTestResult` element. A Cobertura document carries both inside the absolute source paths
it records. Measured precedent in this repo:
`docs/features/active/2026-08-25-quickfiler-high-confidence-partial-screen-backfill-608/evidence/regression-testing/r2-full-diagnostic.trx`
has 1 `runUser=` and 6476 `computerName=` occurrences, none in its name.

**Why:** any plan that commits TRX or Cobertura into `<FEATURE>/evidence/` leaks both tokens
into main. `.gitignore` ignores `*.coverage` and `*.coveragexml` (lines 140-141) and the
repo-root `coverage/` dir (line 144), but NOT `*.trx` and NOT `*.cobertura.xml` — so those two
extensions really are committed.

**How to apply:** pair a rewrite task (case-insensitive substitution over the CONTENT of every
`.trx`, `.cobertura.xml`, `.md` under the evidence tree, recording per-file substitution counts
because the rewrite exits 0 either way — G7) with a completeness gate asserting zero
case-insensitive occurrences in BOTH names and contents. Derive both tokens at run time
(`Split-Path -Leaf $env:USERPROFILE`, `$env:COMPUTERNAME`) and forbid writing either value into
the plan or into the two artifacts, so the gate does not flag its own output. See
[[runtime-derived-account-token-pattern]] and [[zero-hit-grep-gates-need-carveouts]].
`.csharpierignore` line 4 excludes `**/evidence/**`, so the rewrite cannot trigger a formatter restart.

**Ordering trap:** the completeness gate's "every artifact named in this plan exists" clause is
unsatisfiable when a later task (here the reduced-audit handoff) writes an artifact after the
gate runs. Bound the clause to tasks preceding the gate.

**TaskMaster.csproj placement clause.** The ribbon compile-item group (lines 458-470) IS
alphabetical, but `Ribbon\EngineToggleStateCoordinator.cs` (463) and `Ribbon\RibbonController.cs`
(464) are ADJACENT, so "insert alphabetically between them" is self-contradictory. Verify the two
entries that actually bracket the new stem. `TaskMaster.Test/TaskMaster.Test.csproj`'s ribbon
group (314-324) is NOT alphabetical — `RibbonExplorerXmlTests.cs` at 324 trails
`TryFunctionalityInConstructionTests.cs` at 323 — so never assert alphabetical placement there.

**RibbonController.Intelligence.cs Spam Manager method (206-233):** three unguarded globals
dereferences, not two — one at 219-224 inside the `if` condition, plus 229 and 230. A shape
clause naming only the guarded block undercounts; the acceptance clause must demand zero
occurrences across the whole method body.
