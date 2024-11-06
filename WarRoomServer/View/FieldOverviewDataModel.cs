using AGVSystemCommonNet6.AGVDispatch;
using static AGVSystemCommonNet6.clsEnums;

namespace WarRoomServer.View
{
    public class FieldOverviewDataModel
    {
        public string Name { get; set; } = "FieldOverviewDataModel";

        public AGVSystemCommonNet6.Sys.AGVSSystemStatus sysOperateStatus { get; set; } = new();

        public List<EquipmentStatus> equipmentStatus { get; set; } = new List<EquipmentStatus>();
        public List<clsTaskDto> tasks { get; internal set; }

        public List<AGVStatus> agvStatus { get; set; } = new List<AGVStatus>();

        public class EquipmentStatus
        {
            public enum EQ_STATUS
            {
                RUN, DOWN, IDLE
            }

            public string Name { get; set; } = "";
            public string EqType { get; set; } = "AGV";

            public bool Connected { get; set; } = false;

            public EQ_STATUS Status { get; set; } = EQ_STATUS.DOWN;


            public static EQ_STATUS ConvertAGVSStatusToEQSTATUS(MAIN_STATUS agvStatus)
            {
                switch (agvStatus)
                {
                    case MAIN_STATUS.IDLE:
                        return EQ_STATUS.IDLE;
                    case MAIN_STATUS.RUN:
                        return EQ_STATUS.RUN;
                    case MAIN_STATUS.DOWN:
                        return EQ_STATUS.DOWN;
                    case MAIN_STATUS.Charging:
                        return EQ_STATUS.RUN;
                    case MAIN_STATUS.Unknown:
                        return EQ_STATUS.DOWN;
                    default:
                        return EQ_STATUS.DOWN;
                }
            }
        }


        public class AGVStatus
        {
            public string name { get; set; } = "";
            public double[] coordination { get; set; } = new double[2];
            public List<int> path { get; set; } = new List<int>();
            public AGVCargoStatus cargoStatus { get; set; } = new AGVCargoStatus();
            public class AGVCargoStatus
            {
                public bool hasCargo { get; set; } = false;
                public string cargoType { get; set; } = "Tray";
                public string id { get; set; } = "";
            }
            //        {
            //      name: 'AGV_001',
            //      coordination: [-1.96, -5.674],
            //      path: [],
            //      cargoStatus: {
            //        hasCargo: true,
            //        cargoType: 'Tray',
            //        id: 'TAE1423299'
            //      }
            //}

        }
    }

}
