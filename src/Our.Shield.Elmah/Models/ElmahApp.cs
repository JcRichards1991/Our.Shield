﻿using Our.Shield.Core.Attributes;
using Our.Shield.Core.Helpers;
using Our.Shield.Core.Models;
using Our.Shield.Core.Operation;
using Our.Shield.Core.Services;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Umbraco.Core;

namespace Our.Shield.Elmah.Models
{
	[CustomAppTab("Reporting", 0, "/App_Plugins/Shield.Elmah/Views/Reporting.html?version=1.1.0")]
	[AppEditor("/App_Plugins/Shield.Elmah/Views/Elmah.html?version=1.1.0", sortOrder: 1)]
	[AppJournal(sortOrder: 2)]
	public class ElmahApp : App<ElmahConfiguration>
	{
		private readonly IpAccessControlService _ipAccessControlService;
		public ElmahApp()
		{
			_ipAccessControlService = new IpAccessControlService();
		}

		/// <inheritdoc />
		public override string Id => nameof(Elmah);

		/// <inheritdoc />
		public override string Name =>
			ApplicationContext.Current.Services.TextService.Localize("Shield.Elmah.General/Name", CultureInfo.CurrentCulture);

		/// <inheritdoc />
		public override string Description => ApplicationContext.Current.Services.TextService.Localize("Shield.Elmah.General/Description", CultureInfo.CurrentCulture);

		/// <inheritdoc />
		public override string Icon => "icon-combination-lock orange";

		/// <inheritdoc />
		public override IAppConfiguration DefaultConfiguration => new ElmahConfiguration
		{
			UmbracoUserEnable = true,
			IpAccessRules = new IpAccessControl
			{
				AccessType = IpAccessControl.AccessTypes.AllowAll,
				Exceptions = Enumerable.Empty<IpAccessControl.Entry>()
			},
			Unauthorized = new TransferUrl
			{
				TransferType = TransferTypes.Redirect,
				Url = new UmbracoUrl
				{
					Type = UmbracoUrlTypes.Url,
					Value = string.Empty
				}
			}
		};

		private readonly string _allowKey = Guid.NewGuid().ToString();

		public override bool Execute(IJob job, IAppConfiguration c)
		{
			job.UnwatchWebRequests();
			job.UnexceptionWebRequest();
			job.UnignoreWebRequest();

			if (!c.Enable || !job.Environment.Enable)
				return true;

			if (!(c is ElmahConfiguration config))
			{
				job.WriteJournal(new JournalMessage("Error: Config passed into Elmah was not of the correct type"));
				return false;
			}

			foreach (var error in _ipAccessControlService.InitIpAccessControl(config.IpAccessRules))
			{
				job.WriteJournal(new JournalMessage($"Error: Invalid IP Address {error}, unable to add to exception list"));
			}

			if (config.Unauthorized.TransferType != TransferTypes.PlayDead)
				job.ExceptionWebRequest(config.Unauthorized.Url);

			var regex = job.PathToRegex("elmah.axd");
			job.IgnoreWebRequest(regex);

			job.WatchWebRequests(PipeLineStages.AuthenticateRequest, regex, 300000, (count, httpApp) =>
			{
				if (_ipAccessControlService.IsValid(config.IpAccessRules, httpApp.Context.Request))
				{
					httpApp.Context.Items.Add(_allowKey, true);
				}
				return new WatchResponse(WatchResponse.Cycles.Continue);
			});

			job.WatchWebRequests(PipeLineStages.AuthenticateRequest, regex, 300500, (count, httpApp) =>
			{
				if ((bool?)httpApp.Context.Items[_allowKey] == true || (config.UmbracoUserEnable && AccessHelper.IsRequestAuthenticatedUmbracoUser(httpApp)))
				{
					return new WatchResponse(WatchResponse.Cycles.Continue);
				}

				job.WriteJournal(new JournalMessage($"User with IP Address: {httpApp.Context.Request.UserHostAddress}; tried to access {httpApp.Context.Request.Url} Access was denied"));

				return new WatchResponse(config.Unauthorized);
			});

			return true;
		}
	}
}