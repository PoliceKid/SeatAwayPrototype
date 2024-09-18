using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Logging;
using System;
using UnityEngine.Playables;
using System.IO;
using System.Xml;
public class SaveGameSystem 
{
    private static string _gameSaveFilename = "/youngster-save.json";
    private SaveGameData _saveGameData;
    public SaveGameData GetGameData => _saveGameData;
    public SaveGameData Load()
    {
        var gameModel = LoadGameData();
        return gameModel;
    }
    public static string GetGameSavePath()
    {
        return $"{Application.persistentDataPath}{_gameSaveFilename}";
    }
    public void SaveGameData()
    {
        string gameSavePath = GetGameSavePath();
        ShazamLogger.LogTemporaryChannel("SaveGame", $"Save Game Success : {gameSavePath}");
        string content = Newtonsoft.Json.JsonConvert.SerializeObject(_saveGameData,formatting: Newtonsoft.Json.Formatting.Indented);

        File.WriteAllText(gameSavePath, content);
    }
    public SaveGameData LoadGameData()
    {
        try
        {
            if (_saveGameData == null)
            {
                string gameSavePath = GetGameSavePath();

                if (File.Exists(gameSavePath))
                {
#if UNITY_EDITOR
                    //make a copy of the save file for reproducibility for testing purposes
                    string copyFile = $"{Application.persistentDataPath}/youngster-save_copy.json";
                    File.Copy(gameSavePath, copyFile, true);

#endif
                    ShazamLogger.Log($"Loading game save : {gameSavePath}");

                    var bytes = File.ReadAllBytes(gameSavePath);
                    string text = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);

                    try
                    {
                        #if SAVEGAME
                        _saveGameData = Newtonsoft.Json.JsonConvert.DeserializeObject<SaveGameData>(text);
                        #endif
                    }
                    catch (Exception e)
                    {
                        ShazamLogger.Error($">>> Parse GameSave error: {e.Message}");
                    }
                   
                }
                else
                {
                    ShazamLogger.Log("Game save not found, starting a new game!");
                }
            }
        }
        catch (System.Exception ex)
        {
            ShazamLogger.Error($"Failed to load saved game due to: {ex}");
        }
        if (_saveGameData == null)
        {
            _saveGameData = new SaveGameData();
        }
        return _saveGameData;
    }

    
    public bool IsFirstOpen()
    {
        return _saveGameData.IsFirstOpen;
    }

    #region ARCHITECTURE SAVE API
    public void SetBlockOccpier(string Id, BlockSaveGame blockSaveGame)
    {
        _saveGameData.ArchitectureSaveGame.SetBlockOccpier(Id, blockSaveGame);
    }
    public void ClearCellOccpier(string Id) {
        _saveGameData.ArchitectureSaveGame.ClearCellOccpier(Id);
    }
    public void AddRoomSave(RoomSaveGame roomSaveGame)
    {
        if (roomSaveGame == null) return;
        RoomSaveGame existedRoomSave = FindRoomSaveGame(roomSaveGame.Id);
        if (existedRoomSave != null) return;
        _saveGameData.RoomPlacedSaveGames.Add(roomSaveGame);
    }
    public RoomSaveGame FindRoomSaveGame(string id)
    {
        return _saveGameData.RoomPlacedSaveGames.FirstOrDefault(x => x.Id == id);
    }
    public BlockSaveGame GetBlockSave(string Id)
    {
        foreach (var roomSave in _saveGameData.RoomPlacedSaveGames)
        {
            BlockSaveGame blockSave = roomSave.FindBlockSave(Id);
            if(blockSave != null)
            {
                return blockSave;
            }
        }
        return null;
    }

    public void SetLaunchCount(int lauchCountSave)
    {
        _saveGameData.LaunchCount = lauchCountSave;
    }
    #endregion
}
