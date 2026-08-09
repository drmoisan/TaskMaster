---
name: nullable-typecheck-deviation-522
description: CLAUDE.md's type-check command with /p:Nullable=enable is known-defective (issue #522); specs must bind verification to CI's command and record the deviation explicitly
metadata:
  type: project
---

The `CLAUDE.md` type-check command `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` is known-defective and tracked as issue #522. Nullable is per-file opt-in in this solution (no `TaskMaster/Ribbon/` file carries a `#nullable` pragma; only five `AppGlobals` files do), so forcing `/p:Nullable=enable` solution-wide reports 200-414 errors that are red on `main` regardless of any change. CI (`.github/workflows/ci.yml`) deliberately omits the flag.

**Why:** Encoding the CLAUDE.md command verbatim into a spec's toolchain AC creates an unsatisfiable dead gate (same failure class as [[ac-gates-verify-satisfiability]]). First recorded in the #505/#506/#518 spec (2026-08-08).

**How to apply:** In any spec's Verification section and toolchain AC, use CI's actual command — `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` — and record it as a deliberate, documented deviation from CLAUDE.md with the #522 citation, so reviewers do not flag it as non-compliance. Re-verify against CLAUDE.md before relying on this: once #522 is fixed, this memory is stale and should be removed.
