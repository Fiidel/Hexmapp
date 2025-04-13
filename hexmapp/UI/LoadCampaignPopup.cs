using Godot;
using System;

public partial class LoadCampaignPopup : Panel
{
    private const string campaignButtonUid = "uid://b3rk0gac0ce02";
    private PackedScene campaignButtonScene;
    private VBoxContainer campaignsContainer;
    private const string campaignsButtonGroupUid = "uid://e4c8yockpkxv";
    private ButtonGroup campaignsButtonGroup;

    public override void _Ready()
    {
        campaignButtonScene = GD.Load<PackedScene>(campaignButtonUid);
        campaignsButtonGroup = GD.Load<ButtonGroup>(campaignsButtonGroupUid);
        campaignsContainer = GetNode<VBoxContainer>("%CampaignsContainer");
        VisibilityChanged += ReloadCampaigns;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        VisibilityChanged -= ReloadCampaigns;
    }


    private void ReloadCampaigns()
    {
        RemoveListedCampaigns();

        var campaigns = CampaignManager.Instance.ListCampaigns();
        foreach (var campaignName in campaigns)
        {
            GD.Print($"Campaign: {campaignName}");
            var campaignButton = (Button) campaignButtonScene.Instantiate();
            campaignButton.Text = campaignName;
            campaignButton.ButtonGroup = campaignsButtonGroup;
            campaignsContainer.AddChild(campaignButton);
        }
    }

    private void RemoveListedCampaigns()
    {
        foreach (var child in campaignsContainer.GetChildren())
        {
            child.QueueFree();
        }
    }

    private void OnLoadCampaignPopupOk()
    {
        var selectedButton = campaignsButtonGroup.GetPressedButton() as Button;
        if (selectedButton == null)
        {
            return;
        }

        CampaignManager.Instance.LoadCampaign(selectedButton.Text);
    }

    private void OnLoadCampaignPopupCancel()
    {
        Visible = false;
        RemoveListedCampaigns();
    }

    private void OnLoadCampaignPopupDelete()
    {
        var selectedButton = campaignsButtonGroup.GetPressedButton() as Button;
        if (selectedButton == null)
        {
            return;
        }

        var campaignName = selectedButton.Text;
        CampaignManager.Instance.DeleteCampaign(campaignName);
        ReloadCampaigns();
    }
}
