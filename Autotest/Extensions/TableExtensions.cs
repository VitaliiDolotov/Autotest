﻿using System;
using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow;

namespace Autotest.Extensions
{
    public static class TableExtensions
    {
        public static List<string> GetHeaders(this Table table)
        {
            return table.Rows.First().Keys.ToList();
        }

        public static List<string[]> GetContent(this Table table)
        {
            var result = new List<string[]>();
            foreach (var row in table.Rows)
            {
                var line = new List<string>();
                foreach (KeyValuePair<string, string> pair in row)
                {
                    line.Add(pair.Value);
                }
                result.Add(line.ToArray());
            }
            return result;
        }

        public static List<string> GetDataByKey(this Table table, string key)
        {
            var result = new List<string>();
            foreach (var row in table.Rows)
            {
                foreach (KeyValuePair<string, string> pair in row)
                {
                    if (pair.Key.Equals(key))
                    {
                        result.Add(pair.Value);
                    }
                }
            }

            throw new Exception($"Unable to find element with '{key}' in the table.");
        }
    }
}