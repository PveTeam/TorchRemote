using System.IO;
using System.IO.Compression;
using System.Xml.Serialization;
using Torch;

namespace TorchRemote.Plugin.Utils;

public static class PluginManifestUtils
{
    private static readonly XmlSerializer Serializer = new(typeof(PluginManifest));
    
    public static PluginManifest Read(Stream stream)
    {
        return (PluginManifest)Serializer.Deserialize(stream);
    }

    public static PluginManifest ReadFromZip(Stream stream)
    {
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, true);
        using var entryStream = archive.GetEntry("manifest.xml")!.Open();
        return Read(entryStream);
    }

    public static PluginManifest ReadFromZip(string archivePath)
    {
        using var stream = File.OpenRead(archivePath);
        return ReadFromZip(stream);
    }
}