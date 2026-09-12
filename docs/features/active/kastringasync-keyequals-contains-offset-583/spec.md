# kastringasync-keyequals-contains-offset (Spec)

- **Issue:** #583
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-12T10-35
- **Status:** Ready for implementation
- **Version:** 1.0

## Context

- Summary of the bug and its impact: the first branch of KaStringAsync's KeyEquals method
  guards on a substring test (Contains) but computes the argument it passes to the Update
  callback using prefix-only arithmetic (a Substring offset derived from the probe's own
  length, not from where the probe was found). The guard and the offset agree only when the
  probe happens to be a prefix of the stored key. When the probe matches at a later position,
  the offset expression returns the wrong character.
- Observed environment(s): QuickFiler's keyboard-driven collection navigation, on the
  KaStringAsync implementation of the generic keyboard-action list.
- Customer impact and severity: recorded in issue.md as Low (latent). The defect has no
  observable effect in the shipped product today because the only production construction
  paths for a KaStringAsync instance leave its Update callback null; the branch that carries
  the wrong offset is guarded by a null check on Update and therefore never invokes the
  callback in production. This is a latent defect, not a live one.
- First observed date and version(s) impacted: recorded 2026-09-11 as a maintainer decision on
  issue #583; the underlying code has not changed since it was introduced.

## Repro & Evidence

- Steps to reproduce (code-level, not a live user-facing repro given the null-Update finding
  above): construct a KaStringAsync instance with a non-null Update callback, an Activated
  value of true, and a two-character Key whose second character equals a one-character probe
  that is not the key's first character. Call KeyEquals with that probe. See the Regression
  Case section below for the exact literal values.
- Expected vs actual behavior: Update should receive the character at the position where the
  probe actually matched inside the key. It instead receives a character computed purely from
  the probe's own length, which is only correct when the probe is a prefix of the key.
- Logs/screenshots/error snippets: none; this is a pure-logic defect discovered through static
  review of KaStringAsync's KeyEquals method, not through an observed runtime failure. The
  accompanying research record characterizes framework and repository behavior from source
  inspection; it is not a runtime observation.
- Frequency / determinism: deterministic and always reproducible for the input shapes
  described above, given a non-null Update. Not applicable in current production use because
  Update is never non-null there.

## Scope & Non-Goals

- In scope: the offset expression inside the Contains-guarded branch of KaStringAsync's
  KeyEquals method, and its regression coverage in the associated test file.
- Out of scope / non-goals: the second branch of KeyEquals (the single-character
  non-match branch that toggles a control) and the third branch (the multi-character
  non-match branch, whose own Update argument is already the key's first character
  unconditionally and is not affected by this change) are both out of scope. No other method
  of KaStringAsync, and no other implementation of the shared keyboard-action interface, is in
  scope.
- Explicitly excluded systems, integrations, or datasets: the pinned keyboard-matching test in
  the QuickFiler.Test project's KbdActions test file is out of scope for editing. That file
  must not be modified as part of this change; it is named here only to state that it is
  excluded, not to claim it is touched. Likewise, the collection controller that registers the
  two-digit keyboard keys, the keyboard-action interface and its other implementations, the
  generic keyboard-actions collection class, and the keyboard dispatch handler are cited below
  only as historical and structural context for why the defect is currently unreachable in
  production; none of them is modified by this change. The archived feature folder that
  previously recorded a one-time structural-count baseline for a related, already-closed issue
  is likewise cited only as historical context and is not touched.

## Root Cause Analysis

- Confirmed root cause: inside KaStringAsync's KeyEquals method, the branch entered when the
  stored key contains the probe as a substring computes its Update argument as a Substring
  call whose start index is derived only from the probe's own length minus one. That
  expression is correct only when the probe is a prefix of the key, because in that case the
  probe's length minus one coincides with the index of the last matched character. When the
  probe matches at a different position, the two computations diverge and the wrong character
  is passed to Update.
