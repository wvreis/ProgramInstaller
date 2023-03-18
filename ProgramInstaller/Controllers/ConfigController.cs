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
    public List<Programa> Load()
    {
        try {
            XmlSerializer ser = new XmlSerializer(typeof(List<Programa>));
            FileStream fs = new FileStream(@"config\configN.xml", FileMode.OpenOrCreate);
            
            var programas = (List<Programa>)ser.Deserialize(fs);

            return programas;
        }
        catch (InvalidOperationException ex) {
            throw ex;
        }
    }

    public void Save(List<Programa> programas)
    {
        XmlSerializer ser = new XmlSerializer(typeof(List<Programa>));
        FileStream fs = new FileStream(@"config\configN.xml", FileMode.OpenOrCreate);
        ser.Serialize(fs, programas);
        fs.Close();
    }
}
