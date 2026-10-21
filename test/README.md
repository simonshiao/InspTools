# test

本案測試優先驗「機台 Host 使用體驗」：Host 只能 reference public SDK/WPF assembly，不得偷用 internal recovered type、CLI、Named Pipe 或開發機硬編碼路徑。

## Legacy Annotation Contract Audit

`audit_legacy_annotation_labels.py` 驗證 `TrainingImage/<Model>/X|Y/OK|NG` 的資料契約：X/Y pair、尺寸、Y/OK blank label，以及 Y/NG 的 class / ignore / maybe 色彩語意。

```powershell
& D:\python\Pyhon31210\python.exe .\test\audit_legacy_annotation_labels.py `
  --root "D:\0_軟體開發區\1.成品區\Shunt外觀機\Runtime\ShuntVC\TrainingImage" `
  --require-pairs
```

預設是 sample mode；需要全量 pixel scan 時加 `--full`。不要把 `--strict-palette` 當 production Gate：Legacy writer 會 Resize label mat，真實邊界存在正常 interpolation 色。