- Signals/evidence supporting it: confirmed by direct reading of the current implementation
  (KaStringAsync.cs, the KeyEquals method, the Contains-guarded branch) and by the maintainer
  decision recorded in issue.md, which independently describes the same divergence and
  resolves it. The accompanying research record's finding R1 quotes the exact current
  expression and its surrounding structure; finding R9 enumerates the reachable two-digit key
  and probe combinations and shows exactly one combination where the current and corrected
  expressions disagree.
- Affected components/modules: `QuickFiler/Controllers/KaStringAsync.cs` (the KeyEquals
  method) and its regression coverage in
  `QuickFiler.Test/Controllers/KaStringAsyncTests.cs`.

## Proposed Fix

### Design summary (what changes where)

Per the binding maintainer decision recorded in issue.md, this change keeps the Contains guard
on the affected branch of KeyEquals unchanged and corrects only the offset expression that
branch uses when it invokes Update. The offset is derived from the match position — the index
at which the probe was actually found inside the key — rather than from the probe's own
length alone.

The recommended corrected expression, quoted from the research record's finding R1 and its
Recommendation section, replaces the current expression with one derived from IndexOf: the
Substring call's start index becomes the position returned by searching the key for the probe,
plus the probe's own length, minus one. This yields the index of the last character of the
matched span for every match position, not only for a prefix match. The research record notes
this may optionally carry an explicit ordinal comparison argument on the IndexOf call; see the
Comparison Semantics Decision section below for that determination.

### Boundaries and invariants to preserve

- The Contains guard on the affected branch is not changed in any way: neither its condition
  nor a replacement with a prefix-only test is permitted. This is a binding maintainer
  constraint, not a design preference.
- The method's boolean return contract is unchanged: this fix affects only the argument passed
  to the Update callback, never the value KeyEquals returns.
- The null-probe and empty-probe guard clauses that run before the affected branch are
  unchanged and continue to run first, in the same order.
- The multi-character non-match branch's own Update argument (the key's first character,
  unconditional on match position) is a separate, already-correct expression and is not
  touched.

### Dependencies or blocked work

None. The fix is self-contained to the one method and its regression test file. No other
in-flight work blocks or is blocked by this change.

### Implementation strategy (what changes, not sequencing)

#### Files/modules to change

See the Write Set section below for the authoritative list of paths this change's diff
creates, modifies, or deletes.

#### Functions/classes/CLI commands impacted

Only KaStringAsync's KeyEquals method is impacted at the production-code level. No public
signature changes. No CLI or command-surface impact.

#### Data flow and validation changes

None. The guard clauses and their exception contracts (null-probe and empty-probe) are
unchanged. Only the internal arithmetic feeding the Update callback's argument changes.

#### Error handling and logging updates

None required. No new failure mode is introduced; the corrected expression cannot produce a
negative Substring start index, because the affected branch is only entered after the guard
has already confirmed the probe is present in the key, and the probe's length is already
confirmed to be at least one character by the preceding guard clause.

#### Rollback/feature-flag considerations (if applicable)

Not applicable. The change is not behind a feature flag; production behavior does not change
today because Update is null on every production instance, so no rollout risk exists. Rollback
is a plain source revert if ever needed.

### Technical specifications (interfaces/contracts)

#### Inputs/outputs and formats

No change to KeyEquals's public signature (a single string parameter, a boolean return, the
same two exception types on null and empty input). The only observable change under this fix
is the character value passed to a non-null Update callback for a non-prefix match.

#### Required configuration keys and defaults

None.

#### Backward-compatibility expectations

Full backward compatibility is expected for every existing caller: the guard, the return
value, and the exception contract are all unchanged. The corrected argument value only differs
from today's value for match positions that are not currently exercised by any production
construction path, per the research record's finding R3.

#### Performance constraints (latency/throughput/memory)

Not applicable. The change replaces one O(n) substring search argument computation with
another of the same order; no measurable performance difference is expected or required.

## Comparison Semantics Decision

The research record's finding R2 establishes that Contains performs an ordinal (culture-
insensitive) search on both the current and the legacy .NET Framework runtime, while the
parameterless IndexOf overload performs a culture-sensitive search using the current culture
on the legacy .NET Framework runtime this project targets. This is the asymmetry that the
built-in specify-comparison analyzer rules exist to flag. The research record further
establishes, as a static characterization derived from documented framework behavior over the
enumerated input alphabet rather than from any runtime observation, that the two search modes
cannot disagree for this method's actual input domain: the registered keys and probes are
restricted to ASCII digit characters, which have no culture-specific collation or
normalization differences under any standard culture.

