using System;
using System.Linq;
using System.Text.RegularExpressions;
using Our.Shield.Core.Attributes;
using Our.Shield.Core.Helpers;
using Our.Shield.Core.Models;
using Our.Shield.Core.Operation;
using Our.Shield.Core.Services;
using Umbraco.Core;

namespace Our.Shield.Swagger.Models
{
	[AppEditor("/App_Plugins/Shield.Swagger/Views/Swagger.html?version=1.1.0")]
	[AppJournal]
	public class SwaggerApp : App<SwaggerConfiguration>
	{
		private readonly string _allowKey = Guid.NewGuid().ToString();

		/// <inheritdoc />
		public override string Id => nameof(Swagger);

		/// <inheritdoc />
		public override string Name => Localize("Shield.Swagger.General", "Name");

		/// <inheritdoc />
		public override string Description => Localize("Shield.Swagger.General", "Description");

		/// <inheritdoc />
		public override string Icon => "icon-gps orange";
		/// <inheritdoc />
		public override IAppConfiguration DefaultConfiguration => new SwaggerConfiguration
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

		private readonly IpAccessControlService _ipAccessControlService;
		public SwaggerApp()
		{
			_ipAccessControlService = new IpAccessControlService();
		}

		public override bool Execute(IJob job, IAppConfiguration c)
		{
			job.UnwatchWebRequests();
			job.UnexceptionWebRequest();
			job.UnignoreWebRequest();

			var regex = job.PathToRegex("swagger");
			job.IgnoreWebRequest(regex);

			if (!c.Enable || !job.Environment.Enable)
			{
				job.WatchWebRequests(PipeLineStages.AuthenticateRequest, regex, 500000, (count, httpApp) => new WatchResponse(WatchResponse.Cycles.Error));
				return true;
			}

			if (!(c is SwaggerConfiguration config))
			{
				job.WriteJournal(new JournalMessage("Error: Config passed into Swagger was not of the correct type"));
				return false;
			}

			foreach (var error in new IpAccessControlService().InitIpAccessControl(config.IpAccessRules))
			{
				job.WriteJournal(new JournalMessage($"Error: Invalid IP Address {error}, unable to add to exception list"));
			}

			job.ExceptionWebRequest(regex);

			if (config.IpAccessRules.Exceptions.Any())
			{
				job.WatchWebRequests(PipeLineStages.AuthenticateRequest, regex, 500250, (count, httpApp) =>
				{
					if (_ipAccessControlService.IsValid(config.IpAccessRules, httpApp.Context.Request))
					{
						httpApp.Context.Items.Add(_allowKey, true);
					}
					return new WatchResponse(WatchResponse.Cycles.Continue);
				});
			}

			job.WatchWebRequests(PipeLineStages.AuthenticateRequest, regex, 500500, (count, httpApp) =>
			{
				if ((bool?)httpApp.Context.Items[_allowKey] == true
					|| config.UmbracoUserEnable && AccessHelper.IsRequestAuthenticatedUmbracoUser(httpApp))
				{
					return new WatchResponse(WatchResponse.Cycles.Continue);
				}

				job.WriteJournal(new JournalMessage($"User with IP Address: {httpApp.Context.Request.UserHostAddress}; tried to access {httpApp.Context.Request.Url}. Access was denied"));

				return new WatchResponse(config.Unauthorized);
			});

			return true;
		}
	}
}