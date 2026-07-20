# P2-T19 — ToDoModel.Test.csproj CS0169 Remediation (Layer 2, dead-code deletion)

Timestamp: 2026-07-20T02-10

## Grep confirmation (zero live references)

Grep of `mockApplication|_mockPrefix|_peopleScoDictionaryNew` across
`ToDoModel.Test/Data Model/People/PeopleScoDictionaryNewTests.cs`:

```
22:        private Mock<Outlook.Application> mockApplication;
23:        private Mock<IPrefix> _mockPrefix;
24:        private PeopleScoDictionaryNew _peopleScoDictionaryNew;
72:        //    _mockPrefix.Setup(p => p.Value).Returns("prefix");
75:        //    bool result = _peopleScoDictionaryNew.IsPeopleCategory(testCategory);
86:        //    _mockPrefix.Setup(p => p.Value).Returns("prefix");
89:        //    bool result = _peopleScoDictionaryNew.IsPeopleCategory(testCategory);
106:        //    _mockPrefix.Setup(p => p.Value).Returns("prefix");
109:        //    var result = _peopleScoDictionaryNew.GetPeopleCatNames();
129:        //    bool result = _peopleScoDictionaryNew.CategoryExists(category);
147:        //    bool result = _peopleScoDictionaryNew.CategoryExists(category);
161:        //    string result = _peopleScoDictionaryNew.AddPrefix(seed, prefix);
176:        //    _peopleScoDictionaryNew.AddPrefix(seed, prefix);
188:        //    _peopleScoDictionaryNew.AddPrefix(seed, prefix);
198:        //    string result = _peopleScoDictionaryNew.SplitAddressToFirstLastName(address);
212:        //    string result = _peopleScoDictionaryNew.MatchToExisting(existingPeople, newPerson);
226:        //    string result = _peopleScoDictionaryNew.MatchToExisting(existingPeople, newPerson);
```

Every reference outside the three declaration lines (22-24) begins with `//` (commented-out test
body). `mockApplication` has zero references outside its own declaration (not even in comments).
Confirmed: zero live (non-commented) references to any of the three fields.

## Pattern applied

Dead-code deletion. Removed the three unused private field declarations (lines 22-24). The
surrounding commented-out test method bodies were left untouched.

## Verification

`git diff` for the file shows only lines 22-24 removed; every commented-out test body is
byte-identical to its pre-edit state.

Command: `MSBuild.exe "ToDoModel.Test/ToDoModel.Test.csproj" -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU -p:TreatWarningsAsErrors=true`

EXIT_CODE: 0

Output Summary: Build succeeded, 0 Warning(s), 0 Error(s). CS0169 no longer appears for this
file.

## Rationale

Zero live references confirmed by grep; deletion of genuinely-unused private fields in a test
fixture changes no observable behavior (no test method reads or writes these fields, since the
only bodies referencing them are commented out).
