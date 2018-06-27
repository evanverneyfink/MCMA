using Mcma.Core.Model;

namespace Mcma.Extensions.Files.AzureFileStorage
{
    public class AzureFileStorageLocator : Locator
    {
        /// <summary>
        /// Gets or sets the share on which the file resides
        /// </summary>
        public string Share { get; set; }

        /// <summary>
        /// Gets or sets the directory in which the file resides
        /// </summary>
        public string Directory { get; set; }

        /// <summary>
        /// Gets or sets the name of the file
        /// </summary>
        public string FileName { get; set; }
    }
}