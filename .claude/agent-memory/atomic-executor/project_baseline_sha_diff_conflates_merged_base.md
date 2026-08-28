---
name: baseline-sha-diff-conflates-merged-base
description: After an integration-base merge, `git diff <BASELINE_SHA>` reports the base's commits as if they were the feature's; use `<base>..HEAD` for any change-inventory or scope-containment gate
metadata:
  type: project
---

A plan task that says "run `git diff --name-only <BASELINE_SHA>` and classify the paths" becomes
wrong the moment the branch merges the integration base, because `BASELINE_SHA` predates the merge
and the diff is the union of the feature's work and every commit the merge brought in.

Measured on #476, 2026-08-27: `git diff --name-only <BASELINE_SHA>` listed **250** paths with **6**
production files; `git diff --name-only origin/epic/<integration>..HEAD` listed **78** paths with
exactly the **3** in-scope production files. The three extra production paths and eight extra test
paths belonged to merged siblings 444 and 493. A scope-containment gate reading the first command
would have failed the feature for files it never touched — including
`QfcItemController.InitializationTests.Part2.cs`, which one criterion explicitly forbids modifying.

**Why:** two-dot `<base>..HEAD` is "in HEAD, not in base", which after a merge is precisely the
branch's own additions. A diff against a pre-merge SHA has no such property.

**How to apply:** run the command the task names AND the `<base>..HEAD` form, record both, and make
the `<base>..HEAD` classification the authoritative one with an explicit attribution column for the
merge-induced rows. Do not silently substitute — the task text is still the plan of record. Same
merge also drifts every line-number anchor a spec cites (a `csproj:159` anchor moved to `:173`);
check off on the *entry the line number denotes*, not the integer. Relates to
[[epic-integration-base-invalidates-research-line-counts]] and
[[preflight-mergebase-diff-gates-need-commit-cadence]].
