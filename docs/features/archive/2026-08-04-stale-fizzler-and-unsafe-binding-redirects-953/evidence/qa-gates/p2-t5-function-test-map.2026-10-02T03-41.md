# P2-T5 Per-function test enumeration for BindingRedirectVerification.psm1 (AC6 second clause)

Timestamp: 2026-10-02T03-41
Command: Grep pattern `^Export-ModuleMember|^    '(ConvertTo-ReferenceVersionMap|Find-StaleBindingRedirect)'` over `scripts/dependencies/BindingRedirectVerification.psm1`; Grep pattern `^\s*It '` with -n over `tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1`
EXIT_CODE: 0

Export-ModuleMember list (module lines 136-138): `ConvertTo-ReferenceVersionMap`, `Find-StaleBindingRedirect`.

Find-StaleBindingRedirect (test file line in parentheses; each It name appears exactly once on the Grep over the test file):

Positive path:
- (118) `reports no finding when the Fizzler redirect names the deployed 1.3.1.0` (test 2)
- (131) `compares newVersion only and ignores the oldVersion range` (test 3)
- (156) `skips a dependentAssembly block that carries no bindingRedirect` (test 5)
- (194) `accepts a newVersion equal to any one of several deployed versions` (test 8)
- (246) `names 1.3.1.0 in every Fizzler binding redirect across the repository app.config files` (test 13)
- (275) `reports exactly the recorded known-debt set and unverifiable set over every app.config against every csproj Reference` (test 14)

Negative path:
- (102) `reports one finding when the Fizzler redirect names 1.3.0.0 and the provider deploys 1.3.1.0` (test 1)
- (142) `lists an assembly the provider knows nothing about as unverifiable and not as a finding` (test 4)
- (205) `throws when the text is not an application configuration document` (test 9)

Edge path:
- (169) `counts one examined entry per bindingRedirect-bearing block` (test 6)
- (180) `examines zero entries and reports nothing for empty text` (test 7)

ConvertTo-ReferenceVersionMap:

Positive path:
- (216) `maps a Reference Include with a Version to its assembly name` (test 10)

Negative path:
- (226) `omits a Reference Include that declares no Version` (test 11)

Edge path:
- (235) `unions the versions of one assembly across several project texts` (test 12; one assembly at two versions across several project texts)

Acceptance: both exported names appear with at least one positive, one negative and one edge It each (Find-StaleBindingRedirect 6, 3 and 2; ConvertTo-ReferenceVersionMap 1, 1 and 1); all 14 cited It names are present in the test file, each once, as shown by the Grep over the `It '` lines (14 matches, lines 102 to 275).

Output Summary: Both exported functions have positive, negative and edge tests; 14 of 14 It names confirmed present once in the test file.
