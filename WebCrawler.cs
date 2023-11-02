using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace WebCrawler
{
    class Program
    {
        // A set to store the visited URLs
        static HashSet<string> visited = new HashSet<string>();

        // A queue to store the URLs to be crawled
        static Queue<string> queue = new Queue<string>();

        // A lock object to synchronize the access to the shared data structures
        static object lockObj = new object();

        // The maximum number of concurrent tasks
        static int maxTasks = 10;

        // The base URL to crawl
        static string baseUrl = "https://example.com";

        // The HttpClient to send HTTP requests
        static HttpClient client = new HttpClient();

        static async Task Main(string[] args)
        {
            // Enqueue the base URL
            queue.Enqueue(baseUrl);

            // Create a list of tasks
            List<Task> tasks = new List<Task>();

            // Loop until the queue is empty or the maximum number of tasks is reached
            while (queue.Count > 0 && tasks.Count < maxTasks)
            {
                // Dequeue a URL from the queue
                string url = queue.Dequeue();

                // Create a task to crawl the URL and add it to the list
                Task task = CrawlAsync(url);
                tasks.Add(task);
            }

            // Wait for all the tasks to complete
            await Task.WhenAll(tasks);

            // Print the number of visited URLs
            Console.WriteLine($"Visited {visited.Count} URLs.");
        }

        static async Task CrawlAsync(string url)
        {
            try
            {
                // Send a GET request to the URL and get the response
                HttpResponseMessage response = await client.GetAsync(url);

                // Check if the response is successful
                if (response.IsSuccessStatusCode)
                {
                    // Get the response content as a string
                    string content = await response.Content.ReadAsStringAsync();

                    // Parse the HTML content using HtmlAgilityPack
                    HtmlDocument document = new HtmlDocument();
                    document.LoadHtml(content);

                    // Get all the anchor tags in the document
                    var links = document.DocumentNode.SelectNodes("//a[@href]");

                    // Loop through each link
                    foreach (var link in links)
                    {
                        // Get the href attribute value
                        string href = link.GetAttributeValue("href", "");

                        // Check if the href is not empty and starts with the base URL
                        if (!string.IsNullOrEmpty(href) && href.StartsWith(baseUrl))
                        {
                            // Normalize the URL by removing any query string or fragment
                            string normalizedUrl = href.Split(new char[] { '?', '#' })[0];

                            // Lock the shared data structures to avoid race conditions
                            lock (lockObj)
                            {
                                // Check if the URL has not been visited before
                                if (!visited.Contains(normalizedUrl))
                                {
                                    // Add the URL to the visited set
                                    visited.Add(normalizedUrl);

                                    // Enqueue the URL to the queue
                                    queue.Enqueue(normalizedUrl);

                                    // Print the URL for debugging purposes
                                    Console.WriteLine(normalizedUrl);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Print the exception message for debugging purposes
                Console.WriteLine(ex.Message);
            }
        }
    }
}