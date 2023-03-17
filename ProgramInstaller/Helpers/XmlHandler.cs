using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ProgramInstaller.Helpers; 
public static class XmlHandler {
    public static void GetDados(DataGrid dtProgramas, bool selectFirstIndex = false)
    {
        try {
            DataSet dsResultado = new();
            dsResultado.ReadXml(@"config\config.xml");
            if (dsResultado.Tables.Count != 0) {
                if (dsResultado.Tables[0].Rows.Count > 0) {
                    dtProgramas.ItemsSource = new DataView(dsResultado.Tables["Programa"]);
                }
            }

            if (selectFirstIndex)
                dtProgramas.SelectedIndex = 0;
        }
        catch (Exception ex) {
            throw;
        }
    }
}
