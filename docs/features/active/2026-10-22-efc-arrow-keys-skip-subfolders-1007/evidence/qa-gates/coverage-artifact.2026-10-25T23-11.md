# Coverage Artifact for Feature Review (Issue #1007, P19-T6)

Timestamp: 2026-10-25T23-11
Command: Copy-Item coverage/coverage.cobertura.jacoco.xml artifacts/csharp/coverage.xml; Get-FileHash -Algorithm SHA256 on both files; counter-type count; git check-ignore and git ls-files on the copy (helper under SCRATCH)
EXIT_CODE: 0
Output Summary: the JaCoCo package projection written by the P19-T4 run at 23-09 was copied to `artifacts/csharp/coverage.xml`. Both files have SHA-256 `B6548FF6816BB0B0D72F6DD68281ADE26E3120121A3C75D8781546ACF756C452`, so the hashes are equal. The copy contains 9 `counter` elements of type `LINE` and 9 of type `BRANCH`. The copy is ignored by git (`git check-ignore` succeeds) and untracked (0 entries in `git ls-files`), so it stays out of the commit.

| File | SHA-256 |
|---|---|
| coverage/coverage.cobertura.jacoco.xml | B6548FF6816BB0B0D72F6DD68281ADE26E3120121A3C75D8781546ACF756C452 |
| artifacts/csharp/coverage.xml | B6548FF6816BB0B0D72F6DD68281ADE26E3120121A3C75D8781546ACF756C452 |
