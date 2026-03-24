# Change Plan

## Objective
Remove `BayesianSerializationHelper` unit-test filesystem dependencies so the flaky async serialization test is deterministic and compliant with repo unit-test policy.

## Steps
- [x] Add a deterministic regression test path for `SerializeAndSaveAsync` that uses an in-memory test double instead of disk.
- [x] Refactor `BayesianSerializationHelper` to route filesystem reads, deletes, and stream creation through overridable seams.
- [x] Move `BayesianSerializationHelper` tests into a dedicated file and replace filesystem-backed assertions with in-memory assertions.
- [ ] Run the required C# toolchain and confirm the updated tests pass.
