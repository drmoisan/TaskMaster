# Phase 5 — Coverage Delta and Threshold Report (Issue #445)

Timestamp: 2026-08-22T10-40

Command:
```powershell
# (i)/(ii) root attributes from both Cobertura documents
([xml](Get-Content -Raw coverage\baseline.cobertura.xml)).coverage
([xml](Get-Content -Raw coverage\postchange.cobertura.xml)).coverage
# (iii) per-file aggregation by Cobertura filename across all matching class elements
# (iv) changed-line hit lookup: git diff -U0 -- QuickFiler/Controllers/KaStringAsync.cs
#      intersected with the post-change per-line hit map
git diff -U0 -- QuickFiler/Controllers/KaStringAsync.cs
```
Run from `WS` via `pwsh -NoProfile`. Sources: P0-T18 (`coverage\baseline.cobertura.xml`) and P5-T7 (`coverage\postchange.cobertura.xml`), both collected with the identical command and the identical `coverage\effective-coverage.config`.

EXIT_CODE: 0

---

## (i) and (ii) — Repository-wide line and branch percentages, baseline versus post-change

| Field | Baseline (P0-T18) | Post-change (P5-T7) | Delta |
|---|---|---|---|
| **Line rate** | `0.7059714463066419` = **70.60%** | `0.7060371689985226` = **70.60%** | +0.0066 pp |
| **Branch rate** | `0.5874059746400172` = **58.74%** | `0.5874693824932319` = **58.75%** | +0.0063 pp |
| `lines-covered` | 56866 | 56872 | +6 |
| `lines-valid` | 80550 | 80551 | +1 |
| `branches-covered` | 13666 | 13671 | +5 |
| `branches-valid` | 23265 | 23271 | +6 |

Both repository-wide rates moved slightly **upward**. Neither regressed.

---

## (iii) — Per-file covered/total line counts for the four production files, before and after

| File | Baseline covered/total | Baseline % | Post-change covered/total | Post-change % | Movement |
|---|---|---|---|---|---|
| `QuickFiler/Controllers/KaStringAsync.cs` | 49 / 49 | 100.00% | **60 / 60** | **100.00%** | held at 100% while gaining 11 executable lines |
| `QuickFiler/Controllers/KaChar.cs` | 28 / 33 | 84.85% | **28 / 28** | **100.00%** | **+15.15 pp** |
| `QuickFiler/Controllers/KaKey.cs` | 28 / 33 | 84.85% | **28 / 28** | **100.00%** | **+15.15 pp** |
| `QuickFiler/Interfaces/IKbdAction.cs` | 0 / 0 | not measurable | **0 / 0** | not measurable | unchanged |

Uncovered lines after the change: **none** in any measurable file. `KaChar.cs` and `KaKey.cs` each reached 100% by deleting exactly the 5 uncovered lines the P0-T18 artifact identified in advance (`45, 53, 54, 95, 96` in each: the `DelegateType` getter body plus the two dead `Update` accessor pairs). Their covered counts are unchanged at 28, so the gain is the removal of uncovered dead code, not a change in what the tests exercise.

`IKbdAction.cs` produces zero Cobertura `class` elements before and after. It is an interface-only file with no executable body; `.claude/rules/general-unit-test.md` recognises such files as legitimately reporting no executable coverage. This is a measurement fact, not an exclusion.

---

## (iv) — Changed-line coverage on `KaStringAsync.cs`

Method: `git diff -U0` was parsed to collect every new-file line number added or modified by this change, and each was looked up in the post-change per-line hit map built from all Cobertura `class` elements for the file.

| Measurement | Value |
|---|---|
| Added/modified lines in `KaStringAsync.cs` per `git diff` | 67 |
| Of those, **executable** (present in Cobertura) | **12** |
| Of those, **non-executable** (XML doc comment, `//` comment, braces the compiler elides) | 55 |
| **Added executable lines COVERED (hits >= 1)** | **12** |
| **Added executable lines UNCOVERED (hits = 0)** | **0** |
| **Newly-added production line coverage** | **12 / 12 = 100.00%** |

