# stale-fizzler-and-unsafe-binding-redirects (Potential Bug)

- Date captured: 2026-08-04
- Author: Dan Moisan
- Status: Draft

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Work Mode: minor-audit

## Summary

Two families of `app.config` binding redirects name assembly versions that are not deployed. Twelve project configs redirect `Fizzler` to `1.3.0.0` while the deployed assembly is `1.3.1.0`, and `SVGControl/app.config` redirects `System.Runtime.CompilerServices.Unsafe` to `6.0.2.0` while the deployed assembly is `6.0.3.0` and all sixteen sibling configs say `6.0.3.0`. This is the same defect class as bug #418, where a redirect to a non-deployed `ExCSS` version caused `SvgDocument.Open` to fail in hosts that apply the redirect.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- .NET/framework: .NET Framework 4.8.1 (`net481`), WinForms + VSTO
- Command/flags used: static inspection of `*/app.config` against `packages/` and against assembly metadata
- Data source or fixture: the repository's own `app.config` set and restored `packages/` tree

## Steps to Reproduce

Verified 2026-08-04 on branch `bug/svg-renderer-null-document-nre-418` at commit `296eac95`:

1. `grep -rl 'name="Fizzler"' --include=app.config .` returns **13** files. Of their redirects, **12** read `newVersion="1.3.0.0"` and **1** reads `newVersion="1.3.1.0"`.
2. The only deployed Fizzler is `packages/Fizzler.1.3.1/`, and `[System.Reflection.AssemblyName]::GetAssemblyName('packages\Fizzler.1.3.1\lib\netstandard2.0\Fizzler.dll').Version` returns **`1.3.1.0`**. No `1.3.0.0` assembly exists anywhere in the repository.
3. Enumerating `System.Runtime.CompilerServices.Unsafe` redirects across all seventeen project configs: sixteen read `newVersion="6.0.3.0"`; `SVGControl/app.config` alone reads `newVersion="6.0.2.0"`.
4. `SVGControl/bin/Debug/System.Runtime.CompilerServices.Unsafe.dll` is assembly version **`6.0.3.0`**, and both `SVGControl` and `SVGControl.Test` pin package version `6.1.2`.

## Expected Behavior

Every `bindingRedirect` `newVersion` names an assembly version that is actually deployed to the output directory, so a host that honors the redirect can satisfy the bind.

## Actual Behavior

Twelve Fizzler redirects and one `Unsafe` redirect name versions that exist nowhere in the repository. A host that applies these redirects would request an assembly that cannot be found.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: no runtime failure captured. Both findings are currently latent — see below.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

Both findings are latent today, which is why they were deferred rather than folded into #418:

- **Fizzler is inert.** Research during #418 established that nothing in the deployed graph carries a `Fizzler` assembly reference. `Svg 3.4.7` does not reference it (its CSS selector work goes through ExCSS `StylesheetParser`, and the `Fizzler` string is absent from `Svg.dll`), and `ExCSS 4.3.1` does not reference it. The `using Fizzler;` at `SVGControl/PictureBoxSVG.cs:14` is unused and emits no `AssemblyRef`. With no requesting reference, the redirect is never consulted.
- **The `Unsafe` outlier is masked.** `SVGControl` is a library, so the redirect in `SVGControl/app.config` is not the one the CLR reads at runtime; the host's config governs, and every host config in the repository says `6.0.3.0`.

The severity is Low on current evidence, not on principle. Either becomes live the moment a dependency starts carrying the corresponding reference — which is precisely how #418 arose, and #418 was rated High.

## Suspected Cause / Notes

Package updates advanced the deployed assembly versions without a corresponding sweep of the `bindingRedirect` values. PR #419 moved `ExCSS` to 4.3.2, `Svg` to 3.4.8, and `Unsafe` to 6.1.2; the `SVGControl` `Unsafe` redirect was left at `6.0.2.0` and the Fizzler redirects at `1.3.0.0`.

Worth considering as part of the fix: a mechanical check that every `bindingRedirect` `newVersion` in the repository resolves to an assembly version present under `packages/`. That would have caught #418's `ExCSS 4.2.4.0` redirect — a version that existed nowhere — before it reached a designer session, and it would catch the next instance without anyone having to notice by hand. A manual version sweep will not, since sibling-config agreement is exactly what let the `SVGControl` `Unsafe` outlier persist unnoticed.

