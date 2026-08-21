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

G1 through G4 form a cascade over each `--cov` value: the value is decided once, so a value G1 rejects is never additionally reported by G2 or G3. G4 is evaluated independently of the cascade because the ambiguous form is a defect whatever the value resolves to. G6 is evaluated before G5, because cross-line presence falsifies G5's tree-absence claim.

G1 and G4 are context-free and run on every invocation. G2, G3, G5, and G6 require a repository seam; with no context supplied they do not run, and the Blocking list is byte-identical to the pre-change output for the same text.

### Attribution window

A command span is attributed to the current `P#-T#` when it sits on a task line or on a following line that is not itself a task line and is not separated from the task line by a Markdown ATX heading. A span in the document preamble, in a phase preamble, or after an intervening heading belongs to no task and is dropped rather than reported. A span that belongs to no task cannot be reported against one.

### Graceful degradation

A repository seam that raises, or that reports a non-zero exit, causes G2, G3, G5, and G6 to be skipped. No finding is produced and no exception escapes the evaluation entry point. A validation run must never fail because the repository could not be queried.

## Severity Decisions

### G5 — fixed by the corpus measurement and by nothing else

The shipped G5 severity was not chosen by argument. It was fixed by a pre-declared rule applied to a measurement over the committed plan corpus: Blocking if and only if the total G5 finding count is greater than zero **and** the recorded false-positive count is zero; otherwise Warning.

The measurement is recorded in `docs/features/active/2026-08-17-reject-unfalsifiable-acceptance-gates-in-atomic-plans-486/evidence/qa-gates/g5-corpus-measurement.2026-08-20T12-02.md`. It scanned 166 plan files, evaluated 100 candidate literals, and produced a total G5 finding count of 0. A zero false-positive count over zero findings measures nothing, so the first conjunct failed and **G5 ships as a Warning**.

The zero count is a property of the corpus, not a defect in the measurement. Every committed plan is a tracked file, so a fixed-string search for a literal quoted inside a committed plan always finds at least that plan itself, and the tree-absence condition holds for no committed candidate. The measurement artifact records the four checks that established this (non-vacuous enumeration, a working repository seam, a self-hit on every sampled lookup, and predicate-order equivalence with the shipped rule).

The rule remains meaningful for its intended use. The validator runs against a single plan artifact at authoring time, when that plan is typically uncommitted and therefore untracked, so its own text does not satisfy the tracked-tree presence test. The plan-quotation condition is what exonerates a literal the plan instructs the executor to create.

A later feature may revisit the severity, but only against a fresh measurement taken the same way. The severity is a single constant in each runtime (`G5_SEVERITY`), and a parity test asserts the two constants agree.

### G6 — ships as a Warning

The Blocking argument for G6 is real and is preserved here rather than discarded. A literal present only across a line wrap is *provably* unmatched by a line-oriented search: the tracked evidence shows the phrase exists in the file yet matches no single line, so the assertion is known to return zero matches. That is a stronger evidential position than G3, which only reports that resolution is unknown, and it is comparable to G1, which is Blocking.

G6 nonetheless ships as a Warning because of a residual false-positive case the rule cannot distinguish. The window join is computed over the file's committed text at `HEAD`. When the plan's own task is what rewrites that file so the phrase lands on one line, the pre-change committed text legitimately wraps the phrase and the post-change text does not. G6 then reports a search that will match after the task runs. The plan-quotation exoneration catches the common form of this case, but only when the plan quotes the literal contiguously in prose outside the command span; a plan that paraphrases the intended edit is still reported.

Rejecting such a plan would block a correct plan on evidence about a state the plan is about to change. Surfacing the finding without failing the gate gives the author the same information at no such cost. Reclassifying G6 as Blocking requires first eliminating that case, for example by evaluating the window join against the working tree rather than `HEAD`.

### The G6 sliding window is four adjacent non-blank lines

The window size is fixed at four adjacent non-blank lines. Blank lines are removed before windowing, and one window is emitted per start position, so the boundary is exact: two lines further apart than the window size never appear in the same join. The size is recorded here rather than left implicit so that a later feature can revise it against measured wrap-depth data instead of re-deriving it.

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
