using Godot;
using System;

public partial class CreateCampaignPopup : Panel
{
    private LineEdit campaignNameInput;
    private RichTextLabel warningMessage;

    public override void _Ready()
    {
        campaignNameInput = GetNode<LineEdit>("%CampaignNameInput");
        warningMessage = GetNode<RichTextLabel>("%CampaignNameInputWarning");

        campaignNameInput.TextChanged += (_) => ResetInputColorAndWarning();
    }

    private void ResetInputColorAndWarning()
    {
        campaignNameInput.Modulate = new Color(1, 1, 1, 1);
        warningMessage.Visible = false;
    }

    private void ResetInputFieldToDefault()
    {
        campaignNameInput.Text = "";
        ResetInputColorAndWarning();
    }

    private void InvalidateInputField(string errorMessage)
    {
        campaignNameInput.Modulate = new Color(1, 0, 0, 1);
        warningMessage.Text = errorMessage;
        warningMessage.Visible = true;
    }

    public void OnCreateCampaignPopupOk()
    {
        // validate input
        if (campaignNameInput.Text == "")
        {
            InvalidateInputField("Campaign name cannot be blank.");
            return;
        }

        if (!CampaignManager.Instance.IsCampaignNameUnique(campaignNameInput.Text))
        {
            InvalidateInputField("Campaign name must be unique.");
            return;
        }

        // create campaign
        CampaignManager.Instance.LoadCampaign(campaignNameInput.Text, true);
    }

    public void OnCreateCampaignPopupCancel()
    {
        ResetInputFieldToDefault();
        Visible = false;
    }
}
