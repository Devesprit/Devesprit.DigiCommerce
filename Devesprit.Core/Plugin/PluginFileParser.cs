﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Devesprit.Core.Plugin
{
    public static partial class PluginFileParser
    {
        public static IList<string> ParseInstalledPluginsFile(string filePath)
        {
            if (!File.Exists(filePath))
                return new List<string>();

            var text = File.ReadAllText(filePath);
            if (String.IsNullOrEmpty(text))
                return new List<string>();
            
            var lines = new List<string>();
            using (var reader = new StringReader(text))
            {
                string str;
                while ((str = reader.ReadLine()) != null)
                {
                    if (String.IsNullOrWhiteSpace(str))
                        continue;
                    lines.Add(str.Trim());
                }
            }
            return lines;
        }

        public static void SaveInstalledPluginsFile(IList<String> pluginSystemNames, string filePath)
        {
            string result = "";
            foreach (var sn in pluginSystemNames)
                result += $"{sn}{Environment.NewLine}";

            File.WriteAllText(filePath, result);
        }

        public static PluginDescriptor ParsePluginDescriptionFile(string filePath)
        {
            var descriptor = new PluginDescriptor();
            var text = File.ReadAllText(filePath);
            if (String.IsNullOrEmpty(text))
                return descriptor;

            var settings = new List<string>();
            using (var reader = new StringReader(text))
            {
                string str;
                while ((str = reader.ReadLine()) != null)
                {
                    if (String.IsNullOrWhiteSpace(str))
                        continue;
                    settings.Add(str.Trim());
                }
            }

            foreach (var setting in settings)
            {
                var separatorIndex = setting.IndexOf(':');
                if (separatorIndex == -1)
                {
                    continue;
                }
                string key = setting.Substring(0, separatorIndex).Trim();
                string value = setting.Substring(separatorIndex + 1).Trim();

                switch (key)
                {
                    case "Group":
                        descriptor.Group = value;
                        break;
                    case "FriendlyName":
                        descriptor.FriendlyName = value;
                        break;
                    case "SystemName":
                        descriptor.SystemName = value;
                        break;
                    case "Version":
                        descriptor.Version = value;
                        break;
                    case "Author":
                        descriptor.Author = value;
                        break;
                    case "DisplayOrder":
                        {
                            int.TryParse(value, out var displayOrder);
                            descriptor.DisplayOrder = displayOrder;
                        }
                        break;
                    case "FileName":
                        descriptor.PluginFileName = value;
                        break;
                    case "Description":
                        descriptor.Description = value;
                        break;
                    default:
                        break;
                }
            }
            
            return descriptor;
        }

        public static void SavePluginDescriptionFile(PluginDescriptor plugin)
        {
            if (plugin == null)
                throw new ArgumentException("plugin");

            //get the Description.txt file path
            if (plugin.OriginalAssemblyFile == null)
                throw new Exception($"Cannot load original assembly path for {plugin.SystemName} plugin.");
            var filePath = Path.Combine(plugin.OriginalAssemblyFile.Directory.FullName, "Description.txt");
            if (!File.Exists(filePath))
                throw new Exception($"Description file for {plugin.SystemName} plugin does not exist. {filePath}");

            var keyValues = new List<KeyValuePair<string, string>>();
            keyValues.Add(new KeyValuePair<string, string>("Group", plugin.Group));
            keyValues.Add(new KeyValuePair<string, string>("FriendlyName", plugin.FriendlyName));
            keyValues.Add(new KeyValuePair<string, string>("SystemName", plugin.SystemName));
            keyValues.Add(new KeyValuePair<string, string>("Version", plugin.Version));
            keyValues.Add(new KeyValuePair<string, string>("Author", plugin.Author));
            keyValues.Add(new KeyValuePair<string, string>("DisplayOrder", plugin.DisplayOrder.ToString()));
            keyValues.Add(new KeyValuePair<string, string>("FileName", plugin.PluginFileName));
            keyValues.Add(new KeyValuePair<string, string>("Description", plugin.Description));


            var sb = new StringBuilder();
            for (int i = 0; i < keyValues.Count; i++)
            {
                var key = keyValues[i].Key;
                var value = keyValues[i].Value;
                sb.AppendFormat("{0}: {1}", key, value);
                if (i != keyValues.Count - 1)
                    sb.Append(Environment.NewLine);
            }
            //save the file
            File.WriteAllText(filePath, sb.ToString());
        }
    }
}
