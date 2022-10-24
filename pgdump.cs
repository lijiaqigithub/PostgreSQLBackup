using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostgreSQLBackup
{
    public class PgDump
    {
        public string pgDumpPath { get; set; }
        public string password { get; set; }
        public string host { get; set; }
        public string port { get; set; }
        public string username { get; set; }
        public string path { get; set; }
        public string dbname { get; set; }
    }
}
