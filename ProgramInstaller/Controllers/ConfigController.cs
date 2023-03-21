using ProgramInstaller.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ProgramInstaller.Controllers; 
public class ConfigController  {
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
            throw ex;
        }
        finally {
            if (!programas.Any()) {
                fs.Close();
                File.Delete(fullPath);
            }
            else {
                fs.Close();
            }
        }
    }

    public void Save(List<Programa> programas)
    {
        XmlSerializer ser = new XmlSerializer(typeof(List<Programa>));
        FileStream fs = new FileStream(fullPath, FileMode.OpenOrCreate);
        ser.Serialize(fs, programas);
        fs.Close();
    }

    void CheckDirectoryExistence(string directory)
    {
        if (Directory.Exists(directory))
            return;

        Directory.CreateDirectory(directory);
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
