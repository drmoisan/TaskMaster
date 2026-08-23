# F2 — Ribbon XML Size and Attribute Gates (Cycle 1, Issue #503)

> **SUPERSEDED — this measurement no longer describes the tree.** The P2-T1 collapse that produced the 524-line figure below was **reverted** at [P3-T2] because CSharpier 1.3.0 formats XML and mandates the multi-line form for these three lines once the `getEnabled` attribute is added (116 characters against a 100-column print width). `RibbonExplorer.xml` is back at **539** lines and takes a zero-line diff from this cycle. See `evidence/qa-gates/f2-formatter-conflict.2026-08-08T14-52.md` for the measured root cause and the escalation. The record below is retained as the evidence of the attempt.

Timestamp: 2026-08-08T14-52
Task: [P2-T2]
Command: `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; (Get-Content 'TaskMaster\Ribbon\RibbonExplorer.xml' | Measure-Object -Line).Lines; (Select-String -Path 'TaskMaster\Ribbon\RibbonExplorer.xml' -Pattern 'getEnabled=\"EngineCommand_GetEnabled\"' -AllMatches | Measure-Object).Count"`
EXIT_CODE: 0

Corroborating command: `wc -l TaskMaster/Ribbon/RibbonExplorer.xml`

## Output Summary

```text
524
8
```

`wc -l` independently reports **524**. The file contains no blank lines, so both counting methods agree.

### Line count against every reference point

| Reference point | Lines | Relation to the measured 524 |
|---|---|---|
| Merge-base `003c5715` (pre-feature) | 519 | +5 |
| Post-implementation-cycle (pre-remediation) | 539 | **-15** |
| **Measured post-F2** | **524** | — |
| F2 gate ceiling (519 merge-base + 8 functional attribute lines) | 527 | **3 lines under** |
| Repository 500-line cap | 500 | +24 (pre-existing overage; see below) |

The file is **at or below the 527 gate** and is **strictly below** its 539-line pre-remediation count, satisfying both conditions the F2 finding binds.

The residual +5 against the merge-base is the functional cost of the fix that survives the collapse: five of the eight engine-backed buttons (`TrainSpam`, `TrainHam`, `TestSpam`, `FilterTriageGroup`, `ClearTriage`) were **already** multi-line at the merge-base, so adding a `getEnabled` attribute to each added one line apiece. The three `TriageSet*` buttons were single-line at the merge-base and are single-line again, so they now cost zero added lines rather than the six each they cost before this remediation.

The 24-line overage against the repository 500-line cap is the **pre-existing** condition recorded as an accepted exception by `spec.md` AC25 (which quotes 519 as the accepted figure) and is not remediated here. `.claude/rules/general-code-change.md` applies the cap to production, test, and reusable script files; `RibbonExplorer.xml` is a declarative embedded UI resource. This cycle reduces the overage from 39 lines to 24; it does not eliminate it, and eliminating it would require splitting the resource, which is a separate and larger change.

### Attribute count

**8** occurrences of `getEnabled="EngineCommand_GetEnabled"`, unchanged from the pre-remediation count and from the merge-base-plus-fix expectation. All eight engine-backed controls (`TrainSpam`, `TrainHam`, `TestSpam`, `TriageSetA`, `TriageSetB`, `TriageSetC`, `FilterTriageGroup`, `ClearTriage`) retain the callback. The collapse changed layout only; it removed no attribute.

Binary outcome satisfied: the file is at or below 527 lines (measured **524**) and contains exactly **8** occurrences.
