using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blast.Core;
using Blast.Core.Interfaces;
using Blast.Core.Objects;
using Blast.Core.Results;
// Add Logging
using Microsoft.Extensions.Logging;

namespace WindowWalker.Fluent.Plugin
{
    public class WindowWalkerSearchApp : ISearchApplication
    {
        private const string AppName = "WindowWalker";
        private const string SearchTagName = "win"; // Define tag name as constant
        private readonly List<ISearchOperation> _operations;
        private readonly SearchApplicationInfo _appInfo;
        // Add Logger field
        private readonly ILogger<WindowWalkerSearchApp> _logger;

        // Modify constructor to accept ILogger
        public WindowWalkerSearchApp(ILogger<WindowWalkerSearchApp> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _operations = new List<ISearchOperation>
            {
                new FocusWindowOperation()
            };
            _appInfo = new SearchApplicationInfo(AppName, "Search and switch to open windows", _operations)
            {
                MinimumSearchLength = 1,
                IsProcessSearchEnabled = false,
                ApplicationIconGlyph = "\uE791",
                SearchAllTime = ApplicationSearchTime.Fast,
                SearchTags = new List<SearchTag>
                {
                    new SearchTag
                    {
                        Name = SearchTagName, // Use constant
                        IconGlyph = "\uE791",
                        Description = "Search open windows"
                    }
                }
            };
        }

        public ValueTask LoadSearchApplicationAsync() => ValueTask.CompletedTask;

        public SearchApplicationInfo GetApplicationInfo() => _appInfo;

        public async IAsyncEnumerable<ISearchResult> SearchAsync(SearchRequest searchRequest, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (searchRequest.SearchType == SearchType.SearchProcess)
            {
                _logger.LogDebug("Search type is SearchProcess, skipping window search.");
                yield break;
            }

            var query = searchRequest.SearchedText?.Trim() ?? ""; // Removed ToLower()
            var tag = searchRequest.SearchedTag; // Removed ToLower()

            // Only trigger if tag is 'win' or no tag and query is long enough
            if (!string.IsNullOrEmpty(tag) && !tag.Equals(SearchTagName, StringComparison.OrdinalIgnoreCase)) // Use constant and OrdinalIgnoreCase
            {
                _logger.LogDebug("Tag '{Tag}' is not '{ExpectedTag}', skipping window search.", tag, SearchTagName);
                yield break;
            }

            _logger.LogDebug("Starting window enumeration for query '{Query}' and tag '{Tag}'.", query, tag);

            var windows = WindowEnumerator.GetOpenWindows(_logger); // Pass logger

            foreach (var win in windows)
            {
                cancellationToken.ThrowIfCancellationRequested(); // Check for cancellation

                if (!string.IsNullOrEmpty(query))
                {
                    // Use OrdinalIgnoreCase for performance and correctness
                    if (!win.Title.Contains(query, StringComparison.OrdinalIgnoreCase) &&
                        !win.ProcessName.Contains(query, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }

                // Improved scoring using OrdinalIgnoreCase
                double score = 1.0;
                if (win.Title.StartsWith(query, StringComparison.OrdinalIgnoreCase))
                {
                    score += 0.5;
                }
                else if (win.ProcessName.StartsWith(query, StringComparison.OrdinalIgnoreCase))
                {
                    score += 0.25; // Slightly boost process name starts-with
                }

                yield return new WindowWalkerSearchResult(
                    AppName,
                    win.Title,
                    win.Title,
                    win.ProcessName,
                    win.Pid,
                    win.Handle,
                    searchRequest.SearchedText,
                    score,
                    _operations
                );
            }
            _logger.LogDebug("Window enumeration and search completed.");
        }

        public ValueTask<ISearchResult> GetSearchResultForId(object searchObjectId)
        {
            // Optional: support for custom tags (bookmarks)
            return new ValueTask<ISearchResult>((ISearchResult)null); // Explicitly return null
        }

        public ValueTask<IHandleResult> HandleSearchResult(ISearchResult searchResult)
        {
            if (searchResult is not WindowWalkerSearchResult windowResult)
            {
                _logger.LogWarning("HandleSearchResult called with incorrect result type: {Type}", searchResult?.GetType().Name ?? "null");
                return new ValueTask<IHandleResult>(new HandleResult(false, false));
            }

            try
            {
                _logger.LogDebug("Attempting to set foreground window for '{Title}' (PID: {Pid}, Handle: {Handle})", windowResult.WindowTitle, windowResult.ProcessId, windowResult.WindowHandle);
                bool success = Win32Api.SetForegroundWindow(windowResult.WindowHandle);
                if (success)
                {
                    _logger.LogInformation("Successfully set foreground window for '{Title}' (PID: {Pid})", windowResult.WindowTitle, windowResult.ProcessId);
                }
                else
                {
                    // GetLastError might provide more info, but requires more P/Invoke
                    _logger.LogWarning("SetForegroundWindow failed for '{Title}' (PID: {Pid}, Handle: {Handle})", windowResult.WindowTitle, windowResult.ProcessId, windowResult.WindowHandle);
                }
                return new ValueTask<IHandleResult>(new HandleResult(success, false));
            }
            catch (Exception ex) // Catch more specific exceptions if known from Win32 API
            {
                _logger.LogError(ex, "Exception occurred while setting foreground window for '{Title}' (PID: {Pid}, Handle: {Handle})", windowResult.WindowTitle, windowResult.ProcessId, windowResult.WindowHandle);
                return new ValueTask<IHandleResult>(new HandleResult(false, false));
            }
        }
    }
}
