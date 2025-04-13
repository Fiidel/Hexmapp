using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public partial class CampaignManager : Node
{
    public static CampaignManager Instance { get; private set; }
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


    public void LoadCampaign(string campaignName, bool newCampaign = false)
    {
        if (newCampaign)
        {
            var campaignDirectory = Directory.CreateDirectory(Path.Combine(campaignsDirectoryPath, campaignName));
            Directory.CreateDirectory(Path.Combine(campaignsDirectoryPath, campaignName, "hexnotes"));
            Directory.CreateDirectory(Path.Combine(campaignsDirectoryPath, campaignName, "playernotes"));

            // create the save file
            var saveDictionary = new Godot.Collections.Dictionary<string, Variant>
            {
                {"hexMap", new Godot.Collections.Dictionary<string, Variant>()},
                {"playerMap", new Godot.Collections.Dictionary<string, Variant>()},
                {"timeline", new Godot.Collections.Dictionary<string, Variant>()}
            };

            string jsonString = JsonSerializer.Serialize(saveDictionary);
            File.WriteAllText(Path.Combine(campaignsDirectoryPath, campaignName, "save.json"), jsonString);
        }

        // load in all the necessary game scenes
        GameManager.Instance.LoadScenesOnStartup();

        // load data for saved campaigns
        if (!newCampaign)
        {
            // TODO: load all campaign data
        }
    }
}
