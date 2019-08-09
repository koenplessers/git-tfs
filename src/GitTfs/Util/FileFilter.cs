using GitTfs.Core.TfsInterop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitTfs.Util
{
    /// <summary>
    /// FileFilter is more efficient than gitIgnore as it prevents fetching from tfs and if all changes in a changeset are filtered we can stkip this altogether
    /// </summary>
    public class FileFilter
    {
        /// <summary>
        /// Filter on file extension: by default pdb files should not be in source control
        /// </summary>
        public string[] ExtensionsToFilter
        {
            get;
            set;
        } = { ".pdb" };

        /// <summary>
        /// If specified matches the start of the path and only includes if no match
        /// </summary>
        public List<string> BlackListPaths
        {
            get;
            set;
        }

        /// <summary>
        /// If specified matches the start of the path and only includes if there is a match
        /// </summary>
        public List<string> WhiteListPaths
        {
            get;
            set;
        }

        /// <summary>
        /// Avoid problems uploading to github by expluding very large files
        /// </summary>
        public int MaxFileSize
        {
            get;
            set;
        } = 50 * 1024 * 1024;

        public bool IncludeItem (IChange change)
        {
            if (ExtensionsToFilter.Any (e => change.Item.ServerItem.EndsWith (e, StringComparison.InvariantCultureIgnoreCase)))
                return false;
            if (WhiteListPaths != null && ! WhiteListPaths.Any (p => change.Item.ServerItem.StartsWith (p, StringComparison.InvariantCultureIgnoreCase)))
                return false;
            if (BlackListPaths != null && BlackListPaths.Any (p => change.Item.ServerItem.StartsWith (p, StringComparison.InvariantCultureIgnoreCase)))
                return false;

            if (change.Item.ContentLength > MaxFileSize)
            {
                Trace.TraceWarning ($"Filtering {change.Item.ServerItem} at version {change.Item.ChangesetId} because too large with {change.Item.ContentLength} bytes");
                return false;
            }

            return true;
        }
    }
}
