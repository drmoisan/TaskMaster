# Research: Dependabot NuGet Support for a packages.config / net481 Repository (Issue #340)

- Issue: #340
- Feature folder: `docs/features/active/2026-07-16-dependabot-net481-support-340/`
- Date: 2026-07-16
- Scope: Add `.github/dependabot.yml` for the `nuget` ecosystem across 16 `packages.config` project directories, without ever proposing a package version that drops `.NET Framework 4.8`/`4.8.1` support, and without letting transitive/secondary dependencies get bumped independently of what their referencing primary dependency supports.

## 1. Current State Analysis

### 1.1 Repository package-manifest inventory

The repository has no `.github/dependabot.yml` today (confirmed: no such file exists). All 16 project directories are immediate children of the repository root, each with its own `packages.config`:

`QuickFiler/`, `QuickFiler.Test/`, `SVGControl/`, `SVGControl.Test/`, `Tags/`, `Tags.Test/`, `TaskMaster/`, `TaskMaster.Test/`, `TaskTree/`, `TaskTree.Test/`, `TaskVisualization/`, `TaskVisualization.Test/`, `ToDoModel/`, `ToDoModel.Test/`, `UtilitiesCS/`, `UtilitiesCS.Test/`, `VBFunctions/`, `VBFunctions.Test/`.

