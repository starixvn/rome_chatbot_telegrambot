namespace TelegramBot.WebApi.Middlewares;


public class EnableRequestBodyBufferingMiddleware
{
    private readonly RequestDelegate next;

    public EnableRequestBodyBufferingMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.EnableBuffering();

        await next(context);
    }
}


