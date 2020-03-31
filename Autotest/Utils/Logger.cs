using System;

namespace Autotest.Utils
{
    public static class Logger
    {
        public static void Write(string text)
        {
            Console.WriteLine(text);
        }

        public static void Write(Exception exception)
        {
            Write(exception.ToString());
        }

        public static void Write(string format, params object[] arg)
        {
            Console.WriteLine(format, arg);
        }
    }
}
