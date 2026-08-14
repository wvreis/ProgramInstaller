using ProgramInstaller.Models;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;

namespace ProgramInstaller.Controllers;

public sealed class ConfigController
{
    private const string ConfigDirectory = "config";
    private const string ConfigFileName = "config.xml";
    private static readonly XmlSerializer Serializer = new(typeof(Programas));

    private string FullPath => Path.Combine(ConfigDirectory, ConfigFileName);

    public Programas Load()
    {
        Directory.CreateDirectory(ConfigDirectory);

        if (!File.Exists(FullPath))
        {
            Programas defaults = DefaultProgramCatalog.Create();
            Save(defaults);
            return defaults;
        }

        try
        {
            using FileStream stream = File.OpenRead(FullPath);

            if (stream.Length == 0)
            {
                Programas defaults = DefaultProgramCatalog.Create();
                stream.Close();
                Save(defaults);
                return defaults;
            }

            return Serializer.Deserialize(stream) as Programas ?? new Programas();
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(
                $"{ex.Message}\nNão foi possível ler as configurações. O arquivo atual foi preservado.",
                "Configuração inválida",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return new Programas();
        }
    }

    public void Save(Programas programas)
    {
        Directory.CreateDirectory(ConfigDirectory);

        string temporaryPath = $"{FullPath}.tmp";
        XmlSerializerNamespaces namespaces = new();
        namespaces.Add(string.Empty, string.Empty);

        using (FileStream stream = File.Create(temporaryPath))
            Serializer.Serialize(stream, programas, namespaces);

        File.Move(temporaryPath, FullPath, true);
    }
}
