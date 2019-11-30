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
    //TODO how to pause tasks
    //TODO add error logging
    public class UrlScanner: IUrlScanner
    {
        private IRepository<UrlScan> repository;
        private ConcurrentQueue<UrlScan> queue;
        private CancellationTokenSource cancellationTokenSource;
        private Task[] tasks;
        private Regex regex;

        private string url;
        private string word;
        private int maxCountThreads;
        private int maxCountUrls;
        private volatile int urlsCounter;

        public int UrlsCounter => urlsCounter;
        public int MaxCountUrls => maxCountUrls;

        public UrlScanner(string url, string word, int maxCountUrls, int maxCountThreads)
        {
            this.url = url;
            this.word = word;
            this.maxCountUrls = maxCountUrls;
            this.maxCountThreads = maxCountThreads;

            regex = new Regex(@"(?<url>https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}" +
                @"\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*))");

            repository = new UrlScanRepository();
            queue = new ConcurrentQueue<UrlScan>();
            queue.Enqueue(new UrlScan(url));

            cancellationTokenSource = new CancellationTokenSource();
            tasks = new Task[maxCountThreads];

            repository.RemoveAll();
        }

        // ctor with DI
        public UrlScanner(string url, string word, int maxCountUrls, int maxCountThreads, IRepository<UrlScan> repository)
            :this(url,word,maxCountUrls,maxCountThreads)
        {
            this.repository = repository;
        }

        public Task Start()
        {
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = new Task((index) =>
                {
                    while (!cancellationTokenSource.IsCancellationRequested && urlsCounter < maxCountUrls)
                    {
                        Debug.WriteLine($"DoScan from task {index}\tUrlCounter {urlsCounter}");
                        DoScan();
                        Thread.Sleep(1000);
                    }
                }, i);
                tasks[i].Start();
            }

            return Task.CompletedTask;
        }

        public Task Stop()
        {
            cancellationTokenSource.Cancel();

            return Task.CompletedTask;
        }

        private void DoScan()
        {
            if (queue.IsEmpty)
                return;

            UrlScan model;
            if (!queue.TryDequeue(out model))
                return;

            model.DateStart = DateTime.Now;
            model.ScanStatus = ScanStatus.Loading;
            
            if (repository.AddIfNotExists(model))
            {
                Interlocked.Increment(ref urlsCounter);

                string text = null;
                try
                {
                    text = FetchWebPage(model.Url);

                    if (text.Contains(word))
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
                    model.DateEnd = DateTime.Now;
                    repository.Update(model);
                }

                if (!string.IsNullOrWhiteSpace(text))
                { 
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
