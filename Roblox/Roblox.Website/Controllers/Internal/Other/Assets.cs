using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Roblox.Exceptions;
using Roblox.Models.Assets;
using Roblox.Website.Middleware;
using BadRequestException = Roblox.Exceptions.BadRequestException;
using MultiGetEntry = Roblox.Dto.Assets.MultiGetEntry;
using Type = Roblox.Models.Assets.Type;
using MVC = Microsoft.AspNetCore.Mvc;
using Roblox.Services.Exceptions;
using Roblox.Website.WebsiteModels.Asset;
using Roblox.Libraries.RobloxApi;
using Roblox.Libraries.Assets;
using Roblox.Website.Lib;
using System.Diagnostics;

namespace Roblox.Website.Controllers 
{
    [MVC.ApiController]
    [MVC.Route("/")]
    public class Assets : ControllerBase 
    {		
	    [HttpGet("asset/shader")]
        public async Task<MVC.ActionResult> GetShaderAsset(long id)
        {
            var isMaterialOrShader = BypassControllerMetadata.materialAndShaderAssetIds.Contains(id);
            if (!isMaterialOrShader)
            {
                throw new RobloxException(400, 0, "Material/Shader");
            }

            var assetId = id;
            try
            {
                var ourId = await services.assets.GetAssetIdFromRobloxAssetId(assetId);
                assetId = ourId;
            }
            catch (RecordNotFoundException)
            {
                var migrationResult = await MigrateItem.MigrateItemFromRoblox(assetId.ToString(), false, null, default, new ProductDataResponse()
                {
                    Name = "ShaderConversion" + id,
                    AssetTypeId = Type.Special,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow,
                    Description = "ShaderConversion1.0",
                });
                assetId = migrationResult.assetId;
            }

            var latestVersion = await services.assets.GetLatestAssetVersion(assetId);
            if (latestVersion.contentUrl is null)
            {
                throw new RobloxException(403, 0, "Forbidden");
            }
            HttpContext.Response.Headers.CacheControl = new CacheControlHeaderValue()
            {
                Public = true,
                MaxAge = TimeSpan.FromDays(360),
            }.ToString();
            var assetContent = await services.assets.GetAssetContent(latestVersion.contentUrl);
            return base.File(assetContent, "application/binary");
        }

        private bool IsRcc()
        {
            var rccAccessKey = Request.Headers.ContainsKey("accesskey") ? Request.Headers["accesskey"].ToString() : null;
            var isRcc = rccAccessKey == Configuration.RccAuthorization;
            return isRcc;
        }

		[HttpGetBypass("game/players/{userId}")]
		public dynamic GetPlayerChatFilter(long userId)
		{
			return new
			{
				ChatFilter = "whitelist"
			};
		}

		[HttpGetBypass("/Game/ChatFilter.ashx")]
        public string RCC_GetChatFilter()
        {
            return "True";
        }

		private static bool isheaderbad(string headername)
		{
			var badheaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				"Transfer-Encoding",
				"Connection",
				"Keep-Alive",
				"Content-Length",
				"Upgrade",
				"Server"
			};

			return badheaders.Contains(headername);
		}

