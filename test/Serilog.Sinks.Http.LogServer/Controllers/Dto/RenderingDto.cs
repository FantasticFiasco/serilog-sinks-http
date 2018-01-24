namespace Serilog.Sinks.Http.LogServer.Controllers.Dto
{
    public class RenderingDto
    {
        public string Format { get; set; }

        public string Rendering { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is RenderingDto other))
                return false;

            return
                Format == other.Format &&
                Rendering == other.Rendering;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
