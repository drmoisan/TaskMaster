using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("SVGControl.Test")]
namespace SVGControl
{
    internal static class RelativePath
    {
     
        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="anchorPath">Contains the directory that defines the start of the relative path.</param>
        /// <param name="pathToMakeRelative">Contains the path that defines the endpoint of the relative path.</param>
        /// <returns>The relative path from the start directory to the end path or <c>pathToMakeRelative</c> if the paths are not related.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="UriFormatException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static string MakeRelativePath(this String pathToMakeRelative, String anchorPath)
        {
            if (String.IsNullOrEmpty(anchorPath)) throw new ArgumentNullException("anchorPath");
            if (String.IsNullOrEmpty(pathToMakeRelative)) throw new ArgumentNullException("pathToMakeRelative");

            Uri fromUri = new Uri(anchorPath);
            Uri toUri = new Uri(pathToMakeRelative);

            if (fromUri.Scheme != toUri.Scheme) { return pathToMakeRelative; } // path can't be made relative.

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

        public static string GetRelativeURI(this String pathToMakeRelative, String anchorPath)
        {
            if (String.IsNullOrEmpty(anchorPath)) throw new ArgumentNullException("anchorPath");
            if (String.IsNullOrEmpty(pathToMakeRelative)) throw new ArgumentNullException("pathToMakeRelative");

            Uri fromUri = new Uri(anchorPath);
            Uri toUri = new Uri(pathToMakeRelative);

            if (fromUri.Scheme != toUri.Scheme) { return pathToMakeRelative; } // path can't be made relative.

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            String relativePath = Uri.UnescapeDataString(relativeUri.ToString());
            if (!relativePath[0].Equals('.'))
                relativePath = "./" + relativePath;

            return relativePath;
        }

        public static string AbsoluteFromPath(this String pathToMakeAbsolute, String anchorPath)
        {
            if (String.IsNullOrEmpty(anchorPath)) throw new ArgumentNullException("anchorPath");
            if (String.IsNullOrEmpty(pathToMakeAbsolute)) throw new ArgumentNullException("pathToMakeAbsolute");

            anchorPath = NormalizeFolderpath(anchorPath);
            string absolutePath = Path.GetFullPath(anchorPath + pathToMakeAbsolute);

            return absolutePath;
        }

        public static string AbsoluteFromURI(this string uriToMakeAbsolute, string anchorPath)
        {
            if (uriToMakeAbsolute.StartsWith("./"))
                uriToMakeAbsolute = uriToMakeAbsolute.Substring(2);
            anchorPath = NormalizeFolderpath(anchorPath);

            string relativePath = uriToMakeAbsolute.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            string absolutePath = Path.GetFullPath(anchorPath + relativePath);
            return absolutePath;
        }

        static public string NormalizeFolderpath(string filepath)
        {
            //string result = System.IO.Path.GetFullPath(filepath).ToLowerInvariant();
            string result = filepath;

            if (Path.GetExtension(result)=="")
            {
                result = result.TrimEnd(new[] { '\\' });
                result += '\\';
            }
            else
            {
                result = Path.GetDirectoryName(result) + '\\';
            }
            return result;
        }
    }
}
