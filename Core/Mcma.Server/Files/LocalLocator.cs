using Mcma.Core.Model;

namespace Mcma.Server.Files
{
    public class LocalLocator : Locator
    {
        /// <summary>
        /// Gets or sets the file path
        /// </summary>
        public string FolderPath { get; set; }

        /// <summary>
        /// Gets or sets the name of the file
        /// </summary>
        public string FileName { get; set; }
    }
}