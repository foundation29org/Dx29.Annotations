using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Dx29.Services;

namespace Dx29.Annotations
{
    static public class ServiceConfiguration
    {
        static public void AddAnnotations(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDocConverter(configuration);
            services.AddDocTranslator(configuration);
            services.AddNCRAnnotation(configuration);
            services.AddTAHAnnotation(configuration);

            services.AddFileStorage(configuration);
            services.AddResourceGroup(configuration);

            services.AddSignalR(configuration);

            services.AddSingleton<AnnotationService>();
            services.AddSingleton<SyncAnnotationService>();
        }

        static public void AddDocConverter(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<DocConverterService>();
            services.AddHttpClient<DocConverterService>(http =>
            {
                http.BaseAddress = new Uri(configuration["DocConverter:Endpoint"]);
                http.Timeout = TimeSpan.FromMinutes(20);
            });
        }

        static public void AddDocTranslator(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient<DocTranslatorService>("Segmentation", http =>
            {
                http.BaseAddress = new Uri(configuration["Segmentation:Endpoint"]);
            });
            services.AddHttpClient<DocTranslatorService>("CognitiveServices", http =>
            {
                http.BaseAddress = new Uri(configuration["CognitiveServices:Endpoint"]);
                http.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", configuration["CognitiveServices:Authorization"]);
                http.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", configuration["CognitiveServices:Region"]);
            });
            services.AddSingleton<DocTranslatorService>();
        }

        static public void AddNCRAnnotation(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<NCRAnnotationService>();
            services.AddHttpClient<NCRAnnotationService>(http =>
            {
                http.BaseAddress = new Uri(configuration["NCRAnnotation:Endpoint"]);
                http.DefaultRequestHeaders.Add("Authorization", $"Bearer {configuration["NCRAnnotation:Authorization"]}");
                http.Timeout = TimeSpan.FromMinutes(10);
            });
        }

        static public void AddTAHAnnotation(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<TAHAnnotationService>();
            services.AddHttpClient<TAHAnnotationService>(http =>
            {
                http.BaseAddress = new Uri(configuration["TAHAnnotation:Endpoint"]);
                http.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", configuration["TAHAnnotation:Authorization"]);
            });
        }

        static public void AddFileStorage(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<FileStorageClient2>();
            services.AddHttpClient<FileStorageClient2>(http =>
            {
                http.BaseAddress = new Uri(configuration["FileStorage:Endpoint"]);
            });
        }

        static public void AddResourceGroup(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ResourceGroupService>();
            services.AddHttpClient<ResourceGroupService>(http =>
            {
                http.BaseAddress = new Uri(configuration["MedicalHistory:Endpoint"]);
            });
        }

        static public void AddSignalR(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(new SignalRService(configuration["SignalR:ConnectionString"], configuration["SignalR:HubName"]));
        }
    }
}
