namespace Mcma.Services.Ame.MediaInfo
{
    public interface IMediaInfoOutputConverter
    {
        /// <summary>
        /// Converts MediaInfo output to JSON
        /// </summary>
        /// <param name="stdOut"></param>
        /// <returns></returns>
        string GetJson(string stdOut);
    }
}