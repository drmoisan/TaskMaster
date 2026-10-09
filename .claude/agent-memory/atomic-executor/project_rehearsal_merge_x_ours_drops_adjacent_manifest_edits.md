---
name: project-rehearsal-merge-x-ours-drops-adjacent-manifest-edits
description: In a Dependabot-branch rehearsal, git merge -X ours silently dropped fix insertions/deletions next to bumped manifest lines; nuget update CLI leaves test app.config redirects stale
metadata:
  type: project
---

Issue #985 rehearsal (2026-10-09): merging the fix branch into the PR #984 Dependabot branch with `git merge -X ours` exited 0 with an empty porcelain, yet dropped two of four packages.config insertions (those adjacent to `MSTest.TestFramework` lines Dependabot bumped) and the deletion of a duplicate csproj ItemGroup. Only a content check (Grep per declaration, blob equality per script) revealed it; the merge stat was the only visible hint (two manifests and the csproj missing from it).

Also observed: `nuget update <packages.config> -Id X -Version Y` rewrites the manifest and the csproj HintPaths/References but does not touch the project's app.config binding redirect, and it writes the manifest in a non-canonical form that Invoke-ManifestNormalization then reflows.

**Why:** a rehearsal that trusts the merge exit code would build a tree that is not the fix.
**How to apply:** after any `-X ours`/`-X theirs` rehearsal merge, verify every fix element by content and re-apply drops; expect the repair run after a simulated nuget update to write app.config redirects (OwnReference) and the updated manifests (normalisation). Related: [[project-poshqc-analyze-flags-new-verb-test-helpers-and-comma-return]].
