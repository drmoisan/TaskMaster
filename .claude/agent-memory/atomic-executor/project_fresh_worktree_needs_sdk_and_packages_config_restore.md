---
name: fresh-worktree-needs-sdk-and-packages-config-restore
description: A fresh TaskMaster worktree needs .dotnet-sdk provisioning AND `/p:RestorePackagesConfig=true`; the plan-standard `dotnet tool restore` and `msbuild /t:Restore` both no-op or fail
metadata:
  type: project
---

Two bootstrap steps that plans routinely omit, both required before any gate can run in a cold
TaskMaster worktree. Measured on issue #821, worktree `rr0908-821`, 2026-09-09.

**1. `dotnet tool restore` fails outright with no repo-local SDK.** `global.json` pins sdk `8.0.205`
with `rollForward: latestFeature` and search paths `.dotnet-sdk` then `$host$`. A fresh worktree has
no `.dotnet-sdk`, and the host had only `10.0.400`, which `latestFeature` on an `8.0.2xx` pin rejects.
`dotnet tool restore` exits `-2147450725` printing `global.json`'s own `errorMessage`. Fix: run
`./scripts/vscode/Install-RepoDotNetSdk.ps1` (~733 MB, ~1 min). It resolves its target from
`$PSScriptRoot/../..`, so invoking it by path in the target worktree is safe.

**2. `msbuild TaskMaster.sln /t:Restore /m` restores nothing.** It handles `PackageReference` only;
every project here is legacy `packages.config`. It exits 0 printing `Nothing to do. None of the
projects specified contain packages to restore.` while `packages/` stays absent —
`UtilitiesCS.csproj` alone carries **153** `..\packages\` references, so every later build would fail
on unresolvable `HintPath`/`Analyzer Include` rather than on a real diagnostic. Fix: add
`/p:RestorePackagesConfig=true`, which installed **172** packages.

**Why it matters for plan execution:** a plan may explicitly forbid provisioning ("do not provision a
repo-local .NET SDK: that is environment setup outside this plan's scope"). That instruction assumes a
usable SDK exists. When it does not, the plan is unexecutable at task 1 and the executor is past
preflight, so it cannot block — do the minimum needed and record it as a named deviation.

**How to apply:** both writes land only on gitignored or untracked-output paths — `.dotnet-sdk/` is
ignored at `.gitignore:350` (`.dotnet*/`, confirm with `git check-ignore -v .dotnet-sdk/x`) and
`packages/` is restore output — so neither widens an AC20-style footprint. Record each as an explicit
numbered deviation in the bootstrap artifact, quoting the failing command, its exit code and the
observation that proved the need, rather than quietly fixing the environment.
