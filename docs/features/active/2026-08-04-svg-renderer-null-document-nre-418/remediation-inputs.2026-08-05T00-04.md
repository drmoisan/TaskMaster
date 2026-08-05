# Remediation Inputs — svg-renderer-null-document-nre (Issue #418)

- Artifact timestamp: `2026-08-05T00-04`
- Review cycle: reaudit 3 (remediation cycle 2 verification)
- Base: `origin/main` @ `ce0c91e686bf7e060aaab6f185ee6883269e4fd4`
- Head: `bug/svg-renderer-null-document-nre-418` @ `69e675d014d001b2e17ee15c3279ce6a5ba46609`

## Source Artifacts

| Artifact | Path |
|---|---|
| Policy audit | `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/policy-audit.2026-08-05T00-04.md` |
| Code review | `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/code-review.2026-08-05T00-04.md` |
| Feature audit | `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/feature-audit.2026-08-05T00-04.md` |

## Headline

**Blocking count: 1. Changed from 2.** Cycle-2 blocker G-8 is CLOSED and verified by reviewer-executed
measurement. The remaining blocker, G-2 (AC-11), is **not agent-remediable**.

**No remediation plan is authored for this cycle, and none should be.** A remediation plan would contain
zero executable tasks. Both open items require the maintainer, not an agent. The rationale is recorded
in full below so the decision is auditable rather than implicit.

## Cycle-2 Remediation Outcome

| Cycle-2 item | Class | Outcome |
|---|---|---|
| R-1 — Execute the AC-11 human designer-load runbook | BLOCKING | **Not executed.** Correctly so: no agent can execute it. Carried forward as G-2. |
| R-7 — Add the missing `ExCSS` reference to `SVGControl.Test` | BLOCKING | **DISCHARGED.** Verified by the reviewer's own standalone test run at 75/75/0. |
| R-7's `Fizzler` sub-directive | BLOCKING (as written) | **CORRECTLY REFUSED.** The directive's justification was false on disk. See the correction below. |
| R-8 .. R-12 | non-blocking | Not addressed this cycle; the plan scoped to the Blocking item only. Carried forward as Low code-review findings. |

### Correction to a reviewer-authored artifact

`remediation-inputs.2026-08-04T22-28.md` directed: "Add `Fizzler 1.3.1` on the same pattern for parity
with the eight sibling test projects." **That justification is false**, and the reviewer has now verified
the refutation independently:

| Reviewer claim | Measured truth | Verification command |
|---|---|---|
| Eight sibling test projects reference `Fizzler` | **Zero** do. Only `SVGControl/SVGControl.csproj:58` and `UtilitiesCS/UtilitiesCS.csproj:63`, both production. | `grep -rn "Fizzler" --include=*.csproj .` |
| Adding it produces parity | It would produce **divergence** — no test project's output carries `Fizzler.dll`. | `ls SVGControl.Test/bin/Debug/Fizzler.dll` → no such file |
| The redirect is sound | `SVGControl.Test/app.config:27` redirects `Fizzler` to `1.3.0.0`; the on-disk package is `Fizzler.1.3.1` and both production references declare `Version=1.3.1.0`. | `ls -d packages/Fizzler*` |

Had the executor complied, it would have deployed a `1.3.1.0` assembly into a project whose config
redirects `Fizzler` to an absent `1.3.0.0` — activating a stale redirect that is inert today only
because the file is missing. That is the same defect class as issue #418 itself. **The executor was
correct to refuse, and correct to document why.** The underlying stale redirect is properly filed at
`docs/features/potential/2026-08-04-stale-fizzler-and-unsafe-binding-redirects.md`.

Process note for future cycles: remediation inputs must verify on-disk parity claims before directing a
build-configuration change. An executor that complies with a provably wrong directive propagates a
reviewer error into the codebase.

## Remediation-Required Findings

### RM-1 — AC-11 designer-load verification (BLOCKING, human-only, NOT agent-remediable)

- **Source finding:** policy audit G-2; feature audit AC-11 FAIL.
- **Condition:** `issue.md:112` remains `- [ ]`. No designer-load evidence capture exists.
- **Required action:** a human opens `UtilitiesCS/Dialogs/MyBoxViewer.cs` in the Visual Studio WinForms
  designer at this head and confirms the form loads without a `NullReferenceException`, then attaches
  the capture to the feature folder under `evidence/regression-testing/`.
- **Runbook:** `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/runbooks/verify-winforms-designer-load.runbook.md` (283 lines, complete).
- **Tracking, verified by the reviewer against `artifacts/orchestration/orchestrator-state.json`:**
  human-interaction requirements H-1 (`satisfies: AC-11`) and H-2 (`satisfies: AC-7`), both with
  `response: "exception"` and a `runbook_path` that resolves. This satisfies the
  `.claude/rules/orchestrator-state.md` invariant that an `exception` response carry a non-empty
  `runbook_path`.
- **Assignment: MAINTAINER.** Do not route to `atomic-planner`. The legacy in-process WinForms designer
  has no unattended automation surface. Three review cycles have now carried this item unchanged; a
  fourth would carry it unchanged again.
- **Alternative disposition:** an explicit maintainer waiver of AC-11, recorded in `issue.md`, would
  also close it.

### RM-2 — G-9 coverage adjudication on `SvgAssemblyResolver.cs` (non-blocking, maintainer decision)

- **Source finding:** policy audit G-9.
- **Condition:** `SVGControl/SvgAssemblyResolver.cs` measures 106/172 = **61.6279%** line and 28/52 =
  **53.8462%** branch, against the >= 85% line / >= 75% branch uniform floors and the >= 90% new-module
  line threshold. Byte-identical to cycle 2.
