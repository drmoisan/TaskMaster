---
name: binding-redirect-drift-973
description: Why app.config redirects drift (NuGet rewrites only the installing project's config), the csproj Reference is never the stale side, and the System.Linq.AsyncEnumerable install was ruled IN SCOPE for #973 (aliased Reference form only, see [[ixnet-v7-lib-ref-clash-packages-config]])
metadata:
  type: project
---

Binding-redirect drift in this repo follows one rule: a redirect is current exactly in the projects whose own packages.config installs the package, and stale in every project that carries the assembly transitively (verified 2026-10-02 for all 15 #973 pairs across 17 app.config files). NuGet rewrites redirects only in the project it updates. The csproj `Reference Include="X, Version="` side was correct in every case; the app.config was always the stale side.

**Why:** #418, #953 and #973 all came from the same mechanism. `TaskMaster/app.config` (the only honoured production config, applied as `TaskMaster.dll.config` in the Outlook AppDomain) installs almost none of the packages it redirects, so it is the config most likely to be stale and the one that matters. The #953 Pester gate (`tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1`) detects drift against csproj References; it does not prevent it.

**How to apply:** when a redirect question comes up, compare app.config `newVersion` to the csproj `Reference` version first; do not propose csproj edits. Never recommend removing redirects from configs of projects that "do not install the package" — that rule would strip TaskMaster/app.config. Keep the `netstandard` 2.1.0.0->2.0.0.0 block in TaskMaster/app.config: it is deliberate #879 hardening.

Status 2026-10-02 (late): the orchestrator ruled the missing `System.Linq.AsyncEnumerable` package IN SCOPE for #973 under the maintainer's related-defect directive (same files, same defect class), together with deleting the 13 dead ADAL blocks. The supplemental research (`docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/research/2026-10-02T23-40-system-linq-asyncenumerable-install-research.md`) found that a plain NuGet-form Reference breaks the build (CS0121 at ~60 call lines) and recommended an `<Aliases>`-bearing Reference in the five installing projects (UtilitiesCS, QuickFiler, ToDoModel, TaskMaster, UtilitiesCS.Test) at 10.0.12, redirects to the restored DLL's assembly version (expected 10.0.0.12, must be read after restore, not assumed). Check the item folder before re-deriving.
