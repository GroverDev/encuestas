using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comun.BaseDatos
{
    public class Enumeradores
    {
        public enum GestorDB
        {
            SQLSERVER = 0,
            POSTGRESQL = 1
        }

        public enum Sistema
        {
            SEGURIDAD,
            RRHH,
            ADMINISTRATIVO,
            SAMI,
            SAFI,
            RECURSOSCOMUNES,
            TICKETS,
            ENCUESTA,
            ASISTENCIA,
            CRM
        }
        public enum BaseDeDatos
        {
            LOCAL,
            FOTOS,
            AUD,
            EST,
            REPORTES
        }

        public enum Regional
        {
            NACIONAL = 0,
            LAPAZ = 1,
            COCHABAMBA = 2,
            SANTACRUZ = 3,
            ORURO = 4,
            POTOSI = 5,
            SUCRE = 6,
            TARIJA = 7,
            TRINIDAD = 8,
            COBIJA = 9,
            NINGUNO = -1
        }
    }
}
