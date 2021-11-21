namespace Serilog.Sinks.HttpTests.LogServer.Services
{
    public class HealthService
    {
        private readonly object syncRoot = new();

        private bool isHealthy;

        public HealthService()
        {
            isHealthy = true;
        }

        public bool GetIsHealthy()
        {
            lock (syncRoot)
            {
                return isHealthy;
            }
        }

        public void SetIsHealthy(bool isHealthy)
        {
            lock (syncRoot)
            {
                this.isHealthy = isHealthy;
            }
        }
    }
}
