namespace NotFoundMiddlewareSample.Middleware
{
    public class NotFoundRequest
    {
        public NotFoundRequest(string path)
        {
            Path = path.ToLowerInvariant();
        }

        public NotFoundRequest()
        {
        }
        public string Path { get; private set; }
        public int Count { get; private set; }
        public string CorrectedPath { get; private set; }

        public void IncrementCount()
        {
            Count++;
        }

        public void SetCorrectedPath(string path)
        {
            CorrectedPath = path;
        }
    }
}