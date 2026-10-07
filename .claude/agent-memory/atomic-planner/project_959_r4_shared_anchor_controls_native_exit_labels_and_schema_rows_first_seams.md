---
name: project-959-r4-shared-anchor-controls-native-exit-labels-and-schema-rows-first-seams
description: Preflight round 4 seams on #959 (plan revision 1.4) — a negative control that re-types the regex instead of sharing the predicate's variable is vacuous and needs a positive twin; "EXIT_CODE scoped to the dotnet tool restore invocation" has no printed source until every native call gets a $LASTEXITCODE label; schema rows must be written first because parsers take the first occurrence; a loop-rule "edit only spec.md" clause contradicts a check-off task's artifact-repair branch
metadata:
  type: project
---

Round 4 on #959 (2026-10-03) returned six defects against revision 1.3; all were plan-prose seams, no tree facts changed.

- **A control must exercise the predicate actually run.** R3 added `FIELD-CHECK-CONTROL:` by re-typing the anchor regex as a second literal; the reviewer called it vacuous because a later edit to the predicate would leave the control green. Fix: assign `$anchor = "(?m)^[^\w\r\n]*"` once, build predicate and both controls as `$anchor + [regex]::Escape(...)`, and add a positive twin (`- **EXIT_CODE:** 0` must print True) mapped to the same `FIELD CHECK NOT DISCRIMINATING` stop. State the clause-count effect in every gate task that gains the clause.
- **"EXIT_CODE scoped to <native call>" needs a printed source.** A payload that runs several natives (`& pwsh -File install.ps1`, `dotnet --version`, `dotnet tool restore`, `dotnet tool list`) must bracket each with `$global:LASTEXITCODE = 0` and `Write-Output ("<LABEL>: " + $LASTEXITCODE)`; the artifact's `EXIT_CODE:` then equals one named label. `& pwsh -NoProfile -File x.ps1` is a child process, so its label is the process exit code (1 on an uncaught `throw`). A command that is not found raises a non-terminating error and leaves `$LASTEXITCODE` at 0, so an exit label on `tool --version` is only half a gate; the recorded version line is the discriminating half.
- **Schema rows first.** The evidence parsers take the first occurrence of `Timestamp:`/`Command:`/`EXIT_CODE:`/`ExpectedExitCode:`; the convention must say the four rows precede `Output Summary:`, every copied payload line and every appended section. Sweep every `append` instruction to confirm none orders schema rows after copied output.
- **Loop-rule write scope vs repair branches.** A Phase-6 sentence "check-off tasks edit only FEATURE/spec.md" is false once a check-off task (P6-T41) repairs a missing field in an evidence artifact before its Edit; name the exception in the rule itself and re-read the Edit heading and the AC mapping for agreement.
- **Branch (b) expectation flows to every artifact that copies the exit code.** toolchain-final-pass.md copies the P6-T7 collect exit code, so it needs its own `ExpectedExitCode:` under branch (b) and an "equals its declared expectation" clause.
- **Exclusion parentheticals must restate all exclusions of the definition they cite.** P0-T3's `PATHS-CITED` aside named only `.gitignore`; the definition also excludes FEATURE/ and .claude/.

**Why:** each is a wording seam the executor cannot repair at run time (it may not edit gates), so a round is spent on it.

**How to apply:** when adding any printed control, derive it from the same variable as the predicate and pair negative with positive; when writing "EXIT_CODE scoped to X", check X prints a label; put schema rows at the top of every artifact description; grep `edit only|only the five characters` after adding any repair branch.
