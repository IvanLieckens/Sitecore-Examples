using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using HtmlAgilityPack;

using Sitecore.Mvc.Extensions;
using Sitecore.Mvc.Pipelines.Response.RenderRendering;

namespace Examples.EXM.Pipelines.RenderRendering
{
    public class ExmTableRewriter : RenderRenderingProcessor
    {
        public override void Process(RenderRenderingArgs args)
        {
            if (
                !Sitecore.Context.PageMode.IsExperienceEditorEditing
                || !(args.PageContext?.RequestContext?.HttpContext.Request.QueryString?.GetValues("exm")?.Contains("1")).GetValueOrDefault(false))
            {
                return;
            }

            if (!(args.Writer is StringWriter writer))
            {
                return;
            }

            RewriteTables(writer);
        }

        private static void RewriteTables(StringWriter writer)
        {
            StringBuilder sb = writer.GetStringBuilder();
            HtmlDocument doc = LoadDocument(sb);
            bool touched = false;

            touched |= RewriteTag(doc, "table", "table");
            touched |= RewriteTag(doc, "tr", "table-row");
            touched |= RewriteTag(doc, "td", "table-cell");

            if (touched)
            {
                FlushWriter(sb, doc);
            }
        }

        private static HtmlDocument LoadDocument(StringBuilder sb)
        {
            HtmlDocument result = new HtmlDocument();
            result.LoadHtml(sb.ToString());

            return result;
        }

        private static void FlushWriter(StringBuilder sb, HtmlDocument doc)
        {
            using (StringWriter sw = new StringWriter())
            {
                doc.Save(sw);
                sw.Flush();
                sb.Clear();
                sb.Append(sw);
            }
        }

        private static bool RewriteTag(HtmlDocument doc, string tagName, string style)
        {
            bool result = false;
            HtmlNodeCollection tags = doc.DocumentNode.SelectNodes($"//{tagName}");
            if (tags != null && tags.Count > 0)
            {
                foreach (HtmlNode tag in tags)
                {
                    tag.Name = "div";
                    AddStyle(tag, "display", style);
                    RewriteHeight(tag);
                    RewriteWidth(tag);
                    RewriteBgColor(tag);
                    RewriteAlign(tag);
                    RewriteVAlign(tag);
                }

                result = true;
            }

            return result;
        }

        private static void RewriteVAlign(HtmlNode node)
        {
            HtmlAttribute attribute = node.Attributes["valign"];
            if (attribute != null)
            {
                AddStyle(node, "vertical-align", attribute.Value);
            }
        }

        private static void RewriteAlign(HtmlNode node)
        {
            HtmlAttribute attribute = node.Attributes["align"];
            if (attribute != null)
            {
                switch (attribute.Value)
                {
                    case "left":
                        AddStyle(node, "margin", "0 auto 0 0");
                        break;
                    case "right":
                        AddStyle(node, "margin", "0 0 0 auto");
                        break;
                    case "center":
                        AddStyle(node, "margin", "0 auto");
                        break;
                }
            }
        }

        private static void RewriteBgColor(HtmlNode node)
        {
            HtmlAttribute attribute = node.Attributes["bgcolor"];
            if (attribute != null)
            {
                AddStyle(node, "background-color", attribute.Value);
            }
        }

        private static void RewriteWidth(HtmlNode node)
        {
            HtmlAttribute attribute = node.Attributes["width"];
            if (attribute != null)
            {
                string newValue = attribute.Value.Contains("%") ? attribute.Value : $"{attribute.Value}px";
                AddStyle(node, "width", newValue);
            }
        }

        private static void RewriteHeight(HtmlNode node)
        {
            HtmlAttribute attribute = node.Attributes["height"];
            if (attribute != null)
            {
                string newValue = attribute.Value.Contains("%") ? attribute.Value : $"{attribute.Value}px";
                AddStyle(node, "height", newValue);
            }
        }

        private static void AddStyle(HtmlNode node, string key, string value)
        {
            HtmlAttribute styleAttribute = node.Attributes["style"];

            if (styleAttribute != null && !StyleKeys(styleAttribute.Value).Contains(key))
            {
                styleAttribute.Value = styleAttribute.Value.Trim().WithPostfix(";") + $"{key}: {value};";
            }
            else
            {
                node.Attributes.Append("style", $"{key}: {value};");
            }
        }

        private static IEnumerable<string> StyleKeys(string styleValue)
        {
            string[] styles = styleValue.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string style in styles)
            {
                string[] keyValuePair = style.Split(':');
                yield return keyValuePair[0];
            }
        }
    }
}