- **Entire shortfall is one member:** `ResolveByNameAndKey` at 47/80 = 58.75%. It is `private static`,
  subscribed to `AppDomain.CurrentDomain.AssemblyResolve`, and invoked only by the CLR on a failed
  assembly bind. It carries the ratified exception
  `COVERAGE_MEMBER_UNREACHABLE: SVGControl.SvgAssemblyResolver.ResolveByNameAndKey`.
- **Two facts bear on the decision:**
  1. The member was **relocated verbatim** by R-6, not authored, so the >= 90% new-module threshold
     arguably does not attach. `Install()`, the only genuinely new member, measures 6/6 = 100%.
  2. The file exists only because the resolver was extracted first, to relieve `SvgRenderer.cs` at 497
     of its 500-line limit before a `catch` block was added. Absent that sequencing, these 172 lines
     would have counted against an existing file and no new-file threshold would have applied. The
     shortfall is an artifact of where line-count pressure forced the boundary, not a reduction in
     tested behavior.
- **Decision required:** whether the ratified `COVERAGE_MEMBER_UNREACHABLE` exception, or the COM/VSTO
  host-bound exemption class in `CLAUDE.md` UT2, extends to a CLR-invoked `AssemblyResolve` handler.
  `.claude/rules/general-unit-test.md` prohibits excluding production files from coverage measurement
  and directs refactoring instead; the counter-argument is that the residual uncovered lines are
  `Assembly.Load`/`LoadFrom` failure paths that cannot be driven without a genuine failed bind in a real
  AppDomain.
- **Assignment: MAINTAINER.** Do not route to `atomic-planner`. Further agent-side remediation would not
  move the figure without either a new host-level seam or a ratified exemption. The reviewer takes no
  position on the merits.

## Non-Blocking Findings Carried Forward (no action required before merge)

These are recorded for a maintainer's optional follow-up. None affects correctness of the delivered fix,
and none should trigger a remediation cycle on its own.

| ID | File | Summary |
|---|---|---|
| G-1 | `SVGControl/SvgRenderer.cs` | Modified-file line coverage 80.1932% against the 85% floor. Entire shortfall in six untouched pre-existing members. Owned by `docs/features/potential/2026-08-05-svgcontrol-coverage-uplift.md`. |
| G-3 | repository-level | The mandated nullable gate returns exit 0 vacuously with 0 `CoreCompile` targets. Limits what any C# review in this repository can assert about type safety. Recommend a per-changed-project gate. |
| G-4 | `SVGControl.Test/` | Test files sit beside the project rather than in a mirrored `tests/` tree. Pre-existing repository-wide convention. |
| G-7 | `artifacts/pr_context.summary.txt` | Collector defects: C# files misclassified as docs (`Core logic changes: 0 files`), and spurious close candidates including `#AC-1`..`#AC-11` and `#DE06-4337`, a fragment of the `SVGControl.Test` project GUID. |
| CR-Low-1 | `SVGControl/SvgAssemblyResolver.cs` | Resolver still reaches back into `SvgRenderer` for `DescribeFailure` and `typeof`; the R-6 separation is incomplete. |
| CR-Low-2 | `SVGControl/SvgAssemblyResolver.cs` | Diagnostic prefixes still read `"SvgRenderer load ..."`, naming a type the code no longer lives in. |
| CR-Low-3 | `SVGControl/SvgRenderer.cs` | The two byte-array constructors carry near-identical 17-line bodies. |
| CR-Low-4 | `SVGControl/SvgAssemblyResolver.cs` | The pre-guard region (lines 50-54) sits outside the containment `try`. Disclosed and accepted in Design Decision 11. |
| CR-Info | `SVGControl/app.config`, `SVGControl.Test/app.config` | Stale `Fizzler` redirect to an absent `1.3.0.0`. Correctly deferred to `docs/features/potential/2026-08-04-stale-fizzler-and-unsafe-binding-redirects.md`; recommend promoting it. |

## Why No Remediation Plan Is Authored

The SKILL contract directs creating a remediation plan when remediation is triggered. Remediation is
nominally triggered here, because the policy audit records PARTIAL and one acceptance criterion is FAIL.
A plan is nonetheless **not** authored, for a reason that is specific and checkable rather than
discretionary:

**Every remaining finding is assigned to the maintainer, and none has an agent-executable task.**

- RM-1 requires a human GUI session. No tooling in this repository can perform it.
- RM-2 requires a policy adjudication. An agent can neither ratify an exemption nor invent a seam into
  a CLR callback without redesigning the host-binding mechanism, which is far outside a `minor-audit`
  work mode.
- The non-blocking items above are optional polish and repository-level concerns, none attributable to
  this branch's correctness.

Authoring a plan whose task list is empty, or whose tasks restate "wait for a human," would produce a
false remediation cycle: an executor would be dispatched, would find nothing to execute, and the next
review would arrive at this same page. Three cycles have now run on this feature; cycles 1 and 2 each
closed real findings, and this cycle has none left to close.

**Recommendation: stop the remediation loop and route to the maintainer.**

## Go / No-Go

**Conditional GO.** The code is ready to merge. The single blocking item is a human verification step,
not a code defect.

Merge is recommended once **either**:

1. the AC-11 runbook is executed and its evidence attached, restoring AC-11 to PASS and taking the
   blocking count to 0; **or**
2. the maintainer explicitly waives AC-11 and records the waiver in `issue.md`.

The G-9 adjudication (RM-2) is non-blocking and may be settled before or after merge.
