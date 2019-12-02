using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WebScanner.DAL;

namespace WebScanner.BLL
{
    public class UrlScanner: IUrlScanner
    {
        #region === INNER STATE ===

        private static UrlScanner instance;
        private ConcurrentQueue<UrlScan> queue;
        private CancellationTokenSource cancellationTokenSource;
        private ManualResetEvent manual;
        private object locker = new object();
        private volatile bool isPaused;
        private bool IsPaused
        {
            get
            {
                lock(locker)
                {
                    return isPaused;
                }
            }
            set
            {
                lock(locker)
                {
                    isPaused = value;
                }
            }
        }
        private Task[] tasks;
        private Regex regex;
        private string url;
        private string text;
        private int maxCountThreads = 1;
        private int maxCountUrls = 1;
        private volatile int urlsCounter;
        private bool isSetuped = false;

        #endregion

        #region === EXTERNAL STATE ===

        // Dependency injection by property
        public IRepository<UrlScan> Repository { get; set; }
        public int UrlsCounter { get { return urlsCounter; } }
        public int MaxCountUrls { get { return maxCountUrls; } }

        #endregion


        public static UrlScanner GetUrlScanner(bool createNew = false)
        {
            if (createNew)
                instance = new UrlScanner();

            if (instance == null)
                instance = new UrlScanner();

            return instance;
        }


        public void Setup(UrlScannerSource source)
        {
            if (source == null)
                throw new NullReferenceException(nameof(source));

            url = source.Url;
            text = source.Text;
            maxCountUrls = source.MaxCountUrls;
            maxCountThreads = source.MaxCountThreads;
            urlsCounter = 0;

            regex = new Regex(@"(?<url>https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}" +
                @"\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*))");

            Repository = new UrlScanRepository();
            queue = new ConcurrentQueue<UrlScan>();
            queue.Enqueue(new UrlScan(url));
            cancellationTokenSource = new CancellationTokenSource();
            manual = new ManualResetEvent(false);
            tasks = new Task[maxCountThreads];
            IsPaused = false;
            isSetuped = true;

            Repository.RemoveAll();        
        }

        public void Start()
        {
            if (!isSetuped)
                throw new InvalidOperationException("URLSCANNER_WAS_NOT_SETUPED_EXCEPTION");

            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = new Task((index) =>
                {
                    while (!cancellationTokenSource.IsCancellationRequested && urlsCounter < maxCountUrls)
                    {
                        Debug.WriteLine($"DoScan from task {index}\tUrlCounter {urlsCounter}");
                        DoScan();
                    }
                }, i, TaskCreationOptions.LongRunning);
                tasks[i].Start();
            }
        }

        public void Stop()
        {
            if (!isSetuped)
                throw new InvalidOperationException("URLSCANNER_WAS_NOT_SETUPED_EXCEPTION");

            cancellationTokenSource.Cancel();
        }

        public void Pause()
        {
            if (!isSetuped)
                throw new InvalidOperationException("URLSCANNER_WAS_NOT_SETUPED_EXCEPTION");

            IsPaused = true;
        }

        public void Resume()
        {
            if (!isSetuped)
                throw new InvalidOperationException("URLSCANNER_WAS_NOT_SETUPED_EXCEPTION");

            IsPaused = false;
            manual.Set();
            manual.Reset();
        }

        private void DoScan()
        {
            if (IsPaused) // pause before scanning
                manual.WaitOne();

            if (queue.IsEmpty)
                return;

            if (!queue.TryDequeue(out UrlScan model))
                return;

            model.DateStart = DateTime.Now;
            model.ScanStatus = ScanStatus.Loading;

            if (Repository.AddIfNotExists(model))
            {
                Interlocked.Increment(ref urlsCounter);

                string text = null;
                try
                {
                    if (IsPaused) 
                        manual.WaitOne(); // pause before loading webPage

                    text = FetchWebPage(model.Url);

                    if (IsPaused) // pause before search text
                        manual.WaitOne();

                    if (text.Contains(this.text))
                        model.ScanStatus = ScanStatus.Found;
                    else
                        model.ScanStatus = ScanStatus.NotFound;
                }
                catch (Exception ex)
                {
                    model.ScanStatus = ScanStatus.Failure;
                    model.FailureReason = ex.Message;
                }
                finally
                {
                    if (IsPaused) // pause before updating final status
                        manual.WaitOne();

                    model.DateEnd = DateTime.Now;
                    Repository.Update(model);
                }

                if (!string.IsNullOrWhiteSpace(text))
                { 
                    // add child urls to queue
                    GetChildUrls(text)
                        .ForEach(x => queue.Enqueue(new UrlScan(x, model.Url)));
                }
            }
        }

        private List<string> GetChildUrls(string text)
        {
            var matches = regex.Matches(text);
            var urls = new List<string>();

            foreach (Match match in matches)
                urls.Add(match.Groups["url"].Value);

            return urls;
        }

        private string FetchWebPage(string url)
        {
            using (var client = new WebClient())
            {
                return client.DownloadString(url);
            }
        }
    }
}