Every added executable line, with its recorded hit count:

```
LINE 110 hits=1 :: if (other is null)
LINE 111 hits=1 :: {
LINE 112 hits=1 :: throw new ArgumentNullException(nameof(other));
LINE 115 hits=1 :: if (other.Length == 0)
LINE 116 hits=1 :: {
LINE 117 hits=1 :: throw new ArgumentException(
LINE 118 hits=1 ::     "An empty probe is not a valid key. string.Contains(string.Empty) is true for "
LINE 119 hits=1 ::         + "every receiver, so an empty probe would otherwise match every registered "
LINE 120 hits=1 ::         + "action rather than none.",
LINE 121 hits=1 ::     nameof(other)
LINE 122 hits=1 :: );
LINE 138 hits=1 :: if (Activated && Update is not null)
```

Lines 110-112 are the null guard, exercised by `KeyEquals_NullProbe_ThrowsArgumentNullExceptionNamingOther`. Lines 115-122 are the empty guard, exercised by both variants of `KeyEquals_EmptyProbe_ThrowsArgumentExceptionNamingOther`. Line 138 is the branch-3 gate this issue fixes, exercised by `KeyEquals_MultiCharNonMatchWhileNotActivated_DoesNotInvokeUpdateAndReturnsFalse` and by the renamed `KeyEquals_MultiCharNonMatchWhileActivated_InvokesUpdateWithFirstCharAndReturnsFalse` at both `Activated` states. Every changed line has a test that reaches it.

### No changed line that was covered at baseline is uncovered after the change

`KaStringAsync.cs` had **zero** uncovered lines at baseline (49/49) and has **zero** uncovered lines after (60/60). No line in the file regressed from covered to uncovered, so the condition holds vacuously and directly. The one **modified** (not merely added) line, the branch-3 guard, was covered at baseline as `if (Update is not null)` and is covered after as `if (Activated && Update is not null)` with hits = 1.

`KaChar.cs` and `KaKey.cs` changed only by deletion. No line that was covered at baseline in either file became uncovered; their covered counts are identical before and after (28 each). `IKbdAction.cs` lost two comment lines, which were never executable.

---

## (v) — Comparison against BOTH threshold sets

A threshold divergence exists in this repository. It is **pre-existing and unadjudicated**, and it is **not resolved by this issue**. Both figures are reported.

### Threshold set A — CLAUDE.md UT2 and `.claude/rules/csharp.md`

| Requirement | Threshold | Measured | Verdict |
|---|---|---|---|
| Repository-wide line coverage | `>= 80%` | 70.60% (baseline 70.60%) | below threshold, **pre-existing**, not a blocking gate for this bugfix |
| New modules/classes/methods | `>= 90%` | **100.00%** on newly-added production lines | **PASS** |
| No coverage reduction on changed lines | no regression | **no regression** (0 uncovered before, 0 after) | **PASS** |

### Threshold set B — `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md`

| Requirement | Threshold | Measured | Verdict |
|---|---|---|---|
| Line coverage, uniform T1-T4 | `>= 85%` | 70.60% (baseline 70.60%) | below threshold, **pre-existing**, not a blocking gate for this bugfix |
| Branch coverage, uniform T1-T4 | `>= 75%` | 58.75% (baseline 58.74%) | below threshold, **pre-existing**, not a blocking gate for this bugfix |
| No regression on changed lines | no regression | **no regression** | **PASS** |

### Statement on the divergence

Threshold set A requires repository-wide line coverage `>= 80%` and sets no branch-coverage floor. Threshold set B requires line `>= 85%` and branch `>= 75%` uniformly across all tiers. The two sets disagree on the line floor (80 versus 85) and on whether a branch floor exists at all. **This divergence is pre-existing, is not adjudicated by issue #445, and is not resolved here.** It is recorded as awareness-only in the spec's Rollout & Follow-up section. Reporting against both sets, rather than selecting the more favourable one, is what the plan's Coverage Policy Position requires.