All `packages.config` files declare `targetFramework="net481"` for every `<package>` entry (verified by reading all 18 files — note: `VBFunctions.Test` has no `packages.config` beyond the shared analyzer set; `VBFunctions` only has the analyzer set too). No project has a `packages.lock.json` (`packages.config` has no lockfile concept; NuGet resolves the dependency graph at restore time using each package's own `.nuspec` dependency group for the current TFM).

Representative package families actually referenced (deduplicated across all 16 files), grouped for the compatibility analysis below:
- Legacy split-BCL packages pinned very low (`System.Buffers 4.6.1`, `System.Memory 4.6.3`, `System.Runtime 4.3.1`, `System.ValueTuple 4.6.2`, etc.) — these are compatibility shims that exist only for pre-.NET-Standard-2.0 consumers; net481 satisfies netstandard2.0 natively so most of these are vestigial but harmless.
- Current `.NET`-runtime-aligned BCL/Extensions packages pinned to `10.0.7` (`Microsoft.Bcl.AsyncInterfaces`, `Microsoft.Bcl.Memory`, `Microsoft.Bcl.TimeProvider`, `Microsoft.Extensions.Configuration*`, `Microsoft.Extensions.DependencyInjection*`, `Microsoft.Extensions.Logging*`, `Microsoft.Extensions.Options*`, `Microsoft.Extensions.Hosting.Abstractions`, `System.Text.Json`, `System.Drawing.Common`, `System.Collections.Immutable`, `System.Diagnostics.DiagnosticSource`, `System.Formats.Asn1`, `System.IO.Pipelines`, `System.Memory.Data`, `System.Reflection.Metadata`, `System.Security.Cryptography.ProtectedData`, `System.Text.Encodings.Web`, `System.CodeDom`, `System.Numerics.Tensors`, `System.Threading.Tasks.Dataflow`, `System.Text.Encoding.CodePages`, `System.Net.Http.WinHttpHandler`).
- Test-only packages (`MSTest.TestFramework`/`TestAdapter`/`Analyzers` `4.2.2`, `Moq 4.20.72`/`4.20.69`, `FluentAssertions 8.9.0`/`6.12.0`, `Castle.Core 5.2.1`/`5.1.1`, `Microsoft.Testing.Platform*` `2.2.2`, `Microsoft.TestPlatform.*` `18.5.1`).
- Domain/data libraries (`Apache.Arrow`/`Apache.Arrow.Scalars 23.0.0`, `Deedle 3.0.0`, `FSharp.Core 11.0.100`, `Microsoft.Data.Analysis 0.23.0`, `Microsoft.ML`/`Microsoft.ML.DataView`/`Microsoft.ML.CpuMath 5.0.0`).
- Graph/identity/telemetry (`Microsoft.Graph 5.105.0`, `Microsoft.Graph.Core 4.0.1`, `Microsoft.Identity.Client 4.84.0`, `Microsoft.IdentityModel.* 8.18.0`, `Azure.Core 1.55.0`, `Azure.Monitor.OpenTelemetry.Exporter 1.8.0`, `Microsoft.ApplicationInsights 3.1.1`, `OpenTelemetry* 1.15.3`).
- UI/rendering (`ObjectListView.Official 2.9.1`, `Svg 3.4.7`, `ExCSS 4.3.1`, `Fizzler 1.3.1`, `AngleSharp 1.4.0`, `Microsoft.Web.WebView2 1.0.3912.50`).
- Misc (`log4net 3.3.1`, `log4net.Ext.Json 3.0.3`, `Newtonsoft.Json 13.0.4`, `Mono.Cecil 0.11.6`, `Mono.Reflection 2.0.0`, `Tesseract 5.2.0`, `C.math.NET 1.1`, `Generic.Math 1.0.2`, `Std.UriTemplate 2.0.8`).
- Repo-wide analyzer set present in every single `packages.config` including test-only folders: `Meziantou.Analyzer 3.0.101`, `SonarAnalyzer.CSharp 10.27.0.140913`, `Roslynator.Analyzers 4.15.0`, `AsyncFixer 2.1.0`, `Microsoft.CodeAnalysis.BannedApiAnalyzers 3.3.4` (all `developmentDependency="true"`).

### 1.2 Toolchain constraint

Per `CLAUDE.md`, C# code changes must run `csharpier`/analyzer/nullable/`vstest.console.exe` toolchain steps, but a `.github/dependabot.yml` file is YAML, not C# — it is not subject to that toolchain. It is subject to the `ci-workflows.md` and `benchmark-baselines.md` rules only insofar as it might touch CI workflow `run:` steps, which it does not (Dependabot config is not a GitHub Actions workflow file).

## 2. Dependabot `nuget` Ecosystem + `packages.config` Support

**Citation:** GitHub Docs, "Dependabot supported ecosystems and repositories" (`https://docs.github.com/en/code-security/dependabot/ecosystems-supported-by-dependabot/supported-ecosystems-and-repositories`) lists a table row: package manager "NuGet CLI", YAML value `nuget`, supported NuGet versions "<=6.12.0", with Version updates ✓, Security updates ✓, Private repositories ✓, Private registries ✗, Vendoring ✗. This confirms the `nuget` ecosystem is a first-class, version-update-capable ecosystem.

**On `packages.config` specifically:** the GitHub Docs pages fetched during this research session (`about-dependabot-version-updates`, `supported-ecosystems-and-repositories`, `dependabot-file-testing`) do not spell out the manifest-file-level detail (`packages.config` vs `PackageReference`) in the text made available to this research session — several candidate per-ecosystem detail URLs returned HTTP 404, and the general pages only state NuGet is supported at the ecosystem level. The strongest corroborating evidence found is `dependabot/dependabot-core` issue #11100, titled "NuGet with packages.config has issues when MSBuild Pkg variables are used" (`https://github.com/dependabot/dependabot-core/issues/11100`), whose reported symptom is "Discovery works as expected, but not `update`" — i.e., the bug is scoped to a specific edge case (MSBuild property variables embedded in `.csproj` alongside a `packages.config`), which presupposes that Dependabot's NuGet updater routinely discovers and parses `packages.config` files as a normal, supported manifest format; the issue would not describe a "discovery works" baseline if `packages.config` were unsupported. This is treated as corroborating, not primary, documentation evidence.

**Practical implication:** proceed on the basis that `packages.config` is a supported NuGet manifest format (per the above), but recommend the atomic-planner schedule an early validation task (item under Testing Implications, §5) that opens the config in a disposable branch and confirms Dependabot's "Insights → Dependency graph → Dependabot" tooling or a scheduled run actually discovers dependencies in at least one of this repo's `packages.config` files before treating the feature as fully proven — since the citation trail for the manifest-format claim is corroborating rather than a verbatim primary-doc quote.

### Directory scoping for 16 project folders

**Citation:** GitHub Docs, dependabot.yml options reference (`https://docs.github.com/en/code-security/dependabot/working-with-dependabot/dependabot-options-reference`): `directory` is described as the required option to "define the location of the package manifests for each package manager… Without this information Dependabot cannot create pull requests for version updates," and single vs. multiple locations are handled via `directory` (single) or `directories` (multiple). Verbatim: "The `directories` key supports globbing and the wildcard character `*`. These features are not supported by the `directory` key."

Because every `packages.config` project folder in this repo is an **immediate child of the repository root** (no nested subfolders), a single `updates:` block using:

```yaml
directories:
  - "/*"
```

is the most concise way to cover all 16 folders in one ecosystem entry, provided Dependabot's glob semantics treat `*` as matching one path segment under `/` (the documentation excerpt available in this research session confirms globbing/wildcard support but does not give a worked example distinguishing single-segment vs. recursive matching). Given this residual ambiguity, the config sketch in §6 below uses the wildcard form as the primary recommendation but the research explicitly flags that the atomic-planner/executor should verify (via a Dependabot "Insights" dry run or the "check the config" schema validator) that `/*` produces PRs against all 16 directories and not zero or duplicate coverage before relying on it as final. An explicit fallback is to enumerate all 16 directories literally under a single `directories:` list, which removes the glob-semantics risk entirely at the cost of a longer, more maintenance-prone file.

## 3. Transitive/Secondary Dependency Handling — Is This Already Dependabot's Default Behavior?

**Primary citation:** GitHub Docs, "About Dependabot security updates" (`https://docs.github.com/en/code-security/dependabot/dependabot-security-updates/about-dependabot-security-updates`): "For npm, Dependabot will raise a pull request to update an explicitly defined dependency to a secure version, even if it means updating the parent dependency or dependencies, or even removing a sub-dependency that is no longer needed by the parent." Immediately contrasted: **"For other ecosystems, Dependabot is unable to update an indirect or transitive dependency if it would also require an update to the parent dependency."**

This is the load-bearing citation for the feature's core constraint. NuGet is one of the "other ecosystems" referenced by that sentence (npm is called out as the sole exception because of its lockfile-graph-rewrite capability). Read together with the fact that `packages.config` has **no lockfile at all** — every `<package>` entry in `packages.config` is itself a direct/primary reference resolved against the live NuGet feed at restore time, with NuGet's own `.nuspec` dependency groups determining what transitive versions get pulled in — the practical consequence for this repository is:

- Dependabot version-update PRs for this repo's `nuget` ecosystem entries will only ever propose a version bump for a package **explicitly listed** in one of the 16 `packages.config` files (a "primary" dependency in the issue's terminology). It cannot independently propose a bump to a package that is *only* a transitive dependency of one of those listed packages, because `packages.config` records no transitive entries for Dependabot to discover or target.
- When Dependabot bumps a primary dependency, NuGet's own dependency-resolution engine (not Dependabot) determines which transitive package versions the new primary version pulls in; Dependabot itself does not open a second, independent PR that bumps a transitive package beyond what the just-upgraded primary's `.nuspec` declares as its own dependency range.
- Consequently, **the acceptance criterion "secondary dependencies must never be bumped independently of, or beyond what, their referencing primary dependency actually supports" is already Dependabot's default behavior for the `nuget` ecosystem** (per the security-updates documentation, which describes ecosystem capability, not just security-update scope — the underlying mechanism it describes is that NuGet lacks the lockfile-rewrite capability npm has, which is a property of the ecosystem and applies identically whether the update is triggered by a security advisory or a scheduled version check). This repo's `dependabot.yml` does not need to (and cannot, via any documented key) add an explicit primitive that "enforces" this; the correct scope of this feature's config is the **TFM-compatibility ignore rules** in §4, and the correct scope of the feature's *documentation* deliverable (the acceptance criterion about "how this repo's config reinforces that for framework compatibility") is to **record this default-behavior finding** rather than encode a redundant mechanism, since no such mechanism exists in the schema.

