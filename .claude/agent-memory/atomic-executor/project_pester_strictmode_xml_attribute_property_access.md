---
name: pester-strictmode-xml-attribute-property-access
description: Under Set-StrictMode -Version Latest, $node.attr on an XmlElement lacking that attribute THROWS PropertyNotFoundStrict - a Cobertura fixture that omits branch="False" fails with the wrong error, breaking pinned fail-before values
metadata:
  type: project
---

`tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` line 1 is
`Set-StrictMode -Version Latest`, and that mode propagates into production functions
dot-sourced in `BeforeAll`. Under it, PowerShell property access on a *missing* XML
attribute throws `The property '<name>' cannot be found on this object`, it does not
return `$null`.

Measured 2026-08-10 with a Pester 5.6.1 probe against the real
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`:

- Cobertura fixture whose `<line>` elements omit `branch` -> the `It` fails with
  `The property 'branch' cannot be found on this object` (thrown at
  `Helpers.ps1:128`, `$line.branch -eq 'True'`).
- Identical fixture with `branch="False"` added -> the `It` fails with the intended
  assertion text (`Expected: '3' But was: '6'`).

**Why:** every pre-existing fixture in that file carries `branch="False"` on every
`<line>`, so the hazard is invisible until someone authors a new fixture from a prose
spec that only lists `number` and `hits`. A plan that pins fail-before evidence to
exact numeric error text (e.g. "F1 reports 6/4") becomes unsatisfiable, because the
run produces a StrictMode error instead of the assertion diff. `FailedCount` is
unchanged, so a count-only gate does not catch it.

**The hazard is not specific to `branch`.** Measured 2026-08-10 (round 2): a two-class
same-`filename` merge fixture whose `<class>` elements omit `complexity` throws
`The property 'complexity' cannot be found on this object` at `Helpers.ps1:279`
(`$group | ForEach-Object { if ($_.complexity) ... }` inside
`Merge-CoberturaClassesByFilename`). Fixing only `branch` is therefore necessary but
not sufficient: any merge-path fixture also needs `complexity`. Single-class fixtures
never reach that line and are unaffected.

**How to apply:** when authoring or validating Cobertura/XML fixtures for this suite,
do not patch attributes one at a time. Walk every bare `$node.<attr>` read on the code
path the fixture will traverse and require each of those attributes in the fixture. For
this file that is: `branch` and `hits` on every `<line>` on BOTH the
`<methods>/<method>/<lines>` axis and the class-level `<lines>` axis; `complexity` and
`name` on every `<class>` in a merge-path fixture; `name` on every `<package>`. In
production helpers read the flag as `$node.GetAttribute('branch') -eq 'True'` (the
existing union builder at `Helpers.ps1:236` already does); `HasAttribute`/`GetAttribute`
are StrictMode-safe, bare property access is not. `$node.number` is safe only because
every fixture and every real generator emits it.

Related: [[project_poshqc_pester_mcp_exit_minus1]],
[[project_koverage_cobertura_postprocessing_shape]].
