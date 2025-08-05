using System.Collections.Generic;
using Blast.Core.Interfaces;
using Blast.Core.Objects;
using Blast.Core.Results;

namespace WindowWalker.Fluent.Plugin
{
    public class WindowWalkerSearchResult : SearchResultBase
    {
        public string WindowTitle { get; }
        public string ProcessName { get; }
        public int ProcessId { get; }
        public IntPtr WindowHandle { get; }

        public WindowWalkerSearchResult(
            string appName,
            string resultName,
            string windowTitle,
            string processName,
            int pid,
            IntPtr handle,
            string searchedText,
            double score,
            IList<ISearchOperation> operations) : base(
                appName,
                resultName,
                searchedText,
                "Window",
                score,
                operations)
        {
            WindowTitle = windowTitle;
            ProcessName = processName;
            ProcessId = pid;
            WindowHandle = handle;
        }

        protected override void OnSelectedSearchResultChanged()
        {
            // Optional: highlight or preview
        }
    }
}
