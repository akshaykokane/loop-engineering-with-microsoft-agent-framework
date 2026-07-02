const int MaxContextWindowTokens = 1_050_000;
const int MaxOutputTokens = 32_000;

var endpoint = Environment.GetEnvironmentVariable("AZURE_AI_PROJECT_ENDPOINT")
    ?? "https://testmediumazureopenai.services.ai.azure.com/api/projects/testmediumazureopenai-project";
var deploymentName = Environment.GetEnvironmentVariable("AZURE_AI_MODEL_DEPLOYMENT_NAME") ?? "gpt-4.1";

var clientFactory = new AzureAgentClientFactory(endpoint, deploymentName);
var harnessAgentFactory = new HarnessAgentFactory(clientFactory, MaxContextWindowTokens, MaxOutputTokens);
var blogLoop = new BlogLoop(harnessAgentFactory);

var blogText = File.ReadAllText("blogs/blog.txt");
await blogLoop.RunAsync(blogText);
