﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoTest.TestRunners.CmdArguments;
using System.IO;
using AutoTest.TestRunners.Results;
using AutoTest.TestRunners.Shared;
using AutoTest.TestRunners.Shared.Plugins;

namespace AutoTest.TestRunners
{
    class Program
    {
        static void Main(string[] args)
        {
            //args = new string[] { "testrun.xml", "testoutput.xml" };
            if (args.Length != 2)
            {
                printUseage();
                return;
            }
            var parser = new OptionsParser(args[0]);
            parser.Parse();
            if (!parser.IsValid)
                return;

            var result = run(parser);

            var writer = new ResultsXmlWriter(result);
            writer.Write(args[1]);
        }

        private static void printUseage()
        {
            Console.WriteLine("AutoTest.TestRunner is a plugin based generic test runner. ");
        }

        private static IEnumerable<TestResult> run(OptionsParser parser)
        {
            var results = new List<TestResult>();
            foreach (var runner in getRunners(parser))
            {
                foreach (var testRun in parser.Options.TestRuns)
                {
                    if (runner.Handles(testRun.ID))
                        results.AddRange(runner.Run(testRun));
                }
            }
            return results;
        }

        private static IEnumerable<IAutoTestNetTestRunner> getRunners(OptionsParser parser)
        {
            if (parser.Plugins.Count() == 0)
                return allPlugins();
            return getRunnersFrom(parser.Plugins);
        }

        private static IEnumerable<IAutoTestNetTestRunner> getRunnersFrom(IEnumerable<Plugin> plugins)
        {
            var runners = new List<IAutoTestNetTestRunner>();
            foreach (var plugin in plugins)
            {
                var runner = plugin.New();
                if (runner != null)
                    runners.Add(runner);
            }
            return runners;
        }

        private static IEnumerable<IAutoTestNetTestRunner> allPlugins()
        {
            var dir = Path.GetFullPath("TestRunners");
            if (!Directory.Exists(dir))
                return new IAutoTestNetTestRunner[] { };
            var locator = new PluginLocator(dir);
            return getRunnersFrom(locator.Locate());
        }
    }
}
