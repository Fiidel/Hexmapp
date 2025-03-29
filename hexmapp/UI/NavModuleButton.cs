using Godot;
using System;

public partial class NavModuleButton : TextureButton
{
    [Export] private string moduleName;
    private NinePatchRect background;
    private Label label;

    public override void _Ready()
    {
        background = GetNode<NinePatchRect>("LabelBackground");
        label = GetNode<Label>("LabelBackground/ModuleName");
        
        label.Text = moduleName;
        background.Size = new Vector2(label.GetThemeFont("font").GetStringSize(label.Text, HorizontalAlignment.Left, -1, label.GetThemeFontSize("font_size")).X + 125, background.Size.Y);

        background.Visible = false;

        MouseEntered += OnButtonEnterHover;
        MouseExited += OnButtonExitHover;
    }


    public override void _ExitTree()
    {
        MouseEntered -= OnButtonEnterHover;
        MouseExited -= OnButtonExitHover;
    }


    private void OnButtonEnterHover()
    {
        Modulate = new Color(1.2f, 1.2f, 1.2f, 1);
        background.Visible = true;
    }


    private void OnButtonExitHover()
    {
        Modulate = new Color(1, 1, 1, 1);
        background.Visible = false;
    }


    public void OnHostButtonPressed()
    {
        WsClient.Instance.HostGame();
    }
}
