# P4-T4 — Host-Neutrality and Coverage-Exemption Audit (AC-14)

Timestamp: 2026-08-08T21-13

Command:

```
pwsh -NoProfile -Command "Set-Location '<REPO>'; Select-String -Path 'TaskMaster\Ribbon\EngineToggleCatalog.cs','TaskMaster\Ribbon\EngineToggleStateCoordinator.cs' -Pattern 'Microsoft\.Office\.|Microsoft\.Office\.Interop\.Outlook|MessageBox|System\.Windows\.Forms|ExcludeFromCodeCoverage'; git diff f910ff2f21c67a03cf8eebcb340727d5415d8e08..HEAD | Select-String -Pattern '^[+-].*ExcludeFromCodeCoverage'"
```

Executed through a scratchpad `.ps1` so the nested quoting survives intact; the commands,
patterns, and paths are exactly as written above.

EXIT_CODE: 0

## Output Summary — the four required facts

### Fact 1 — zero `Microsoft.Office.` and zero `Microsoft.Office.Interop.Outlook` references

| Token | Matches in the two new files |
|---|---|
| `Microsoft\.Office\.` | 2 — **both are prose inside XML doc comments**, not code |
| `Microsoft\.Office\.Interop\.Outlook` | **0** |

The two `Microsoft.Office.` matches are, verbatim:

```
EngineToggleCatalog.cs:31: /// data with no COM and no <c>Microsoft.Office.*</c> reference, and is fully unit-tested. It
EngineToggleStateCoordinator.cs:37: /// decision logic with no COM, no <c>Microsoft.Office.*</c> reference, no <c>MessageBox</c>,
```

Both occur inside `<c>...</c>` markup in a `<remarks>` block that asserts the absence of the very
reference it names. Neither file has a `Microsoft.Office` `using` directive: the complete `using`
sets are `System`, `System.Collections.Generic`, `System.Collections.ObjectModel`
(`EngineToggleCatalog.cs`) and `System`, `System.Collections.Concurrent`, `System.Globalization`,
`System.Threading`, `System.Threading.Tasks`, `UtilitiesCS`
(`EngineToggleStateCoordinator.cs`).

### Fact 2 — zero `MessageBox` and zero WinForms types

| Token | Matches |
|---|---|
| `MessageBox` | 1 — prose, `EngineToggleStateCoordinator.cs:37`, `no <c>MessageBox</c>` |
| `System\.Windows\.Forms` | **0** |

No executable `MessageBox` reference and no WinForms type exists in either file. The blocked-click
notice reaches `MessageBox.Show` only through the injected `notifyUnavailable` delegate, whose
production implementation is `RibbonController.NotifyEngineCommandNotReady` inside the exempt
shim.

### Fact 3 — no `[ExcludeFromCodeCoverage]` attribute in the two new files

`ExcludeFromCodeCoverage` matches: 2, both prose:

```
EngineToggleCatalog.cs:30: /// This type is deliberately NOT marked <c>[ExcludeFromCodeCoverage]</c>: it is host-neutral
EngineToggleStateCoordinator.cs:36: /// This type is deliberately NOT marked <c>[ExcludeFromCodeCoverage]</c>: it is host-neutral
```

Neither file declares the attribute. Both are therefore in the coverage denominator, which is what
the P5-T7 0.90 floor measures.

### Fact 4 — the branch diff adds no attribute, and removes none

`git diff <MERGE_BASE>..HEAD | Select-String '^[+-].*ExcludeFromCodeCoverage'`:

| Classification | Count |
|---|---|
| Total matching diff lines | 25 |
| **Removed (`-`) lines** | **0** |
| Added (`+`) lines | 25 |
| Added lines that are an **actual attribute declaration** (`^\+\s*\[ExcludeFromCodeCoverage\]`) | **0** |

All 25 added lines are Markdown prose in the feature folder (`spec.md`, `issue.md`, the plan, the
research artifact) or C# XML doc-comment prose in the two new files. Zero removed lines means **no
existing exemption attribute was removed or moved**: `RibbonViewer.cs:32` and
`RibbonController.cs:36` keep their type-level attributes untouched, corroborated by the P4-T2
zero-line diff on `RibbonViewer.cs`.

Binary outcome: PASS — all four facts established. The ratified VSTO/COM exemption is neither
removed, widened, nor added to.
