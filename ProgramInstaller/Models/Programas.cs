using System.Collections.Generic;
using System.Xml.Serialization;

namespace ProgramInstaller.Models;

[XmlRoot("Programas")]
public class Programas {
    [XmlElement("Programa")]
    public List<Programa> ListaProgramas { get; set; }

    public Programas()
    {
        ListaProgramas = new();
    }
}
