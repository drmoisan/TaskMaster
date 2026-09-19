# Dependabot fan-out and CI-failing NuGet upgrades — research (Issue #911)

- **Issue:** #911
- **Feature folder:** `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/`
- **Author:** task-researcher
- **Date:** 2026-09-19T11-30
- **Scope:** R1–R5 in the delegation brief, plus current-state analysis and design mapping.

---

## 0. Method and evidence classification

No command execution was available in this session. The `Bash` tool is disabled
(`Error: No such tool available: Bash. Bash is disabled for this session, in subagents as well
as here.`), and no PowerShell tool is granted. Available tools were `Read`, `Grep`, `Glob`,
`WebFetch`, `Write`, `Edit`. Every finding below is therefore either a **file-system observation**
(Read/Grep/Glob against the working tree), a **remote observation** (WebFetch against a live
GitHub page or API returning this repository's own data), or a **documentation/source reading**.

Labels used throughout:

| Label | Meaning |
|---|---|
| `[OBS-LOCAL]` | Observed directly in this worktree via Read/Grep/Glob. Highest confidence. |
| `[OBS-REMOTE]` | Observed in live repository data (GitHub PR page / REST API) via WebFetch. High confidence; subject to the summarising model's fidelity, so corroborated where load-bearing. |
| `[DOC]` | Read from vendor documentation. Confidence stated per item. |
| `[SRC]` | Read from vendor source code. Confidence stated per item. |
| `[INFER]` | Reasoned conclusion. The inputs are named so the reasoning can be checked. |

**Nothing in this document was executed.** Where the brief asked for execution (R2), that is
stated explicitly and a recommended reading with a confidence level is given instead.

---

## 1. Corrections to the premises in `spec.md` / `issue.md`

Three premises carried into the spec are contradicted by evidence gathered here. They are listed
first because two of them change what the fix has to do.

### 1.1 Dependabot **does** maintain the coupled `.csproj` and `app.config` state

`spec.md` line 41 and line 89 state that Dependabot "edits `packages.config` but cannot maintain
the coupled `.csproj` state" and that the branch diff shows "`packages.config` version attributes
changed, `.csproj` unchanged."

`[OBS-REMOTE]` PR #908 (`Bump the test-frameworks group with 11 updates`) changes **30 files**:
10 `.csproj`, 10 `app.config`, 10 `packages.config`. Full enumeration is in
§ Numeric Derivation Evidence, claim N3.

`[OBS-REMOTE]` The `.csproj` patch retrieved from
`https://api.github.com/repos/drmoisan/TaskMaster/pulls/908/files` shows Dependabot rewriting
exactly the elements the brief attributes to the NuGet CLI:

```
-  <Import Project="..\packages\MSTest.TestAdapter.4.4.0\build\net462\MSTest.TestAdapter.props"
+  <Import Project="..\packages\MSTest.TestAdapter.4.4.1\build\net462\MSTest.TestAdapter.props"

-    <Reference Include="FluentAssertions, Version=8.10.0.0, Culture=neutral, PublicKeyToken=33f2691a05b67b6a">
-      <HintPath>..\packages\FluentAssertions.8.10.0\lib\net47\FluentAssertions.dll</HintPath>
+    <Reference Include="FluentAssertions, Version=8.11.0.0, Culture=neutral, PublicKeyToken=33f2691a05b67b6a">
+      <HintPath>..\packages\FluentAssertions.8.11.0\lib\net47\FluentAssertions.dll</HintPath>
+      <Private>True</Private>
```

and the `app.config` patch shows binding redirects being moved:

```
-        <bindingRedirect oldVersion="0.0.0.0-8.10.0.0" newVersion="8.10.0.0" />
+        <bindingRedirect oldVersion="0.0.0.0-8.11.0.0" newVersion="8.11.0.0" />
```

### 1.2 The reason is that Dependabot **is** the NuGet CLI update command

`[SRC]` `dependabot-core`'s packages.config updater
(`nuget/helpers/lib/NuGetUpdater/NuGetUpdater.Core/Updater/PackagesConfigUpdater.cs`) does not
reimplement NuGet. It invokes `NuGet.CommandLine.Program.Main` in-process. The literal argument
arrays are:

- restore: `"restore", packagesConfigPath, "-PackagesDirectory", packagesDirectory, "-NonInteractive"`
- update: `"update", packagesConfigPath, "-Id", dependencyName, "-Version", newDependencyVersion, "-RepositoryPath", packagesDirectory, "-NonInteractive"` (plus `"-MSBuildPath", msbuildDirectory` when resolvable)

It runs **one restore + one update per dependency**, and it handles binding redirects separately
through its own `BindingRedirectManager.UpdateBindingRedirectsAsync`. It contains **no**
`<Analyzer Include>` handling. Confidence: high — this is the file that names the behaviour, read
from `raw.githubusercontent.com`.

This is the single most consequential finding in this research. It means:

- Everything the NuGet CLI `update` command writes, Dependabot already writes today. Building a
  workflow around `nuget update` will not, by itself, produce a `.csproj` that is any more correct
  than the one Dependabot produces.
- The `-Id` + `-Version` per-package invocation shape that the settled design proposes is
  byte-for-byte the shape Dependabot already uses. It is therefore known-workable in this
  repository, which materially de-risks that part of the design.
- The observed failure surface reduces to the classes the CLI provably does not write, plus
  formatting.

### 1.3 The fan-out is per **group**, not per group × directory

`spec.md` line 63-64 and `issue.md` line 60-61 attribute duplicate PRs to "four groups combined
with the directory glob." That is not what happens.

`[OBS-LOCAL]` The four live Dependabot branches, read from
`C:\Users\DanMoisan\repos\TaskMaster\.git\info\refs` lines 161-164, are:

```
dependabot/nuget/QuickFiler.Test/analyzers-dev-deps/Meziantou.Analyzer-9e457076d6
dependabot/nuget/QuickFiler.Test/graph-identity-telemetry/Azure.Monitor.OpenTelemetry.Exporter-555213f64e
dependabot/nuget/QuickFiler.Test/microsoft-extensions-and-bcl/Microsoft.Bcl.AsyncInterfaces-7646a09fb2
dependabot/nuget/QuickFiler.Test/test-frameworks/Microsoft.TestPlatform.AdapterUtilities-978cb75f92
```

Four branches for four groups. All four carry the **same** directory segment. If the directory
glob multiplied the groups, there would be up to 4 × 18 branches and the `open-pull-requests-limit: 10`
ceiling would be saturated; it is not.

`[OBS-REMOTE]` PR #908 confirms the consolidation directly: one PR, 10 distinct project
directories. See § Numeric Derivation Evidence, claim N3.

`[OBS-LOCAL]` Lines 165-175 of the same file are a different, older shape
(`dependabot/nuget/UtilitiesCS/AngleSharp-1.8.1`) with no group segment and versions that predate
the current manifests. These are stale branches from before `groups:` was configured and are not
evidence of current fan-out.

### 1.4 Two smaller corrections

- **Required checks count.** `spec.md` line 21/45 and `issue.md` line 26/42 say "six required
  checks" and name `pester`. The brief's verified fact is **five**, with no `pester` check.
  `[OBS-LOCAL]` there is no Pester workflow in `.github/workflows/` (`_actionlint`,
  `_build-analyzers`, `_build-nullable`, `_format-check`, `_mstest-coverage`, `ci`,
  `codex-web-setup-test`). The spec text should be corrected to five.
- **Project count.** The brief and spec say "17 non-SDK projects." `[OBS-LOCAL]` there are
  **18** — `Glob **/*.csproj` and `Glob **/packages.config` both return 18, over the same 18
  directories. The figure 17 is the count of projects carrying `<Analyzer Include>` items;
  `SVGControl\SVGControl.csproj` carries none. See § Numeric Derivation Evidence, claim N1/N4.

---

## 2. Current state analysis

### 2.1 CI topology and the five required checks

`[OBS-LOCAL]` `.github/workflows/ci.yml` is a pure orchestrator (33 lines, no inline `steps:`),
triggered on `push: [main, development]`, `pull_request: [main, development]`, and
`workflow_dispatch`. It declares `permissions: contents: read` and a concurrency group keyed on
`github.event.pull_request.number || github.ref`.

Check-run names are `<ci.yml job name> / <callee job name>`:

| `ci.yml` job `name:` | callee job `name:` | resulting check name |
|---|---|---|
| `actionlint` | `actionlint` | `actionlint / actionlint` |
| `format-check` | `Verify formatting` | `format-check / Verify formatting` |
| `build-analyzers` | `Build with analyzers and code style enforcement` | `build-analyzers / Build with analyzers and code style enforcement` |
| `build-nullable` | `Build with nullable warnings treated as errors` | `build-nullable / Build with nullable warnings treated as errors` |
| `mstest-coverage` | `Run MSTest suite with coverage` | `mstest-coverage / Run MSTest suite with coverage` |

These match the five required checks in the brief exactly. `[OBS-LOCAL]`
`.github/workflows/README.md` line 87 documents the same derivation.

`[OBS-LOCAL]` No workflow in the repository references any `secrets.*` value. Every workflow
declares `permissions: contents: read` only. Any new upgrade workflow will be the first consumer
of a secret and the first to need write permissions.

### 2.2 The four defect classes, re-derived

**(a) `<Analyzer Include>` is the only `.csproj` element class nobody writes.**

`[OBS-LOCAL]` In `UtilitiesCS\UtilitiesCS.csproj`:

- line 3 and line 1300 reference `Meziantou.Analyzer.3.0.235` (the `<Import>` and the
  package-imports `<Error>` target — each occurrence appears twice on its line, in `Project=` and
  in `Condition=`);
- line 1308 references `Meziantou.Analyzer.3.0.203` (the `<Analyzer Include>` item);
- `UtilitiesCS\packages.config` line 17-22 pins `3.0.235`.

The manifest, the `<Import>`, and the `<Error>` target all moved. Only the `<Analyzer Include>`
did not. That is exactly the signature predicted by § 1.2: the NuGet CLI moved everything it knows
about, and nothing moved the analyzer item. 15 projects are in this state (claim N2).

**(b) The formatting gate fails because Dependabot writes collapsed XML.**

`[OBS-LOCAL]` `.csharpierignore` excludes `**/evidence/**`, coverage/test artefacts, and
`*.csproj` / `*.props` / `*.targets`. It does **not** exclude `packages.config` or `app.config`.
Per CLAUDE.md § C#1, CSharpier 1.2.6 processes `*.xml` and `packages.config`.

The repository's committed files are in CSharpier-reflowed form. `UtilitiesCS\packages.config`
lines 17-22:

```xml
  <package
    id="Meziantou.Analyzer"
    version="3.0.235"
    targetFramework="net481"
    developmentDependency="true"
  />
```

and `SVGControl.Test\app.config` lines 6-10:

```xml
        <assemblyIdentity
          name="System.Threading.Tasks.Extensions"
          publicKeyToken="cc7b13ffcd2ddd51"
          culture="neutral"
        />
```

`[OBS-REMOTE]` PR #908 collapses both back to single lines — its `SVGControl.Test/app.config`
hunk is **73 additions / 193 deletions**, almost entirely reflow. So `app.config` is as much a
`format-check` failure source as `packages.config`, and `spec.md` names only `packages.config`.
Adding only `packages.config` to `.csharpierignore` will leave `format-check` red.

**(c) Reference-identity churn.** `[OBS-REMOTE]` the same hunk adds `<Private>True</Private>` and
drops `processorArchitecture=MSIL` from the `Reference Include` attribute. This is the fingerprint
of `MSBuildProjectSystem.AddReference` regenerating the element rather than patching a version
string — see R2. It is churn, not a build failure, but any verifier that diffs `.csproj` shape
must tolerate it.

**(d) Manifest/`.csproj` drift that blocks `nuget update` entirely.** `[OBS-LOCAL]` Issue #903 is
confirmed: `ToDoModel.Test\ToDoModel.Test.csproj` lines 92-96 carry `<HintPath>` entries for
`Deedle.3.0.0` and `FSharp.Core.11.0.100`, but `ToDoModel.Test\packages.config` contains no entry
for either (grep for `FSharp\.Core|Deedle` across `ToDoModel.Test\` returns only the `.csproj`,
the `app.config` binding redirect, and two test sources). `nuget update <packages.config> -Id X`
cannot update a package that is not in that manifest, so this project's references are frozen and
only resolve because a sibling project restores those packages into the shared `packages\` folder.

### 2.3 The existing repair script

`[OBS-LOCAL]` `scripts/vscode/Sync-PackageReferences.ps1` (160 lines) is the only existing
`.csproj` repair logic. Two properties matter for the design:

- Its `$hintPathRegex` is `<HintPath>(\.\.\\packages\\(.+?)\\lib\\([^\\]+)\\([^<]+))</HintPath>`
  — hard-anchored on `\lib\`. `<Analyzer Include>` paths live under `\analyzers\`, so this script
  **cannot** repair analyzer items even in principle. It is not a starting point for the analyzer
  post-pass.
- Line 18 of `$tfmPreference` ranks `netstandard2.1` above `netstandard2.0`. This is issue #902.
  `net481` cannot consume `netstandard2.1` at all, so this entry must be deleted, not demoted.

It is a plain script (`param(...)`, no `CmdletBinding`), which will need restructuring if it is
to satisfy `.claude/rules/powershell.md` (advanced functions, wrapper seams, Pester coverage).

---

## 3. R1 — Will the upgrade PR carry the required checks?

### 3.1 (a) Does the suppression apply?

**Yes, to `push`. Partially, to `pull_request`.**

`[DOC]` GitHub Actions documentation, *Triggering a workflow → Triggering a workflow from a
workflow*. Verbatim:

> "When you use the repository's `GITHUB_TOKEN` to perform tasks, events triggered by the
> `GITHUB_TOKEN` will not create a new workflow run, with the following exceptions:"

Exception 1, verbatim: `workflow_dispatch` and `repository_dispatch` events "always create
workflow runs."

Exception 2, verbatim:

> "`pull_request` events with the `opened`, `synchronize`, or `reopened` activity types: when a
> workflow using `GITHUB_TOKEN` creates or updates a pull request, the resulting `pull_request`
> event creates workflow runs in an **approval-required** state. The pull request displays a
> banner in the merge box, and a user with write access to the repository can start the runs by
> selecting **Approve workflows to run**."

Confidence: **high**. Extracted three times with differently-phrased prompts against the rendered
page, returning the same text each time, and corroborated independently from the page's markdown
source on `raw.githubusercontent.com`, which states that a GitHub App or PAT "also allows
`pull_request` workflows to run automatically without approval prompts when pull requests are
created or updated by automation" — the same behaviour described from the other side.

Applied to this repository:

- **`push` on the bot branch — dead twice over.** Suppressed by the `GITHUB_TOKEN` rule, and
  `ci.yml`'s `push` filter is `[main, development]`, which a feature branch would not match
  regardless.
- **`pull_request` on the bot-created PR — created, but parked.** `ci.yml` triggers on
  `pull_request: [main, development]` with default activity types (`opened`, `synchronize`,
  `reopened`), so all five checks *are* created against the PR head SHA. They sit in
  approval-required state until a repository writer clicks **Approve workflows to run**.

So the brief's worst case — "the upgrade PR would carry **zero** required checks and could never
satisfy `strict_required_status_checks_policy`" — is **not** what happens. The accurate statement
is: the PR carries all five required checks, and they do not start until a human approves them
once. The design is not invalidated; it acquires a recurring one-click human dependency.

`[DOC]` There is **no repository setting that removes this requirement.** The
"Approval for running fork pull request workflows from contributors" settings
(first-time contributors new to GitHub / first-time contributors / all external contributors)
govern fork PRs, not `GITHUB_TOKEN`-authored PRs. The `pull_request_target` carve-out
("workflows triggered by these events will always run, regardless of approval settings") does not
apply — `ci.yml` does not use `pull_request_target`, and adopting it for this purpose would be a
security regression. Confidence: **medium-high** (one source, read-only).

### 3.2 (b) Workarounds, mechanism, and cost

#### (ii) `workflow_dispatch` against `ci.yml` — **ELIMINATED**

This is the critical sub-question in the brief, and the answer is decisive.

`[DOC]` GitHub documentation, *Troubleshooting required status checks*, under the heading
**"Checks from some workflow jobs are not evaluated"**, verbatim:

> "For checks created by workflow jobs to be evaluated for a pull request, the workflow run must
> be triggered by one of these events:
> * `push`
> * `pull_request`
> * `pull_request_review`
> * `pull_request_target`
> * `deployment`
> * `deployment_status`"

and, verbatim:

> "For example, if a workflow is triggered by `workflow_dispatch` on a pull request's head branch,
> checks reported by its jobs do not appear in the pull request's checks section."

Confidence: **high**. Extracted twice with independent prompts; the second run returned the exact
heading and the literal `workflow_dispatch` example sentence.

The same source also states: "Required checks must pass on the latest commit SHA. Checks from
earlier commits don't satisfy the requirement," and "If a check and a commit status have the same
name, both must pass when that name is required."

`[INFER]` The structural evidence from the REST API pointed the other way and would have misled a
shallower reading: `[DOC]` the *List check runs for a Git reference* endpoint keys check runs on
`head_sha` + `name` + optional `app_id`, with **no** field identifying the triggering event. The
absence of an event field in the data model does not imply the evaluator ignores the event — the
evaluator applies an event allow-list that is not represented in the check-run payload. Option
(ii) is therefore eliminated on documentation that is specific, recent, and directly on point.

This also eliminates **(iv) `workflow_run` chaining** by the same rule: `workflow_run` is not in
the allow-list, so a `workflow_run`-triggered `ci.yml` produces checks that are not evaluated for
the PR.

#### (iii) Add the upgrade-branch pattern to `ci.yml`'s `push` filter — **ELIMINATED as stated**

`push` **is** in the allow-list, so on its face this works. It fails on the other constraint: a
push performed with `GITHUB_TOKEN` does not create a `push` workflow run at all. Widening the
filter changes nothing.

It becomes viable only if the branch is pushed with a **non-`GITHUB_TOKEN` credential**. The
cheapest such credential is an SSH **deploy key** with write access — `[DOC]`
`peter-evans/create-pull-request` documents deploy keys as the workaround that triggers
`on: push` workflows only. Cost: a one-time human setup (generate keypair, add the public key as
a repo deploy key with write access, store the private key as a secret), plus the fact that a
deploy key cannot open a pull request, so PR creation still needs `GITHUB_TOKEN` (permitted:
`can_approve_pull_request_reviews: true`). The result is a PR whose checks came from the `push`
event — allowed — but whose `pull_request` runs sit unapproved and, being required by name, would
themselves block. `[INFER]` This mixed state is fragile and not recommended: two runs of the same
check name on the same SHA, one green from `push` and one pending from `pull_request`, and the
documentation says same-named checks must both pass. Confidence: medium — the same-name rule is
quoted, but the specific interaction was not observed.

#### (i) Fine-grained PAT or GitHub App installation token — **RECOMMENDED**

Mechanism: the upgrade workflow authenticates PR creation (and the branch push) with a token that
is not `GITHUB_TOKEN`. The recursion suppression is keyed on the token, so the `pull_request`
`opened` event fires normally, un-parked. `ci.yml`'s existing `pull_request: [main, development]`
trigger fires, all five checks run on the PR head SHA under the `pull_request` event — which is in
the allow-list — and they are evaluated for the ruleset.

`[DOC]` Verbatim from the GitHub Actions documentation:

> "If you do want to trigger a workflow from within a workflow run, you can use a GitHub App
> installation access token or a personal access token instead of `GITHUB_TOKEN` to trigger events
> that require a token."

Cost, compared against each other:

| | Fine-grained PAT | GitHub App installation token |
|---|---|---|
| Setup | Create PAT scoped to this repo with Contents: write, Pull requests: write; store as a secret | Create App, grant Contents: write + Pull requests: write, install on repo, store App ID + private key as secrets, mint token per run via `actions/create-github-app-token` |
| Expiry | Fine-grained PATs expire (max 1 year); silent breakage at expiry | Installation tokens are minted per run and expire in 1 hour; the App private key does not expire |
| Attribution | Acts as the human owner; their name is on every upgrade PR | Acts as the App; clean bot attribution |
| Blast radius | Tied to a human account's permissions | Scoped to the App's declared permissions |
| Extra action dependency | None | `actions/create-github-app-token` (allowed: `allowed_actions: all`) |

**Recommendation: GitHub App installation token.** The deciding factor is expiry. A PAT converts
this workflow into a thing that silently stops working on a date nobody has written down; the App
private key does not expire and the per-run token does. Attribution and least-privilege both also
favour the App. The extra action dependency is acceptable given `allowed_actions: all`.

#### Interaction with `strict_required_status_checks_policy: true`

`[DOC]` "The topic branch **must** be up to date with the base branch before merging." `[INFER]`
This is an ongoing obligation, not a one-time one: if `main` advances after the upgrade PR is
opened, the PR goes stale and needs a branch update, which produces a new head SHA and a new round
of checks. Two consequences for the design:

1. The upgrade workflow must cut its branch from the **current** `origin/main` immediately before
   pushing, not from a cached checkout.
2. The workflow should enable auto-merge (`gh pr merge --auto --squash`) so the PR lands as soon as
   the five checks go green, minimising the window in which `main` can move. If it does go stale,
   recovery is `gh pr update-branch`, which — performed with the App token — produces a
   `synchronize` event that re-runs the checks un-parked.

### 3.3 (c) Unattended capability

| Option | Unattended after setup? | Human interaction required |
|---|---|---|
| GitHub App token | **Yes** | One-time: create App, install, store two secrets |
| Fine-grained PAT | Yes until expiry | One-time: create PAT, store secret. Recurring: rotate before expiry |
| `GITHUB_TOKEN` only | **No** | Recurring: click "Approve workflows to run" on every upgrade PR |
| Deploy key + `push` filter | Partially | One-time: generate and install key. Plus unresolved same-name-check conflict |
| `workflow_dispatch` | n/a | Eliminated — checks not evaluated |
| `workflow_run` | n/a | Eliminated — checks not evaluated |

**There is no option with zero human setup.** Every route that produces evaluated checks on an
automation-created PR requires either a credential a human must mint and store, or a click a human
must make. This is classified in § Automation Feasibility.

### 3.4 Secondary R1 finding: "Dependabot detect-only" is not a Dependabot mode

`[INFER]` The settled design says "Dependabot detects only." Dependabot has no detect-only mode for
version updates: it always opens a pull request containing its edits. With
`open-pull-requests-limit: 1` it will open one red PR per cycle, which the upgrade workflow must
consume (read the target versions from the branch diff or the PR body) and then close. Closing a
Dependabot PR without merging suppresses re-creation of that same version bump, which is the
desired behaviour. This consumption-and-close step is not in the spec's design summary and needs
to be added. Inputs: `.github/dependabot.yml` has no detect-only key `[OBS-LOCAL]`; PR #908 and
the branch list show PRs are always opened with edits `[OBS-REMOTE]`.

---

## 4. R2 — Does `nuget update` add a `<Reference>` for a newly added assembly?

**This could not be executed here.** No shell tool was available (§ 0), no `packages\` folder
exists in this worktree (`Glob packages/Meziantou.Analyzer.*/**/*.dll` → no files), and neither
`nuget.exe` nor `msbuild` could be located or invoked. The question is therefore settled by
documentation, source, and an observed artefact of the command's real output on this repository.

### 4.1 The documentation says no

`[DOC]` `learn.microsoft.com/en-us/nuget/reference/cli-reference/cli-ref-update`, verbatim:

> "The `update` command also updates assembly references in the project file, provided those
> references already exist. If an updated package has an added assembly, a new reference is *not*
> added. New package dependencies also don't have their assembly references added. To include
> these operations as part of an update, update the package in Visual Studio using the Package
> Manager UI or the Package Manager Console."

Note the page metadata: `ms.date: 2017-12-07`, `updated_at: 2021-06-17`. This text predates
nothing relevant in NuGet 6.x/7.x but is itself eight years old.

### 4.2 The source says yes

`[SRC]` `NuGet.Client/src/NuGet.Core/NuGet.PackageManagement/Projects/MSBuildNuGetProject.cs`,
`InstallPackageAsync`:

```csharp
if (!IsSkipAssemblyReferences(nuGetProjectContext) &&
    MSBuildNuGetProjectSystemUtility.IsValid(compatibleReferenceItemsGroup))
{
    foreach (var referenceItem in compatibleReferenceItemsGroup.Items)
    {
        if (IsAssemblyReference(referenceItem))
        {
            var referenceItemFullPath = Path.Combine(packageInstallPath, referenceItem);
            var referenceName = Path.GetFileName(referenceItem);

            if (await ProjectSystem.ReferenceExistsAsync(referenceName))
            {
                await ProjectSystem.RemoveReferenceAsync(referenceName);
            }

            await ProjectSystem.AddReferenceAsync(referenceItemFullPath);
        }
    }
}
```

This iterates **every** compatible reference item in the new package and adds each one. There is no
"only if it already existed" condition — the `ReferenceExistsAsync` check exists to remove a stale
element before re-adding, not to skip new ones. The guard:

```csharp
private static bool IsSkipAssemblyReferences(INuGetProjectContext nuGetProjectContext)
{
    var msBuildNuGetProjectContext = nuGetProjectContext as IMSBuildNuGetProjectContext;
    return msBuildNuGetProjectContext != null &&
           msBuildNuGetProjectContext.SkipAssemblyReferences;
}
```

returns true only when the context explicitly opts out; `nuget.exe update` does not set it.
`[SRC]` `UpdateCommand.cs` reaches this code by calling
`packageManager.PreviewUpdatePackagesAsync(...)` then
`packageManager.ExecuteNuGetProjectActionsAsync(...)` against an `MSBuildNuGetProject`, i.e. the
update is executed as uninstall actions followed by install actions.

### 4.3 The observed artefact breaks the tie

`[OBS-REMOTE]` Because Dependabot *is* `nuget update` on this repository (§ 1.2), PR #908 is an
observation of this command's real output here. In it:

- `<Private>True</Private>` is **added** to a reference that did not have it;
- `processorArchitecture=MSIL` is **dropped** from the `Reference Include` attribute value.

`[INFER]` A code path that merely rewrites version substrings in an existing element cannot
produce either change. Both are the signature of the element being removed and regenerated by
`MSBuildProjectSystem.AddReference` from the assembly's own identity. The regeneration loop is the
one quoted in § 4.2, and that loop necessarily emits an element for an assembly that was not
previously referenced.

### 4.4 Recommendation and confidence

**Recommended reading: the optimistic one. `nuget update` does add a `<Reference>` for a
newly-added assembly. The 2017-era documentation note describes pre-3.x behaviour and is stale.**

**Confidence: medium-high.** The implementation is unambiguous and the observed diff corroborates
that the regenerating path is what actually runs in this repository. It falls short of high
because no case was observed in which the new package version genuinely introduced an assembly the
old version lacked — the corroboration is of the mechanism, not of the specific outcome.

**A third post-pass is therefore not required.** However, because the confidence is not high and
the failure mode (a missing `<Reference>`) is silent until compile time, the plan should include a
**reference-completeness check inside the verifier** rather than a post-pass: for each package in
each manifest, resolve the compatible `lib\` assets on disk and assert a `<Reference>` with a
matching `<HintPath>` exists. This is cheap, idempotent, pure over parsed state, and it makes the
optimistic reading falsifiable on the first run instead of load-bearing.

Explicit assumption: that `dependabot-core` pins a NuGet.Client version whose
`MSBuildNuGetProject` matches the `dev`-branch source read here. Not verified. If Dependabot
pinned a much older NuGet, § 4.3's corroboration weakens but § 4.2's source reading still applies
to whatever version the workflow itself pins (R5).

---

## 5. R3 — Dependabot YAML for exactly one PR per cycle

### 5.1 What the current configuration actually does

`[OBS-LOCAL]` `.github/dependabot.yml` declares one `nuget` block with `directories: ["/*"]`,
`open-pull-requests-limit: 10`, four groups, and eight `ignore` entries scoped to
`version-update:semver-major`.

Two facts settle the consolidation question:

- `[DOC]` The Dependabot options reference states, of groups: "All updates for dependencies that
  match a rule are combined in a single pull request," and specifically about directories: "If
  directories have incompatible version constraints for a dependency, Dependabot will create
  separate pull requests." That exception only makes sense if the rule is consolidation.
- `[OBS-REMOTE]` PR #908 is one pull request spanning 10 project directories (claim N3). This is
  direct observation of consolidation on this repository, not a documentation reading.

**Answer: a single group with a catch-all pattern combined with the multi-directory key does
consolidate across directories.** Dependabot does not split per directory. The current
configuration produces four PRs because it declares four groups, not because of the glob.

### 5.2 The `group-by` key is not real

`[OBS-LOCAL]` All four groups carry `group-by: "dependency-name"`.

`[DOC]` The Dependabot options reference enumerates the group keys as `applies-to`,
`dependency-type`, `patterns`, `exclude-patterns`, and `update-types`. A separate documentation
fetch, prompted with a leading question ("Does `group-by` exist as a valid key?"), answered yes and
produced an example — `[INFER]` this is very likely small-model confirmation bias, because the
enumerated key list from the reference page does not contain it.

The observed behaviour settles it: if `group-by: dependency-name` meant "one PR per dependency
name," `open-pull-requests-limit: 10` would be saturated every cycle. `[OBS-REMOTE]` PR #908 bundles
11 dependency updates in one PR. The key is having no effect. Remove it.

Confidence: **medium-high**. The key's inertness is observed; its formal invalidity is inferred
from an enumeration rather than from a schema.

### 5.3 Recommended YAML

Collapse the four groups to one catch-all group, drop `group-by`, set the limit to 1, add Deedle
to `ignore`, and keep the eight major-version ignores.

```yaml
version: 2
updates:
  - package-ecosystem: "nuget"
    directories:
      - "/*"
    schedule:
      interval: "weekly"
    # Detection only. The upgrade is performed by .github/workflows/nuget-upgrade.yml,
    # which consumes this pull request's target versions and then closes it. One PR per
    # cycle keeps that consumption step unambiguous. See issue #911.
    open-pull-requests-limit: 1
    groups:
      all-nuget-dependencies:
        applies-to: version-updates
        patterns:
          - "*"
    ignore:
      # Deedle 8.x targets net10.0 only and ships no netstandard2.0 asset; net481
      # cannot consume it at any version above the pinned 3.0.0. See issue #911.
      - dependency-name: "Deedle"
      # Major-version bumps for Microsoft's .NET-runtime-aligned package families are
      # the only observed point at which supported TFMs (net462/netstandard2.0) have
      # historically changed; gate major bumps behind manual review rather than
      # guessing an unverified version-ceiling number.
      - dependency-name: "Microsoft.Extensions.*"
        update-types: ["version-update:semver-major"]
      - dependency-name: "Microsoft.Bcl.*"
        update-types: ["version-update:semver-major"]
      - dependency-name: "System.Text.Json"
        update-types: ["version-update:semver-major"]
      - dependency-name: "System.Drawing.Common"
        update-types: ["version-update:semver-major"]
      - dependency-name: "Microsoft.Graph*"
        update-types: ["version-update:semver-major"]
      - dependency-name: "Apache.Arrow*"
        update-types: ["version-update:semver-major"]
      - dependency-name: "Microsoft.Data.Analysis"
        update-types: ["version-update:semver-major"]
      - dependency-name: "Microsoft.ML*"
        update-types: ["version-update:semver-major"]
```

Notes on the choices:

- `applies-to: version-updates` is declared explicitly. Without it the group would also claim
  security updates, and a security update that the upgrade workflow's compatibility gate skips
  would then have no independent PR. Keeping security updates ungrouped preserves that escape
  hatch. `[DOC]` `applies-to` is a documented group key; confidence medium-high.
- `directories: ["/*"]` is retained unchanged. `[OBS-LOCAL]` It demonstrably matches the project
  directories today — PR #908 touched 10 of them — so the concern carried over from issue #340
  about whether `*` is single-segment is now answered empirically for this repository.
- `ignore` with a bare `dependency-name` and no `versions`/`update-types` ignores the dependency
  entirely. `[DOC]` "Ignore updates for dependencies with matching names."
- `[INFER]` One residual risk remains documented rather than eliminated: the "incompatible version
  constraints" carve-out could still split the PR if two directories pin genuinely irreconcilable
  versions of one package. `[OBS-LOCAL]` The repository is currently consistent (for example every
  manifest pins Meziantou.Analyzer 3.0.235), so this should not fire. The upgrade workflow must
  nonetheless tolerate finding more than one Dependabot PR and consume all of them.

---

## 6. R4 — Can `install`/`restore` reproduce `<Analyzer Include>`?

**No. The post-pass must synthesize them.**

### 6.1 Why the CLI can never do it

`[SRC]` `MSBuildNuGetProject.cs` contains **no** analyzer handling whatsoever — no
`compatibleAnalyzersGroup`, no `AddAnalyzer`, no handling of the `analyzers/` folder. It processes
lib items, references, framework references, content files, build files (`AddImport`), and tool
items. Confidence: high; this was the explicit subject of a targeted search of the file.

`[INFER]` The consequences are exhaustive across the CLI's verbs:

- `nuget restore` never opens a `.csproj` for writing at all; it materialises packages into
  `packages\`.
- `nuget install` installs into a folder, not a project; it has no project system.
- `nuget update` does have a project system (`MSBuildProjectSystem`) but, per the source above,
  never asks it to add an analyzer.

Analyzer items in `packages.config` projects are written only by Visual Studio's project system.
`[SRC]` `MSBuildNuGetProject.InstallPackageAsync` does call
`ProjectServices.ScriptService.ExecutePackageScriptAsync(...)` for `install.ps1`, but under
`nuget.exe` there is no EnvDTE host for such a script to drive.

This is fully consistent with the observed repository state: manifests, `<Import>` and `<Error>`
all at Meziantou.Analyzer 3.0.235, `<Analyzer Include>` alone at 3.0.203 (§ 2.2a).

### 6.2 The mapping is not a single convention — an important trap

The obvious algorithm — `..\packages\<Id>.<Version>\analyzers\dotnet\cs\<Id>.dll` — is **wrong for
three of the five analyzer families in this repository**. `[OBS-LOCAL]`
`UtilitiesCS\UtilitiesCS.csproj` lines 1308-1317:

```xml
<Analyzer Include="..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll" />
<Analyzer Include="..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll" />
<Analyzer Include="..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Common.dll" />
<Analyzer Include="..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Core.dll" />
<Analyzer Include="..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.CSharp.dll" />
<Analyzer Include="..\packages\AsyncFixer.2.1.0\analyzers\dotnet\cs\AsyncFixer.dll" />
<Analyzer Include="..\packages\Microsoft.CodeAnalysis.BannedApiAnalyzers.5.6.0\analyzers\dotnet\cs\Microsoft.CodeAnalysis.BannedApiAnalyzers.dll" />
<Analyzer Include="..\packages\Microsoft.CodeAnalysis.BannedApiAnalyzers.5.6.0\analyzers\dotnet\cs\Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers.dll" />
<Analyzer Include="..\packages\SonarAnalyzer.CSharp.10.34.0.3385\analyzers\SonarAnalyzer.CSharp.dll" />
```

Four distinct shapes are present:

| Family | Shape | Assemblies |
|---|---|---|
| `AsyncFixer`, `Microsoft.CodeAnalysis.BannedApiAnalyzers`, `MSTest.Analyzers` | `analyzers\dotnet\cs\` | 1, 2, 2 |
| `Meziantou.Analyzer` | `analyzers\dotnet\roslyn5.0\cs\` — Roslyn-version-qualified | 1 |
| `Roslynator.Analyzers` | `analyzers\dotnet\roslyn4.7\cs\` — different Roslyn version, mangled assembly names | 4 |
| `SonarAnalyzer.CSharp` | `analyzers\` — no `dotnet`, no language folder | 1 |

`[INFER]` A synthesizer that assumes `analyzers\dotnet\cs\<Id>.dll` would produce eight wrong
paths in this one project. Worse, the Roslyn-version folder is not a fixed string: the correct
folder depends on which `roslynNN` subfolders the *new* package version ships and which the build's
Roslyn supports, and that can change on a version bump — which is exactly when the post-pass runs.

### 6.3 The algorithm the post-pass must use

The only sound approach is **enumerate the restored package on disk**; do not pattern-match from
the id. Because the workflow runs `nuget restore` before `nuget update` anyway, the new version's
content is already materialised under `packages\<Id>.<Version>\` when the post-pass runs.

For each project directory `P` with a `packages.config`:

1. Parse `P\packages.config` into `{id → version}`.
2. Compute the current `<Analyzer Include>` set `A_old` from `P\<project>.csproj`, and derive from
   each path the owning `<Id>.<Version>` folder segment.
3. For each package `(id, version)` in the manifest:
   a. Let `root = packages\<id>.<version>\analyzers`. If it does not exist, the package ships no
      analyzers — contribute nothing.
   b. Enumerate candidate asset directories under `root`, recursively, retaining only those whose
      terminal path is one of:
      - `<root>\dotnet\<roslynFolder>\cs`
      - `<root>\dotnet\cs`
      - `<root>\cs`
      - `<root>` itself
      where `<roslynFolder>` matches `^roslyn\d+(\.\d+)*$`.
   c. Discard any candidate whose path contains a language folder other than `cs`
      (`vb`, `fs`), and any under a `tools\` or `build\` sibling.
   d. Among surviving candidates that differ only by `<roslynFolder>`, select the **highest**
      `roslynFolder` version that does not exceed the Roslyn version of the MSBuild in use.
      Prefer the plain `dotnet\cs` (unversioned) form only when no versioned folder qualifies.
   e. Emit one `<Analyzer Include>` per `*.dll` directly in the selected directory, excluding
      resource satellite assemblies (`*.resources.dll`) and any file under a culture subfolder.
   f. Render each path relative to `P` as `..\packages\<id>.<version>\analyzers\...`, using
      backslash separators to match the existing file.
4. Replace the `<ItemGroup>` containing `A_old` with the computed set, **preserving**:
   - the existing explanatory comment (`<!-- Issue #181: analyzer-only references ... -->`), and
   - the `<AdditionalFiles Include="$(MSBuildThisFileDirectory)..\BannedSymbols.txt" />` element,
     which `[OBS-LOCAL]` lives inside the same `ItemGroup` (line 1316) and is **not** an analyzer
     item. Losing it would silently disable `BannedApiAnalyzers`.
5. Emit a per-project report of added/removed/retargeted paths for the PR body's "Repairs applied"
   block.

Two properties make this testable without the network, which matters for the Pester coverage
requirement: steps 1-3 are a pure function of `(manifest, directory listing)`, and step 4 is a pure
function of `(csproj text, computed set)`. The directory listing is the only I/O and can be
injected as a delegate per `.claude/rules/powershell.md` § Design Seams.

Explicit assumption: that the Roslyn version of the MSBuild on `windows-latest` is >= the highest
`roslynNN` folder any of these five packages ships, making step 3(d) a simple "pick the highest."
Not verified. If it is not, 3(d) needs a real MSBuild-version-to-Roslyn-version table, and the
verifier will catch the mistake as a `CS0006`/analyzer-load failure in `build-analyzers`.

---

## 7. R5 — Pinning the NuGet CLI version

### 7.1 Current state

`[OBS-LOCAL]` Three workflows carry `nuget-version: latest` at line 33:
`_build-analyzers.yml`, `_build-nullable.yml`, `_mstest-coverage.yml`. All three use
`nuget/setup-nuget@v2`. `_format-check.yml` and `_actionlint.yml` use no NuGet.

### 7.2 What `latest` resolves to

`[DOC]` `nuget/setup-nuget` accepts `latest` ("the latest blessed NuGet release"), `preview`,
an exact `X.Y.Z`, or a semver range.

`[DOC]` `https://dist.nuget.org/tools.json`, most recent `ReleasedAndBlessed` entries:

| Version | Release date |
|---|---|
| **7.9.0** | 2026-08-11 |
| 7.6.0 | 2026-05-12 |
| 7.3.1 | 2026-04-14 |
| 7.3.0 | 2026-02-10 |
| 7.0.3 | 2026-04-14 |
| 6.14.3 | 2026-04-14 |

So `latest` resolves to **7.9.0** today, and has since 2026-08-11.

### 7.3 Recommendation: pin `7.9.0`

```yaml
      - name: Setup NuGet
        uses: nuget/setup-nuget@v2
        with:
          # Pinned, not floating. `latest` resolves to the newest ReleasedAndBlessed entry in
          # dist.nuget.org/tools.json and would silently change the tool that writes .csproj and
          # app.config during a dependency upgrade. 7.9.0 is what `latest` resolved to when this
          # pin was taken (released 2026-08-11), so the pin freezes current behaviour rather than
          # changing it. See issue #911.
          nuget-version: '7.9.0'
```

How the version was chosen:

1. **Freeze observed behaviour, do not change it.** CI has been green on `latest` since 7.9.0
   shipped on 2026-08-11. Pinning 7.9.0 is the only choice with a zero behaviour delta. Pinning
   6.14.3 "for conservatism" would be an untested downgrade introduced by a change whose purpose
   is to reduce variance.
2. **`update` and `packages.config` survive in 7.x.** `[DOC]` The NuGet 7.0 release notes list the
   breaking changes: package-id validation during restore, removal of `project.json`, package
   pruning for .NET 10, SHA-1 fingerprint errors in `sign`/`mssign`, HTTPS enforcement for service
   index resources, plus SDK API removals. None touches `packages.config` or `update`. Community
   PRs in the same release (`6641` "remove redundant null condition in
   `UpdateCommand.ExecuteCommandAsync`", `6600` "enable nullable in `NuGetUpdateCommandTests`")
   confirm the command is still present and maintained.
3. **7.x fixes MSBuild discovery bugs that this design depends on.** The workflow will pass
   `-MSBuildPath`. `[DOC]` NuGet 7.0 fixed "`nuget.exe` restore finding MSBuild from SSMS instead
   of Visual Studio" (#6530 / #14349) and NuGet 7.9 fixed "nuget.exe restore `-MSBuildPath` crashes
   when pointing it to .NET SDK directory" (#14844). Pinning below 7.9.0 reintroduces the second
   of these.
4. **7.9.0 introduces nothing that threatens this path.** `[DOC]` Its breaking changes are
   `SearchFilter.PackageTypes` → `PackageType`, nullable annotations in `NuGet.Protocol`, a
   `monoandroid` TFM deprecation warning, and a `dotnet nuget why` output change. All are SDK/CLI
   surfaces this repository does not use.
5. **Exact version, not a range.** `'7.x'` would reintroduce the float one level up. The point of
   the pin is that the tool which rewrites `.csproj` and `app.config` is a known quantity for a
   given commit.

Apply the same pin in all four places: the three existing workflows and the new upgrade workflow.
Divergence between the version used to upgrade and the version used to build would make the
verifier's judgement unreliable.

Secondary, non-blocking observation: `[DOC]` `nuget/setup-nuget@v3` exists; the repository is on
`@v2`. This is outside the scope of #911 and should not be bundled into it.

---

## 8. Recommended approach

### 8.1 The recommendation

**Adopt the settled design (Dependabot detects, a new workflow performs the upgrade), with four
amendments forced by the findings above.** The design survives, but its justification changes and
two of its steps must be re-specified.

Why it survives even though Dependabot already runs `nuget update` (§ 1.2): running the command
ourselves is not what buys the improvement. What it buys is **control around** the command —
per-package compatibility gating with a recorded skip reason, an analyzer post-pass, a formatting
normalisation pass, a self-verify/self-repair step, and a single consolidated branch whose content
we own. None of those can be attached to a Dependabot branch, because Dependabot rebases and
force-pushes its branches and would clobber them.

The four amendments:

1. **Token.** Use a GitHub App installation token for the branch push and PR creation (R1). Without
   it the PR's checks are created but parked behind a manual approval click on every cycle.
2. **Consume-and-close.** Add an explicit step that reads the target versions from the Dependabot
   PR and closes it. "Detect-only" is not a Dependabot mode (§ 3.4).
3. **Formatting scope.** The formatting normalisation must cover `app.config` as well as
   `packages.config`; `app.config` is the larger reflow surface (§ 2.2b). Decide deliberately
   between (a) adding both to `.csharpierignore` and (b) running `csharpier format` as the last
   post-pass. **Recommend (b)**: `.csharpierignore` entries would permanently exempt two
   hand-maintained config classes from formatting, whereas running the formatter as a post-pass
   keeps them in scope and guarantees `format-check` parity by construction. The `.csharpierignore`
   change named in the spec's prerequisites should be dropped in favour of this.
4. **Third post-pass → verifier check.** Do not add a third post-pass for newly-added references
   (R2 resolves against needing one). Add a reference-completeness assertion to the verifier
   instead.

### 8.2 Proposed workflow shape

```
on: schedule (weekly, offset after the Dependabot cycle) + workflow_dispatch

permissions: contents: write, pull-requests: write

 1. Mint App installation token
 2. Checkout origin/main at current tip; create branch deps/nuget-upgrade-<runid>
 3. Setup MSBuild; Setup NuGet 7.9.0; restore solution
 4. Discover targets: find open Dependabot PR(s); parse target {id, version} set
 5. Compatibility gate (pure): for each target, resolve candidate assets; accept only
    if an asset exists that net481 can consume. netstandard2.1 excluded outright.
    Record skipped packages with reasons.
 6. For each accepted target, for each manifest containing that id:
       nuget update <P>\packages.config -Id <id> -Version <v>
                    -RepositoryPath packages -NonInteractive
                    -FileConflictAction Overwrite -MSBuildPath <dir>
 7. Post-pass A: synthesize <Analyzer Include> (§ 6.3)
 8. Post-pass B: binding redirects in app.config
 9. Post-pass C: dotnet tool run csharpier format .
10. Verifier: manifest/HintPath/Import/Error/Analyzer/redirect consistency
    + reference completeness. Repair freely; fail only if still inconsistent.
11. Commit, push with App token, open PR with App token
    - label deps:autofixed if a repair outside classes A/B was applied
    - body carries "Repairs applied" and "Packages skipped" blocks
12. Close the consumed Dependabot PR(s)
13. gh pr merge --auto --squash
```

Step 6 note: `-Version` applies only when exactly one `-Id` is supplied, so the loop must be
per-package. This matches Dependabot's own invocation shape (§ 1.2), which is evidence the shape
works against these projects.

Step 5 note: the gate is asset-level, not `<frameworkAssemblies>`-level. `net481` accepts
`net481`…`net20`, `netstandard2.0` and below. It does **not** accept `netstandard2.1`, which must
be excluded outright rather than ranked last — this is the same defect as #902 and the two must be
fixed with one shared TFM-ranking function, not two.

### 8.3 Rejected alternatives

- **Repair Dependabot's branch in place.** Cheapest on paper, since the `.csproj`/`app.config`
  work is already done (§ 1.1). Rejected: Dependabot rebases and force-pushes its branches, which
  destroys the repairs; and a `GITHUB_TOKEN` push to the branch produces no `synchronize` run
  (§ 3.1), so the red checks would never be re-evaluated.
- **`workflow_dispatch` against `ci.yml` for the bot branch.** Rejected on documentation that is
  specific and directly on point: `workflow_dispatch` is not in the event allow-list for check
  evaluation (§ 3.2). This was the design's stated fallback and it does not work.
- **`workflow_run` chaining.** Rejected for the same reason — not in the allow-list.
- **Deploy key + widened `push` filter on `ci.yml`.** Rejected: requires the same one-time human
  setup as the App token while additionally producing two runs of each required check name on the
  same SHA, one of which stays parked.
- **`pull_request_target` on `ci.yml`.** Rejected: a security regression for a convenience gain,
  and `[DOC]` GitHub is shipping a default protection restricting `pull_request_target` from
  2026-11-02.
- **Drop Dependabot and discover versions from the NuGet V3 API directly.** Attractive because it
  removes the red-PR consumption step entirely. Rejected for this change: it would require
  reimplementing the eight semver-major ignore rules and the security-advisory awareness that
  Dependabot provides. Worth revisiting if the consume-and-close step proves unreliable.

---

## 9. Behaviour semantics

### 9.1 Success conditions

- Exactly one Dependabot PR exists per cycle, and it is closed by the end of the upgrade run.
- Exactly one upgrade PR is opened per cycle, on a branch cut from the then-current `origin/main`.
- All five required checks are **created under the `pull_request` event** on the upgrade PR's head
  SHA and reach `success` without human edits.
- Every accepted package is at its target version in every manifest that contains it, and the
  `.csproj` `<Import>`, `<Error>`, `<Reference>`/`<HintPath>`, and `<Analyzer Include>` items and
  the `app.config` `<bindingRedirect>` entries all name that same version.
- Every skipped package is recorded in the PR body with a machine-derived reason.

### 9.2 Failure conditions

- A `nuget update` invocation exits non-zero → fail the run; do not open a partial PR.
- The verifier finds a residual inconsistency after repair → fail the run.
- No Dependabot PR is found → exit success with no PR (nothing to upgrade). This must not be an
  error; an empty cycle is the normal steady state.
- The App token cannot be minted → fail loudly. Falling back to `GITHUB_TOKEN` would silently
  produce a PR with parked checks, which is the failure mode this design exists to avoid.

### 9.3 Ordering rules

1. `restore` strictly before `update` — the documentation recommends it and Dependabot does it.
2. All `update` invocations before any post-pass. Post-pass A reads `packages\` contents that only
   exist after the updates complete.
3. Post-pass C (`csharpier format`) strictly last among the passes. A and B write XML; formatting
   them before they are written wastes the pass.
4. Verifier strictly after C — the verifier must judge the tree that will actually be committed.
5. Branch creation immediately before push, not at checkout, to minimise the strict-policy staleness
   window (§ 3.2).

### 9.4 Edge cases

- **A package appears in some manifests but not others.** Normal — loop per manifest, not per
  solution.
- **A package is in a `.csproj` `<HintPath>` but not in that project's manifest** (#903,
  `ToDoModel.Test` / `Deedle`, `FSharp.Core`). `nuget update` cannot touch it. The verifier must
  flag orphaned HintPaths as a distinct, named condition rather than silently passing.
- **A package ships no analyzers.** Post-pass A contributes nothing for it — must not emit an
  empty `<ItemGroup>` or delete the surrounding comment.
- **An analyzer package changes its Roslyn folder set across the bump.** Post-pass A must re-derive
  the folder from disk, never carry the old one forward (§ 6.2).
- **`SVGControl` has no analyzers at all** `[OBS-LOCAL]`. Post-pass A must not synthesize an
  `<ItemGroup>` into a project that never had one.
- **Dependabot splits despite the single group** (incompatible version constraints, § 5.3).
  Consume all open Dependabot PRs, not just the first.
- **`main` advances mid-run.** Auto-merge plus a branch cut at push time; `gh pr update-branch`
  with the App token as recovery.

---

## 10. Numeric Derivation Evidence

### Claim N1 — 162 `<Analyzer Include>` items across 17 project files

- **Complete Family:** every MSBuild `Analyzer` item element (any of the `Include`, `Update`, or
  `Remove` attribute forms) in every MSBuild project or import file in the working tree
  (`*.csproj`, `*.vbproj`, `*.props`, `*.targets`), excluding prose in Markdown/txt documentation
  and evidence artefacts.
- **Exhaustive Search Scope:** the entire working tree
  `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-09-12T10-15`, unfiltered by path, with
  `head_limit: 0` so no result was truncated. Both records cover all three attribute forms, not
  only `Include`.
- **Inclusion Rules:** any occurrence of an `Analyzer` item element inside a file that MSBuild
  evaluates as part of a project build.
- **Exclusion Rules:** occurrences inside `docs/**`, `.claude/**`, `**/evidence/**` and any `.md`
  or `.txt` file — these are prose quotations of project XML, not project XML.
- **Primary Search Strategy or Query Expression:** literal substring search,
  `Grep pattern="Analyzer Include"`, `glob="*.csproj"`, `output_mode="count"`, `head_limit=0`.
  Constrains the file set by glob; matches the attribute name loosely.
- **Primary Member Set:** `VBFunctions.Test\VBFunctions.Test.csproj` 11,
  `TaskVisualization.Test\TaskVisualization.Test.csproj` 11, `VBFunctions\VBFunctions.csproj` 9,
  `ToDoModel.Test\ToDoModel.Test.csproj` 11, `SVGControl.Test\SVGControl.Test.csproj` 2,
  `TaskMaster.Test\TaskMaster.Test.csproj` 11, `TaskTree\TaskTree.csproj` 9,
  `TaskTree.Test\TaskTree.Test.csproj` 11, `TaskVisualization\TaskVisualization.csproj` 9,
  `UtilitiesCS.Test\UtilitiesCS.Test.csproj` 11, `UtilitiesCS\UtilitiesCS.csproj` 9,
  `TaskMaster\TaskMaster.csproj` 9, `QuickFiler\QuickFiler.csproj` 9,
  `ToDoModel\ToDoModel.csproj` 9, `Tags.Test\Tags.Test.csproj` 11,
  `QuickFiler.Test\QuickFiler.Test.csproj` 11, `Tags\Tags.csproj` 9.
- **Primary Count:** **162**, across **17** files.
- **Cross-check Search Strategy or Query Expression:** XML-element-anchored regular expression over
  the **whole tree with no glob filter**, covering all three attribute forms and tolerating
  arbitrary whitespace: `Grep pattern="<Analyzer\s+(Include|Update|Remove)="`,
  `output_mode="count"`, `head_limit=0`. This is a different strategy (element-anchored regex,
  unfiltered scope, full attribute-form coverage) rather than a re-run of the primary.
- **Cross-check Member Set:** the same 17 `.csproj` paths with the same 17 per-file counts, plus
  17 non-project files which the Exclusion Rules remove:
  `docs\features\potential\promoted\2026-09-14-…md` 1,
  `docs\features\potential\promoted\2026-08-23-…md` 5, `.claude\rules\csharp.md` 1,
  `docs\features\active\…\preflight-round-5.2026-08-29T04-10.md` 1,
  `docs\features\active\…-817\plan.2026-09-08T23-50.md` 1,
  `docs\features\active\…-501\evidence\baseline\bootstrap-analyzer-backfill.2026-08-27T19-56.md` 5,
  `docs\features\active\…-511\evidence\baseline\analyzer-package-backfill.2026-08-21T18-10.md` 5,
  `docs\features\active\…-796\evidence\baseline\p0-t4-analyzer-version-skew.md` 1,
  `docs\features\archive\…-181\plan.2026-06-08T12-12.md` 1,
  `.claude\agent-memory\atomic-planner\project_440_breadcrumb_left_arrow_plan_seams.md` 1,
  `docs\features\active\…-468\evidence\baseline\phase0-instructions-read.md` 1,
  `docs\features\active\…-468\evidence\baseline\p0-t8-analyzer-backfill.2026-08-26T08-25.md` 5,
  `docs\features\active\…-801-805-812\evidence\baseline\phase0-analyzers.2026-09-07T22-12.md` 1,
  `docs\features\active\…-784-787-788-809\plan.2026-09-07T20-14.md` 1,
  `docs\features\active\…-784-787-788-809\evidence\baseline\p0-t7-analyzer-parity.md` 1,
  `docs\features\archive\…-181\evidence\other\analyzer-dll-paths.2026-06-08T12-12.md` 10,
  `docs\features\archive\…-449\evidence\baseline\step1-dotnet-tool-restore.2026-08-22T09-16.md` 5.
  Raw total 208 across 34 files; 208 − 46 (documentation) = **162** across **17** project files.
- **Cross-check Count:** **162**, across **17** files.
- **Member-set Comparison:** After normalising the cross-check by the declared Exclusion Rules, the
  two member sets are **identical** — the same 17 file paths with identical per-file counts
  (8 files × 11, 8 files × 9, plus `SVGControl.Test` at 2; 88 + 72 + 2 = 162). The cross-check
  additionally establishes that **no** `.props`, `.targets`, or `.vbproj` file in the tree contains
  an `Analyzer` item, and that **no** `Update=` or `Remove=` attribute form exists anywhere, so the
  primary's narrower `*.csproj` / `Include`-only scope loses nothing. **Counts agree; assertion
  admitted.**

### Claim N2 — 15 stale `Meziantou.Analyzer.3.0.203` sites, all `<Analyzer Include>`

- **Complete Family:** every textual reference in a project file to the versioned package folder
  `Meziantou.Analyzer.<version>`, across all element kinds in which such a path can appear in this
  repository (`<Import Project=>`, `<Import Condition=>`, `<Error Condition=>`,
  `<Analyzer Include=>`).
- **Exhaustive Search Scope:** the entire working tree, `head_limit=0` on both records.
- **Inclusion Rules:** an occurrence inside a `.csproj` file.
- **Exclusion Rules:** occurrences in `docs/**`, `.claude/**`, `**/evidence/**`, and this feature's
  own `spec.md`/`issue.md` — those quote the figure rather than constitute it.
- **Primary Search Strategy or Query Expression:** version-agnostic, match-extracting regex,
  `Grep pattern="Meziantou\.Analyzer\.[0-9][0-9.]*"`, `glob="*.csproj"`, `output_mode="content"`,
  `-o=true`, `-n=true`, `head_limit=0`. This enumerates **every** version present, so the family
  is covered rather than one named version, and reports the line number of each occurrence.
- **Primary Member Set (the `3.0.203` members, one per file, by file:line):**
  `TaskTree\TaskTree.csproj:100`, `TaskVisualization.Test\TaskVisualization.Test.csproj:332`,
  `VBFunctions.Test\VBFunctions.Test.csproj:287`, `Tags.Test\Tags.Test.csproj:307`,
  `VBFunctions\VBFunctions.csproj:58`, `TaskVisualization\TaskVisualization.csproj:150`,
  `TaskMaster.Test\TaskMaster.Test.csproj:383`, `ToDoModel.Test\ToDoModel.Test.csproj:350`,
  `QuickFiler.Test\QuickFiler.Test.csproj:518`, `Tags\Tags.csproj:97`,
  `TaskTree.Test\TaskTree.Test.csproj:308`, `UtilitiesCS.Test\UtilitiesCS.Test.csproj:978`,
  `QuickFiler\QuickFiler.csproj:595`, `ToDoModel\ToDoModel.csproj:189`,
  `UtilitiesCS\UtilitiesCS.csproj:1308`.
  The same query's `3.0.235` members total 65 (15 files × 4 occurrences — two on the `<Import>`
  line, two on the `<Error>` line — plus 5 in `TaskMaster.csproj`, which additionally has its
  `<Analyzer Include>` already at 3.0.235).
- **Primary Count:** **15** stale sites (and 65 current sites, total 80).
- **Cross-check Search Strategy or Query Expression:** fixed-version literal search over the
  **whole tree with no glob filter**, `Grep pattern="Meziantou\.Analyzer\.3\.0\.203"`,
  `output_mode="count"`, `head_limit=0`. A different strategy (literal vs. version-agnostic regex,
  count vs. content, unfiltered vs. globbed scope).
- **Cross-check Member Set:** `VBFunctions.Test\VBFunctions.Test.csproj` 1,
  `VBFunctions\VBFunctions.csproj` 1, `UtilitiesCS.Test\UtilitiesCS.Test.csproj` 1,
  `Tags.Test\Tags.Test.csproj` 1, `Tags\Tags.csproj` 1, `QuickFiler.Test\QuickFiler.Test.csproj` 1,
  `QuickFiler\QuickFiler.csproj` 1, `UtilitiesCS\UtilitiesCS.csproj` 1,
  `ToDoModel.Test\ToDoModel.Test.csproj` 1, `ToDoModel\ToDoModel.csproj` 1,
  `TaskVisualization.Test\TaskVisualization.Test.csproj` 1,
  `TaskVisualization\TaskVisualization.csproj` 1, `TaskTree.Test\TaskTree.Test.csproj` 1,
  `TaskTree\TaskTree.csproj` 1, `TaskMaster.Test\TaskMaster.Test.csproj` 1 — 15 project files at
  1 occurrence each. All other hits are in `docs/**`, `.claude/**` or `**/evidence/**` and are
  removed by the Exclusion Rules.
- **Cross-check Count:** **15**.
- **Member-set Comparison:** The normalised member sets are **identical** — the same 15 `.csproj`
  paths, one occurrence each. `TaskMaster\TaskMaster.csproj`, `SVGControl\SVGControl.csproj` and
  `SVGControl.Test\SVGControl.Test.csproj` are absent from both, consistent with TaskMaster already
  being corrected and SVGControl* not consuming Meziantou. The primary additionally establishes
  that each stale site is the **only** Meziantou occurrence on its line while the 3.0.235 sites
  occur in pairs, which is what identifies the stale sites as `<Analyzer Include>` items (single
  path attribute) rather than `<Import>`/`<Error>` (path in both `Project`/`Condition`).
  **Counts agree; assertion admitted.**

### Claim N3 — Dependabot PR #908 is one PR spanning 10 project directories, 30 files

- **Complete Family:** every file path in the changed-files set of pull request #908 on
  `drmoisan/TaskMaster`.
- **Exhaustive Search Scope:** the full changed-file list of the PR, retrieved with `per_page=100`
  so no page boundary truncated the set (30 < 100).
- **Inclusion Rules:** any path reported as changed by the PR.
- **Exclusion Rules:** none — the family is the complete change set.
- **Primary Search Strategy or Query Expression:** REST API,
  `GET https://api.github.com/repos/drmoisan/TaskMaster/pulls/908/files?per_page=100`, machine
  JSON, enumerating `filename` per entry.
- **Primary Member Set:** `QuickFiler.Test/{QuickFiler.Test.csproj, app.config, packages.config}`,
  `SVGControl.Test/{SVGControl.Test.csproj, app.config, packages.config}`,
  `Tags.Test/{Tags.Test.csproj, app.config, packages.config}`,
  `TaskMaster.Test/{TaskMaster.Test.csproj, app.config, packages.config}`,
  `TaskTree.Test/{TaskTree.Test.csproj, app.config, packages.config}`,
  `TaskVisualization.Test/{TaskVisualization.Test.csproj, app.config, packages.config}`,
  `ToDoModel.Test/{ToDoModel.Test.csproj, app.config, packages.config}`,
  `UtilitiesCS.Test/{UtilitiesCS.Test.csproj, app.config, packages.config}`,
  `UtilitiesCS/{UtilitiesCS.csproj, app.config, packages.config}`,
  `VBFunctions.Test/{VBFunctions.Test.csproj, app.config, packages.config}`.
- **Primary Count:** **30** files; **10** distinct directories; per-extension 10 `.csproj`,
  10 `app.config`, 10 `packages.config`.
- **Cross-check Search Strategy or Query Expression:** rendered HTML diff page,
  `https://github.com/drmoisan/TaskMaster/pull/908/files`, counting distinct top-level project
  directories containing a changed `packages.config`. A different transport (HTML vs. JSON API), a
  different rendering, and a different counting unit (directories via `packages.config`, not file
  paths).
- **Cross-check Member Set:** QuickFiler.Test, SVGControl.Test, Tags.Test, TaskMaster.Test,
  TaskTree.Test, TaskVisualization.Test, ToDoModel.Test, UtilitiesCS.Test, UtilitiesCS,
  VBFunctions.Test.
- **Cross-check Count:** **10** distinct top-level project directories with a changed
  `packages.config`.
- **Member-set Comparison:** The 10 directories in the cross-check are **identical** to the 10
  distinct directory prefixes in the primary's 30 paths, in the same set. The cross-check's first
  pass reported "20 files" alongside "three changed files per directory," an internal
  inconsistency (10 × 3 = 30) that the primary's explicit enumeration resolves in favour of 30;
  the **directory** count, which is the load-bearing figure for the consolidation claim, agrees at
  10 in both records. **Directory counts agree; assertion admitted for the consolidation claim
  (10 directories in one PR). The file count 30 rests on the primary enumeration alone and is
  reported as such.**

### Claim N4 — 18 non-SDK projects, not 17

- **Complete Family:** every non-SDK-style project in the solution, identified by the co-location
  of a `.csproj` and a `packages.config` in the same directory.
- **Exhaustive Search Scope:** the whole working tree, recursive glob (`**/`), not depth-limited.
- **Inclusion Rules:** a directory containing both a `*.csproj` and a `packages.config`.
- **Exclusion Rules:** `packages.config` files under a restored `packages\` folder — none exist in
  this worktree (`Glob packages/**` returned no files).
- **Primary Search Strategy or Query Expression:** `Glob pattern="**/*.csproj"` — recursive,
  unconstrained depth.
- **Primary Member Set:** SVGControl.Test, SVGControl, Tags.Test, Tags, TaskTree.Test, TaskTree,
  TaskVisualization.Test, TaskVisualization, ToDoModel.Test, ToDoModel, VBFunctions.Test,
  VBFunctions, QuickFiler.Test, QuickFiler, TaskMaster.Test, TaskMaster, UtilitiesCS.Test,
  UtilitiesCS.
- **Primary Count:** **18**.
- **Cross-check Search Strategy or Query Expression:** `Glob pattern="**/packages.config"` — a
  different artefact type (manifest rather than project file), also recursive. Corroborated by a
  third, depth-constrained query `Glob pattern="*/*.csproj"` which returned the same 18 paths,
  establishing that no project sits at any other depth.
- **Cross-check Member Set:** the same 18 directory names, each contributing exactly one
  `packages.config`.
- **Cross-check Count:** **18**.
- **Member-set Comparison:** The two member sets are **identical** as directory sets. The figure
  17 used in the brief and `spec.md` matches instead the count of projects carrying
  `<Analyzer Include>` items (claim N1) — the difference is `SVGControl\SVGControl.csproj`, which
  has a `packages.config` but no analyzer items. **Counts agree; assertion admitted.**

---

## 11. Testing implications

No test code is proposed here; this is strategy only, consistent with
`.claude/rules/powershell.md` and `.claude/rules/general-unit-test.md`.

### 11.1 Pester unit tests — the pure cores

The spec already commits to Pester coverage for the compatibility evaluator and the verifier.
Three pure functions should be extracted and are the natural units:

1. **`Test-Net481AssetCompatibility`** — given a set of asset folder names, return the selected
   TFM or `$null`. Scenarios: `net481` exact; `net48` fallback; `netstandard2.0` accepted;
   `netstandard2.1` **rejected** (the #902 regression guard, and it must be a negative assertion,
   not a ranking assertion); `net481` + `netstandard2.1` both present, `net481` chosen;
   empty set; `netcoreapp`/`net6.0`-only set rejected. Note this function is shared with
   `Sync-PackageReferences.ps1`'s `$tfmPreference`; one implementation, not two.
2. **`Get-AnalyzerAssetPath`** — given a manifest entry and an injected directory listing,
   return the ordered `<Analyzer Include>` path set. Scenarios must cover all four shapes observed
   in § 6.2: plain `dotnet\cs`; Roslyn-versioned `dotnet\roslyn5.0\cs`; multi-assembly with
   mangled names (Roslynator, 4 DLLs); bare `analyzers\<dll>` (Sonar); a package with no
   `analyzers\` folder; two Roslyn folders present (highest supported chosen); `vb`/`fs` sibling
   folders present and excluded; a `*.resources.dll` present and excluded.
3. **`Test-ProjectConsistency`** — given parsed `(packages.config, csproj text, app.config text)`,
   return the set of inconsistencies. Scenarios: all-consistent; stale `<Analyzer Include>` (the
   #898 state); stale `<HintPath>`; stale `<Import>`/`<Error>`; stale `<bindingRedirect>`;
   orphaned `<HintPath>` with no manifest entry (the #903 state); a `<Reference>` missing for a
   restored `lib\` asset (the R2 guard).

The directory listing must enter these functions through an injected delegate, per
`.claude/rules/powershell.md` § Design Seams, so no test touches the filesystem. **No temporary
files** may be created — fixtures are in-memory strings and hashtables.

### 11.2 What cannot be unit-tested, and what replaces it

`nuget update`'s own behaviour is out of scope for unit tests (external process, network). It is
covered by the spec's integration scenario: run the upgrade workflow against a deliberately stale
manifest and confirm the required checks pass on the produced branch. Two additions to that
scenario, driven by findings here:

- Confirm the produced PR's checks were created under the **`pull_request`** event, not
  `workflow_dispatch`, and that none are in an approval-required state. This is the direct test of
  R1 and it is the one that would catch a token misconfiguration. Inspect the check runs' source
  workflow run event.
- Confirm `main` builds from a **cold** NuGet cache after the #898 correction. The spec already
  lists this; it is the only way to observe the `CS0006` that the cache prefix fallback currently
  masks.

### 11.3 Coverage obligations

New PowerShell modules must meet the >= 85% line threshold
(`.claude/rules/quality-tiers.md`). Branch coverage is not measurable by Pester and no branch gate
applies. The upgrade workflow YAML itself is not coverage-measurable; the feature-review rule
`modified-workflow-needs-green-run` requires a green run against the branch head before it can
merge, which is the substitute.

### 11.4 A caution on gate credibility

Consistent with `.claude/rules/ci-workflows.md` and prior experience in this repository: each new
gate in the verifier must be observed **failing** on a deliberately broken input before it is
trusted. A verifier that passes because its detector never fires is worse than no verifier, because
it converts a visible red build into a silent green one. In particular, the reference-completeness
check (§ 4.4) exists precisely to falsify an assumption; if it cannot be made to fail, it is not
testing anything.

---

## Automation Feasibility

Every step of the proposed design was assessed for unattended execution. Four items cannot run
unattended.

### A1. Minting the automation credential — **requires a documented exception (one-time)**

**Step:** create a GitHub App (or fine-grained PAT), grant Contents: write and Pull requests: write,
install it on `drmoisan/TaskMaster`, and store the App ID and private key as repository secrets.

**Why it cannot run unattended:** secrets cannot be created by a workflow. `GITHUB_TOKEN` cannot
mint a credential with a different identity, and the entire point of the credential is to have an
identity other than `GITHUB_TOKEN` so that the recursion suppression does not apply (§ 3.1).
`[OBS-LOCAL]` no secret is referenced by any workflow in this repository today, so this is genuinely
new setup, not a reuse.

**Classification:** **requiring a documented exception.** It is one-time, performed by a repository
admin, and after it the pipeline runs unattended indefinitely (installation tokens are minted per
run and the App private key does not expire). It is not removable by scope change: every route that
produces evaluated required checks on an automation-authored PR needs either this credential or the
recurring click in A2. It is not a hard halt, because the work is a few minutes of admin action
with no ongoing obligation.

**If it is refused:** the design degrades to A2, not to failure.

### A2. Fallback if A1 is refused — **requires a documented exception (recurring, per PR)**

**Step:** a repository writer clicks **Approve workflows to run** on each upgrade PR.

**Why it cannot run unattended:** `[DOC]` when a workflow using `GITHUB_TOKEN` creates a pull
request, the resulting `pull_request` runs are created in an approval-required state, and there is
no repository setting that disables this (§ 3.1). A workflow cannot approve its own runs.

**Classification:** **requiring a documented exception**, recurring weekly. It is materially worse
than A1 — it is a standing obligation rather than a one-time one, and an unclicked PR is
indistinguishable from a healthy quiet week — but it is not a hard halt: the checks do exist on the
correct SHA and do satisfy the ruleset once started. This corrects the brief's stated worst case,
which was that the PR would carry zero checks and be permanently unmergeable.

### A3. Merging the upgrade PR — **removable by scope change**

**Step:** merging the PR after the five checks pass.

**Why it may not run unattended:** the design as written stops at "mergeable without human edits,"
which a human then merges.

**Classification:** **removable by scope change.** `gh pr merge --auto --squash` executed with the
App token would land the PR automatically once all five checks pass, and doing so also shrinks the
`strict_required_status_checks_policy` staleness window (§ 3.2). This is a policy decision — whether
dependency upgrades may land on `main` with no human in the loop — not a technical limitation.
Recommend enabling auto-merge; if the project prefers a human merge gate, the residual manual step
is one click on an already-green PR.

### A4. The cold-cache verification of #898 — **requires a documented exception (one-time)**

**Step:** the spec's manual verification note, "confirm `main` builds from a cold cache after the
#898 correction."

**Why it cannot run unattended in the normal pipeline:** `[OBS-LOCAL]` the cache key in all three
build workflows is `nuget-${{ runner.os }}-${{ hashFiles('**/packages.config') }}` with a bare
`restore-keys: nuget-${{ runner.os }}-` prefix fallback. That fallback is what currently masks the
stale analyzer paths. A scheduled run will hit the fallback and pass whether or not #898 is fixed,
so the pipeline structurally cannot observe the condition it is meant to verify.

**Classification:** **requiring a documented exception**, one-time. It is satisfiable unattended by
a one-off `workflow_dispatch` run of `ci.yml` performed after manually purging the Actions cache,
or by a temporary cache-key change — but a human must initiate one of those. Recording the run URL
as evidence discharges it.

### Steps confirmed to run unattended

Discovery of Dependabot targets, the compatibility gate, per-package `nuget update`, both
post-passes, `csharpier format`, the verifier and its repairs, branch creation and push, PR body
composition, labelling, and closing the consumed Dependabot PR — all execute inside the workflow
with no human input, given A1.

### Explicitly eliminated, not merely unattended-incapable

`workflow_dispatch` against `ci.yml` and `workflow_run` chaining do not fail on automation grounds;
they fail on correctness grounds. `[DOC]` Checks created by workflow runs triggered by those events
are **not evaluated** for a pull request (§ 3.2). No amount of automation makes them satisfy the
ruleset.

---

## Assumptions stated explicitly

1. **Dependabot's pinned NuGet.Client matches the source read here.** § 4.3's corroboration of R2
   assumes `dependabot-core` pins a NuGet.Client version whose `MSBuildNuGetProject` matches the
   `dev` branch. Not verified. § 4.2's source reading is unaffected and applies to the version the
   workflow itself pins.
2. **`windows-latest` MSBuild's Roslyn version is at least the highest `roslynNN` folder any of
   these five analyzer packages ships.** Assumed in § 6.3 step 3(d). Not verified. If false, that
   step needs a real version table; `build-analyzers` will surface the error.
3. **`group-by` is not a valid Dependabot group key.** Its inertness is observed (§ 5.2); its
   formal invalidity is inferred from a documentation key enumeration rather than from a published
   schema. Removing it is safe either way.
4. **The ruleset's required-check entries are not scoped to a specific app id.** If they were
   scoped to GitHub Actions (app id 15368), the recommendation is unaffected, since all five checks
   are produced by GitHub Actions in every option considered. Not verified; supplied as established
   by the brief.
5. **The repository will accept a first-ever repository secret and a first-ever workflow with
   `contents: write`.** `[OBS-LOCAL]` every existing workflow is `permissions: contents: read` with
   no secrets. This is a governance posture change, not just a technical one.
6. **Dependabot will not split the consolidated PR.** Depends on the "incompatible version
   constraints" carve-out not firing (§ 5.3). The repository is currently consistent, and the
   design tolerates a split by consuming all open Dependabot PRs.

---

## Appendix — file references

| Path | Relevance |
|---|---|
| `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-09-12T10-15\.github\dependabot.yml` | R3 — replace per § 5.3 |
| `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-09-12T10-15\.github\workflows\ci.yml` | R1 — triggers and check-name derivation |
| `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-09-12T10-15\.github\workflows\_build-analyzers.yml` | R5 — `nuget-version: latest` at line 33 |
| `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-09-12T10-15\.github\workflows\_build-nullable.yml` | R5 — `nuget-version: latest` at line 33 |
| `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-09-12T10-15\.github\workflows\_mstest-coverage.yml` | R5 — `nuget-version: latest` at line 33 |
| `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-09-12T10-15\.csharpierignore` | § 2.2b — no `packages.config` / `app.config` entry |
| `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-09-12T10-15\scripts\vscode\Sync-PackageReferences.ps1` | § 2.3 — `netstandard2.1` at line 18 (#902); `\lib\`-anchored regex at line 58 |
| `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-09-12T10-15\UtilitiesCS\UtilitiesCS.csproj` | § 6.2 — all four analyzer path shapes, lines 1305-1318 |
| `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-09-12T10-15\UtilitiesCS\packages.config` | § 2.2b — CSharpier-reflowed manifest entry, lines 17-22 |
| `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-09-12T10-15\SVGControl.Test\app.config` | § 2.2b — CSharpier-reflowed `assemblyIdentity`, lines 6-10 |
| `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-09-12T10-15\ToDoModel.Test\ToDoModel.Test.csproj` | § 2.2d — orphaned Deedle/FSharp.Core HintPaths, lines 92-96 (#903) |
| `C:\Users\DanMoisan\repos\TaskMaster\.git\info\refs` | § 1.3 — Dependabot branch names, lines 161-175 |
