# 2026-08-26-issue-468-residual-reflective-caller-risk (Spec)

- **Issue:** #635
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-29T00-23
- **Status:** Approved
- **Version:** 1.0

> **No production source file is modified by this item.** This is an evidence-producing audit.
> Its entire output is Markdown: this specification, the plan, and evidence artifacts under the
> feature folder. Any change to a `.cs`, `.csproj`, `.resx`, `.config`, `.settings`, `.xaml` or
> `.json` file is out of scope and is a defect in the execution of this item.

## Context

Issue #468 removed thirteen dead members from the QuickFiler collection controller source file:
twelve methods and one private field. The removal commit is `63eebd47`, subject
`fix(468): remove unreachable load paths and the dead _templateTlp field`. The removed identifiers
are:

| # | Identifier | Kind |
|---|---|---|
| 1 | `WireUpKeyboardHandler` | method |
| 2 | `AnyOpenDropDownsAsync` | method |
| 3 | `LoadGroups_02cAsync` | method |
| 4 | `LoadGroups_02bAsync` | method |
| 5 | `LoadGroup_03bAsync` | method |
| 6 | `LoadConversationsAndFoldersAsync` | method |
| 7 | `LoadItemGroup` | method |
| 8 | `LoadSequentialAsync` | method |
| 9 | `LoadGroupSequential` | method |
| 10 | `CacheTlpForMove` | method |
| 11 | `SwapTlp` | method |
| 12 | `CaptureTlpTemplate` | method |
| 13 | `_templateTlp` | private field |

A successful compilation proves that no compile-time caller of any of these members survived. It
cannot prove the absence of a caller that resolves a member by name at runtime — through reflection,
a resource or configuration token, or a late-bound host invocation. Acceptance criterion AC-16 of
issue #468 required a residual-risk search to close that gap. That search was performed and recorded
in the issue #468 feature folder (docs/features/active/qfc-collection-controller-defects-468, under
evidence/other), and it returned no caller.

The AC-16 search is now known to be incomplete in three specific respects:

1. It searched twelve identifiers, omitting the private field `_templateTlp` from its build-input
   file-type search. Field reflection is the only name-based mechanism that demonstrably exists
   anywhere near the affected type, so the omitted identifier is the one for which the search
   mattered most.
2. Its reflection inventory covered only the `GetMethod(` and `InvokeMember(` patterns. It did not
   cover the `GetField(` family, which is the family actually used against the affected type.
3. Its file-type scope covered six build-input extensions. It did not cover PowerShell, YAML,
   Markdown outside the docs tree, XML, XSD, text, backup, solution, or extensionless tracked files.

Additionally, one factual statement recorded by AC-16 is no longer true: it recorded zero
occurrences of any removed identifier anywhere in the QuickFiler test tree, and a documentation
comment naming `WireUpKeyboardHandler` has since been added to that tree.

This item widens the search, measures its scope so that a zero result is demonstrably non-vacuous,
enumerates every reflection entry point in the QuickFiler production and test trees, corrects the
AC-16 record, and writes a decision that either closes the residual risk or names the specific
caller found.

## Nature of the Item (in place of Repro and Evidence)

- **Steps to reproduce:** none exist. There is no observed failure, no error, no incorrect output,
  and no user-visible symptom. Nothing in the product behaves differently before and after this
  item.
- **Expected versus actual behavior:** not applicable. The item does not assert a behavioral
  expectation; it discharges a verification obligation inherited from issue #468.
- **Frequency and determinism:** not applicable for the same reason. The searches this item performs
  are deterministic against a fixed base commit and are reproducible by re-running the recorded
  commands.
- **Consequence for `full-bug` work mode:** the normal fail-before regression test required by this
  work mode is **structurally impossible** here. A test asserting that a search finds no genuine
  name-based caller cannot be made to fail before the work and pass after, because the work changes
  no executable code; such a test would be a tautology at both ends. The plan must therefore carry a
  **fail-before exception dossier** in
  `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/regression-testing/`
  in place of a failing test, recording why a failing run is impossible and supplying the
  alternative proof. The alternative proof is the non-vacuity measurement itself: a measured,
  non-empty search scope with a fully classified hit set.

## Scope and Non-Goals

**In scope**

- The widened identifier sweep over tracked files, in three partitions: tracked non-`.cs` files
  outside the docs tree and the .claude tree; the same sweep including the docs tree and the .claude tree; and tracked `.cs`
  files.
- A supplementary pass over untracked, unignored files.
- A measured scope size for every search whose result is zero.
- A reflection entry-point inventory across the QuickFiler production and test trees, covering the
  full pattern list rather than the two patterns AC-16 used.
