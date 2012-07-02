﻿namespace Services.Processors
{
    using System.Collections.Generic;
    using System.Linq;

    using HtmlAgilityPack;

    using Services.Model;

    public class HtmlProcessor : IItemProcessor
    {
        private readonly string[] allowedTags = new[] { "p", "h1", "h2", "h3", "h4", "h5", "h6" };
        private readonly int limit;

        public HtmlProcessor(int limit = 250)
        {
            this.limit = limit;
        }

        public void Process(Item item)
        {
            if (!string.IsNullOrEmpty(item.Content))
            {
                item.Content = this.TrimContent(item.Content);
            }
        }

        private string TrimContent(string content)
        {
            var summary = new List<HtmlNode>();

            var html = new HtmlDocument();
            html.LoadHtml(content);

            var length = 0;
            var node = html.DocumentNode.FirstChild;

            if (!allowedTags.Contains(node.Name))
            {
                return content;
            }

            var loop = true;
            while (loop && node != null)
            {
                if (allowedTags.Contains(node.Name))
                {
                    length += node.InnerText.Length;
                    if (length >= limit)
                    {
                        node.ChildNodes.Append(HtmlNode.CreateNode("[...]"));
                        loop = false;
                    }

                    summary.Add(node);
                    node = node.NextSibling;
                }
                else if (node.NodeType == HtmlNodeType.Text)
                {
                    // skip it
                    node = node.NextSibling;
                }
                else
                {
                    summary.Last().ChildNodes.Append(HtmlNode.CreateNode("[...]"));
                    loop = false;
                }
            }

            return string.Concat(summary.Select(n => n.OuterHtml));
        }
    }
}