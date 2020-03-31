using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Autotest.Utils
{
    public class LockCategory
    {
        private static List<List<string>> _testsTags;

        static LockCategory()
        {
            _testsTags = new List<List<string>>();
        }

        public static void AddTags(List<string> tags)
        {
            lock (_testsTags)
            {
                _testsTags.Add(tags);
            }
        }

        public static void RemoveTags(List<string> tags)
        {
            lock (_testsTags)
            {
                var index = -1;
                for (int i = 0; i < _testsTags.Count; i++)
                {
                    if (_testsTags[i].SequenceEqual(tags))
                    {
                        index = i;
                        break;
                    }
                }
                if (index >= 0)
                    _testsTags.RemoveAt(index);
            }
        }

        private static bool IsTagExistInCurrentSession(string expectedTag)
        {
            return _testsTags.Any(x => x.Any(y => y.Contains(expectedTag)));
        }

        private static List<string> GetDoNotRunWithTags()
        {
            List<string> doNotRunWithTag = new List<string>();
            foreach (List<string> tagsList in _testsTags)
            {
                foreach (string s in tagsList)
                {
                    if (s.Contains("Do_Not_Run_With"))
                        doNotRunWithTag.Add(s.Replace("Do_Not_Run_With_", string.Empty));
                }
            }

            return doNotRunWithTag;
        }

        public static void AwaitTags(List<string> categories)
        {
            //If test contains tag that depends on other tags
            if (categories.Any(x => x.Contains("Do_Not_Run_With")))
            {
                var lockTag = categories.First(x => x.Contains("Do_Not_Run_With"))
                    .Replace("Do_Not_Run_With_", string.Empty);
                //Check that there is no tests with mentioned tag
                if (_testsTags.Any(x => x.Any(y => y.Contains(lockTag))))
                {
                    while (IsTagExistInCurrentSession(lockTag))
                    {
                        Thread.Sleep(2000);
                    }
                }
            }

            //If test contains tag with which we can't run
            if (IsTagExistInCurrentSession("Do_Not_Run_With"))
            {
                while (GetDoNotRunWithTags().Intersect(categories).Any())
                {
                    Thread.Sleep(2000);
                }
            }
        }
    }
}
