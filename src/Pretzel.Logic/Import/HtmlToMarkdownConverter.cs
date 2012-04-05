﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using Pretzel.Logic.Extensions;

namespace Pretzel.Logic.Import
{
    /// <summary>
    /// This one uses the HTML agility pack
    /// </summary>
    public class HtmlToMarkdownConverter
    {
        public string Convert(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            StringBuilder markdown = new StringBuilder();
            doc.LoadHtml(html);
            ProcessNodes(markdown, doc.DocumentNode.ChildNodes);
            return markdown.ToString();
        }

        private static Regex regexBr = new Regex(@"\<br\s*/?\>",
            RegexOptions.IgnoreCase
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled);

        private int listNestingLevel;

        private void ProcessNodes(StringBuilder markdown, IEnumerable<HtmlNode> htmlNodes)
        {
            foreach (var htmlNode in htmlNodes)
            {
                switch (htmlNode.Name)
                {
                    case "#comment":
                        break;
                    case "#text":
                        markdown.Append(htmlNode.InnerText);
                        break;
                    case "h1":
                    case "h2":
                    case "h3":
                    case "h4":
                    case "h5":
                    case "h6":
                        string hashes = new string('#', htmlNode.Name[1] - '0');
                        markdown.AppendLine();
                        markdown.AppendFormat("{0} {1}", hashes, htmlNode.InnerText);
                        markdown.AppendLine();
                        break;
                    case "ul":
                        markdown.AppendLine();
                        listNestingLevel++;
                        ProcessNodes(markdown, htmlNode.ChildNodes);
                        listNestingLevel--;
                        if (listNestingLevel < 0) listNestingLevel = 0;
                        markdown.AppendLine();
                        break;
                    case "li":
                        // n.b. don't yet support nested lists:
                        markdown.AppendLine();
                        if (listNestingLevel == 0) // missing ul
                            listNestingLevel = 1;
                        markdown.AppendFormat("{0}* ", new string(' ', 4 * (listNestingLevel - 1)));
                        listNestingLevel++;
                        ProcessNodes(markdown, htmlNode.ChildNodes);
                        listNestingLevel--;
                        if (listNestingLevel < 0) listNestingLevel = 0;
                        break;
                    case "p":
                        markdown.AppendLine();
                        ProcessNodes(markdown, htmlNode.ChildNodes);
                        markdown.AppendLine();
                        break;
                    case "b":
                    case "strong":
                        var boldText = htmlNode.InnerText;
                        bool addSpace = false;
                        if (boldText.EndsWith(" "))
                        {
                            boldText = boldText.Substring(0, boldText.Length - 1);
                            addSpace = true;
                        }
                        markdown.AppendFormat("**{0}**{1}", htmlNode.InnerText, addSpace ? " " : "");
                        break;
                    case "i":
                    case "em":
                        markdown.AppendFormat("*{0}*", htmlNode.InnerText);
                        break;
                    case "br":
                        markdown.AppendLine();
                        break;
                    case "a":
                        markdown.AppendFormat("[{0}]({1})", htmlNode.InnerText, htmlNode.Attributes["href"].Value);
                        break;
                    case "img":
                    case "blockquote":
                        // leave html unchanged for now, maybe revisit later
                        markdown.Append(htmlNode.OuterHtml);
                        break;
                    case "object":
                    case "table":
                    case "div":
                    case "span":
                    case "iframe":
                    case "embed":
                        // leave html unchanged
                        markdown.Append(htmlNode.OuterHtml);
                        break;
                    case "pre":
                    case "code":
                        var code = htmlNode.InnerText;
                        // a bit hacky, but we need to sort out where lines of code end
                        code = code.Replace("\r\n", "\n");
                        code = code.Replace("\r", "\n");
                        code = regexBr.Replace(code, "\n");
                        var lines = code.Split('\n');
                        markdown.Append(Environment.NewLine + "    ");
                        markdown.Append(string.Join(Environment.NewLine + "    ", lines));
                        break;
                    default:
                        ProcessNodes(markdown, htmlNode.ChildNodes);
                        Tracing.Info(String.Format("{0}", htmlNode.OuterHtml));
                        break;
                }
            }
        }
    }
}