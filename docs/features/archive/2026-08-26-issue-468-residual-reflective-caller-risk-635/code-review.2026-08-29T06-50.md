# Code Review — Issue #635 Residual Reflective-Caller Risk

- **Issue:** #635
- **Branch:** `bug/issue-468-residual-reflective-caller-risk-635`
- **Head reviewed:** `73bd8082`
- **Timestamp:** 2026-08-29T06-50
- **Verdict:** PASS — 0 blocking findings, 8 non-blocking observations

## Scope of This Review

The branch contains no executable code. All 32 changed paths are Markdown. Conventional code-quality
review dimensions — naming, error handling, module cohesion, API surface, dependency hygiene — have no
subject matter in this diff.

This review therefore assesses the artifacts as **evidence engineering**: whether each command is
reproducible, whether each recorded figure is accurate, whether each negative claim carries the scope
that makes it meaningful, and whether the argument structure supports the conclusions drawn.

Every load-bearing measurement was re-executed at review head rather than accepted from the artifact.

## Findings

| ID | Severity | Location | Finding |
|---|---|---|---|
| NB-1 | Non-blocking | `evidence/qa-gates/p4-t2-no-modification-proof.2026-08-29T04-55.md` | Union of 28 paths predates the final commit; final diff is 32 |
| NB-2 | Non-blocking | `evidence/other/p2-t1-reflection-inventory.2026-08-29T04-55.md` | Narrower reflection patterns than the specification's baseline list |
| NB-3 | Non-blocking | `evidence/other/p3-t3-decision-record.2026-08-29T04-55.md` | `dynamic` late binding not enumerated as a mechanism |
| NB-4 | Non-blocking | `spec.md` lines 254-264, AC-9 | Superseded "six variable-argument sites" figure retained |
| NB-5 | Non-blocking | `evidence/other/p2-t4-binding-serialization-surface.2026-08-29T04-55.md` | Per-type `ComVisible(true)` override not measured |
| NB-6 | Non-blocking | `evidence/other/p2-t3-variable-argument-closure.2026-08-29T04-55.md` | Inconsistent line-number semantics in the site table |
| NB-7 | Non-blocking | `evidence/baseline/p0-t5-scope-census.2026-08-29T04-55.md` | AC-16's own 398-file figure never reconciled with the 153-file comparable scope |
| NB-8 | Non-blocking | Repository state | PR context artifacts absent |

---

### NB-1 — The no-modification proof's union predates the final commit

**Location:** `evidence/qa-gates/p4-t2-no-modification-proof.2026-08-29T04-55.md`, `UNION_PATHS: 28`.

`[P4-T2]` recorded a 28-path union and `[P4-T3]` branched on it. The final branch diff at head
`73bd8082` contains 32 paths — the four additional paths are the `[P4-T2]`, `[P4-T3]`, `[P4-T4]` and
`[P4-T7]` artifacts themselves, plus the `[P3-T4]` and decision artifacts committed in the same
sequence, none of which could appear in a proof written before they existed.

This is an inherent self-reference limitation, not an error of method: an artifact proving "no
production file is modified" cannot enumerate itself or its successors. The artifact is explicit that
"both commands were run and their output captured before this artifact was written".

**Impact: none on the conclusion.** I verified the property over the *final* 32-path diff
independently: `NON_MD_COUNT: 0`. The branch-two selection in `[P4-T3]` remains correct against the
final diff, because the four later paths are all Markdown under the feature folder.

**Recommendation:** for future Markdown-only items, note in the artifact that the union is a snapshot
at the time of writing and that the final diff will be larger by the count of subsequently written
artifacts. The reviewer's independent re-measurement is the durable check.

---

### NB-2 — Reflection inventory used narrower patterns than the specification's baseline

**Location:** `evidence/other/p2-t1-reflection-inventory.2026-08-29T04-55.md`, pattern list.

The specification's baseline (lines 250-252) describes a combined search including the bare tokens
`Activator.` and `CreateDelegate`. The executed inventory used `Activator.CreateInstance` and
`Delegate.CreateDelegate` instead. Both are strictly narrower: `Delegate.CreateDelegate` misses the
instance form `methodInfo.CreateDelegate(...)`, and `Activator.CreateInstance` misses
`Activator.CreateInstanceFrom(...)`.

A narrower pattern than the one the specification promised is the failure mode that could hide a real
hit, so I tested the broader forms directly:

```
CreateDelegate    prod=0
Activator.        prod=0
GetRuntimeMethod  prod=0
GetRuntimeField   prod=0
Reflection.Emit   prod=0
GetCustomAttribute prod=0
```

**Impact: none.** The bare forms also return zero over the production tree, so the substitution
concealed nothing. The finding is that the artifact does not record the substitution or justify it.

