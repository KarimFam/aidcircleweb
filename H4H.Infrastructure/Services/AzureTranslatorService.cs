using Azure;
using Azure.AI.Translation.Text;
using H4H.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace H4H.Infrastructure.Services
{
    public interface IAzureTranslatorService
    {
        Task<(string detectedLanguage, string translatedText)> DetectAndTranslateAsync(string text, string targetLanguageCode);
        Task<string> TranslateAsync(string text, string fromLanguageCode, string toLanguageCode);
        Task<string> DetectLanguageAsync(string text);
    }

    public class AzureTranslatorService : IAzureTranslatorService
    {
        private readonly TextTranslationClient _client;
        private readonly string _region;

        public AzureTranslatorService(IConfiguration configuration)
        {
            var key = configuration["AzureTranslator:Key"] ?? throw new InvalidOperationException("Azure Translator Key not configured");
            var endpoint = configuration["AzureTranslator:Endpoint"] ?? "https://api.cognitive.microsofttranslator.com";
            _region = configuration["AzureTranslator:Region"] ?? "eastus";

            var credential = new AzureKeyCredential(key);
            _client = new TextTranslationClient(credential, endpoint);
        }

        public async Task<(string detectedLanguage, string translatedText)> DetectAndTranslateAsync(string text, string targetLanguageCode)
        {
            try
            {
                var response = await _client.TranslateAsync(targetLanguageCode, text);
                var translation = response.Value.FirstOrDefault();
                
                if (translation != null)
                {
                    var detectedLang = translation.DetectedLanguage?.Language ?? "unknown";
                    var translatedText = translation.Translations.FirstOrDefault()?.Text ?? text;
                    return (detectedLang, translatedText);
                }

                return ("unknown", text);
            }
            catch (Exception ex)
            {
                // Log error and return original text
                Console.WriteLine($"Translation error: {ex.Message}");
                return ("unknown", text);
            }
        }

        public async Task<string> TranslateAsync(string text, string fromLanguageCode, string toLanguageCode)
        {
            try
            {
                var response = await _client.TranslateAsync(toLanguageCode, text, sourceLanguage: fromLanguageCode);
                var translation = response.Value.FirstOrDefault();
                return translation?.Translations.FirstOrDefault()?.Text ?? text;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Translation error: {ex.Message}");
                return text;
            }
        }

        public async Task<string> DetectLanguageAsync(string text)
        {
            try
            {
                var response = await _client.TranslateAsync("en", text);
                var translation = response.Value.FirstOrDefault();
                return translation?.DetectedLanguage?.Language ?? "unknown";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Language detection error: {ex.Message}");
                return "unknown";
            }
        }
    }

    public static class ChatLanguageExtensions
    {
        public static string ToLanguageCode(this ChatLanguage language)
        {
            return language switch
            {
                ChatLanguage.English => "en",
                ChatLanguage.Spanish => "es",
                ChatLanguage.French => "fr",
                ChatLanguage.German => "de",
                ChatLanguage.Chinese => "zh",
                ChatLanguage.Arabic => "ar",
                ChatLanguage.Portuguese => "pt",
                ChatLanguage.Russian => "ru",
                ChatLanguage.Japanese => "ja",
                ChatLanguage.Korean => "ko",
                ChatLanguage.Hindi => "hi",
                _ => "en"
            };
        }

        public static ChatLanguage FromLanguageCode(string code)
        {
            return code?.ToLower() switch
            {
                "en" => ChatLanguage.English,
                "es" => ChatLanguage.Spanish,
                "fr" => ChatLanguage.French,
                "de" => ChatLanguage.German,
                "zh" => ChatLanguage.Chinese,
                "ar" => ChatLanguage.Arabic,
                "pt" => ChatLanguage.Portuguese,
                "ru" => ChatLanguage.Russian,
                "ja" => ChatLanguage.Japanese,
                "ko" => ChatLanguage.Korean,
                "hi" => ChatLanguage.Hindi,
                _ => ChatLanguage.English
            };
        }
    }
}
