using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using SkiaSharp;
using Svg;
using Svg.Transforms;

namespace FlowChartSVG
{
    internal static class FlowChartMain
    {
        private const string FolderPath = "FolderPath";

        private enum MenuOptions
        {
            ZeroPlaceholder,
            DisplayHelp,
            SaveFolderPathToConfigFile,
            CreateNewEmptySvg,
            CreateNewSvg,
            Exit
        }

        private static IConfigurationRoot Configuration { get; set; }

        static void Main(string[] args)
        {
            EnsureAppSettingsJson();
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            Configuration = builder.Build();

            var exit = false;
            while (!exit)
            {
                Console.WriteLine("Please select an option:");
                Console.WriteLine("1. Display help menu");
                Console.WriteLine("2. Change folder path for file save");
                Console.WriteLine("3. Create a new empty file in the specified folder: " + Configuration[FolderPath]);
                Console.WriteLine("4. Create a new file based on prompt in the specified folder: " + Configuration[FolderPath]);
                Console.WriteLine("5. Exit");

                switch (int.Parse(Console.ReadLine() ?? string.Empty))
                {
                    case (int)MenuOptions.DisplayHelp:
                        DisplayHelpMenu();
                        break;
                    case (int)MenuOptions.SaveFolderPathToConfigFile:
                        SaveFolderPath();
                        break;
                    case (int)MenuOptions.CreateNewEmptySvg:
                        CreateNewEmptyFile();
                        break;
                    case (int)MenuOptions.CreateNewSvg:
                        CreateNewFile();
                        break;
                    case (int)MenuOptions.Exit:
                        exit = true;
                        break;
                    default:
                        InvalidOption();
                        break;
                }
            }
        }

        private static void EnsureAppSettingsJson()
        {
            var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            if (File.Exists(appSettingsPath))
            {
                return;
            }

            var defaultContent = "{\n  \"FolderPath\": \"" + Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\"\n}";
            File.WriteAllText(appSettingsPath, defaultContent);
        }

        private static void InvalidOption()
        {
            Console.WriteLine("Invalid option. Please try again.\n");
        }

        private static void DisplayHelpMenu()
        {
            Console.WriteLine("\nHelp Menu:");
            Console.WriteLine("1. Display help menu: Displays this help menu");
            Console.WriteLine("2. Change folder path for file save: Save the folder path where the exported file is saved to the program's config file");
            Console.WriteLine("3. Creates a new empty file in the specified folder: Create a new file in " + Configuration[FolderPath]);
            Console.WriteLine("4. Creates a new file in the specified folder: Create a new file in " + Configuration[FolderPath] + "\n" +
                              "\nPrompt has the following structure:" +
                              "\n*fileName* *orientation* (TD/BT/LR/RL)" +
                              "\n*firstNode* *-->* *secondNode*\n");
            Console.WriteLine("5. Exit: Exits the application\n");
            Console.WriteLine("Press anything to continue...\n");
            Console.ReadLine();
        }

        private static void SaveFolderPath()
        {
            Console.Write("Enter the folder path to save: ");
            var folderPath = Console.ReadLine();

            if (Directory.Exists(folderPath))
            {
                var jsonFile = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
                var jsonContent = File.ReadAllText(jsonFile);
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonContent);
                if (jsonObj == null)
                {
                    throw new FileNotFoundException(nameof(jsonFile));
                }

                jsonObj[FolderPath] = folderPath;
                string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(jsonFile, output);

                Console.WriteLine("Folder path saved successfully.\n");
            }
            else
            {
                Console.WriteLine("Invalid folder path. Please try again.\n");
            }
        }

        private static void CreateNewEmptyFile()
        {
            if (!IsValidFolderPath())
            {
                return;
            }

            ParseNewFlowChartLine("defaultName.svg LR", out string fileName, out string filePath, out FlowChart flowChart);

            using (FileStream fs = File.Create(filePath))
            {
                Node currentNode = null;

                var start = flowChart.AddNode(new ParallelogramNode(flowChart, "Start"));

                var woop = start.AddChild("woop");
                var waka = woop.AddChild("waka");
                var waka2 = woop.AddChild(new ParallelogramNode(flowChart, woop, "waka2"));

                var whatDis = waka.AddChild("what dis?");
                var thisNode = waka.AddChild("this node");
                var thisNodeTwoo = waka.AddChild("this node twoo");

                var hui = waka2.AddChild("hui");

                flowChart.GenerateSvgContent(fs);

                Console.WriteLine(
                    $"An empty SVG file named '{fileName}' has been created in the specified folder: {Configuration[FolderPath]}");
            }
        }

