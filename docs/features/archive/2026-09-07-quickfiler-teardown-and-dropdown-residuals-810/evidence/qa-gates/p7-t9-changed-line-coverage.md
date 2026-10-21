# [P7-T9] Changed-Line Coverage

Timestamp: 2026-09-08T10-27
Command: `git diff --unified=0 origin/main -- QuickFiler` to derive the changed production line set mechanically from the `@@` hunk headers, cross-referenced against the `line` elements of `coverage/810-post.cobertura.xml` for the same file. Where a file contributes several Cobertura class elements, including compiler-generated closure and state-machine classes, the hit count for a line is the maximum reported across them.

CHANGED-LINE-COVERAGE: 97.12

Over the measurable subset: 101 covered of 104 measurable.

NEW-MODULE-COVERAGE: 100.00

`QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs` reports 9 measurable lines, all 9 covered. The task requires at least 90 and the measured value is 100.00.

UNMEASURABLE-FILES: `QuickFiler/Viewers/QfcFormViewer.cs` — 9 added lines, none measurable. That class carries a class-level coverage exemption attribute and therefore emits no Cobertura class element at all, so its lines cannot be reported as covered or uncovered in either direction. They are recorded as `NOT MEASURABLE` with that reason rather than as zero, which would misstate them as uncovered.

## CHANGED-LINES per file

### `QuickFiler/Controllers/QfcFormController.Deactivate.cs`

Added lines: 27, 91, 118, 119, 120, 121, 122, 123, 124, 125. Measurable: 5. Covered: 5.

```
27:1  122:1  123:1  124:1  125:1
```

Lines 91, 118, 119, 120 and 121 emit no `line` node: 91 is the method signature and 118 through 121 are the four added comment lines of the AC1 rationale block. Line 122 is the `if (` opening of the split guard condition, and 123 through 125 are its conjuncts, closing parenthesis and the guarded `return`.

### `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`

Added lines: 144, 145, 146, 147. Measurable: 4. Covered: 4.

```
144:1  145:1  146:1  147:1
```

These are the four lines of the split `RunTeardownStage("park-focus", ...)` call.

### `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs`

Added lines: 217 through 260 and 262, 264 through 271. Measurable: 43. Covered: 40.

```
217:1  218:0  219:0  220:0  222:1  223:1  224:1  226:1  227:1  228:1  229:1  230:1
232:1  233:1  234:1  235:1  236:1  237:1  239:1  240:1  241:1  242:1  243:1  244:1
245:1  246:1  247:1  248:1  249:1  250:1  251:1  252:1  253:1  254:1  255:1  256:1
257:1  258:1  259:1  260:1  269:1  270:1  271:1
```

The added set is large because the [P3-T4] `try` wrap re-indented the whole method body, so every line of it is an added line in the diff even where the statement is unchanged.

The three uncovered lines, 218, 219 and 220, are the brace pair and body of `if (_globals?.Ol is not null)`. They are pre-existing uncovered code rather than a regression introduced here. The same three statements sit at lines 216, 217 and 218 on `origin/main`, and `coverage/810-baseline.cobertura.xml` reports 0 hits for all three at those numbers. The re-indent moved already-uncovered lines into the changed set; it did not stop covering anything. No test in the suite constructs a controller whose `_globals.Ol` is non-null.

### `QuickFiler/Controllers/QfcHomeController.cs`

Added lines: 390, 391. Measurable: 2. Covered: 2.

```
390:1  391:1
```

These are the two AC3 field nullings.

### `QuickFiler/Controllers/QfcItemController.EventHandlers.cs`

Added lines: none. [P5-T1] only deleted lines, so this file contributes nothing to the changed-line denominator.

### `QuickFiler/Viewers/BreadcrumbDropDownHost.cs`

Added lines: none. Its [P4-T4] edit was subsequently moved out by the [P4-T8] relocation, so relative to `origin/main` this file has deletions only.

### `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs`

Added lines: 103 through 108 and 132 through 176. Measurable: 41. Covered: 41.

```
134:1  135:1  136:1  137:1  138:1  139:1  140:1  141:1  142:1  143:1  144:1  145:1
146:1  147:1  148:1  149:1  150:1  151:1  152:1  153:1  154:1  155:1  156:1  157:1
158:1  159:1  160:1  161:1  162:1  165:1  166:1  167:1  168:1  169:1  170:1  171:1
172:1  173:1  174:1  175:1  176:1
```

Lines 103 through 108 are the rewritten `///` doc lines of [P4-T5] and emit no `line` node. Lines 132 through 176 are the two members the [P4-T8] relocation moved in, including the AC5 latch clear, and every measurable line of both is covered.

### `QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs`

Added lines: 1 through 61, the whole new file. Measurable: 9. Covered: 9.

```
23:1  24:1  43:1  44:1  45:1  46:1  49:1  50:1  59:1
```

The 52 non-measurable added lines are the `#nullable enable` directive, the using directives, the namespace and type declarations, the XML documentation blocks and the brace lines, none of which emit a `line` node. The nine measurable lines are the field initialiser, the `Register` guard and its two branches, and the `AnyOpen` derivation, and all nine are covered by the six cases [P6-T8] ran green.

### `QuickFiler/Viewers/QfcFormViewer.cs`

Added lines: 12, 210, 211, 212, 213, 214, 227, 228, 239. Measurable: 0. Recorded as `NOT MEASURABLE` for the reason given above. These are the `using QuickFiler.Viewers;` directive, the replaced field declaration with its comment, the forwarding `Register` call and the forwarding `AnyOpen` read.

## A note on the denominator

`QuickFiler/QuickFiler.csproj` also appears in the `git diff --unified=0 origin/main -- QuickFiler` output with one added line, 417, being the `<Compile Include>` item added by [P6-T5]. It is excluded from this calculation because it is a project file rather than production `.cs` source and carries no coverage measurement of any kind.
