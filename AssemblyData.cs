using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crusade
{
    public static class AssemblyData
    {
        public const string Description = 
            "A simple, fast tool for merging a list of images into a single file (zip/pdf/docx)";
        public const bool IsPrerelease = true;
        public const string Stage = "a";
        public const string Version = "0.2.0";
        public const string VersionName =
            (IsPrerelease ? "prerelease " : "") + "v" + Version + "-" + Stage;
    }
}
