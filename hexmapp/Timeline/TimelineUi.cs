using Godot;
using System;

public partial class TimelineUi : CanvasLayer
{
    private Calendar currentCalendar;
    private PanelContainer setupContainer;
    private PanelContainer calendarContainer;
    private Button modeButton;
    private Label dateLabel;
    private Label yearLabel;
    private Label timeLabel;
    private SpinBox timeSkipInput;
    private SpinBox yearSetupInput;
    private CenterContainer monthDaysContainer;
    private Godot.Collections.Array dayButtonsForMonth;
    private ButtonGroup calendarDaysButtonGroup;
    private DateTimeCustom currentDateTime;
    private Label monthYearLabel;
    private int currentlyViewedYear;
    private int currentlyViewedMonth;

    public override void _Ready()
    {
        setupContainer = GetNode<PanelContainer>("%SetupContainer");
        calendarContainer = GetNode<PanelContainer>("%CalendarContainer");
        modeButton = GetNode<Button>("%ModeButton");
        dateLabel = GetNode<Label>("%DateLabel");
        yearLabel = GetNode<Label>("%YearLabel");
        timeLabel = GetNode<Label>("%TimeLabel");
        timeSkipInput = GetNode<SpinBox>("%TimeSkipInput");
        yearSetupInput = GetNode<SpinBox>("%YearSetupInput");
        yearSetupInput.MaxValue = float.MaxValue;
        monthDaysContainer = GetNode<CenterContainer>("%MonthDaysContainer");
        calendarDaysButtonGroup = GD.Load<ButtonGroup>("uid://bh8lus6ojqwn1");
        monthYearLabel = GetNode<Label>("%MonthYearLabel");

        // hide the popup UIs
        setupContainer.Visible = false;
        calendarContainer.Visible = false;

        // if campaignmanager calendar isn't null, that means the campaign has just been created
        if (CampaignManager.Instance.CurrentCalendar != null)
        {
            currentCalendar = CampaignManager.Instance.CurrentCalendar;
            currentDateTime = new DateTimeCustom(currentCalendar);
            CreateCalendarDaysInMonthViews();
        }
    }

    private void ToggleTimeSkipPopup()
    {
        setupContainer.Visible = !setupContainer.Visible;
    }

    private void ToggleCalendarPopup()
    {
        calendarContainer.Visible = !calendarContainer.Visible;
    }

    public Godot.Collections.Dictionary<string, Variant> Save()
    {
        var calendar = CampaignManager.Instance.CurrentCalendar ?? currentCalendar;

        var saveData = new Godot.Collections.Dictionary<string, Variant>();

        // calendar data
        var monthsData = new Godot.Collections.Array();
        foreach (var month in calendar.Months)
        {
            var data = new Godot.Collections.Dictionary<string, Variant>();
            data["name"] = month.Name;
            data["days"] = month.Days;
            monthsData.Add(data);
        }
        saveData["months"] = monthsData;

        // current datetime
        var currentDateTimeData = new Godot.Collections.Dictionary<string, Variant>();
        currentDateTimeData["year"] = currentDateTime.currentYear;
        currentDateTimeData["month_index"] = currentDateTime.currentMonthIndex;
        currentDateTimeData["day"] = currentDateTime.currentDay;
        currentDateTimeData["time_minutes"] = currentDateTime.currentTime_minutes;
        saveData["currentDateTime"] = currentDateTimeData;

        return saveData;
    }

    public void Load(Godot.Collections.Dictionary<string, Variant> data)
    {
        // load the calendar from the stored data
        var monthsData = (Godot.Collections.Array) data["months"];
        var currentDateTimeData = (Godot.Collections.Dictionary<string, Variant>) data["currentDateTime"];
        
        var calendar = new Calendar();
        var monthArray = new Month[monthsData.Count];
        for (int i = 0; i < monthsData.Count; i++)
        {
            var monthData = (Godot.Collections.Dictionary<string, Variant>) monthsData[i];
            var month = new Month() {
                Name = (string) monthData["name"],
                Days = (int) monthData["days"]
            };
            monthArray[i] = month;
        }
        calendar.Months = monthArray;
        currentCalendar = calendar;

        var currentYear = (int) currentDateTimeData["year"];
        var currentMonthIndex = (int) currentDateTimeData["month_index"];
        var currentDay = (int) currentDateTimeData["day"];
        var currentTime_minutes = (int) currentDateTimeData["time_minutes"];
        currentDateTime = new DateTimeCustom(currentCalendar) {
            currentYear = currentYear,
            currentMonthIndex = currentMonthIndex,
            currentDay = currentDay,
            currentTime_minutes = currentTime_minutes
        };

        // log the current year
        currentlyViewedYear = currentYear;

        // make the calendar month days
        CreateCalendarDaysInMonthViews();
        ToggleMonthCalendarView(currentMonthIndex);

        // update the UI
        UpdateCurrentDateLabelsAndUi();
    }

