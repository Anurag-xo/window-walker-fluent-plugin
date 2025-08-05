namespace WindowWalker.Fluent.Plugin
{
    public class WindowInfo
    {
        public string Title { get; set; }
        public string ProcessName { get; set; }
        public int Pid { get; set; }
        public IntPtr Handle { get; set; }
    }
}
