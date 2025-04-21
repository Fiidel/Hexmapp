using Godot;
using System;

public partial class CreateCampaignPopup : Panel
{
    // campaign name
    private LineEdit campaignNameInput;
    private RichTextLabel warningMessage;
    // player map size
    private SpinBox playerMapWidthInput;
    private SpinBox playerMapHeightInput;
    // months list
    private SpinBox numberOfMonthsInput;
    private VBoxContainer monthList;
    private PackedScene monthItemScene;
    private double numOfMonths;

    public override void _Ready()
    {
        // campaign name
        campaignNameInput = GetNode<LineEdit>("%CampaignNameInput");
        warningMessage = GetNode<RichTextLabel>("%CampaignNameInputWarning");

        // player map size
        playerMapWidthInput = GetNode<SpinBox>("%PlayerMapWidthInput");
        playerMapHeightInput = GetNode<SpinBox>("%PlayerMapHeightInput");

        // months list
        monthList = GetNode<VBoxContainer>("%MonthsContainer");
        monthItemScene = GD.Load<PackedScene>("uid://ndbh2bksl174");

        numberOfMonthsInput = GetNode<SpinBox>("%NumOfMonthsInput");
        numOfMonths = numberOfMonthsInput.Value;
        for (int i = 0; i < numOfMonths; i++)
        {
            monthList.AddChild(monthItemScene.Instantiate());
        }

        // register signals
        campaignNameInput.TextChanged += (_) => ResetInputColorAndWarning();
        numberOfMonthsInput.ValueChanged += OnNumberOfMonthsValueChanged;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        numberOfMonthsInput.ValueChanged -= OnNumberOfMonthsValueChanged;
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

    private void OnNumberOfMonthsValueChanged(double value)
    {
        if (numOfMonths > value)
        {
            var childCount = monthList.GetChildCount();
            for (int i = 0; i < numOfMonths - value; i++)
            {
                var lastItem = monthList.GetChild(childCount - 1);
                lastItem.QueueFree();
                childCount--;
            }
        }
        else
        {
            for (int i = 0; i < value - numOfMonths; i++)
            {
                monthList.AddChild(monthItemScene.Instantiate());
            }
        }
        
        numOfMonths = value;
    }

    public void OnDoneButtonPressed()
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
        var mapSize = new Vector2I((int)playerMapWidthInput.Value, (int)playerMapHeightInput.Value);
        var monthArray = new MonthEntity[(int)numOfMonths];
        var months = monthList.GetChildren();
        for (int i = 0; i < numOfMonths; i++)
        {
            var monthName = months[i].GetNode<LineEdit>("%MonthNameInput").Text;
            var numOfDays = months[i].GetNode<SpinBox>("%NumberOfDaysSpinbox").Value;
            monthArray[i] = new MonthEntity { Name = monthName, Days = (int)numOfDays };
        }
        var calendar = new Calendar { Months = monthArray };
        CampaignManager.Instance.CreateCampaign(campaignNameInput.Text, mapSize, calendar);
    }

    public void OnCancelButtonPressed()
    {
        ResetInputFieldToDefault();
        Visible = false;
    }
}
