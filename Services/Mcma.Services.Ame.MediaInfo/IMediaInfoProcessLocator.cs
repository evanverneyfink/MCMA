namespace Mcma.Services.Ame.MediaInfo
{
    public interface IMediaInfoProcessLocator
    {
        /// <summary>
        /// Gets the path to the media info process
        /// </summary>
        /// <returns></returns>
        string GetMediaInfoLocation();
    }
}