using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using VersOne.Epub;
using HtmlAgilityPack;

namespace Bookix.Services
{
    public static class BookChunkService
    {
        // Returns number of chunks (paragraphs) for given file path
        public static async Task<int> CountChunksAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return 0;

            if (filePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                using var archive = ZipFile.OpenRead(filePath);
                var entry = archive.Entries.FirstOrDefault(e => e.FullName.EndsWith(".fb2", StringComparison.OrdinalIgnoreCase));
                if (entry != null)
                {
                    using var stream = entry.Open();
                    var list = ReadFb2FromStream(stream);
                    return list.Count;
                }
            }

            if (filePath.EndsWith(".epub", StringComparison.OrdinalIgnoreCase))
            {
                var list = await ReadEpubChunksAsync(filePath);
                return list.Count;
            }

            if (filePath.EndsWith(".fb2", StringComparison.OrdinalIgnoreCase))
            {
                using var stream = File.OpenRead(filePath);
                var list = ReadFb2FromStream(stream);
                return list.Count;
            }

            return 0;
        }

        private static async Task<List<string>> ReadEpubChunksAsync(string filePath)
        {
            EpubBook book = await EpubReader.ReadBookAsync(filePath);
            List<string> allParagraphs = new List<string>();

            foreach (EpubLocalTextContentFile textFile in book.ReadingOrder)
            {
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(textFile.Content);

                var nodes = htmlDoc.DocumentNode.SelectNodes("//p | //div | //span | //li");

                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        string text = node.InnerText.Trim();
                        if (!string.IsNullOrWhiteSpace(text) && text.Length > 3)
                        {
                            string cleanedText = System.Net.WebUtility.HtmlDecode(text);
                            allParagraphs.Add(cleanedText);
                        }
                    }
                }
                else
                {
                    var bodyText = htmlDoc.DocumentNode.InnerText;
                    var lines = bodyText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line)) allParagraphs.Add(line.Trim());
                    }
                }
            }
            return allParagraphs;
        }

        private static List<string> ReadFb2FromStream(Stream stream)
        {
            XDocument doc = XDocument.Load(stream);
            XNamespace fb2Ns = "http://www.gribuser.ru/xml/fictionbook/2.0";

            var textNodes = doc.Descendants()
                               .Where(n => n.Name == fb2Ns + "p" || n.Name == fb2Ns + "v")
                               .Select(n => n.Value.Trim())
                               .Where(t => !string.IsNullOrWhiteSpace(t))
                               .ToList();

            return textNodes;
        }
    }
}
