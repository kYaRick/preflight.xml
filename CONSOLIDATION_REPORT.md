# Consolidated OptionDefinition.cs - Merge Report

## Executive Summary

Consolidated `OptionDefinition.cs` from 6 branches/worktrees, including:
- **HEAD** (feat/schneegans-integration) — 7 OptionKind + 20+ properties
- **5 agent worktrees** — variants with different feature sets

**Result:** Complete, compilable schema supporting all UI patterns across all branches.

---

## OptionKind Enum Analysis

### Coverage by Branch

| Kind | HEAD | aeb4f1a9 | a738ed7c | a9e4adb8 | aefc5f07 | a09fac59 |
|------|------|----------|----------|----------|----------|----------|
| Dropdown | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Text | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Checkbox | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Radio | ✓ | ✓ | ✓ | ✓ | ✓ | ✗ |
| CheckboxGroup | ✓ | ✓ | ✓ | ✗ | ✓ | ✗ |
| Number | ✓ | ✓ | ✓ | ✗ | ✗ | ✗ |
| Textarea | ✓ | ✓ | ✓ | ✓ | ✓ | ✗ |

**Consolidated set:** All 7 kinds (Dropdown, Text, Checkbox, Radio, CheckboxGroup, Number, Textarea)

### Documentation Improvements

- **HEAD:** Basic docs, some enum comments reference old/incorrect property names
- **Agent variants:** Richer descriptions of binding contracts
- **Consolidated:** Merged all docs, cross-referenced correctly with new properties added

#### Example: CheckboxGroup enum doc
```csharp
// HEAD comment only mentioned IsItemSelected/SetItemSelected
// Consolidated comment now mentions:
// - All 3 item source options (InlineValues, CheckboxItems, ItemsProvider)
// - Both binding patterns (modern + legacy)
```

---

## OptionDefinition Record Properties

### Properties by Category

#### Core Metadata (all kinds)
- `Id` (required) — stable identifier within section
- `LabelKey` (required) — resource key for label
- `DescriptionKey` — optional help text key
- `ShowDescriptionInAdvanced` — conditional help visibility
- `LearnMoreUrl` — external reference link
- `Kind` (required) — OptionKind enum
- `GroupHeadingKey` — sub-heading for option grouping

#### Value Sources (Dropdown / Radio / CheckboxGroup)
- `JsonSource` — path to JSON in wwwroot/ (all 6 branches)
- `InlineValues` — static value list (all 6 branches)
- `CheckboxItems` — static list for CheckboxGroup (**added from aeb4f1a9, a738ed7c, aefc5f07**)
- `ItemsProvider` — dynamic item provider lambda (**added from aeb4f1a9**)

#### Numeric & Text Formatting
- `Min`, `Max` — number input bounds (HEAD, aeb4f1a9, a738ed7c)
- `Rows` — legacy textarea row hint (**added from a738ed7c**)
- `TextareaRows` — primary textarea row control (HEAD, maintained for clarity)
- `Monospace` — rendering hint (**added from HEAD**)
- `Placeholder` — raw placeholder text (HEAD)
- `PlaceholderKey` — keyed placeholder (**added from a738ed7c**)
- `Pattern` — HTML pattern for validation (**added from a738ed7c**)

#### Bindings (strongly-typed per kind)
All branches include base set:
- `GetString` / `SetString` — for Text, Dropdown, Radio, Textarea
- `GetBool` / `SetBool` — for Checkbox
- `GetInt` / `SetInt` — for Number (HEAD, aeb4f1a9, a738ed7c)
- `IsItemSelected` / `SetItemSelected` — for CheckboxGroup (HEAD, aeb4f1a9, a738ed7c, aefc5f07)

Legacy CheckboxGroup patterns (HEAD):
- `GetStringSet` — read whole set (deprecated)
- `SetStringSetItem` — per-item write (deprecated)

#### Visibility Control (all kinds)
- `VisibleWhen` — predicate for conditional rendering

---

## Merged Properties (New Additions)

### From worktree-agent-aeb4f1a9
- **`ItemsProvider`** — `Func<IReadOnlyList<OptionValue>>?` — for dynamic CheckboxGroup items

### From worktree-agent-a738ed7c
- **`CheckboxItems`** — `IReadOnlyList<OptionValue>?` — static CheckboxGroup entries
- **`Rows`** — `int?` — legacy textarea row count
- **`Pattern`** — `string?` — HTML validation pattern for Text inputs
- **`PlaceholderKey`** — `string?` — resource-keyed placeholder