- An explicit closure argument for the reflection call sites whose member-name argument is a
  variable rather than a string literal.
- Two corrections to the AC-16 record: the omitted thirteenth identifier, and the superseded
  zero-hits-in-the-test-tree claim.
- A recorded decision that either closes the residual risk or names the specific caller found.
- Evidence artifacts for all of the above, plus the QA gates applicable to a Markdown-only diff.

**Out of scope / non-goals**

- Modifying any production source file. No `.cs` file changes.
- Modifying any test source file. No new, changed, or deleted tests.
- Modifying any build input, resource, configuration, or project file.
- Re-litigating the issue #468 removal. Whether those thirteen members should have been removed is
  settled; this item asks only whether anything still reaches them by name.
- Repairing any caller that is found. If a genuine name-based caller exists, the correct disposition
  is to record it and escalate it as a separate issue. The issue text asks that such a caller be
  **named**, not fixed; and the repository bugfix workflow directs deeper problems uncovered during
  a fix to a new issue rather than to a widened scope. A repair would additionally require its own
  reproducible failing test, which is a design problem in its own right.
- Changing the AC-16 artifact in the issue #468 feature folder. That artifact is a time-stamped
  historical record. Its corrections are recorded in this item's evidence, not by rewriting it.

**Explicitly excluded from the search set**

Ignored paths — build output, intermediate object directories, restored package payloads, test
result directories, generated coverage output, and local agent state. A hit in any of these would be
a consequence of a tracked source file, never an independent cause, so excluding them does not weaken
the claim. The exclusion is a deliberate scoping decision and is recorded as such with this reason.

## Root Cause Analysis

- **Confirmed nature:** this is not a defect with a failing behavior. It is an **open verification
  obligation** inherited from issue #468 — a gap between what compilation proves and what the
  acceptance criterion claimed. The gap is procedural, not behavioral.
- **Why the gap exists:** the AC-16 search was scoped to the mechanisms judged plausible at the time
  (build inputs plus two reflection patterns) and to a twelve-identifier list that did not match the
  fourteen-member removal recorded in the commit subject. Both narrowings were defensible for the
  merge of issue #468 and both are now measurable, so the cost of closing them is low.
- **Signals supporting this reading:** the QuickFiler production tree performs no name-based member
  resolution of any kind; the affected type carries no serialization surface, no data-binding
  surface, and no COM-visible registration; and the widened sweep over tracked non-`.cs` files
  outside the prose trees already returns zero. The expected finding is confirmation, not
  remediation.
- **Affected components:** none in the executable sense. The trees under audit are the QuickFiler
  production tree and the QuickFiler test tree; both are read and searched only. The files this item
  writes are listed under "Files this item creates or modifies" below.

## Approach

### Design summary

Four measurement passes, one inventory, one closure argument, and one decision record — all
producing Markdown evidence artifacts and nothing else.

1. **Identifier derivation.** Confirm at commit level that `63eebd47` removed exactly the thirteen
   identifiers listed in Context, so the search set is derived from the commit rather than from the
   AC-16 list that omitted one of them.
2. **Partition A — tracked non-`.cs` files outside the docs tree and the .claude tree.** This is the partition
   whose result must be zero. Its scope size is measured so the zero is non-vacuous.
3. **Partition B — the same sweep including the docs tree and the .claude tree.** Its result is a large hit
   count. Every hit is assigned a category derived from its path. The acceptance condition is a
   total classification with the "genuine name-based caller" category empty, never a hit count.
4. **Partition C — tracked `.cs` files.** The hit count is small enough to enumerate individually,
   one row per hit, each carrying its category.
5. **Supplementary pass over untracked, unignored files**, with the file list recorded so the result
   is auditable whether or not it is empty.
6. **Reflection entry-point inventory** across both QuickFiler trees, covering the full pattern
   list, with per-pattern counts reported separately for the production tree and the test tree.
7. **Closure argument** for the reflection call sites whose member-name argument is a variable,
   naming each site individually and stating the argument that bounds the values the variable can
   take, together with the stated limit of that argument.
8. **Decision record** closing the residual risk or naming the caller, plus the two AC-16
   corrections, plus the no-production-change proof.

### The satisfiability constraint on acceptance conditions

An acceptance condition of the form "zero hits repository-wide" is **unsatisfiable by construction**
and must not be written. Two independent reasons, both measured:

- One identifier, `LoadSequentialAsync`, names three live and unrelated members in the TaskMaster
  startup assembly. Those members and their tests are legitimate and are not going to be renamed.
