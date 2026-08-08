---
name: nullable_build_gate_is_vacuous
description: TaskMaster's mandated type-check gate (msbuild /t:Build with /p:Nullable=enable) cannot fail, because MSBuild skips CoreCompile when only /p: values change; force /t:Rebuild on the changed project to get a real signal.
metadata:
  type: project
---

CLAUDE.md CUT3 step 3 is `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`. It reliably returns EXIT 0 — including on a tree with hundreds of real `CS86xx` nullable errors — because MSBuild's up-to-date check skips `CoreCompile` when only `/p:` property values change relative to the last build. A green result from that command proves nothing about nullability.

Real signal on #503 (2026-08-08), from `msbuild TaskMaster\TaskMaster.csproj /t:Rebuild /p:Configuration=Debug /p:Nullable=enable /p:TreatWarningsAsErrors=true`:

- EXIT 1, 195 errors, 64 of them `CS86xx`
- top offenders, all untouched: `OutlookItemTry.cs` 35, `OutlookItemFlaggableTry.cs` 30, `ItemInfo.cs` 20, `PropertyStore.cs` 17, `SubjectMapEntry.cs` 14, `BayesianClassifier.cs` 13
- **zero** errors in any file authored by the feature under review

Note the platform trap: at project level, `/p:Platform='Any CPU'` fails with `The BaseOutputPath/OutputPath property is not set`. Omit `/p:Platform` entirely for a single-project rebuild. (Solution level still needs `Any CPU`.) See [[msbuild-invocation-via-bash]].

**Why:** Reporting the solution gate as a PASS without qualification overstates the evidence; reporting the forced-rebuild error count as a feature FAIL wrongly blames the change for pre-existing repository debt. Both are wrong.

**How to apply:** Run the mandated command (it is the policy gate and its EXIT 0 is the DoD fact), then run the forced project rebuild and attribute the errors by filename. Grade the type-check PASS when zero errors are attributable to authored files, and raise the gate's vacuity as a separate governance finding. On #503 the executor had already found and recorded this in `evidence/qa-gates/msbuild-nullable.<ts>.md` and routed it for promotion, so check the evidence tree before treating it as a new discovery. Related but distinct: [[langversion-missing-test-projects-cs8630]] (#418), where test projects default to C# 7.3 and `/p:Nullable=enable` fails outright.
