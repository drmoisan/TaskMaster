# Receiver-Scoped Reflection Call Sites and Variable-Argument Closure (P2-T3) — discharges AC-9

- **Issue:** #635
- **Plan task:** [P2-T3]

Timestamp: 2026-08-29T06-34

## Output Summary

Eight reflection call sites in the QuickFiler test tree have `typeof(QfcCollectionController)` as their
receiver and a member-name argument that is neither a string literal nor a `const string` identifier.
Each of the eight is named individually below by file and line with the API it calls, the form of its
member-name argument, and its closure statement. Three further sites pass a named constant whose
declared value is recorded; that value is not one of the thirteen identifiers. Every remaining
receiver-scoped site passes a string literal, and each literal is enumerated and is not one of the
thirteen.

VARIABLE_ARGUMENT_SITES: 8
NAMED_CONSTANT_SITES: 3

## Command 1

Command:

```
git grep -n -I -F -e "typeof(QfcCollectionController)" -- "QuickFiler.Test/*"
```

EXIT_CODE: 0

Output, verbatim, 26 lines:

```
QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs:38:            FieldInfo field = typeof(QfcCollectionController).GetField(name, NonPublicInstance);
QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs:51:            FieldInfo field = typeof(QfcCollectionController).GetField(name, NonPublicInstance);
QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs:65:            FieldInfo field = typeof(QfcCollectionController).GetField(name, NonPublicInstance);
QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs:80:            FieldInfo field = typeof(QfcCollectionController).GetField(name, NonPublicStatic);
QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs:95:            FieldInfo field = typeof(QfcCollectionController).GetField(name, NonPublicStatic);
QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs:118:            MethodInfo method = typeof(QfcCollectionController).GetMethod(name, NonPublicInstance);
QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs:149:                FormatterServices.GetUninitializedObject(typeof(QfcCollectionController));
QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs:76:            typeof(QfcCollectionController)
QuickFiler.Test/Controllers/QfcCollectionControllerDefects468ConversationTests.cs:41:            Type controller = typeof(QfcCollectionController);
QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs:44:            FieldInfo counter = typeof(QfcCollectionController).GetField(
QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs:66:            FieldInfo counter = typeof(QfcCollectionController).GetField(
QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs:86:            FieldInfo counter = typeof(QfcCollectionController).GetField(
QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs:115:            ConstructorInfo[] constructors = typeof(QfcCollectionController).GetConstructors();
QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs:34:            var field = typeof(QfcCollectionController).GetField(
QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs:71:                FormatterServices.GetUninitializedObject(typeof(QfcCollectionController));
QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs:112:                FormatterServices.GetUninitializedObject(typeof(QfcCollectionController));
QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:37:                FormatterServices.GetUninitializedObject(typeof(QfcCollectionController));
QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:69:            typeof(QfcCollectionController)
QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:148:                FormatterServices.GetUninitializedObject(typeof(QfcCollectionController));
QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:167:            typeof(QfcCollectionController)
QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:178:            typeof(QfcCollectionController)
QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:255:                FormatterServices.GetUninitializedObject(typeof(QfcCollectionController));
QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:262:            typeof(QfcCollectionController)
QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:344:                FormatterServices.GetUninitializedObject(typeof(QfcCollectionController));
QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:381:            typeof(QfcCollectionController)
QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:496:                typeof(QfcCollectionController)
```

## Command 2

Command:

```
git grep -n -I -F -e "GetField(" -e "GetMethod(" -- "QuickFiler.Test/Controllers/*"
```

EXIT_CODE: 0

The command printed 191 lines across the QuickFiler test tree's `Controllers` directory. That output is
the superset from which the receiver-scoped subset is drawn; it is not reproduced in full here because
the great majority of its lines have a receiver other than `QfcCollectionController` and are outside
the derivation below. Every line of it that intersects the derivation is reproduced verbatim in the
enumeration sections below, together with the argument line that command 1 alone does not show.

## Derivation of the set to enumerate

