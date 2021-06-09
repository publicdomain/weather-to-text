﻿// <copyright file="Program.cs" company="PublicDomain.com">
//     CC0 1.0 Universal (CC0 1.0) - Public Domain Dedication
//     https://creativecommons.org/publicdomain/zero/1.0/legalcode
// </copyright>
// <auto-generated />

namespace WeatherToText
{
    // Directives
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Xml.XPath;
    using ConsoleTableExt;
    using HtmlAgilityPack;

    /// <summary>
    /// Main class.
    /// </summary>
    class MainClass
    {
        /// <summary>
        /// The regex.
        /// </summary>
        private static readonly Regex regex = new Regex(@"Â|\s+|\(\d+\)", RegexOptions.Compiled);

        /// <summary>
        /// The entry point of the program, where the program control starts and ends.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        private static void Main(string[] args)
        {
            // Set encoding
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Set console foreground color
            var consoleForegoundColor = Console.ForegroundColor;

            // Check for no args
            if (args.Length == 0)
            {
                // Advice user
                Console.WriteLine("Missing arguments.");

                // Halt program flow
                return;
            }

            // Catch errors
            try
            {
                // Check for a color argument
                if (args.Length > 1)
                {
                    // Set passed color ensuring first letter's uppercase 
                    string passedColor = $"{char.ToUpperInvariant(args[1][0])}{args[1].Substring(1)}";

                    // Set text color for program output
                    Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), passedColor);
                }

                // Configure service point manager
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;

                // Set web client
                WebClient webClient = new WebClient();

                // Declare the html string
                string htmlString = string.Empty;

                // Check for local or remote fetch
                if (args[0].StartsWith("http"))
                {
                    // Set html string by download
                    htmlString = webClient.DownloadString(args[0]);
                }
                else
                {
                    // Set html strnig by local file
                    htmlString = File.ReadAllText(args[0]);
                }

                // The HTML document
                HtmlDocument htmlDocument = new HtmlDocument();

                // Load the HTML
                htmlDocument.LoadHtml(htmlString);

                // The table data list
                List<List<object>> tableDataList = new List<List<object>>();

                // Insert first-row substitutions
                var firstRowDataList = new List<object>();
                firstRowDataList.Add("Tid");
                firstRowDataList.Add("temp");
                firstRowDataList.Add("känns");
                firstRowDataList.Add("mm");
                firstRowDataList.Add("m/s");
                firstRowDataList.Add($"nederbörd  ");
                firstRowDataList.Add($"åska");

                // Add first row
                tableDataList.Add(firstRowDataList);

                // Set rows
                HtmlNodeCollection rows = htmlDocument.DocumentNode.SelectNodes("//*[starts-with(@id, 'hour-1_')]");

                // Process rows
                foreach (HtmlNode rowHtmlNode in rows)
                {
                    // Declare row data list
                    var rowDataList = new List<object>();

                    // Insert time
                    rowDataList.Add(rowHtmlNode.SelectSingleNode(".//time").InnerText.Trim());

                    // Set columns 
                    HtmlNodeCollection values = rowHtmlNode.SelectNodes(".//span[@class='value']");

                    // Iterate cells
                    for (int valueIndex = 0; valueIndex < 6; valueIndex++)
                    {
                        // Set processed inner text
                        string value = regex.Replace(values[valueIndex].InnerText.Trim(), string.Empty);                        

                        // Check length
                        if (value.Length > 0)
                        {
                            // Has length and it's within working range. Add it
                            rowDataList.Add(value);
                        }
                    }

                    // Push row into table data
                    tableDataList.Add(rowDataList);
                }

                // Check if must display
                if (tableDataList.Count > 0)
                {
                    // Set exported table 
                    string exportedTable = ConsoleTableBuilder.From(tableDataList)
                   .WithFormat(ConsoleTableBuilderFormat.Minimal)
                   .Export().ToString();

                    // Fetch first line
                    string firstLine = exportedTable.Substring(0, exportedTable.IndexOf(Environment.NewLine));

                    // Prepend line
                    Console.CursorLeft = exportedTable.IndexOf("nederbörd");
                    Console.Write("Sannolikhet");
                    Console.CursorLeft = exportedTable.IndexOf("åska");
                    Console.Write("Sannolikhet" + Environment.NewLine);

                    // Output table
                    Console.WriteLine(exportedTable);
                }
            }
            catch (Exception exception)
            {
                // Report error
                Console.WriteLine($"Error: {exception.Message}");
            }
            finally
            {
                // Set console color back
                if (args.Length > 1)
                {
                    // Set foregronud color
                    Console.ForegroundColor = consoleForegoundColor;
                }
            }
        }
    }
}
