using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class SaveGameData
{
    public bool IsFirstOpen;
    public int CurrentLevel;
    public int LaunchCount;
    public ArchitectureSaveGame ArchitectureSaveGame;
    public List<RoomSaveGame> RoomPlacedSaveGames;
    public List<RoomSaveGame> RoomSpawnerSaveGames;
    public List<GatewaySaveGame> GatewaySaveGames;
}
[System.Serializable]
public class GatewaySaveGame
{
    public string Id;
    public List<UnitSaveGame> UnitSaveGames;

    public void CloneDataFromOriginal(Gateway gateway)
    {
        Id = gateway.name;
    }
}

[System.Serializable]
public class ArchitectureSaveGame
{
    public List<CellSaveGame> CellSaves;

    public ArchitectureSaveGame()
    {
        CellSaves = new List<CellSaveGame>();
    }

    public CellSaveGame FindCell(string Id)
    {
        return CellSaves.FirstOrDefault(x => x.Id == Id);
    }
    public void SetBlockOccpier(string Id, BlockSaveGame blockSaveGame)
    {
        CellSaveGame cellSaveGame = FindCell(Id);
        if (cellSaveGame != null)
        {
            cellSaveGame.OccupierSaveGame = blockSaveGame;
        }
    }
    public void ClearCellOccpier(string Id)
    {
        CellSaveGame cellSaveGame = FindCell(Id);
        if (cellSaveGame != null)
        {
            cellSaveGame.ClearCellOccpier();
        }
    }
    public void AddCellSave(CellSaveGame cellSaveGame)
    {
        CellSaves.Add(cellSaveGame);
    }
}
[System.Serializable]
public class CellSaveGame
{
    public string Id;
    public OccupierSaveGame OccupierSaveGame;
    public CellSaveGame()
    {
        
    }
    public void SetBlockOccpier(OccupierSaveGame occupierSaveGame)
    {
        OccupierSaveGame = occupierSaveGame;
    }
    public void ClearCellOccpier()
    {
        OccupierSaveGame = null;
    }
}
[System.Serializable]
public class RoomSaveGame
{
    public string Id;
    public string PlaceableState;
    public List<BlockSaveGame> BlockSaves = new List<BlockSaveGame>();
    public float X, Z;
    public RoomSaveGame()
    {
        
    }
    public void CloneDataFromOriginal(Room room)
    {
        Id = room.GetId();
        X = room.transform.position.x;
        Z = room.transform.position.z;
        List<Block> blocks = room.GetBlocks;
        foreach (var block in blocks)
        {
            BlockSaveGame blockSaveGame = new BlockSaveGame();
            blockSaveGame.CloneFromOriginal(block);
            if (block.IsOccupier())
            {
                OccupierSaveGame occupierSaveGame = new OccupierSaveGame();
                blockSaveGame.SetBlockOccpier(occupierSaveGame);
            }
            AddBlockSave(blockSaveGame);

        }
    }
    public void AddBlockSave(BlockSaveGame blockSaveGame)
    {
        BlockSaves.Add(blockSaveGame);
    }
    public BlockSaveGame FindBlockSave(string Id)
    {
        return BlockSaves.FirstOrDefault(x => x.Id == Id);
    }
    public void SetBlockOccpier(string Id, OccupierSaveGame occupierSaveGame)
    {
        BlockSaveGame blockSaveGame = FindBlockSave(Id);
        if (blockSaveGame != null)
        {
            blockSaveGame.SetBlockOccpier(occupierSaveGame);
        }
    }
    public void ClearBlockOccpier(string Id)
    {
        BlockSaveGame blockSaveGame = FindBlockSave(Id);
        if (blockSaveGame != null)
        {
            blockSaveGame.SetBlockOccpier(null);
        }
    }
}
[System.Serializable]
public class BlockSaveGame : OccupierSaveGame
{
    public string Id;
    public string CodeName;
    public float DirectionY;
    public float X, Z;  
    public OccupierSaveGame OccupierSaveGame;
    public BlockSaveGame()
    {

    }
    public void CloneFromOriginal(Block block)
    {
        X = block.transform.localPosition.x;
        Z = block.transform.localPosition.z;
        Id = block.name;
        DirectionY = block.transform.localPosition.y;
        CodeName = block.GetCodeName();
    }
    public void ClearBlockOccpier()
    {
        OccupierSaveGame = null;
    }
    public void SetBlockOccpier(OccupierSaveGame OccupierSaveGame)
    {
        this.OccupierSaveGame = OccupierSaveGame;
    }
}
[System.Serializable]

public class UnitSaveGame : OccupierSaveGame
{
    public string Id;
    public string CodeName;
}
[System.Serializable]
public class OccupierSaveGame
{
}
public class BaseConfig
{
    public string CodeName;
}
