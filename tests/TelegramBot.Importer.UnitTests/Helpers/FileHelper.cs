using System.Data;

namespace TelegramBot.Importer.UnitTests.Helpers;

public static class FileHelper
{
    public static string GetFilePath(string relativeFilePath)
    {
        return Directory.GetCurrentDirectory() + relativeFilePath;
    }

    public static DataSet LoadDataSet(string relativeFilePath)
    {
        var dataSet = new DataSet();
        dataSet.ReadXml(GetFilePath(relativeFilePath));

        return dataSet;
    }
}