- The the docs tree and the .claude tree trees quote every one of the identifiers extensively, in authored prose
  and in machine-generated evidence that cannot be edited to remove the names.

The condition is therefore written as a **total classification with one empty category**: every hit
is assigned to exactly one category by a path-derived or line-derived test, the counts sum to the
total, and the category "genuine name-based caller" is empty. The bare stem `LoadItemGroup` is also
a strict prefix of a live, preserved member name, so any assertion that must reach zero uses the
parenthesised form of that identifier while the breadth sweep uses the bare stem.

### Exit-code handling

A bare search command that finds no match exits `1`, not `0`. Any evidence artifact recording a
zero-hit search must declare the expected exit code accordingly, or a correct zero-hit run will be
normalised to a failure. A PowerShell wrapper that counts matching lines exits `0` regardless of
whether the inner search matched, so an artifact using a counting wrapper must assert the count and
not the exit code. The two styles must not be mixed within one artifact file, because the expected
exit code is declared per file.

### Files this item creates or modifies

- `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/spec.md`
- `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/issue.md`
- `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/plan.2026-08-29T00-23.md`
- `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/research/reflective-caller-closure.md`
- `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/baseline/`
  — the identifier derivation from commit `63eebd47` and the tracked-file census.
- `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/`
  — the three sweep partitions, the untracked-file pass, the reflection inventory, the closure
  argument, the AC-16 corrections, and the decision record.
- `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/regression-testing/`
  — the fail-before exception dossier.
- `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/qa-gates/`
  — the toolchain gates applicable to a Markdown-only diff, and the no-production-change proof.

No other file in the repository is created, modified, or deleted.

### Boundaries and invariants to preserve

- The QuickFiler production tree and the QuickFiler test tree are read-only for the duration of this
  item.
- Evidence artifacts are written only under the four feature-folder evidence directories named
  above. Writing to any other evidence location is a policy violation.
- No artifact contains an absolute host path, an account name, or a machine name.
- Every negative claim records its search scope, its search patterns, and its search result.

## Verified Baseline Measurements

Measured directly with git in this worktree at base commit
`b56400ab663a85b6039139d4548f408821e957ce`. The plan re-runs each measurement and records it as
evidence; the figures below are the expected results and set the budget.

| Measurement | Value |
|---|---|
| Tracked files, repository total | 11,866 |
| Tracked `.cs` files | 1,599 |
| Tracked non-`.cs` files | 10,267 |
| Partition A scope (tracked non-`.cs`, excluding the docs tree and the .claude tree) | 683 files |
| Comparable scope of the AC-16 six-extension build-input search | 153 files |
| Partition A result | zero hits, exit code 1 |
| Partition B result (same sweep including the docs tree and the .claude tree) | 2,229 hits, all prose or generated evidence |
| Partition C result (tracked `.cs` files) | exactly 31 hits |
| Untracked, unignored files present in the worktree | 5, all belonging to this item's own preparation |

Partition C's 31 hits decompose as follows, and none is a caller of a removed member:

- 1 hit — a `///` documentation comment in the QuickFiler test tree naming `WireUpKeyboardHandler`.
- 2 hits — the live, preserved member `LoadItemGroupsAndViewers_02`, matched only because the bare
  stem `LoadItemGroup` is a strict prefix of it.
- 28 hits — `LoadSequentialAsync`, naming three live and unrelated members in the TaskMaster startup
  assembly together with their tests and comments.

Reflection surface, measured over the same base commit:

- **QuickFiler production tree: zero reflection call sites of any kind.** A combined search for
  `GetMethod(`, `GetField(`, `GetProperty(`, `GetMember(`, `InvokeMember(`, `Activator.`,
  `Type.GetType(`, `Assembly.Load`, `CreateDelegate` and `CallByName` across the production tree
  produces no output and exits 1.
