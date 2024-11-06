using AGVSystemCommonNet6;
using AGVSystemCommonNet6.AGVDispatch;
using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.Equipment.AGV;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using WarRoomServer.View;
using static AGVSystemCommonNet6.clsEnums;
using static WarRoomServer.View.FieldOverviewDataModel;

namespace WarRoomServer.Data
{
    public class FieldDatabaseQuery : IDisposable
    {
        public string DataBaseName { get; }

        private readonly AGVSDbContext _context;
        private bool disposedValue;

        public FieldDatabaseQuery(string dataBaseName)
        {
            DataBaseName = dataBaseName;
            string _connectstring = $"Server=localhost;Database={dataBaseName};User Id=sa;Password=12345678;";
            var optionsBuilder = new DbContextOptionsBuilder<AGVSDbContext>();
            optionsBuilder.UseSqlServer(_connectstring);
            _context = new AGVSDbContext(optionsBuilder.Options, true);
        }
        internal FieldOverviewDataModel GetOverviewData()
        {
            var _agvStates = _context.AgvStates.AsNoTracking().ToList();
            AGVSystemCommonNet6.Sys.AGVSSystemStatus? _sysStatus = _context.SysStatus.AsNoTracking().FirstOrDefault();
            List<clsTaskDto> _tasks = CollectTasks(_context);
            List<EquipmentStatus> _equipmentStatus = CollectEquipmentStatus(_context);
            List<FieldOverviewDataModel.AGVStatus> _agvStatus = CollectAGVStatus(_context);

            return new FieldOverviewDataModel
            {
                sysOperateStatus = _sysStatus,
                equipmentStatus = _equipmentStatus,
                tasks = _tasks,
                agvStatus = _agvStatus
            };
        }

        private List<FieldOverviewDataModel.AGVStatus> CollectAGVStatus(AGVSDbContext context)
        {
            var agvStates = context.AgvStates.AsNoTracking().ToList();
            return agvStates.Select(agv => GetAGVStateData(agv, context)).ToList();

        }
        static FieldOverviewDataModel.AGVStatus GetAGVStateData(clsAGVStateDto agvState, AGVSDbContext context)
        {
            FieldOverviewDataModel.AGVStatus state = new FieldOverviewDataModel.AGVStatus
            {
                name = agvState.AGV_Name,
                cargoStatus = new FieldOverviewDataModel.AGVStatus.AGVCargoStatus
                {
                    hasCargo = agvState.CargoStatus == 1,
                    cargoType = agvState.CargoType == 200 ? "Tray" : "Rack",
                    id = agvState.CurrentCarrierID
                }
            };
            AGVSystemCommonNet6.Equipment.AGV.AGVStatus? agvRealTimeStates = context.EQStatus_AGV.AsNoTracking().FirstOrDefault(item => item.Name == agvState.AGV_Name);
            if (agvRealTimeStates != null)
            {
                state.coordination = new double[] { agvRealTimeStates.CoordinateX, agvRealTimeStates.CoordinateY };
                state.path = string.IsNullOrEmpty(agvRealTimeStates.CurrentPathTag) ? new List<int>() { agvRealTimeStates.Tag } : agvRealTimeStates.CurrentPathTag.Split('-').Select(int.Parse).ToList();
            }

            return state;
        }
        private List<clsTaskDto> CollectTasks(AGVSDbContext context)
        {
            return context.Tasks.AsNoTracking().OrderByDescending(_task => _task.RecieveTime).Take(30).ToList();
        }

        private List<FieldOverviewDataModel.EquipmentStatus> CollectEquipmentStatus(AGVSDbContext context)
        {
            List<FieldOverviewDataModel.EquipmentStatus> equipmentStatuses = new List<FieldOverviewDataModel.EquipmentStatus>();

            List<FieldOverviewDataModel.EquipmentStatus> agvStatusCollection = context.AgvStates.AsNoTracking().Select(v => new FieldOverviewDataModel.EquipmentStatus
            {
                Connected = v.Connected,
                EqType = "AGV",
                Name = v.AGV_Name,
                Status = FieldOverviewDataModel.EquipmentStatus.ConvertAGVSStatusToEQSTATUS(v.MainStatus)
            }).ToList();

            equipmentStatuses.AddRange(agvStatusCollection);

            return equipmentStatuses;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 處置受控狀態 (受控物件)
                }
                _context?.Dispose();
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    }
}
