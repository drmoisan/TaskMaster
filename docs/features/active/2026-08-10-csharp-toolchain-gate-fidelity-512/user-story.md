# `2026-08-10-csharp-toolchain-gate-fidelity-512` — User Story

- Issue: #512 (also closes #492, #509, #522)
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-08-10T15-30

> **Acceptance-criteria authority.** This feature's work mode is `full-bug`, for which
> `acceptance-criteria-tracking` resolves `spec.md` as the sole acceptance-criteria source. This file
> is supplied at the epic planner's request for audience context only and deliberately carries **no**
> checkbox criteria. Track and check off AC1-AC13 in `spec.md`.

## Story Statement

- As an **agent session or contributor running the mandatory C# toolchain**, I want each documented
  command to execute against the tools this repository actually pins and to compile the code it
  claims to check, so that a green result means my change is clean rather than that the command did
  nothing.
- As an **agent session or contributor**, I want the documented type-check command to be one that a
  clean checkout passes, so that a red result identifies a regression I introduced rather than
  pre-existing debt the command conscripts.
- As the **repository maintainer**, I want the documented commands to match
  `.github/workflows/ci.yml`, so that a local pass predicts a CI pass and any deliberate difference
  is written down where the command is documented.

## Problem / Why

The toolchain is the contract every C# session executes, and three of its four steps currently break
that contract.

Step 1 (`dotnet tool run csharpier .`) is CSharpier v0 syntax against a 1.2.6 pin. It returns exit 1
and formats nothing, so the first stage of a mandatory loop cannot be completed as written.

Steps 2 and 3 return exit 0 in under two seconds having skipped `CoreCompile` on all 18 projects,
because MSBuild's up-to-date check compares timestamps and does not invalidate on a command-line
`/p:` change. An agent that runs them in a warm working tree — the normal state during a toolchain
loop — receives a pass that was produced by compiling nothing. Every "nullable gate passed" line in
prior feature evidence therefore overstates what was verified.

The failure is not only silent. Step 3 also carries `/p:Nullable=enable`, which CI deliberately
omits because this repository opts in to nullable analysis per file via `#nullable enable`. When an
agent does force a compile, the flag conscripts the 1,100-odd files that never opted in and produces
a wall of `CS86xx` errors that belong to no one's change. On 2026-08-08 two separate deliveries
(#507, #508) each required a human to overrule a subagent's false `CS8603` blocker before work could
continue. Both outcomes cost the user directly: one hides real defects, the other invents defects
that do not exist and stops delivery until a human intervenes.

## Personas & Scenarios

- **Persona: Agent session executing an atomic plan**
  - Runs format → analyze → type-check → test as a mandatory loop, restarting from step 1 on any
    change, and must report which commands it ran and that all passed.
  - Cares about: commands that run as written on the pinned toolchain; a failure signal it can trust
    enough to act on; not manufacturing blocking findings.
  - Constraints: cannot relax a policy to make a gate pass; cannot skip a step; must record
    `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:` evidence for each step.
  - Frustration: a step that passes in 1.8 seconds gives it nothing to report truthfully, and a step
    that fails with 195 pre-existing errors gives it nothing it is allowed to fix.

- **Persona: Contributor using the repo-defined VS Code tasks**
  - Follows the repository's own guidance to prefer repo-defined tasks over ad-hoc commands, and so
    runs `lint: TaskMaster.sln (.NET analyzers)` and
    `type-check: TaskMaster.sln (nullable warnings as errors)`.
  - Cares about: the task surface agreeing with the written policy and with CI.
  - Frustration: those two tasks carry the same defect as the prose — `/t:Build` and
    `-EnableNullable` — so the preferred execution path is also the one that cannot fail. The Pester
    suite currently asserts that defect as expected behavior, so it would not be caught by a test
    run either.

- **Scenario: A clean change through the corrected loop**
  - The contributor edits a C# file, runs `dotnet tool run csharpier format .` (which now runs),
    then the two `/t:Rebuild` builds. Each performs a genuine full recompile of about 20 seconds and
    returns exit 0. The evidence artifact records zero occurrences of
    `Skipping target "CoreCompile"`, so the pass is auditable rather than assumed.
  - Outcome: a green local result that predicts CI, because the type-check command is CI's command
    character for character.

- **Scenario: A change that introduces a real nullable defect**
  - The contributor returns `null` from a non-nullable reference return in a file carrying
    `#nullable enable`. The corrected type-check step recompiles, reports `CS8603` as an error, and
    exits non-zero, naming the file the contributor just edited.
  - Outcome: the contributor fixes their own defect. No human override is required, and no
    pre-existing debt appears in the failure.

- **Scenario: A contributor reads the policy a year from now and wonders why the flag is missing**
  - The corrected documentation states in-line, next to the command, that `/p:Nullable=enable` is
    deliberately absent because no project carries a `<Nullable>` element and the flag opts in every
    un-annotated file at once, and that `/t:Build` is deliberately avoided because a warm build
    skips compilation.
  - Outcome: the contributor does not "restore" either one, which is the failure mode this feature
    is most exposed to.

## Outcomes (non-authoritative; tracked as AC1-AC13 in `spec.md`)

- Every documented format, analyzer and type-check command executes against the pinned toolchain and
  returns exit 0 on a clean checkout.
- Every documented MSBuild step performs a genuine compile, proven from a file log rather than from
  an exit code.
- A deliberately introduced nullable violation fails the corrected gate with the expected `CS86xx`
  diagnostic, with a paired positive control and a recorded revert.
- The repo-defined VS Code tasks and the script behind them execute the same corrected commands, and
  the Pester suite asserts the corrected behavior instead of the defect.
- Any deliberate difference between a documented command and CI's command is written down beside the
  command.

## Non-Goals

- Fixing the nullable diagnostics the corrected gate leaves un-enforced. The measured figure (195
  errors in `UtilitiesCS.csproj` under the forced flag, a lower bound) is recorded for a follow-on
  burn-down epic; none of it is repaired here.
- Coverage thresholds and coverage exclusion policy, which belong to sibling feature #494.
- The Codex/Copilot instruction mirrors (`AGENTS.md`, `.agents/`, `.codex/`,
  `.github/instructions/`, `.github/agents/`), which are excluded on authorization and ownership
  grounds and handed to a follow-up issue. Until that lands, a Codex- or Copilot-driven session can
  still read a stale command; this cost is accepted knowingly and recorded in `spec.md` § SD1.
- Changing `.github/workflows/ci.yml`, which is already correct and is the target this feature
  converges onto.
