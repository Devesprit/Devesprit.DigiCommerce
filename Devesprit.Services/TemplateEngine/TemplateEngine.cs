using System.IO;
using Scriban;

namespace Devesprit.Services.TemplateEngine
{
    public partial class TemplateEngine: ITemplateEngine
    {
        public virtual string CompileTemplate(string template, object model)
        {
            var parser = Template.Parse(template);
            return parser.Render(model, m => m.Name);
        }

        public virtual string CompileTemplateFromFile(string templateFile, object model)
        {
            return CompileTemplate(File.ReadAllText(templateFile), model);
        }
    }
}
