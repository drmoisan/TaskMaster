---
name: nullable-pragma-gate-mechanics
description: How to actually verify the epic's per-file #nullable enable gate on UtilitiesCS — solution-wide TWAE aborts on vendored non-nullable warnings; use an isolated csproj rebuild and grep CS86xx
metadata:
  type: project
---

The utilitiescs-nullable-remediation epic (#363 and its Wave-1 siblings) remediates pre-existing CS86xx debt under a per-file `#nullable enable` opt-in, verified with `msbuild ... /t:Rebuild ... /p:TreatWarningsAsErrors=true` WITHOUT `/p:Nullable=enable`.

**Why:** The literal solution-wide command `msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true` CANNOT produce a clean pass and is NOT a usable AC1 proof: global TWAE promotes pre-existing NON-nullable warnings to errors, and under `-m` the vendored `SVGControl` compile fails first (CS0649 `_relativeImagePath`/`_absoluteImagePath`) and aborts the graph before UtilitiesCS even recompiles (exit 1, ~0.4s). UtilitiesCS itself also carries pre-existing CS0168 x2 + CS0618 x28 that TWAE promotes.

**How to apply:** Verify with the ISOLATED csproj gate that recompiles all target sources in one assembly:
`msbuild UtilitiesCS/UtilitiesCS.csproj -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU -p:TreatWarningsAsErrors=true -p:BuildProjectReferences=false` (dash-switches + MSYS_NO_PATHCONV=1 in git-bash; a single legacy csproj needs `Platform=AnyCPU` no-space, NOT "Any CPU"; `BuildProjectReferences=false` uses already-built vendored DLLs so their warnings don't derail you — first do a normal `msbuild TaskMaster.sln -t:Build` to produce SVGControl.dll etc.). The AC1 metric is `grep -coE "error CS86[0-9]{2}"` on the log (must be 0); the non-zero build exit from CS0168/CS0618 is expected pre-existing noise, not a nullable failure. Record both the literal solution-command result AND the isolated-gate CS86xx=0 as evidence. See [[nullable_remediation_annotation_patterns]].
