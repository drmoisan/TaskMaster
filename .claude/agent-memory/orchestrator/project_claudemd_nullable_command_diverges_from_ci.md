---
name: claudemd-nullable-command-diverges-from-ci
description: CLAUDE.md's nullable toolchain command adds /p:Nullable=enable but ci.yml does not; forced-flag CS86xx in a file with no #nullable pragma is a false blocker, not a merge gate
metadata:
  type: project
---

`CLAUDE.md` documents the nullable stage as
`msbuild TaskMaster.sln /t:Build ... /p:Nullable=enable /p:TreatWarningsAsErrors=true`.

The gate that actually governs merge, in `.github/workflows/ci.yml` ("Build with nullable warnings
treated as errors"), is:

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

**How to apply:** when a delegated agent reports a nullable failure, do not relay it. Check whether
the diagnostic is in a file carrying `#nullable enable`; if not, reproduce `ci.yml`'s command
verbatim before accepting a blocker or leaving an AC unchecked. Do not "fix" a forced-flag-only
diagnostic with `!` or a `Type?` annotation — `Type?` in a nullable-disabled context emits CS8632,
which makes the *enforced* gate worse.

Related: [[feedback_verify_subagent_capability_claims]] (same failure mode — verify a subagent's
blocking claim against ground truth before relaying it).
