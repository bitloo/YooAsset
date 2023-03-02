using System;
using System.IO;
using YooAsset;
using YooAsset.Editor;

/// <summary>
/// 不加密
/// </summary>
public class NoneEncryption : IEncryptionServices
{
    public EncryptResult Encrypt(EncryptFileInfo fileInfo)
    {
        EncryptResult result;
        result.LoadMethod = EBundleLoadMethod.Normal;
        result.EncryptedData = null;
        return result;
    }
}

/// <summary>
/// 偏移加密
/// </summary>
public class OffsetEncryption : IEncryptionServices
{
    public EncryptResult Encrypt(EncryptFileInfo fileInfo)
    {
        EncryptResult result;
        result.LoadMethod = EBundleLoadMethod.Normal;
        result.EncryptedData = null;
            
        byte[] fileData = File.ReadAllBytes(fileInfo.FilePath);
        if (!EditorTools.CheckBundleFileValid(fileData))
        {
            return result;
        }
            
        int offset = 32;
        result.EncryptedData = new byte[fileData.Length + offset];
        Buffer.BlockCopy(fileData, 0, result.EncryptedData, offset, fileData.Length);
        return result;
    }
}