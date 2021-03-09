using Sitecore.Xml.Xsl;

namespace Examples.ContentEditorCulture.Pipelines.RenderField
{
    public class GetDateFieldValue : Sitecore.Pipelines.RenderField.GetDateFieldValue
    {
        protected override DateRenderer CreateRenderer()
        {
            return new Xml.Xsl.DateRenderer();
        }
    }
}