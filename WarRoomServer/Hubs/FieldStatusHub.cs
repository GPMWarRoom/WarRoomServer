using AGVSystemCommonNet6.Equipment;
using EquipmentManagment.MainEquipment;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using WarRoomServer.Data;
using WarRoomServer.Data.Contexts;
using WarRoomServer.Data.Entities;
using WarRoomServer.Services;
using WarRoomServer.View;

namespace WarRoomServer.Hubs
{
    public class FieldStatusHub : Hub
    {
        IMemoryCache _memoryCache;
        WarRoomDbContext _warRoomDB;
        public FieldStatusHub(IMemoryCache memoryCache, WarRoomDbContext warRoomDB) : base()
        {
            _memoryCache = memoryCache;
            _warRoomDB = warRoomDB;
        }


        public static ConcurrentDictionary<string, FieldStatusHubClientRequest> clientStates { get; set; } = new ConcurrentDictionary<string, FieldStatusHubClientRequest>();



        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();

            // 获取查询参数
            string floorStr = httpContext.Request.Query["floor"].ToString();
            string name = httpContext.Request.Query["name"].ToString();
            int.TryParse(floorStr, out int floor);
            IClientProxy _client = Clients.Client(Context.ConnectionId);

            FieldStatusHubClientRequest _clientState = new FieldStatusHubClientRequest()
            {
                connnectionID = Context.ConnectionId,
                cancelToskenSource = new CancellationTokenSource(),
                Floor = floor,
                FieldName = name,
                client = _client
            };
            clientStates.TryAdd(Context.ConnectionId, _clientState);
            await Task.Delay(500);
            _ = SendFieldDataSteady(_clientState);
            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            if (clientStates.TryRemove(Context.ConnectionId, out var _state))
                _state.cancelToskenSource.Cancel();
            return base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// 持續發送Field數據
        /// </summary>
        /// <param name="clientState"></param>
        /// <returns></returns>
        private async Task SendFieldDataSteady(FieldStatusHubClientRequest clientState)
        {
            FieldInfo field = _warRoomDB.Fields.FirstOrDefault(f => f.Floor == clientState.Floor && f.Name == clientState.FieldName);

            while (!clientState.cancelToskenSource.IsCancellationRequested)
            {
                FieldOverviewDataModel _data = FetchSpeficFieldData(clientState, field);
                await clientState.client.SendAsync("FieldStatusData", _data);
                try
                {
                    await Task.Delay(1000, clientState.cancelToskenSource.Token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        private FieldOverviewDataModel FetchSpeficFieldData(FieldStatusHubClientRequest clientState, FieldInfo field)
        {
            string key = $"{clientState.Floor}-{clientState.FieldName}";
            if (!_memoryCache.TryGetValue(key, out FieldOverviewDataModel data))
            {
                data = new FieldOverviewDataModel();
                data = QueryFromDataBase(clientState, field);
                _memoryCache.Set(key, data, TimeSpan.FromMilliseconds(400));
            }
            else
            {

            }
            return data;
        }

        private FieldOverviewDataModel QueryFromDataBase(FieldStatusHubClientRequest clientState, FieldInfo field)
        {
            try
            {
                if (field == null)
                    return new();
                using FieldDatabaseQuery databaseQuery = new FieldDatabaseQuery(field.DataBaseName);
                return databaseQuery.GetOverviewData();
            }
            catch (Exception ex)
            {
                return new();
            }
        }

        public class FieldStatusHubClientRequest : ClientRequestState
        {

            public int Floor { get; set; } = 3;
            public string FieldName { get; set; } = "AOI";
        }

    }
}
