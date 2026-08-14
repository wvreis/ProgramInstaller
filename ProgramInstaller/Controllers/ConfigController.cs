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
            Programas programas;

            using (FileStream stream = File.OpenRead(FullPath))
            {
                if (stream.Length == 0)
                {
                    Programas defaults = DefaultProgramCatalog.Create();
                    stream.Close();
                    Save(defaults);
                    return defaults;
                }

                programas = Serializer.Deserialize(stream) as Programas ?? new Programas();
            }

            if (MigrateConfiguration(programas))
                Save(programas);

            return programas;
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
        MigrateConfiguration(programas);

        string temporaryPath = $"{FullPath}.tmp";
        XmlSerializerNamespaces namespaces = new();
        namespaces.Add(string.Empty, string.Empty);

        using (FileStream stream = File.Create(temporaryPath))
            Serializer.Serialize(stream, programas, namespaces);

        File.Move(temporaryPath, FullPath, true);
    }

    private static bool MigrateHashOverride(Programas programas)
    {
        bool changed = false;

        foreach (Programa programa in programas.ListaProgramas)
        {
            if (!IsWingetCommand(programa.Caminho) ||
                !WingetSecurityOptions.ContainsIgnoreSecurityHash(programa.Argumentos))
                continue;

            programa.PermitirHashDiferente = true;
            programa.Argumentos = WingetSecurityOptions.RemoveIgnoreSecurityHash(programa.Argumentos);
            changed = true;
        }

        return changed;
    }

    private static bool MigrateAvastPreset(Programas programas)
    {
        Programa? avast = programas.ListaProgramas.FirstOrDefault(
            programa => programa.Id == DefaultProgramCatalog.AvastProgramId);

        if (avast is null ||
            !IsWingetCommand(avast.Caminho) ||
            !avast.Argumentos.Equals(
                DefaultProgramCatalog.LegacyAvastInstallArguments,
                StringComparison.OrdinalIgnoreCase))
            return false;

        avast.Argumentos = DefaultProgramCatalog.AvastInstallArguments;
        return true;
    }

    private static bool MigrateConfiguration(Programas programas) =>
        MigrateHashOverride(programas) | MigrateAvastPreset(programas);

    private static bool IsWingetCommand(string command) =>
        command.Trim().Equals("winget", StringComparison.OrdinalIgnoreCase) ||
        command.Trim().Equals("winget.exe", StringComparison.OrdinalIgnoreCase);
}
