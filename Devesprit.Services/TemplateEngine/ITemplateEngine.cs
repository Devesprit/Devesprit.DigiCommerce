namespace Devesprit.Services.TemplateEngine
{
    public partial interface ITemplateEngine
    {
        string CompileTemplate(string template, object model);
        string CompileTemplateFromFile(string template, object model);
    }
}
