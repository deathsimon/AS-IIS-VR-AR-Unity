# AS-IIS-VR-AR-Unity

## 概觀

- 本專案為RoomFusion主要執行程式
- 須配合C extension來執行，專案為：[AS-IIS-VR-AR-DLL](https://github.com/lctseng/AS-IIS-VR-AR-DLL)
- 資料夾結構(僅列出必要的資料夾)
  - AS-IIS-VR-AR-DLL
    - lib
    - Debug
    - Release
  - AS-IIS-VR-AR-Unity
    - Assets
    - Build (此為輸出資料夾，Unity建置時應把RoomFusion.exe放在此目錄內)
    - ProjectSettings
  - dependencies
    - eigen
      - Eigen (本資料夾內應包含Array、Core等檔案)
    - lz4
      - include (本資料夾內應包含lz4.h等檔案)
      - lib (本資料夾內應包含lz4.lib等檔案)
    - opencv
      - include
        - opencv (本資料夾內應包含cv.hpp等檔案)
        - opencv2 (本資料夾內應包含core.hpp等檔案)
      - x64
        - vc12
          - lib (本資料夾內應包含opencv_world310等檔案)
    - pthread
      - include (本資料夾內應包含pthread.h等檔案)
      - lib (本資料夾內應包含pthreadVC2.lib等檔案)
- dependencies資料夾內容可以在[這裡下載](https://drive.google.com/file/d/0B6KmSTnbOf-CYkxaSy1oSzN1Z28/view?usp=sharing)


## 執行環境

- Unity版本：5.4.0
- 需在x64 的Windows上執行
- 需使用Direct3D 11做為Rendering Engine
- 需安裝Oculus SDK 1.7
- 需安裝Oculus Runtime 
- 在`Assets\Plugins\dll_windows`下放置執行需要的.dll
  - 可以從[這裡下載](https://drive.google.com/file/d/0B6KmSTnbOf-CY0lRRDRaWDdDNmc/view?usp=sharing)

## 執行方法

- 在執行Unity程式或Unity編輯器前，請先編譯C extension
  - 編譯後產生的`RoomFusionDLL.dll`請放在`Assets\Plugins\dll_windows`底下
  - 切勿在開著Unity編輯器或者遊戲時編譯C extension，會導致.dll檔案無法更新
  - 若不幸更新失敗，可以在關掉程式後手動複製，或者任意修改.dll原始碼後重新編譯(會觸發自動複製)
- 直接進入play mode，或者build之後執行輸出的exe程式
- 注意，本程式在編輯器中有機率Crash，建議先build之後再來測試(雖然還是有機率Crash)

## 遊戲操作方式

- 剛啟動程式時，Oculus可能會要求先戴上HMD中確認一些訊息，處理完後才會進入主程式
- `空白鍵`：顯示/隱藏觀察者房間
  - 有時候想檢驗遠端Server是否有把遠端房間的影像正確傳輸過來時，可以用這個功能
  - 此功能會隱藏所有觀察者房間的影像
- `N`：重新校正定位
  - 當觀察者正確站好並面向要取代的牆壁時，可以用此功能重新定位
  - 此功能會把遠端房間的正前方放在觀察者的正前方
- `M`：切換單眼、雙眼模式。預設是單眼模式。雙眼模式則有立體的效果。
- `Z、X、C、V`：角落定位
  - 程式中需要定位四邊形的四個點來進行深度取代修正
  - 這四個按鍵分別對應了左上、右上、左下、右下
  - 定位方式：
    - 按下想要校正的方位，此時會有一個對應顏色的球出現在畫面正中央
    - 點擊滑鼠利用游標改變定位球的位置，直到滿意為止
    - 注意滑鼠游標的位置與定位球並不同步，因此需要花一段時間適應下
    - 定位好某一個點之後，再按下一個定位按鍵來設定其他定位點
- `B`：設定取代的深度Threshold
  - 按下此鍵後，會有一個深度定位球出現，位置隨滑鼠游標改變
  - 定位球出現時會根據該點的深度設定threshold，並顯示在電腦螢幕上
  - 注意要重新定位深度時，不是點擊滑鼠，而是要重複按`B`鍵。這和角落定位不同

## 場景(Scene)物件介紹

- 此節將會介紹幾個在Unity Scene中的重要物件
- `TargetDepth`：定位深度threshold的球，若要不顯示，可以調整外觀
- `TargetLT、RT、LD、RD`：定位四邊形四個點的球，若要不顯示，可以調整外觀
- `World`：虛擬場景，可以讓觀察者房間與虛擬場景融合。 **目前不使用**
- `106 Room`：遠端房間的六面體影像
- `ObserverRegion`：觀察者所在區域，用來調整觀察者在世界座標的位置
  - `Tracking`：進行位置Tracking與深度Threshold的修正
  - `WorldCamera`：用來觀看世界景象的Camera
  - `LeftEyeAnchor`：左眼相機
    - `CornerDetect`：讓玩家決定四邊形的四個點
    - `CameraViewLeft`：左眼觀察者房間影像
      - `Back plane`：左眼影像背景
  - `RightEyeAnchor`：右眼相機
    - `CameraViewRight`：右眼觀察者房間影像
      - `Back plane`：右眼影像背景

## Script介紹

- 此節將會介紹放在`Assets\Scripts`下的幾份重要的C# Script
- `CameraTracking.cs`：利用ZED Tracking更新為置
- `CornerDetect.cs`：偵測四個角落定位點，並轉為螢幕座標
- `DepthCorrection.cs`：根據觀察者在房間內的移動，修正深度取代的threshold
- `FPSDisplay.cs`：在電腦螢幕上顯示FPS等其他資訊
- `FusedImageToTexture.cs`：把ZED的影像貼到Texture上，讓觀察者看到自己房間內的影像
- `RemoteRoomTexture.cs`：將遠端房間的影像從C extension複製到Unity中
- `RoomFusion.cs`：RoomFusionDLL.dll的進入點
- `Rotator.cs`：測試時，讓3D物件旋轉用的，目前用不到