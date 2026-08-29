---
name: reflective-caller-closure-635
description: "#635/#468 residual reflective-caller risk: the removal was THIRTEEN members (the field _templateTlp is the 13th, omitted from AC-16 search (a)); AC-16 never searched GetField( — the only mechanism that actually reaches QfcCollectionController by name; QuickFiler is ComVisible(false)"
metadata:
  type: project
---

Research for issue #635 (2026-08-29), closing the issue #468 residual reflective-caller risk.
Artifact: `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/research/reflective-caller-closure.md`.

**1. The #468 removal was thirteen members, not twelve.** The thirteenth is the private field
`_templateTlp`. It was included in AC-16's *corroborating scoped* sweep but NOT in AC-16's search (a)
over build-input file types. Any follow-up sweep must search thirteen identifiers.
**Why:** the only name-based mechanism that actually exists in this repo is FIELD reflection, so the
one omitted identifier is the one the omitted pattern could have reached.
**How to apply:** when auditing a dead-code removal, take the identifier list from the removal commit
subject and the spec's member table, not from the prior search's `-e` flags.

**2. AC-16 search (b) covered only `GetMethod(` and `InvokeMember(`; `GetField(` was never searched.**
Measured 2026-08-29 in `QuickFiler.Test`: `GetField(` = 172 hits / 65 files; `GetMethod(` = 69 hits /
31 files (AC-16 recorded 42 on 2026-08-26 — the tree moves fast). Several sites resolve members on
`typeof(QfcCollectionController)` by *variable* name, including
`QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs:38/51/65/80/95/118`.
**Why:** AC-16 concluded "the QuickFiler production assembly performs no reflective method lookup" —
true, but it left the test assembly's field-reflection surface unexamined, and that surface names
`QfcCollectionController` private fields routinely.
**How to apply:** never accept `GetMethod(`+`InvokeMember(` as an exhaustive reflection sweep. The
variable-argument sites are closed by a *source-text closure* argument (the identifiers appear
nowhere in the calling assembly's source), not by reading each call site.

**3. `QuickFiler` is `[assembly: ComVisible(false)]`** (`QuickFiler/Properties/AssemblyInfo.cs:22`),
and the production tree has ZERO `[Serializable]`, `DataContract`, `JsonProperty`, `XmlElement`,
`DataBindings.Add`, `DisplayMember`, `ValueMember`, `DataPropertyName`, and zero `dynamic`
declarations. That is the affirmative argument that no VBA/IDispatch/serialization/data-binding
late-binding path can reach any QuickFiler member by name — stronger than a grep negative.

**4. Zero-hit repo-wide identifier gates remain unsatisfiable, and the constraint has GROWN.**
2026-08-29 counts: `LoadSequentialAsync` 1331 occurrences / 200 files (three live unrelated members
under `TaskMaster/AppGlobals/` — `ApplicationGlobals.cs:144` (was `:139` at AC-16 time),
`AppToDoObjects.cs:63`, `AppAutoFileObjects.cs:84`). Even the narrow `LoadItemGroup(` is 14/5. The
smallest, `_templateTlp`, is 27/10. `docs/` holds 2216 of the 2259 total matching lines.
**How to apply:** write the AC as a total classification with one empty class (self-file / live
unrelated member / code comment / docs prose / agent-memory prose / generated evidence / GENUINE
CALLER = 0), never as a count. See [[project_preflight_zerohit_identifier_and_red_test_straddle]]
(atomic-executor memory) for the same trap caught at preflight.

**5. Executor command-form traps (allow-list is `git *` / `pwsh *` only).** Bare `git grep` exits 1
on zero matches, so a zero-hit artifact needs `ExpectedExitCode: 1`. But a `pwsh -Command` wrapper
around it exits 0 regardless of `$LASTEXITCODE` unless the command string ends `exit $LASTEXITCODE` —
so a counting pipeline must assert the COUNT, not the exit code. Also: `git grep` searches tracked
working-tree files only, which already excludes `bin/`, `obj/`, `packages/`, `TestResults/` — the
AC-16 post-filter `grep -v "/bin/\|/obj/"` is unnecessary.

Related: [[qfc-collection-controller-defects-468]], [[qfc-collection-defects-468]].
