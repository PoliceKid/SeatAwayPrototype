using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public bool IsFirstOpen;
    public int CurrentLevel;
    public ArchitectureSaveGame ArchitectureSaveGame;
    public List<RoomSaveGame> RoomSaveGames;
 
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
    public RoomSaveGame()
    {
        Id = System.Guid.NewGuid().ToString();
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
