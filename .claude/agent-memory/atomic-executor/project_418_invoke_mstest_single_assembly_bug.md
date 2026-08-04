---
name: 418-invoke-mstest-single-assembly-bug
description: scripts/vscode/Invoke-MSTest.ps1 throws "property 'Count' cannot be found" whenever discovery finds exactly ONE test assembly, because StrictMode Latest rejects .Count on a scalar String
metadata:
  type: project
---

`scripts/vscode/Invoke-MSTest.ps1` cannot run a single-project scope. Line 115 evaluates
`$testAssemblies.Count` while line 77 sets `Set-StrictMode -Version Latest`. When
`Get-ChildItem ... | Select-Object -ExpandProperty FullName` matches exactly one file it returns a
scalar `System.String`, and StrictMode `Latest` rejects `.Count` on a scalar, so the script dies with
`The property 'Count' cannot be found on this object` before reaching vstest.

**Why:** Issue #418 `[P1-T9]`/`[P1-T23]` commanded
`Invoke-MSTest.ps1 -SearchRoot SVGControl.Test`, which discovers exactly one assembly
(`SVGControl.Test/bin/Debug/SVGControl.Test.dll`) and therefore can never execute. `-SearchRoot .`
finds nine assemblies (an array), so the Phase 0 baseline and repo-wide runs are unaffected — which is
why this stayed latent.

Verified in isolation:
```
pwsh -NoProfile -Command "Set-StrictMode -Version Latest; $s='one'; $s.Count"  -> throws
pwsh -NoProfile -Command "Set-StrictMode -Version Latest; $a=@('one','two'); $a.Count" -> 2
```

**How to apply:** If the script is outside your Scope Lock, do not edit it. Run the faithful
equivalent by calling `vstest.console.exe` with the argument list the script's own pure
`Get-VsTestArgumentList` builds — assemblies + `/Settings:scripts/vscode/TaskMaster.cli.runsettings` +
`/InIsolation` + `/TestCaseFilter:TestCategory!=LiveOutlook` — and record both the failed plan command
and the equivalent in the evidence artifact. Resolve `vstest.console.exe` via
`vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'`. Invoke
through `pwsh` rather than git-bash so `/`-prefixed switches are not path-mangled. The real fix, when
in scope, is `@($testAssemblies).Count`.
