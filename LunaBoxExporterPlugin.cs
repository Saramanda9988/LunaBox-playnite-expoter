using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;

namespace LunaBox.PlayniteExporter
{
    public sealed class LunaBoxExporterPlugin : GenericPlugin
    {
        private static readonly Guid PluginId = Guid.Parse("3F3BC4E7-339D-4B7B-A7F7-F6B0F80D4DC7");
        private readonly ILogger logger;

        public override Guid Id => PluginId;

        public LunaBoxExporterPlugin(IPlayniteAPI api)
            : base(api)
        {
            logger = LogManager.GetLogger();
            Properties = new GenericPluginProperties
            {
                HasSettings = false
            };
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            yield return new MainMenuItem
            {
                MenuSection = "@",
                Description = GetText("LOCLunaBoxExporterMenuItem", "Export library for LunaBox"),
                Action = actionArgs => ExportLibrary()
            };
        }

        private void ExportLibrary()
        {
            var exportPath = SelectExportPath();
            if (string.IsNullOrWhiteSpace(exportPath))
            {
                return;
            }

            if (!string.Equals(System.IO.Path.GetExtension(exportPath), ".json", StringComparison.OrdinalIgnoreCase))
            {
                exportPath += ".json";
            }

            try
            {
                var exportedAt = DateTime.UtcNow;
                var games = PlayniteApi.Database.Games
                    .Where(game => game != null)
                    .Select(game => MapGame(game, exportedAt))
                    .OrderBy(game => game.Name, StringComparer.CurrentCultureIgnoreCase)
                    .ToList();

                var json = Serialization.ToJson(games, true);
                File.WriteAllText(exportPath, json, new UTF8Encoding(false));

                logger.Info($"Exported {games.Count} Playnite games to {exportPath}.");
                PlayniteApi.Dialogs.ShowMessage(
                    string.Format(
                        GetText("LOCLunaBoxExporterSuccessMessage", "Exported {0} games.\n\nFile: {1}"),
                        games.Count,
                        exportPath),
                    GetText("LOCLunaBoxExporterTitle", "LunaBox Exporter"));
            }
            catch (Exception exception)
            {
                logger.Error(exception, "Failed to export the Playnite library for LunaBox.");
                PlayniteApi.Dialogs.ShowErrorMessage(
                    string.Format(
                        GetText("LOCLunaBoxExporterErrorMessage", "Export failed: {0}"),
                        exception.Message),
                    GetText("LOCLunaBoxExporterTitle", "LunaBox Exporter"));
            }
        }

        private string SelectExportPath()
        {
            var dialog = new SaveFileDialog
            {
                Title = GetText("LOCLunaBoxExporterTitle", "LunaBox Exporter"),
                Filter = GetText("LOCLunaBoxExporterFileFilter", "LunaBox JSON|*.json"),
                DefaultExt = ".json",
                AddExtension = true,
                OverwritePrompt = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                FileName = $"LunaBox_Playnite_Export_{DateTime.Now:yyyyMMdd_HHmmss}.json"
            };

            var owner = PlayniteApi.Dialogs.GetCurrentAppWindow();
            var accepted = owner == null
                ? dialog.ShowDialog()
                : dialog.ShowDialog(owner);

            return accepted == true ? dialog.FileName : string.Empty;
        }

        private LunaBoxExportGame MapGame(Game game, DateTime exportedAt)
        {
            var sourceType = ResolveSourceType(game);
            var isSteam = string.Equals(sourceType, "steam", StringComparison.Ordinal);
            var playAction = ResolveFilePlayAction(game);
            var launchPath = ResolveLaunchPath(game, playAction);
            if (isSteam && string.IsNullOrWhiteSpace(launchPath))
            {
                launchPath = Clean(game.InstallDirectory);
            }

            return new LunaBoxExportGame
            {
                Id = game.Id.ToString(),
                Name = Clean(game.Name),
                CoverUrl = ResolveCoverPath(game.CoverImage),
                CoverSourceUrl = ResolveCoverSourceUrl(game.CoverImage),
                Company = JoinNames(game.Developers),
                Summary = Clean(game.Description),
                Rating = ResolveRating(game),
                ReleaseDate = game.ReleaseDate?.Serialize() ?? string.Empty,
                Path = launchPath,
                GameDirectory = Clean(game.InstallDirectory),
                SavePath = null,
                ProcessName = ResolveProcessName(game, playAction),
                Status = ResolveStatus(game),
                SourceType = sourceType,
                SourceId = sourceType == "local" ? string.Empty : Clean(game.GameId),
                LaunchMode = isSteam ? "steam" : "normal",
                SteamLaunchId = isSteam ? Clean(game.GameId) : string.Empty,
                SteamLaunchKind = isSteam && !string.IsNullOrWhiteSpace(game.GameId) ? "native" : string.Empty,
                Tags = ResolveTags(game),
                CachedAt = exportedAt,
                CreatedAt = (game.Added ?? exportedAt).ToUniversalTime()
            };
        }

        private GameAction ResolveFilePlayAction(Game game)
        {
            var action = game.GameActions?
                .FirstOrDefault(item => item != null && item.IsPlayAction && item.Type == GameActionType.File);
            if (action == null)
            {
                return null;
            }

            return PlayniteApi.ExpandGameVariables(game, action);
        }

