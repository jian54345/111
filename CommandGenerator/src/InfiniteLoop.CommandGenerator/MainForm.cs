using System.Diagnostics;
using System.Text;
using InfiniteLoop.CommandGenerator.Models;
using InfiniteLoop.CommandGenerator.Services;

namespace InfiniteLoop.CommandGenerator;

public sealed class MainForm : Form
{
    private readonly SettingsService _settingsService = new();
    private readonly AscNetClient _client = new();
    private AppSettings _settings;
    private CommandHistory _history;

    private readonly TreeView _tree = new();
    private readonly TextBox _commandBox = new();
    private readonly TextBox _parameterBox = new();
    private readonly TextBox _serverBox = new();
    private readonly TextBox _endpointBox = new();
    private readonly ComboBox _methodBox = new();
    private readonly NumericUpDown _timeoutBox = new();
    private readonly CheckBox _autoCopy = new();
    private readonly CheckBox _autoSave = new();
    private readonly ListBox _historyBox = new();
    private readonly RichTextBox _logBox = new();
    private readonly Label _statusLabel = new();
    private readonly Label _parameterLabel = new();
    private CommandDefinition? _selectedDefinition;

    public MainForm()
    {
        _settings = _settingsService.Load();
        _history = new CommandHistory(_settings.HistoryLimit);

        Text = "InfiniteLoop Command Generator";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1050, 700);
        Size = new Size(1250, 800);
        Font = new Font("Segoe UI", 9F);
        KeyPreview = true;