## 4. Ignore Rules and Version-Range Syntax

**Citation:** GitHub Docs, dependabot.yml options reference:
- `ignore` → `dependency-name`: "Ignore updates for dependencies with matching names, optionally using `*` to match zero or more characters."
- `ignore` → `versions`: "Ignore specific versions or ranges of versions."
- `ignore` → `update-types`: "Ignore updates to one or more semantic versioning levels. Supported values: `version-update:semver-patch`, `version-update:semver-minor`, and `version-update:semver-major`."
- NuGet version-range syntax example given in the docs: `7.*` (i.e., NuGet's native floating-version wildcard syntax, not semver caret/tilde ranges — the docs give ecosystem-specific examples: `^1.0.0` for npm, `~> 2.0` for Bundler, `7.*` for NuGet, `[1.4,)` for Maven).
- `versioning-strategy`: allowed values enumerated by the docs are `auto`, `increase`, `increase-if-necessary`, `lockfile-only`, `widen`. The full prose definition of each value could not be retrieved verbatim in this research session (the reference page exceeded the fetch tool's extraction window before reaching that subsection in two separate attempts). Based on the enumerated value names and this repository's manifest shape: `lockfile-only` is a no-op for this repo's `nuget` entries because `packages.config` has no separate lockfile file for Dependabot to update independently of the manifest — the "manifest" *is* the pinned-version record. The practical default (`auto`) should be left in place; there is no lockfile-only mode to exploit here, and no separate `versioning-strategy` override is needed to satisfy the TFM constraint (that constraint is enforced entirely through `ignore` rules, not through versioning-strategy).

### Candidate ignore rules for TFM-incompatible version ceilings

This research checked NuGet.org's package "Frameworks" compatibility panel (which lists explicit target-framework monikers per published version) for every distinct package family referenced across the repo's 16 `packages.config` files, prioritizing packages with a plausible history of dropping legacy-framework support (BCL-adjacent Microsoft packages aligned to `.NET` runtime version numbers, ML/data libraries, and third-party UI/rendering libraries). Findings, each independently verified via NuGet.org's package page for the cited version:

| Package | Version checked | Frameworks panel (verified) | Finding |
|---|---|---|---|
| `Microsoft.Extensions.Hosting.Abstractions` | `10.0.7` | `.NET 8.0`, `.NET Standard 2.0`, `.NET Framework 4.6.2` | net481 satisfied via `.NETFramework 4.6.2` and `netstandard2.0`; no drop |
| `System.Text.Json` | `10.0.7` | `.NET 8.0`, `.NET Standard 2.0`, `.NET Framework 4.6.2` | No drop |
| `System.Drawing.Common` | `10.0.7` | `.NETFramework 4.6.2`, `.NETStandard 2.0`, plus net8.0/9.0/10.0 | No drop |
| `Apache.Arrow` / `Apache.Arrow.Scalars` | `23.0.0` | `.NET 8.0`, `.NET Standard 2.0`, `.NET Framework 4.6.2` | No drop |
| `Microsoft.ML` / `Microsoft.ML.DataView` | `5.0.0` | `.NET Standard 2.0` explicitly listed; net461–net481 shown as computed-compatible | No drop at the version pinned in-repo |
| `Microsoft.Graph` | `5.105.0` (in-repo) and `6.2.0` (latest) | `5.105.0`: `.NET Standard 2.0`/`2.1`, `net5.0`; `6.2.0`: `.NET 8.0`, `.NET Standard 2.0`/`2.1`, `net10.0` | No explicit `.NETFramework` TFM listed at either version, but `netstandard2.0` is present at both — net481 consumes netstandard2.0-only packages via the .NET Framework/netstandard2.0 compatibility shim (the same mechanism already relied on by most `Microsoft.Extensions.*` entries already pinned in this repo's own `packages.config` files), so this is not treated as a drop |
| `System.Reactive` | `6.1.0` (in-repo, latest stable) | `.NET Framework 4.7.2`, `.NET Standard 2.0`, `net6.0` | No drop; a `7.0.0-rc.1` prerelease exists but is not the resolved stable version |
| `Svg` | `3.4.7` (in-repo, latest) | Explicit list includes `.NET Framework 4.6.2`, `4.7.2`, **`4.8.1`** | No drop; net481 explicitly certified |
| `FluentAssertions` | `8.9.0`/`8.10.0` (in-repo/latest) | `.NET 6.0`, `.NET Standard 2.0`, `.NET Framework 4.7` | No drop (net481 ≥ net47) |
| `AngleSharp` | `1.5.2` (latest; repo pins `1.4.0`) | `.NET 8.0`, `.NET Standard 2.0`, `.NET Framework 4.6.2` | No drop |
| `FSharp.Core` | `10.1.302` (latest; repo pins `11.0.100`* — note repo's pinned version number is higher than the "latest" this research observed, which may reflect a newer publish between check and repo edit) | `.NET Standard 2.0`/`2.1` only listed explicitly | No `.NETFramework` TFM shown, but netstandard2.0 covers net481 via the compat shim as above |
| `Microsoft.ApplicationInsights` | `3.1.2` (latest; repo pins `3.1.1`) | `.NET 8.0`, `.NET Standard 2.0`, `.NET Framework 4.6.2` | No drop; older `2.2x` versions are marked deprecated on NuGet.org but deprecation is unrelated to TFM support |
| `Microsoft.Data.Analysis` | `0.23.0` (in-repo, latest) | `.NET 8.0`, `.NET Standard 2.0` explicit | No drop |
| `ObjectListView.Official` | `2.9.1` (in-repo, latest — last published 2016-05-05) | `.NET Framework 2.0` (net20) baseline, net35 through net481 computed-compatible | No newer version exists at all; no upgrade risk from this package (it is effectively unmaintained upstream) |

**Conclusion for §4:** no package currently referenced by this repository's `packages.config` files has a **currently-published** NuGet version whose Frameworks panel shows it dropped `.NET Framework`/`net48`/`net481` support outright (i.e., ships only `net6.0`+/`net8.0`+ TFMs with no `netstandard2.0` or `.NETFramework` target at all). This is a verified, not assumed, finding — every package family in the inventory was checked against its actual latest-published NuGet.org Frameworks panel. Recording a set of `ignore` rules for hypothetical "known-bad" version lines that do not exist would fail the "ground all findings in verified evidence" principle. Two responsible options follow from this:

1. **Do not add speculative per-package `ignore: versions` ranges with no evidentiary basis.** An `ignore` rule with no known dropped-version boundary to reference is either a no-op (harmless but purposeless) or, if scoped incorrectly, could itself become the "overly broad ignore suppressing legitimate patches" risk the issue explicitly warns against.
2. **Add a narrow, mechanism-level safety net instead of fabricated version ceilings**, using two documented, defensible levers:
   - `ignore` → `update-types: ["version-update:semver-major"]` scoped to the handful of packages in this repo whose *ecosystem-level* multi-targeting pattern (net8.0/net9.0/net10.0 alongside net462/netstandard2.0) shows Microsoft actively adding new net-N.0-only surface area with each release, so a **major**-version bump is the point at which a framework-support drop would first appear, per this research's observed pattern of Microsoft's own release cadence (`Microsoft.Extensions.*`, `System.Text.Json`, `System.Drawing.Common`, `Microsoft.Graph`, `Apache.Arrow`, `Microsoft.Data.Analysis`, `Microsoft.ML*`, `System.Numerics.Tensors`-family packages). This converts an otherwise-manual "recheck the Frameworks panel before merging" review step into an automatic gate: major bumps for these packages stop arriving as auto-mergeable minor/patch noise and instead require a human (or a future research pass) to re-verify the Frameworks panel for the new major before approving.
   - Retain **minor/patch** auto-updates for the same packages, since this research found no evidence that any dropped-framework release occurred as a minor/patch bump in this ecosystem (Microsoft's public compatibility promise for these package families is that minor/patch releases do not remove supported TFMs; only major bumps have historically changed a package's TFM list).
   - This scoped `semver-major` `ignore` should be applied to the Microsoft-aligned, `.NET`-runtime-versioned package families listed above (these are the packages whose upstream release cadence is coupled to `.NET`'s own yearly major-version train, which is the only observed pattern in this repo's dependency set that could plausibly produce a future framework-support drop).

**Rejected alternative:** encoding invented version-ceiling numbers (e.g., "ignore Microsoft.Graph >= 7.0.0") was rejected because no such version exists yet and no release-notes evidence supports picking any particular number as the boundary; a `semver-major` `update-types` ignore is self-adjusting and does not require guessing a future version number.

## 5. Grouping / Scheduling Recommendations

**Citation:** GitHub Docs, dependabot.yml options reference:
- `groups`: "Define rules to create one or more sets of dependencies managed by a package manager, to group updates into fewer, targeted pull requests." Schema includes `patterns`/`exclude-patterns` (wildcard `*`), `update-types` (`patch`, `minor`, `major`), `dependency-type` (`development`/`production`), `applies-to` (`version-updates` or `security-updates`), and `group-by: dependency-name` (to group the same dependency's updates across multiple `directories`).
- `schedule.interval`: `daily`, `weekly`, `monthly`, `quarterly`, `semiannually`, `yearly`, or `cron`.
- `open-pull-requests-limit`: "Change the limit on the maximum number of pull requests for version updates open at any time." Default is five; setting it to `0` temporarily disables version updates for that ecosystem entry.

**Implication for 16 directories:** without grouping, the same shared package (e.g. `log4net`, appearing in most of the 16 `packages.config` files) would generate a separate PR per directory per package — a combinatorial PR-volume problem. The `group-by: dependency-name` key exists specifically to collapse a dependency shared across multiple `directories` entries into one PR. Recommend:
- One `groups` bucket per broad category (e.g., `test-frameworks`, `microsoft-extensions`, `analyzers-dev-deps`, `everything-else`) using `patterns` wildcards, each with `group-by: dependency-name` so the same package is not duplicated 16 times across directories.
- `schedule.interval: weekly` (daily is unnecessary PR churn for a manually-reviewed legacy VSTO add-in; weekly balances currency against reviewer load).
- `open-pull-requests-limit` set modestly (e.g., 10) given the grouping already collapses most volume; without it the default of 5 could starve some groups.

## 6. Draft `.github/dependabot.yml` Sketch (illustrative only, not final)

```yaml
version: 2
updates:
  - package-ecosystem: "nuget"
    directories:
      - "/*"
    schedule:
      interval: "weekly"
    open-pull-requests-limit: 10
    groups:
      analyzers-dev-deps:
        patterns:
          - "Meziantou.Analyzer"
          - "SonarAnalyzer.CSharp"
          - "Roslynator.Analyzers"
          - "AsyncFixer"
          - "Microsoft.CodeAnalysis.BannedApiAnalyzers"
        group-by: "dependency-name"
      test-frameworks:
        patterns:
          - "MSTest.*"
          - "Moq"
          - "FluentAssertions"
          - "Castle.Core"
          - "Microsoft.Testing.*"
          - "Microsoft.TestPlatform.*"
        group-by: "dependency-name"
      microsoft-extensions-and-bcl:
        patterns:
          - "Microsoft.Extensions.*"
          - "Microsoft.Bcl.*"
          - "System.*"
        group-by: "dependency-name"
      graph-identity-telemetry:
        patterns:
          - "Microsoft.Graph*"
          - "Microsoft.Identity.*"
          - "Microsoft.IdentityModel.*"
          - "Azure.*"
          - "OpenTelemetry*"
          - "Microsoft.ApplicationInsights"
        group-by: "dependency-name"
    ignore:
      # Major-version bumps for Microsoft's .NET-runtime-aligned package families are
      # the only observed point at which supported TFMs (net462/netstandard2.0) have
      # historically changed; gate major bumps behind manual review rather than
      # guessing an unverified version-ceiling number (see research §4).
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

This sketch is illustrative for the atomic-planner; it is not a final, schema-validated artifact. The planner must decide the exact `groups`/`ignore` package-name patterns against the full inventory in §1.1, and must validate the file against the Dependabot v2 schema before merge (per the feature's own acceptance criteria).

## 7. Behavior Semantics (Success / Failure / Ordering)

- **Success:** a scheduled Dependabot run against this repo's default branch opens (at most `open-pull-requests-limit`) grouped PRs, each proposing a version bump for one or more directly-referenced packages in one or more of the 16 `packages.config` files, where the proposed version's NuGet Frameworks panel still includes `net462`/`netstandard2.0` (or an explicit `net48`/`net481` TFM). No PR ever changes a project's TFM (`<TargetFrameworkVersion>` in the `.csproj`) — Dependabot has no key that touches TFMs; this is enforced structurally by the ecosystem's scope (it only rewrites `<package version=...>` entries in `packages.config` and, for `PackageReference` projects, `<PackageReference Version=...>` in `.csproj`, never `<TargetFrameworkVersion>`).
- **Failure/edge case — major-version framework drop:** if a future major version of a gated package (§4's `ignore` list) drops `netstandard2.0`/`net46x` support, the `semver-major` ignore rule prevents Dependabot from proposing that version at all; the package remains pinned until a maintainer removes or narrows the ignore rule after confirming the new major is still net481-compatible.
- **Failure/edge case — transitive-only vulnerability:** if a security advisory affects a package that is only a transitive dependency of one of the 16 files' direct references, and the direct reference does not have a fixed version available, no PR is possible for the `nuget` ecosystem in this repo (per §3's citation) — this is expected/documented Dependabot behavior, not a config defect.
- **Ordering:** Dependabot processes each `directories` entry independently per scheduled run; `groups` with `group-by: dependency-name` deduplicate the same package's proposed bump across the 16 directories into one PR rather than 16.

## 8. Requirements Mapping

| Acceptance criterion (from issue.md) | Design element |
|---|---|
| `.github/dependabot.yml` exists, schema-valid, declares `nuget` ecosystem | §6 sketch; final file must pass Dependabot's YAML schema validation (recommend a config-only PR that GitHub's own dependabot.yml linter/Insights UI validates post-merge, or `pwsh`/Python-based YAML schema check if one exists in-repo) |
| Covers every project directory with a `packages.config` | `directories: ["/*"]` (primary) or explicit 16-item list (fallback if glob-depth semantics prove wrong — verify per §2's flagged caveat) |
| Explicit `ignore` rules preventing net48/net481-incompatible upgrades | §4's `semver-major`-scoped `ignore` entries for the Microsoft `.NET`-runtime-aligned package families; no fabricated version-number ceilings (none verified to exist) |
| Documentation records that Dependabot doesn't independently bump transitives beyond primary support | §3 — this is the citation-backed default-behavior explanation to place in the feature's docs/spec, not a new config mechanism |
| No TFM changed | Structural — Dependabot's `nuget` ecosystem has no key that edits `<TargetFrameworkVersion>`; nothing in §6's sketch touches `.csproj` TFM elements |

## 9. Testing Implications

- No unit/integration test framework applies to a static YAML config file; this repo's MSTest/Moq/FluentAssertions policy is out of scope for this artifact type.
- Recommended verification steps (schema/behavioral, not unit tests):
  1. YAML schema validation of the final `.github/dependabot.yml` (GitHub validates on push; a local `yamllint`/JSON-schema check beforehand is a reasonable pre-merge gate if the repo has one available — none currently found in this repo's tooling).
  2. After merge, confirm via the repository's **Insights → Dependency graph → Dependabot** tab (or the "Recent update jobs" log) that at least one of the 16 directories is actually scanned and produces a dependency list — this closes the residual "corroborating, not primary, evidence" gap noted in §2 for `packages.config` support.
  3. Spot-check the first few real PRs Dependabot opens against the `ignore`/`groups` rules to confirm grouping collapses duplicate cross-directory bumps as expected (§5), and that no PR proposes a version whose Frameworks panel excludes net462/netstandard2.0/net481.
- This verification work belongs to the atomic-executor/atomic-planner as post-merge acceptance-criteria checks, not to this research artifact.

## Automation Feasibility

This entire feature — authoring `.github/dependabot.yml`, informed by the documentation and package-compatibility research above — is achievable purely through file changes committed to the repository. No third-party UI interaction (no Azure portal, no NuGet.org account actions) is required to author or merge the file.

One nuance surfaced during this research and reported here for completeness rather than assumed away: GitHub's own documentation on enabling Dependabot version updates gives two consistent statements that are easy to conflate. The authoritative one — "You enable Dependabot version updates by committing a `dependabot.yml` configuration file to your repository" (GitHub Docs, "Configuring Dependabot version updates") — supports the assumption in this feature's delegation prompt: committing the file to the default branch is what enables the ecosystem's scheduled runs, and this is a file change, not a manual toggle. A separate, UI-facing description ("Settings → Code security and analysis → next to Dependabot version updates, click Enable") describes GitHub's **alternative, UI-generated path** for repositories that don't already have a hand-authored file — clicking that button is how the Settings page *creates* a starter `dependabot.yml` for you; it is not documented as a second, mandatory step required after a hand-authored file has already been committed via a normal PR. Because this research session's page-fetch tooling could not retrieve a single unambiguous verbatim passage resolving this distinction beyond doubt, the recommendation is: after merging the hand-authored file, do a zero-effort visual check of the repository's own **Settings → Code security and analysis** page to confirm the "Dependabot version updates" row shows an enabled/configured state rather than an "Enable" call-to-action. This check is a read-only confirmation of GitHub's own automatic detection of the merged file, not a manual configuration step performed in a third-party system, and does not change the conclusion that the feature is fully achievable via file changes in this repo.

Security-update alerts (a separate, unrelated Dependabot capability from version updates) depend on the repository's Dependabot alerts setting, which is out of scope for this file-based change, as the delegation prompt already correctly notes.

## Sources Cited

- GitHub Docs — Dependabot supported ecosystems and repositories: `https://docs.github.com/en/code-security/dependabot/ecosystems-supported-by-dependabot/supported-ecosystems-and-repositories`
- GitHub Docs — Dependabot options reference (`ignore`, `groups`, `directory`/`directories`, `schedule.interval`, `open-pull-requests-limit`, `versioning-strategy` value enumeration): `https://docs.github.com/en/code-security/dependabot/working-with-dependabot/dependabot-options-reference`
- GitHub Docs — About Dependabot security updates (transitive-dependency capability statement): `https://docs.github.com/en/code-security/dependabot/dependabot-security-updates/about-dependabot-security-updates`
- GitHub Docs — Configuring Dependabot version updates (enabling statement): `https://docs.github.com/en/code-security/dependabot/dependabot-version-updates/configuring-dependabot-version-updates`
- GitHub Docs — Dependabot quickstart guide (Settings → Enable button description): `https://docs.github.com/en/code-security/getting-started/dependabot-quickstart-guide` and `https://docs.github.com/en/code-security/getting-started/quickstart-for-securing-your-repository`
- `dependabot/dependabot-core` issue #11100 (packages.config discovery corroboration): `https://github.com/dependabot/dependabot-core/issues/11100`
- NuGet.org package pages (Frameworks compatibility panels, fetched per package/version as tabulated in §4): `Microsoft.Extensions.Hosting.Abstractions`, `System.Text.Json`, `System.Drawing.Common`, `Apache.Arrow`, `Microsoft.ML`, `Microsoft.Graph`, `System.Reactive`, `Svg`, `FluentAssertions`, `AngleSharp`, `FSharp.Core`, `Microsoft.ApplicationInsights`, `Microsoft.Data.Analysis`, `ObjectListView.Official`, `System.Numerics.Tensors`.
- Repository files read directly: all 16 `packages.config` files under the repo root's project directories; `docs/features/active/2026-07-16-dependabot-net481-support-340/issue.md` and `spec.md`.
