# Feature Audit — dependabot-net481-support (Issue #340)

- Feature folder: `docs/features/active/2026-07-16-dependabot-net481-support-340/`
- Reviewer timestamp: 2026-07-16T16-40

## Scope and Baseline

- Resolved base branch: `main`
- Merge-base SHA: `1ac990b7ef4b5c2a0db388b3bb792be4c4190838`
- Branch head SHA: `bb669ee938893945d3849ef2a059e93a5c34d102`
- Work mode (from `issue.md` `- Work Mode:` marker): `full-feature`
- Acceptance-criteria source files (per `full-feature` mode): `spec.md` (AC-1 through AC-12) and `user-story.md` (restated maintainer-perspective bullets)
- Baseline state (pre-change, per `evidence/baseline/pre-change-state.2026-07-16T15-56.md`, independently spot-checked): `.github/dependabot.yml` did not exist (`Test-Path` returned `False`); `README.md` had no `Dependency updates (Dependabot)` entry in `## Contents` and no such section between `## Configuration & storage` and `## Common issues`.

## Acceptance Criteria Inventory

From `spec.md`:

1. AC-1 — `.github/dependabot.yml` exists, begins with `version: 2`, exactly one `updates:` entry with `package-ecosystem: "nuget"`.
2. AC-2 — the `nuget` entry's six keys present, correctly typed, valid YAML.
3. AC-3 — directory-scoping mechanism is `directories: ["/*"]`, shipped as final.
4. AC-4 — pre-merge enumeration confirms `packages.config` files exist only at depth 1, across the 16 spec-listed directories (plus `VBFunctions`/`VBFunctions.Test`).
5. AC-5 — pre-decided fallback to the literal 16-entry `directories:` list, triggered only if AC-11 shows under-coverage.
6. AC-6 — `ignore` list contains one entry per named Microsoft `.NET`-runtime-aligned package family, each scoped to `semver-major`, no fabricated `versions:` ceiling.
7. AC-7 — no `ignore` entry includes `semver-minor`/`semver-patch`.
8. AC-8 — `groups` defines four named buckets with `patterns` + `group-by: "dependency-name"`.
9. AC-9 — `schedule.interval` is `"weekly"`, `open-pull-requests-limit` is `10`.
10. AC-10 — diff review shows zero `.csproj` TFM-element modifications; only `.github/dependabot.yml` and `README.md` changed.
11. AC-11 — post-merge manual confirmation via GitHub Insights → Dependency graph → Dependabot that at least one directory was scanned.
12. AC-12 — `README.md` gains the `## Dependency updates (Dependabot)` section with all four required content points and a `## Contents` entry.

From `user-story.md` (restated, mapped 1:1 to the spec ACs above): directory coverage, safety-net semantics, grouped PRs, weekly cadence/PR cap, TFM non-modification, documentation rationale, and the deferred manual post-merge check.

## Acceptance Criteria Evaluation

