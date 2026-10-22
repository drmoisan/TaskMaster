# R5 Fail-Before — The Reference Assembly Version Is Rewritten to the Package Version

- Timestamp: 2026-09-20T08-49-40
- Task: [P2-T2] **[expect-fail]**
- Finding: R5
- Command: CMD-PESTER-FILTERED, `<FILE>` = `tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1`,
  `<FILTER>` = `*R5- preserves a Reference assembly version*`
- EXIT_CODE: 1
- ExpectedExitCode: 1

## Test Added

`It 'R5- preserves a Reference assembly version the package version does not track'`

Drives `Invoke-ProjectConsistencyRepair`, the module's exported entry point, over a project text
whose `<Reference Include="Contoso.Widgets, Version=4.5.6.7, ..." />` declares an assembly version
unrelated to the manifest's declared package version of `2.0.0`. Every folder segment in the
fixture already agrees with the manifest, so the Reference line is the only element the
reconciliation could touch.

It asserts two things: the returned `ProjectText` still contains `Version=4.5.6.7`, and no repair
record carries `Kind` equal to `Reference`.

The entry point is called exactly as a consumer can call it. It exposes no parameter through which
a caller could supply a resolved assembly version, which is the substance of R5: the failure mode
is one its callers cannot avoid.

## Fixture Provenance

The Reference-element shape reuses the one the existing `Invoke-VersionReconciliation` cases in
`tests/scripts/dependencies/ProjectConsistency.Tests.ps1` already exercise — the
`Include="<Id>, Version=<n>, Culture=neutral, processorArchitecture=MSIL"` form at lines 19, 35,
46, 49, 62 and 65 of that file. That reuse is the proof that the parser classifies such a line as
a reconciled `Reference` element and that the rewrite path is **live rather than unreachable**: a
fixture the parser did not recognise would produce no repair at all and the test would pass for
the wrong reason.

## Counts Line, Verbatim

```
PESTER Passed=0 Failed=1 Skipped=0 Executed=1 Total=12 NotRun=11
```

| Measurement | Required | Measured | Result |
|---|---|---|---|
| `Executed` = `Passed + Failed + Skipped` | 1 | **1** | PASS |
| `Failed` | 1 | **1** | PASS |
| `EXIT_CODE` | 1 | **1** | PASS |
| `ExpectedExitCode` | 1 | 1 | declared |

## The Failure Message, Verbatim

```
Expected like wildcard '*Version=4.5.6.7*' to match '<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0">
  <ItemGroup>
    <Reference Include="Contoso.Widgets, Version=2.0.0, Culture=neutral, processorArchitecture=MSIL">
      <HintPath>..\packages\Contoso.Widgets.2.0.0\lib\net472\Contoso.Widgets.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>', because an assembly version is not required to track its package version, so the entry point must preserve it, but it did not match.
```

The message shows the defect directly: the `Include` attribute the fixture supplied as
`Version=4.5.6.7` comes back as **`Version=2.0.0`**, the manifest's package version. That is the
R5 mechanism verbatim — `Invoke-ProjectConsistencyRepair` calls `Invoke-VersionReconciliation`
without `-AssemblyVersion`, the parameter defaults to the empty string,
`$resolvedAssemblyVersion` falls back to `$ManifestVersion`, and
`Get-RewrittenReferenceVersionLine` writes the package version into the assembly-version slot.

## Why a Passing Test Here Would Be a Failure of This Task

If this test passed before [P2-T3], the defect the review reported would not be present and
[P2-T3] would have nothing to repair. It failed, and the failure message names the exact
substitution the review described.

## Output Summary

The test is **red before the fix**, exit 1 as expected, with the failure message showing
`Version=4.5.6.7` rewritten to `Version=2.0.0`. [P2-T4] records the pass-after half.
