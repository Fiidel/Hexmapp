using Godot;
using System;

public partial class MainMenu : Control
{
    private void OnCreateCampaignButtonPressed()
    {
        GameManager.Instance.LoadCampaign();
    }
}
