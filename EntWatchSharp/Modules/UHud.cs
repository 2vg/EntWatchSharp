using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using EntWatchSharp.Items;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Entities;
using Cysharp.Text;

namespace EntWatchSharp.Modules
{
	abstract class UHud
	{
		public float fXEntity = -6.5f;
		public float fYEntity = 2.0f;
		public float fZEntity = 7.0f;
		public int[] colorEntity = [255, 255, 255, 255];
		public int iSheetMax = 5;
		public int iRefresh = 3;
		public int iSize = 54;
		int iCurrentNumList = 0;
		double fNextUpdateList = EW.fGameTime - 3;
		public UHud() { }
		public void ConstructString(CCSPlayerController HudPlayer)
		{
			var listShow = new List<Item>();
			bool bAdminPermissions = AdminManager.PlayerHasPermissions(HudPlayer, "@css/ew_hud") && Cvar.AdminHud < 2;
			foreach (Item itemTest in EW.g_ItemList)
			{
				if (itemTest.Owner != null)
				{
					if (itemTest.Hud && (!Cvar.TeamOnly || HudPlayer.TeamNum < 2 || itemTest.Team == HudPlayer.TeamNum || bAdminPermissions))
					{
						listShow.Add(itemTest);
					}
				}
			}
			if (listShow.Count > 0)
			{
				int iCountList = (listShow.Count - 1) / iSheetMax + 1;

				if (fNextUpdateList <= EW.fGameTime)
				{
					iCurrentNumList++;
					fNextUpdateList = EW.fGameTime + iRefresh;
				}
				if (iCurrentNumList >= iCountList) iCurrentNumList = 0;

				using (var sItemsBuilder = ZString.CreateStringBuilder())
				{
					sItemsBuilder.Append("EntWatch:");

					for (int i = iCurrentNumList * iSheetMax; i < listShow.Count && i < (iCurrentNumList + 1) * iSheetMax; i++)
					{
						sItemsBuilder.Append($"\n{listShow[i].ShortName}");
						if (!Cvar.TeamOnly || HudPlayer.TeamNum < 2 || listShow[i].Team == HudPlayer.TeamNum || bAdminPermissions && Cvar.AdminHud == 0)
						{
							if (listShow[i].CheckDelay())
							{
								int iAbilityCount = 0;
								foreach (Ability abilityTest in listShow[i].AbilityList)
								{
									if (++iAbilityCount > Cvar.DisplayAbility) break;
									if (!abilityTest.Ignore) sItemsBuilder.Append($"[{abilityTest.GetMessage()}]");
								}

							}
							else sItemsBuilder.Append($"[-{Math.Round(listShow[i].fDelay - EW.fGameTime, 1)}]");
						}
						sItemsBuilder.Append($": {listShow[i].Owner.PlayerName}");
					}
					if (iCountList > 1) sItemsBuilder.Append($"\nList:[{iCurrentNumList + 1}/{iCountList}]");
					UpdateText(sItemsBuilder.ToString(), HudPlayer);
				}
			}
			else UpdateText("", HudPlayer);
		}
		public abstract void UpdateText(string sItems, CCSPlayerController HudPlayer);
	}

	class HudNull : UHud
	{
		public HudNull() { }
		public override void UpdateText(string sItems, CCSPlayerController HudPlayer) { }
	}

	class HudCenter : UHud
	{
		public HudCenter() { }
		public override void UpdateText(string sItems, CCSPlayerController HudPlayer)
		{
			if (HudPlayer is { IsValid: true, IsBot: false } && !string.IsNullOrEmpty(sItems)) HudPlayer.PrintToCenter(sItems);
		}
	}
	class HudAlert : UHud
	{
		public HudAlert() { }
		public override void UpdateText(string sItems, CCSPlayerController HudPlayer)
		{
			if (HudPlayer is { IsValid: true, IsBot: false } && !string.IsNullOrEmpty(sItems)) HudPlayer.PrintToCenterAlert(sItems);
		}
	}

	class HudWorldText : UHud
	{
		public HudWorldText() { }
		public void InitHud(CCSPlayerController HudPlayer)
		{
			if (EW._GH_api != null && HudPlayer.IsValid)
			{
				EW._GH_api.Native_GameHUD_SetParams(HudPlayer, EW.HUDCHANNEL, fXEntity, fYEntity, fZEntity, System.Drawing.Color.FromArgb(colorEntity[3], colorEntity[0], colorEntity[1], colorEntity[2]), iSize, "Verdana", iSize / 7000.0f);
			}
		}
		public override void UpdateText(string sItems, CCSPlayerController HudPlayer)
		{
			if (EW._GH_api != null && HudPlayer.IsValid)
			{
				EW._GH_api.Native_GameHUD_Show(HudPlayer, EW.HUDCHANNEL, sItems, 1.2f);
			}
		}
	}
}
