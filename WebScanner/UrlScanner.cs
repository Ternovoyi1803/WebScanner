using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WebScanner.Models;
using WebScanner.Repositories;

namespace WebScanner
{
    //TODO add CancellationToken Stop
    //TODO how to pause tasks
    //TODO add scanned urls counter. Use Interlocked ? 
    //TODO add error logging
    public class UrlScanner
    {
        private static Semaphore semaphore;
        private UrlScanRepository repository;
        private Regex regex;

        private string url;
        private string word;
        private int maxCountUrls;
        private int maxCountThreads;

        public UrlScanner(string url, string word, int maxCountUrls, int maxCountThreads)
        {
            this.url = url;
            this.word = word;
            this.maxCountUrls = maxCountUrls;
            this.maxCountThreads = maxCountThreads;

            semaphore = new Semaphore(maxCountThreads, maxCountThreads);
            repository = new UrlScanRepository();
            regex = new Regex(@"(?<url>https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}" +
                @"\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*))");

            repository.RemoveAll();
        }

        public Task DoScan()
        {
            var startUrl = new UrlScan(url);
            var task = new Task(() => DoScan(startUrl));
            task.Start();

            return task;
        }

        private void DoScan(UrlScan urlScan)
        {
            try
            {
                semaphore.WaitOne();
                Console.WriteLine($"Thread: {Thread.CurrentThread.ManagedThreadId} starts...");

                urlScan.ScanStatus = ScanStatus.Loading;
                urlScan.DateStart = DateTime.Now;

                if (!repository.AddIfNotExists(urlScan))
                    return;

                string text = null;
                try
                {
                    text = FetchWebPage(urlScan.Url);

                    if (text.Contains(word))
                        urlScan.ScanStatus = ScanStatus.Found;
                    else
                        urlScan.ScanStatus = ScanStatus.NotFound;
                }
                catch (Exception ex)
                {
                    urlScan.ScanStatus = ScanStatus.Failure;
                    urlScan.FailureReason = ex.Message;
                }
                finally
                {
                    urlScan.DateEnd = DateTime.Now;
                    repository.Update(urlScan);
                }

                if (string.IsNullOrWhiteSpace(text))
                    return;

                // Поиск ссылок внутри текста не считаеться как процесс сканирования
                foreach (var url in GetChildUrls(text))
                {
                    var urlForScan = new UrlScan(url, urlScan.Url);
                    new Task(() => DoScan(urlForScan)).Start();
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.WriteLine($"Thread: {Thread.CurrentThread.ManagedThreadId} ends...");
                semaphore.Release();
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

        private async Task<string> FetchWebPageAsync(string url)
        {
            using (var client = new HttpClient())
            {
                return await client.GetStringAsync(url);
            }
        }
    }
}
