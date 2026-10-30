# InspTools 1.3.16.28 Editable Source

This is the canonical DEV-01 physical editable source tree.

- Frozen evidence source: `src-rawbaml/InspTools` under the decompile baseline.
- All recognized constant-decoder invocations are materialized as C# literals.
- Vendor BAML/resources are preserved for reproducible WPF builds.
- `TemplateRuntimeRoot` is an MSBuild property and may be overridden.
- Normal build/verify must preserve this tree. Re-materialization with `--clean` is a destructive reset to the accepted frozen reconstruction.

Do not replace the production InspTools.dll directly from this tree; use the project regression/release gates.
