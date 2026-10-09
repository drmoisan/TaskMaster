---
name: dependabot-repair-workflow-run-script-source-985
description: dependabot-repair runs default-branch YAML but the Dependabot branch's script; YAML edits (incl. workflows/README.md) can't be green-run pre-merge; redirect invariant is a union
metadata:
  type: project
---

`.github/workflows/dependabot-repair.yml` (workflow_run on CI) executes the default-branch YAML but checks out `head_branch` and runs THAT branch's `scripts/dependencies/Repair-PackageManifestConsistency.ps1`. A script fix reaches an existing Dependabot PR only after `@dependabot recreate` (`rebase` is refused once the repair bot has pushed a commit).

**Why:** Any diff under `.github/workflows/**` (README.md included) trips `modified-workflow-needs-green-run`, and this workflow can never run green against a feature branch (dependabot/ filter, no workflow_dispatch). Issue #985 research therefore put the new redirect-sync pass inside the script, made it unconditional, and kept its records out of `Verification[].Report.Repair` so the `beyond-known-weak` filter and the YAML stay untouched.

**How to apply:** For any repair-behaviour change, prefer script/module changes over YAML. Note also that `BindingRedirectVerification.Tests.ps1` checks `newVersion` membership in a UNION of all csproj Reference versions, so a stale Reference in one test csproj masks a stale redirect. Packages.config/app.config/csproj are in `.csharpierignore`, so format-check never sees them. Related: [[measure-item-worktree]].