The single Fizzler config already at `1.3.1.0` should be identified, since it may indicate a partial fix already attempted.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: a test over the repository's `app.config` set asserting every `bindingRedirect` `newVersion` matches a deployed assembly version. Language choice depends on where it lands — a Pester test under `tests/scripts/` carries the PoshQC toolchain and the `>= 85%` line / `>= 75%` branch floors per `.claude/rules/powershell.md`.
- [ ] Integration scenario to retest: open a form hosting `PictureBoxSVG` in the WinForms designer after the sweep, confirming no regression in the #418 fix path.
- [ ] Manual verification notes: confirm the twelve Fizzler redirects and the one `Unsafe` outlier, then re-verify each edited config against its deployed assembly version rather than against its sibling configs.

Referred here from #418, which scoped both out explicitly: "Fizzler binding redirects" and "`System.Runtime.CompilerServices.Unsafe` redirects in any project other than `SVGControl.Test`". #418's `evidence/baseline/` artifacts and its research artifact carry the supporting assembly-metadata analysis.

## Preparation Reconciliation (2026-10-02, origin/main 59cbab04f)

The record above was captured 2026-08-04. The state on main at preparation time differs in three respects; the Acceptance Criteria below are written against the current state.

- The `System.Runtime.CompilerServices.Unsafe` outlier is already fixed. All 17 `app.config` files redirect it to `6.0.3.0`, including `SVGControl/app.config` (changed by issue 929, PR 949). No edit is planned for it.
- Fizzler: 13 configs carry a redirect. `SVGControl/app.config` (fixed by issue 929) and `UtilitiesCS/app.config` read `1.3.1.0`; eleven still read `oldVersion="0.0.0.0-1.3.0.0" newVersion="1.3.0.0"`. The deployed `Fizzler.dll` is `1.3.1.0` and the only csproj `Reference` for it is `1.3.1.0`.
- A gate asserting that every `newVersion` in the repository matches a deployed version is not satisfiable today. A text-only measurement (all 17 configs, 1176 redirect entries, 18 csproj files) found 148 entries in 16 distinct assembly and version pairs whose `newVersion` is below the only csproj `Reference` version (Fizzler is one of the 16), and 29 entries for 3 assemblies with no csproj `Reference`. The gate in this fix therefore ratchets: it fails on any mismatch outside a recorded known-debt set and fails when a known-debt entry stops mismatching. Correcting the other 15 pairs is out of scope for this fix and is reported as a follow-up.

## Acceptance Criteria

- [ ] AC1: All 13 `app.config` files that carry a Fizzler `bindingRedirect` read `oldVersion="0.0.0.0-1.3.1.0" newVersion="1.3.1.0"`; the 11 stale files (QuickFiler, QuickFiler.Test, SVGControl.Test, Tags, TaskMaster, TaskTree, TaskVisualization, TaskVisualization.Test, ToDoModel, ToDoModel.Test, UtilitiesCS.Test) each change on that one line only, with the UTF-8 BOM and CRLF line endings preserved.
- [ ] AC2: All 17 `app.config` files still redirect `System.Runtime.CompilerServices.Unsafe` to `6.0.3.0` (already delivered by issue 929; verified, not edited).
- [ ] AC3: A new Pester-tested detector reports a `bindingRedirect` whose `newVersion` equals no csproj `Reference` version for that assembly name, with a negative control (fixture redirect to a version no `Reference` provides yields one finding), a positive control (matching fixture yields none), and an examined-entry count that guards against a vacuous zero-finding pass.
- [ ] AC4: A repository-level Pester test runs the detector over every `app.config` against every csproj `Reference` and asserts that the findings equal exactly the recorded known-debt set (15 assembly and version pairs, none of them Fizzler or Unsafe) and that the unverifiable names equal exactly the recorded set of 3; a regression of any Fizzler redirect, a new mismatch, or a stale known-debt entry fails the test.
- [ ] AC5: The Fizzler regression test fails before the 11 config edits and passes after them (fail-before evidence recorded).
- [ ] AC6: PoshQC format, analyze, and test report no errors, and Pester line coverage for the new module is at least 85 percent.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