**Recommendation:** record pattern substitutions explicitly when they narrow a specification-stated
pattern, with the broader form's measured result alongside.

---

### NB-3 — `dynamic` late binding is not enumerated as a name-resolution mechanism

**Location:** `evidence/other/p3-t3-decision-record.2026-08-29T04-55.md`, "Classes of caller proved
absent" table.

The reflection inventory covers the `System.Reflection` API surface, WinForms property-name binding,
serialization attributes, and COM late binding. It does not cover C# `dynamic`, which resolves member
names at run time through the DLR and — importantly for this item's closure argument — **does not
require the member name to appear as a string literal anywhere**. A `dynamic` call site is therefore a
mechanism that would escape both the identifier sweep and the literal-bounded closure argument.

I measured it:

```
git grep -n -I -F -e "dynamic " -- "QuickFiler/*"
QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:10:// Allows Moq's dynamic proxy generator to mock the internal IFolderScoringService seam in tests.
git grep -c -I -F -e "dynamic " -- "QuickFiler.Test/*"   EXIT: 1
```

The single production hit is a comment about Moq's proxy generator, not a `dynamic` declaration, and
the test tree contains none.

**Impact: none on the conclusion.** The mechanism is verifiably absent from both trees, so the
decision record's closure stands. The finding is that the mechanism was never named, so a reader
cannot tell from the artifacts alone whether it was considered and excluded or simply overlooked.

**Recommendation:** add `dynamic` and `IDynamicMetaObjectProvider` to the pattern list of any future
sweep of this kind, and add a row to the decision record's table. This is the one mechanism class in
the same family as the stated limit (runtime-assembled names) that the inventory does not touch.

---

### NB-4 — `spec.md` retains the superseded six-site figure

**Location:** `spec.md` lines 254-264 (Verified Baseline Measurements) and AC-9 at line 348.

The specification states "Six `GetField(` call sites take a `string name` variable against
`typeof(QfcCollectionController)`" and AC-9 is worded "Each of the six variable-argument reflection
call sites". The mechanical derivation yields eight: seven `GetField(` sites and one `GetMethod(`
site. I verified all eight in source.

The executor's disposition — enumerate all eight, record the divergence, decline to edit the approved
specification — is correct and is the only route compliant with the acceptance-criteria-tracking
protocol. AC-9 is discharged by a superset.

**Impact: none on the verdict.** The residual cost is documentary: the specification now contains a
figure that the evidence contradicts, and a future reader consulting only `spec.md` would be misled.

**Recommendation:** the maintainer should amend the specification's baseline figure from six to eight
at merge, citing `evidence/other/p2-t3-variable-argument-closure.2026-08-29T04-55.md`. This is a
maintainer action, not an executor or reviewer action, because the specification is approved.

---

### NB-5 — COM-visibility limb does not measure per-type overrides

**Location:** `evidence/other/p2-t4-binding-serialization-surface.2026-08-29T04-55.md`, third limb.

The artifact states that `[assembly: ComVisible(false)]` "suppresses COM registration for every type
in the assembly". That is true only in the absence of a per-type `[ComVisible(true)]` override — and
the very file being cited says so three lines above the measured line: "if you need to access a type
in this assembly from COM, set the ComVisible attribute to true on that type."

I measured the override directly:

```
git grep -n -I -F -e "ComVisible(true)" -- "QuickFiler/*"       EXIT: 1
git grep -n -I -F -e "ClassInterface" -e "ProgId" -- "QuickFiler/*"  EXIT: 1
```

**Impact: none.** No per-type override, `ClassInterface`, or `ProgId` attribute exists in the
production tree, so the conclusion is correct as stated.

**Recommendation:** state the limb as a conjunction of two measurements — assembly-level
`ComVisible(false)` **and** zero per-type `ComVisible(true)` — rather than as an inference from the
assembly-level attribute alone. The current wording asserts slightly more than the single measurement
it cites supports.

---

### NB-6 — Inconsistent line-number semantics in the eight-site table

**Location:** `evidence/other/p2-t3-variable-argument-closure.2026-08-29T04-55.md`, site table.

Sites 1 through 7 cite the line number printed by command 1. Site 8 cites line 382, which is the call
line; the printed line for that site is 381, which carries the receiver. The divergence is reconciled
in the adjacent prose ("receiver `typeof(QfcCollectionController)` on line 381") and the two-line
excerpt is reproduced, so nothing is hidden.

**Impact: none.** Both line numbers are correct for what they denote. I verified `Tests.cs:381` is the
receiver and `:382` is the `.GetField(name, ...)` call.

**Recommendation:** add a column header note stating which line the column denotes when a site spans
two lines, or cite both consistently as `381/382`.

---

### NB-7 — The AC-16 398-file figure is never reconciled with the 153-file comparable scope

