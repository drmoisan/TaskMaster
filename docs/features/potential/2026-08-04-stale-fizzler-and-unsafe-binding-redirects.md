# stale-fizzler-and-unsafe-binding-redirects (Potential Bug)

- Date captured: 2026-08-04
- Author: Dan Moisan
- Status: Draft

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

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

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
