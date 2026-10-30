# AI 定位隔離診斷 2026-10-29

- 對照 Chao8Side 的 AI Locate 模型、Runtime 和原版載入時序，使用 InspTools Build.ps1 與 BuildStandaloneR3 建立獨立候選，匯入 3923-R002 副本並透過 StageChao8SideAiParity 放入所需模型和依賴。
- 原版授權檔存在，實際載入隔離 Recipe 後 readiness log 顯示 S1 AI Locate Recipe=True、vendor_authorized=False、model=R002-CCD7DW；未執行原版模型推論或定位，不可視為通過。
- 授權是否過期、權限版本是否不同或其他驗證問題尚未確定。CheckAuthNoDLG 受原版驗證控制，不得偽造旗標。
- 已在 NativeR3RecipeSession 加入只記錄原版授權成功狀態和功能級別的診斷，不輸出授權檔內容；新候選已編譯及完成資產 staging，但尚未完成此新增診斷的 GUI 實測。
- 正式 release/current、原廠 Recipe、Chao8Side、ShuntVC 未改動。持續在隔離候選排查合法授權的原版檢查結果，之後再進行實圖定位驗證。

## 追加診斷
- 以隔離候選反射執行原版八站初始化和 CheckAuthNoDLG；新診斷實測：Vendor authorization accepted=False、feature tier=0，S1 Recipe=True、vendor_authorized=False。不是只因授權檔不存在；具體失敗原因仍未知。
- 找到 CHAO 自研 300E 離線替代方案：僅 CCD1，holdout IoU 0.777，production_qualified=false；不等同原廠 R002-CCD7DW 模型，不可當原廠定位已修復，也不可無驗證接入正式量測。
- 本輪未改原廠授權或真實檢測判定；AI Locate 完整修復仍未完成。