**Location:** `evidence/baseline/p0-t5-scope-census.2026-08-29T04-55.md`, `AC16_SIX_EXTENSION_SCOPE`.

`issue.md` records that the AC-16 search (a) covered "398 build-input files". `[P0-T5]` measures the
"comparable scope of the AC-16 six-extension search" at 153 files and derives `WIDENING_DELTA: 530`
from it. I reproduced the 153 figure exactly.

The two numbers measure different populations: 153 is a tracked-only `git ls-files` count over the six
extensions with the docs and `.claude` trees excluded, whereas AC-16's 398 came from a filesystem
`grep -r` with different exclusions. Neither figure is wrong, but the artifacts never state that they
differ or why, so a reader may take 530 as the widening against AC-16's actual scope when it is the
widening against a re-derived, tracked-only equivalent.

**Impact: none on any acceptance condition.** AC-3 requires "the comparable scope of the AC-16
six-extension search", which is precisely what 153 is, and the artifact labels it as such.

**Recommendation:** add one sentence noting that 153 is a re-derived tracked-only equivalent and is
not AC-16's own 398, which was measured over a different file set.

---

### NB-8 — PR context artifacts absent

**Location:** `artifacts/pr_context.summary.txt`, `artifacts/pr_context.appendix.txt` — both absent.

The review context sources normally include these artifacts. Both are missing from this worktree.
Regenerating them would require writing outside the feature folder, which the review directive
forbids, so this review derived scope directly from `git diff origin/main...HEAD` instead — a more
authoritative source than the summary, and one that avoids the known misclassification defect in which
`pr_context.summary.txt` reports C# changes as docs-only.

**Impact: none.** Scope was established from git directly and every figure in the audit is traceable
to a re-executed command.

---

## What This Item Does Well

Recorded because these are patterns worth repeating, not as praise.

1. **Acceptance conditions are drift-invariant by construction.** The decision to express Partitions B
   and C as total classifications with one empty category, rather than as hit counts, is what allows
   the evidence to survive prose accretion. This review re-ran Partition B at a third commit and the
   total had moved again to 2474 — and both identities still held. A count-based condition would have
   been red.

2. **The non-vacuity control is genuinely discriminating.** `[P1-T2]` differs from `[P1-T1]` in exactly
   one variable, and the two files it surfaces — an extensionless file and a `.bak` file — are
   precisely the file types the narrower AC-16 scope could not reach. This is the strongest element of
   the evidence set.

3. **Ordering-dependent classification is disclosed rather than smoothed over.** Both `[P1-T4]` and
   `[P2-T2]` state the test order and name the specific rows whose class depends on it. I verified the
   `QuickFileController.cs:20` case at character level.

4. **The closure argument's limit is carried at every level.** The unclosed class — runtime-assembled
   member names — appears in the specification's risk table, in the closure artifact, and as a
   dedicated section of the decision record. The two mitigations are labelled mitigations, not
   closure. This is the correct treatment of a negative result with a known boundary.

5. **Divergences are recorded rather than resolved by adjusting the evidence.** The six-versus-eight
   count and the reference drift were both surfaced with their reconciliations. The failure mode here
   would have been selecting a six-element subset to make the figure agree; that did not happen.

## Test Policy Assessment

No test is added or modified. The specification and the fail-before dossier both argue that a
search-based test would encode a point-in-time measurement as a permanent gate over prose files that
legitimately accrete these identifiers.

**This review independently confirms that reasoning is correct, and by measurement rather than
agreement.** Partition B's total moved from 2229 at the specification's base commit, to 2337 at
execution, to 2474 at review head — three values in three days, driven entirely by documentation and
agent-memory writes. Any test asserting a fixed count, or asserting zero over a scope including those
trees, would already have failed twice since the specification was written, while the property it
purports to guard never changed.

The `full-bug` work mode's failing-regression-test requirement is structurally unsatisfiable here, and
the exception dossier supplies the correct substitute: a measured, non-empty, reachable search scope
with a fully classified hit set. The dossier cites five artifacts by path and identifies `[P1-T2]` as
the closest available analogue to a fail-before run — the same gate over the same corpus producing a
non-zero result. That is an accurate characterization.

## Tone and Documentation Quality

Artifacts are factual, measured, and free of hyperbole. Claims are matched to evidence strength
throughout: figures that were asserted are called asserted, figures that were not are called reference
values, and the one unclosed risk class is stated as unclosed rather than mitigated into silence. This
satisfies `.claude/rules/tonality.md`.

## Verdict

**PASS — 0 blocking findings.** The eight non-blocking observations are documentation and
completeness improvements. Three of them (NB-2, NB-3, NB-5) identify claims whose supporting
measurement was narrower than the claim; in all three cases I performed the broader measurement and
the claim held.
