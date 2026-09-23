# LunaBox Playnite 导出器

[English](README.md) | **简体中文**

这是一个面向 Playnite 10 的 C# 通用扩展，用于将 Playnite 游戏库和封面图片导出为 LunaBox 支持的 ZIP 文件。

扩展以首个 Playnite 10 稳定版随附的 PlayniteSDK 6.4.0 为兼容基准，可由已经发布的全部 Playnite 10 版本加载。

## 导出内容

每个游戏可以包含名称、开发商、简介、评分、发售日期、封面、本地启动文件、安装目录、监控进程、完成状态、标签和来源标识。Steam 游戏还会包含 LunaBox 使用的 Steam 启动信息。

ZIP 中包含 `games.json` 和 `covers/` 下的本地封面图片。JSON 使用包内相对地址引用封面；导入时，LunaBox 将图片复制到自身的封面目录。启动文件和安装目录仍保留原有地址，跨电脑或系统使用时需要重新指定。旧版 JSON 文件仍可导入 LunaBox。

Playnite 仅保存累计游玩时间，因此此扩展不会构造虚假的单次游玩记录。

## 安装

1. 从 [GitHub 最新发行版](../../releases/latest) 下载 `.pext` 安装包。
2. 打开安装包，在 Playnite 中确认安装，然后重启 Playnite。

## 使用

1. 在 Playnite 桌面模式中打开“扩展”，选择“为 LunaBox 导出游戏库”。
2. 选择生成 ZIP 文件的保存位置。
3. 打开 LunaBox 游戏库，选择“添加游戏 → 从 Playnite 导入”。
4. 选择导出的 ZIP 文件，检查预览内容，然后开始导入。

## 本地构建

```powershell
.\build.ps1
```

构建结果位于 `bin\Release`。开发时可以在 Playnite 的“设置 → 开发者 → 外部扩展”中添加该目录。

如需生成 `.pext` 安装包，请传入 Playnite 自带的 `Toolbox.exe`：

```powershell
.\build.ps1 -ToolboxPath "C:\Playnite\Toolbox.exe"
```

## 使用限制

Playnite 通过商店扩展启动的非 Steam 游戏通常没有可供其他程序使用的本地启动文件。这类游戏仍会出现在 `games.json` 中，并在 LunaBox 导入预览中显示为空启动文件。