        private static void CreateNewFile()
        {
            if (!IsValidFolderPath())
            {
                return;
            }

            ParseNewFlowChartLine(Console.ReadLine(), out string fileName, out string filePath, out FlowChart flowChart);

            using (FileStream fs = File.Create(filePath))
            {
                Node currentNode = null;

                string line;
                while ((line = Console.ReadLine()) != "")
                {
                    ParseNewNodeLine(flowChart, line);
                }

                flowChart.GenerateSvgContent(fs);

                Console.WriteLine(
                    $"An SVG file named '{fileName}' has been created in the specified folder: {Configuration[FolderPath]}");
            }
        }

        private static void ParseNewNodeLine(FlowChart flowChart, string line)
        {
            var lineSplit = line.Split("-->");
            var parentContent = lineSplit.First().Trim();
            var content = lineSplit.Last().Trim();

            string nodeContent;
            string nodeType;

            ParseContent(content, out nodeContent, out nodeType);

            Node parentNode;
            if (flowChart.Head == null)
            {
                string parentnodeContent;
                string parentNodeType;
                ParseContent(parentContent, out parentnodeContent, out parentNodeType);
                parentNode = CreateNodeByType(flowChart, null, parentnodeContent, parentNodeType);
                flowChart.AddNode(parentNode);
            }
            else
            {
                parentNode = flowChart.Head.FindNode(parentContent);
            }

            Node childNode = CreateNodeByType(flowChart, parentNode, nodeContent, nodeType);
            parentNode.AddChild(childNode);
        }

        private static void ParseContent(string content, out string nodeContent, out string nodeType)
        {
            const int TYPE_GROUP_INDEX = 2;
            const string pattern = @"-(\w+)\s+(\w+)$";  // Matches -{letter} {letter} at the end of the string
            nodeType = "default";  // Set default type initially
            nodeContent = content;

            while (true)
            {
                var match = Regex.Match(nodeContent, pattern);
                if (match.Success)
                {
                    // If the pattern is matched, remove it from the content string
                    nodeContent = nodeContent.Substring(0, match.Index).Trim();

                    // If the tag is "-t", assign the type
                    if (match.Groups[1].Value == "t")
                    {
                        nodeType = match.Groups[TYPE_GROUP_INDEX].Value;
                    }
                }
                else
                {
                    // If no pattern is matched, break the loop
                    break;
                }
            }
        }

        private static Node CreateNodeByType(FlowChart flowChart, Node parentNode, string nodeContent, string nodeType)
        {
            if (parentNode == null)
            {
                return nodeType == "p"
                    ? new ParallelogramNode(flowChart, nodeContent)
                    : new Node(flowChart, nodeContent);
            }

            return nodeType == "p"
                ? new ParallelogramNode(flowChart, parentNode, nodeContent)
                : new Node(flowChart, parentNode, nodeContent);
        }

        private static void ParseNewFlowChartLine(string firstLine, out string fileName, out string filePath, out FlowChart flowChart)
        {
            var firstLineSplit = firstLine.Split(' ');
            fileName = firstLineSplit[0].Trim();
            flowChart = firstLineSplit[1].Trim().ToUpper() switch
            {
                "TD" => new FlowChart(FlowChartDirection.TopDown),
                "BT" => new FlowChart(FlowChartDirection.BottomUp),
                "LR" => new FlowChart(FlowChartDirection.LeftToRight),
                "RL" => new FlowChart(FlowChartDirection.RightToLeft),
                _ => throw new InvalidOperationException()
            };

            if (!fileName.EndsWith(".svg"))
            {
                fileName += ".svg";
            }

            filePath = Path.Combine(Configuration[FolderPath], fileName);
        }

        private static bool IsValidFolderPath()
        {
            var folderPath = Configuration[FolderPath];

            if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
            {
                return true;
            }

            Console.WriteLine(
                "No folder path found in the config file or folder doesn't exist. Please save a folder path first.\n");
            return false;
        }
    }
}