The research record also establishes that no analyzer override for either specify-comparison
rule exists anywhere in the repository's analyzer configuration, and that the repository's
blanket analyzer-severity default is set to the lowest, non-blocking severity level. Neither
the analyzer rebuild command nor the nullable rebuild command promotes that severity level to
a build failure. An explicit ordinal StringComparison argument on the IndexOf call is
therefore not required by any active gate.

Decision: the implementation adds an explicit ordinal StringComparison argument to the
IndexOf call used in the corrected offset expression. This is not required for either rebuild
command to pass, and it does not change behavior for the method's actual input domain, but it
makes the corrected expression's comparison semantics textually consistent with the ordinal
Contains guard it depends on, and it removes any ambiguity for a future reader or a future
widening of the input domain.

## Retention Gate Finding

The research record's finding R7 searched the full repository for the literal expressions this
change replaces and confirmed that the only prior structural-count check against them was a
one-time, manually run baseline captured as a dated evidence table inside the archived feature
folder for a separate, already-closed issue. That baseline was never wired into a script, a
repository hook, or a continuous-integration workflow, and no such live enforcement point exists
today. This change requires no update to any retention gate, coverage gate, or automated count,
because none currently counts the literal being replaced.

## Assumptions, Constraints, Dependencies

- Assumptions (environment, data, access): the input alphabet reaching KeyEquals in production
  is restricted to ASCII digit characters, per the research record's findings R2 and R9. No
  change to that alphabet is assumed or required by this fix.
- Constraints (budget, performance, compatibility): the binding maintainer decision constrains
  the guard clause to remain a Contains-based substring test; no alternative guard design may
  be substituted.
- External dependencies (services, libraries, releases): none.

## Data / API / Config Impact

- User-facing or API changes: none. KeyEquals's public signature and exception contract are
  unchanged.
- Data or migration considerations: none.
- Logging/telemetry updates (if any): none.
- Compatibility notes (CLI flags, config schemas, versioning): none.

## Test Strategy

