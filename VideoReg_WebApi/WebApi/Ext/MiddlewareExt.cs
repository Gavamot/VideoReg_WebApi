using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using System;
using System.IO;
using System.Threading.Tasks;
using WebApi.Dto;

namespace WebApi.Ext
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;

        public RequestLoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, RecyclableMemoryStreamManager recyclableMemoryStreamManager)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger("request");
            this._recyclableMemoryStreamManager = recyclableMemoryStreamManager;
        }

        /// <summary>
        /// ReadStreamInChunks
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        private static string ReadStreamInChunks(Stream stream)
        {
            const int readChunkBufferLength = 4096;
            stream.Seek(0, SeekOrigin.Begin);
            using var textWriter = new StringWriter();
            using var reader = new StreamReader(stream);
            var readChunk = new char[readChunkBufferLength];
            int readChunkLength;
            do
            {
                readChunkLength = reader.ReadBlock(readChunk,
                                                   0,
                                                   readChunkBufferLength);
                textWriter.Write(readChunk, 0, readChunkLength);
            } while (readChunkLength > 0);
            return textWriter.ToString();
        }

        /// <summary>
        /// LogRequest
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="IOException">Ignore.</exception>
        private async Task LogRequest(HttpContext context)
        {
            context.Request.EnableBuffering();
            await using var requestStream = _recyclableMemoryStreamManager.GetStream();
            await context.Request.Body.CopyToAsync(requestStream);
            _logger.LogInformation($"Http Request Information:{Environment.NewLine}" +
                                   $"Schema:{context.Request.Scheme} " +
                                   $"Host: {context.Request.Host} " +
                                   $"Path: {context.Request.Path} " +
                                   $"QueryString: {context.Request.QueryString} " +
                                   $"Request Body: {ReadStreamInChunks(requestStream)}");
            context.Request.Body.Position = 0;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await LogRequest(context);
            }
            catch (ObjectDisposedException)
            {

            }
            await _next(context);

        }
    }

    public static class MiddlewareExt
    {
        #region Request

        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }

        public static IServiceCollection AddRequestLogging(this IServiceCollection services)
        {
            return services.AddTransient<RecyclableMemoryStreamManager>();
        }

        #endregion
        public static void ConfigureException(this IApplicationBuilder app, ILoggerFactory logFactory)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        var log = logFactory.CreateLogger("unhanded");
                        var errorMessage = contextFeature.Error.Message;
                        log.LogError($"Unhanded error : {errorMessage}");
                        await context.Response.WriteAsync(new ErrorDetailsDto
                        {
                            StatusCode = context.Response.StatusCode,
                            Message = errorMessage
                        }.ToString());
                    }
                });
            });
        }
    }
}
