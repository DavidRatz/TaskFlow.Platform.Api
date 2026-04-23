using TaskFlow.Platform.Domain.Emails.Models;
using TaskFlow.Platform.Domain.Emails.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace TaskFlow.Platform.Infrastructure.Emails.Services;

public class RazorEmailService(
    IRazorViewEngine razorViewEngine,
    ITempDataProvider tempDataProvider,
    IServiceProvider serviceProvider,
    ISendEmailService sendEmailService)
    ////IOptions<EmailOptions> emailOptions)
    : IEmailService
{
    public async Task SendEmailAsync<T>(string templateName, T model, string to, string subject,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var email = await CreateEmailAsync(templateName, model, to, subject);
        await sendEmailService.SendEmailAsync(email, cancellationToken);
    }

    public async Task SendEmailWithAttachmentsAsync<T>(string templateName, T model, string to,
        string subject, List<EmailAttachment> attachments,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var email = await CreateEmailAsync(templateName, model, to, subject);
        attachments.ForEach(attachment => email.AddAttachment(attachment));
        await sendEmailService.SendEmailAsync(email, cancellationToken);
    }

    private async Task<Email> CreateEmailAsync<T>(string templateName, T model, string to, string subject)
        where T : class
    {
        var htmlContent = await RenderViewToStringAsync(templateName, model);
        return new Email
        {
            From = string.Empty, ////emailOptions.Value.FromAddress,
            To = to,
            Subject = subject,
            Message = htmlContent,
        };
    }

    private async Task<string> RenderViewToStringAsync<T>(string viewName, T model)
    {
        var actionContext = GetActionContext();
        var view = FindView(actionContext, viewName);

        await using var output = new StringWriter();
        var viewContext = new ViewContext(
            actionContext,
            view,
            new ViewDataDictionary<T>(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = model },
            new TempDataDictionary(actionContext.HttpContext, tempDataProvider),
            output,
            new HtmlHelperOptions());

        await view.RenderAsync(viewContext);
        return output.ToString();
    }

    private IView FindView(ActionContext actionContext, string viewName)
    {
        var getViewResult = razorViewEngine.GetView(null, viewName, false);
        if (getViewResult.Success)
        {
            return getViewResult.View;
        }

        var findViewResult = razorViewEngine.FindView(actionContext, viewName, false);
        if (findViewResult.Success)
        {
            return findViewResult.View;
        }

        var searchedLocations = getViewResult.SearchedLocations.Concat(findViewResult.SearchedLocations);
        var errorMessage = string.Join(
            Environment.NewLine,
            new[] { $"Unable to find view '{viewName}'. The following locations were searched:" }.Concat(
                searchedLocations));

        throw new InvalidOperationException(errorMessage);
    }

    private ActionContext GetActionContext()
    {
        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };
        return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
    }
}
