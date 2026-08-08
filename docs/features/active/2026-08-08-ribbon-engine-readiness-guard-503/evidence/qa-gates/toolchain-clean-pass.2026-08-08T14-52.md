# Phase 3 QC Step 12 — Single Uninterrupted Clean Toolchain Pass (Cycle 1, Issue #503)

Timestamp: 2026-08-08T14-52
Task: [P3-T12]

## The recorded pass, in order

| Order | Task | Gate | Artifact | `EXIT_CODE:` |
|---|---|---|---|---|
| 1 | **P3-T1** | Format (CSharpier, scope-locked) | `evidence/qa-gates/csharpier-format.2026-08-08T14-52.md` | **0** |
| 2 | **P3-T2** | Format verification (CSharpier, repo-wide, read-only) | `evidence/qa-gates/csharpier-check.2026-08-08T14-52.md` | **0** |
| 3 | **P3-T4** | Lint (MSBuild analyzers) | `evidence/qa-gates/msbuild-analyzers.2026-08-08T14-52.md` | **0** |
| 4 | **P3-T5** | Type-check (MSBuild nullable) | `evidence/qa-gates/msbuild-nullable.2026-08-08T14-52.md` | **0** |
| 5 | **P3-T6** | Test with coverage | `evidence/qa-gates/tests-with-coverage.remediation.2026-08-08T14-52.md` | **0** |

All five gates ran **in one pass, in the order above, with no restart between them.**

## No intervening source change during the pass

The two scope-locked source paths were fingerprinted at the end of the pass:

| Path | `git hash-object` at end of pass | Changed during the pass |
|---|---|---|
| `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs` | `7d422ef399d5be44176acb629a0199bddcf6ff93` | **no** |
| `TaskMaster/Ribbon/RibbonExplorer.xml` | `9d8403ee3d2e7f02c6d29d73efb25f9e065b461e` | **no** |

The `.cs` hash is identical to the value recorded on **both** sides of the P3-T1 formatting invocation, which is direct evidence that CSharpier did not rewrite the file and that nothing altered it afterwards. `git diff --numstat` over both paths reports `12  3` for the `.cs` file and **no line at all** for the `.xml` file, confirming the XML takes a zero-line diff.

No `.cs`, `.csproj`, `.xml`, or `.sln` file changed on disk between P3-T1 and P3-T6. Writing this phase's own Markdown and JaCoCo evidence artifacts under `<FEATURE>\evidence\` is not an intervening file change, per the phase's own loop semantics.

## Restart history, disclosed

This is the **second** Phase 3 attempt. The first attempt is disclosed in full rather than omitted:

| Attempt | P3-T1 | P3-T2 | Outcome |
|---|---|---|---|
| 1 | exit 0 | **exit 1** — `TaskMaster\Ribbon\RibbonExplorer.xml` reported unformatted | **aborted at P3-T2** |
| **2 (recorded above)** | exit 0 | exit 0 | ran through P3-T6 with no restart |

The first attempt failed because the P2-T1 XML collapse is rejected by CSharpier 1.3.0: the single-line form is 116 characters against a 100-column print width once the required `getEnabled` attribute is present, so the formatter mandates the multi-line form. The cause was fixed by reverting the P2-T1 collapse, and the phase restarted from P3-T1 exactly as its loop semantics require. Measured root cause and escalation: `evidence/qa-gates/f2-formatter-conflict.2026-08-08T14-52.md`.

The restart occurred **before** the recorded pass began. The recorded sequence P3-T1 → P3-T2 → P3-T4 → P3-T5 → P3-T6 itself **contains no restart**.

## Supporting gates in the same pass

These ran within the same pass and also passed, and none of them mutated a source file:

| Task | Gate | Artifact | Result |
|---|---|---|---|
| P3-T3 | Post-format file-size audit | `evidence/qa-gates/file-size-audit.2026-08-08T14-52.md` | `.cs` at 318/500 PASS; `RibbonExplorer.xml` at 539 recorded as an unmet F2 objective, escalated |
| P3-T7 | Coverage projection | `evidence/qa-gates/coverage-projection.2026-08-08T14-52.md` | exit 0 |
| P3-T8 | Canonical gate artifact | `evidence/qa-gates/coverage-gate-artifact.2026-08-08T14-52.md` | exit 0; LINE 85.8561 >= 85, BRANCH 79.2702 >= 75 |
| P3-T9 | Coverage comparison | `evidence/qa-gates/coverage-comparison.2026-08-08T14-52.md` | no regression; both rates up |
| P3-T10 | AC15 zero-line diff (working tree) | `evidence/qa-gates/zero-line-diff.2026-08-08T14-52.md` | empty output |
| P3-T11 | Scope-lock audit | `evidence/qa-gates/scope-lock-audit.2026-08-08T14-52.md` | bucket (d) empty |

Binary outcome satisfied: the recorded sequence contains no restart.