- Regression tests to add or update: one new regression test method is added to
  `QuickFiler.Test/Controllers/KaStringAsyncTests.cs` covering the two-digit-width non-prefix
  case described in the Regression Case section below. Following the file's established
  conventions (per the research record's findings R4 and R8), the new test uses the existing
  NewKa factory helper, MSTest attributes, FluentAssertions assertions with a because-style
  explanation string, Arrange/Act/Assert section comments, and an Intent comment block naming
  the defect being pinned and both the pre-fix and post-fix results.
- Unit tests for the fixed behavior and boundaries: the new regression test, plus the existing
  prefix-case test, together bound the corrected behavior at both the previously-passing
  (prefix) case and the previously-failing (non-prefix) case.
- Edge cases and negative scenarios: the existing null-probe and empty-probe tests already
  cover the guard clauses that run before the affected branch and require no change, because
  those guards are unchanged by this fix.
- Error handling and logging verification: not applicable; no new error path is introduced.
- Coverage impact and targets for changed lines/modules: the changed line is already exercised
  by existing tests today; the new regression test adds coverage for a previously-untested
  code path (a non-prefix match) rather than merely re-exercising an already-covered one.
- Toolchain commands to run (format, lint, type-check, test): the CSharpier format command,
  the CSharpier check command, the analyzer rebuild command, the nullable rebuild command, and
  the MSTest run against the QuickFiler.Test assembly, all as defined in this repository's
  CLAUDE.md, executed in that order and restarted from the beginning if any step fails or
  auto-fixes any file.
- Manual validation steps (if required): none; the defect has no observable production effect
  today, so no manual verification beyond the automated toolchain is required.

Additionally, one existing test's explanatory because-string is reworded because it quotes the
literal expression being replaced; its asserted value does not change. See the Regression Case
section below for that test's identity and current text.

## Regression Case

The primary regression case, carried forward from issue.md and confirmed by the research
record's finding R9 as the only reachable two-digit key and single-character probe combination
where the current and corrected expressions disagree: a Key equal to "01" and an other equal
to "1", with a non-null Update, must cause Update to receive "1". Under the current,
unfixed expression, Update instead receives "0".

The pre-existing prefix-case example, which must remain unaffected by this fix: a Key equal to
"abc" and an other equal to "ab", with a non-null Update, causes Update to receive "b" under
both the current and the corrected expression, because "ab" is a prefix of "abc" and the two
formulas agree whenever the probe is a prefix of the key.

The one existing test whose because-string quotes the literal expression being replaced is the
test named for a contains-match while activated that invokes Update and returns true, in
`QuickFiler.Test/Controllers/KaStringAsyncTests.cs`. Its asserted value ("b") does not change;
only its explanatory string is reworded so it no longer quotes the replaced literal expression,
describing instead the last-character-of-the-matched-span reasoning that the corrected
expression embodies.

## Acceptance Criteria

- [ ] AC1: The recorded maintainer decision (retain the Contains guard, correct the offset
  arithmetic) is reflected in the implementation; the branch-one guard text is unchanged and
  no StartsWith call is introduced in `QuickFiler/Controllers/KaStringAsync.cs`.
- [ ] AC2: KaStringAsync's KeyEquals branch-one derives its Update argument from the match
  position via Key.IndexOf(other), so the character passed to Update is the last character of
  the matched span for a non-prefix match as well as for a prefix match.
- [ ] AC3: A regression test in `QuickFiler.Test/Controllers/KaStringAsyncTests.cs` covers the
  two-digit-width non-prefix case described in the Regression Case section above, with a
  non-null Update, and asserts that the received argument equals the corrected value stated
  there.
- [ ] AC4: The pre-existing prefix-case behavior is preserved: for a Key equal to "abc" and an
  other equal to "ab", Update still receives "b".
- [ ] AC5: The pinned keyboard-matching test in the QuickFiler.Test KbdActionsTests file passes
  unchanged, and that file is not modified by this change.
- [ ] AC6: The full C# toolchain passes in order: CSharpier check, the analyzer rebuild, the
  nullable rebuild, and the MSTest run.

## Write Set

- `QuickFiler/Controllers/KaStringAsync.cs`
- `QuickFiler.Test/Controllers/KaStringAsyncTests.cs`
- `docs/features/active/kastringasync-keyequals-contains-offset-583/issue.md`
- `docs/features/active/kastringasync-keyequals-contains-offset-583/spec.md`
- `docs/features/active/kastringasync-keyequals-contains-offset-583/plan.2026-09-12T10-25.md`
- `docs/features/active/kastringasync-keyequals-contains-offset-583/research/2026-09-12T10-35-kastringasync-keyequals-contains-offset-research.md`
- `docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/baseline/`
- `docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/regression-testing/`
- `docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/qa-gates/`
- `docs/features/active/kastringasync-keyequals-contains-offset-583/evidence/other/`

## Risks & Mitigations

- Technical or operational risks: minimal. The change is confined to one expression inside one
  method, guarded by the same conditions as before, in a code path that no production
  construction site currently exercises with a non-null Update. The primary risk is
  introducing a regression in the already-passing prefix-case test; this is mitigated by
  keeping that test's asserted value unchanged and only rewording its explanatory string.
- Mitigations and rollbacks: the full toolchain run (format, analyzer rebuild, nullable
  rebuild, test run) after the change is the primary verification gate. Rollback is a plain
  source revert of the two changed source files if a regression is discovered.

## Rollout & Follow-up

- Release/rollout steps: standard pull-request merge; no flag, migration, or staged rollout is
  required, because the change has no observable effect on current production behavior.
- Post-fix monitoring or clean-up tasks: none identified. If a future change wires a non-null
  Update callback into a production KaStringAsync construction site, that change should
  re-verify this corrected offset behavior against the newly reachable code path.
- Links: issue #583; the research record at
  `docs/features/active/kastringasync-keyequals-contains-offset-583/research/2026-09-12T10-35-kastringasync-keyequals-contains-offset-research.md`.
