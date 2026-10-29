# 原版按鈕隔離實驗｜2026-10-28

使用者要求載入 Recipe 後直接用原版圖示編輯。主線仍未完成，實驗禁止當正式結果。

在單獨 R3 候選中，以環境變數 INSPTOOLS_ORIGINAL_INPUT_EXPERIMENT=1、portable SHA pin 與 original-input-experiment.marker 同時啟用實驗；Recipe 為本機隔離複本。NativeR3RecipeSession 實驗分支呼叫 SetThroughputRecipe，並確認八站 paramFile.ThroughputFile 非 null。原方法可改寫 Setting.ini，僅隔離副本實驗。

output/standalone-r3/InspTools-Portable-R3-20261028-154452 實驗：GUI 載入 3923-R002 成功。保留原有裝飾元件命中屬性後，點定位圖示可開啟原版 LocateWindow，original-input-experiment.log 確認呼叫 Btn_LocateTool_Click；無影像時原廠以預覽模式開啟。另一個 153053 候選原版站別 1 切換到 2 成功。未做定位修改、保存、重新讀取。

同一 154452 實驗點二值化圖示，尚未開啟視窗，日誌未顯示回呼；文字工具反射名稱 Btn_FontTool_Click 無法找到，需檢查實際 R3 DLL metadata。其餘圖示、影像、AI 先知、模型、Inspect、編輯保存、退出／重啟和跨機均未完成；全介面可用不得宣稱 PASS。不要給使用者這個全部放行的實驗版作正式版。

先前將全部 UIElement 的 IsHitTestVisible 設 true 會改變原本裝飾層的命中屬性，導致點擊事件無法抵達。後續只恢復啟動器改動過的 ButtonBase，逐一找出二值化等的命中元素、回呼與依賴；以 GUI 真實點擊、真圖與隔離 Recipe 的修改→保存→重啟回讀驗證。限制與原生來源證據見 docs/RECIPE_DIALOG_RESET_20261028.md 和 docs/UI_BUTTON_DIAGNOSIS_20261022.md。
