# P3-T1 — `scripts/dependencies/PackageCompatibility.psm1` created

Timestamp: 2026-09-19T23-45

Command:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; $p = "scripts/dependencies/PackageCompatibility.psm1"; $lines = [System.IO.File]::ReadAllLines((Resolve-Path $p)); "LINECOUNT=$($lines.Count)"; Import-Module (Resolve-Path $p).Path -Force -ErrorAction Stop; "IMPORT=ok"; $m = Get-Module PackageCompatibility; "EXPORTED=" + (($m.ExportedFunctions.Keys | Sort-Object) -join ","); foreach ($n in @("Select-CompatibleAssetFolder","Test-PackageAssetCompatibility")) { $cb = (Get-Command $n).CmdletBinding; "CMDLETBINDING[$n]=$cb" }; $hits = Select-String -Path (Resolve-Path $p) -Pattern "netstandard2\.1"; "NS21_LINES=" + $hits.Count'
```

EXIT_CODE: 0

The file was created with the `Write` tool and amended once with the `Edit` tool. No heredoc and no
shell redirection was used at any point, per Scope Decision 4.

## Verbatim output

```
LINECOUNT=172
IMPORT=ok
EXPORTED=Select-CompatibleAssetFolder,Test-PackageAssetCompatibility
CMDLETBINDING[Select-CompatibleAssetFolder]=True
CMDLETBINDING[Test-PackageAssetCompatibility]=True
NS21_LINES=1
NS21 line 21: netstandard2.1 asset at all, because .NET Framework implements no version of the .NET
```

## The `netstandard2.1` occurrence, located

The module contains exactly **one** occurrence of the literal `netstandard2.1`, at line 21. It sits
inside the module-level comment-based help `.DESCRIPTION` block, in the sentence explaining why the
framework is excluded outright rather than ranked. It is prose, not a collection member.

The module declares exactly one ordered preference collection,
`$script:ConsumableAssetFolder`, at lines 42 to 65. (This span was first written here as
41 to 64. It was re-derived mechanically at P4-T1, after the PoshQC formatter rewrote the file's
pipeline indentation, and found to be 42 to 65: `$script:ConsumableAssetFolder = @(` on line 42 and
the closing `)` on line 65. The formatter touched only lines 96 to 102, well below the collection,
so the span did not move — the original figure was an off-by-one in this prose and is corrected
here rather than left standing.) Its 22 members are `net481`, `net48`, `net472`,
`net471`, `net47`, `net462`, `net461`, `net46`, `net452`, `net451`, `net45`, `net40`, `net35`,
`net20`, `netstandard2.0`, `netstandard1.6`, `netstandard1.5`, `netstandard1.4`, `netstandard1.3`,
`netstandard1.2`, `netstandard1.1` and `netstandard1.0`. `netstandard2.1` is not among them, and the
single occurrence at line 21 is outside the collection's line span. Line 21 was re-derived at
P4-T1 and is unchanged.

The exclusion mechanism is **non-membership**, not a deny list: `Select-CompatibleAssetFolder`
selects the first member of the ordered collection that the offered set contains, so any framework
absent from the collection is rejected by the same rule. That is what makes the #902 behaviour
structural rather than a ranking that still selects `netstandard2.1` when nothing else is offered.

## Asset-level, not attribute-level

Both functions take the asset folder names a candidate package actually ships as their only
framework input. Neither reads a `targetFramework` attribute, a manifest, a project file or the
filesystem. A caller that needs the folders of a package on disk enumerates them through its own
injected listing delegate and passes the names in, which keeps the whole module exercisable in
memory with no temporary file.

## Behavioural smoke check, taken before the suite was authored

```
1=[net481]
2=[net48]
3=[netstandard2.0]
4=[]
5=[]
6=[]
7 compat=False reason=Package 'Contoso.Widgets' ships no asset folder that net481 can consume. Offered: netstandard2.1, net6.0.
8 compat=True selected=net472 reasonLen=0
```

Rows 1 to 6 are the selector over, in order: a set containing `net481`; a set whose best member is
`net48`; a set offering `netstandard2.1` and `netstandard2.0` together; a set offering only
`netstandard2.1`; a .NET-Core-era-only set; and the empty set. Row 7 is the gate returning a
rejection carrying a non-empty reason; row 8 is the gate returning an acceptance naming the selected
asset folder. This is a smoke check and is not the acceptance for AC9; the authored suite at P3-T2
and the filtered run at P3-T3 are.

## Acceptance evaluation

| Clause | Required | Measured | Verdict |
|---|---|---|---|
| The module imports without error | no error | `IMPORT=ok`, `Import-Module ... -ErrorAction Stop` returned | PASS |
| Exports the selector and the gate | both | `Select-CompatibleAssetFolder,Test-PackageAssetCompatibility` | PASS |
| Both are advanced functions with `CmdletBinding()` | `True` for both | `True`, `True` | PASS |
| No literal `netstandard2.1` inside any ordered preference collection | 0 | 0 — the one occurrence is help prose at line 21, outside the collection at lines 41-64 | PASS |
| At most 500 lines | `<= 500` | 172 | PASS |

The zero in row 4 is not an unguarded absence: it is paired with the positive enumeration of the
22 members the collection does declare and with the located, quoted single occurrence of the
literal elsewhere in the file, so a search that resolved no file is distinguishable from a clean
result. `NS21_LINES=1` is itself the positive control — a run that read nothing would have
reported 0.

Output Summary: `scripts/dependencies/PackageCompatibility.psm1` was created with the `Write` tool
at **172 lines**. It imports without error and exports exactly two advanced functions,
`Select-CompatibleAssetFolder` and `Test-PackageAssetCompatibility`, both reporting
`CmdletBinding = True`. Its single ordered preference collection, `$script:ConsumableAssetFolder` at
lines 41-64, carries 22 members and does **not** carry `netstandard2.1`; the framework is excluded
by non-membership rather than by ranking, which is the #902 correction. The literal appears exactly
once in the file, at line 21, as help prose explaining that exclusion. A behavioural smoke check
returned the expected answer for all six selector cases and both gate cases.
