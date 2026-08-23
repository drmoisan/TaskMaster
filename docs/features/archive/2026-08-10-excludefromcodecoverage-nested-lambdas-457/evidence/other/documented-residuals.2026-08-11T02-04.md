# [P4-T1] Documented residuals and follow-up handoffs

Timestamp: 2026-08-11T02-04

The three residuals below are **deliberate scope choices, not oversights**. Each is recorded here,
each has its own follow-up potential entry, and **none of them is to be absorbed into #457 or used to
widen its scope**.

Every one of the three fails in the **under-exclusion** direction: a file measures no better than it
truly is. Over-exclusion — deleting coverage that should count — is not an acceptable failure mode
anywhere in this feature, and no residual permits it.

---

## Residual (a) — lambda bodies inside `[ExcludeFromCodeCoverage]` **async** members remain counted

**Statement.** If an attributed member is `async` or an iterator, its state machine class
`Type.<Member>d__<N>` is the only trace of the member in the report. Because a `d__` class is admitted
into the presence set, lambdas declared inside an attributed async member are retained.

**Why it is a deliberate scope choice.** Admitting `d__` class names is **mandatory**, not optional.
An async member emits no plain `<method>` element, so without source (2) of the presence set a naive
"no `<method>` implies exempt" rule would delete covered lambdas inside **non-exempt** async members
and fail #457's required direction 2. The verified live counter-example is
`BreadcrumbPopupUiOperations.<>c__DisplayClass33_1` / `33_2` (`line-rate="1"`, covered) declared inside
the non-exempt async `CreateAndInstallSurfaceAsync`. Regression case 3 pins that direction and would
fail immediately if the residual were "fixed" by dropping the admission. Distinguishing an attributed
async member from a non-attributed one requires attribute metadata the Cobertura document does not
carry.

**Measured, not assumed.** The `[P0-T12]` probe settled the open sub-question that research §6.2 left
unverified. `Probe Answer: YES` — the collector does emit a `d__` class for an attributed async
member: `QuickFiler.Controllers.QfcItemController.<ToggleExpansionAsync>d__203`, whose member carries
`[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]`. The soundness guard was satisfied: the
attribute has been present since commit `6b821480` (2026-07-03), 35 days before the corpus was
captured (2026-08-07T02:19:25Z). Evidence:
`<FEATURE>/evidence/baseline/async-d-state-machine-probe.2026-08-11T00-38.md`. The residual therefore
stands as written in `spec.md`; it is not narrower than described.

**Follow-up entry:** `docs/features/potential/2026-08-11-exempt-async-member-lambdas-remain-counted.md`

---

## Residual (b) — local functions inside attributed members remain counted

**Statement.** A local function is emitted as `<method name="&lt;Member&gt;g__Local|N_M">` inside the
**declaring type's own** `<class>` element rather than inside a closure type, and does not inherit the
member's attribute. The filter scopes to closure classes only, so these are untouched.

**Why it is a deliberate scope choice.** Extending the filter to strip `g__` methods from a declaring
type's own class is a natural symmetric extension, but it means mutating **non-closure** classes. That
breaks the "no behaviour change for non-closure classes" invariant and broadens the blast radius from
compiler-generated closure types to every production class in the report — beyond #457's stated scope,
which is "a lambda declared inside a member". The option was evaluated during planning and deferred.

**Interaction any fix must preserve.** `g__` methods are deliberately **not** admitted to the presence
set, so a local function cannot mask an otherwise-absent declaring member and keep that member's
lambdas in the denominator. Regression case 5 part B is the discharging test for that rule.

**Follow-up entry:** `docs/features/potential/2026-08-11-local-functions-in-exempt-members-remain-counted.md`

---

## Residual (c) — overload-name collisions cause under-exclusion, never over-exclusion

**Statement.** The presence set is keyed by member *name*, not signature. If one overload of `Foo` is
attributed and another is not, the non-attributed overload keeps `Foo` in the presence set and the
attributed overload's lambdas are retained.

**Why it is a deliberate scope choice.** A closure method is named `<Member>b__N_M`, where `N` and `M`
are compiler-assigned ordinals that are not a stable, documented mapping back to a specific overload's
signature. Joining a closure to a particular overload would rest on a Roslyn implementation detail
rather than a contract. Signature-based keying was deliberately not attempted in #457.

**Related keying dimensions that behave correctly.** Two types in the same file sharing a member name
are separated by the declaring-type component of the presence-set key. A partial type spanning files
is separated by the filename component, which also errs toward under-exclusion.

**Follow-up entry:** `docs/features/potential/2026-08-11-overload-name-collision-under-exclusion.md`

---

## Follow-up entry paths ([P4-T2], [P4-T3], [P4-T4])

| Residual | Task | Entry path | Exists |
|---|---|---|---|
| (a) exempt async member lambdas | `[P4-T2]` | `docs/features/potential/2026-08-11-exempt-async-member-lambdas-remain-counted.md` | yes |
| (b) local functions | `[P4-T3]` | `docs/features/potential/2026-08-11-local-functions-in-exempt-members-remain-counted.md` | yes |
| (c) overload-name collisions | `[P4-T4]` | `docs/features/potential/2026-08-11-overload-name-collision-under-exclusion.md` | yes |

**Intended promotion path for all three:** `potential_to_issue`, to be run by the epic-orchestrator at
epic close. Each entry records this in its own `## Provenance` section.

### Authoring method, recorded per the plan's fallback branch

`mcp__drm-copilot__new_potential_bug_entry` is **not available to the executing agent** — it is not in
this agent's exposed MCP tool set, which comprises only `run_poshqc_format`, `run_poshqc_analyze`,
`run_poshqc_test` and `run_poshqc_analyze_autofix`. Per the explicit fallback in `[P4-T2]`, `[P4-T3]`
and `[P4-T4]` ("If `mcp__drm-copilot__new_potential_bug_entry` is unavailable to the executing agent,
do not block"), the three entries were **authored directly** at
`docs/features/potential/<yyyy-MM-dd>-<slug>.md`, following the shape of the existing entries in that
folder — specifically `docs/features/potential/2026-08-04-invoke-mstest-scalar-count-strictmode.md`,
whose section headings the promotion tooling maps into the GitHub bug-report template.

Each entry preserves the required headings verbatim and in order: `## Summary`, `## Environment`,
`## Steps to Reproduce`, `## Expected Behavior`, `## Actual Behavior`, `## Logs / Screenshots`,
`## Impact / Severity`, `## Suspected Cause / Notes`, `## Proposed Fix / Validation Ideas`,
`## Next Step`, plus the automation note and the front-matter fields (`Date captured`, `Author`,
`Status`). A `## Provenance` section is appended to each, naming issue #457, this feature folder, the
epic, and the intended promotion path.

## Output Summary

Three residuals recorded, each with its rationale as a deliberate scope choice and each with a
follow-up potential entry on disk. All three fail in the under-exclusion direction. None is absorbed
into #457 or used to widen its scope. The `[P0-T12]` probe measured `YES`, confirming residual (a) as
stated rather than narrowing it. The MCP entry tool was unavailable, so the plan's authored-directly
fallback branch was taken and recorded.
