﻿using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin;
using System;
using System.Reflection;
using ImGuiNET;
using System.Text;
using Dalamud.Game.Internal;

namespace XIVLogger
{

    public class Plugin : IDalamudPlugin
    {
        public string Name => "XIVLogger";

        private const string commandName = "/xivlogger";

        private DalamudPluginInterface pi;
        private Configuration configuration;
        private PluginUI ui;
        public ChatLog log;

        public string Location { get; private set; } = Assembly.GetExecutingAssembly().Location;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {

            this.pi = pluginInterface;

            this.configuration = this.pi.GetPluginConfig() as Configuration ?? new Configuration();
            this.configuration.Initialize(this.pi);

            this.ui = new PluginUI(this.configuration);

            this.pi.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Opens settings window for XIVLogger"
            });

            this.pi.CommandManager.AddHandler("/savelog", new CommandInfo(OnSaveCommand)
            {
                HelpMessage = "Saves a chat log as a text file with the current settings, /savelog <number> to save the last <number> messages"
            });

            this.pi.CommandManager.AddHandler("/copylog", new CommandInfo(OnCopyCommand)
            {
                HelpMessage = "Copies a chat log to your clipboard with the current settings, /copylog <number> to copy the last <number> messages"
            });

            this.log = new ChatLog(configuration, pi);
            this.ui.log = log;

            this.pi.UiBuilder.OnBuildUi += DrawUI;
            this.pi.UiBuilder.OnOpenConfigUi += (sender, args) => DrawConfigUI();

            this.pi.Framework.Gui.Chat.OnChatMessage += OnChatMessage;

            this.pi.Framework.OnUpdateEvent += OnUpdate;

            // To do: if autosave file exists, start new autosave file to avoid overwriting previous session

        }

        private void OnUpdate(Framework framework)
        {
            if (configuration.fAutosave)
            {
                if (configuration.checkTime())
                {
                    log.autoSave();
                    configuration.updateAutosaveTime();
                }
            }
        }

        private void OnChatMessage(XivChatType type, uint id, ref SeString sender, ref SeString message, ref bool handled)
        {
            log.addMessage(type, sender.TextValue, message.TextValue);

            //PluginLog.Log("Chat message from type {0}: {1}", type, message.TextValue);
        }

        public void Dispose()
        {

            this.pi.CommandManager.RemoveHandler(commandName);
            this.pi.CommandManager.RemoveHandler("/savelog");
            this.pi.CommandManager.RemoveHandler("/copylog");
            this.pi.Dispose();

            this.pi.Framework.Gui.Chat.OnChatMessage -= OnChatMessage;
        }

        private void OnCommand(string command, string args)
        {
            this.ui.SettingsVisible = true;
        }

        private void OnSaveCommand(string command, string args)
        {
            log.printLog(args);
        }

        private void OnCopyCommand(string command, string args)
        {
            ImGui.SetClipboardText(log.printLog(args, aClipboard: true));
        }


        private void DrawUI()
        {
            this.ui.Draw();
        }

        private void DrawConfigUI()
        {
            this.ui.SettingsVisible = true;
        }
    }
}