        // ============================================================
        //  МЕТОД: Загрузка меша/изображения напрямую с Roblox CDN
        //  с использованием cookie для авторизации
        // ============================================================
        private async Task<MVC.ActionResult?> FetchFromRobloxCdn(long assetId, string assetType)
        {
            // ============================================================
            //  ВАЖНО: Замените это значение на ваш реальный .ROBLOSECURITY cookie
            //  Получить его можно так:
            //  1. Зайдите в Roblox в браузере (Chrome/Firefox)
            //  2. Откройте DevTools (F12) -> Application -> Cookies -> roblox.com
            //  3. Найдите .ROBLOSECURITY и скопируйте его значение
            // ============================================================
            var robloxCookie = "_|WARNING:-DO-NOT-SHARE-THIS.--Sharing-this-will-allow-someone-to-log-in-as-you-and-to-steal-your-ROBUX-and-items.|_CAEQAhoGCAIQBBgBIhwKBGR1aWQSFDE0NTEyNDUxODY0MDUwODEzNzAxIhcKBXVuYW1lEg5idWJiYWJsb3h0ZXN0MSISCgN1aWQSCzExNDYyOTc0NDA0KAM.PgfaDQ9HoD0CqF-ZR0cfxODbDbblYcUIaNASLgWktfaSbHaNHgHhYjpzlD7xuAU8sNhiCfU88HqOgbYVx6XRVTqI7NReEI0pIy2_TUKDjdifH3uI95S-Bj-6JRM-RWpZGGWLBsQGEMmqZvKUrtGPhCOhNulGd1lUl2ecLbtE3hG94f4qZqgFJV_ooOOg9VNllS6X7rc5cGyjnKMoe32MRhMpx2MY3XF5piX-hpV67H8NIq6pHOZyFaA4VA7ERKl3egiCqhLeXNB6jR5WBy9kVSxyMP5BBMb-uuTgxJoJbEAnRJzRnXlsif7CQn2uSz36Tcnn8qRyxJZGU9nVfHF4nWUxRKDFb5sfaFA-3wnNgKYJ2rRvT0wdb1hKn9gF777q7HpnT-Mp9bjLX4zM9bCcOIht9Xh_Ch62dJgAjGWdxvbV70xeS2Z49DhVyEhfXHdZpvpR75HGbCWX7Z3WhmCtM_2jToh76WnPbEci-8m7dOEXLNIqxvNH7QGB8fgZtRWzVaXZEBrIKwf-iFOmJabxImWSeFE5jhIvHhg7FLZ64yaCzBBBf9WdtpZJ0mFhhCBEKEYECoE5YhNsAwry_Lm3D8x3Kt1RY1BE8SepvfdqnLCoTmuKfPLDRrjPJdwA9lsP-kcSvdD1dRqDisEjI8w8bhhDKnC4G418lmZObFF_qENXv2De3lCQAwFt36QMwRLD1lr1_hzrRuoKDIOtr2qGJz8ER4gFQex0EBz_UMXc_rVxCD133fMlBVo0m2B3mEgRTITVpkj2_ZLyZoomhQP1ICBdznsWGrmgIHpSuW5jkW768Py02uIemp9ytTvHUCa9kml4wzhRoila7liMJ2ZtnVeHyYTE7BQhUG3_Maft5tagghsWezExZGHPKUCRyBJKTrYANPrX2lahaN5tj3LSfA.TayrS5bJ__xIU1TKjTXSAHrcjHw";

            var url = $"https://assetdelivery.roblox.com/v1/asset?id={assetId}";

            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(15);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Roblox/WinInet");
            httpClient.DefaultRequestHeaders.Add("Accept", "application/octet-stream");
            
            // Добавляем cookie для авторизации
            if (!string.IsNullOrEmpty(robloxCookie) && robloxCookie != "ВАШ_ROBLOSECURITY_COOKIE_ЗДЕСЬ")
            {
                httpClient.DefaultRequestHeaders.Add("Cookie", $".ROBLOSECURITY={robloxCookie}");
                Console.WriteLine($"[cdn] using cookie for authentication");
            }
            else
            {
                Console.WriteLine($"[cdn] WARNING: no cookie set, trying anonymous access");
            }

            try
            {
                Console.WriteLine($"[cdn] trying: {url}");
                var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsByteArrayAsync();
                    var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

                    // Проверяем, что это не HTML-страница с ошибкой
                    if (content.Length > 0 && content[0] != (byte)'<')
                    {
                        Console.WriteLine($"[cdn] success: {url} ({content.Length} bytes)");
                        await CacheAsset(assetId, content, contentType);
                        return base.File(content, contentType);
                    }
                    else
                    {
                        Console.WriteLine($"[cdn] got HTML instead of binary: {url}");
                        // Логируем первые 200 символов HTML для отладки
                        var htmlPreview = Encoding.UTF8.GetString(content.Take(200).ToArray());
                        Console.WriteLine($"[cdn] HTML preview: {htmlPreview}");
                    }
                }
                else
                {
                    Console.WriteLine($"[cdn] failed ({response.StatusCode}): {url}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[cdn] exception for {url}: {ex.Message}");
            }

            return null;
        }

