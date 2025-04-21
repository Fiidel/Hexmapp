using Godot;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Text.Json;

public partial class CampaignManager : Node
{
    public static CampaignManager Instance { get; private set; }
    public string currentCampaignName { get; private set; }
    public Calendar currentCalendar { get; private set; }
    private readonly string campaignsDirectoryPath = ProjectSettings.GlobalizePath("user://Campaigns");

    public override void _Ready()
    {
        Instance = this;
    }

    public List<string> ListCampaigns()
    {
        var campaignNames = new List<string>();

        // if no Campaigns directory exists, return an empty list
        if (!Directory.Exists(campaignsDirectoryPath))
        {
            return campaignNames;
        }

        // else get the directory info, add each name of the campaigns to the list and return it
        DirectoryInfo directoryInfo = new DirectoryInfo(campaignsDirectoryPath);
        DirectoryInfo[] campaignDirectories = directoryInfo.GetDirectories();
        foreach (var campaignDirectory in campaignDirectories)
        {
            campaignNames.Add(campaignDirectory.Name);
        }

        return campaignNames;
    }


    public bool IsCampaignNameUnique(string campaignName)
    {
        if (ListCampaigns().Contains(campaignName))
        {
            return false;
        }

        return true;
    }


    public void CreateCampaign(string campaignName, Vector2I playerMapSize, Calendar calendar)
    {
        currentCampaignName = campaignName;

        // create the campaign directory
        var campaignDirectory = Directory.CreateDirectory(Path.Combine(campaignsDirectoryPath, campaignName));
        Directory.CreateDirectory(Path.Combine(campaignsDirectoryPath, campaignName, "hexnotes"));
        Directory.CreateDirectory(Path.Combine(campaignsDirectoryPath, campaignName, "playernotes"));

        // create an empty save file
        var savePath = Path.Combine(campaignsDirectoryPath, campaignName, "save.json");
        File.CreateText(savePath).Close();

        // pass the player map size input
        GameManager.Instance.PlayerMapSize = playerMapSize;

        // pass the calendar
        currentCalendar = calendar;

        // load in all the necessary game scenes
        GameManager.Instance.LoadScenesOnStartup();
    }


    public void LoadCampaign(string campaignName)
    {
        currentCampaignName = campaignName;

        // load in all the necessary game scenes
        GameManager.Instance.LoadScenesOnStartup();

        // load data for a saved campaign
        var savePath = Path.Combine(campaignsDirectoryPath, campaignName, "save.json");
        if (!Godot.FileAccess.FileExists(savePath))
        {
            GD.Print($"Save file not found.");
            return;
        }
        using var saveFile = Godot.FileAccess.Open(savePath, Godot.FileAccess.ModeFlags.Read);
        var jsonString = saveFile.GetAsText();
        var json = new Json();
        var parseResult = json.Parse(jsonString);

        if (parseResult != Error.Ok)
        {
            GD.Print($"Error in json parsing for the save file: {parseResult}");
            return;
        }

        try
        {
            var data = (Godot.Collections.Dictionary<string, Variant>) json.Data;
            GameManager.Instance.LoadCampaignData(data);
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error loading save file: {e.Message}");
            return;
        }
    }

    public void DeleteCampaign(string campaignName)
    {
        var path = Path.Combine(campaignsDirectoryPath, campaignName);
        if (!Directory.Exists(path))
        {
            GD.PrintErr($"Directory does not exist: {path}");
            return;
        }
        GD.Print($"Deleting directory: {path}");
        Directory.Delete(path, true);
    }
}
