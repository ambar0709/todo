using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Todo.Models;

namespace Todo.Services
{
    public static class Gravatar
    {
        private static HttpClient _httpClient = new();

        private static Dictionary<string, GravatarProfile> _cacheProfile = new();

        public static string GetHash(string emailAddress)
        {
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.Default.GetBytes(emailAddress.Trim().ToLowerInvariant());
                var hashBytes = md5.ComputeHash(inputBytes);

                var builder = new StringBuilder();
                foreach (var b in hashBytes)
                {
                    builder.Append(b.ToString("X2"));
                }
                return builder.ToString().ToLowerInvariant();
            }
        }

        public static async Task<GravatarProfile> GetGravatarUser(string email)
        {
            if (_cacheProfile.ContainsKey(email))
            {
                return _cacheProfile[email];
            }
            var profile = new GravatarProfile();
            var emailHash = GetHash(email);
            var avatarUrl = $"https://www.gravatar.com/avatar/{emailHash}";
            try
            {
                _httpClient.Timeout = TimeSpan.FromSeconds(5);
                var response = await _httpClient.GetStringAsync($"https://en.gravatar.com/{emailHash}.json");

                var json = JsonConvert.DeserializeObject<dynamic>(response);
                var displayName = (string)json.entry[0]?.displayName;
                profile = new GravatarProfile(displayName, avatarUrl,email);

            }
            catch (Exception ex)
            {
                profile = new GravatarProfile(string.Empty, avatarUrl, email);
            }
                
            _cacheProfile.Add(email, profile);

            return _cacheProfile[email];
        }
    }
}