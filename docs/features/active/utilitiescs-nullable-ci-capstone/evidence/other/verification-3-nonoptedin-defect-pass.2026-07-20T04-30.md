# P4-T5 — Verification 3: Non-Opted-In Defect Does Not Fail the Gate

Timestamp: 2026-07-20T04-30

## Candidate correction

The originally-illustrative non-opted-in candidate, `UtilitiesCS/Dialogs/ActionButton.cs`, was
found (via the corrected ripgrep-based re-grep, see `nullable-opt-in-regrep.2026-07-20T04-10.md`)
to actually be opted-in (`#nullable enable` at line 11). Substituted with
`UtilitiesCS/EmailIntelligence/Bayesian/Obsolete/BayesianClassifier.cs`, ripgrep-confirmed to
carry zero `#nullable` pragmas anywhere in the file.

## Defect introduced

`UtilitiesCS/EmailIntelligence/Bayesian/Obsolete/BayesianClassifier.cs`, in the parameterless
constructor:

```csharp
public BayesianClassifier()
{
    string local = null;
    Console.WriteLine(local.Length);
}
```

Command: `MSBuild.exe TaskMaster.sln -t:Rebuild -p:Configuration=Debug "-p:Platform=Any CPU" -p:TreatWarningsAsErrors=true`

EXIT_CODE: 0

Output Summary: Build succeeded, 0 Error(s) (the same pre-existing CS2002 warning noted
throughout Phase 2/Phase 4 remains, unrelated to this file). No `error CS` diagnostic line
appears anywhere in the build output, and no diagnostic line references
`BayesianClassifier.cs`. This confirms the gate does not cross-block on a null-literal
dereference in a non-opted-in file: the same defect class that failed the build in the opted-in
candidate (P4-T3) does not fail the build here, because the file carries no `#nullable` pragma.
