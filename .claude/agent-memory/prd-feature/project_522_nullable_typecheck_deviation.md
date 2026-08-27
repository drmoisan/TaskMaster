---
name: nullable-typecheck-deviation-522
description: RESOLVED in-repo as of 2026-08-26 — CLAUDE.md now documents the CI-parity type-check command and forbids /p:Nullable=enable and /t:Build; quote CLAUDE.md directly, no deviation note needed
metadata:
  type: project
---

**Status update (verified 2026-08-26, worktree `2026-08-26T09-44`).** CLAUDE.md § C#1.2 and § C#1.3 have been fixed. They now prescribe `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` and state in-line that `/p:Nullable=enable` must **not** be added and `/t:Build` must **not** be substituted for `/t:Rebuild`, each with the reason. A spec's toolchain AC can therefore quote CLAUDE.md verbatim and no longer needs a "deliberate deviation from CLAUDE.md, see #522" note — writing one now would confuse a reviewer. Keep the *reasons* below: they are still the substance a spec should assert (non-vacuity of the MSBuild steps).

**Historical record (why the commands are shaped this way).** The `CLAUDE.md` type-check command `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` was known-defective and tracked as issue #522. Nullable is per-file opt-in in this solution (no `TaskMaster/Ribbon/` file carries a `#nullable` pragma; only five `AppGlobals` files do), so forcing `/p:Nullable=enable` solution-wide reports 200-414 errors that are red on `main` regardless of any change. CI (`.github/workflows/ci.yml`) deliberately omits the flag.

**Why:** Encoding the CLAUDE.md command verbatim into a spec's toolchain AC creates an unsatisfiable dead gate (same failure class as [[ac-gates-verify-satisfiability]]). First recorded in the #505/#506/#518 spec (2026-08-08).

**How to apply:** In any spec's Verification section and toolchain AC, use CI's actual command — `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` — and record it as a deliberate, documented deviation from CLAUDE.md with the #522 citation, so reviewers do not flag it as non-compliance. Re-verify against CLAUDE.md before relying on this: once #522 is fixed, this memory is stale and should be removed.

**Measured 2026-08-10 (feature `2026-08-10-csharp-toolchain-gate-fidelity-512`, which proposes the CLAUDE.md fix):** the documented `/t:Build` form returns EXIT 0 in 1.8 s with `Skipping target "CoreCompile"` on 18 of 18 projects — it compiles nothing, so it can neither pass nor fail honestly. CI's command returns EXIT 0 in 20.0 s with 0 skips. Retaining `/p:Nullable=enable` under `/t:Rebuild` gives EXIT 1 with **195 errors, all in `UtilitiesCS.csproj`** — and 195 is a **lower bound**, because the build aborted after 16 of 74 CoreCompile executions. A naive `Select-String 'error CS'` over the MSBuild log returns 390 (exactly double: each error prints inline and again in the summary block), which is how the historical 195/220/~414 spread arose. Trust MSBuild's own `N Error(s)` line. The documented ANALYZER step is vacuous by the identical mechanism. See also [[msbuild-nonvacuity-assertion]].
