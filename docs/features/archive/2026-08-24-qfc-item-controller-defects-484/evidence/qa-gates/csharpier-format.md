# Final QC stage 1 — CSharpier format (scope-locked)

Timestamp: 2026-08-26T13-41
Task: [P7-T1]

Command (run from the worktree root; the nine owned file paths of constraint C1 supplied
explicitly, not a repository-wide `.` argument, per decision D6):

```
dotnet tool run csharpier format QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs QuickFiler/Controllers/QfcItemController.EventWiring.cs QuickFiler/Controllers/QfcItemController.ViewerSetup.cs QuickFiler/Controllers/QfcItemController.MailActions.cs QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs
```

EXIT_CODE: 0

Tool output: `Formatted 9 files in 3440ms.` — "Formatted" is CSharpier's PROCESSED count, not a
rewritten count, so content change is determined below by SHA-256 comparison instead.

## Per-file SHA-256 before and after

| File | SHA-256 before | SHA-256 after | Content changed |
|---|---|---|---|
| `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs` | `bf0b886e8ccc77ecee583418ba840db4a73bae896098a4865ca380c454f0aefc` | `bf0b886e8ccc77ecee583418ba840db4a73bae896098a4865ca380c454f0aefc` | no |
| `QuickFiler/Controllers/QfcItemController.EventWiring.cs` | `378748eba4b24f5f739ae726aa967b947b768e4480477e5f9e16f7788ddcb8c1` | `378748eba4b24f5f739ae726aa967b947b768e4480477e5f9e16f7788ddcb8c1` | no |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | `b9ed07ec887d2f194819f716d79e72e4f9a50e4df5653e3e698dfc796ed4602d` | `b9ed07ec887d2f194819f716d79e72e4f9a50e4df5653e3e698dfc796ed4602d` | no |
| `QuickFiler/Controllers/QfcItemController.MailActions.cs` | `299bc8ad90640cf0505161315113bb4451d8d736e44aeb4d2b91a1ac0a41d58a` | `299bc8ad90640cf0505161315113bb4451d8d736e44aeb4d2b91a1ac0a41d58a` | no |
| `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` | `a3c35259f1c5e5d2ed8d8a3e5ba923a964e2b164abe9d9ac7b6b32ec30644e4b` | `a3c35259f1c5e5d2ed8d8a3e5ba923a964e2b164abe9d9ac7b6b32ec30644e4b` | no |
| `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs` | `55cb918b6fb4d629d4d6d4bd3eb7320a0fa4f3c947895b374dd420e32d0aefe1` | `55cb918b6fb4d629d4d6d4bd3eb7320a0fa4f3c947895b374dd420e32d0aefe1` | no |
| `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` | `a65daf290761c09b5dab70f269cd3e632e633430083d217c2101267ff8715fc7` | `a65daf290761c09b5dab70f269cd3e632e633430083d217c2101267ff8715fc7` | no |
| `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs` | `502633b48282d7211b6994f8fcc21b7c0a503d93d9fd89358fdfb8e186a9a178` | `502633b48282d7211b6994f8fcc21b7c0a503d93d9fd89358fdfb8e186a9a178` | no |
| `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` | `6293904bd2dfacc7c2678481409d576ff651a400ae550cc3a628f89ec6958cdf` | `6293904bd2dfacc7c2678481409d576ff651a400ae550cc3a628f89ec6958cdf` | no |

All nine hashes are identical before and after. No owned file was rewritten by this pass, because
every file was already formatted with the manifest-pinned CSharpier 1.2.6 as it was edited during
Phases 4 and 5. The toolchain loop therefore does not restart on account of this stage.

Output Summary: EXIT_CODE 0. Nine files processed, zero rewritten (SHA-256 identical before and
after for all nine).
