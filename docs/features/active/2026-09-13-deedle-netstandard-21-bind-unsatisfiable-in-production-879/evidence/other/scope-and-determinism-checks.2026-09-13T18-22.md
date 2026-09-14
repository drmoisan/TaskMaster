# Scope and Determinism Checks — [P4-T9], [P4-T11]

Timestamp: 2026-09-14T11-42

Command:

```
pwsh -NoProfile -Command '
Set-Location -LiteralPath "<repo-root>"
$paths = @("UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs","TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs","TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs","TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs")
$banned = @("Thread.Sleep","Task.Delay","File.WriteAllText","File.WriteAllLines","File.WriteAllBytes","File.AppendAllText","File.Create","File.Delete","File.Move","File.Copy","Directory.CreateDirectory","Directory.Delete","Path.GetTempFileName","Path.GetTempPath","StreamWriter","DateTime.Now","DateTime.UtcNow")
foreach ($p in $paths) { foreach ($b in $banned) { Write-Output ($p + " " + $b + " HITS=" + @(Select-String -LiteralPath $p -SimpleMatch -CaseSensitive -Pattern $b).Count) } }
foreach ($p in $paths) { Write-Output ($p + " CONTROL_AppDomain HITS=" + @(Select-String -LiteralPath $p -SimpleMatch -CaseSensitive -Pattern "AppDomain").Count) }
'
```

The `Set-Location` prefix is mechanically necessary and measurement-neutral: this executor was launched
without worktree isolation, so its inherited working directory is a different checkout and every
repository-relative path in the plan would otherwise resolve into the wrong tree. It is disclosed here
rather than omitted.

EXIT_CODE: 0

Output Summary:

```
UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs Thread.Sleep HITS=0
UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs Task.Delay HITS=0
UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs File.WriteAllText HITS=0
UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs File.WriteAllLines HITS=0
UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs File.WriteAllBytes HITS=0
UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs File.AppendAllText HITS=0
UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs File.Create HITS=0
UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs File.Delete HITS=0
UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs File.Move HITS=0
UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs File.Copy HITS=0
UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs Directory.CreateDirectory HITS=0
UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs Directory.Delete HITS=0
UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs Path.GetTempFileName HITS=0
UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs Path.GetTempPath HITS=0
UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs StreamWriter HITS=0
UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs DateTime.Now HITS=0
UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs DateTime.UtcNow HITS=0
TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs Thread.Sleep HITS=0
TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs Task.Delay HITS=0
TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs File.WriteAllText HITS=0
TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs File.WriteAllLines HITS=0
TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs File.WriteAllBytes HITS=0
TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs File.AppendAllText HITS=0
TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs File.Create HITS=0
TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs File.Delete HITS=0
TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs File.Move HITS=0
TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs File.Copy HITS=0
TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs Directory.CreateDirectory HITS=0
TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs Directory.Delete HITS=0
TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs Path.GetTempFileName HITS=0
TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs Path.GetTempPath HITS=0
TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs StreamWriter HITS=0
TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs DateTime.Now HITS=0
TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs DateTime.UtcNow HITS=0
TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs Thread.Sleep HITS=0
TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs Task.Delay HITS=0
TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs File.WriteAllText HITS=0
TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs File.WriteAllLines HITS=0
TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs File.WriteAllBytes HITS=0
TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs File.AppendAllText HITS=0
TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs File.Create HITS=0
TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs File.Delete HITS=0
TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs File.Move HITS=0
TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs File.Copy HITS=0
TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs Directory.CreateDirectory HITS=0
TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs Directory.Delete HITS=0
TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs Path.GetTempFileName HITS=0
TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs Path.GetTempPath HITS=0
TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs StreamWriter HITS=0
TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs DateTime.Now HITS=0
TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs DateTime.UtcNow HITS=0
TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs Thread.Sleep HITS=0
TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs Task.Delay HITS=0
TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs File.WriteAllText HITS=0
TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs File.WriteAllLines HITS=0
TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs File.WriteAllBytes HITS=0
TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs File.AppendAllText HITS=0
TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs File.Create HITS=0
TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs File.Delete HITS=0
TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs File.Move HITS=0
TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs File.Copy HITS=0
TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs Directory.CreateDirectory HITS=0
TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs Directory.Delete HITS=0
TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs Path.GetTempFileName HITS=0
TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs Path.GetTempPath HITS=0
TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs StreamWriter HITS=0
TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs DateTime.Now HITS=0
TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs DateTime.UtcNow HITS=0
UtilitiesCS.Test/Bootstrap/AssemblyBindingFallbackTests.cs CONTROL_AppDomain HITS=4
TaskMaster.Test/Bootstrap/ChildDomainBindProbe.cs CONTROL_AppDomain HITS=7
TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs CONTROL_AppDomain HITS=9
TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs CONTROL_AppDomain HITS=1
```

Acceptance Condition: MET. All 68 banned-token lines end with `HITS=0`, and all four `CONTROL_AppDomain`
lines carry a count greater than 0 (4, 7, 9 and 1), so the search mechanism and each file path are live
and the zero counts are evidence rather than an artefact of a broken search.

---

# No-New-Deployment Sweep — [P4-T11]

Timestamp: 2026-09-14T11-44

Command:

```
pwsh -NoProfile -Command '
Set-Location -LiteralPath "<repo-root>"
Write-Output ("PACKAGES_CONFIG_CHANGED=" + @(git diff --name-only origin/main...HEAD -- **/packages.config).Count)
Write-Output ("PORCELAIN_PACKAGES_CONFIG=" + @(git status --porcelain --untracked-files=all -- **/packages.config).Count)
Write-Output ("NETSTANDARD_DLL_IN_PROJECTS=" + @(Select-String -Path "*/*.csproj" -SimpleMatch -CaseSensitive -Pattern "netstandard.dll").Count)
Write-Output ("FSHARP_REDIRECT_LINES=" + @(Select-String -Path "*/app.config" -CaseSensitive -Pattern "oldVersion=.0\.0\.0\.0-11\.0\.0\.0.").Count)
'
```

EXIT_CODE: 0

Output Summary:

```
PACKAGES_CONFIG_CHANGED=0
PORCELAIN_PACKAGES_CONFIG=0
NETSTANDARD_DLL_IN_PROJECTS=0
FSHARP_REDIRECT_LINES=15
```

Comparator, read from `evidence/baseline/write-set-decision.2026-09-13T18-22.md` line 99:

```
BASELINE_FSHARP_REDIRECT_LINES=15
```

Acceptance Condition: MET. `PACKAGES_CONFIG_CHANGED=0`; `PORCELAIN_PACKAGES_CONFIG=0`;
`NETSTANDARD_DLL_IN_PROJECTS=0`; and `FSHARP_REDIRECT_LINES` is 15, exactly equal to the
`BASELINE_FSHARP_REDIRECT_LINES` integer recorded at `[P0-T15]`, so this work changed the figure by zero.
The baseline figure is 15 rather than 0, so the comparison is discriminating.

No `netstandard.dll` entered any project and no `packages.config` was changed or created. The porcelain
span is the companion the name-listing diff needs: the diff enumerates tracked changes only and cannot
report a `packages.config` this work might have created, so the two spans together cover both states.
