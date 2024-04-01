using System;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Linq;
using System.Collections;

namespace ComputerVisionDemo
{
    class Program
    {
        static string subscriptionKey = "50b57b45b29942c4b5fe77e7d2c85e0f";
        static string endpoint = "https://sjt75compvision.cognitiveservices.azure.com/";
        private const string ANALYZE_URL_IMAGE = "https://sjt75artefactstore.blob.core.windows.net/img/SampleImage1.jpg";
        //private const string ANALYZE_URL_IMAGE = "https://sjt75artefactstore.blob.core.windows.net/img/Landmark.jpg";
        private const string READ_TEXT_URL_IMAGE = "https://az900course.blob.core.windows.net/coursefiles/13238807_228d3a094b_o.jpg";
        private const string DOMAIN_URL_IMAGE = "https://sjt75artefactstore.blob.core.windows.net/img/Taj.jpg";
        private const string DOMAIN_URL_CELEB_IMAGE = "https://sjt75artefactstore.blob.core.windows.net/img/Celebrity.jpg";
        private const string DOMAIN_URL_BRAND_IMAGE = "https://sjt75artefactstore.blob.core.windows.net/img/gray-shirt-logo.jpg";
        private const string DOMAIN_URL_ADULT_IMAGE = "https://sjt75artefactstore.blob.core.windows.net/img/CheckModeration.jpg";
         private const string READ_TEXT_PRNT_IMAGE = "https://sjt75artefactstore.blob.core.windows.net/img/IMG_20240328_103427.jpg";

        static void Main(string[] args)
        {
            ComputerVisionClient client = Authenticate(endpoint, subscriptionKey);

            //Tag and image
           AnalyzeImageUrl(client, ANALYZE_URL_IMAGE).Wait();
            //ReadFileUrl(client, READ_TEXT_URL_IMAGE).Wait();
        }

        public static ComputerVisionClient Authenticate(string endpoint, string key)
        {
            ComputerVisionClient client =
              new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
              { Endpoint = endpoint };
            return client;
        }

        public static async Task AnalyzeImageUrl(ComputerVisionClient client, string imageUrl)
        {
            List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
            {
                VisualFeatureTypes.Categories, VisualFeatureTypes.Description,
                VisualFeatureTypes.Faces, VisualFeatureTypes.ImageType,
                VisualFeatureTypes.Tags, VisualFeatureTypes.Adult,
                VisualFeatureTypes.Color, VisualFeatureTypes.Brands,
                VisualFeatureTypes.Objects
            };

            List<VisualFeatureTypes?> brandfeatures = new List<VisualFeatureTypes?>()
            {
                VisualFeatureTypes.Brands
            };
            Console.WriteLine($"Analyzing the image {Path.GetFileName(imageUrl)}...");
            Console.WriteLine();
            // Analyze the URL image 
            ImageAnalysis results = await client.AnalyzeImageAsync(imageUrl, visualFeatures: features);

            Console.WriteLine("Tags:");
            foreach (var tag in results.Tags)
            {
                Console.WriteLine($"{tag.Name} {tag.Confidence}");
            }

            //Describe the image
            foreach (var desc in results.Description.Captions)
            {
                Console.WriteLine($"{desc.Text} {desc.Confidence}");
            }

            //Domain based analysis
            DomainModelResults domainresults = await client.AnalyzeImageByDomainAsync("landmarks", DOMAIN_URL_IMAGE);
            if (domainresults.Result != null)
            {
                foreach (var item in (IEnumerable)domainresults.Result)
                {
                    Console.WriteLine(item);
                }

            }

                        
            Console.WriteLine("Domain Landmark result");

            List<VisualFeatureTypes?> moderationfeatures = new List<VisualFeatureTypes?>()
            {
                VisualFeatureTypes.Adult
            };

            //Content Moderation

            Console.WriteLine("Moderate Content");
            ImageAnalysis moderationResults = await client.AnalyzeImageAsync(DOMAIN_URL_ADULT_IMAGE, moderationfeatures);

            Console.WriteLine($"{moderationResults.Adult.IsAdultContent} {moderationResults.Adult.AdultScore}");
            Console.WriteLine($"{moderationResults.Adult.IsGoryContent} {moderationResults.Adult.GoreScore}");
            Console.WriteLine($"{moderationResults.Adult.IsRacyContent} {moderationResults.Adult.RacyScore}");
            //nsole.WriteLine(domainresults.Result()landmark']);

            Console.WriteLine();


            ///Generate Thubmail
            ///
            /// 
            ///

            Stream thumbnail_file = await client.GenerateThumbnailAsync(100, 100, DOMAIN_URL_ADULT_IMAGE,true);
            //Save stream to a file on local drive

            Stream output_file = File.Create ("D:\\temp\\thumbnail.jpg");
            thumbnail_file.CopyTo(output_file);


            //Read printed text
            ReadFileUrl(client, READ_TEXT_PRNT_IMAGE).Wait();
       }

        public static async Task ReadFileUrl(ComputerVisionClient client, string urlFile)
        {
            var textHeaders = await client.ReadAsync(urlFile);

            string operationLocation = textHeaders.OperationLocation;
            Thread.Sleep(2000);

            const int numberOfCharsInOperationId = 36;
            string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

            ReadOperationResult results;
            Console.WriteLine($"Extracting text from URL file {Path.GetFileName(urlFile)}...");
            Console.WriteLine();
            do
            {
                results = await client.GetReadResultAsync(Guid.Parse(operationId));
            }
            while ((results.Status == OperationStatusCodes.Running ||
                results.Status == OperationStatusCodes.NotStarted));

            Console.WriteLine();
            var textUrlFileResults = results.AnalyzeResult.ReadResults;
            foreach (ReadResult page in textUrlFileResults)
            {
                foreach (Line line in page.Lines)
                {
                    Console.WriteLine(line.Text);
                }
            }
            Console.WriteLine();



           
        }
    }
}

