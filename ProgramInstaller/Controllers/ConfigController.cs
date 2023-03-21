using ProgramInstaller.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;

namespace ProgramInstaller.Controllers;
public class ConfigController {
    const string path = "config";
    const string fileName = "config.xml";

    public List<Programa>? Load()
    {
        FileStream fs = new FileStream(fullPath, FileMode.OpenOrCreate);
        List<Programa> programas = new();

        try {
            XmlSerializer? ser = new XmlSerializer(typeof(List<Programa>));

            CheckDirectoryExistence(path);
            CheckFileExistence(fullPath);

            if (fs.Length > 0)
                programas = ser.Deserialize(fs) as List<Programa>;

            return programas;
        }
        catch (InvalidOperationException ex) {
            MessageBox.Show(ex.Message);
            return programas;
        }
        finally {
            fs.Close();
            CheckIfListIsEmptyToDeleteFile(programas);
        }
    }

    public void Save(List<Programa> programas)
    {
        if (File.Exists(fullPath))
            File.Delete(fullPath);

        XmlSerializer ser = new XmlSerializer(typeof(List<Programa>));
        FileStream fs = new FileStream(fullPath, FileMode.OpenOrCreate);
        ser.Serialize(fs, programas);
        fs.Close();

        CheckIfListIsEmptyToDeleteFile(programas);
    }

    void CheckDirectoryExistence(string directory)
    {
        if (Directory.Exists(directory))
            return;

        Directory.CreateDirectory(directory);
    }

    void CheckIfListIsEmptyToDeleteFile(List<Programa> programas)
    {
        if (!programas.Any() && File.Exists(fullPath)) {
            File.Delete(fullPath);
        }
    }

    void CheckFileExistence(string file)
    {
        if (!File.Exists(file))
            Save(new());
    }

    #region VALIDATIONS
    string fullPath => $@"{path}\{fileName}";
    #endregion
}
