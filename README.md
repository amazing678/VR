from ds_python_interpreter import call_ds_python_interpreter

# 定义 README 内容
readme_content = """# DPVR 虚拟现实交互项目 (SDK 迁移版)

## 项目概述
本项目是一个基于 Unity 开发的 VR 交互系统，专为 **大朋VR (DPVR)** 硬件优化。项目核心功能涵盖了设备远程绑定、中控指令同步、全景视频播放以及多种场景下的 VR 交互逻辑（如森林游览、零件拼装、物体收集等）。

项目已完成从旧版 **NXR (Nibiru) SDK** 到现代 **DPVR SDK (基于 XR Interaction Toolkit/OpenXR)** 的架构迁移，显著提升了交互的标准化与硬件适配度。

## 技术栈
* **引擎版本**: Unity 2019.4+ 
* **VR 架构**: DPVR SDK / XR Interaction Toolkit (OpenXR 兼容)
* **核心插件**:
    * **AVPro Video**: 用于高性能全景视频播放及双眼视差补偿。
    * **DG.Tweening (DoTween)**: 处理 UI 渐变、物体缩放及平滑移动。
    * **SWS (Spline Move)**: 用于处理森林与松鼠场景的路径点运动。
    * **Newtonsoft.Json**: 负责 WebSocket 通信协议的解析。

## 核心模块索引

### 1. 基础架构与生命周期管理
* **`VRMain.cs`**: 全局核心管理器，负责单例维护、跨场景不销毁逻辑及全局场景切换调度（如从主控界面跳转至各个训练关卡）。
* **`XRSceneInitializer.cs`**: 场景初始化组件，负责将跨场景的 `XROrigin` 动态注入旧有业务逻辑，并重置相机挂载关系。
* **`XRHeadPoseSync.cs`**: 针对 DPVR 硬件的姿态同步方案，确保视角随头显实时更新，解决视角死锁问题。

### 2. 通信与业务逻辑
* **`WebSocketManager.cs`**: 建立双工长连接，接收后端指令（如播放、暂停、跳转、解绑等），支持心跳机制。
* **`LogicManager.cs`**: 业务状态机，处理 6 位设备绑定码、轮询用户在线状态及跨场景状态保留。
* **`VRVideoOffsetManager.cs`**: 视频同步专家，处理双眼视频流的时间异步补偿（默认左眼提前 86ms），确保 VR 视差一致性。

### 3. 交互系统
* **手柄射线交互**:
    * `AutoFollowRay.cs`: 物体随手柄射线自动悬浮跟随的交互逻辑。
    * `VRMedium.cs`: 零件拼装逻辑，支持插槽吸附与自动化对齐。
* **凝视交互 (Gaze)**:
    * `VRGazeRaycaster.cs`: 从相机中心发射射线，支持对注视目标的检测与反馈。
    * `VRGazeButton.cs`: 具备进度环反馈的注视触发按钮。
* **VR 虚拟键盘**:
    * `VRKeys-Scripts/`: 完整的空间打字系统，支持多种语言布局与手柄敲击反馈。

### 4. 场景业务逻辑
* **森林游览 (`VRForestPlayer`)**: 处理自动/手动路径漫游、宝箱生成与进度控制。
* **生物交互 (`VRShongShu` / `VRBeiKe`)**: 包含松鼠受惊逃跑逻辑、贝壳注视收集动画及路径点切换。
* **热气球 (`VRBallonPlayer`)**: 复杂的空间目标生成系统，支持注视点位同步。

## 迁移要点记录
1.  **设备标识**: 移除了 NXR 的 `GetMacAddress()` 接口，统一采用原生 `SystemInfo.deviceUniqueIdentifier`。
2.  **相机适配**: 废弃了旧版 `NvrHead`，改为由 `XRSceneInitializer` 动态将 `XR Origin` 的相机注入 `VRPlayer.instance.RayCamera`。
3.  **层级管理**: 射线检测从 `Default` 层迁移至特定的交互层（如 `TT`、`SongShu` 等），并使用 `Ignore Raycast` 层防止遮挡。

## 部署与运行
* **环境要求**: 必须预装 `XR Interaction Toolkit` 插件，并配置好 DPVR 运行环境。
* **启动方式**: 建议从 `LoginScene` 或 `Main` 场景启动以初始化全局单例。
* **资源路径**: 视频资源需放置在 Android 设备的 `/sdcard/VRVideo/` 目录下。
"""

# 执行 Python 代码生成 Markdown 文件
code = f"""
with open('README.md', 'w', encoding='utf-8') as f:
    f.write({repr(readme_content)})
"""
call_ds_python_interpreter(code)
