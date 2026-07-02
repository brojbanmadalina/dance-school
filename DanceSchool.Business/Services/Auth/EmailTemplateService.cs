using Scriban;

namespace DanceSchool.Business.Services.Auth
{
    public class EmailTemplateService
    {
        private readonly string _templatesPath;

        public EmailTemplateService(string templatesPath)
        {
            _templatesPath = templatesPath;
        }

        public async Task<string> RenderAsync(string templateName, object model)
        {
            var templatePath = Path.Combine(_templatesPath, templateName);
            var source = await File.ReadAllTextAsync(templatePath);

            var template = Template.Parse(source);

            if (template.HasErrors)
            {
                throw new InvalidOperationException(
                    string.Join(", ", template.Messages.Select(m => m.Message)));
            }

            return await template.RenderAsync(model);
        }
    }
}