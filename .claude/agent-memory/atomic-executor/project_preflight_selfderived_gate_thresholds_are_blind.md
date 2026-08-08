---
name: preflight-selfderived-gate-thresholds-are-blind
description: A plan gate whose threshold is computed from the same measurement it validates cannot detect the condition it exists for; check commensurability and derivation-independence during preflight
metadata:
  type: project
---

When preflight-validating a plan gate of the form "later measurement >= threshold recorded
earlier", check two things before accepting it:

1. **Derivation independence** — is the threshold computed from a measurement that would
   itself be degraded by the failure condition the gate detects? If yes, the gate is blind.
2. **Commensurability** — are the two numbers the same unit and the same scope?

**Why:** #230 cycle 3. A plan added a "silent unwired `*.Part2.cs` file" guard as
`P7-T8 records an expected-minimum test count = sum of the Phase 1-6 filtered-run executed
counts`, then `P8-T5 asserts full-run executed count >= that floor`. Both failure modes fired:
(a) an unwired file never compiles, so the *phase filtered run* already reports the deflated
count — floor and actual deflate together and the comparison passes; (b) `Invoke-MSTestWithCoverage.ps1`
discovers every first-party `*.Test.dll` repo-wide (thousands) while the floor came from
`QuickFiler.Test` filtered runs (tens), so the comparison was vacuous. Overlapping
`/TestCaseFilter` values across phases (three tasks shared `~InitializationTests`) also made
the sum triple-count. The script emits no TRX logger and hard-codes
`/TestCaseFilter:TestCategory!=LiveOutlook`, so a per-assembly count was not extractable.

**How to apply:** replace count-comparison gates with positive-existence proofs derived from
a source that does not change under the failure condition. For csproj-wiring guards in legacy
non-SDK projects the working pair is: (a) enumerate added files via `git status --porcelain` /
`git diff --name-only` and check each against `<Compile Include>`; (b) rebuild, then
`& $vstest <assembly>.dll /ListTests` and confirm every statically-enumerated `[TestMethod]`
name from source appears in discovery. Both are wiring-sensitive; the static source count does
not shrink when a file is unwired. `/ListTests` is valid on VSTest 18.8.0 in both the
positional (`<dll> /ListTests`) and colon (`/ListTests:<file>`) forms — verified by argument
parsing, which rejects only the missing file, not the switch. Related: [[project_legacy_csproj_no_transitive_compile_refs]],
[[project_vstest_testcasefilter_or_operator_and_env_setup]].