A site belongs to the variable-argument set when its reflection receiver is the expression
`typeof(QfcCollectionController)` and its member-name argument is neither a string literal nor an
identifier declared as a `const string` in the same file. The receiver expression may sit on the same
printed line as the call or on the immediately preceding printed line; the member-name argument may sit
on the same printed line as the call or on the immediately following printed line. A site whose
argument is a `const string` identifier is enumerated separately under the named-constant section and
is not counted toward `VARIABLE_ARGUMENT_SITES`.

Applying that derivation to the 26 lines of command 1:

- Eight lines are not reflection member lookups at all. Seven are
  `FormatterServices.GetUninitializedObject(typeof(QfcCollectionController))`, at
  QfcCollectionController.TestSupport.cs 149, QfcCollectionControllerNavigationDigitsTests.cs 71 and
  112, and QfcCollectionControllerTests.cs 37, 148, 255 and 344; each constructs an uninitialized
  instance from a `Type` and takes no member-name argument. The eighth is `GetConstructors()` at
  QfcCollectionControllerDefects468Tests.cs 115, which takes no member-name argument either.
- Three lines at QfcCollectionControllerDefects468Tests.cs 44, 66 and 86 pass a `const string`
  identifier and are enumerated under the named-constant section.
- Six lines are literal-argument sites — QfcCollectionControllerDarkModeTests.cs 76 and
  QfcCollectionControllerTests.cs 69, 167, 178, 262 and 496 — each with the receiver on the printed
  line and the literal argument on the following line.
- One line, QfcCollectionControllerDefects468ConversationTests.cs 41, binds the receiver to a local
  `Type controller`; its two calls at lines 44 and 45 pass string literals.
- The remaining eight are the variable-argument set.

The five groups account for all 26 printed lines: `8 + 3 + 6 + 1 + 8 = 26`.

## The eight variable-argument sites

| # | File | Line | API | Member-name argument | Closure statement |
|---|---|---|---|---|---|
| 1 | QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs | 38 | `GetField` | variable `name`, on the same line | Bounded by the string literals present in the calling assemblies' source text; none of the thirteen occurs there as a literal, so this site cannot supply one. |
| 2 | QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs | 51 | `GetField` | variable `name`, on the same line | As site 1. |
| 3 | QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs | 65 | `GetField` | variable `name`, on the same line | As site 1. |
| 4 | QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs | 80 | `GetField` | variable `name`, on the same line | As site 1. |
| 5 | QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs | 95 | `GetField` | variable `name`, on the same line | As site 1. |
| 6 | QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs | 118 | `GetMethod` | variable `name`, on the same line | As site 1. |
| 7 | QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs | 34 | `GetField` | variable `name`, supplied on line 35 | As site 1. |
| 8 | QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs | 382 | `GetField` | variable `name`, on the same line; receiver `typeof(QfcCollectionController)` on line 381 | As site 1. |

VARIABLE_ARGUMENT_SITES: 8

Sites 7 and 8 span two lines, so the printed argument text is reproduced here:

```
QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs:32:        private static void SetControllerField(object target, string name, object value)
QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs:34:            var field = typeof(QfcCollectionController).GetField(
QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs:35:                name,
QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs:36:                BindingFlags.NonPublic | BindingFlags.Instance
```

```
QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:380:        private static void SetControllerField(object target, string name, object value) =>
QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:381:            typeof(QfcCollectionController)
QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:382:                .GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)
```

In both cases the variable `name` is the `string name` parameter of the enclosing private static
helper, so its value is supplied by the helper's callers.

## The three named-constant sites

| File | Line | API | Member-name argument | Argument line |
|---|---|---|---|---|
| QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs | 44 | `GetField` | `ReentrancyCounterField` | 45 |
| QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs | 66 | `GetField` | `ReentrancyCounterField` | 67 |
| QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs | 86 | `GetField` | `ReentrancyCounterField` | 87 |

The constant is declared at line 30 of the same file:

```
QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs:30:        private const string ReentrancyCounterField = "removespecificcontrolgroupcounter";
```

Its declared value is `"removespecificcontrolgroupcounter"`. That value is not one of the thirteen
identifiers. A named constant is literal-equivalent — the compiler substitutes its value at every use
site, so the set of values it can take at run time is the single declared value — and it is therefore
closed by naming its value rather than by the variable-argument argument. All three sites resolve the
same private static field, which is a live member of `QfcCollectionController` and was not removed by
commit `63eebd47`.

