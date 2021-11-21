namespace System.Threading.Tasks
{
    public static class TaskFactoryExtensions
    {
        public static void StartNewAfterDelay(this TaskFactory self, TimeSpan duration, Action action)
        {
            self.StartNew(() =>
            {
                Thread.Sleep(duration);
                action();
            });
        }
    }
}
