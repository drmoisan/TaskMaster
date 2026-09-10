---
name: local-csharp-gate-for-a-non-csharp-change
description: Running the local four-step C# toolchain in a fresh child worktree - mirror the 2.1GB bootstrap from a sibling worktree, expect a flaky testhost crash, and know that .csharpierignore already excludes evidence XML
metadata:
  type: project
---

An epic child whose PR is based on the integration branch gets **zero** CI runs (`ci.yml`'s
`pull_request` trigger is scoped to `[main, development]`), so merge-on-green requires running the
four-step C# toolchain locally. Four things make that cheap or expensive, and none is obvious.

**1. Mirror the bootstrap from a sibling worktree instead of installing it.** A fresh child worktree
has no `.dotnet-sdk` and no `packages/`, and every C# gate fails without them. Both are gitignored,
so a plain `Copy-Item -Recurse` from a sibling worktree that is already bootstrapped works and is far
faster than `Install-RepoDotNetSdk.ps1` plus `nuget restore`. Measured on `rr0908-815` (2026-09-09):
`.dotnet-sdk` is 733 MB / 5266 files and `packages` is 1369 MB / 5299 files, and the pair copied in
under a minute on local disk. Afterwards `dotnet --version` prints `8.0.205` (matching the
`global.json` pin) and `dotnet tool restore` brings back CSharpier 1.2.6. Note `.config/dotnet-tools.json`
IS tracked, so only the two gitignored trees need mirroring.

**2. `.csharpierignore` already excludes `**/evidence/**`.** CLAUDE.md warns that CSharpier 1.2.6
also processes `*.xml`, which makes a plan that commits JaCoCo or Cobertura XML under
`<FEATURE>/evidence/` look like a format-gate risk. It is not: the ignore file's first non-comment
line is `**/evidence/**`, followed by `*.cobertura.xml`, `*.coverage`, `*.coveragexml`, `*.trx`, and
the three project-file globs. Check that file before planning a remediation for evidence XML.

**3. A testhost crash on the first vstest run is usually flake — re-run before investigating.** On
2026-09-09 the first `Invoke-MSTestWithCoverage.ps1` run aborted with
`Test host process crashed` after 2341 passed; the immediate re-run on the identical tree returned
`7196/7196` and exit 0. The corroborating control is the total: a sibling child had recorded 7196 on
the same base, so a matching total on the re-run is evidence the suite is intact rather than that the
crash was suppressed. Record the aborted attempt in the checkpoint rather than discarding it.

**4. `Test-OrchestratorStateCompletionReadiness` lives in a DIFFERENT module.** It is exported by
`.claude/lib/orchestrator-state/OrchestratorStateCompletion.psm1`, not
`OrchestratorState.psm1` (which exports `Invoke-OrchestratorStatePreflight` and
`Test-OrchestratorStatePrCreationReadiness`). Importing the wrong one gives
`The term ... is not recognized`, which reads like the function was renamed.

**How to apply.** When a child's diff touches **no** `.cs`, `.csproj`, `.props`, `.targets`,
`packages.config`, `.editorconfig` or `.globalconfig`, the C# gate outcome is identical to the base
by construction, so the local run is a confirmation rather than a discovery. Run it anyway when the
change modifies the coverage driver script itself — `scripts/vscode/Invoke-MSTestWithCoverage.ps1` is
what *runs* the C# suite, so step 4 doubles as the only end-to-end test of that change and will print
the script's new output lines live. See [[csharp-agent-worktree-needs-three-bootstrap-steps]],
[[vstest-aggregate-crash-isolate-per-assembly]], [[whole-repo-ci-gate-not-out-of-scope]].
