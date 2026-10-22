# P0-T5 — CSharpier Tool Manifest Restore

Timestamp: 2026-09-01T08-05

Command: `dotnet tool restore` (run from the repository root)

EXIT_CODE: 0

Output Summary: The manifest-pinned local tool was restored successfully. Verbatim output:

```text
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier

Restore was successful.
```

The restored tool is **`csharpier`** at version **`1.2.6`**, which matches the pin recorded in the
repository-root `dotnet-tools.json` and the version `CLAUDE.md` names as the CI-parity version. All
subsequent formatting invocations in this plan go through `dotnet tool run csharpier`, so the
manifest-pinned 1.2.6 is the version used and no globally installed CSharpier is invoked.

Acceptance: met. `EXIT_CODE: 0`, and the `Output Summary:` names `csharpier` and the version `1.2.6`.
