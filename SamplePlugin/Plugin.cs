﻿using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SamplePlugin
{

    public class Plugin : IDalamudPlugin
    {
        public string Name => "My Cool Plugin";

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

            // you might normally want to embed resources and load them from the manifest stream
            this.ui = new PluginUI(this.configuration);

            this.pi.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "hello world"
            });

            this.log = new ChatLog(configuration.EnabledChatTypes);
            this.ui.log = log;

            this.pi.UiBuilder.OnBuildUi += DrawUI;
            this.pi.UiBuilder.OnOpenConfigUi += (sender, args) => DrawConfigUI();

            this.pi.Framework.Gui.Chat.OnChatMessage += OnChatMessage;


        }
        private void OnChatMessage(XivChatType type, uint id, ref SeString sender, ref SeString message, ref bool handled)
        {
            log.addMessage(type, sender.TextValue, message.TextValue);

            //PluginLog.Log("Chat message from type {0}: {1}", type, message.TextValue);
        }

        public void Dispose()
        {

            this.pi.CommandManager.RemoveHandler(commandName);
            this.pi.Dispose();

            this.pi.Framework.Gui.Chat.OnChatMessage -= OnChatMessage;
        }

        private void OnCommand(string command, string args)
        {
            this.ui.SettingsVisible = true; 
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