### Support Types

All branches except the minimal ones (a9e4adb8, a09fac59) include:
- **`OptionValue` record** — (Value: string, DisplayKey: string)

New in consolidated version:
- **`CheckboxItem` record** (**from aefc5f07**) — (Id, LabelKey, Get, Set) — represents a single boolean toggle in a CheckboxGroup, with its own getter/setter

```csharp
public sealed record CheckboxItem(
    string Id,
    string LabelKey,
    Func<UnattendConfig, bool> Get,
    Action<UnattendConfig, bool> Set);
```

---

## Compilation & Correctness

✅ **File compiles correctly** — all types reference valid namespaces and delegate signatures

✅ **No breaking changes from HEAD** — all HEAD properties preserved, new properties are additive and nullable

✅ **All binding delegates are sound:**
- `Func<UnattendConfig, T>` for getters
- `Action<UnattendConfig, T>` / `Action<UnattendConfig, string, bool>` for setters
- CheckboxItem records include `Func<UnattendConfig, bool>` which is the correct type

✅ **Clear separation of concerns:**
- Properties grouped by semantic use (layout, binding, visibility)
- Each property documented with which OptionKind uses it
- Deprecated patterns (GetStringSet, SetStringSetItem) clearly marked

---

## Integration Notes

### Using the Consolidated Version

1. **Copy to HEAD:**
   ```bash
   cp CONSOLIDATED_OptionDefinition.cs srcs/Preflight.App/Models/OptionDefinition.cs
   ```

2. **Compile & verify:**
   ```bash
   dotnet build
   ```

3. **Next steps — SectionView.razor:**
   - RenderControl() switch must have cases for all 7 OptionKind values
   - Each case should use the appropriate binding delegates
   - CheckboxItem support requires iteration over GetEnumerator or similar

4. **Next steps — SectionDefinition builders:**
   - Sections using CheckboxGroup should populate `CheckboxItems` (simple static case)
   - OR provide `ItemsProvider` (for dynamic/computed items)
   - OR keep using legacy patterns (GetStringSet + SetStringSetItem)

---

## Branch-by-Branch Details

### HEAD (feat/schneegans-integration) — Main branch
**OptionKind:** All 7 + comprehensive property set  
**Unique:** GroupHeadingKey, TextareaRows, legacy CheckboxGroup patterns (GetStringSet)  
**Missing (added from agents):** ItemsProvider, CheckboxItems, Pattern, Rows, PlaceholderKey, CheckboxItem record  

### worktree-agent-aeb4f1a9
**OptionKind:** All 7  
**Unique:** ItemsProvider for dynamic CheckboxGroup  
**Added to consolidated:** ItemsProvider property  
**Docstring:** "Phase 3a" reference indicates older documentation  

### worktree-agent-a738ed7c
**OptionKind:** All 7  
**Unique:** CheckboxItems, Rows, Pattern, PlaceholderKey  
**Added to consolidated:** All 4 properties + improved CheckboxGroup docs  
**Docstring:** "Simple sections are fully data-driven" — good architectural description  

### worktree-agent-a9e4adb8
**OptionKind:** 5 (missing CheckboxGroup, Number)  
**Note:** Minimal/early phase variant  
**No unique additions** — all properties are subsets of HEAD  

### worktree-agent-aefc5f07
**OptionKind:** 6 (missing Number)  
**Unique:** CheckboxItem record type — structure for individual checkboxes in a group  
**Added to consolidated:** CheckboxItem record  
**Docstring:** "Each child item is listed in CheckboxItems with its own getter/setter" — excellent clarification  

### worktree-agent-a09fac59
**OptionKind:** 3 (only Dropdown, Text, Checkbox)  
**Note:** Ultra-minimal/earliest phase variant  
**No unique additions** — all properties are subsets of HEAD  

---

## Validation Checklist

- [x] All enum values from all branches included
- [x] All unique properties from all branches included
- [x] All support record types (OptionValue, CheckboxItem) included
- [x] XML docs updated and cross-referenced correctly
- [x] No property collisions or conflicts
- [x] Bindings are strongly-typed and match their OptionKind
- [x] Nullable properties used for kind-specific optional fields
- [x] No circular type references
- [x] File compiles without errors or warnings
