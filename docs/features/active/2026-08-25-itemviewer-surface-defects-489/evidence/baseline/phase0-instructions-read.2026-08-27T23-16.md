# Phase 0 — Policy Instructions Read

Timestamp: 2026-08-27T23-16
Feature: itemviewer-surface-defects-489
Branch: bug/itemviewer-surface-defects-489
Work Mode: full-bug

Policy Order:
1. `CLAUDE.md` (standing instructions, always loaded)
2. `.claude/rules/general-code-change.md` (cross-language code change policy)
3. `.claude/rules/general-unit-test.md` (cross-language unit test policy)
4. Language- or domain-specific rules based on files in scope; for this feature the language in
   scope is C#, so `.claude/rules/csharp.md` applies.

Files Read:
- `CLAUDE.md` (447 lines, read in full; sha256 0deb5c764d385d4190e23e37b9e91d7fddca7e79f49d1ffeef3017baf4ee316d)
- `.claude/rules/general-code-change.md` (81 lines, read in full; sha256 91a89164532368f02b617ae9ff2b4e5247ba155c4d6f34acefd767b74ae46f53)
- `.claude/rules/general-unit-test.md` (106 lines, read in full; sha256 c0b3f9b1bd2e55c29484611d64655e2f71a1db97e05ba0680e289754713b63bf)
- `.claude/rules/csharp.md` (97 lines, read in full; sha256 05e69e4a114dafb1a337e0428909f522a3f57a82544b07b52af95108f271172c)
- `.claude/rules/quality-tiers.md` (52 lines, read in full; sha256 4a21f084c11fd3614ec1540e7841d353c7e5b6d03fc3065d13c74fd58989a626)
- `.claude/rules/tonality.md` (81 lines, read in full; sha256 48e35a5a941e72537a222cfd93d548218c6c6c8cbbad3173c455f60a27415948)
- `.claude/rules/plan-acceptance-gates.md` (129 lines, read in full; sha256 868f0ac377ee120f90e3e75ebd862e19a010bba2c2a34164d046be6d86aabe85)
- `.claude/skills/atomic-plan-contract/SKILL.md` (205 lines, read in full; sha256 9a84f99f35796dd92dd3b3eb56fdfcb018ab0c2970e7fbf2202179934f55e137)
- `.claude/skills/evidence-and-timestamp-conventions/SKILL.md` (177 lines, read in full; sha256 146f498fb6abeb45c84d159d3bd6b5eddd8dd692d4c894e50a2f8e5546852719)
- `docs/features/active/itemviewer-surface-defects-489/spec.md` (855 lines, read in full)
- `docs/features/active/itemviewer-surface-defects-489/issue.md` (74 lines, read in full)
- `docs/features/active/itemviewer-surface-defects-489/research/2026-08-25T02-15-itemviewer-surface-defects-research.md` (1232 lines, read in full)
- `docs/features/active/qfc-item-controller-defects-484/spec.md` (upstream contract section, read from line 329)
- `docs/features/active/quickfiler-keyboard-action-defects-444/spec.md` (upstream contract section, read from line 704)

Output Summary: `CLAUDE.md` read in full from the worktree root. It establishes the policy
compliance order, the General Code Change Policy, the General Unit Test Policy, the C# Code Change
Policy and the C# Unit Test Policy, the Tone Policy, and the four-stage C# toolchain
(csharpier format -> msbuild analyzers -> msbuild TreatWarningsAsErrors -> vstest.console.exe with
code coverage), to be run in that order with a restart from stage 1 on any failure or file rewrite.

P0-T2 addendum: `.claude/rules/general-code-change.md` and `.claude/rules/general-unit-test.md` were
read in full from this worktree. Both carry a YAML frontmatter block (`paths: - "**"`) that is absent
from the session-loaded copies, which is why their sha256 values differ from other checkouts; the
policy body text is otherwise the same. Noted divergence, recorded and not resolved here: `CLAUDE.md`
states a repository-wide line-coverage floor of 80 percent with 90 percent for new modules, while
`.claude/rules/general-unit-test.md` states 85 percent line and 75 percent branch across all tiers.
Phase 0 only records baselines and asserts no coverage threshold, so the divergence does not gate any
Phase 0 task; the coverage figures captured in P0-T14 are recorded verbatim against both floors.

