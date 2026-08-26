# Cross-Cutting Findings Recorded During Execution of #498

Timestamp: 2026-08-26T01-30
Author: child orchestrator for `bug/breadcrumb-router-navigation-defects-498`
Scope: two repository-wide conditions encountered while executing this feature's plan. Neither is caused
by this feature, neither is repaired by this feature, and neither is in this feature's ownership set.
This artifact exists so the findings survive the merge of this branch and remain reviewable.

---

## Finding 1 — Analyzer package version skew breaks every fresh checkout

Severity: High (blocks all C# work in any newly created worktree or clone)
State: worked around locally by environment provisioning; **upstream repair still outstanding**

### What is wrong

Dependabot commit `f8e22af7` ("Bump the analyzers-dev-deps group with 2 updates") raised:

- `Meziantou.Analyzer` from `3.0.156` to `3.0.174`
- `Roslynator.Analyzers` from `4.16.0` to `4.16.1`

It updated `packages.config` and each project's `<Import>` and restore-check `<Error>` lines, but it did
**not** update the hand-written `<Analyzer Include>` item paths, which still name the superseded
`3.0.156` and `4.16.0` directories.

The skew is present in **16** first-party project files:

`QuickFiler`, `QuickFiler.Test`, `Tags`, `Tags.Test`, `TaskMaster`, `TaskMaster.Test`, `TaskTree`,
`TaskTree.Test`, `TaskVisualization`, `TaskVisualization.Test`, `ToDoModel`, `ToDoModel.Test`,
`UtilitiesCS`, `UtilitiesCS.Test`, `VBFunctions`, `VBFunctions.Test`.

`<Analyzer Include>` is unconditional, so both the analyzer gate and the plain
`/p:TreatWarningsAsErrors=true` gate fail identically with `error CS0006` on a fresh `packages/`.
Only `SVGControl` and `SVGControl.Test` compile. Measured in this worktree before remediation:
10 `error CS0006`, 0 warnings, on both gates.

### Why CI and developer machines do not catch it

This is the important part, because the condition is invisible from a green pipeline.

- `.github/workflows/_build-analyzers.yml` caches `packages` with
  `key: nuget-${{ runner.os }}-${{ hashFiles('**/packages.config') }}` and
  `restore-keys: nuget-${{ runner.os }}-`. When the `packages.config` hash changes, the exact key
  misses and the **restore-key prefix hits an older cache** that still contains `3.0.156` and `4.16.0`.
  `nuget restore` then adds `3.0.174` and `4.16.1` alongside them without removing anything.
- Developer checkouts accumulate the same way. The main checkout of this repository currently holds
  `Meziantou.Analyzer.3.0.101`, `3.0.123`, `3.0.156` and `3.0.174` side by side, plus
  `Roslynator.Analyzers.4.16.0` and `4.16.1`.

So the stale paths resolve everywhere that has history, and fail only where `packages/` is genuinely
new. CI has been green on `main` continuously — verified across the five most recent runs — while this
defect was present. **Green CI here is cache-explained, not evidence of correctness.**

### What was done in this feature

`packages/Meziantou.Analyzer.3.0.156/` and `packages/Roslynator.Analyzers.4.16.0/` were provisioned into
this worktree's gitignored `packages/` directory, bringing it to parity with CI and with developer
machines. `git status --porcelain -- packages` is empty; **no tracked file changed**, so AC-30 and the
`P7-T3` ownership gate are unaffected.

Verification after provisioning: the plan's exact analyzer Rebuild recipe over `TaskMaster.sln` returned
`EXIT_CODE: 0` with all twenty projects producing assemblies. The corrected baselines are recorded in
`evidence/baseline/p0-t13-analyzer-rebuild.md` and `evidence/baseline/p0-t14-nullable-rebuild.md`.

This mattered for gate integrity, not merely for convenience. Had the red readings been kept as the
baseline, the plan's Baseline-Comparison Rule would have degraded `P8-T3` and `P8-T4` into gates that
could not fail, and would have licensed every intermediate analyzer check in Phases 1 through 7 to pass
while compiling nothing.

### Recommended upstream repair (not performed here)

Update the 16 `<Analyzer Include>` version strings to `3.0.174` and `4.16.1`. This was deliberately not
done on this branch: the 16 project files lie far outside this feature's ownership set, and editing them
would guarantee rebase conflicts with the sibling epic children executing concurrently against
`epic/quickfiler-bug-family-integration`.

Two durable follow-ups are worth considering, because the condition recurs on **every** analyzer bump:

1. Extend `scripts/vscode/Sync-PackageReferences.ps1` (which already reconciles csproj references against
   `packages.config`) to cover `<Analyzer Include>` items, and run it in CI as a drift check.
2. Drop the `restore-keys:` prefix fallback from the analyzer and nullable workflow caches, so a
   `packages.config` change forces a clean restore and the pipeline stops masking this class of defect.

---

## Finding 2 — Tracked TRX evidence embeds the account and machine name

Severity: Low (privacy hygiene in a public repository)
State: accepted as pre-existing repo convention; not changed by this feature

`vstest.console.exe` writes identity attributes into the TRX body itself — `TestRun/@name`,
`TestRun/@runUser`, and `Deployment/@runDeploymentRoot` — carrying the operating-system account and the
machine name. Controlling `/ResultsDirectory:` and `LogFileName=` changes the file *name* but not these
attributes, and vstest exposes no switch to suppress them.

Measured scope: **50** TRX files are already tracked in this repository and carry these attributes, across
`docs/features/archive/**` and `docs/features/active/**`. One is even named
`docs/features/archive/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/trx/DanMoisan_MEGALODON4_2026-06-08_19_57_49_net481.trx`.

Decision taken for this feature: leave TRX bodies unmodified. Hand-editing tool-generated evidence would
falsify the artifact that the plan's gates read, and redacting two files out of fifty-two would produce an
inconsistent corpus without removing the information from history. All prose authored for this feature —
every `.md` evidence artifact — is free of account, host, and absolute-path identifiers, and the
Cobertura coverage file uses a relative `<source>.</source>` root with relative filenames.

The durable fix, if the project wants one, is a post-processing step in
`scripts/vscode/Invoke-MSTestWithCoverage.ps1` that rewrites those three attributes before the TRX is
copied into an evidence folder, applied repo-wide rather than per-feature.
