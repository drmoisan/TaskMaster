# Fail-Before Exception Dossier — R2, Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T08-37-50
- Task: [P1-T1]
- Finding: R2 — `scripts/vscode/Sync-PackageReferences.ps1` negative and error paths untested
- Scope: the eight tests [P1-T2] through [P1-T9] add to
  `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1`

## WhyFailingRunImpossible

The eight tests assert behaviour the production file **already implements correctly**. No test can
be shown failing before a fix, because there is no fix: the defect R2 reports is the **absence of
tests**, not a wrong behaviour.

Concretely, each of the nine lines is a `return` or a `Write-Warning` on a path the file already
takes. Running any of the eight new tests before this phase would pass, so a "failing run before"
does not exist and cannot be manufactured without first breaking the production file, which would
be a fabricated red rather than evidence.

This is the case the `evidence-and-timestamp-conventions` skill anticipates: when a failing run is
structurally impossible, an exception dossier supplying an alternative proof discharges the
fail-before requirement.

## Alternative Proof — The Uncovered-Line List, Quoted Verbatim

Source artifact, cited by path:
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/remediation-baseline/p0-t8-pester.2026-09-20T01-37.md`

The `UNCOVERED=` list that artifact recorded for `scripts/vscode/Sync-PackageReferences.ps1`,
quoted verbatim:

```
UNCOVERED=60,61,63,64,66,68,69,71,73,74,76,78,80,82,84,86,88,90,91,151,180,248,290,293,330,336,337,345,387,390,410,422
```

The nine members that matter, each present in that list:

| # | Line | Owning function | Behaviour left unexercised |
|---|---|---|---|
| 1 | **151** | `Resolve-ManifestPackageId` | `return ''` when no manifest identifier prefixes the folder |
| 2 | **180** | `Resolve-PackageAssetFolder` | `return ''` when the library directory is absent |
| 3 | **248** | `Get-HintPathRepair` | the issue #902 rejection warning |
| 4 | **290** | `Repair-ProjectReferenceVersion` | early return, no matching `Include` |
| 5 | **293** | `Repair-ProjectReferenceVersion` | early return, the version already agrees |
| 6 | **330** | `Invoke-ProjectReferenceSync` | no project file beside the manifest |
| 7 | **336** | `Invoke-ProjectReferenceSync` | the conflict-marker warning |
| 8 | **337** | `Invoke-ProjectReferenceSync` | the corresponding skip return |
| 9 | **345** | `Invoke-ProjectReferenceSync` | the empty repair set |

**All nine are present in the quoted list.** That is the proof of absence this dossier supplies in
place of a failing run: the behaviour is implemented and no test reaches it.

The owning function names are the real ones, read from the production file. The review named
`Get-PackageIdentifier` for line 151 and `Set-ReferenceAssemblyVersion` for lines 290 and 293;
neither identifier exists in the file. The review's line numbers are correct.

## The Task That Must Observe the Complement

**[P1-T10]** is the task that must observe every one of the nine **absent** from the same
`UNCOVERED=` list, read from the same `CMD-JACOCO-PERFILE` expression against
`coverage/p1-t10-pester-coverage.xml`. Its acceptance checks each of the nine individually and
records each as covered or not, so a partial discharge is visible rather than averaged away.

[P1-T10] additionally asserts that the file's covered plus missed is still **127**. That is what
keeps this dossier's line citations valid: Phase 1 edits no production file, so the instrumented
count cannot move, and a moved count would mean the nine numbers no longer name the nine
behaviours.

## Eight Tests, Nine Lines

Lines 336 and 337 are discharged by one test, [P1-T8], because the warning and its skip return are
one behaviour and splitting them would give two tests with one observable outcome between them.
Every other line has its own test.

| Task | Line or lines |
|---|---|
| [P1-T2] | 151 |
| [P1-T3] | 180 |
| [P1-T4] | 248 |
| [P1-T5] | 290 |
| [P1-T6] | 293 |
| [P1-T7] | 330 |
| [P1-T8] | 336 and 337 |
| [P1-T9] | 345 |

## Output Summary

A failing run before the fix is structurally impossible for R2, because the finding is the absence
of tests over already-correct behaviour rather than a wrong behaviour. The alternative proof is the
[P0-T8] `UNCOVERED=` list, quoted verbatim above, which contains all nine target lines. [P1-T10] is
the task that must observe all nine absent from the same list.
