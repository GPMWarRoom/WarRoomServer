using System.ComponentModel.DataAnnotations;

namespace WarRoomServer.Data.Entities
{
    public class FieldInfo
    {
        [Key]
        public string ID { get; set; }
        public int Floor { get; set; }
        public string Name { get; set; }
        public string DataBaseName { get; set; }
        public string IP { get; set; } = "127.0.0.1";
        public int AGVSPort { get; set; } = 5216;
        public int VMSPort { get; set; } = 5036;
    }
}