| AC | Evaluation | Evidence |
|---|---|---|
| AC-1 | **PASS** | `.github/dependabot.yml` line 1 is `version: 2`; `updates:` has exactly one entry; `package-ecosystem: "nuget"` present. Independently re-read the file. |
| AC-2 | **PASS** | `python -c "import yaml; ..."` (re-run by this reviewer) parses the file successfully and confirms all 6 keys (`package-ecosystem`, `directories`, `schedule`, `open-pull-requests-limit`, `groups`, `ignore`) on the single `updates[0]` entry, each of the documented type (string, list, mapping, integer, mapping, list). |
| AC-3 | **PASS** | `directories:` key's only value is `/*` (block-style list), confirmed by direct file read. |
| AC-4 | **PASS** | Independently re-ran `find . -maxdepth 2 -iname "packages.config"` — found exactly 18 files at depth 1, matching the feature's own `ac4-packages-config-enumeration` evidence, spanning the 16 spec-listed directories plus `VBFunctions`/`VBFunctions.Test`. |
| AC-5 | **Not yet applicable (intentionally unchecked)** | Correctly left `[ ]` in `spec.md`. This is a pre-decided contingency that only activates on an under-coverage result from AC-11, which has not yet run (AC-11 is a manual, post-merge, GitHub-Insights-dependent check that cannot be executed in this offline review environment — GitHub CLI is unavailable per `artifacts/pr_context.summary.txt`). Leaving both unchecked is the correct, spec-mandated state, not a gap. |
| AC-6 | **PASS** | Independently re-ran `grep -c "^      - dependency-name:"` (8 matches) and `grep -n "versions:"` (0 matches) against `.github/dependabot.yml`; the 8 `dependency-name` values match the 8 families named in AC-6 exactly (`Microsoft.Extensions.*`, `Microsoft.Bcl.*`, `System.Text.Json`, `System.Drawing.Common`, `Microsoft.Graph*`, `Apache.Arrow*`, `Microsoft.Data.Analysis`, `Microsoft.ML*`); each has `update-types: ["version-update:semver-major"]`. |
| AC-7 | **PASS** | `grep -c "semver-minor\|semver-patch"` against `.github/dependabot.yml` returns 0. |
| AC-8 | **PASS** | Direct file read confirms `groups:` has exactly four sub-keys (`analyzers-dev-deps`, `test-frameworks`, `microsoft-extensions-and-bcl`, `graph-identity-telemetry`), each with a non-empty `patterns` list and `group-by: "dependency-name"`. |
| AC-9 | **PASS** | `schedule.interval: "weekly"` and `open-pull-requests-limit: 10` both present, confirmed by direct read. |
| AC-10 | **PASS** | Independently re-ran `git diff --name-only <merge-base>..HEAD -- "*.csproj"` — no output. Full `git diff --name-only` (17 files) contains no `.cs`/`.csproj` path. |
| AC-11 | **UNVERIFIED (deferred by design, not a review gap)** | This is an explicitly manual, post-merge, out-of-toolchain check per the spec's own text ("This check is explicitly out of scope for the atomic-executor's automated toolchain... it does not block the initial merge, but the feature is not considered fully verified (feature-audit) until it passes"). GitHub CLI is unavailable in this environment (`artifacts/pr_context.summary.txt`: "GitHub CLI unavailable"), so the Insights → Dependency graph → Dependabot tab cannot be checked from this session. Per the spec's own terms, full feature verification is contingent on this maintainer action occurring after merge; this audit records it as the sole outstanding item rather than treating it as a failure. |
| AC-12 | **PASS** | Independently re-read `README.md` lines 157–163: the `## Dependency updates (Dependabot)` section is present between `## Configuration & storage` and `## Common issues`, contains all four required content points (transitive-dependency behavior, `ignore` safety-net rationale, `directories`/fallback decision, AC-11 runbook pointer), and the `## Contents` list (line 17) carries the matching entry in the correct position. |

## Summary

- 10 of 12 acceptance criteria: **PASS**, independently re-verified against the live repository state (not merely accepted from feature-supplied evidence).
- 1 of 12 (AC-5): correctly and intentionally left unchecked as a pre-decided contingency not yet triggered.
- 1 of 12 (AC-11): correctly and intentionally left unverified/unchecked pending a manual, post-merge, GitHub-UI-dependent maintainer action that cannot be executed from this offline review session.
- No acceptance criterion evaluated as FAIL or PARTIAL.
- No C#, PowerShell, TypeScript, or Python coverage gate applies (zero changed files in any of those languages, verified independently — see `policy-audit.2026-07-16T16-40.md` Section 1–2).
- **Recommendation: PR-ready for merge**, contingent on the maintainer performing the AC-11 manual verification after merge and applying the AC-5 fallback only if that check shows under-coverage. Neither of these is a blocking condition on merge per the spec's own Definition of Done.

### Acceptance Criteria Status

- Source: `docs/features/active/2026-07-16-dependabot-net481-support-340/spec.md` and `docs/features/active/2026-07-16-dependabot-net481-support-340/user-story.md`
- Total AC items: 12 (spec.md AC-1–AC-12) plus 7 restated bullets (user-story.md)
- Checked off (delivered): 10 (spec.md AC-1, AC-2, AC-3, AC-4, AC-6, AC-7, AC-8, AC-9, AC-10, AC-12); 6 of 7 in user-story.md
- Remaining (unchecked): 2 in spec.md (AC-5, AC-11); 1 in user-story.md (the AC-11-mirroring post-merge-confirmation bullet)
- Items remaining:
  - `spec.md` AC-5 — pre-decided fallback, not yet triggered (contingent on AC-11 result)
  - `spec.md` AC-11 — manual post-merge GitHub Insights confirmation (cannot be executed in this offline session)
  - `user-story.md` — "After merge, Dan (or another maintainer) confirms via GitHub's Dependabot Insights tab..." (mirrors AC-11)

## Acceptance Criteria Check-off

No new check-offs were made by this review: all criteria evaluated as PASS above were **already** checked `[x]` in `spec.md` and `user-story.md` prior to this review (confirmed by direct read — see the "Acceptance Criteria Evaluation" table). AC-5 and AC-11 remain correctly unchecked in both files; this reviewer did not alter their state, consistent with the acceptance-criteria-tracking protocol's rule against checking off criteria that cannot be verified as delivered (AC-11 requires a live GitHub UI action unavailable in this session).
