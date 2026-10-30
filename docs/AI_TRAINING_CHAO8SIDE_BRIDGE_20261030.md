# AI Training bridge｜2026-10-30

## Current implementation

The isolated portable R3 candidate `output\InspToolsV11` routes the original
compiled `ToolsWindow.btn_AITrain` to the Chao8Side rebuilt
`RebuiltTrainingWindow`. The bridge handles mouse-down before the original
button click and also handles Enter/Space, so it does not enter the protected
vendor `AITrainingWIndow` flow.

The rebuilt window and preview window are compiled from the Chao8Side host
sources. Runtime paths are pinned to the candidate AppBase by setting
`CHAO8SIDE_ROOT` and `DIA_PRODUCT_ROOT` in the candidate process before the
window opens. This prevents its runtime helpers from resolving to an installed
machine product.

## Runtime contract

The candidate carries the Chao8Side `DIA_COMPAT` runtime folders:

- `Runtime\TrainingPython\Python38`
- `Runtime\TrainingRebuild`
- `Runtime\CUDA11_118`
- `Runtime\AIRuntime`
- `Runtime\ModelConverter`

`Runtime\TrainingPython\GpuRuntime` is a candidate-local junction to
`Runtime\CUDA11_118`. The pinned backend files are recorded in
`training-runtime-manifest.json`. General DAVS training uses
`TrainingRebuild\train_compat.py` and the recovered DAVS modules packaged under
`ModelConverter\PyFrozenLib\DAVS`.

## Safety boundary

- `InspToolsV11` is a review candidate, not a production release.
- Training outputs, model conversion, and any deployment helper resolve to
  `InspToolsV11` because the bridge pins the process product-root variables.
- No production Recipe or active Model was copied into this candidate.
- No training run, PB conversion, model replacement, or rollback was executed
  while integrating the UI.
- The new window opens without a Recipe, matching the Chao8Side button route;
  its Recipe-specific defaults remain unavailable until a Recipe is loaded.

## Verification

- `scripts\Build.ps1 -Configuration Release -OutputSubdir ai-training-build-20261030` — PASS.
- `scripts\PackageTrainingReviewCandidate.ps1` — PASS; reused the previously
  verified R3 DLL SHA256 `ACD6DBCF266705108021126B9E91C605D87500F21608DA70946F5CE25789C34B`.
- `scripts\StageChao8SideTrainingRuntime.ps1` — PASS; pinned wrapper and DAVS
  bytecode hashes match the Chao8Side Runtime source.
- UI smoke: candidate starts as `INSPECTION TOOLS`, logs
  `bridge attached to btn_AITrain`, and pressing Space opens
  `DELTA AI Training Console`; the training agent reports `READY`.
