using System;
using System.IO;
using System.Threading.Tasks;
using team_viewer_manager.TeamViewer;

namespace team_viewer_manager {

    internal class Program {

        private static async Task Main(string[] args) {
            if (!tryGetAuthorizationToken(out var token)) {
                Console.Write("Please type the authorization token: ");
                token = Console.ReadLine().Trim();
            }

            try {
                var tvClient = new TeamViewerApiClient();
                if (!await tvClient.Authorize(token)) {
                    throw new Exception("Login failed");
                }
                ConsoleWriteLineSuccess("Login successfully");
            } catch (Exception ex) {
                ConsoleWriteLineError("Exception occured:");
                ConsoleWriteLineError(ex.ToString());
            }
        }

        private static bool tryGetAuthorizationToken(out string token) {
            if (File.Exists("authorization.token")) {
                token = File.ReadAllText("authorization.token");
                return !string.IsNullOrWhiteSpace(token);
            }
            token = null;
            return false;
        }

        #region console helpers

        private static void ConsoleWriteLineSuccess(string text) {
            ConsoleWriteLineColor(text, ConsoleColor.DarkGreen);
        }

        private static void ConsoleWriteLineError(string text) {
            ConsoleWriteLineColor(text, ConsoleColor.Red);
        }

        private static void ConsoleWriteLineColor(string text, ConsoleColor color) {
            var oldColor = Console.ForegroundColor;
            try {
                Console.ForegroundColor = color;
                Console.WriteLine(text);
            } finally {
                Console.ForegroundColor = oldColor;
            }
        }

        #endregion console helpers
    }
}