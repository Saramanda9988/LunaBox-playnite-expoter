using System;
using System.Collections.Generic;
using Playnite.SDK.Data;

namespace LunaBox.PlayniteExporter
{
    public sealed class LunaBoxExportGame
    {
        [SerializationPropertyName("id")]
        public string Id { get; set; }

        [SerializationPropertyName("name")]
        public string Name { get; set; }

        [SerializationPropertyName("cover_url")]
        public string CoverUrl { get; set; }

        [SerializationPropertyName("cover_source_url")]
        public string CoverSourceUrl { get; set; }

        [SerializationPropertyName("company")]
        public string Company { get; set; }

        [SerializationPropertyName("summary")]
        public string Summary { get; set; }

        [SerializationPropertyName("rating")]
        public double Rating { get; set; }

        [SerializationPropertyName("release_date")]
        public string ReleaseDate { get; set; }

        [SerializationPropertyName("path")]
        public string Path { get; set; }

        [SerializationPropertyName("game_directory")]
        public string GameDirectory { get; set; }

        [SerializationPropertyName("save_path")]
        public string SavePath { get; set; }

        [SerializationPropertyName("process_name")]
        public string ProcessName { get; set; }

        [SerializationPropertyName("status")]
        public string Status { get; set; }

        [SerializationPropertyName("source_type")]
        public string SourceType { get; set; }

        [SerializationPropertyName("source_id")]
        public string SourceId { get; set; }

        [SerializationPropertyName("launch_mode")]
        public string LaunchMode { get; set; }

        [SerializationPropertyName("steam_launch_id")]
        public string SteamLaunchId { get; set; }

        [SerializationPropertyName("steam_launch_kind")]
        public string SteamLaunchKind { get; set; }

        [SerializationPropertyName("tags")]
        public List<string> Tags { get; set; }

        [SerializationPropertyName("cached_at")]
        public DateTime CachedAt { get; set; }

        [SerializationPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
