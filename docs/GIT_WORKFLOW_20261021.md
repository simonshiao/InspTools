# DIA 檢測執行環境｜Git 使用說明

本機 Git 專案根目錄：

`E:\0_軟體開發區\3.開發中專案區\1.進行中\InspTools二次解譯`

## 版本追蹤邊界

- 追蹤：`src/`、`docs/`、`test/` 原始碼與文件、`Build.ps1`、`BuildDeliver.ps1`、`AGENTS.md`、`HANDOFF.md`。
- **不追蹤**：`output/`（約 883 MB）、`release/`（約 122 MB）、Recipe / Model / Training 執行副本、DLL/EXE/ZIP 和測試記錄。
- `release/current` 是既有正式 R1 套件，不屬於本次 Git 轉換範圍，不可用 Git 操作覆蓋。
- 目前僅建立 **本機 Git 版本庫**；尚未建立或推送 GitHub / GitLab 遠端。遠端位置需另行決定。
- Git 不包含已解碼 InspTools DLL 的全部 C# 來源；建置仍依賴本機既有、Hash pinned 的 R1 交付包。不可聲稱僅靠此 repository 就能從零重建所有 proprietary dependencies。

## 日常指令

```powershell
git status --short
git log --oneline -5
git diff
git add src docs test Build.ps1 BuildDeliver.ps1 HANDOFF.md README.md
git commit -m "說明變更內容"
```

如需複製到另一台機器，仍須依專案 Build 所記的 Hash Gate，另外取得已授權的二進位依賴；不要把模型、大型 Runtime 或生產 Recipe 推上公開 Git。