        BuildUi();
        LoadCatalog();
        LoadSettingsToUi();
        Log("Ready.");
    }

    private void BuildUi()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(8)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 72));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 28));
        Controls.Add(root);

        _tree.Dock = DockStyle.Fill;
        _tree.AfterSelect += (_, e) =>
        {
            if (e.Node?.Tag is CommandDefinition def)
                SelectDefinition(def);
        };
        root.Controls.Add(_tree, 0, 0);

        var main = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 5,
            ColumnCount = 1
        };
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        main.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        root.Controls.Add(main, 1, 0);

        var serverPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 5 };
        serverPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 65));
        serverPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        serverPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
        serverPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        serverPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));

        serverPanel.Controls.Add(new Label { Text = "Server", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        _serverBox.Dock = DockStyle.Fill;
        serverPanel.Controls.Add(_serverBox, 1, 0);
        serverPanel.Controls.Add(new Label { Text = "Method", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter }, 2, 0);
        _methodBox.Dock = DockStyle.Fill;
        _methodBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _methodBox.Items.AddRange(["GET", "POST"]);
        serverPanel.Controls.Add(_methodBox, 3, 0);

        var testButton = new Button { Text = "测试", Dock = DockStyle.Fill };
        testButton.Click += async (_, _) => await TestServerAsync();
        serverPanel.Controls.Add(testButton, 4, 0);

        var endpointRow = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        endpointRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
        endpointRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        endpointRow.Controls.Add(new Label { Text = "Endpoint", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        _endpointBox.Dock = DockStyle.Fill;
        endpointRow.Controls.Add(_endpointBox, 1, 0);

        main.Controls.Add(serverPanel, 0, 0);
        main.Controls.Add(endpointRow, 0, 1);

        var commandRow = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3 };
        commandRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
        commandRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        commandRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300));
        commandRow.Controls.Add(new Label { Text = "命令", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        _commandBox.Dock = DockStyle.Fill;
        _commandBox.Font = new Font("Consolas", 10F);
        commandRow.Controls.Add(_commandBox, 1, 0);

        var commandButtons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
        var copy = Button("复制");
        copy.Click += (_, _) => CopyCommand();
        var clear = Button("清空");
        clear.Click += (_, _) => _commandBox.Clear();
        var execute = Button("执行");
        execute.Click += async (_, _) => await ExecuteCurrentAsync();
        var append = Button("追加");
        append.Click += (_, _) => GenerateSelected(true);
        commandButtons.Controls.AddRange([copy, clear, append, execute]);
        commandRow.Controls.Add(commandButtons, 2, 0);
        main.Controls.Add(commandRow, 0, 2);

        var output = new TabControl { Dock = DockStyle.Fill };
        var logPage = new TabPage("执行日志");
        _logBox.Dock = DockStyle.Fill;
        _logBox.ReadOnly = true;
        _logBox.Font = new Font("Consolas", 9F);
        logPage.Controls.Add(_logBox);
        output.TabPages.Add(logPage);

        var historyPage = new TabPage("历史");
        _historyBox.Dock = DockStyle.Fill;
        _historyBox.DoubleClick += (_, _) =>
        {
            if (_historyBox.SelectedItem is string cmd)
                _commandBox.Text = cmd;
        };
        historyPage.Controls.Add(_historyBox);
        output.TabPages.Add(historyPage);

        var settingsPage = new TabPage("选项");
        var settingsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(10)
        };

        _autoCopy.Text = "生成后自动复制";
        _autoCopy.AutoSize = true;
        _autoSave.Text = "批量执行后自动 /save";
        _autoSave.AutoSize = true;

        settingsPanel.Controls.Add(_autoCopy);
        settingsPanel.Controls.Add(_autoSave);

        var timeoutRow = new FlowLayoutPanel { AutoSize = true };
        timeoutRow.Controls.Add(new Label { Text = "超时(秒)：", AutoSize = true });
        _timeoutBox.Minimum = 1;
        _timeoutBox.Maximum = 120;
        _timeoutBox.Width = 70;
        timeoutRow.Controls.Add(_timeoutBox);
        settingsPanel.Controls.Add(timeoutRow);

        var saveSettings = Button("保存设置");
        saveSettings.Click += (_, _) => SaveSettingsFromUi();
        settingsPanel.Controls.Add(saveSettings);

        settingsPage.Controls.Add(settingsPanel);
        output.TabPages.Add(settingsPage);

        main.Controls.Add(output, 0, 3);

        var statusRow = new Panel { Dock = DockStyle.Fill };
        _statusLabel.Text = "Ready";
        _statusLabel.Dock = DockStyle.Fill;
        _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        statusRow.Controls.Add(_statusLabel);
        main.Controls.Add(statusRow, 0, 4);

        var rightBottom = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        rightBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
        rightBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        root.Controls.Add(rightBottom, 1, 1);

        var help = new Label
        {
            Dock = DockStyle.Fill,
            Text = "快捷键：F5 执行 | Ctrl+C 复制 | Enter 在命令框中执行\n" +
                   "Shift+点击快捷命令：追加到当前命令，使用 | 分隔。\n" +
                   "Reset / Level reset 属于会改变持久状态的操作，请确认服务端逻辑后再执行。",
            Padding = new Padding(5)
        };
        rightBottom.Controls.Add(help, 0, 0);

        var batch = Button("全套 Max");
        batch.Click += (_, _) => _commandBox.Text =
            "/character modify all max | /equip modify all level max | /level max | /save";
        rightBottom.Controls.Add(batch, 1, 0);

        KeyDown += MainForm_KeyDown;
        _commandBox.KeyDown += CommandBox_KeyDown;
    }

    private static Button Button(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Height = 30
    };

    private void LoadCatalog()
    {
        _tree.Nodes.Clear();

        foreach (var category in CommandCatalog.Categories)
        {
            var node = _tree.Nodes.Add(category);
            node.Tag = category;

            foreach (var def in CommandCatalog.GetCategory(category))
            {
                var child = node.Nodes.Add(def.Name);
                child.Tag = def;
            }
            node.Expand();
        }
    }

    private void LoadSettingsToUi()
    {
        _serverBox.Text = _settings.ServerUrl;
        _endpointBox.Text = _settings.CommandEndpoint;
        _methodBox.SelectedItem = _settings.HttpMethod;
        if (_methodBox.SelectedIndex < 0) _methodBox.SelectedIndex = 0;
        _autoCopy.Checked = _settings.AutoCopy;
        _autoSave.Checked = _settings.AutoSaveAfterBatch;
        _timeoutBox.Value = Math.Clamp(_settings.RequestTimeoutSeconds, 1, 120);
    }

    private void SaveSettingsFromUi()
    {
        _settings.ServerUrl = _serverBox.Text.Trim().TrimEnd('/');
        _settings.CommandEndpoint = _endpointBox.Text.Trim();
        _settings.HttpMethod = _methodBox.SelectedItem?.ToString() ?? "GET";
        _settings.AutoCopy = _autoCopy.Checked;
        _settings.AutoSaveAfterBatch = _autoSave.Checked;
        _settings.RequestTimeoutSeconds = (int)_timeoutBox.Value;
        _settingsService.Save(_settings);
        Log("Settings saved.");
    }

    private void SelectDefinition(CommandDefinition def)
    {
        _selectedDefinition = def;
        _parameterLabel.Text = def.ParameterLabel ?? "";
        GenerateSelected(false);
        _statusLabel.Text = def.Description;
    }

    private void GenerateSelected(bool append)
    {
        if (_selectedDefinition is null)
            return;

        string? parameter = null;
        if (!string.IsNullOrWhiteSpace(_selectedDefinition.Parameter))
        {
            parameter = Prompt("参数", _selectedDefinition.ParameterLabel ?? _selectedDefinition.Parameter, _parameterBox.Text);
            if (parameter is null) return;
            _parameterBox.Text = parameter;
        }

        var command = _selectedDefinition.Build(parameter);

        if (append && !string.IsNullOrWhiteSpace(_commandBox.Text))
            _commandBox.Text = _commandBox.Text.TrimEnd() + " | " + command;
        else
            _commandBox.Text = command;

        AddHistory(_commandBox.Text);

        if (_settings.AutoCopy)
            CopyCommand();
    }

    private void AddHistory(string command)
    {
        _history.Add(command);
        _historyBox.Items.Clear();
        foreach (var item in _history.Items)
            _historyBox.Items.Add(item);
    }

    private async Task ExecuteCurrentAsync()
    {
        var command = _commandBox.Text.Trim();
        if (command.Length == 0)
        {
            MessageBox.Show(this, "请输入或生成命令。", "提示",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        SaveSettingsFromUi();
        AddHistory(command);

        var parts = command
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length > 1)
        {
            var result = MessageBox.Show(
                this,
                $"检测到 {parts.Length} 条命令。\n\n将按顺序执行，并等待每条 HTTP 请求完成。\n\n继续？",
                "批量执行确认",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;
        }

        SetBusy(true);
        try
        {
            foreach (var part in parts)
            {
                Log($"> {part}");
                var result = await _client.ExecuteAsync(_settings, part);

                if (result.Success)
                {
                    Log($"< HTTP {result.StatusCode}: {result.Response}");
                    _statusLabel.Text = "执行成功";
                }
                else
                {
                    Log($"! HTTP {result.StatusCode}: {result.ErrorMessage}");
                    if (!string.IsNullOrWhiteSpace(result.Response))
                        Log($"< {result.Response}");

                    _statusLabel.Text = "执行失败";
                    MessageBox.Show(
                        this,
                        $"{part}\n\n{result.ErrorMessage}",
                        "命令执行失败",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }
            }

            if (parts.Length > 1 && _settings.AutoSaveAfterBatch &&
                !parts.Any(x => x.Equals("/save", StringComparison.OrdinalIgnoreCase)))
            {
                Log("> /save");
                var saveResult = await _client.ExecuteAsync(_settings, "/save");
                Log(saveResult.Success
                    ? $"< HTTP {saveResult.StatusCode}: {saveResult.Response}"
                    : $"! /save: {saveResult.ErrorMessage}");
            }
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task TestServerAsync()
    {
        SaveSettingsFromUi();
        SetBusy(true);
        try
        {
            var ok = await _client.TestAsync(_settings);
            _statusLabel.Text = ok ? "Server 可访问" : "Server 不可访问";
            Log(ok ? "[OK] Server reachable." : "[FAIL] Server unreachable.");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void CopyCommand()
    {
        if (string.IsNullOrWhiteSpace(_commandBox.Text))
            return;

        Clipboard.SetText(_commandBox.Text);
        _statusLabel.Text = "已复制";
        Log("[COPY] " + _commandBox.Text);
    }

    private void SetBusy(bool busy)
    {
        Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
        _tree.Enabled = !busy;
    }

    private void Log(string text)
    {
        _logBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {text}{Environment.NewLine}");
        _logBox.SelectionStart = _logBox.TextLength;
        _logBox.ScrollToCaret();
    }

    private void MainForm_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.F5)
        {
            e.SuppressKeyPress = true;
            _ = ExecuteCurrentAsync();
        }
        else if (e.Control && e.KeyCode == Keys.C)
        {
            CopyCommand();
            e.SuppressKeyPress = true;
        }
    }

    private void CommandBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter && e.Control)
        {
            e.SuppressKeyPress = true;
            _ = ExecuteCurrentAsync();
        }
    }

    private static string? Prompt(string title, string label, string defaultValue)
    {
        using var form = new Form
        {
            Width = 380,
            Height = 150,
            Text = title,
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false
        };

        var text = new TextBox
        {
            Left = 20,
            Top = 40,
            Width = 325,
            Text = defaultValue
        };
        var labelControl = new Label { Left = 20, Top = 15, AutoSize = true, Text = label };
        var ok = new Button { Text = "确定", Left = 185, Top = 75, Width = 75, DialogResult = DialogResult.OK };
        var cancel = new Button { Text = "取消", Left = 270, Top = 75, Width = 75, DialogResult = DialogResult.Cancel };

        form.Controls.AddRange([labelControl, text, ok, cancel]);
        form.AcceptButton = ok;
        form.CancelButton = cancel;

        return form.ShowDialog() == DialogResult.OK ? text.Text : null;
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        SaveSettingsFromUi();
        _client.Dispose();
        base.OnFormClosed(e);
    }
}
