---
name: msbuild-nonvacuity-assertion
description: The only reliable proof that an MSBuild gate actually compiled is zero occurrences of 'Skipping target "CoreCompile"' in an /fl log; csc.exe counts and CoreCompile headers both mislead
metadata:
  type: project
---

When a spec or plan must prove an MSBuild step performed a genuine compile (not just returned exit 0), require: `/fl "/flp:logfile=<path>;verbosity=normal"` plus an assertion of **zero** occurrences of the literal `Skipping target "CoreCompile"`.

Two mechanisms that look right and are not, both measured 2026-08-10:

- **Counting `csc.exe` does not work at `verbosity=normal`.** All four probe runs reported zero `csc.exe` occurrences, including the two that genuinely recompiled the whole solution. A zero count proves nothing.
- **`CoreCompile:` header lines print even when the target is skipped.** Counting those headers as "executions" is what produced the contradictory artifact `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/baseline/nullable-build-baseline.2026-08-06T22-23.md`, which claims 18 executions and 0 short-circuits for a run that in fact skipped everything.

**Why:** MSBuild's incremental up-to-date check compares timestamps and does not invalidate on a command-line `/p:` change, so outputs built under one property set are silently accepted as validating a different one. Exit code alone cannot distinguish a real pass from a vacuous one, and the two obvious counting proxies are both unreliable.

**How to apply:** Use the skip-count assertion in any AC or evidence requirement about "the gate actually ran". If an issue's AC text prescribes the `csc.exe` count (issue #512 AC2 does), satisfy the substantive requirement with the skip-count assertion and record the substitution as an explicit deviation rather than renumbering or rewriting the AC. See [[nullable-typecheck-deviation-522]].
