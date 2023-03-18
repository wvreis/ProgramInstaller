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
    public Programas Load()
    {
        try {
            XmlSerializer ser = new XmlSerializer(typeof(Programas));
            FileStream fs = new FileStream(@"config\configN.xml", FileMode.OpenOrCreate);
            
            var programas = (Programas)ser.Deserialize(fs);

            return programas;
        }
        catch (InvalidOperationException ex) {
            throw ex;
        }
    }

    public void Save(Programas programas)
    {
        XmlSerializer ser = new XmlSerializer(typeof(Programas));
        FileStream fs = new FileStream(@"config\configN.xml", FileMode.OpenOrCreate);
        ser.Serialize(fs, programas);
        fs.Close();
    }
}