        [HttpGetBypass("v2/asset")]
        [HttpGetBypass("v1/asset")]
        [HttpGetBypass("asset")]
        [HttpPostBypass("v1/asset")]
        [HttpPostBypass("asset")]
		public async Task<MVC.ActionResult> GetAssetById(long id, [MVC.FromQuery] string? apiKey = null, [MVC.FromQuery(Name = "assetversionid")] long? assetVersionId = null)
        {
			var CachedRobloxAsset = await GetCachedAsset(id);
			if (CachedRobloxAsset != null)
			{
				Console.WriteLine($"[cache] returning cached asset {id} from cache");
				return CachedRobloxAsset;
			}

			if (assetVersionId.HasValue)
			{
				id = assetVersionId.Value;
			}

			if (apiKey == Configuration.RccAuthorization || apiKey == Configuration.RenderAuthorization)
			{
				var latestVersionSecret = await services.assets.GetLatestAssetVersion(id);
				if (latestVersionSecret?.contentUrl == null)
				{
					// Если нет в базе — пробуем с Roblox CDN
					var cdnResult = await FetchFromRobloxCdn(id, "unknown");
					if (cdnResult != null) return cdnResult;
					throw new RobloxException(400, 0, "Content URL is null");
				}

				var assetContentSecret = await services.assets.GetAssetContent(latestVersionSecret.contentUrl);
				return base.File(assetContentSecret, "application/binary");
			}

            var is18OrOver = false;
            if (userSession != null)
            {
                is18OrOver = await services.users.Is18Plus(userSession.userId);
            }

            if (HttpContext.Request.Headers.ContainsKey("RbxTempBypassFor18PlusAssets"))
            {
                is18OrOver = true;
            }

            var assetId = id;
            var invalidIdKey = "InvalidAssetIdForConversionV1:" + assetId;
            if (Services.Cache.distributed.StringGetMemory(invalidIdKey) != null)
                throw new RobloxException(400, 0, "Asset is invalid or does not exist");

            var isBotRequest = Request.Headers["bot-auth"].ToString() == Roblox.Configuration.BotAuthorization;
            var isLoggedIn = userSession != null;
            var encryptionEnabled = !isBotRequest;

            var isMaterialOrShader = BypassControllerMetadata.materialAndShaderAssetIds.Contains(assetId);
            if (isMaterialOrShader)
            {
                return new MVC.RedirectResult("/asset/shader?id=" + assetId);
            }

            var isRcc = IsRcc();
            if (isRcc)
                encryptionEnabled = false;
#if DEBUG
            encryptionEnabled = false;
#endif
            MultiGetEntry details;
			try
			{
				details = await services.assets.GetAssetCatalogInfo(assetId);
			}
			catch (RecordNotFoundException)
			{
				try
				{
					var ourId = await services.assets.GetAssetIdFromRobloxAssetId(assetId);
					assetId = ourId;
				}
				catch (RecordNotFoundException)
				{		
					// ============================================================
					//  ИЗМЕНЕНО: Вместо проксирования на свой сайт — идём на Roblox CDN
					// ============================================================
					Console.WriteLine($"[asset] asset {assetId} not found in DB, trying Roblox CDN...");
					
					var cdnResult = await FetchFromRobloxCdn(assetId, "unknown");
					if (cdnResult != null)
					{
						return cdnResult;
					}

					// Если CDN не помог — пробуем старый способ (проксирование на AssetUrl)
					var pxyurl = $"{Configuration.AssetUrl}/asset/?id={assetId}";

					using var httpClient = new HttpClient();
					httpClient.Timeout = TimeSpan.FromSeconds(10);

					try
					{
						var response = await httpClient.GetAsync(pxyurl, HttpCompletionOption.ResponseHeadersRead);

						if (response.IsSuccessStatusCode)
						{
							var content = await response.Content.ReadAsByteArrayAsync();
							var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

							Response.Headers.Clear();

							foreach (var header in response.Headers)
							{
								if (!isheaderbad(header.Key))
								{
									Response.Headers[header.Key] = header.Value.ToArray();
								}
							}

							Response.Headers["Content-Type"] = contentType;

							await CacheAsset(assetId, content, contentType);
							return base.File(content, contentType);
						}
						else
						{
							throw new RobloxException(400, 0, $"{response.StatusCode}");
						}
					}
					catch (Exception ex)
					{				
						if (ex is TaskCanceledException && !ex.Message.Contains("canceled"))
						{
							throw new RobloxException(400, 0, "Timeout");
						}

						throw new RobloxException(400, 0, $"{ex.Message}");
					}
				}
				details = await services.assets.GetAssetCatalogInfo(assetId);
			}
			if (details.is18Plus && !isRcc && !isBotRequest && !is18OrOver)
				throw new RobloxException(400, 0, "AssetTemporarilyUnavailable");
			if (details.moderationStatus != ModerationStatus.ReviewApproved && !isRcc && !isBotRequest)
				throw new RobloxException(403, 0, "Asset is not approved");

            var latestVersion = await services.assets.GetLatestAssetVersion(assetId);
            Stream? assetContent = null;
			Console.WriteLine($"[debug] assetId={assetId}, assetType={details.assetType}, moderation={details.moderationStatus}, isRcc={isRcc}, isBot={isBotRequest}, is18={is18OrOver}");
            switch (details.assetType)
            {
				case Roblox.Models.Assets.Type.TeeShirt:
					var teeShirtData = ContentFormatters.GetTeeShirt(latestVersion.contentId);
					var teeShirtBytes = Encoding.UTF8.GetBytes(teeShirtData);
					return new MVC.FileContentResult(teeShirtBytes, "application/binary");

				case Models.Assets.Type.Shirt:
					var shirtData = ContentFormatters.GetShirt(latestVersion.contentId);
					var shirtBytes = Encoding.UTF8.GetBytes(shirtData);
					return new MVC.FileContentResult(shirtBytes, "application/binary");

				case Models.Assets.Type.Pants:
					var pantsData = ContentFormatters.GetPants(latestVersion.contentId);
					var pantsBytes = Encoding.UTF8.GetBytes(pantsData);
					return new MVC.FileContentResult(pantsBytes, "application/binary");

                case Models.Assets.Type.Image:
                case Models.Assets.Type.Special:
                    if (latestVersion.contentUrl != null)
                        assetContent = await services.assets.GetAssetContent(latestVersion.contentUrl);
                    else
                    {
                        // ============================================================
                        //  ИЗМЕНЕНО: Если нет contentUrl — берём с Roblox CDN
                        // ============================================================
                        var cdnResult = await FetchFromRobloxCdn(assetId, "image");
                        if (cdnResult != null) return cdnResult;
                    }
                    break;

                case Models.Assets.Type.Audio:
                case Models.Assets.Type.Mesh:
                case Models.Assets.Type.Hat:
                case Models.Assets.Type.Model:
                case Models.Assets.Type.Decal:
                case Models.Assets.Type.Head:
                case Models.Assets.Type.Face:
                case Models.Assets.Type.Gear:
                case Models.Assets.Type.Badge:
                case Models.Assets.Type.Animation:
                case Models.Assets.Type.Torso:
                case Models.Assets.Type.RightArm:
                case Models.Assets.Type.LeftArm:
                case Models.Assets.Type.RightLeg:
                case Models.Assets.Type.LeftLeg:
                case Models.Assets.Type.Package:
                case Models.Assets.Type.GamePass:
                case Models.Assets.Type.Plugin:
                case Models.Assets.Type.MeshPart:
                case Models.Assets.Type.HairAccessory:
                case Models.Assets.Type.FaceAccessory:
                case Models.Assets.Type.NeckAccessory:
                case Models.Assets.Type.ShoulderAccessory:
                case Models.Assets.Type.FrontAccessory:
                case Models.Assets.Type.BackAccessory:
                case Models.Assets.Type.WaistAccessory:
                case Models.Assets.Type.ClimbAnimation:
                case Models.Assets.Type.DeathAnimation:
                case Models.Assets.Type.FallAnimation:
                case Models.Assets.Type.IdleAnimation:
                case Models.Assets.Type.JumpAnimation:
                case Models.Assets.Type.RunAnimation:
                case Models.Assets.Type.SwimAnimation:
                case Models.Assets.Type.WalkAnimation:
                case Models.Assets.Type.PoseAnimation:
				case Models.Assets.Type.EmoteAnimation:
                case Models.Assets.Type.SolidModel:
                    if (latestVersion.contentUrl is null)
                    {
                        // ============================================================
                        //  ИЗМЕНЕНО: Если нет contentUrl — берём с Roblox CDN
                        // ============================================================
                        Console.WriteLine($"[asset] no content URL for {assetId} ({details.assetType}), trying Roblox CDN...");
                        var cdnResult = await FetchFromRobloxCdn(assetId, details.assetType.ToString());
                        if (cdnResult != null) return cdnResult;
                        
                        throw new RobloxException(400, 0, "Content URL is null");
                    }
                    if (details.assetType == Models.Assets.Type.Audio)
                    {
                        assetContent = await services.assets.GetAudioContentAsWav(assetId, latestVersion.contentUrl);
                    }
                    else
                    {
                        assetContent = await services.assets.GetAssetContent(latestVersion.contentUrl);
                    }
                    break;

                default:
                    var ok = false;
                    if (isRcc)
                    {
                        encryptionEnabled = false;
						ok = true;
                        var placeIdHeader = Request.Headers["roblox-place-id"].ToString();
                        long placeId = 0;
                        if (!string.IsNullOrEmpty(placeIdHeader))
                        {
                            try
                            {
                                placeId = long.Parse(Request.Headers["roblox-place-id"].ToString());
                            }
                            catch (FormatException)
                            {
                            }
                        }
                        ok = (placeId == assetId);
                        if (!ok && details.assetType == Models.Assets.Type.Place && placeId == 0)
                        {
                            ok = true;
                        }
                        if (!ok)
                        {
                            var placeDetails = await services.assets.GetAssetCatalogInfo(placeId);
                            if (placeDetails.creatorType == details.creatorType &&
                                placeDetails.creatorTargetId == details.creatorTargetId)
                            {
                                ok = true;
                            }
                        }
						Console.WriteLine($"[debug] default branch, ok={ok}, creatorType={details.creatorType}, creatorTargetId={details.creatorTargetId}");
                    }
                    else
                    {
                        if (userSession != null)
                        {
                            ok = await services.assets.CanUserModifyItem(assetId, userSession.userId);
                            if (!ok)
                            {
                                ok = (details.creatorType == CreatorType.User && details.creatorTargetId == 1);
                            }
#if DEBUG
                            if (await services.users.IsUserStaff(userSession.userId))
                            {
                                ok = true;
                            }
#endif
                            if (ok)
                            {
                                encryptionEnabled = false;
                            }
                        }
                    }

                    if (ok && latestVersion.contentUrl != null)
                    {
                        assetContent = await services.assets.GetAssetContent(latestVersion.contentUrl);
                    }

                    break;
            }

            if (assetContent != null)
            {
                return base.File(assetContent, "application/binary");
            }

            Console.WriteLine("[info] got BadRequest on /asset/ endpoint");
            throw new BadRequestException();
        }

