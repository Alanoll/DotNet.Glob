using System;
using System.Collections.Generic;
using System.Text;

namespace System.Text
{
    internal static class StringBuilderExtensions
    {
#if NET35
        public static void Clear(this StringBuilder stringBuilder)
        {
            stringBuilder.Remove(0, stringBuilder.Length);
        }
#endif
    }
}
