﻿using ReactiveUI;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static System.Application.SteamApiUrls;

namespace System.Application.Models
{
    public class SteamApp : ReactiveObject, IComparable<SteamApp>
    {
        public SteamApp(){}

        public SteamApp(uint appid) 
        {
            AppId = appid;
        }

        /// <summary>
        /// gog default logo 0~21
        /// </summary>
        public const string defaultLogoPath = @"avares://System.Application.SteamTools.Client.Desktop.Avalonia/Application/UI/Assets/AppResources/Placeholders/{0}.png";

        private const string NodeAppInfo = "appinfo";

        private const string NodeAppType = "type";

        private const string NodeCommon = "common";

        private const string NodeId = "gameid";

        private const string NodeName = "name";

        private const string NodeParentId = "parent";

        private const string NodePlatforms = "oslist";

        private const string NodePlatformsLinux = "linux";

        private const string NodePlatformsMac = "mac";

        private const string NodePlatformsWindows = "windows";

        public int Index { get; set; }

        public uint AppId { get; set; }

        public bool IsInstalled { get; set; }

        public string? InstalledDir { get; set; }

        public string? Name { get; set; }

        public string? EditName
        {
            get
            {
                if (this._cachedName == null)
                {
                    this._cachedName = this._properties.GetPropertyValue<string>(null, new string[]
                    {
                        "appinfo",
                        "common",
                        "name"
                    });
                }
                return this._cachedName;
            }
            set
            {
                this._properties.SetPropertyValue(SteamAppPropertyType.String, value, new string[]
                {
                    "appinfo",
                    "common",
                    "name"
                });
                this.ClearCachedProps();
            }
        }

        public string? DisplayName => string.IsNullOrEmpty(EditName) ? Name : EditName;

        public string? Logo { get; set; }

        public string? Icon { get; set; }

        public SteamAppType Type { get; set; }

        public uint ParentId { get; set; }

        public IList<SteamApp> ChinldApp { get; set; } = new List<SteamApp>();

        public string? LogoUrl => string.IsNullOrEmpty(Logo) ? null :
            string.Format(STEAMAPP_LOGO_URL, AppId, Logo);

        public string LibraryLogoUrl => string.Format(STEAMAPP_LIBRARY_URL, AppId);

        private string? _LibraryLogoStream;
        public string? LibraryLogoStream
        {
            get => _LibraryLogoStream;
            set => this.RaiseAndSetIfChanged(ref _LibraryLogoStream, value);
        }

        public string LibraryHeaderUrl => string.Format(STEAMAPP_LIBRARYHERO_URL, AppId);

        private string? _LibraryHeaderStream;
        public string? LibraryHeaderStream
        {
            get => _LibraryHeaderStream;
            set => this.RaiseAndSetIfChanged(ref _LibraryHeaderStream, value);
        }

        public string LibraryNameUrl => string.Format(STEAMAPP_LIBRARYLOGO_URL, AppId);

        private string? _LibraryNameStream;
        public string? LibraryNameStream
        {
            get => _LibraryNameStream;
            set => this.RaiseAndSetIfChanged(ref _LibraryNameStream, value);
        }

        public string DefaultLibraryLogo { get; } = string.Format(defaultLogoPath, Random2.Next(22));

        public string HeaderLogoUrl => string.Format(STEAMAPP_CAPSULE_URL, AppId);

        private string? _HeaderLogoStream;
        public string? HeaderLogoStream
        {
            get => _HeaderLogoStream;
            set => this.RaiseAndSetIfChanged(ref _HeaderLogoStream, value);
        }

        public string? IconUrl => string.IsNullOrEmpty(Icon) ? null :
            string.Format(STEAMAPP_LOGO_URL, AppId, Icon);

        public Process? Process { get; set; }

        public TradeCard? Card { get; set; }

        public SteamAppInfo? Common { get; set; }

        public string GetIdAndName()
        {
            return $"{AppId} | {Name}";
        }

        public int CompareTo(SteamApp? other) => string.Compare(Name, other?.Name);

        private string? _cachedName;

        private bool? _cachedHasSortAs;

        private string? _cachedSortAs;

        private byte[] _stuffBeforeHash;

        private uint _changeNumber;

        private byte[] _originalData;

        private SteamAppPropertyTable _properties;

        private void ClearCachedProps()
        {
            this._cachedName = null;
            this._cachedSortAs = null;
            this._cachedHasSortAs = null;
        }

        public event EventHandler Modified;

        private void OnEntryModified(object sender, EventArgs e)
        {
            EventHandler modified = this.Modified;
            if (modified == null)
            {
                return;
            }
            modified(this, new EventArgs());
        }


        public static SteamApp? FromReader(BinaryReader reader)
        {
            uint num = reader.ReadUInt32();
            if (num == 0)
            {
                return null;
            }
            SteamApp app = new()
            {
                AppId = num,
            };
            try
            {
                int count = reader.ReadInt32();
                byte[] array = reader.ReadBytes(count);
                using BinaryReader binaryReader = new(new MemoryStream(array));
                app._stuffBeforeHash = binaryReader.ReadBytes(16);
                binaryReader.ReadBytes(20);
                app._changeNumber = binaryReader.ReadUInt32();
                app._properties = binaryReader.ReadPropertyTable();
                var nodes = new string[3] { NodeAppInfo, NodeCommon, string.Empty };
                var installpath = app._properties.GetPropertyValue<string>("", new string[]
                {
                    NodeAppInfo,
                    "config",
                    "installdir"
                 });
                app.InstalledDir = Path.Combine(Services.ISteamService.Instance.SteamDirPath, "steamapps", NodeCommon, installpath);

                nodes[2] = NodeParentId;
                app.ParentId = (uint)app._properties.GetPropertyValue<int>(0, nodes);

                nodes[2] = NodeAppType;
                string type = app._properties.GetPropertyValue<string>("", nodes);

                if (string.IsNullOrEmpty(type))
                {
                    if (Enum.TryParse(type, true, out SteamAppType apptype))
                    {
                        app.Type = apptype;
                    }
                    else
                    {
                        app.Type = SteamAppType.Unknown;
                        Debug.WriteLine(string.Format("AppInfo: New AppType '{0}'", type));
                    }
                }


                nodes[2] = NodePlatforms;
                string oslist = app._properties.GetPropertyValue<string>("", NodePlatforms);



                var propertyValue = app._properties.GetPropertyValue<string>("", new string[]
                {
                        "appinfo",
                        "steam_edit",
                        "base_name"
                });
                if (propertyValue != "")
                {
                    nodes[2] = NodeName;
                    app._properties.SetPropertyValue(SteamAppPropertyType.String, propertyValue, nodes);
                }
                var propertyValue2 = app._properties.GetPropertyValue<string>("", new string[]
                {
                        "appinfo",
                        "steam_edit",
                        "base_type"
                });
                if (propertyValue2 != "")
                {
                    nodes[2] = NodeAppType;
                    app._properties.SetPropertyValue(SteamAppPropertyType.String, propertyValue2, nodes);
                }
                app._originalData = array;
                app.ClearCachedProps();
            }
            catch (Exception ex)
            {
                Log.Error(nameof(SteamApp), ex, string.Format("Failed to load entry with appId {0:X8}", app.AppId));
            }
            return app;
        }
    }
}