P0-T3 addendum: `.claude/rules/csharp.md`, `.claude/rules/quality-tiers.md` and
`.claude/rules/tonality.md` were read in full. `csharp.md` fixes the four-stage command set this plan
uses verbatim, states that `/t:Rebuild` is intentional for a warm local worktree because `/t:Build`
can skip `CoreCompile` through MSBuild incrementality and exit 0 without running analyzers, and
states that `/p:Nullable=enable` must not be passed because nullable is per-file opt-in via
`#nullable enable`. It also states a repository-wide line-coverage floor of 80 percent and 90 percent
for any new module, class, or method, which is the same figure `CLAUDE.md` carries and differs from
the 85 percent in `.claude/rules/quality-tiers.md` and `.claude/rules/general-unit-test.md`. The
divergence is recorded, not resolved; no Phase 0 task asserts a coverage threshold.

P0-T4 addendum: `.claude/rules/plan-acceptance-gates.md` was read in full, including the rule table
for G1 through G6 and the authoring guidance. G1 and G2 are Blocking; G3 through G6 ship as Warning.
`.claude/skills/atomic-plan-contract/SKILL.md` was read in full, including the Non-Overridable
Evidence Path Clause and the `full-bug` mode gate, which requires `spec.md` and treats
`user-story.md` as optional and absent by default. `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`
was read in full, including the canonical `<FEATURE>/evidence/<kind>/` scheme, the five canonical
kinds this plan uses, the `yyyy-MM-ddTHH-mm` timestamp format, and the `ExpectedExitCode:` rules
(exact case-sensitive spelling, integer value, per-file scope, first occurrence wins, defaults to 0
when absent). Those rules govern the P0-T9, P0-T13 and P0-T14 artifacts in this phase.

Upstream Contracts Cited:

- `docs/features/active/qfc-item-controller-defects-484/spec.md`
  section "Upstream contract (exhaustive) - required by features 464 and 489", heading at line 329,
  running through line 408 (the post-`Cleanup()` lifecycle invariant) before
  "### Boundaries and invariants to preserve" at line 410. Read together with the coverage carve-out
  set at line 704 onward, which the 489 spec cites as deliberately three items:
  (a) the capture-field assignments and lambda adapter inside `[ExcludeFromCodeCoverage]`
  `InitializeWebViewAsync`, (b) `DetachWebResourceRequestedHandler`, (c) the default
  `MoveFailureNotifier` delegate. 484 states at line 367 that no member is removed, no public member
  is added, and no interface is modified, so `IItemViewer` is untouched by 484.
- `docs/features/active/quickfiler-keyboard-action-defects-444/spec.md`
  section "Upstream contract (exhaustive) - required by features 464 and 489", heading at line 704,
  covering `QuickFiler/Controllers/KbdActions.cs` (line 711) and
  `QuickFiler/Controllers/QfcItemController.Navigation.cs` (line 742). 444 adds exactly one member to
  `Navigation.cs`, `private void SyncExpandedRegistrations(bool expanded)` (line 748), removes none,
  and changes two (`ToggleExpansion`, `ToggleExpansionAsync`). `MenuDropDown` and
  `JumpToSearchTextbox` are both in the UNCHANGED list at line 776.

P0-T5 addendum - a documented divergence found while reading, recorded and not resolved here.
`spec.md:401` and research section 3.1 both state that neither upstream's production code is on this
branch. `issue.md:49-50`, in the same feature folder, states the opposite: "Both upstreams are
already on the integration branch and their post-change shape is authoritative for planning." The
question is measured, not assumed, by P0-T17, whose recorded `Upstream484Landed:` and
`Upstream444Landed:` booleans govern. P0-T18 re-derives every anchor into the six sibling-owned
partials against the actual branch head regardless of that outcome, and this plan's § Fact base
states that every edit is anchored on a quoted member signature, quoted source text, or quoted
project-file entry, never on a printed line number.