    private void CreateCalendarDaysInMonthViews()
    {
        // create the grid containers for each month
        dayButtonsForMonth = new Godot.Collections.Array();
        var dayButtonScene = GD.Load<PackedScene>("uid://d3cu22irrhhuq");
        foreach (var month in currentCalendar.Months)
        {
            var daysGridContainer = new GridContainer();
            daysGridContainer.Columns = 7;
            daysGridContainer.AddThemeConstantOverride("hseparation", 4);
            daysGridContainer.AddThemeConstantOverride("vseparation", 4);
            daysGridContainer.SetAnchorsPreset(Control.LayoutPreset.TopWide);
            
            for (int i = 0; i < month.Days; i++)
            {
                var calendarDayControl = (CenterContainer) dayButtonScene.Instantiate();
                var calendarDayButton = calendarDayControl.GetNode<Button>("DayButton");
                calendarDayButton.ButtonGroup = calendarDaysButtonGroup;
                calendarDayButton.Text = $"{i + 1}";
                calendarDayButton.Connect(Button.SignalName.Pressed, Callable.From(OnDayInCalendarButtonPressed));
                daysGridContainer.AddChild(calendarDayControl);
            }

            daysGridContainer.Visible = false;
            monthDaysContainer.AddChild(daysGridContainer);
            dayButtonsForMonth.Add(daysGridContainer);
        }
    }

    private void ToggleMonthCalendarView(int monthIndex)
    {
        for (int i = 0; i < dayButtonsForMonth.Count; i++)
        {
            ((GridContainer) dayButtonsForMonth[i]).Visible = i == monthIndex;
        }

        currentlyViewedMonth = monthIndex;
        UpdateCurrentDateLabelsAndUi();
    }

    private void UpdateCurrentDateLabelsAndUi()
    {
        // current datetime
        dateLabel.Text = $"{currentCalendar.Months[currentDateTime.currentMonthIndex].Name} {currentDateTime.currentDay}";
        yearLabel.Text = $"{currentDateTime.currentYear}";
        timeLabel.Text = $"{currentDateTime.currentTime_minutes / 60}:{((currentDateTime.currentTime_minutes % 60) > 9 ? $"{currentDateTime.currentTime_minutes % 60}" : $"0{currentDateTime.currentTime_minutes % 60}")}";
        yearSetupInput.Value = currentDateTime.currentYear;

        // viewed datetime ("browsing" the calendar)
        monthYearLabel.Text = $"{currentCalendar.Months[currentlyViewedMonth].Name} {currentlyViewedYear}";
    }

    private void OnCurrentYearChanged(double year)
    {
        currentDateTime.currentYear = (int) yearSetupInput.Value;
        currentlyViewedYear = currentDateTime.currentYear;
        monthYearLabel.Text = $"{currentCalendar.Months[currentDateTime.currentMonthIndex].Name} {currentDateTime.currentYear}";
        yearLabel.Text = $"{currentDateTime.currentYear}";
    }

    private void OnNextMonthButtonPressed()
    {
        if (currentlyViewedMonth + 1 >= currentCalendar.Months.Length)
        {
            currentlyViewedMonth = 0;
            currentlyViewedYear++;
        }
        else
        {
            currentlyViewedMonth++;
        }
        ToggleMonthCalendarView(currentlyViewedMonth);
    }

    private void OnPreviousMonthButtonPressed()
    {
        if (currentlyViewedMonth - 1 < 0)
        {
            currentlyViewedMonth = currentCalendar.Months.Length - 1;
            currentlyViewedYear--;
        }
        else
        {
            currentlyViewedMonth--;
        }
        ToggleMonthCalendarView(currentlyViewedMonth);
    }

    private void OnDayInCalendarButtonPressed()
    {
        var pressedButton = (Button) calendarDaysButtonGroup.GetPressedButton();
        if (pressedButton == null)
        {
            return;
        }

        var day = int.Parse(pressedButton.Text);
        currentDateTime.currentDay = day;
        currentDateTime.currentMonthIndex = currentlyViewedMonth;
        currentDateTime.currentYear = currentlyViewedYear;
        UpdateCurrentDateLabelsAndUi();
    }

    private void OnForwardTimeButtonPressed()
    {
        currentDateTime.currentTime_minutes += (int) timeSkipInput.Value;
        if (currentDateTime.currentTime_minutes >= 1440) // a day in minutes
        {
            currentDateTime.currentTime_minutes -= 1440;
            currentDateTime.currentDay++;
            if (currentDateTime.currentDay > currentCalendar.Months[currentDateTime.currentMonthIndex].Days)
            {
                currentDateTime.currentDay = 1;
                currentDateTime.currentMonthIndex++;
                if (currentDateTime.currentMonthIndex >= currentCalendar.Months.Length)
                {
                    currentDateTime.currentMonthIndex = 0;
                    currentDateTime.currentYear++;
                }
            }
        }
        UpdateCurrentDateLabelsAndUi();
    }

    private void OnRewindTimeButtonPressed()
    {
        currentDateTime.currentTime_minutes -= (int) timeSkipInput.Value;
        if (currentDateTime.currentTime_minutes < 0)
        {
            currentDateTime.currentTime_minutes += 1440;
            currentDateTime.currentDay--;
            if (currentDateTime.currentDay < 1)
            {
                currentDateTime.currentDay = currentCalendar.Months[currentDateTime.currentMonthIndex].Days;
                currentDateTime.currentMonthIndex--;
                if (currentDateTime.currentMonthIndex < 0)
                {
                    currentDateTime.currentMonthIndex = currentCalendar.Months.Length - 1;
                    currentDateTime.currentYear--;
                }
            }
        }
        UpdateCurrentDateLabelsAndUi();
    }
}