The repository-wide shortfall against both sets was measured **before any edit in this plan** (P0-T18: 70.60% line, 58.74% branch) and is therefore pre-existing repository state that this change did not create. This change moved both rates marginally upward.

---

## Blocking-gate verdict

The plan fixes two blocking coverage conditions and designates the repository-wide figure as tracked but non-blocking for this bugfix.

| Blocking condition | Requirement | Measured | Verdict |
|---|---|---|---|
| Newly-added production line coverage | `>= 90%` | **100.00%** (12 of 12) | **PASS** |
| No changed line covered at baseline is uncovered after | no regression | **no regression** on any of the four production files | **PASS** |

**Both blocking coverage gates PASS.**

The repository-wide figure is reported and tracked above and is not a blocking gate for this bugfix, being pre-existing state this change does not create and does in fact marginally improve.

---

## Coverage-exemption position

**No coverage exemption is sought.** `coverage.config` is unchanged and reports 0 dirty lines under `git status --porcelain`. No `[ExcludeFromCodeCoverage]` attribute was added to any file. None of the four in-scope production files falls under any limb of the CLAUDE.md UT2 COM/VSTO/WinForms exemption: `KaStringAsync`, `KaChar`, and `KaKey` are pure in-memory value objects with no Outlook Interop, COM, or WinForms-form dependency, and `IKbdAction` is an interface. CLAUDE.md UT2 additionally names `KbdActions<>` explicitly as a testable seam that is **not** exempt, and these are its element types.

The only exclusion applied anywhere is the `.*\.Test\.dll$` module pattern in the derived `coverage\effective-coverage.config`, which removes the nine **test** assemblies from the denominator. That is required by CLAUDE.md UT2 ("Configure coverage tooling to exclude test files ... so metrics reflect the application code, not the tests themselves") and by the `.claude/rules/general-unit-test.md` permitted-exclusions list. No production path is excluded.

Output Summary: (i) Baseline repository-wide coverage is line 70.60% (`0.7059714463066419`) and branch 58.74% (`0.5874059746400172`). (ii) Post-change is line 70.60% (`0.7060371689985226`) and branch 58.75% (`0.5874693824932319`); both moved marginally upward and neither regressed. (iii) Per-file counts moved `KaStringAsync.cs` 49/49 to 60/60 (100% held while gaining 11 executable lines), `KaChar.cs` 28/33 to 28/28 (84.85% to 100%, +15.15 pp), `KaKey.cs` 28/33 to 28/28 (84.85% to 100%, +15.15 pp), and `IKbdAction.cs` 0/0 to 0/0 (interface-only, no executable line); no uncovered line remains in any measurable file. (iv) Of 67 lines added or modified in `KaStringAsync.cs`, 12 are executable and **all 12 have hits >= 1, giving newly-added production line coverage of 100.00%**; the per-line hit table is recorded above, and no changed line that was covered at baseline is uncovered after, holding both directly and because the file had zero uncovered lines before and after. (v) Against CLAUDE.md UT2 (`>= 80%` repository line, `>= 90%` new code) the new-code gate PASSES at 100% and the repository figure is below threshold; against `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` (`>= 85%` line, `>= 75%` branch) both repository figures are below threshold. Those repository-wide shortfalls were measured before any edit in this plan, are pre-existing and unadjudicated, are explicitly not resolved by this issue, and are not blocking gates for this bugfix. **Both blocking coverage gates PASS**: newly-added production line coverage is 100.00% (requirement `>= 90%`) and there is no coverage regression on any changed line. No coverage exemption is sought, `coverage.config` is unmodified, and no `[ExcludeFromCodeCoverage]` attribute was added.
