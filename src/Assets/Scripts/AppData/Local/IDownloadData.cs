﻿namespace PatchKit.Unity.Patcher.AppData.Local
{
    public interface IDownloadData
    {
        string GetFilePath(string fileName);

        string GetContentPackagePath(int versionId);

        string GetDiffPackagePath(int versionId);

        void Clear();
    }
}