- **QuickFiler test tree:** 172 `GetField(` hits, 69 `GetMethod(` hits, 24 `GetProperty(` hits.
  Eight `GetField(` call sites take a `string name` variable against `typeof(QfcCollectionController)`
  — the exact type whose members were removed. AC-16 searched only `GetMethod(` and `InvokeMember(`
  and therefore never saw them. *(Corrected 2026-09-02, issue #692: the original derivation recorded
  six; the actual implementation-time count, on which the merged fix (PR #688) was discharged, is
  eight. No six-element subset of the eight is separately identifiable, so this correction updates the
  count in place rather than distinguishing an original six from two additional sites.)*

Those eight variable-argument sites are closed mechanically by the Partition C result: no string
literal anywhere in the QuickFiler test tree equals one of the thirteen identifiers, so no value the
variable can take resolves a removed member. The stated limit of this argument is that it does not
cover a member name assembled at runtime by concatenation or interpolation; no such construction was
observed at any reviewed site, but its absence in general was not proved. This limit is recorded
rather than argued away.

## Assumptions, Constraints, Dependencies

- **Assumptions:** the base commit is `b56400ab663a85b6039139d4548f408821e957ce`; git is available
  to the executor; the working tree contains no uncommitted production changes at the start of the
  item.
- **Constraints:** the executor's tool allow-list permits git and PowerShell invocations but not
  bare `grep`, `rg`, `find`, `wc`, `sed` or `awk` as a leading command, so every search must be
  expressed as a git command or wrapped in PowerShell.
- **Dependencies:** the research artifact
  `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/research/reflective-caller-closure.md`
  is the primary input and supplies the command forms, the classification rule, the reflection
  inventory, and the prior-art conventions the evidence artifacts must follow. No external service,
  library, or release is involved.

## Data, API, and Configuration Impact

- **User-facing or API changes:** none.
- **Data or migration considerations:** none.
- **Logging or telemetry updates:** none.
- **Compatibility notes:** none. No public surface, CLI flag, configuration schema, or version
  changes.

## Test Strategy

**No new unit tests are added, and no existing test is modified.** The reason is direct: nothing
executable changes. A unit test asserts behavior of code; this item changes no code, so there is no
behavior to assert that is not already asserted by the existing suite. Writing a test that re-runs a
repository search would encode a point-in-time measurement as a permanent gate over prose files that
legitimately accrete these identifiers, and it would fail on the next evidence artifact that quotes
one of them.

What stands in place of tests:

- **The three sweep partitions**, each recorded with its verbatim command, its verbatim output
  including an explicit "(no output)" where there is none, its exit code, and its measured scope
  size.
- **The classification table** for Partition B and the per-hit enumeration for Partition C, with the
  "genuine name-based caller" category shown empty in both.
- **The reflection entry-point inventory**, with per-pattern counts for the production tree and the
  test tree reported separately.
- **The closure argument** for the eight variable-argument call sites, stated explicitly rather than
  asserted, with its limit recorded.
- **The fail-before exception dossier**, which records why a failing regression run is impossible
  and supplies the non-vacuity measurement as the alternative proof.
- **The no-production-change proof**: a diff anchored to the merge base with the base branch,
  showing only Markdown files under the feature folder, together with a porcelain status check
  showing no unintended working-tree modification.

**Toolchain gates.** The gates applicable to this change are determined by the languages present in
the branch diff. That diff will contain Markdown only. C# formatting, analyzer, nullable and test
gates have no input in this branch and are recorded as not applicable with that reason stated,
rather than being reported as passed or skipped without explanation. Coverage is unchanged because
no production or test code is touched; the item cannot reduce coverage for any changed line, because
no executable line changes.

**Manual validation:** none required.

## Acceptance Criteria

- [x] **AC-1** — The thirteen-identifier search set is derived at commit level from `63eebd47` and
      recorded in
      `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/baseline/`,
      confirming that the commit removed exactly the twelve methods and the one private field
      `_templateTlp` listed in the Context section.
- [x] **AC-2** — The thirteen-identifier sweep over tracked non-`.cs` files outside the docs tree and
      the .claude tree is executed and returns zero hits, with its verbatim command, verbatim output, and
      exit code recorded.
- [x] **AC-3** — The scope of the AC-2 sweep is measured and recorded as a file count, together with
      the repository-wide tracked, tracked-`.cs`, and tracked-non-`.cs` totals and the comparable
      scope of the AC-16 six-extension search, so the AC-2 zero is demonstrably non-vacuous and the
      widening is quantified.
- [x] **AC-4** — The same sweep including the docs tree and the .claude tree is executed, its total hit count is
      recorded, and every hit is assigned to exactly one category derived from its path; the
      per-category counts sum to the recorded total.
- [x] **AC-5** — The category "genuine name-based caller" is empty in the AC-4 classification, and
      the mechanical test by which each hit was assigned its category is stated in the artifact.
- [x] **AC-6** — The sweep over tracked `.cs` files is executed and every hit is enumerated
      individually with its file, line, matched identifier, and category; the enumerated row count
      equals the recorded total, and the "genuine name-based caller" category is empty.
- [x] **AC-7** — A supplementary pass over untracked, unignored files is executed and recorded,
      including the enumerated list of files searched, so the result is auditable whether or not the
      list is empty.
- [x] **AC-8** — A reflection entry-point inventory is recorded covering the full pattern list —
      including the `GetField(` and `GetFields(` family that AC-16 omitted — with a per-pattern hit
      count reported separately for the QuickFiler production tree and the QuickFiler test tree, and
      the production-tree count recorded as zero for every name-resolving pattern.
- [x] **AC-9** — Each of the eight variable-argument reflection call sites (corrected 2026-09-02, issue #692 — originally recorded as six) that target the removed
      members' own type is named individually by file and line, and each is closed by an explicit
      stated argument bounding the values its member-name variable can take; the stated limit of
      that argument is recorded rather than omitted.
- [x] **AC-10** — Both corrections to the AC-16 record are stated in this item's evidence: that
      AC-16's build-input search omitted the thirteenth identifier `_templateTlp`, and that its
      claim of zero occurrences anywhere in the QuickFiler test tree no longer holds, with the
      superseding occurrence identified by file, line, and category.
- [x] **AC-11** — Every search in this item whose result is zero records its search scope, its
      search patterns, its search result, and a measured scope size, so that no zero result rests on
      an empty or unstated search set.
- [x] **AC-12** — No production source file is modified. Proven by a diff anchored to the merge base
      with the base branch showing only Markdown files under
      `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/`, together
      with a porcelain working-tree status check; both are recorded in
      `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/qa-gates/`.
- [x] **AC-13** — A decision record is written in
      `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/`
      that either closes the residual reflective-caller risk on the recorded evidence, or names the
      specific caller found together with the separate issue raised to address it.
- [x] **AC-14** — A fail-before exception dossier is recorded in
      `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/regression-testing/`
      in place of the structurally impossible failing regression test, stating why a failing run is
      impossible and supplying the non-vacuity measurement as the alternative proof.
- [x] **AC-15** — The toolchain gates applicable to the branch diff are recorded in
      `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/qa-gates/`,
      with the language composition of the diff stated and each non-applicable gate marked
      not applicable with its reason.

## Risks and Mitigations

| Risk | Mitigation |
|---|---|
| An acceptance condition is written as a repository-wide hit count and becomes unsatisfiable. | Acceptance conditions AC-4 through AC-6 are written as total classifications with one empty category, never as counts. The measured 2,229-hit prose corpus is recorded in this spec as the reason. |
| A zero-hit search is recorded with the wrong expected exit code and is normalised to a failure. | The expected exit code is declared per artifact file; a bare zero-hit search declares exit code 1, and a counting wrapper asserts the count instead. The two styles are not mixed in one file. |
| A zero result is produced by an empty search set rather than by genuine absence. | AC-3 and AC-11 require a measured scope size alongside every zero. |
| Scope creep into a source-file change, for example by adding a test or removing the documentation comment that names a removed identifier. | AC-12 makes a Markdown-only diff a blocking acceptance criterion, and the non-goals section names test-source changes as out of scope. |
| A genuine name-based caller is found and is fixed in place, violating the bugfix workflow. | The disposition is fixed in advance: record and name the caller, escalate it as a separate issue, and close this item on the decision record. |
| The closure argument for variable-argument reflection sites is overstated. | Its limit — a member name assembled at runtime — is recorded explicitly in this spec and is required by AC-9 to appear in the evidence. |
| Evidence is written outside the canonical evidence directories. | All artifacts are written under the four feature-folder evidence directories enumerated in the approach section; the location is enforced by a pre-tool hook. |
| An artifact leaks an absolute host path, account name, or machine name. | No test-runner output is produced by this item, which removes the usual source of such leaks; artifacts are reviewed for host identifiers before commit. |

## Rollout and Follow-up

- **Rollout:** none. No binary, configuration, or behavior ships. The change is documentation and
  evidence only, merged through the normal pull request path.
- **Post-item monitoring:** none required. If a name-resolution failure is later observed in the
  QuickFiler trees, this item's decision record is the starting point for the investigation and
  identifies exactly which classes of caller were and were not proved absent.
- **Follow-up:** if the decision record names a genuine caller, the separate issue raised for it is
  the follow-up. If the risk is closed, follow-up candidate 9 of the issue #468 specification is
  discharged and no further work is outstanding.
- **Links:** issue #635 (https://github.com/drmoisan/TaskMaster/issues/635); origin issue #468, task
  `[P14-T5]`; the issue #468 feature folder at
  docs/features/active/qfc-collection-controller-defects-468.
