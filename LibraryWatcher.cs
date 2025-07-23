using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LibraryWatcher
{
    public class LibraryWatcher : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private LibraryWatcherSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("2ad54430-40aa-4230-b0be-1e1d00322f16");

        public LibraryWatcher(IPlayniteAPI api) : base(api)
        {
            settings = new LibraryWatcherSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            PlayniteApi.Database.Games.ItemCollectionChanged += (_, args) =>
            {
                ExportLibraryToJson();
            };
        }

        public override void OnGameInstalled(OnGameInstalledEventArgs args)
        {
            // Add code to be executed when game is finished installing.
        }

        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            // Add code to be executed when game is started running.
        }

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            // Add code to be executed when game is preparing to be started.
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            // Add code to be executed when game is preparing to be started.
        }

        public override void OnGameUninstalled(OnGameUninstalledEventArgs args)
        {
            // Add code to be executed when game is uninstalled.
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            // Add code to be executed when Playnite is initialized.
            ExportLibraryToJson();
        }

        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
            // Add code to be executed when Playnite is shutting down.
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            ExportLibraryToJson();
        }

        private void ExportLibraryToJson()
        {
            try
            {
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var filePath = Path.Combine(documentsPath, "playnite-raycast-library.json");
                
                var sb = new StringBuilder();
                sb.AppendLine("[");
                
                var games = PlayniteApi.Database.Games.Where(game => game.IsInstalled).ToList();
                for (int i = 0; i < games.Count; i++)
                {
                    var game = games[i];
                    sb.AppendLine("  {");
                    sb.AppendLine($"    \"Id\": \"{EscapeJson(game.Id.ToString())}\",");
                    sb.AppendLine($"    \"Name\": \"{EscapeJson(game.Name)}\",");
                    sb.AppendLine($"    \"Source\": \"{EscapeJson(game.Source?.Name)}\",");
                    sb.AppendLine($"    \"Platform\": \"{EscapeJson(game.Platforms?.FirstOrDefault()?.Name)}\",");
                    sb.AppendLine($"    \"ReleaseDate\": \"{EscapeJson(game.ReleaseDate?.ToString())}\",");
                    sb.AppendLine($"    \"Added\": \"{EscapeJson(game.Added?.ToString())}\",");
                    sb.AppendLine($"    \"LastActivity\": \"{EscapeJson(game.LastActivity?.ToString())}\",");
                    sb.AppendLine($"    \"Playtime\": {game.Playtime},");
                    sb.AppendLine($"    \"InstallDirectory\": \"{EscapeJson(game.InstallDirectory)}\",");
                    sb.AppendLine($"    \"IsInstalled\": {game.IsInstalled.ToString().ToLower()},");
                    sb.AppendLine($"    \"Hidden\": {game.Hidden.ToString().ToLower()},");
                    sb.AppendLine($"    \"Favorite\": {game.Favorite.ToString().ToLower()},");
                    sb.AppendLine($"    \"CompletionStatus\": \"{EscapeJson(game.CompletionStatus?.Name)}\",");
                    sb.AppendLine($"    \"UserScore\": {game.UserScore?.ToString() ?? "null"},");
                    sb.AppendLine($"    \"CriticScore\": {game.CriticScore?.ToString() ?? "null"},");
                    sb.AppendLine($"    \"CommunityScore\": {game.CommunityScore?.ToString() ?? "null"},");
                    sb.AppendLine($"    \"Genres\": [{string.Join(", ", game.Genres?.Select(g => $"\"{EscapeJson(g.Name)}\"") ?? new string[0])}],");
                    sb.AppendLine($"    \"Developers\": [{string.Join(", ", game.Developers?.Select(d => $"\"{EscapeJson(d.Name)}\"") ?? new string[0])}],");
                    sb.AppendLine($"    \"Publishers\": [{string.Join(", ", game.Publishers?.Select(p => $"\"{EscapeJson(p.Name)}\"") ?? new string[0])}],");
                    sb.AppendLine($"    \"Categories\": [{string.Join(", ", game.Categories?.Select(c => $"\"{EscapeJson(c.Name)}\"") ?? new string[0])}],");
                    sb.AppendLine($"    \"Tags\": [{string.Join(", ", game.Tags?.Select(t => $"\"{EscapeJson(t.Name)}\"") ?? new string[0])}],");
                    sb.AppendLine($"    \"Features\": [{string.Join(", ", game.Features?.Select(f => $"\"{EscapeJson(f.Name)}\"") ?? new string[0])}],");
                    sb.AppendLine($"    \"Description\": \"{EscapeJson(game.Description)}\",");
                    sb.AppendLine($"    \"Notes\": \"{EscapeJson(game.Notes)}\",");
                    sb.AppendLine($"    \"Manual\": \"{EscapeJson(game.Manual)}\",");
                    sb.AppendLine($"    \"CoverImage\": \"{EscapeJson(game.CoverImage)}\",");
                    sb.AppendLine($"    \"BackgroundImage\": \"{EscapeJson(game.BackgroundImage)}\",");
                    sb.AppendLine($"    \"Icon\": \"{EscapeJson(game.Icon)}\"");
                    sb.AppendLine(i < games.Count - 1 ? "  }," : "  }");
                }
                
                sb.AppendLine("]");
                
                File.WriteAllText(filePath, sb.ToString());
                
                logger.Info($"Exported {games.Count} games to {filePath}");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to export library to JSON");
            }
        }

        private string EscapeJson(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            
            return input.Replace("\\", "\\\\")
                       .Replace("\"", "\\\"")
                       .Replace("\n", "\\n")
                       .Replace("\r", "\\r")
                       .Replace("\t", "\\t");
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new LibraryWatcherSettingsView();
        }
    }
}