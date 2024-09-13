using Injection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class RoomSpawnerManager 
{
    [Inject] private RandomSelectedService _randomSelectedService;
    private  List<Room> _roomSpawners;
   
    public RoomSpawnerManager(List<Room> rooms)
    {
        _roomSpawners = new List<Room>();
        //List<Room> roomConfigs = LoadAllRoomConfig();
        if (rooms != null)
        {
            AddRoomConfig(rooms);
        }

    }
    public Room GetRoomConfig(int count)
    {
        List<Room> roomWithBlockCount = GetRoomWithMaxBlockCount(count);
        if (roomWithBlockCount.Count >0)
        {
            Room room = GetRoomByWeight(roomWithBlockCount);
            Debug.Log(room);
            return room;
        }
        return GetRoomWithMinBlockCount();
    }
    public void DecreaseRoomConfigWeight(Room roomConfig, int weight)
    {
        if (roomConfig == null) return;
        roomConfig.UpdateWeight(weight);
    }
    public void AddRoomConfig(List<Room> rooms)
    {
        foreach (var room in rooms)
        {
            //_roomWeights.Add(room, room.GetBlockCount);
            _roomSpawners.Add(room);
        }
    }
    public List<Room> GetRoomWithMaxBlockCount(int blockCount)
    {
        return _roomSpawners.Where(x => x.GetBlockCount <= blockCount).ToList();
    }
    public Room GetRoomWithMinBlockCount()
    {
        var _roomSpawnersSortBlockCount = _roomSpawners.OrderBy(x => x.GetBlockCount);
        return _roomSpawnersSortBlockCount.First();
    }
    public float GetWeightFromBlockCount(int blockCount)
    {
        switch (blockCount)
        {
            case 1:
                return 1.5f;
            case 2:
                return 2f;
            case 3:
                return 2.5f;
            case 4:
                return 3f;
            case 5:
                return 3.5f;
            case 6:
                return 4f;
            case 7:
                return 5.5f;
            default:
                return 1;
        }
    }
    public Room GetRoomByWeight(List<Room> rooms)
    {
        Dictionary<Room, double> _roomWeights = new Dictionary<Room, double>();
        foreach (var room in rooms)
        {
            int blockCount = room.GetBlockCount;
            //float weight = GetWeightFromBlockCount(blockCount);
            float weight = room.GetWeight;
            if (!_roomWeights.ContainsKey(room))
            {
                _roomWeights.Add(room, weight);
            }
        }
        return _randomSelectedService.SelectRandom<Room>(_roomWeights);

    }

    public Room LoadRoom(string codeName)
    {
        GameObject roomGO = Resources.Load<GameObject>($"Prefabs/Rooms/{codeName}");

        if (roomGO != null)
        {
            Room room = roomGO.GetComponent<Room>();

            return room;
        }
        return null;
    }
    public List<Room> LoadAllRoomConfig()
    {
        GameObject[] roomGOs = Resources.LoadAll<GameObject>($"Prefabs/Rooms");
        List<Room> roomConfigs = new List<Room>();
        foreach (var roomGO in roomGOs)
        {
            if (roomGO != null)
            {
                Room room = roomGO.GetComponent<Room>();
                if (room != null)
                {
                    roomConfigs.Add(room);
                }
               
            }
        }
        return roomConfigs;
    }
}
