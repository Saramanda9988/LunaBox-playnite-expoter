# LunaBox Exporter for Playnite

这是一个面向 Playnite 10 的 C# 通用扩展。它从 Playnite 游戏库生成 LunaBox 可读取的 UTF-8 JSON 文件。

## 导出内容

每个游戏会包含名称、开发商、简介、评分、发售日期、封面、本地启动文件、安装目录、监控进程、完成状态、标签和来源标识。Steam 游戏还会包含 LunaBox 的 Steam 启动信息。

Playnite 仅保存累计游玩时间，因此此扩展不会构造虚假的单次游玩记录。

## 构建

```powershell
.\build.ps1
```

构建结果位于 `bin\Release`。开发时可以在 Playnite 的“设置 → 开发者 → 外部扩展”中添加该目录。

如需生成 `.pext` 安装包，请传入 Playnite 自带的 `Toolbox.exe`：

```powershell
.\build.ps1 -ToolboxPath "C:\Playnite\Toolbox.exe"
```

## 使用

在 Playnite 桌面模式中打开“扩展”，选择“为 LunaBox 导出游戏库”，并保存 JSON 文件。

随后打开 LunaBox 游戏库，在“添加游戏”菜单中选择“从 Playnite 导入”，选取刚才生成的 JSON 文件，检查预览后执行导入。

Playnite 通过商店扩展启动的非 Steam 游戏通常没有可供其他程序使用的本地启动文件。这类游戏仍会出现在导出文件中，并在 LunaBox 预览中显示为空启动文件。
