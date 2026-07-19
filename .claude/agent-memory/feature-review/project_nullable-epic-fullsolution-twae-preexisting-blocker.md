---
name: nullable-epic-fullsolution-twae-preexisting-blocker
description: For utilitiescs-nullable-remediation epic children, the plan-literal full-solution pragma-only TWAE build exits 1 on pre-existing out-of-scope warnings, not on the child's work
metadata:
  type: project
---

Every child of the `utilitiescs-nullable-remediation` epic runs a per-file `#nullable enable`
opt-in with a pragma-only verification build (`msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true`,
deliberately WITHOUT `/p:Nullable=enable`). That full-solution command exits 1 for a pre-existing,
out-of-scope reason, not the child's annotations:

- `SVGControl/SvgImageSelector.cs` 4x `CS0649` (vendored, 2023 WIP fields never assigned). SVGControl
  builds early (project ref of UtilitiesCS) so the solution halts there.
- Non-HelperClasses `UtilitiesCS/EmailIntelligence/` + `Extensions/` `CS0618` (obsolete AsyncEnumerable
  APIs) + `CS0168`, promoted to errors by TWAE.

This is the P0-T4 baseline (present before any child edit). It surfaced only after commit `20d163ac`
changed the nullable gate from `/t:Build` (silent no-op) to `/t:Rebuild` (genuine recompile).

**Why:** These files are outside the maintainer scope lock (`UtilitiesCS/HelperClasses/` only), so a
child cannot fix them.

**How to apply:** Adjudicate the DoD/AC "full toolchain passes" item as PASS for the in-scope
obligation on the evidence that the ISOLATED build
`msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild ... /p:BuildProjectReferences=false` exits 0 with
zero CS86xx. Classify the full-solution TWAE exit 1 as pre-existing/out-of-scope, NOT a blocking
in-scope defect. #364 (utilitiescs-nullable-helperclasses) confirmed all 42 opted-in files clean;
`DvgForm.Designer.cs` stays oblivious (43rd file, not opted-in). Related: [[csharp-repowide-coverage-below-80]].
