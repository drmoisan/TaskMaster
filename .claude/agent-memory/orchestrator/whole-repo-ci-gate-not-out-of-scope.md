---
name: whole-repo-ci-gate-not-out-of-scope
description: A pre-existing whole-repo format/lint finding cannot be deferred as "out of scope" — the whole-repo CI gate blocks the PR's required check (AC6)
metadata:
  type: feedback
---

When a feature PR inherits a pre-existing repo-wide format/lint failure (e.g., a CSharpier `dotnet csharpier check .` finding on a file the feature never touched), it is NOT safe to classify it "out of scope / pre-existing baseline." The CI gate runs whole-repo and is the first step of the required check, so any unformatted file makes the PR's required CI check RED — which fails AC6 (PR CI green) for every branch off that main.

**Why:** During issue #181, the first feature-review deferred a CSharpier finding in `UtilitiesCS/Extensions/IEnumerableExtensions.cs` (byte-identical to main; main's own CI HEAD was red from PR #180 landing an unformatted file). That deferral produced a guaranteed-RED CI run and forced an extra remediation cycle. The CI formatting gate also masks the build/nullable/test steps that run after it, so you cannot even see those results until formatting passes.

**How to apply:** Before opening the PR, check whether `main`'s latest CI is green (`gh run list --branch main --workflow CI`). If main is red on a whole-repo gate, expect the branch to inherit it. Resolve the blocking whole-repo finding (apply the formatter to the offending file via a scoped remediation cycle, or fix main first) rather than deferring it — AC6 cannot pass otherwise. A one-file formatter-output change is acceptable scope to unblock the required check. See [[csharp-analyzer-packages-config-quirks]] and [[evidence-and-lifecycle-for-every-change]].
