using Blast.Core.Results;

namespace WindowWalker.Fluent.Plugin
{
    public class FocusWindowOperation : SearchOperationBase
    {
        public FocusWindowOperation() : base("Switch to Window", "Bring this window to front", "\uE791")
        {
        }
    }
}
