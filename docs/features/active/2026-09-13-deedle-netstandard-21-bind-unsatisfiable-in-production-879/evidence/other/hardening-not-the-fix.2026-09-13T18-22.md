# Hardening Is Not The Fix — [P3-T5]

Timestamp: 2026-09-14T18-05

Command:

```
pwsh -NoProfile -Command '
Set-Location -LiteralPath "<repo-root>"
$p = "docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/spec.md"
foreach ($m in @(Select-String -LiteralPath $p -SimpleMatch -Pattern "cannot manufacture an assembly")) { Write-Output ("LINE=" + $m.LineNumber) }
'
```

The `Set-Location` prefix is mechanically necessary and measurement-neutral: this executor was launched
without worktree isolation, so its inherited working directory is a different checkout and every
repository-relative path in the plan would otherwise resolve into the wrong tree. It is disclosed here
rather than omitted.

EXIT_CODE: 0

Output Summary:

```
LINE=262
LINE=535
```

Two `LINE=` values were recorded, which satisfies the "at least two" acceptance condition.

`spec.md` already states the ground at both of those lines: a binding redirect rewrites an identity and
cannot manufacture an assembly. The `netstandard` `dependentAssembly` block added to
`TaskMaster/app.config` by `[P3-T4]` is therefore declarative hardening and not the remedy. The remedy is
the resolution ladder in `UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs`, whose rung 3 loads the
`2.0.0.0` facade from the runtime directory by absolute path and so does not depend on any redirect.
