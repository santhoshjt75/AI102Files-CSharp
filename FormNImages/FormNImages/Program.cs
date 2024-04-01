
using Azure.AI.Vision.ImageAnalysis;
using System;
using System.IO;
using System.Drawing;

namespace FormNImages
{
    internal class Program
    {
        static string subscriptionKey = "50b57b45b29942c4b5fe77e7d2c85e0f";
        static string endpoint = "https://sjt75compvision.cognitiveservices.azure.com/";
        public static string IMG_TO_ANALYZE = @"D:/temp/Lincoln.jpg";

        static void Main(string[] args)
        {
            ImageAnalysisClient client = new ImageAnalysisClient(new Uri(endpoint), new Azure.AzureKeyCredential(subscriptionKey));
            using FileStream stream = new FileStream(IMG_TO_ANALYZE, FileMode.Open);
            // Use Analyze image function to read text in image
            ImageAnalysisResult result = client.Analyze(
                BinaryData.FromStream(stream),
                // Specify the features to be retrieved
                VisualFeatures.Read);

            stream.Close();

            // Display analysis results
            if (result.Read != null)
            {
                Console.WriteLine($"Text:");
                // Prepare image for drawing
                System.Drawing.Image image = Image.FromFile(IMG_TO_ANALYZE);
                Graphics graphics = Graphics.FromImage(image);
                Pen pen = new Pen(Color.Cyan, 3);

                foreach (var line in result.Read.Blocks.SelectMany(block => block.Lines))
                {
                    {
                        // Return the text detected in the image

                        Console.WriteLine($"   '{line.Text}'");

                        // Draw bounding box around line
                        var drawLinePolygon = true;

                        // Return each line detected in the image and the position bounding box around each line
                        Console.WriteLine($"   Bounding Polygon: [{string.Join(" ", line.BoundingPolygon)}]");


                        // Return each word detected in the image and the position bounding box around each word with the confidence level of each word
                        foreach (DetectedTextWord word in line.Words)
                        {
                            Console.WriteLine($"     Word: '{word.Text}', Confidence {word.Confidence:F4}, Bounding Polygon: [{string.Join(" ", word.BoundingPolygon)}]");
                            // Draw word bounding polygon
                            drawLinePolygon = false;
                            var r = word.BoundingPolygon;

                            Point[] polygonPoints = {
                                new Point(r[0].X, r[0].Y),
                                new Point(r[1].X, r[1].Y),
                                new Point(r[2].X, r[2].Y),
                                new Point(r[3].X, r[3].Y)
                            };

                            graphics.DrawPolygon(pen, polygonPoints);
                        }


                        // Draw line bounding polygon
                        if (drawLinePolygon)
                        {
                            var r = line.BoundingPolygon;
                            Point[] polygonPoints = {
                            new Point(r[0].X, r[0].Y),
                            new Point(r[1].X, r[1].Y),
                            new Point(r[2].X, r[2].Y),
                            new Point(r[3].X, r[3].Y)
};

                            graphics.DrawPolygon(pen, polygonPoints);
                        }


                    }

                    // Save image
                    String output_file = "text.jpg";
                    image.Save(output_file);
                    Console.WriteLine("\nResults saved in " + output_file + "\n");
                }



            }



        }
    }
}