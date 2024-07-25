using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace SemanticKernelEmailDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Sk sk = new Sk();
            sk.RunLoop().Wait();
        }
    }

    public class Sk
    { 
        public async Task<int> RunLoop()
        {
            // Create the kernel
            var builder = Kernel.CreateBuilder();
            builder.Services.AddLogging(c => c.SetMinimumLevel(LogLevel.Trace).AddDebug());
            builder.Services.AddAzureOpenAIChatCompletion(
                 "GPT4",                      // Azure OpenAI Deployment Name
                 "https://openaisvcdataaidemos.openai.azure.com", // Azure OpenAI Endpoint
                 "65b4fb7cf60d4663943d0c6c8d91b5c9");
            builder.Plugins.AddFromType<EmailPlugin>();
            builder.Plugins.AddFromType<TextMessagePlugin>();
            Kernel kernel = builder.Build();

            // Retrieve the chat completion service from the kernel
            IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            // Create the chat history
            ChatHistory chatMessages = new ChatHistory("""
    You are a friendly assistant who likes to follow the rules. You will complete required steps
    and request approval before taking any consequential actions, like sending emails or text messages. If the user doesn't provide
    enough information for you to complete a task, you will keep asking questions until you have
    enough information to complete the task.  Make sure to review the content of the email or text message before sending it
    """);

            // Start the conversation
            while (true)
            {
                // Get user input
                System.Console.Write("User > ");
                chatMessages.AddUserMessage(Console.ReadLine()!);

                // Get the chat completions
                OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
                {
                    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
                };
                var result = chatCompletionService.GetStreamingChatMessageContentsAsync(
                    chatMessages,
                    executionSettings: openAIPromptExecutionSettings,
                    kernel: kernel);

                // Stream the results
                string fullMessage = "";
                await foreach (var content in result)
                {
                    if (!content.Role.HasValue)
                    {
                        System.Console.Write("Assistant > ");
                    }
                    System.Console.Write(content.Content);
                    fullMessage += content.Content;
                }
                System.Console.WriteLine();

                // Add the message from the agent to the chat history
                chatMessages.AddAssistantMessage(fullMessage);
            }

            return 1;
        }
    }

    public class EmailPlugin
    {
        [KernelFunction("send_email")]
        [Description("Sends an email to a recipient.")]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task SendEmailAsync(
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            Kernel kernel,
            List<string> recipientEmails,
            string subject,
            string body
        )
        {
            // Add logic to send an email using the recipientEmails, subject, and body
            // For now, we'll just print out a success message to the console
            Console.WriteLine("Email sent!");
        }
    }

    public class TextMessagePlugin
    {
        [KernelFunction("send_text_message")]
        [Description("Sends a text message to a recipient.")]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task SendTxtMsgAsync(
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            Kernel kernel,
            List<string> recipientEmails,
            string subject,
            string body
        )
        {
            // Add logic to send an email using the recipientEmails, subject, and body
            // For now, we'll just print out a success message to the console
            Console.WriteLine("Text Message sent!");
        }
    }
}
