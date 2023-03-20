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

    public Programas? Load()
    {
        try {
            XmlSerializer? ser = new XmlSerializer(typeof(Programas));
            
            CheckDirectoryExistence(path);
            CheckFileExistence(fullPath);

            FileStream fs = new FileStream(fullPath, FileMode.OpenOrCreate);
            
            var programas = ser.Deserialize(fs) as Programas;

            return programas;
        }
        catch (InvalidOperationException ex) {
            throw ex;
        }
    }

    public void Save(Programas programas)
    {
        XmlSerializer ser = new XmlSerializer(typeof(Programas));
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