## The literal-argument receiver-scoped sites

Recorded for completeness of the receiver-scoped enumeration. Each passes a string literal, and none of
the literals is one of the thirteen identifiers.

```
QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:69:            typeof(QfcCollectionController)
QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:70:                .GetField("_itemGroupsToMove", BindingFlags.NonPublic | BindingFlags.Instance)
QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:167:            typeof(QfcCollectionController)
QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:168:                .GetField("_itemGroups", BindingFlags.NonPublic | BindingFlags.Instance)
QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:178:            typeof(QfcCollectionController)
QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:179:                .GetField("_removeGroupByEntryId", BindingFlags.NonPublic | BindingFlags.Instance)
QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:262:            typeof(QfcCollectionController)
QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:263:                .GetField("_removeGroupByEntryId", BindingFlags.NonPublic | BindingFlags.Instance)
QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:496:                typeof(QfcCollectionController)
QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:497:                    .GetField("_itemGroups", BindingFlags.NonPublic | BindingFlags.Instance)
QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs:76:            typeof(QfcCollectionController)
QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs:77:                .GetField("_itemGroups", BindingFlags.NonPublic | BindingFlags.Instance)
QuickFiler.Test/Controllers/QfcCollectionControllerDefects468ConversationTests.cs:41:            Type controller = typeof(QfcCollectionController);
QuickFiler.Test/Controllers/QfcCollectionControllerDefects468ConversationTests.cs:44:            MethodInfo resolve = controller.GetMethod("ResolveConversationInsertions", AnyStatic);
QuickFiler.Test/Controllers/QfcCollectionControllerDefects468ConversationTests.cs:45:            MethodInfo reconcile = controller.GetMethod("ReconcileInsertionCount", AnyStatic);
```

The distinct literals passed are `"_itemGroupsToMove"`, `"_itemGroups"`, `"_removeGroupByEntryId"`,
`"ResolveConversationInsertions"` and `"ReconcileInsertionCount"`. None is one of the thirteen.

## The closure argument, stated in full

For each variable-argument site, the set of values the member-name variable can take is bounded by the
string literals present in the source text of the assemblies that call it. [P1-T4] established that the
thirteen identifiers occur in the QuickFiler test tree exactly once, inside a triple-slash documentation
comment at QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs line 60, and
occur nowhere in the QuickFiler production tree except the two lines of the live preserved member
`LoadItemGroupsAndViewers_02`. Therefore no call site can supply one of the thirteen.

The argument is mechanical rather than a review judgment: it does not depend on reading the control flow
of any test, only on the measured absence of the identifiers from the source text that could supply a
literal. [P1-T4]'s enumeration is exhaustive over tracked `.cs` files, so the bound holds over the whole
compiled corpus and not only over the sites reviewed here.

## The stated limit of the closure argument

The argument does not cover a member name assembled at run time by string concatenation or
interpolation. Such a name would not appear as a literal anywhere in the source text and would
therefore escape the bound. No such construction was observed at any site enumerated here — every
member-name argument enumerated above is either a literal, a `const string` identifier, or a `string`
parameter of a private static helper — but its absence in general was not proved. This limit is
recorded rather than argued away.

## Reconciliation note

AC-9 in the specification names six variable-argument reflection call sites; the mechanical derivation
above yields eight. The eight are a superset of any six the specification could mean, so enumerating all
eight individually discharges AC-9.

The specification's baseline section describes AC-9's six as `GetField(` sites, but the measured set
contains seven variable-argument `GetField(` sites — QfcCollectionController.TestSupport.cs lines 38,
51, 65, 80 and 95, QfcCollectionControllerNavigationDigitsTests.cs line 34, and
QfcCollectionControllerTests.cs line 382 — together with one variable-argument `GetMethod(` site at
QfcCollectionController.TestSupport.cs line 118. No six-element subset can be identified with the
specification's six, so this artifact records the full eight and does not claim a subset identity. The
count difference is recorded here as an evidence note; the approved specification is not edited to
change the figure.