		private async Task CacheAsset(long assetId, byte[] content, string contentType)
		{
			try
			{
				var CacheDIR = Path.Combine(Directory.GetCurrentDirectory(), "AssetCache");
				if (!Directory.Exists(CacheDIR))
				{
					Directory.CreateDirectory(CacheDIR);
				}

				var Cache = Path.Combine(CacheDIR, $"{assetId}.cache");
				var Meta = Path.Combine(CacheDIR, $"{assetId}.meta");

				await System.IO.File.WriteAllBytesAsync(Cache, content);

				var MetaData = new
				{
					ContentType = contentType,
					CachedAt = DateTime.UtcNow,
					AssetId = assetId
				};
				await System.IO.File.WriteAllTextAsync(Meta, System.Text.Json.JsonSerializer.Serialize(MetaData));

				Console.WriteLine($"[cache] cached asset {assetId}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[cache] failed to cache asset {assetId}: {ex.Message}");
			}
		}

		private async Task<MVC.FileContentResult?> GetCachedAsset(long assetId)
		{
			try
			{
				var CacheDIR = Path.Combine(Directory.GetCurrentDirectory(), "AssetCache");
				var Cache = Path.Combine(CacheDIR, $"{assetId}.cache");
				var Meta = Path.Combine(CacheDIR, $"{assetId}.meta");

				if (System.IO.File.Exists(Cache) && System.IO.File.Exists(Meta))
				{
					var MetaJSON = await System.IO.File.ReadAllTextAsync(Meta);
					var MetaData = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(MetaJSON);
					string contentType = MetaData.TryGetProperty("ContentType", out var ct)
						? ct.GetString() ?? "application/octet-stream"
						: "application/octet-stream";

					var content = await System.IO.File.ReadAllBytesAsync(Cache);

					return new MVC.FileContentResult(content, contentType);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[cache] error getting cached asset {assetId}: {ex.Message}");
			}

			return null;
		}

		public class BatchAssetRequest
		{
			public long assetId { get; set; }
			public string assetType { get; set; }
			public string requestId { get; set; }
		}

		[HttpPostBypass("asset/batch")]
        [HttpPostBypass("v1/assets/batch")]
        public async Task<MVC.IActionResult> AssetBatch()
        {
            List<BatchAssetRequest> requestData;
            bool isGzip = Request.Headers["Content-Encoding"].ToString() == "gzip";

            if (isGzip)
            {
                using (var decompressedStream = new MemoryStream())
                {
                    using (var requestStream = Request.Body)
                    {
                        using (var gzipStream = new GZipStream(requestStream, CompressionMode.Decompress))
                        {
                            await gzipStream.CopyToAsync(decompressedStream);
                        }
                    }
                    decompressedStream.Seek(0, SeekOrigin.Begin);

                    using (var reader = new StreamReader(decompressedStream, Encoding.UTF8))
                    {
                        var json = await reader.ReadToEndAsync();
                        Console.WriteLine(json);
                        requestData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BatchAssetRequest>>(json);
                    }
                }
            }
            else
            {
                using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    var json = await reader.ReadToEndAsync();
                    Console.WriteLine(json);
                    requestData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BatchAssetRequest>>(json);
                }
            }
            if (requestData == null)
            {
                throw new BadRequestException();
            }
            var assetReturnInfo = new List<object>();
            foreach (var request in requestData)
            {
                Console.WriteLine(request.assetId);
                assetReturnInfo.Add(new
                {
                    Location = $"{Configuration.BaseUrl}/v1/asset?id={request.assetId}",
                    RequestId = request.requestId,
                    IsHashDynamic = true,
                    IsCopyrightProtected = true, 
                    IsArchived = false,
                });
            }

            return Content(Newtonsoft.Json.JsonConvert.SerializeObject(assetReturnInfo), "application/json");
        }
	}
}