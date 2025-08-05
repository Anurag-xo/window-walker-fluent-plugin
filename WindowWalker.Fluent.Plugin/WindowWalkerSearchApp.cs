using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Blast.Core;
using Blast.Core.Interfaces;
using Blast.Core.Objects;
using Blast.Core.Results;

namespace WindowWalker.Fluent.Plugin
{
    public class WindowWalkerSearchApp : ISearchApplication
    {
        private const string AppName = "WindowWalker";
        private readonly List<ISearchOperation> _operations;
        private readonly SearchApplicationInfo _appInfo;

        public WindowWalkerSearchApp()
        {
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
                    new SearchTag { Name = "win", IconGlyph = "\uE791", Description = "Search open windows" }
                }
            };
        }

        public ValueTask LoadSearchApplicationAsync() => ValueTask.CompletedTask;

        public SearchApplicationInfo GetApplicationInfo() => _appInfo;

        public async IAsyncEnumerable<ISearchResult> SearchAsync(SearchRequest searchRequest, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (searchRequest.SearchType == SearchType.SearchProcess)
                yield break;

            var query = searchRequest.SearchedText?.Trim().ToLower() ?? "";
            var tag = searchRequest.SearchedTag?.ToLower();

            // Only trigger if tag is 'win' or no tag and query is long enough
            if (!string.IsNullOrEmpty(tag) && tag != "win")
                yield break;

            var windows = WindowEnumerator.GetOpenWindows();

            foreach (var win in windows)
            {
                if (!string.IsNullOrEmpty(query))
                {
                    if (!win.Title.ToLower().Contains(query) && !win.ProcessName.ToLower().Contains(query))
                        continue;
                }

                double score = 1.0 + (win.Title.StartsWith(query, StringComparison.OrdinalIgnoreCase) ? 0.5 : 0);

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
        }

        public ValueTask<ISearchResult> GetSearchResultForId(object searchObjectId)
        {
            // Optional: support for custom tags (bookmarks)
            return new ValueTask<ISearchResult>();
        }

        public ValueTask<IHandleResult> HandleSearchResult(ISearchResult searchResult)
        {
            if (searchResult is not WindowWalkerSearchResult windowResult)
                return new ValueTask<IHandleResult>(new HandleResult(false, false));

            try
            {
                Win32Api.SetForegroundWindow(windowResult.WindowHandle);
                return new ValueTask<IHandleResult>(new HandleResult(true, false));
            }
            catch
            {
                return new ValueTask<IHandleResult>(new HandleResult(false, false));
            }
        }
    }
}