        private static string ResolveLaunchPath(Game game, GameAction action)
        {
            var launchPath = Clean(action?.Path);
            if (launchPath.Length == 0 || System.IO.Path.IsPathRooted(launchPath))
            {
                return launchPath;
            }

            var workingDirectory = Clean(action?.WorkingDir);
            if (System.IO.Path.IsPathRooted(workingDirectory))
            {
                return System.IO.Path.Combine(workingDirectory, launchPath);
            }

            var installDirectory = Clean(game.InstallDirectory);
            return installDirectory.Length == 0
                ? launchPath
                : System.IO.Path.Combine(installDirectory, launchPath);
        }

        private static string ResolveProcessName(Game game, GameAction action)
        {
            if (action == null || action.TrackingMode != TrackingMode.ProcessName)
            {
                return string.Empty;
            }

            var processName = Clean(action.TrackingPath);
            if (processName.Length == 0)
            {
                return string.Empty;
            }

            processName = System.IO.Path.GetFileName(processName);
            return System.IO.Path.HasExtension(processName) ? processName : processName + ".exe";
        }

        private string ResolveCoverPath(string coverImage)
        {
            coverImage = Clean(coverImage);
            if (coverImage.Length == 0)
            {
                return string.Empty;
            }

            if (IsHttpUrl(coverImage))
            {
                return coverImage;
            }

            return Clean(PlayniteApi.Database.GetFullFilePath(coverImage));
        }

        private static string ResolveCoverSourceUrl(string coverImage)
        {
            coverImage = Clean(coverImage);
            return IsHttpUrl(coverImage) ? coverImage : string.Empty;
        }

        private static double ResolveRating(Game game)
        {
            var score = game.UserScore ?? game.CommunityScore ?? game.CriticScore;
            return score.HasValue ? Math.Round(score.Value / 10.0, 1) : 0;
        }

        private string ResolveStatus(Game game)
        {
            var statusName = Clean(game.CompletionStatus?.Name).ToLowerInvariant();

            if (ContainsAny(statusName, "completed", "beaten", "finished", "已完成", "已通关", "通关", "クリア", "完了"))
            {
                return "completed";
            }

            if (ContainsAny(statusName, "on hold", "paused", "abandoned", "shelved", "搁置", "暂停", "放弃", "保留", "中断"))
            {
                return "on_hold";
            }

            if (ContainsAny(statusName, "plan to play", "want to play", "backlog", "计划游玩", "想玩", "待玩", "プレイ予定", "積み"))
            {
                return "want_to_play";
            }

            if (ContainsAny(statusName, "playing", "in progress", "currently playing", "游玩中", "正在玩", "进行中", "プレイ中"))
            {
                return "playing";
            }

            if (ContainsAny(statusName, "not played", "unplayed", "未开始", "未游玩", "未プレイ"))
            {
                return "not_started";
            }

            try
            {
                var completionSettings = PlayniteApi.ApplicationSettings?.CompletionStatus;
                if (completionSettings != null)
                {
                    if (game.CompletionStatusId == completionSettings.DefaultStatus)
                    {
                        return "not_started";
                    }

                    if (game.CompletionStatusId == completionSettings.PlayedStatus && game.Playtime > 0)
                    {
                        return "playing";
                    }
                }
            }
            catch (Exception exception)
            {
                logger.Debug(exception, $"Could not inspect completion status settings for {game.Name}.");
            }

            return game.Playtime > 0 ? "playing" : "not_started";
        }

        private static string ResolveSourceType(Game game)
        {
            var sourceName = Clean(game.Source?.Name).ToLowerInvariant();
            if (sourceName.Contains("steam"))
            {
                return "steam";
            }

            if (sourceName.Contains("bangumi"))
            {
                return "bangumi";
            }

            if (sourceName.Contains("vndb"))
            {
                return "vndb";
            }

            if (sourceName.Contains("ymgal"))
            {
                return "ymgal";
            }

            return "local";
        }

        private static List<string> ResolveTags(Game game)
        {
            return (game.Tags ?? new List<Tag>())
                .Where(tag => tag != null && !string.IsNullOrWhiteSpace(tag.Name))
                .Select(tag => tag.Name.Trim())
                .Distinct(StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }

        private static string JoinNames(IEnumerable<Company> companies)
        {
            if (companies == null)
            {
                return string.Empty;
            }

            return string.Join(
                ", ",
                companies
                    .Where(company => company != null && !string.IsNullOrWhiteSpace(company.Name))
                    .Select(company => company.Name.Trim())
                    .Distinct(StringComparer.CurrentCultureIgnoreCase));
        }

        private string GetText(string key, string fallback)
        {
            try
            {
                return PlayniteApi.Resources.GetString(key) ?? fallback;
            }
            catch
            {
                return fallback;
            }
        }

        private static bool ContainsAny(string value, params string[] candidates)
        {
            return candidates.Any(candidate => value.Contains(candidate));
        }

        private static bool IsHttpUrl(string value)
        {
            return Uri.TryCreate(value, UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        private static string Clean(string value)
        {
            return value?.Trim() ?? string.Empty;
        }
    }
}
