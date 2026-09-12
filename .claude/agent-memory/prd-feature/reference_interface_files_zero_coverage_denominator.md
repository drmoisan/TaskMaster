---
name: interface-files-zero-coverage-denominator
description: C# interface-only files in this repo contribute zero lines to Cobertura coverage; the evidence trio (no member body, net48 forecloses DIM, no <class> element with MailItemActionsAdapter as positive control) is reusable across the quickfiler-per-file-coverage epic
metadata:
  type: reference
---

Interface-only `.cs` files are legitimately outside the coverage denominator here, and the argument
does not need re-deriving per file. The three independent proofs:

1. No IL-producing construct — every member declaration terminates in `;`; no member body, no
   `static` member, no `const`, no attribute, no nested type.
2. `QuickFiler/QuickFiler.csproj:13` is `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>`.
   Default interface implementations need CLR support .NET Framework does not have, so Roslyn rejects
   them regardless of `LangVersion=preview` (line 14). An interface file on this target cannot
   silently acquire executable content later.
3. A committed instrumented Cobertura artifact emits **no `<class>` element** for any
   `QuickFiler.*I<Uppercase>` type. Positive control: the concrete class
   `QuickFiler.Interfaces.MailItemActionsAdapter` **is** present, proving the instrumenter reaches the
   `QuickFiler\Interfaces\` folder — so the absence is a property of interfaces, not of the folder or
   of a `coverage.config` exclusion.

Rule text that matches: `.claude/rules/general-unit-test.md` § Coverage Requirements carve-out for
"C# interface-only files". This is the correct instrument, **not** the `CLAUDE.md` § UT2 COM/VSTO
exemption — that exemption governs lines that are executable but hard to reach, and implies a
testability debt that a zero-line file does not have.

Corollary: reflection shape-assertion tests on such files buy zero coverage (test-assembly lines are
excluded by policy), duplicate a stronger compiler check, and pin a contract siblings may need to
change. Reject them.

**Where this recurs:** the `quickfiler-per-file-coverage` epic (#136) states ~24 of 121 compiled
QuickFiler files are interface-only; every child that owns one needs this argument. Related:
[[net48-no-init-record-struct]], [[ac-gates-verify-satisfiability]].
