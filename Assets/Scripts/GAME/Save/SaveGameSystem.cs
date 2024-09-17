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
    private GameData _gameData;
    public GameData GetGameData => _gameData;
    public GameData Load()
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
        string content = Newtonsoft.Json.JsonConvert.SerializeObject(_gameData,formatting: Newtonsoft.Json.Formatting.Indented);

        File.WriteAllText(gameSavePath, content);
    }
    public GameData LoadGameData()
    {
        try
        {
            if (_gameData == null)
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
                        _gameData = Newtonsoft.Json.JsonConvert.DeserializeObject<GameData>(text);
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
        if (_gameData == null)
        {
            _gameData = new GameData();
        }
        return _gameData;
    }

    
    public bool IsFirstOpen()
    {
        return _gameData.IsFirstOpen;
    }

    #region ARCHITECTURE SAVE API
    public void SetBlockOccpier(string Id, BlockSaveGame blockSaveGame)
    {
        _gameData.ArchitectureSaveGame.SetBlockOccpier(Id, blockSaveGame);
    }
    public void ClearCellOccpier(string Id) {
        _gameData.ArchitectureSaveGame.ClearCellOccpier(Id);
    }
    public void AddRoomSave(RoomSaveGame roomSaveGame)
    {
        _gameData.RoomSaveGames.Add(roomSaveGame);
    }
    public BlockSaveGame GetBlockSave(string Id)
    {
        foreach (var roomSave in _gameData.RoomSaveGames)
        {
            BlockSaveGame blockSave = roomSave.FindBlockSave(Id);
            if(blockSave != null)
            {
                return blockSave;
            }
        }
        return null;
    }
    #endregion
}
