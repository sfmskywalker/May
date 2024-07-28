using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace Elsa.SemanticKernel;

public class KernelFactory(KernelConfig kernelConfig, IServiceProvider serviceProvider, ILogger<KernelFactory> logger)
{
    public Kernel CreateKernel(string agentName)
    {
        var agent = kernelConfig.Agents[agentName];
        return CreateKernel(agent);
    }

    public Kernel CreateKernel(AgentConfig agentConfig)
    {
        var builder = Kernel.CreateBuilder();
        builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));

        AddModels(builder, agentConfig);
        AddSkills(builder, agentConfig);

        return builder.Build();
    }

    private void AddModels(IKernelBuilder builder, AgentConfig agentConfig)
    {
        foreach (string modelName in agentConfig.Models)
        {
            if (!kernelConfig.Models.TryGetValue(modelName, out var model))
            {
                logger.LogWarning($"Model {modelName} not found");
                continue;
            }

            AddServices(builder, model);
        }
    }
    
    private void AddSkills(IKernelBuilder builder, AgentConfig agentConfig)
    {
        foreach (string skillName in agentConfig.Skills)
        {
            if (!kernelConfig.Skills.TryGetValue(skillName, out var skill))
            {
                logger.LogWarning($"Skill {skillName} not found");
                continue;
            }

            AddPlugins(builder, skill);
        }
    }

    private void AddServices(IKernelBuilder builder, ModelConfig model)
    {
        foreach (var service in model.Services)
        {
            switch (service.Type)
            {
                case "OpenAIChatCompletion":
                    var modelId = (string)service.Settings["ModelId"];
                    var apiKey = GetApiKey(service);
                    builder.AddOpenAIChatCompletion(modelId, apiKey);
                    break;
                default:
                    logger.LogWarning($"Unknown service type {service.Type}");
                    break;
            }
        }
    }
    
    private string GetApiKey(ServiceConfig service)
    {
        var settings = service.Settings;
        if(settings.TryGetValue("ApiKey", out var apiKey))
            return (string)apiKey;
        
        if(settings.TryGetValue("ApiKeyRef", out var apiKeyRef))
            return kernelConfig.ApiKeys[(string)apiKeyRef].Value;
        
        throw new KeyNotFoundException($"No api key found for service {service.Type}");
    }

    private void AddPlugins(IKernelBuilder builder, SkillConfig skill)
    {
        foreach (var pluginName in skill.Plugins)
        {
            if (!kernelConfig.Plugins.TryGetValue(pluginName, out var plugin))
            {
                logger.LogWarning($"Plugin {pluginName} not found");
                continue;
            }

            var pluginTypeName = plugin.Type;
            var pluginType = Type.GetType(pluginTypeName);

            if (pluginType == null)
            {
                logger.LogWarning($"Plugin type {pluginType} not found");
                continue;
            }

            var pluginInstance = ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, pluginType);
            builder.Plugins.AddFromObject(pluginInstance, pluginName);
        }
    }
}