using System.Collections.Concurrent;
using System.Threading;
using RequestReduce.Filter;

namespace RequestReduce.Reducer
{
    public class ReducingQueue : IReducingQueue
    {
        protected readonly IReducer reducer;
        protected readonly IReductionRepository reductionRepository;
        protected ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
        protected Thread backgroundThread;
        protected bool isRunning = true;

        public ReducingQueue(IReducer reducer, IReductionRepository reductionRepository)
        {
            this.reducer = reducer;
            this.reductionRepository = reductionRepository;
            backgroundThread = new Thread(new ThreadStart(ProcessQueue)) { IsBackground = true };
            backgroundThread.Start();
        }

        public void Enqueue(string urls)
        {
            queue.Enqueue(urls);
        }

        private void ProcessQueue()
        {
            while(isRunning)
            {
                ProcessQueuedItem();
                Thread.Sleep(100);
            }
        }

        protected void ProcessQueuedItem()
        {
            string urlsToReduce = null;
            if (queue.TryDequeue(out urlsToReduce) && reductionRepository.FindReduction(urlsToReduce) == null)
            {
                var reducedUrl = reducer.Process(urlsToReduce);
                reductionRepository.AddReduction(urlsToReduce, reducedUrl);
            }
        }
    }
}
