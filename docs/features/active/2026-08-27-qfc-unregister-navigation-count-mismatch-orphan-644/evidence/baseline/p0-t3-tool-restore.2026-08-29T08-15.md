# Baseline — dotnet local tool restore ([P0-T3])

- Issue: #644
- Task: `[P0-T3]`
- Timestamp: 2026-08-29T08-15

Command: `dotnet tool restore`
Working directory: repository root (`<repo-root>`)
EXIT_CODE: 0

Output:

```
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier

Restore was successful.
```

Output Summary: The command exited 0 and its output names `csharpier`, restored at the
manifest-pinned version 1.2.6 from `dotnet-tools.json`. Every CSharpier invocation in this plan
is therefore made through `dotnet tool run csharpier`, which resolves this pinned version rather
than any globally installed one.
