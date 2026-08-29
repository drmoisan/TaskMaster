# Decision Record (P3-T3) — discharges AC-13

- **Issue:** #635
- **Plan task:** [P3-T3]

Timestamp: 2026-08-29T06-37

## Decision

DECISION: RESIDUAL RISK CLOSED

The residual reflective-caller risk inherited from issue #468 acceptance criterion AC-16 is closed on
the evidence recorded by this item. No genuine name-based caller of any of the thirteen removed members
exists in the tracked repository or among its untracked, unignored files. The alternative branch of
this decision, which would have named a specific caller and the separate issue raised to address it,
does not apply: no caller was found.

## Output Summary

Every measurement this item performed is consistent with closure and none is in tension with it. The
widened identifier sweep returns zero over a measured 683-file scope that the earlier AC-16 search
could not reach; the same sweep including the prose trees classifies all 2,337 hits with the
genuine-caller category empty; the sweep over tracked `.cs` files enumerates all 31 hits individually
with the same category empty; the untracked pass finds no hit outside this item's own artifacts; the
QuickFiler production tree contains no name-resolving reflection call site of any kind across sixteen
patterns; and the assembly carries no binding surface, no serialization surface, and no COM
registration. One class of caller is not proved absent, and it is named below.

## Artifacts the closure rests on

Phase 1 — the identifier sweep:

- `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p1-t1-partition-a-sweep.2026-08-29T04-55.md`
- `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p1-t2-partition-a-control.2026-08-29T04-55.md`
- `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p1-t3-partition-b-classification.2026-08-29T04-55.md`
- `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p1-t4-partition-c-enumeration.2026-08-29T04-55.md`
- `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p1-t5-untracked-pass.2026-08-29T04-55.md`

Phase 2 — the reflection surface:

- `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p2-t1-reflection-inventory.2026-08-29T04-55.md`
- `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p2-t2-production-reflection-classification.2026-08-29T04-55.md`
- `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p2-t3-variable-argument-closure.2026-08-29T04-55.md`
- `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p2-t4-binding-serialization-surface.2026-08-29T04-55.md`

Supporting baseline measurements:

- `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/baseline/p0-t4-identifier-derivation.2026-08-29T04-55.md`
- `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/baseline/p0-t5-scope-census.2026-08-29T04-55.md`

## Classes of caller proved absent

| Class of caller | Proved absent | Evidence |
|---|---|---|
| A build-input, resource, configuration, script, manifest, notes or backup file naming a removed identifier | yes | [P1-T1], zero hits over a measured 683-file scope; [P1-T2], the same pathspec reaching real content |
| A tracked `.cs` file naming a removed identifier in any role other than a live unrelated member, a stem collision, or a comment | yes | [P1-T4], all 31 hits enumerated individually, genuine-caller category empty |
| An untracked, unignored file naming a removed identifier | yes | [P1-T5], zero hit files outside this item's own feature folder |
| A reflective member lookup in the QuickFiler production assembly | yes | [P2-T1], sixteen name-resolving patterns each `prod=0` over a 228-file scope; [P2-T2], all 39 `System.Reflection` occurrences classified with the member-name-argument class empty |
| A reflective member lookup in the QuickFiler test tree passing a string literal or a named constant equal to a removed identifier | yes | [P2-T3], every literal and the one named constant enumerated with its declared value; none is one of the thirteen |
| A reflective member lookup in the QuickFiler test tree passing a variable that could take the value of a removed identifier | yes, subject to the limit below | [P2-T3] closure argument, bounded by [P1-T4]'s measured absence of any such literal in the calling source text |
| A WinForms property-name data binding naming a removed member | yes | [P2-T4], four binding patterns each `prod=0` |
| A serializer directed at a removed member by name | yes | [P2-T4], four serialization attribute patterns each `prod=0` |
| A host-side late-binding call through COM — a VBA `CallByName`, an `Application.Run`, or an Outlook macro | yes | [P2-T4], `[assembly: ComVisible(false)]` at QuickFiler/Properties/AssemblyInfo.cs line 22 |

## The class not proved absent

**A member name assembled at run time by string concatenation or interpolation.** This is the single
class the evidence does not close.

The closure argument for the variable-argument reflection call sites bounds the values a member-name
variable can take by the string literals present in the source text of the calling assemblies. A name
built at run time from fragments would not appear as a literal anywhere in that source text and would
therefore escape the bound. [P2-T3] records that no such construction was observed at any of the eight
variable-argument sites — every member-name argument enumerated there is a literal, a `const string`
identifier, or a `string` parameter of a private static helper — but its absence in general was not
proved, and this record does not claim otherwise.

Two facts bound the practical consequence without closing the class. First, the sites in question are
in test code, not in the shipped production assembly, so a failure would surface as a red test rather
than as a runtime fault in the product. Second, the test-support helpers assert that the resolved
`FieldInfo` or `MethodInfo` is not null, so an unresolvable name fails loudly rather than silently.
Neither fact is a proof, and both are recorded as mitigations rather than as closure.

## Disposition

No caller is named, because none was found, so no separate issue is raised by this item. Had a genuine
caller been found, the disposition fixed in advance by the specification and by the repository bugfix
workflow would have applied: record and name the caller, escalate it as a separate issue, and close
this item on this decision record without repairing the caller in place. A repair would additionally
have required its own reproducible failing test, which is a design problem in its own right.

A hit under the docs tree or the .claude tree is a category D or E hit, never a caller, and does not
trigger the caller-found branch of this decision. Both prose trees quote the identifiers extensively —
[P1-T3] measures 2,319 hits under the docs tree and 18 under the .claude tree — and none of those hits
is compiled, resolvable, or reachable by any reflection API.

## Follow-up

Follow-up candidate 9 of the issue #468 specification is discharged. No further work is outstanding
under this item. If a name-resolution failure is later observed in the QuickFiler trees, this record is
the starting point for the investigation and identifies exactly which classes of caller were and were
not proved absent.
