---
paths:
  - "scripts/dev_tools/plan_gate_*"
  - "scripts/dev_tools/validate_orchestration_artifacts.py"
  - "extensions/drm-copilot/src/lib/validate/plan-gate-*"
  - "extensions/drm-copilot/src/lib/validate/orchestration-artifacts.ts"
  - "docs/features/**/plan.*.md"
  - "docs/features/**/remediation-plan.*.md"
  - ".claude/skills/atomic-plan-contract/SKILL.md"
description: Acceptance-gate rules G1 through G6 applied to the shell commands an atomic plan states as acceptance conditions.
---

# Atomic-Plan Acceptance Gates (G1 through G6)

This rule governs the acceptance-gate rules the plan validator applies to the shell commands an atomic plan states as acceptance conditions. It exists because a plan can state an acceptance condition that cannot fail: a coverage argument that collects no data, or a search for a literal that returns zero matches whatever the executor does. Such a condition reads as a verification step and gates nothing (issue #486).

The rules are enforced by `scripts/dev_tools/plan_gate_discrimination.py` and by the TypeScript parity port at `extensions/drm-copilot/src/lib/validate/plan-gate-discrimination.ts` with its shared-predicate module `plan-gate-rules.ts`, both fed by the command extractor (`scripts/dev_tools/plan_gate_commands.py` and `extensions/drm-copilot/src/lib/validate/plan-gate-commands.ts`). Enforcement is validator logic plus this prose file. No JSON Schema is authored, imported, or read.

## Scope of Invocation — no grandfathering or exemption mechanism

The plan validator only ever runs against the single artifact it is pointed at. No CI job, test, or scheduled task sweeps the committed plan corpus, and none is added by this feature. A pre-existing plan that would produce a finding is therefore never evaluated unless someone deliberately points the validator at it.

That scope is the argument against a grandfathering list, an exemption marker, a per-plan suppression comment, and an allowlist file. Each of those mechanisms exists to protect an existing corpus from a newly added sweep. With no sweep there is nothing to protect, and the mechanism would add a suppression surface whose only reachable use is to silence a finding on the plan currently being authored — which is precisely the case the gate exists to report.

The consequence is that adding a rule to this set is cheap in migration cost and expensive in authoring cost. Weigh a new rule on its false-positive rate at authoring time, not on how many committed plans it would have flagged.

## Rule Table

Every finding string begins with the square-bracketed `P#-T#` identifier of the task the command is attributed to, and renders the offending value or literal between backticks.

| Rule | Condition | Shipped severity |
| --- | --- | --- |
| **G1** | A non-placeholder `--cov` value whose text, truncated at the first `::`, ends with `.py`. A `.py` suffix proves a filesystem path, which `coverage.py` rejects; the check is context-free and needs no repository lookup. | **Blocking** |
| **G2** | A `--cov` value containing a path separator whose text plus `.py` is a tracked file. The tracked sibling names the intended module exactly, so the dotted remedy is known. | **Blocking** |
| **G3** | A `--cov` value containing a path separator that resolves to neither a tracked file plus `.py` nor a tracked directory. Data collection is unknown rather than provably absent. | **Warning** |
| **G4** | A `--cov` value supplied space-separated (`--cov <value>`) rather than with `=`. The ambiguous form can bind the following positional argument. Independent of resolvability, so it is reported for every value. | **Warning** |
| **G5** | A checkable search literal that is absent from the tracked tree **and** not quoted in the plan document outside the command span it was read from. | **Warning** (see below) |
| **G6** | A checkable search literal absent from every single line of a tracked file but present in that file's sliding-window join of adjacent lines. A line-oriented search returns zero matches. | **Warning** |
| **G7** | A write-mode command — one matching a write-mode register entry, that is a tool that rewrites tracked source and exits 0 after rewriting — whose attributed task text carries none of that entry's observation markers. The exit code alone cannot distinguish a clean run from a repairing one, so the acceptance condition holds either way. | **Warning** |
| **G8** | A `git diff` invocation carrying no non-flag ref operand and neither `--cached` nor `--staged`. It compares the worktree against the index, which is ambient state: the comparison passes vacuously once the change is committed. Exonerated when the attributed task text carries a second `git diff` or a `git status` span. | **Warning** |
| **G8b** | A `git diff` invocation carrying a non-flag ref operand together with `--name-only` or `--name-status`, whose attributed task text carries neither a `git add` span nor a `git status --porcelain` span. A name-listing diff enumerates tracked changes only, so a path the plan creates is invisible to it. | **Warning** |
| **G9** | A command carrying a `--cov` token, carrying no token beginning `--cov-report=term` and no token beginning `--cov-fail-under`, whose project `addopts` value also carries no `--cov-report=term`. No coverage table is printed, so a coverage number the acceptance condition demands can never be read. | **Warning** |

G1 through G9 are the complete shipped set. G1 through G6 were added by issue #486 and G7 through G9 by issue #519; the parenthetical in this file's title names the original set only.

G1 through G4 form a cascade over each `--cov` value: the value is decided once, so a value G1 rejects is never additionally reported by G2 or G3. G4 is evaluated independently of the cascade because the ambiguous form is a defect whatever the value resolves to. G6 is evaluated before G5, because cross-line presence falsifies G5's tree-absence claim.

G8b is a distinct rule from G8 and not a sub-case of it: G8 reports a diff with no ref operand, G8b reports one that has a ref operand but cannot observe an untracked path. A single invocation can satisfy only one of the two, because the presence of a ref operand decides between them.

G1 and G4 are context-free and run on every invocation. G2, G3, G5, and G6 require a repository seam; with no context supplied they do not run, and the Blocking list is byte-identical to the pre-change output for the same text. G7, G8, and G8b are likewise context-free and run on every invocation. G9 requires the repository seam, because it reads the project `addopts` value through it, so with no context supplied G9 does not run. All four of the rules added by issue #519 ship in the Warning channel, so none of them can alter the Blocking list at all.

### Attribution window

A command span is attributed to the current `P#-T#` when it sits on a task line or on a following line that is not itself a task line and is not separated from the task line by a Markdown ATX heading. A span in the document preamble, in a phase preamble, or after an intervening heading belongs to no task and is dropped rather than reported. A span that belongs to no task cannot be reported against one.

### Graceful degradation

A repository seam that raises, or that reports a non-zero exit, causes G2, G3, G5, and G6 to be skipped. No finding is produced and no exception escapes the evaluation entry point. A validation run must never fail because the repository could not be queried.

## Severity Decisions

### G5 — fixed by the corpus measurement and by nothing else

The shipped G5 severity was not chosen by argument. It was fixed by a pre-declared rule applied to a measurement over the committed plan corpus: Blocking if and only if the total G5 finding count is greater than zero **and** the recorded false-positive count is zero; otherwise Warning.

The measurement is recorded in `docs/features/completed/2026-08-17-reject-unfalsifiable-acceptance-gates-in-atomic-plans-486/evidence/qa-gates/g5-corpus-measurement.2026-08-20T12-02.md`. That feature has since been completed and its folder moved out of the active tree, so the citation names the completed tree; the path this file previously carried, under `docs/features/active/`, no longer resolves. It scanned 166 plan files, evaluated 100 candidate literals, and produced a total G5 finding count of 0. A zero false-positive count over zero findings measures nothing, so the first conjunct failed and **G5 ships as a Warning**.

The zero count is a property of the corpus, not a defect in the measurement. Every committed plan is a tracked file, so a fixed-string search for a literal quoted inside a committed plan always finds at least that plan itself, and the tree-absence condition holds for no committed candidate. The measurement artifact records the four checks that established this (non-vacuous enumeration, a working repository seam, a self-hit on every sampled lookup, and predicate-order equivalence with the shipped rule).

The rule remains meaningful for its intended use. The validator runs against a single plan artifact at authoring time, when that plan is typically uncommitted and therefore untracked, so its own text does not satisfy the tracked-tree presence test. The plan-quotation condition is what exonerates a literal the plan instructs the executor to create.

A later feature may revisit the severity, but only against a fresh measurement taken the same way. The severity is a single constant in each runtime (`G5_SEVERITY`), and a parity test asserts the two constants agree.

### G6 — ships as a Warning

The Blocking argument for G6 is real and is preserved here rather than discarded. A literal present only across a line wrap is *provably* unmatched by a line-oriented search: the tracked evidence shows the phrase exists in the file yet matches no single line, so the assertion is known to return zero matches. That is a stronger evidential position than G3, which only reports that resolution is unknown, and it is comparable to G1, which is Blocking.

G6 nonetheless ships as a Warning because of a residual false-positive case the rule cannot distinguish. The window join is computed over the file's committed text at `HEAD`. When the plan's own task is what rewrites that file so the phrase lands on one line, the pre-change committed text legitimately wraps the phrase and the post-change text does not. G6 then reports a search that will match after the task runs. The plan-quotation exoneration catches the common form of this case, but only when the plan quotes the literal contiguously in prose outside the command span; a plan that paraphrases the intended edit is still reported.

Rejecting such a plan would block a correct plan on evidence about a state the plan is about to change. Surfacing the finding without failing the gate gives the author the same information at no such cost. Reclassifying G6 as Blocking requires first eliminating that case, for example by evaluating the window join against the working tree rather than `HEAD`.

### The G6 sliding window is four adjacent non-blank lines

The window size is fixed at four adjacent non-blank lines. Blank lines are removed before windowing, and one window is emitted per start position, so the boundary is exact: two lines further apart than the window size never appear in the same join. The size is recorded here rather than left implicit so that a later feature can revise it against measured wrap-depth data instead of re-deriving it.

### The shared measurement behind G7, G8, G8b, and G9

The four severities below were fixed by one corpus measurement recorded in `docs/features/active/2026-08-23-plan-acceptance-gates-miss-unobservable-and-ambient-state-gates-519/evidence/qa-gates/corpus-measurement.2026-08-24T00-00.md`. The pre-declared decision rule was written and committed **before** the driver existed and before any count was taken, so the ordering is verifiable from git history rather than only asserted in prose. Its form matches the G5 precedent: the shipped severity is Blocking if and only if the total finding count for that rule is greater than zero **and** the recorded false-positive count for that rule is zero; otherwise Warning.

The false-positive definition the rule consumes is likewise pre-declared: a finding is a false positive when the acceptance condition it reports is in fact falsifiable — when the plan states an observation sufficient to distinguish a passing run from a failing one, by a mechanism the rule's predicate does not recognise. A finding is a true positive when the acceptance condition it reports genuinely cannot fail, or can only be satisfied vacuously.

The measurement scanned 194 plan files under `docs/features`, covering the `active`, `completed`, and `archive` trees. Unlike the G5 measurement, every rule here found findings, so each false-positive count is a count over an examined population rather than over an empty one. The vacuity declaration therefore applies to no rule of this set.

| Rule | Corpus files | Candidates | Findings | True positives | False positives | Decision | Shipped |
| --- | --- | --- | --- | --- | --- | --- | --- |
| G7 | 194 | 519 | 466 | 444 | 22 | second conjunct fails | **Warning** |
| G8 | 194 | 237 | 82 | 75 | 7 | second conjunct fails | **Warning** |
| G8b | 194 | 47 | 19 | 19 | 0 | pre-declared unconditional clause | **Warning** |
| G9 | 194 | 273 | 8 | 4 | 4 | second conjunct fails | **Warning** |

### G7 — ships as a Warning because the measurement recorded 22 false positives

G7's finding count of 466 satisfies the first conjunct, so the rule was not decided by a vacuous measurement. It fails the second conjunct: 22 of the 466 findings are false positives, in two classes named in full in the measurement artifact.

**Class 1, two findings — read-only argv shape.** The `prettier-write` register entry matches the argv shape `npm run format` and declares no exclusion for a check flag, so a check-mode invocation written as `npm run format -- --check` is matched as if it wrote. That command does not write and exits non-zero on drift, so its acceptance condition is falsifiable by the exit code alone.

**Class 2, twenty findings — the task observes the tree rather than the tool's stdout.** The attributed task text carries a `git status --porcelain` or `git status` span, so the plan distinguishes a clean run from a repairing one by comparing the tree before and after. G7's marker set recognises tool-output observation only, so it does not see this mechanism. Class 2 is the larger of the two and is the reason the rule cannot ship Blocking on this measurement: a plan that observes the tree has stated a real acceptance condition, and rejecting it would block a correct plan.

Both classes are addressable by a later feature — Class 1 by adding a check-flag exclusion to the `prettier-write` entry, Class 2 by admitting a tree-observation span as an alternative to a marker — but neither was changed here, because narrowing a predicate after reading its measurement would invalidate the measurement that decided its severity. A later feature that narrows either class must re-take the measurement the same way.

Counts, and every one of the 22 false positives named by plan path, task identifier, and offending span: `docs/features/active/2026-08-23-plan-acceptance-gates-miss-unobservable-and-ambient-state-gates-519/evidence/qa-gates/corpus-measurement.2026-08-24T00-00.md`.

### G8 — ships as a Warning because the measurement recorded 7 false positives

G8's finding count of 82 satisfies the first conjunct. It fails the second: 7 findings are false positives, in two classes.

**Class 1, six findings — `--no-index`.** `git diff --no-index` compares two named paths on disk. It does not compare the worktree against the index, so G8's stated claim that the comparison passes vacuously once the change is committed is false for this form.

**Class 2, one finding — the unmerged-path filter.** `--diff-filter=U` selects conflicted paths during a merge or rebase. In that state the worktree-against-index comparison is the correct one and does not become vacuous on commit, because the conflict is precisely what is not yet committed.

Counts, and every one of the 7 false positives named by plan path, task identifier, and offending span: `docs/features/active/2026-08-23-plan-acceptance-gates-miss-unobservable-and-ambient-state-gates-519/evidence/qa-gates/corpus-measurement.2026-08-24T00-00.md`.

### G8b — ships as a Warning unconditionally, by the pre-declared rule

G8b is exempt from the two-conjunct rule and **cannot reach the Blocking channel by any measured outcome**. That exemption was declared before the counts were taken, and it holds even though G8b is the only rule of the four whose measured false-positive count is zero.

The reason is that G8b carries the highest false-positive surface of the set, and a false-positive count taken over one corpus does not bound the false-positive surface of a predicate. Promoting a rule on the strength of a clean run over one corpus would convert an accident of that corpus into a gate. The 19 findings were nonetheless examined individually and all 19 were retained as true positives, across three sub-classes: a placeholder ref operand, which resolves to a real ref at run time and still cannot make the diff see an untracked path; an artifact-field label inside the span, which is cosmetic and leaves the underlying acceptance condition as the finding describes it; and a pathspec written without the `--` separator, where the predicate reads the pathspec as a ref operand. That last sub-class is a true positive for G8b and is simultaneously recorded in the measurement as a **G8 false negative**: the same span should also have been reported by G8, and was not.

Counts, the zero false-positive record, and the three sub-classes with their plan paths and offending spans: `docs/features/active/2026-08-23-plan-acceptance-gates-miss-unobservable-and-ambient-state-gates-519/evidence/qa-gates/corpus-measurement.2026-08-24T00-00.md`.

### G9 — ships as a Warning because the measurement recorded 4 false positives

G9's finding count of 8 satisfies the first conjunct. It fails the second: 4 of the 8 findings are false positives, and all four share one cause — the offending span is not a command the plan states as an acceptance condition. Two are prose quotations of a flag or flag pair used to describe something the task declares out of scope; one is a test-data literal inside a sentence asserting a helper's return values; and one is a truncated restatement of a command whose full form on the task line does supply a terminal reporter. In every one of the four, the acceptance condition the task actually states is decided by a pass count, an exit code, or an artifact field, not by a coverage number.

The remaining 4 findings are true positives, and each is the exact defect the rule was written for: the acceptance condition demands a numeric coverage percentage, the command supplies no terminal reporter, the project `addopts` supplies only an LCOV reporter, and the number the acceptance demands is therefore never printed.

Counts, and every one of the 4 false positives named by plan path, task identifier, offending span, and the reason its acceptance condition is falsifiable: `docs/features/active/2026-08-23-plan-acceptance-gates-miss-unobservable-and-ambient-state-gates-519/evidence/qa-gates/corpus-measurement.2026-08-24T00-00.md`.

## The Write-Mode Register

G7 reads a fixed register of six entries. Each entry is an argv predicate plus a set of observation markers matched case-sensitively as substrings of the owning task's attributed text. The register is data, not code, so the same six entries are transcribed into the TypeScript twin without porting behaviour.

### Membership criterion

**A tool belongs in the write-mode register when it rewrites tracked source and still exits 0 after rewriting.** That conjunction is the whole criterion. The exit code of such a tool is the same on a clean run and on a repairing run, so a plan that states the invocation as an acceptance condition and records nothing but the exit code has asserted nothing the tool can fail. The register's markers name the literals the tool's success-case output prints, which is the observation that separates the two runs.

The six entries are `black-write`, `ruff-fix`, `prettier-write`, `poshqc-format`, `run_poshqc_analyze_autofix`, and `poshqc-suite`. The fifth is named for the MCP tool `run_poshqc_analyze_autofix`, whose argv predicate is an argv word ending with that name.

### Executable-position constraint

A word satisfies an entry's argv shape only when its index lies within the leading four-word scan window and the word immediately preceding it does not begin with a hyphen. A tool name supplied as the operand of a search flag is therefore never read as an invocation, which mirrors the extractor's existing rule that a grep-family name appearing as an operand is not the executable. Without this constraint a task that searches a policy file for a register member's name would report a finding against its own search command — including a task that searches this file.

### Exclusions, with the reason for each

Two tools that do write are deliberately **not** register members.

- **`git add` is excluded.** A plan stages in order to make a later diff complete, so its acceptance concerns the diff, not the staging. The staging itself carries no acceptance condition to make unfalsifiable, and G8b already reads a `git add` span as an exonerating companion rather than as an offence.
- **`npm ci` is excluded.** Its only write target is git-ignored, so it rewrites no tracked source and fails the membership criterion's first half. A plan can still state a real observation of it — an installed-package count, or the existence of a resolved binary — but it is not a case G7 needs to report.

### Two writers that are not register members and are not exclusions

Two further tools write files without being register members, and they are recorded here so a later maintainer does not read their absence as an oversight.

- **The Python test runner** writes coverage output under the artifacts tree, for example the LCOV file the project `addopts` value names. It rewrites no tracked source, and its exit code already distinguishes a passing run from a failing one, so it fails the membership criterion on both halves.
- **The PoshQC test tool** likewise writes result and coverage files under the artifacts tree without rewriting tracked source, and likewise reports pass and fail through its own result. The three PoshQC entries that *are* members — format, analyze-autofix, and the suite — are members because they rewrite tracked PowerShell source in place.

A tool that writes only under the artifacts tree is therefore outside the register by construction. The register is about tracked source, not about writing in general.

### Known false-negative class — the single-token tool-name span

The command extractor drops any command span shorter than two shell words. That two-word minimum-argv floor predates these rules, is pinned by an existing test, and was deliberately left unchanged: relaxing it would newly admit a single-word coverage-argument span into the G1 and G4 scan, which would change existing output, and changing existing output is exactly what this addition forbids itself.

The consequence is a false-negative class. **A tool invoked as a bare single-token name is never extracted, so G7 can never report it, however unobservable its acceptance condition is.** This is the form plans commonly use for MCP tools, including the PoshQC formatter, the PoshQC analyzer autofix tool, and the PoshQC composite suite tool — three of the six register entries. Those three entries are reachable only by a span whose first word is the tool name and whose second word is an operand, for example the tool name followed by a path.

This is a stated limitation, not a promise deferred, and it is recorded here for the same reason the placeholder guard's false-negative class is recorded above: it is the cost side of a trade, and a later feature must weigh it rather than rediscover it. The limitation is pinned by a test asserting that a single-token tool-name span produces zero findings from every one of the four rules, so the boundary cannot move silently.

A later feature may revisit the floor, but only together with a plan for the coverage-rule output change that relaxing it would cause. The two cannot be separated: the floor is one value read by one extractor feeding every rule, so any relaxation reaches G1 and G4 as well as G7.

## Deliberately Uncovered Sub-Classes

Issue #519 measured five classes of unfalsifiable acceptance condition on a single plan. G7 and G9 cover two decidable slices of the first, G8 and G8b cover the third. The remainder is recorded here so a reviewer knows what these rules do **not** reach and that human attention is still required for it.

### The general unobservable-success-output class, beyond what G7 and G9 reach

The class is: an acceptance condition that asserts over output the command does not emit on a successful run. G9 covers the slice where a coverage command prints no table at all, and G7 covers the slice where a write-mode tool's exit code is identical on a clean and a repairing run. Neither reaches the general case.

Three measured instances outside their reach illustrate why. A gate requiring a formatter's `reformatted` line to be recorded verbatim is unsatisfiable on a clean run, because that line is printed only when a file was rewritten. A gate demanding a zero-diagnostic count from an analyzer that returns only an ok flag and a one-sentence summary names a value that has no source. A gate demanding separate line and branch percentages from a coverage run that prints one combined `Cover` column reads two numbers that are never printed.

**Deciding these requires knowing each tool's success-case output, which is not derivable from the plan text, the rules, or the tool documentation.** That is what makes the class invisible to review: four careful review cycles on the measured plan missed it entirely, and it surfaced only when the reviewer ran the tools and observed what they actually printed. A rule would need a per-tool output model, which is a different mechanism from a register of argv predicates. The class is therefore addressed by an authoring requirement in `.claude/skills/atomic-plan-contract/SKILL.md` — that a plan author observe a command's success-case output before asserting over that output — and not by a rule.

### The task-ordering class

The class is: an acceptance condition that is unsatisfiable because of where its task sits in the plan rather than because of what the command does. A gate that runs a test path containing deliberately-failing cases added by an earlier task, before the later task that makes them pass, cannot exit 0. A baseline captured *after* a write-mode formatter has already repaired pre-existing drift becomes either a blanket waiver or makes the later gate unsatisfiable.

**No rule covers this class.** Detecting it requires intra-plan dependency reasoning across phases: the validator would have to model which task changes which file, and which later assertion depends on that change. That is a different analysis from the per-command predicates these rules apply. It is recorded here so a reviewer knows the class still needs human attention.

### The executor-choice heuristic — rejected, and closed rather than deferred

The proposal was a rule that flags selection vocabulary in acceptance text — "any", "a suitable", "the known", "choose" — on the reasoning that an executor free to pick the evidence it is judged against cannot fail. The underlying concern is real and was measured: two conditions on the examined plan asked the executor to identify "the known-genuine pair" and to choose a survivor list.

**The rule is rejected.** The vocabulary it would scan for is ordinary plan prose used in roles that carry no selection semantics, and the research recorded a corpus instance in which the word "any" appears inside a *prohibition* rather than a selection — a case the scan would report and the author would be right to ignore. A keyword scan over prose is not statically decidable for the property the rule would claim to detect. This file's own guidance is to weigh a new rule on its authoring-time false-positive rate rather than on how many committed plans it would have flagged, and on that test the proposal fails.

The concern is addressed as authoring guidance in `.claude/skills/atomic-plan-contract/SKILL.md` instead, where a false positive costs an author a moment's judgment rather than a blocked plan.

**This is closed, not deferred.** Reviving it requires new evidence about its false-positive rate on plan prose, not a restatement of the original proposal.

## Checkable-Literal Definition and the Placeholder Guard

G5 and G6 apply only to a *checkable* literal. The specification defines a checkable literal by two conditions: the command carries the fixed-string flag `-F`, or the pattern contains none of the regular-expression metacharacters `. * [ ] ^ $ \ ( ) { } | + ?`. That condition is conservative in POSIX BRE, POSIX ERE, PCRE, and the Rust regex dialect simultaneously, so no dialect-selection logic is required.

The shipped predicate **extends** that definition with a third condition, and the extension is deliberate: a pattern operand containing any placeholder or interpolation marker is never checkable, even when `-F` is supplied. The markers are `<`, `>`, `${`, `$(`, and `%` — the same set the coverage rules use to skip a placeholder `--cov` value.

The guard exists because a command span whose operand is a placeholder was never intended to be executed verbatim. It documents a command *shape*, so it states no real acceptance assertion, and the resolvability of a placeholder operand is not decidable. Without the guard, every plan that documents a command shape using a placeholder operand receives a G5 finding.

### Known false-negative class

The guard is purely textual, so it fires on any pattern containing a placeholder character in any role. A literal that uses `<`, `>`, or `%` as an ordinary character — a TypeScript generic, a comparison operator, a version constraint, a percentage, an HTML or XML tag — is therefore skipped and can never produce a G5 or G6 finding, however unfalsifiable the assertion actually is.

This is a false-negative class, not a defect to be silently tolerated: it is the cost side of the trade recorded below, and a later feature that narrows the guard must re-measure the false-positive side before doing so. Narrowing candidates include restricting the markers to bracket *pairs* enclosing an identifier-like token, or to the interpolation forms `${` and `$(` plus `%NAME%`, rather than treating every bare `<`, `>`, and `%` as a marker.

### Preflight measurement that fixed the trade

The trade was settled by measurement, not by preference. Across the 164-plan corpus examined at preflight, the placeholder guard suppressed exactly three pattern operands and suppressed zero additional findings:

1. An **angle-bracketed placeholder** inside a documented `git grep` command shape. This is the guard's intended target: the command was written to show a shape, not to be run.
2. A **TypeScript generic** of the `warnings?: ReadonlyArray<string>` shape. Its own plan quotes the token contiguously in prose, so the plan-quotation condition would have exonerated it regardless; the guard changed nothing for this operand.
3. A **version constraint** of the `Node >=18` shape. The token is present in the tracked tree, so the tree-absence condition never held and no finding would have been produced; the guard again changed nothing.

Only the first operand was suppressed by the guard in a way that altered the outcome, and it is the case the guard is for. The other two were already exonerated by conditions the guard does not touch. Against that, removing the guard would have produced a finding on every plan that documents a placeholder-bearing command shape. The measured cost of the guard on this corpus is therefore zero suppressed true positives.

## Message Formatting — no `repr()`, no `!r`, no `pythonRepr`

Every gate message renders the offending coverage value or search literal **between backticks**, in both runtimes, with no surrounding quote characters supplied by a formatting helper.

The following are prohibited in gate messages:

- Python `repr()` and the `!r` conversion in an f-string.
- Any `pythonRepr` helper on the TypeScript side.

The reason is byte-identity across the two runtimes. Python's `repr` selects its quote character based on the value's contents, switching to double quotes when the value contains a single quote, while the TypeScript `pythonRepr` helper used elsewhere in this repository always single-quotes. A value carrying an apostrophe would therefore render differently in the two runtimes, and the parity requirement would fail on exactly the class of value a maintainer is most likely to encounter in a path or a prose literal. Backtick delimiting has no content-dependent behaviour and needs no helper.

The prohibition is enforced by tests, not only by prose: a parity test asserts the Python gate module contains neither `!r` nor `repr(`, a companion test asserts no `pythonRepr(` call appears in any of the three TypeScript gate modules, and the parity fixture set includes an apostrophe-bearing `--cov` value and an apostrophe-bearing search literal whose expected strings are asserted identically in both runtimes.

## Authoring Guidance for Plan Authors

- Express coverage targets as importable dotted names (`--cov=scripts.dev_tools.module`), never as filesystem paths, and always with the `=` form.
- Where an acceptance condition is a search, assert a short, single-line, non-interpolated token that the plan quotes verbatim.
- Prefer a named test over a phrase search whenever a test can carry the assertion.

`.claude/skills/atomic-plan-contract/SKILL.md` carries the authoring-side statement of this guidance and cross-references this file.

## Enforcement

- `scripts/dev_tools/plan_gate_commands.py` extracts task-attributed command candidates; `scripts/dev_tools/plan_gate_discrimination.py` evaluates G1 through G6 and returns the two severity channels.
- `scripts/dev_tools/validate_orchestration_artifacts.py` routes the existing `plan` artifact type through the two-channel entry point, prints each Warning to stderr prefixed with `PLAN GATE WARNING: `, and derives its exit code from the error channel alone. No new flag, option, or artifact type is added.
- The TypeScript parity port is dispatched from `extensions/drm-copilot/src/lib/validate/orchestration-artifacts.ts` for the existing `plan` artifact type. The MCP `validate_orchestration_artifacts` input-schema property-key set is unchanged; Warnings surface on an optional `warnings` field that is absent when there are none.
- `.claude/hooks/validate-planner-output.ps1` is not modified by this rule and carries no part of its enforcement.
