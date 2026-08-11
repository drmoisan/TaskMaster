---
name: claudemd-nullable-command-diverges-from-ci
description: RESOLVED 2026-08-11 by PR #540 - CLAUDE.md's C# toolchain commands now match ci.yml; the historical divergence (forced /p:Nullable=enable, vacuous /t:Build) and its false-blocker failure mode are recorded here
metadata:
  type: project
---

**STATUS: RESOLVED on 2026-08-11 by issue #512 / PR #540** (child of epic
`build-ci-coverage-gate-fidelity`). `CLAUDE.md`, `.claude/rules/csharp.md` and
`.claude/skills/csharp-qa-gate/SKILL.md` now document `/t:Rebuild /m ... /p:TreatWarningsAsErrors=true`
with no `/p:Nullable=enable`, character-for-character `ci.yml`'s command, with in-line prohibitions
against re-adding either defect. The format command is now `dotnet tool run csharpier format .`
(v0 bare-path syntax fixed, #509). Read the sections below as the historical record of what the
divergence was and what it cost, not as a live description of the repo.

**If you find `/p:Nullable=enable` or `/t:Build` back in a documented C# toolchain command, that is
a regression of #512/#522, not policy.** The corrected sites carry explicit "do not restore this"
prose for exactly that reason.

---

Historically `CLAUDE.md` documented the nullable stage as
`msbuild TaskMaster.sln /t:Build ... /p:Nullable=enable /p:TreatWarningsAsErrors=true`.

The gate that actually governs merge, in `.github/workflows/ci.yml` ("Build with nullable warnings
treated as errors"), is (and always was):

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```

It uses `/t:Rebuild` deliberately (to defeat the incremental up-to-date vacuity) but it does **not**
pass `/p:Nullable=enable`. Its inline comment states enforcement "relies entirely on each file's own
`#nullable enable` pragma (the repo's per-file opt-in convention)".

**Why this matters:** `/p:Nullable=enable` force-enables nullable analysis across thousands of
never-annotated files. Measured 2026-08-08: 195 pre-existing errors in `UtilitiesCS.csproj` and 219
in `TaskMaster.csproj`, red on `main` independently of any change. A subagent measuring against the
documented command will hand you a blocker that no gate enforces and that cannot be fixed within a
minor-audit scope.

**Worked case (#507).** An executor left AC5 unchecked and reported a "new CS8603 attributable to
the fix" after changing `Globals.Engines` to `Globals?.Engines`. The file has no `#nullable` pragma
and `TaskMaster.csproj` has no `<Nullable>` element. Running CI's exact command with the change
applied returned EXIT 0, zero errors, zero CS8603. The blocker was an artifact of the documented-but-
unenforced flag. The sibling `SB` property already returns `null` from a non-nullable declared
return type, so the pattern was pre-existing anyway.

**Measured figure for the follow-on burn-down (2026-08-10, #492).** Under an explicit
`/p:Nullable=enable` probe the corrected `/t:Rebuild` gate reports **195 errors, all in
`UtilitiesCS.csproj`** (CS8766 x130, CS8618 x23, CS8625 x12, CS8600 x9, CS8601 x8, CS8604 x7,
CS8602 x3, CS8603 x2, CS8714 x1). That is a **lower bound**: the build aborted after 22 of 73
`CoreCompile` executions, so `UtilitiesCS`'s dependents never compiled. Size that epic by measuring
the solution-wide total first, not by trusting 195.

**How to apply:** when a delegated agent reports a nullable failure, do not relay it. Check whether
the diagnostic is in a file carrying `#nullable enable`; if not, reproduce `ci.yml`'s command
verbatim before accepting a blocker or leaving an AC unchecked. Do not "fix" a forced-flag-only
diagnostic with `!` or a `Type?` annotation — `Type?` in a nullable-disabled context emits CS8632,
which makes the *enforced* gate worse.

Related: [[feedback_verify_subagent_capability_claims]] (same failure mode — verify a subagent's
blocking claim against ground truth before relaying it).
